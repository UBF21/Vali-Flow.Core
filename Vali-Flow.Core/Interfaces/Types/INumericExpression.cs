using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for constructing Vali-Flow.Core conditions based on numeric property evaluations.
/// </summary>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
public interface INumericExpression<out TBuilder, T>
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

    /// <summary>
    /// Adds a condition to check if the selected integer property is greater than the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the integer property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder GreaterThan(Expression<Func<T, int>> selector, int value);

    /// <summary>
    /// Adds a condition to check if the selected long property is greater than the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the long property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder GreaterThan(Expression<Func<T, long>> selector, long value);

    /// <summary>
    /// Adds a condition to check if the selected float property is greater than the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the float property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder GreaterThan(Expression<Func<T, float>> selector, float value);

    /// <summary>
    /// Adds a condition to check if the selected double property is greater than the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the double property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder GreaterThan(Expression<Func<T, double>> selector, double value);

    /// <summary>
    /// Adds a condition to check if the selected decimal property is greater than the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the decimal property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder GreaterThan(Expression<Func<T, decimal>> selector, decimal value);

    /// <summary>
    /// Adds a condition to check if the selected short property is greater than the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the short property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder GreaterThan(Expression<Func<T, short>> selector, short value);

    /// <summary>
    /// Adds a condition to check if the selected integer property is greater than or equal to the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the integer property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder GreaterThanOrEqualTo(Expression<Func<T, int>> selector, int value);

    /// <summary>
    /// Adds a condition to check if the selected long property is greater than or equal to the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the long property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder GreaterThanOrEqualTo(Expression<Func<T, long>> selector, long value);

    /// <summary>
    /// Adds a condition to check if the selected float property is greater than or equal to the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the float property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder GreaterThanOrEqualTo(Expression<Func<T, float>> selector, float value);

    /// <summary>
    /// Adds a condition to check if the selected double property is greater than or equal to the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the double property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder GreaterThanOrEqualTo(Expression<Func<T, double>> selector, double value);

    /// <summary>
    /// Adds a condition to check if the selected decimal property is greater than or equal to the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the decimal property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder GreaterThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value);

    /// <summary>
    /// Adds a condition to check if the selected short property is greater than or equal to the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the short property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder GreaterThanOrEqualTo(Expression<Func<T, short>> selector, short value);

    /// <summary>
    /// Adds a condition to check if the selected integer property is less than the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the integer property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder LessThan(Expression<Func<T, int>> selector, int value);

    /// <summary>
    /// Adds a condition to check if the selected long property is less than the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the long property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder LessThan(Expression<Func<T, long>> selector, long value);

    /// <summary>
    /// Adds a condition to check if the selected float property is less than the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the float property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder LessThan(Expression<Func<T, float>> selector, float value);

    /// <summary>
    /// Adds a condition to check if the selected double property is less than the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the double property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder LessThan(Expression<Func<T, double>> selector, double value);

    /// <summary>
    /// Adds a condition to check if the selected decimal property is less than the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the decimal property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder LessThan(Expression<Func<T, decimal>> selector, decimal value);

    /// <summary>
    /// Adds a condition to check if the selected short property is less than the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the short property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder LessThan(Expression<Func<T, short>> selector, short value);

    /// <summary>
    /// Adds a condition to check if the selected integer property is less than or equal to the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the integer property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder LessThanOrEqualTo(Expression<Func<T, int>> selector, int value);

    /// <summary>
    /// Adds a condition to check if the selected long property is less than or equal to the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the long property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder LessThanOrEqualTo(Expression<Func<T, long>> selector, long value);

    /// <summary>
    /// Adds a condition to check if the selected float property is less than or equal to the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the float property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder LessThanOrEqualTo(Expression<Func<T, float>> selector, float value);

    /// <summary>
    /// Adds a condition to check if the selected double property is less than or equal to the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the double property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder LessThanOrEqualTo(Expression<Func<T, double>> selector, double value);

    /// <summary>
    /// Adds a condition to check if the selected decimal property is less than or equal to the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the decimal property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder LessThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value);

    /// <summary>
    /// Adds a condition to check if the selected short property is less than or equal to the specified value.
    /// </summary>
    /// <param name="selector">An expression to select the short property to evaluate.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder LessThanOrEqualTo(Expression<Func<T, short>> selector, short value);

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

    /// <summary>
    /// Adds a condition to check if the selected integer property is greater than or equal to the specified minimum value.
    /// </summary>
    /// <param name="selector">An expression to select the integer property to evaluate.</param>
    /// <param name="minValue">The minimum value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder MinValue(Expression<Func<T, int>> selector, int minValue);

    /// <summary>
    /// Adds a condition to check if the selected long property is greater than or equal to the specified minimum value.
    /// </summary>
    /// <param name="selector">An expression to select the long property to evaluate.</param>
    /// <param name="minValue">The minimum value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder MinValue(Expression<Func<T, long>> selector, long minValue);

    /// <summary>
    /// Adds a condition to check if the selected float property is greater than or equal to the specified minimum value.
    /// </summary>
    /// <param name="selector">An expression to select the float property to evaluate.</param>
    /// <param name="minValue">The minimum value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder MinValue(Expression<Func<T, float>> selector, float minValue);

    /// <summary>
    /// Adds a condition to check if the selected double property is greater than or equal to the specified minimum value.
    /// </summary>
    /// <param name="selector">An expression to select the double property to evaluate.</param>
    /// <param name="minValue">The minimum value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder MinValue(Expression<Func<T, double>> selector, double minValue);

    /// <summary>
    /// Adds a condition to check if the selected decimal property is greater than or equal to the specified minimum value.
    /// </summary>
    /// <param name="selector">An expression to select the decimal property to evaluate.</param>
    /// <param name="minValue">The minimum value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder MinValue(Expression<Func<T, decimal>> selector, decimal minValue);

    /// <summary>
    /// Adds a condition to check if the selected short property is greater than or equal to the specified minimum value.
    /// </summary>
    /// <param name="selector">An expression to select the short property to evaluate.</param>
    /// <param name="minValue">The minimum value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder MinValue(Expression<Func<T, short>> selector, short minValue);

    /// <summary>
    /// Adds a condition to check if the selected integer property is less than or equal to the specified maximum value.
    /// </summary>
    /// <param name="selector">An expression to select the integer property to evaluate.</param>
    /// <param name="maxValue">The maximum value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder MaxValue(Expression<Func<T, int>> selector, int maxValue);

    /// <summary>
    /// Adds a condition to check if the selected long property is less than or equal to the specified maximum value.
    /// </summary>
    /// <param name="selector">An expression to select the long property to evaluate.</param>
    /// <param name="maxValue">The maximum value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder MaxValue(Expression<Func<T, long>> selector, long maxValue);

    /// <summary>
    /// Adds a condition to check if the selected float property is less than or equal to the specified maximum value.
    /// </summary>
    /// <param name="selector">An expression to select the float property to evaluate.</param>
    /// <param name="maxValue">The maximum value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder MaxValue(Expression<Func<T, float>> selector, float maxValue);

    /// <summary>
    /// Adds a condition to check if the selected double property is less than or equal to the specified maximum value.
    /// </summary>
    /// <param name="selector">An expression to select the double property to evaluate.</param>
    /// <param name="maxValue">The maximum value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder MaxValue(Expression<Func<T, double>> selector, double maxValue);

    /// <summary>
    /// Adds a condition to check if the selected decimal property is less than or equal to the specified maximum value.
    /// </summary>
    /// <param name="selector">An expression to select the decimal property to evaluate.</param>
    /// <param name="maxValue">The maximum value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder MaxValue(Expression<Func<T, decimal>> selector, decimal maxValue);

    /// <summary>
    /// Adds a condition to check if the selected short property is less than or equal to the specified maximum value.
    /// </summary>
    /// <param name="selector">An expression to select the short property to evaluate.</param>
    /// <param name="maxValue">The maximum value to compare against.</param>
    /// <returns>The builder instance to enable method chaining.</returns>
    TBuilder MaxValue(Expression<Func<T, short>> selector, short maxValue);

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

    // ── IsBetweenExclusive ─────────────────────────────────────────────────────

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

    // ── IsCloseTo ──────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected double value is within <paramref name="tolerance"/> of <paramref name="value"/> (i.e. |val - value| ≤ tolerance).</summary>
    TBuilder IsCloseTo(Expression<Func<T, double>> selector, double value, double tolerance);

    /// <summary>Validates that the selected float value is within <paramref name="tolerance"/> of <paramref name="value"/> (i.e. |val - value| ≤ tolerance).</summary>
    TBuilder IsCloseTo(Expression<Func<T, float>> selector, float value, float tolerance);

    // ── IComparable<TValue> generic overloads ──────────────────────────────────

    /// <summary>Adds a condition to check if the selected property is greater than <paramref name="value"/> using IComparable&lt;TValue&gt;.</summary>
    TBuilder GreaterThan<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the selected property is greater than or equal to <paramref name="value"/> using IComparable&lt;TValue&gt;.</summary>
    TBuilder GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the selected property is less than <paramref name="value"/> using IComparable&lt;TValue&gt;.</summary>
    TBuilder LessThan<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the selected property is less than or equal to <paramref name="value"/> using IComparable&lt;TValue&gt;.</summary>
    TBuilder LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the selected property is within [<paramref name="min"/>, <paramref name="max"/>] using IComparable&lt;TValue&gt;.</summary>
    TBuilder InRange<TValue>(Expression<Func<T, TValue>> selector, TValue min, TValue max) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the selected property is equal to <paramref name="value"/> using IComparable&lt;TValue&gt;.</summary>
    TBuilder EqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : IComparable<TValue>;

    // ── Cross-property comparisons ─────────────────────────────────────────────

    /// <summary>Adds a condition to check if the property selected by <paramref name="selector"/> is greater than the property selected by <paramref name="otherSelector"/>.</summary>
    TBuilder GreaterThan<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the property selected by <paramref name="selector"/> is greater than or equal to the property selected by <paramref name="otherSelector"/>.</summary>
    TBuilder GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the property selected by <paramref name="selector"/> is less than the property selected by <paramref name="otherSelector"/>.</summary>
    TBuilder LessThan<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector) where TValue : IComparable<TValue>;

    /// <summary>Adds a condition to check if the property selected by <paramref name="selector"/> is less than or equal to the property selected by <paramref name="otherSelector"/>.</summary>
    TBuilder LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector) where TValue : IComparable<TValue>;

    // ── Nullable numeric ───────────────────────────────────────────────────────

    /// <summary>Adds a condition to check if the selected nullable int property is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, int?>> selector);

    /// <summary>Adds a condition to check if the selected nullable decimal property is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, decimal?>> selector);

    /// <summary>Adds a condition to check if the selected nullable long property is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, long?>> selector);

    /// <summary>Adds a condition to check if the selected nullable double property is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, double?>> selector);

    /// <summary>Adds a condition to check if the selected nullable float property is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, float?>> selector);

    /// <summary>Adds a condition to check if the selected nullable int property has a value.</summary>
    TBuilder HasValue(Expression<Func<T, int?>> selector);

    /// <summary>Adds a condition to check if the selected nullable decimal property has a value.</summary>
    TBuilder HasValue(Expression<Func<T, decimal?>> selector);

    /// <summary>Adds a condition to check if the selected nullable long property has a value.</summary>
    TBuilder HasValue(Expression<Func<T, long?>> selector);

    /// <summary>Adds a condition to check if the selected nullable double property has a value.</summary>
    TBuilder HasValue(Expression<Func<T, double?>> selector);

    /// <summary>Adds a condition to check if the selected nullable float property has a value.</summary>
    TBuilder HasValue(Expression<Func<T, float?>> selector);

    /// <summary>Adds a condition to check if the selected nullable short property is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, short?>> selector);

    /// <summary>Adds a condition to check if the selected nullable short property has a value.</summary>
    TBuilder HasValue(Expression<Func<T, short?>> selector);

    /// <summary>Adds a condition to check if the selected nullable int property has a value and is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, int?>> selector, int value);

    /// <summary>Adds a condition to check if the selected nullable decimal property has a value and is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, decimal?>> selector, decimal value);

    /// <summary>Adds a condition to check if the selected nullable long property has a value and is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, long?>> selector, long value);

    /// <summary>Adds a condition to check if the selected nullable double property has a value and is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, double?>> selector, double value);

    /// <summary>Adds a condition to check if the selected nullable float property has a value and is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, float?>> selector, float value);

    /// <summary>Adds a condition to check if the selected nullable short property has a value and is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, short?>> selector, short value);

    /// <summary>Adds a condition to check if the selected nullable int property has a value and is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, int?>> selector, int value);

    /// <summary>Adds a condition to check if the selected nullable decimal property has a value and is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, decimal?>> selector, decimal value);

    /// <summary>Adds a condition to check if the selected nullable long property has a value and is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, long?>> selector, long value);

    /// <summary>Adds a condition to check if the selected nullable double property has a value and is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, double?>> selector, double value);

    /// <summary>Adds a condition to check if the selected nullable float property has a value and is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, float?>> selector, float value);

    /// <summary>Adds a condition to check if the selected nullable short property has a value and is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, short?>> selector, short value);

    /// <summary>Adds a condition to check if the selected nullable int property has a value and is within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, int?>> selector, int min, int max);

    /// <summary>Adds a condition to check if the selected nullable decimal property has a value and is within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, decimal?>> selector, decimal min, decimal max);

    /// <summary>Adds a condition to check if the selected nullable long property has a value and is within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, long?>> selector, long min, long max);

    /// <summary>Adds a condition to check if the selected nullable double property has a value and is within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, double?>> selector, double min, double max);

    /// <summary>Adds a condition to check if the selected nullable float property has a value and is within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, float?>> selector, float min, float max);

    /// <summary>Adds a condition to check if the selected nullable short property has a value and is within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, short?>> selector, short min, short max);
}
