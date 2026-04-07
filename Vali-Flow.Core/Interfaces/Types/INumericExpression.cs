using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for constructing Vali-Flow.Core conditions based on numeric property evaluations.
/// Combines sign checks, comparisons, range validations, nullable support, and parity checks.
/// All methods accept any numeric type implementing <see cref="System.Numerics.INumber{TSelf}"/>.
/// </summary>
/// <remarks>
/// Breaking change from v1.x: Typed per-primitive overloads (int, long, double, etc.) have been replaced
/// with generic methods constrained to <see cref="System.Numerics.INumber{TSelf}"/>.
/// <see cref="IComparableExpression{TBuilder,T}"/> is no longer part of this interface;
/// use it directly if you need <see cref="System.IComparable{T}"/>-based ordering for non-numeric types.
/// </remarks>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface INumericExpression<out TBuilder, T>
    : INumericSignExpression<TBuilder, T>
    , INumericComparisonExpression<TBuilder, T>
    , INumericRangeExpression<TBuilder, T>
    , INullableNumericExpression<TBuilder, T>
    , INumericParityExpression<TBuilder, T>
{
}
