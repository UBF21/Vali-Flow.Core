using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for numeric parity checks: IsEven, IsOdd, and IsMultipleOf for int and long.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface INumericParityExpression<out TBuilder, T>
{
    /// <summary>Validates that the selected integer property is even.</summary>
    TBuilder IsEven(Expression<Func<T, int>> selector);

    /// <summary>Validates that the selected integer property is even.</summary>
    TBuilder IsEven(Expression<Func<T, long>> selector);

    /// <summary>Validates that the selected integer property is odd.</summary>
    TBuilder IsOdd(Expression<Func<T, int>> selector);

    /// <summary>Validates that the selected integer property is odd.</summary>
    TBuilder IsOdd(Expression<Func<T, long>> selector);

    /// <summary>Validates that the selected integer property is a multiple of <paramref name="divisor"/>.</summary>
    TBuilder IsMultipleOf(Expression<Func<T, int>> selector, int divisor);

    /// <summary>Validates that the selected integer property is a multiple of <paramref name="divisor"/>.</summary>
    TBuilder IsMultipleOf(Expression<Func<T, long>> selector, long divisor);
}
