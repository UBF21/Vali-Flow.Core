using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for scalar numeric comparisons: GreaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo, MinValue, MaxValue.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface INumericComparisonExpression<out TBuilder, T>
{
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
}
