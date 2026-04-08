using System.Linq.Expressions;
using System.Reflection;
using Vali_Flow.Core.Interfaces.General;

namespace Vali_Flow.Core.Builder;

/// <summary>
/// A fluent sort-criteria builder that applies <c>OrderBy</c>/<c>ThenBy</c> chains
/// to both <see cref="IQueryable{T}"/> (EF Core) and <see cref="IEnumerable{T}"/> (in-memory) sequences.
/// </summary>
/// <remarks>
/// <b>Thread safety:</b> A <c>ValiSort&lt;T&gt;</c> instance is not safe for concurrent use.
/// For concurrent scenarios, create a separate <c>ValiSort&lt;T&gt;</c> instance per thread.
/// <para>
/// Unlike <c>ValiFlow&lt;T&gt;</c>, <c>ValiSort&lt;T&gt;</c> does not implement a freeze/fork model.
/// It is designed to be configured once and reused for repeated <see cref="Apply"/> calls on
/// different sequences — but never mutated concurrently. If you need concurrent sort configurations,
/// keep one <c>ValiSort&lt;T&gt;</c> per logical sort contract and share it read-only after <see cref="By{TKey}"/> is done.
/// </para>
/// </remarks>
public sealed class ValiSort<T> : IValiSort<T>
{
    // Cached open-generic MethodInfo for Queryable sort methods — avoids string-based method lookup at call time.
    private static readonly MethodInfo _orderByMethod     = FindQueryableMethod("OrderBy");
    private static readonly MethodInfo _orderByDescMethod = FindQueryableMethod("OrderByDescending");
    private static readonly MethodInfo _thenByMethod      = FindQueryableMethod("ThenBy");
    private static readonly MethodInfo _thenByDescMethod  = FindQueryableMethod("ThenByDescending");

