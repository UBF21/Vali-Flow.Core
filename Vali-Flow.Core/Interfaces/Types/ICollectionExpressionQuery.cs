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
    TBuilder NotEmpty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector);

    /// <summary>Validates that the selected collection is not null and has no elements.</summary>
    TBuilder Empty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector);

    /// <summary>Validates that the selected scalar value is contained in <paramref name="values"/>.</summary>
    TBuilder In<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values);

    /// <summary>Validates that the selected scalar value is NOT contained in <paramref name="values"/>.</summary>
    TBuilder NotIn<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values);

    /// <summary>Validates that the collection has exactly <paramref name="count"/> elements.</summary>
    TBuilder Count<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int count);

    /// <summary>Validates that the collection has at least <paramref name="min"/> elements.</summary>
    TBuilder MinCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min);

    /// <summary>Validates that the collection has at most <paramref name="max"/> elements.</summary>
    TBuilder MaxCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int max);

    /// <summary>Validates that the selected collection has between <paramref name="min"/> and <paramref name="max"/> elements (inclusive).</summary>
    TBuilder CountBetween<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min, int max);
}
