using System.Collections.Concurrent;
using System.Linq.Expressions;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Builder;

/// <summary>
/// An instanciable registry of global filter expressions applied via
/// <see cref="Classes.Base.BaseExpression{TBuilder,T}.BuildWithGlobal(ValiFlowGlobalRegistry)"/>.
/// </summary>
/// <remarks>
/// <para>
/// Use this class when you need per-tenant, per-scope, or per-test filter isolation.
/// Register the instance in your DI container as a singleton (or scoped for per-request isolation)
/// and inject it where needed:
/// </para>
/// <code>
/// // Registration (e.g. ASP.NET Core)
/// services.AddSingleton&lt;ValiFlowGlobalRegistry&gt;(sp =>
/// {
///     var registry = new ValiFlowGlobalRegistry();
///     registry.Register&lt;Order&gt;(o => !o.IsDeleted);
///     return registry;
/// });
///
/// // Usage
/// var expr = new ValiFlowQuery&lt;Order&gt;()
///     .GreaterThan(o => o.Total, 0)
///     .BuildWithGlobal(registry);  // combines local + registry filters
/// </code>
/// <para>
/// For process-wide filters without DI, use the static <see cref="ValiFlowGlobal.Default"/> instance
/// via <see cref="Classes.Base.BaseExpression{TBuilder,T}.BuildWithGlobal()"/> (no parameter).
/// </para>
/// <para>
/// <b>Thread safety:</b> <see cref="Register{T}"/>, <see cref="Clear{T}"/>, and <see cref="ClearAll"/> are
/// protected by an internal lock and safe to call concurrently, but are intended for startup configuration.
/// <see cref="GetFilters{T}"/> takes a snapshot under the lock and builds expressions outside it.
/// </para>
/// </remarks>
public sealed class ValiFlowGlobalRegistry
{
    private readonly Dictionary<Type, List<LambdaExpression>> _filters = new();
    private readonly object _lock = new();
    private readonly ConcurrentDictionary<Type, object> _filtersCache = new();

    /// <summary>Registers a global filter expression applied to all <typeparamref name="T"/> queries via <c>BuildWithGlobal(registry)</c>.</summary>
    /// <remarks>
    /// <b>EF Core compatibility:</b> Registering a filter for an interface type (e.g., <c>Register&lt;ISoftDeletable&gt;(...)</c>)
    /// produces an <c>Expression.Convert</c> cast in the combined expression tree. Most EF Core providers cannot translate
    /// <c>Convert</c> casts — use interface-based filters only with in-memory collections, or register on the concrete entity type.
    /// </remarks>
    public void Register<T>(Expression<Func<T, bool>> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        lock (_lock)
        {
            if (!_filters.TryGetValue(typeof(T), out var list))
            {
                list = new List<LambdaExpression>();
                _filters[typeof(T)] = list;
            }
            list.Add(filter);
            _filtersCache.Clear();
        }
    }

    /// <summary>Removes all globally registered filters for type <typeparamref name="T"/>.</summary>
    public void Clear<T>()
    {
        lock (_lock)
        {
            _filters.Remove(typeof(T));
            _filtersCache.Clear();
        }
    }

    /// <summary>Removes all registered filters for all types. Useful in test teardown to prevent cross-test state pollution.</summary>
    public void ClearAll()
    {
        lock (_lock)
        {
            _filters.Clear();
            _filtersCache.Clear();
        }
    }

    /// <summary>Returns <c>true</c> if any filters are registered for <typeparamref name="T"/> or any interface it implements.</summary>
    public bool HasFilters<T>()
    {
        lock (_lock)
        {
            if (_filters.TryGetValue(typeof(T), out var exact) && exact.Count > 0)
                return true;
            foreach (var iface in typeof(T).GetInterfaces())
            {
                if (_filters.TryGetValue(iface, out var ifilters) && ifilters.Count > 0)
                    return true;
            }
            return false;
        }
    }

    /// <summary>Returns a snapshot of all filters registered for <typeparamref name="T"/> and any interfaces it implements.</summary>
    /// <remarks>
    /// Results are cached per type and invalidated automatically when <see cref="Register{T}"/>,
    /// <see cref="Clear{T}"/>, or <see cref="ClearAll"/> is called.
    /// For consistent results, all <see cref="Register{T}"/> calls should complete
    /// before <see cref="GetFilters{T}"/> is called concurrently from multiple threads.
    /// </remarks>
    public IReadOnlyList<Expression<Func<T, bool>>> GetFilters<T>()
    {
        if (_filtersCache.TryGetValue(typeof(T), out var cached))
            return (IReadOnlyList<Expression<Func<T, bool>>>)cached;

        List<LambdaExpression>? exactSnapshot = null;
        List<(Type Iface, List<LambdaExpression> Filters)>? ifaceSnapshot = null;

        lock (_lock)
        {
            if (_filters.TryGetValue(typeof(T), out var exact))
                exactSnapshot = new List<LambdaExpression>(exact);

            foreach (var iface in typeof(T).GetInterfaces())
            {
                if (!_filters.TryGetValue(iface, out var ifilters)) continue;
                ifaceSnapshot ??= new List<(Type, List<LambdaExpression>)>();
                ifaceSnapshot.Add((iface, new List<LambdaExpression>(ifilters)));
            }
        }

        var result = new List<Expression<Func<T, bool>>>();

        if (exactSnapshot != null)
            result.AddRange(exactSnapshot.OfType<Expression<Func<T, bool>>>());

        if (ifaceSnapshot != null)
        {
            foreach (var (iface, ifilters) in ifaceSnapshot)
            {
                foreach (var f in ifilters)
                {
                    var newParam = Expression.Parameter(typeof(T), f.Parameters[0].Name);
                    var converted = Expression.Convert(newParam, iface);
                    var body = new ParameterReplacer(f.Parameters[0], converted).Visit(f.Body);
                    result.Add(Expression.Lambda<Func<T, bool>>(body, newParam));
                }
            }
        }

        IReadOnlyList<Expression<Func<T, bool>>> built = result.AsReadOnly();
        _filtersCache.TryAdd(typeof(T), built);
        return built;
    }
}
