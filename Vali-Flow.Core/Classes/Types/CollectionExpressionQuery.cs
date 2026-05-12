using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;

namespace Vali_Flow.Core.Classes.Types;

/// <summary>
/// EF Core-safe collection validation conditions for the fluent builder.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type returned by each fluent method.</typeparam>
/// <typeparam name="T">The entity type being validated.</typeparam>
/// <remarks>
/// Only contains predicates that EF Core can translate to SQL.
/// Methods relying on element-level predicates (Any/All with lambda) are intentionally excluded
/// and are available only in <see cref="CollectionExpression{TBuilder,T}"/>.
/// </remarks>
public class CollectionExpressionQuery<TBuilder, T> : ICollectionExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    /// <summary>The underlying builder that accumulates conditions.</summary>
    private readonly BaseExpression<TBuilder, T> _builder;

    /// <summary>
    /// Initializes a new instance of <see cref="CollectionExpressionQuery{TBuilder,T}"/>.
    /// </summary>
    /// <param name="builder">The parent builder that owns the condition list.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public CollectionExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Adds a condition that the selected collection is not <see langword="null"/> and contains at least one element.</summary>
    /// <typeparam name="TValue">The element type of the collection.</typeparam>
    /// <param name="selector">Property selector for the collection member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder NotEmpty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Any();
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected collection is not <see langword="null"/> and contains no elements.</summary>
    /// <typeparam name="TValue">The element type of the collection.</typeparam>
    /// <param name="selector">Property selector for the collection member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder Empty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && !val.Any();
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected value is contained in the provided <paramref name="values"/> list (SQL IN).</summary>
    /// <typeparam name="TValue">The type of the selected member and list elements.</typeparam>
    /// <param name="selector">Property selector for the member to test.</param>
    /// <param name="values">The allowed values; must not be null or empty.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> or <paramref name="values"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    public TBuilder In<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(values);
        List<TValue> valueList = values.ToList();
        if (valueList.Count == 0)
            throw new ArgumentException("values must not be empty for In(). An empty IN list would silently filter out every row.", nameof(values));
        Expression<Func<TValue, bool>> predicate = val => valueList.Contains(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected value is NOT contained in the provided <paramref name="values"/> list (SQL NOT IN).</summary>
    /// <typeparam name="TValue">The type of the selected member and list elements.</typeparam>
    /// <param name="selector">Property selector for the member to test.</param>
    /// <param name="values">The disallowed values; must not be null or empty.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> or <paramref name="values"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    public TBuilder NotIn<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(values);
        List<TValue> valueList = values.ToList();
        if (valueList.Count == 0)
            throw new ArgumentException("values must not be empty for NotIn(). An empty NOT IN list would silently pass every row.", nameof(values));
        Expression<Func<TValue, bool>> predicate = val => !valueList.Contains(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected collection has exactly <paramref name="count"/> elements.</summary>
    /// <typeparam name="TValue">The element type of the collection.</typeparam>
    /// <param name="selector">Property selector for the collection member.</param>
    /// <param name="count">The required element count; must be &gt;= 0.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
    public TBuilder Count<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int count)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "count must be >= 0.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() == count;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected collection has at least <paramref name="min"/> elements.</summary>
    /// <typeparam name="TValue">The element type of the collection.</typeparam>
    /// <param name="selector">Property selector for the collection member.</param>
    /// <param name="min">The minimum element count; must be &gt;= 0.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="min"/> is negative.</exception>
    public TBuilder MinCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "min must be >= 0.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() >= min;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected collection has at most <paramref name="max"/> elements.</summary>
    /// <typeparam name="TValue">The element type of the collection.</typeparam>
    /// <param name="selector">Property selector for the collection member.</param>
    /// <param name="max">The maximum element count; must be &gt;= 0.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is negative.</exception>
    public TBuilder MaxCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < 0) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= 0.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected collection element count falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <typeparam name="TValue">The element type of the collection.</typeparam>
    /// <param name="selector">Property selector for the collection member.</param>
    /// <param name="min">Inclusive minimum count; must be &gt;= 0.</param>
    /// <param name="max">Inclusive maximum count; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="min"/> is negative or <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder CountBetween<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "min must be >= 0.");
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() >= min && val.Count() <= max;
        return _builder.Add(selector, predicate);
    }
}