    private static MethodInfo FindQueryableMethod(string name) =>
        typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == name && m.GetParameters().Length == 2);

    private readonly List<SortEntry> _sorts = new();

    /// <summary>
    /// Encapsulates sort criteria for one key: the original expression (for IQueryable),
    /// and pre-compiled delegates (for IEnumerable) produced at registration time — no reflection needed.
    /// </summary>
    private readonly struct SortEntry
    {
        public LambdaExpression Selector { get; }
        public bool Descending { get; }
        public bool IsPrimary { get; }

        /// <summary>Pre-compiled delegate for <c>OrderBy</c>/<c>OrderByDescending</c> on IEnumerable.</summary>
        public Func<IEnumerable<T>, IOrderedEnumerable<T>>? OrderApply { get; }

        /// <summary>Pre-compiled delegate for <c>ThenBy</c>/<c>ThenByDescending</c> on IEnumerable.</summary>
        public Func<IOrderedEnumerable<T>, IOrderedEnumerable<T>>? ThenApply { get; }

        public SortEntry(
            LambdaExpression selector,
            bool descending,
            bool isPrimary,
            Func<IEnumerable<T>, IOrderedEnumerable<T>>? orderApply,
            Func<IOrderedEnumerable<T>, IOrderedEnumerable<T>>? thenApply)
        {
            if (isPrimary && orderApply == null)
                throw new ArgumentNullException(nameof(orderApply), "Primary sort entries require OrderApply.");
            if (!isPrimary && thenApply == null)
                throw new ArgumentNullException(nameof(thenApply), "Secondary sort entries require ThenApply.");
            Selector = selector;
            Descending = descending;
            IsPrimary = isPrimary;
            OrderApply = orderApply;
            ThenApply = thenApply;
        }
    }

    /// <summary>Sets the primary sort key, replacing any previously configured sort criteria.</summary>
    /// <remarks>
    /// Calling <c>By</c> resets all previously configured sort criteria, including any <c>ThenBy</c> clauses.
    /// To add a secondary sort key, call <see cref="ThenBy{TKey}"/> after this method.
    /// </remarks>
    public IValiSort<T> By<TKey>(Expression<Func<T, TKey>> selector, bool descending = false)
    {
        ArgumentNullException.ThrowIfNull(selector);
        _sorts.Clear();

        var compiled = selector.Compile();
        Func<IEnumerable<T>, IOrderedEnumerable<T>> orderApply = descending
            ? source => source.OrderByDescending(compiled)
            : source => source.OrderBy(compiled);

        _sorts.Add(new SortEntry(selector, descending, true, orderApply, null));
        return this;
    }

    /// <summary>Adds a secondary sort key. Must be called after <see cref="By{TKey}"/>.</summary>
    public IValiSort<T> ThenBy<TKey>(Expression<Func<T, TKey>> selector, bool descending = false)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (_sorts.Count == 0)
        {
            throw new InvalidOperationException("Call By() before ThenBy().");
        }

        var compiled = selector.Compile();
        Func<IOrderedEnumerable<T>, IOrderedEnumerable<T>> thenApply = descending
            ? source => source.ThenByDescending(compiled)
            : source => source.ThenBy(compiled);

        _sorts.Add(new SortEntry(selector, descending, false, null, thenApply));
        return this;
    }

    /// <summary>Applies the configured sort criteria to an <see cref="IQueryable{T}"/> (EF Core / SQL).</summary>
    public IOrderedQueryable<T> Apply(IQueryable<T> query)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (_sorts.Count == 0)
        {
            throw new InvalidOperationException("No sort criteria defined. Call By() first.");
        }

        IOrderedQueryable<T>? result = null;
        foreach (var entry in _sorts)
        {
            result = entry.IsPrimary || result == null
                ? ApplyOrderBy(query, entry.Selector, entry.Descending)
                : ApplyThenBy(result, entry.Selector, entry.Descending);
        }

        return result ?? throw new InvalidOperationException("Failed to apply sort criteria.");
    }

    /// <summary>Applies the configured sort criteria to an in-memory <see cref="IEnumerable{T}"/>.</summary>
    /// <remarks>
    /// Sort delegates are compiled eagerly in <see cref="By{TKey}"/> and <see cref="ThenBy{TKey}"/>,
    /// so this overload performs no reflection or lazy compilation at call time.
    /// For database queries, use <see cref="Apply(IQueryable{T})"/> instead.
    /// </remarks>
    public IOrderedEnumerable<T> Apply(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (_sorts.Count == 0)
        {
            throw new InvalidOperationException("No sort criteria defined. Call By() first.");
        }

        IOrderedEnumerable<T>? result = null;
        foreach (var entry in _sorts)
        {
            if (entry.IsPrimary || result == null)
            {
                result = (entry.OrderApply ?? throw new InvalidOperationException("OrderApply is null on a primary sort entry."))(source);
            }
            else
            {
                result = (entry.ThenApply ?? throw new InvalidOperationException("ThenApply is null on a secondary sort entry."))(result);
            }
        }

        return result ?? throw new InvalidOperationException("Sort produced a null result — _sorts was empty or all entries were skipped.");
    }

    /// <summary>Returns a human-readable description of the configured sort order.</summary>
    public string Explain()
    {
        if (_sorts.Count == 0)
        {
            return "(no sort)";
        }

        var parts = new List<string>();
        for (int i = 0; i < _sorts.Count; i++)
        {
            var entry = _sorts[i];
            string name = entry.Selector.Body is MemberExpression member
                ? member.Member.Name
                : entry.Selector.Body.ToString();
            string dir = entry.Descending ? "DESC" : "ASC";
            parts.Add(i == 0 ? $"ORDER BY {name} {dir}" : $"{name} {dir}");
        }

        return string.Join(", ", parts);
    }

    private static IOrderedQueryable<T> ApplyOrderBy(IQueryable<T> query, LambdaExpression selector, bool desc)
        => ApplyQuerySort(query, selector, desc ? _orderByDescMethod : _orderByMethod);

    private static IOrderedQueryable<T> ApplyThenBy(IOrderedQueryable<T> query, LambdaExpression selector, bool desc)
        => ApplyQuerySort(query, selector, desc ? _thenByDescMethod : _thenByMethod);

    private static IOrderedQueryable<T> ApplyQuerySort(IQueryable<T> source, LambdaExpression selector, MethodInfo genericMethod)
    {
        var method = genericMethod.MakeGenericMethod(typeof(T), selector.ReturnType);
        var callExpr = Expression.Call(null, method, source.Expression, Expression.Quote(selector));
        var result = source.Provider.CreateQuery<T>(callExpr);
        return result as IOrderedQueryable<T>
            ?? throw new InvalidOperationException(
                $"The IQueryable provider '{source.Provider.GetType().Name}' did not return an IOrderedQueryable<T>. " +
                "ValiSort requires a provider that supports ordered queries (e.g., EF Core, LINQ to SQL).");
    }
}
