using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for constructing Vali-Flow.Core conditions based on numeric property evaluations.
/// Combines sign checks, comparisons, range validations, nullable support, parity checks, and generic IComparable operations.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface INumericExpression<out TBuilder, T>
    : INumericSignExpression<TBuilder, T>
    , INumericComparisonExpression<TBuilder, T>
    , INumericRangeExpression<TBuilder, T>
    , INullableNumericExpression<TBuilder, T>
    , INumericParityExpression<TBuilder, T>
    , IComparableExpression<TBuilder, T>
{
}
