using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for checking the sign of numeric properties (zero, not-zero, positive, negative).
/// </summary>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface INumericSignExpression<out TBuilder, T>
{
    /// <summary>
    /// Adds a condition to check if the selected integer property is zero.
    /// </summary>
    /// <param name="selector">An expression to select the integer property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Zero(Expression<Func<T, int>> selector);

    /// <summary>
    /// Adds a condition to check if the selected long property is zero.
    /// </summary>
    /// <param name="selector">An expression to select the long property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Zero(Expression<Func<T, long>> selector);

    /// <summary>
    /// Adds a condition to check if the selected float property is zero.
    /// </summary>
    /// <param name="selector">An expression to select the float property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Zero(Expression<Func<T, float>> selector);

    /// <summary>
    /// Adds a condition to check if the selected double property is zero.
    /// </summary>
    /// <param name="selector">An expression to select the double property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Zero(Expression<Func<T, double>> selector);

    /// <summary>
    /// Adds a condition to check if the selected decimal property is zero.
    /// </summary>
    /// <param name="selector">An expression to select the decimal property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Zero(Expression<Func<T, decimal>> selector);

    /// <summary>
    /// Adds a condition to check if the selected short property is zero.
    /// </summary>
    /// <param name="selector">An expression to select the short property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Zero(Expression<Func<T, short>> selector);

    /// <summary>
    /// Adds a condition to check if the selected integer property is not zero.
    /// </summary>
    /// <param name="selector">An expression to select the integer property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder NotZero(Expression<Func<T, int>> selector);

    /// <summary>
    /// Adds a condition to check if the selected long property is not zero.
    /// </summary>
    /// <param name="selector">An expression to select the long property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder NotZero(Expression<Func<T, long>> selector);

    /// <summary>
    /// Adds a condition to check if the selected float property is not zero.
    /// </summary>
    /// <param name="selector">An expression to select the float property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder NotZero(Expression<Func<T, float>> selector);

    /// <summary>
    /// Adds a condition to check if the selected double property is not zero.
    /// </summary>
    /// <param name="selector">An expression to select the double property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder NotZero(Expression<Func<T, double>> selector);

    /// <summary>
    /// Adds a condition to check if the selected decimal property is not zero.
    /// </summary>
    /// <param name="selector">An expression to select the decimal property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder NotZero(Expression<Func<T, decimal>> selector);

    /// <summary>
    /// Adds a condition to check if the selected short property is not zero.
    /// </summary>
    /// <param name="selector">An expression to select the short property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder NotZero(Expression<Func<T, short>> selector);

    /// <summary>
    /// Adds a condition to check if the selected integer property is positive (greater than zero).
    /// </summary>
    /// <param name="selector">An expression to select the integer property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Positive(Expression<Func<T, int>> selector);

    /// <summary>
    /// Adds a condition to check if the selected long property is positive (greater than zero).
    /// </summary>
    /// <param name="selector">An expression to select the long property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Positive(Expression<Func<T, long>> selector);

    /// <summary>
    /// Adds a condition to check if the selected float property is positive (greater than zero).
    /// </summary>
    /// <param name="selector">An expression to select the float property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Positive(Expression<Func<T, float>> selector);

    /// <summary>
    /// Adds a condition to check if the selected double property is positive (greater than zero).
    /// </summary>
    /// <param name="selector">An expression to select the double property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Positive(Expression<Func<T, double>> selector);

    /// <summary>
    /// Adds a condition to check if the selected decimal property is positive (greater than zero).
    /// </summary>
    /// <param name="selector">An expression to select the decimal property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Positive(Expression<Func<T, decimal>> selector);

    /// <summary>
    /// Adds a condition to check if the selected short property is positive (greater than zero).
    /// </summary>
    /// <param name="selector">An expression to select the short property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Positive(Expression<Func<T, short>> selector);

    /// <summary>
    /// Adds a condition to check if the selected integer property is negative (less than zero).
    /// </summary>
    /// <param name="selector">An expression to select the integer property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Negative(Expression<Func<T, int>> selector);

    /// <summary>
    /// Adds a condition to check if the selected long property is negative (less than zero).
    /// </summary>
    /// <param name="selector">An expression to select the long property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Negative(Expression<Func<T, long>> selector);

    /// <summary>
    /// Adds a condition to check if the selected float property is negative (less than zero).
    /// </summary>
    /// <param name="selector">An expression to select the float property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Negative(Expression<Func<T, float>> selector);

    /// <summary>
    /// Adds a condition to check if the selected double property is negative (less than zero).
    /// </summary>
    /// <param name="selector">An expression to select the double property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Negative(Expression<Func<T, double>> selector);

    /// <summary>
    /// Adds a condition to check if the selected decimal property is negative (less than zero).
    /// </summary>
    /// <param name="selector">An expression to select the decimal property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Negative(Expression<Func<T, decimal>> selector);

    /// <summary>
    /// Adds a condition to check if the selected short property is negative (less than zero).
    /// </summary>
    /// <param name="selector">An expression to select the short property to evaluate.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder Negative(Expression<Func<T, short>> selector);
}
