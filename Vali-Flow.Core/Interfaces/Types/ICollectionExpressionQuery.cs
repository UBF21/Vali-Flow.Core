using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for EF Core-safe collection expression conditions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder implementing the interface.</typeparam>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface ICollectionExpressionQuery<out TBuilder, T>
{
    /// <summary>Validates that the selected collection is not null and has at least one element.</summary>
    /// <typeparam name="TValue">The element type of the collection.</typeparam>
    /// <param name="selector">Expression selecting the collection property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder NotEmpty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector);

    /// <summary>Validates that the selected collection is not null and has no elements.</summary>
    /// <typeparam name="TValue">The element type of the collection.</typeparam>
    /// <param name="selector">Expression selecting the collection property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder Empty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector);

    /// <summary>Validates that the selected scalar value is contained in <paramref name="values"/>.</summary>
    /// <typeparam name="TValue">The type of the scalar property being evaluated.</typeparam>
    /// <param name="selector">Expression selecting the scalar property.</param>
    /// <param name="values">The set of allowed values.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder In<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values);

    /// <summary>Validates that the selected scalar value is NOT contained in <paramref name="values"/>.</summary>
    /// <typeparam name="TValue">The type of the scalar property being evaluated.</typeparam>
    /// <param name="selector">Expression selecting the scalar property.</param>
    /// <param name="values">The set of excluded values.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder NotIn<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values);

    /// <summary>Validates that the collection has exactly <paramref name="count"/> elements.</summary>
    /// <typeparam name="TValue">The element type of the collection.</typeparam>
    /// <param name="selector">Expression selecting the collection property.</param>
    /// <param name="count">The exact number of elements required.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder Count<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int count);

    /// <summary>Validates that the collection has at least <paramref name="min"/> elements.</summary>
    /// <typeparam name="TValue">The element type of the collection.</typeparam>
    /// <param name="selector">Expression selecting the collection property.</param>
    /// <param name="min">The minimum number of elements required (inclusive).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder MinCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min);

    /// <summary>Validates that the collection has at most <paramref name="max"/> elements.</summary>
    /// <typeparam name="TValue">The element type of the collection.</typeparam>
    /// <param name="selector">Expression selecting the collection property.</param>
    /// <param name="max">The maximum number of elements allowed (inclusive).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder MaxCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int max);

    /// <summary>Validates that the selected collection has between <paramref name="min"/> and <paramref name="max"/> elements (inclusive).</summary>
    /// <typeparam name="TValue">The element type of the collection.</typeparam>
    /// <param name="selector">Expression selecting the collection property.</param>
    /// <param name="min">The minimum number of elements (inclusive).</param>
    /// <param name="max">The maximum number of elements (inclusive).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder CountBetween<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min, int max);
}
