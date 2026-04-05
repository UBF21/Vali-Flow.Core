using System.Linq.Expressions;
using Vali_Flow.Core.Interfaces.General;

namespace Vali_Flow.Core.Builder;

/// <summary>
/// A fluent sort-criteria builder that applies <c>OrderBy</c>/<c>ThenBy</c> chains
/// to both <see cref="IQueryable{T}"/> (EF Core) and <see cref="IEnumerable{T}"/> (in-memory) sequences.
/// </summary>
/// <remarks>
/// <b>Thread safety:</b> A <c>ValiSort&lt;T&gt;</c> instance is not safe for concurrent use.
/// For concurrent scenarios, create a separate <c>ValiSort&lt;T&gt;</c> instance per thread.
/// </remarks>
public sealed class ValiSort<T> : IValiSort<T>
{
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
                result = entry.OrderApply!(source);
            }
            else
            {
                result = entry.ThenApply!(result);
            }
        }

        return result!;
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
    {
        string method = desc ? "OrderByDescending" : "OrderBy";
        return (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(
            Expression.Call(
                typeof(Queryable),
                method,
                new[] { typeof(T), selector.ReturnType },
                query.Expression,
                Expression.Quote(selector)));
    }

    private static IOrderedQueryable<T> ApplyThenBy(IOrderedQueryable<T> query, LambdaExpression selector, bool desc)
    {
        string method = desc ? "ThenByDescending" : "ThenBy";
        return (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(
            Expression.Call(
                typeof(Queryable),
                method,
                new[] { typeof(T), selector.ReturnType },
                query.Expression,
                Expression.Quote(selector)));
    }
}
