using System.Linq.Expressions;
using System.Numerics;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for nullable numeric property evaluations: IsNullOrZero, HasValue, GreaterThan, LessThan, and InRange.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface INullableNumericExpression<out TBuilder, T>
{
    /// <summary>Validates that the selected nullable numeric value is null or equal to zero.</summary>
    /// <typeparam name="TValue">Any value-type numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the nullable numeric property to evaluate.</param>
    TBuilder IsNullOrZero<TValue>(Expression<Func<T, TValue?>> selector) where TValue : struct, INumber<TValue>;

    /// <summary>Validates that the selected nullable numeric value has a non-null value.</summary>
    /// <typeparam name="TValue">Any value-type numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the nullable numeric property to evaluate.</param>
    TBuilder HasValue<TValue>(Expression<Func<T, TValue?>> selector) where TValue : struct, INumber<TValue>;

    /// <summary>Validates that the selected nullable numeric value has a value and is greater than <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">Any value-type numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the nullable numeric property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    TBuilder GreaterThan<TValue>(Expression<Func<T, TValue?>> selector, TValue value) where TValue : struct, INumber<TValue>;

    /// <summary>Validates that the selected nullable numeric value has a value and is less than <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">Any value-type numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the nullable numeric property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    TBuilder LessThan<TValue>(Expression<Func<T, TValue?>> selector, TValue value) where TValue : struct, INumber<TValue>;

    /// <summary>Validates that the selected nullable numeric value has a value and falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    /// <typeparam name="TValue">Any value-type numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the nullable numeric property to evaluate.</param>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (inclusive).</param>
    TBuilder InRange<TValue>(Expression<Func<T, TValue?>> selector, TValue min, TValue max) where TValue : struct, INumber<TValue>;
}
