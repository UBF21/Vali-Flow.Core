using System.Linq.Expressions;
using System.Numerics;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for scalar numeric comparisons: GreaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo, MinValue, MaxValue.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface INumericComparisonExpression<out TBuilder, T>
{
    /// <summary>Validates that the selected numeric value is greater than <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    TBuilder GreaterThan<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : INumber<TValue>;

    /// <summary>Validates that the selected numeric value is greater than or equal to <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    TBuilder GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : INumber<TValue>;

    /// <summary>Validates that the selected numeric value is less than <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    TBuilder LessThan<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : INumber<TValue>;

    /// <summary>Validates that the selected numeric value is less than or equal to <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    TBuilder LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : INumber<TValue>;

    /// <summary>Validates that the selected numeric value is greater than or equal to <paramref name="minValue"/>.</summary>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    /// <param name="minValue">The minimum value to compare against.</param>
    TBuilder MinValue<TValue>(Expression<Func<T, TValue>> selector, TValue minValue) where TValue : INumber<TValue>;

    /// <summary>Validates that the selected numeric value is less than or equal to <paramref name="maxValue"/>.</summary>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    /// <param name="maxValue">The maximum value to compare against.</param>
    TBuilder MaxValue<TValue>(Expression<Func<T, TValue>> selector, TValue maxValue) where TValue : INumber<TValue>;
}
