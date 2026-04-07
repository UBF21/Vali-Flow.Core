using System.Linq.Expressions;
using System.Numerics;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for numeric range validations: InRange (scalar and cross-property), IsBetweenExclusive, and IsCloseTo.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface INumericRangeExpression<out TBuilder, T>
{
    /// <summary>Validates that the selected numeric value is within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (inclusive).</param>
    TBuilder InRange<TValue>(Expression<Func<T, TValue>> selector, TValue min, TValue max) where TValue : INumber<TValue>;

    /// <summary>Validates that the selected numeric value is within the range defined by <paramref name="minSelector"/> and <paramref name="maxSelector"/> (inclusive).</summary>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    /// <param name="minSelector">An expression selecting the property providing the minimum value.</param>
    /// <param name="maxSelector">An expression selecting the property providing the maximum value.</param>
    TBuilder InRange<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> minSelector,
        Expression<Func<T, TValue>> maxSelector) where TValue : INumber<TValue>;

    /// <summary>Validates that the selected numeric value is strictly between <paramref name="min"/> and <paramref name="max"/> (both exclusive).</summary>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    /// <param name="min">The exclusive lower bound.</param>
    /// <param name="max">The exclusive upper bound.</param>
    TBuilder IsBetweenExclusive<TValue>(Expression<Func<T, TValue>> selector, TValue min, TValue max) where TValue : INumber<TValue>;

    /// <summary>Validates that the selected floating-point value is within <paramref name="tolerance"/> of <paramref name="value"/> (i.e. |val - value| ≤ tolerance).</summary>
    /// <remarks>Not EF Core translatable — uses <c>IFloatingPointIeee754&lt;TValue&gt;.Abs</c> which cannot be translated to SQL.</remarks>
    /// <typeparam name="TValue">Any IEEE 754 floating-point type (e.g. <c>double</c>, <c>float</c>).</typeparam>
    /// <param name="selector">An expression selecting the floating-point property to evaluate.</param>
    /// <param name="value">The target value.</param>
    /// <param name="tolerance">The maximum allowed deviation from <paramref name="value"/>.</param>
    TBuilder IsCloseTo<TValue>(Expression<Func<T, TValue>> selector, TValue value, TValue tolerance) where TValue : IFloatingPointIeee754<TValue>;
}
