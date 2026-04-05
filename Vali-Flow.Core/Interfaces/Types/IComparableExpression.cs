using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for generic IComparable&lt;TValue&gt; comparisons and cross-property comparisons.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface IComparableExpression<out TBuilder, T>
{
    /// <summary>Adds a condition to check if the selected property is greater than <paramref name="value"/> using IComparable&lt;TValue&gt;.</summary>
    TBuilder GreaterThan<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the selected property is greater than or equal to <paramref name="value"/> using IComparable&lt;TValue&gt;.</summary>
    TBuilder GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the selected property is less than <paramref name="value"/> using IComparable&lt;TValue&gt;.</summary>
    TBuilder LessThan<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the selected property is less than or equal to <paramref name="value"/> using IComparable&lt;TValue&gt;.</summary>
    TBuilder LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the selected property is within [<paramref name="min"/>, <paramref name="max"/>] using IComparable&lt;TValue&gt;.</summary>
    TBuilder InRange<TValue>(Expression<Func<T, TValue>> selector, TValue min, TValue max) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the selected property is equal to <paramref name="value"/> using IComparable&lt;TValue&gt;.</summary>
    TBuilder EqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the property selected by <paramref name="selector"/> is greater than the property selected by <paramref name="otherSelector"/>.</summary>
    TBuilder GreaterThan<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the property selected by <paramref name="selector"/> is greater than or equal to the property selected by <paramref name="otherSelector"/>.</summary>
    TBuilder GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the property selected by <paramref name="selector"/> is less than the property selected by <paramref name="otherSelector"/>.</summary>
    TBuilder LessThan<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the property selected by <paramref name="selector"/> is less than or equal to the property selected by <paramref name="otherSelector"/>.</summary>
    TBuilder LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector) where TValue : IComparable<TValue>;
}
