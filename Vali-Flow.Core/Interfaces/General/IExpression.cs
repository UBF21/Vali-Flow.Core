namespace Vali_Flow.Core.Interfaces.General;

/// <summary>
/// Combined expression interface — backward-compatible aggregate of all segregated expression interfaces.
/// Consumers can depend on the narrower interfaces (<see cref="IExpressionBuilder{TBuilder,T}"/>,
/// <see cref="IExpressionEvaluator{T}"/>, <see cref="IExpressionCompiler{T}"/>,
/// <see cref="IExpressionLifecycle{TBuilder}"/>) when they only need a subset of the contract.
/// </summary>
/// <typeparam name="TBuilder">The specific builder type that implements this interface.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface IExpression<out TBuilder, T> :
    IExpressionBuilder<TBuilder, T>,
    IExpressionEvaluator<T>,
    IExpressionCompiler<T>,
    IExpressionLifecycle<TBuilder>,
    INestedValidation<TBuilder, T>
{
}
