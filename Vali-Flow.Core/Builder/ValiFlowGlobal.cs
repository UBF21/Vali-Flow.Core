using System.Linq.Expressions;

namespace Vali_Flow.Core.Builder;

public static class ValiFlowGlobal
{
    private static readonly Dictionary<Type, List<LambdaExpression>> _globalFilters = new();
    private static readonly object _lock = new();

    /// <summary>Registers a global filter expression applied to all <typeparamref name="T"/> queries via <c>BuildWithGlobal()</c>.</summary>
    /// <remarks>
    /// <b>EF Core compatibility:</b> Registering a filter for an interface type (e.g., <c>Register&lt;ISoftDeletable&gt;(...)</c>)
    /// produces an <c>Expression.Convert</c> cast in the combined expression tree returned by <c>BuildWithGlobal()</c>.
    /// Most EF Core providers cannot translate <c>Convert</c> casts and will throw at query translation time.
    /// Use interface-based global filters only with in-memory collections (LINQ-to-Objects), or register filters
    /// directly on the concrete entity type instead.
    /// <para>
    /// <b>Concurrency:</b> <c>Register</c> and <c>Clear</c> take an internal lock and are safe to call
    /// concurrently, but they are intended to be called during application startup before any query is issued.
    /// <c>BuildWithGlobal()</c> takes a snapshot of the registered filters under the lock and then builds the
    /// combined expression outside it; filters registered <em>after</em> <c>BuildWithGlobal()</c> starts will
    /// not be included in that build. Avoid calling <c>Register</c> or <c>Clear</c> concurrently with active
    /// query execution.
    /// </para>
    /// <para>
    /// <b>Unit tests:</b> The filter dictionary is static and persists for the lifetime of the process.
    /// Call <see cref="ClearAll"/> (or <see cref="Clear{T}"/>) in test teardown to prevent cross-test
    /// state pollution.
    /// </para>
    /// </remarks>
    public static void Register<T>(Expression<Func<T, bool>> filter)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        lock (_lock)
        {
            if (!_globalFilters.ContainsKey(typeof(T)))
            {
                _globalFilters[typeof(T)] = new List<LambdaExpression>();
            }

            _globalFilters[typeof(T)].Add(filter);
        }
    }

    /// <summary>Removes all globally registered filters for type <typeparamref name="T"/>.</summary>
    public static void Clear<T>()
    {
        lock (_lock)
        {
            _globalFilters.Remove(typeof(T));
        }
    }

    /// <summary>Removes all globally registered filters for all types. Call in test teardown to prevent cross-test state pollution.</summary>
    public static void ClearAll()
    {
        lock (_lock)
        {
            _globalFilters.Clear();
        }
    }

    /// <summary>Returns <c>true</c> if any global filters are registered for type <typeparamref name="T"/>, including filters registered for interfaces implemented by <typeparamref name="T"/>.</summary>
    public static bool HasFilters<T>()
    {
        lock (_lock)
        {
            if (_globalFilters.TryGetValue(typeof(T), out var exact) && exact.Count > 0)
                return true;
            foreach (var iface in typeof(T).GetInterfaces())
            {
                if (_globalFilters.TryGetValue(iface, out var ifilters) && ifilters.Count > 0)
                    return true;
            }
            return false;
        }
    }

    /// <summary>Returns a snapshot of all global filters registered for type <typeparamref name="T"/> and any interfaces it implements.</summary>
    public static IReadOnlyList<Expression<Func<T, bool>>> GetFilters<T>()
    {
        // Snapshot under lock; expression building (ExpressionVisitor) runs outside the lock
        // to avoid holding it during potentially slow tree-walking operations.
        List<LambdaExpression>? exactSnapshot = null;
        List<(Type Iface, List<LambdaExpression> Filters)>? ifaceSnapshot = null;

        lock (_lock)
        {
            if (_globalFilters.TryGetValue(typeof(T), out var exact))
            {
                exactSnapshot = new List<LambdaExpression>(exact);
            }

            foreach (var iface in typeof(T).GetInterfaces())
            {
                if (!_globalFilters.TryGetValue(iface, out var ifilters))
                {
                    continue;
                }

                ifaceSnapshot ??= new List<(Type, List<LambdaExpression>)>();
                ifaceSnapshot.Add((iface, new List<LambdaExpression>(ifilters)));
            }
        }

        var result = new List<Expression<Func<T, bool>>>();

        if (exactSnapshot != null)
        {
            result.AddRange(exactSnapshot.OfType<Expression<Func<T, bool>>>());
        }

        if (ifaceSnapshot != null)
        {
            foreach (var (iface, ifilters) in ifaceSnapshot)
            {
                foreach (var f in ifilters)
                {
                    var newParam = Expression.Parameter(typeof(T), f.Parameters[0].Name);
                    var converted = Expression.Convert(newParam, iface);
                    var body = new GlobalParameterReplacer(f.Parameters[0], converted).Visit(f.Body);
                    result.Add(Expression.Lambda<Func<T, bool>>(body, newParam));
                }
            }
        }

        return result;
    }

    private sealed class GlobalParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _old;
        private readonly Expression _new;

        internal GlobalParameterReplacer(ParameterExpression old, Expression @new)
        {
            _old = old;
            _new = @new;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _old ? _new : base.VisitParameter(node);
    }
}
