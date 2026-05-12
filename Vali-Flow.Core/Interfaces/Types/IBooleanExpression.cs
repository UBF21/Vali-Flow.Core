using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for constructing Vali-Flow.Core conditions based on boolean property evaluations.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder implementing the interface.</typeparam>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface IBooleanExpression<out TBuilder,T>
{
    /// <summary>Validates that the selected boolean property evaluates to <see langword="true"/>.</summary>
    /// <param name="selector">Expression selecting the boolean property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsTrue(Expression<Func<T, bool>> selector);

    /// <summary>Validates that the selected boolean property evaluates to <see langword="false"/>.</summary>
    /// <param name="selector">Expression selecting the boolean property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsFalse(Expression<Func<T, bool>> selector);
}