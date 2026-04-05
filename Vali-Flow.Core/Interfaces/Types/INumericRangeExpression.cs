using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for numeric range validations: InRange (scalar and cross-property), IsBetweenExclusive, and IsCloseTo.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface INumericRangeExpression<out TBuilder, T>
{
    /// <summary>
    /// Adds a condition to check if the selected integer property is within the specified range (inclusive).
    /// </summary>
    /// <param name="selector">An expression to select the integer property to evaluate.</param>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (inclusive).</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder InRange(Expression<Func<T, int>> selector, int min, int max);

    /// <summary>
    /// Adds a condition to check if the selected long property is within the specified range (inclusive).
    /// </summary>
    /// <param name="selector">An expression to select the long property to evaluate.</param>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (inclusive).</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder InRange(Expression<Func<T, long>> selector, long min, long max);

    /// <summary>
    /// Adds a condition to check if the selected float property is within the specified range (inclusive).
    /// </summary>
    /// <param name="selector">An expression to select the float property to evaluate.</param>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (inclusive).</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder InRange(Expression<Func<T, float>> selector, float min, float max);

    /// <summary>
    /// Adds a condition to check if the selected double property is within the specified range (inclusive).
    /// </summary>
    /// <param name="selector">An expression to select the double property to evaluate.</param>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (inclusive).</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder InRange(Expression<Func<T, double>> selector, double min, double max);

    /// <summary>
    /// Adds a condition to check if the selected decimal property is within the specified range (inclusive).
    /// </summary>
    /// <param name="selector">An expression to select the decimal property to evaluate.</param>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (inclusive).</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder InRange(Expression<Func<T, decimal>> selector, decimal min, decimal max);

    /// <summary>
    /// Adds a condition to check if the selected short property is within the specified range (inclusive).
    /// </summary>
    /// <param name="selector">An expression to select the short property to evaluate.</param>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (inclusive).</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder InRange(Expression<Func<T, short>> selector, short min, short max);

    /// <summary>
    /// Adds a condition to check if the selected integer property is within the range defined by the minimum and maximum values of other properties (inclusive).
    /// </summary>
    /// <param name="selector">An expression to select the integer property to evaluate.</param>
    /// <param name="minSelector">An expression to select the property providing the minimum value of the range.</param>
    /// <param name="maxSelector">An expression to select the property providing the maximum value of the range.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder InRange(Expression<Func<T, int>> selector, Expression<Func<T, int>> minSelector,
        Expression<Func<T, int>> maxSelector);

    /// <summary>
    /// Adds a condition to check if the selected long property is within the range defined by the minimum and maximum values of other properties (inclusive).
    /// </summary>
    /// <param name="selector">An expression to select the long property to evaluate.</param>
    /// <param name="minSelector">An expression to select the property providing the minimum value of the range.</param>
    /// <param name="maxSelector">An expression to select the property providing the maximum value of the range.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder InRange(Expression<Func<T, long>> selector, Expression<Func<T, long>> minSelector,
        Expression<Func<T, long>> maxSelector);

    /// <summary>
    /// Adds a condition to check if the selected float property is within the range defined by the minimum and maximum values of other properties (inclusive).
    /// </summary>
    /// <param name="selector">An expression to select the float property to evaluate.</param>
    /// <param name="minSelector">An expression to select the property providing the minimum value of the range.</param>
    /// <param name="maxSelector">An expression to select the property providing the maximum value of the range.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder InRange(Expression<Func<T, float>> selector, Expression<Func<T, float>> minSelector,
        Expression<Func<T, float>> maxSelector);

    /// <summary>
    /// Adds a condition to check if the selected double property is within the range defined by the minimum and maximum values of other properties (inclusive).
    /// </summary>
    /// <param name="selector">An expression to select the double property to evaluate.</param>
    /// <param name="minSelector">An expression to select the property providing the minimum value of the range.</param>
    /// <param name="maxSelector">An expression to select the property providing the maximum value of the range.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder InRange(Expression<Func<T, double>> selector, Expression<Func<T, double>> minSelector,
        Expression<Func<T, double>> maxSelector);

    /// <summary>
    /// Adds a condition to check if the selected decimal property is within the range defined by the minimum and maximum values of other properties (inclusive).
    /// </summary>
    /// <param name="selector">An expression to select the decimal property to evaluate.</param>
    /// <param name="minSelector">An expression to select the property providing the minimum value of the range.</param>
    /// <param name="maxSelector">An expression to select the property providing the maximum value of the range.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder InRange(Expression<Func<T, decimal>> selector, Expression<Func<T, decimal>> minSelector,
        Expression<Func<T, decimal>> maxSelector);

    /// <summary>
    /// Adds a condition to check if the selected short property is within the range defined by the minimum and maximum values of other properties (inclusive).
    /// </summary>
    /// <param name="selector">An expression to select the short property to evaluate.</param>
    /// <param name="minSelector">An expression to select the property providing the minimum value of the range.</param>
    /// <param name="maxSelector">An expression to select the property providing the maximum value of the range.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder InRange(Expression<Func<T, short>> selector, Expression<Func<T, short>> minSelector,
        Expression<Func<T, short>> maxSelector);

    /// <summary>Validates that the selected integer is strictly between <paramref name="min"/> and <paramref name="max"/> (both exclusive).</summary>
    TBuilder IsBetweenExclusive(Expression<Func<T, int>> selector, int min, int max);

    /// <summary>Validates that the selected long is strictly between <paramref name="min"/> and <paramref name="max"/> (both exclusive).</summary>
    TBuilder IsBetweenExclusive(Expression<Func<T, long>> selector, long min, long max);

    /// <summary>Validates that the selected float is strictly between <paramref name="min"/> and <paramref name="max"/> (both exclusive).</summary>
    TBuilder IsBetweenExclusive(Expression<Func<T, float>> selector, float min, float max);

    /// <summary>Validates that the selected double is strictly between <paramref name="min"/> and <paramref name="max"/> (both exclusive).</summary>
    TBuilder IsBetweenExclusive(Expression<Func<T, double>> selector, double min, double max);

    /// <summary>Validates that the selected decimal is strictly between <paramref name="min"/> and <paramref name="max"/> (both exclusive).</summary>
    TBuilder IsBetweenExclusive(Expression<Func<T, decimal>> selector, decimal min, decimal max);

    /// <summary>Validates that the selected short is strictly between <paramref name="min"/> and <paramref name="max"/> (both exclusive).</summary>
    TBuilder IsBetweenExclusive(Expression<Func<T, short>> selector, short min, short max);

    /// <summary>Validates that the selected double value is within <paramref name="tolerance"/> of <paramref name="value"/> (i.e. |val - value| ≤ tolerance).</summary>
    TBuilder IsCloseTo(Expression<Func<T, double>> selector, double value, double tolerance);

    /// <summary>Validates that the selected float value is within <paramref name="tolerance"/> of <paramref name="value"/> (i.e. |val - value| ≤ tolerance).</summary>
    TBuilder IsCloseTo(Expression<Func<T, float>> selector, float value, float tolerance);
}
