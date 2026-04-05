namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for constructing Vali-Flow.Core conditions based on string property evaluations.
/// Combines <see cref="IStringLengthExpression{TBuilder,T}"/>, <see cref="IStringContentExpression{TBuilder,T}"/>,
/// <see cref="IStringStateExpression{TBuilder,T}"/>, and <see cref="IStringFormatExpression{TBuilder,T}"/>.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder that supports method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being validated.</typeparam>
public interface IStringExpression<out TBuilder, T>
    : IStringLengthExpression<TBuilder, T>
    , IStringContentExpression<TBuilder, T>
    , IStringStateExpression<TBuilder, T>
    , IStringFormatExpression<TBuilder, T>
{
}
