using System.Linq.Expressions;
using Vali_Flow.Core.Interfaces.General;

namespace Vali_Flow.Core.Builder;

/// <summary>
/// A fluent sort-criteria builder that applies <c>OrderBy</c>/<c>ThenBy</c> chains
/// to both <see cref="IQueryable{T}"/> (EF Core) and <see cref="IEnumerable{T}"/> (in-memory) sequences.
/// </summary>
/// <remarks>
/// <b>Thread safety:</b> A <c>ValiSort&lt;T&gt;</c> instance is not safe for concurrent use.
/// <see cref="Apply(IEnumerable{T})"/> lazily compiles and caches the sort delegate on first call;
/// do not call it from multiple threads simultaneously on the same instance.
/// For concurrent scenarios, create a separate <c>ValiSort&lt;T&gt;</c> instance per thread.
/// </remarks>
public sealed class ValiSort<T> : IValiSort<T>
{
    // Plain Dictionary is intentional: ValiSort<T> is documented as non-thread-safe.
    // Using ConcurrentDictionary would send a misleading thread-safety signal.
    private readonly Dictionary<Type, System.Reflection.MethodInfo>
        _orderByCache = new();

    private readonly Dictionary<Type, System.Reflection.MethodInfo>
        _thenByCache = new();

    private readonly List<(LambdaExpression Selector, bool Descending, bool IsPrimary, Delegate? CompiledDelegate)> _sorts = new();

    /// <summary>Sets the primary sort key, replacing any previously configured sort criteria.</summary>
    /// <remarks>
    /// Calling <c>By</c> resets all previously configured sort criteria, including any <c>ThenBy</c> clauses.
    /// To add a secondary sort key, call <see cref="ThenBy{TKey}"/> after this method.
    /// </remarks>
    public IValiSort<T> By<TKey>(Expression<Func<T, TKey>> selector, bool descending = false)
    {
        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        _sorts.Clear();
        _sorts.Add((selector, descending, true, null));
        return this;
    }

    public IValiSort<T> ThenBy<TKey>(Expression<Func<T, TKey>> selector, bool descending = false)
    {
        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        if (_sorts.Count == 0)
        {
            throw new InvalidOperationException("Call By() before ThenBy().");
        }

        _sorts.Add((selector, descending, false, null));
        return this;
    }

    public IOrderedQueryable<T> Apply(IQueryable<T> query)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (_sorts.Count == 0)
        {
            throw new InvalidOperationException("No sort criteria defined. Call By() first.");
        }

        IOrderedQueryable<T>? result = null;
        foreach (var (selector, descending, isPrimary, _) in _sorts)
        {
            if (isPrimary || result == null)
            {
                result = ApplyOrderBy(query, selector, descending);
            }
            else
            {
                result = ApplyThenBy(result, selector, descending);
            }
        }

        return result ?? throw new InvalidOperationException("Failed to apply sort criteria.");
    }

    /// <summary>Applies the configured sort criteria to an in-memory sequence.</summary>
    /// <remarks>This overload evaluates the sort in memory. For database queries, use <see cref="Apply(IQueryable{T})"/> instead.</remarks>
    public IOrderedEnumerable<T> Apply(IEnumerable<T> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (_sorts.Count == 0)
        {
            throw new InvalidOperationException("No sort criteria defined. Call By() first.");
        }

        IOrderedEnumerable<T>? result = null;
        for (int i = 0; i < _sorts.Count; i++)
        {
            var (selector, descending, isPrimary, compiled) = _sorts[i];
            if (compiled == null)
            {
                compiled = selector.Compile();
                _sorts[i] = (selector, descending, isPrimary, compiled);
            }
            if (isPrimary || result == null)
            {
                result = ApplyOrderByEnumerable(source, selector, descending, compiled);
            }
            else
            {
                result = ApplyThenByEnumerable(result, selector, descending, compiled);
            }
        }

        return result!;
    }

    public string Explain()
    {
        if (_sorts.Count == 0)
        {
            return "(no sort)";
        }

        var parts = new List<string>();
        for (int i = 0; i < _sorts.Count; i++)
        {
            var (selector, descending, _, _) = _sorts[i];
            string name = selector.Body is MemberExpression member
                ? member.Member.Name
                : selector.Body.ToString();
            string dir = descending ? "DESC" : "ASC";
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

    private IOrderedEnumerable<T> ApplyOrderByEnumerable(
        IEnumerable<T> source, LambdaExpression selector, bool descending, Delegate compiled)
    {
        if (!_orderByCache.TryGetValue(selector.ReturnType, out var method))
        {
            method = typeof(ValiSort<T>)
                .GetMethod(nameof(ApplyOrderByTyped),
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(selector.ReturnType);
            _orderByCache[selector.ReturnType] = method;
        }
        return (IOrderedEnumerable<T>)method.Invoke(null, new object[] { source, compiled, descending })!;
    }

    private IOrderedEnumerable<T> ApplyThenByEnumerable(
        IOrderedEnumerable<T> source, LambdaExpression selector, bool descending, Delegate compiled)
    {
        if (!_thenByCache.TryGetValue(selector.ReturnType, out var method))
        {
            method = typeof(ValiSort<T>)
                .GetMethod(nameof(ApplyThenByTyped),
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(selector.ReturnType);
            _thenByCache[selector.ReturnType] = method;
        }
        return (IOrderedEnumerable<T>)method.Invoke(null, new object[] { source, compiled, descending })!;
    }

    private static IOrderedEnumerable<T> ApplyOrderByTyped<TKey>(
        IEnumerable<T> source, Delegate compiledDelegate, bool descending)
    {
        var compiled = (Func<T, TKey>)compiledDelegate;
        return descending ? source.OrderByDescending(compiled) : source.OrderBy(compiled);
    }

    private static IOrderedEnumerable<T> ApplyThenByTyped<TKey>(
        IOrderedEnumerable<T> source, Delegate compiledDelegate, bool descending)
    {
        var compiled = (Func<T, TKey>)compiledDelegate;
        return descending ? source.ThenByDescending(compiled) : source.ThenBy(compiled);
    }
}
