using System.Linq.Expressions;
using System.Numerics;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for numeric parity checks: IsEven, IsOdd, and IsMultipleOf.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface INumericParityExpression<out TBuilder, T>
{
    /// <summary>Validates that the selected numeric value is even (value % 2 == 0).</summary>
    /// <remarks>Not EF Core translatable — uses the modulo operator which is not supported by all providers.</remarks>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    TBuilder IsEven<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>;

    /// <summary>Validates that the selected numeric value is odd (value % 2 != 0).</summary>
    /// <remarks>Not EF Core translatable — uses the modulo operator which is not supported by all providers.</remarks>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    TBuilder IsOdd<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>;

    /// <summary>Validates that the selected numeric value is a multiple of <paramref name="divisor"/> (value % divisor == 0).</summary>
    /// <remarks>Not EF Core translatable — uses the modulo operator which is not supported by all providers.</remarks>
    /// <typeparam name="TValue">Any numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">An expression selecting the numeric property to evaluate.</param>
    /// <param name="divisor">The divisor to check against. Must not be zero.</param>
    TBuilder IsMultipleOf<TValue>(Expression<Func<T, TValue>> selector, TValue divisor) where TValue : INumber<TValue>;
}
