using System.Linq.Expressions;
using System.Numerics;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for checking the sign of numeric properties (zero, not-zero, positive, negative).
/// Accepts any numeric type that implements <see cref="INumber{TSelf}"/> (e.g., <c>int</c>, <c>long</c>,
/// <c>double</c>, <c>decimal</c>, <c>float</c>, <c>short</c>).
/// </summary>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface INumericSignExpression<out TBuilder, T>
{
    /// <summary>Validates that the selected numeric value equals zero.</summary>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    TBuilder Zero<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>;

    /// <summary>Validates that the selected numeric value is not zero.</summary>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    TBuilder NotZero<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>;

    /// <summary>Validates that the selected numeric value is positive (&gt; 0).</summary>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    TBuilder Positive<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>;

    /// <summary>Validates that the selected numeric value is negative (&lt; 0).</summary>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    TBuilder Negative<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>;
}
