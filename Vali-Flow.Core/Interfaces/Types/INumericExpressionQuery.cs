using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for EF Core-safe numeric expression conditions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder implementing the interface.</typeparam>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface INumericExpressionQuery<out TBuilder, T>
{
    // ── int ───────────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="int"/> equals zero.</summary>
    TBuilder Zero(Expression<Func<T, int>> selector);

    /// <summary>Validates that the selected <see cref="int"/> is not zero.</summary>
    TBuilder NotZero(Expression<Func<T, int>> selector);

    /// <summary>Validates that the selected <see cref="int"/> is greater than zero.</summary>
    TBuilder Positive(Expression<Func<T, int>> selector);

    /// <summary>Validates that the selected <see cref="int"/> is less than zero.</summary>
    TBuilder Negative(Expression<Func<T, int>> selector);

    /// <summary>Validates that the selected <see cref="int"/> is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, int>> selector, int value);

    /// <summary>Validates that the selected <see cref="int"/> is greater than or equal to <paramref name="value"/>.</summary>
    TBuilder GreaterThanOrEqualTo(Expression<Func<T, int>> selector, int value);

    /// <summary>Validates that the selected <see cref="int"/> is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, int>> selector, int value);

    /// <summary>Validates that the selected <see cref="int"/> is less than or equal to <paramref name="value"/>.</summary>
    TBuilder LessThanOrEqualTo(Expression<Func<T, int>> selector, int value);

    /// <summary>Validates that the selected <see cref="int"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    TBuilder MinValue(Expression<Func<T, int>> selector, int minValue);

    /// <summary>Validates that the selected <see cref="int"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    TBuilder MaxValue(Expression<Func<T, int>> selector, int maxValue);

    /// <summary>Validates that the selected <see cref="int"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, int>> selector, int min, int max);

    /// <summary>Validates that the selected <see cref="int"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    TBuilder InRange(Expression<Func<T, int>> selector,
        Expression<Func<T, int>> minSelector, Expression<Func<T, int>> maxSelector);

    /// <summary>Validates that the selected <see cref="int"/> is even.</summary>
    TBuilder IsEven(Expression<Func<T, int>> selector);

    /// <summary>Validates that the selected <see cref="int"/> is odd.</summary>
    TBuilder IsOdd(Expression<Func<T, int>> selector);

    /// <summary>Validates that the selected <see cref="int"/> is a multiple of <paramref name="divisor"/>.</summary>
    TBuilder IsMultipleOf(Expression<Func<T, int>> selector, int divisor);

    // ── long ──────────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="long"/> equals zero.</summary>
    TBuilder Zero(Expression<Func<T, long>> selector);

    /// <summary>Validates that the selected <see cref="long"/> does not equal zero.</summary>
    TBuilder NotZero(Expression<Func<T, long>> selector);

    /// <summary>Validates that the selected <see cref="long"/> is greater than zero.</summary>
    TBuilder Positive(Expression<Func<T, long>> selector);

    /// <summary>Validates that the selected <see cref="long"/> is less than zero.</summary>
    TBuilder Negative(Expression<Func<T, long>> selector);

    /// <summary>Validates that the selected <see cref="long"/> is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, long>> selector, long value);

    /// <summary>Validates that the selected <see cref="long"/> is greater than or equal to <paramref name="value"/>.</summary>
    TBuilder GreaterThanOrEqualTo(Expression<Func<T, long>> selector, long value);

    /// <summary>Validates that the selected <see cref="long"/> is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, long>> selector, long value);

    /// <summary>Validates that the selected <see cref="long"/> is less than or equal to <paramref name="value"/>.</summary>
    TBuilder LessThanOrEqualTo(Expression<Func<T, long>> selector, long value);

    /// <summary>Validates that the selected <see cref="long"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    TBuilder MinValue(Expression<Func<T, long>> selector, long minValue);

    /// <summary>Validates that the selected <see cref="long"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    TBuilder MaxValue(Expression<Func<T, long>> selector, long maxValue);

    /// <summary>Validates that the selected <see cref="long"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, long>> selector, long min, long max);

    /// <summary>Validates that the selected <see cref="long"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    TBuilder InRange(Expression<Func<T, long>> selector,
        Expression<Func<T, long>> minSelector, Expression<Func<T, long>> maxSelector);

    /// <summary>Validates that the selected <see cref="long"/> is even.</summary>
    TBuilder IsEven(Expression<Func<T, long>> selector);

    /// <summary>Validates that the selected <see cref="long"/> is odd.</summary>
    TBuilder IsOdd(Expression<Func<T, long>> selector);

    /// <summary>Validates that the selected <see cref="long"/> is a multiple of <paramref name="divisor"/>.</summary>
    TBuilder IsMultipleOf(Expression<Func<T, long>> selector, long divisor);

    // ── double ────────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="double"/> equals zero.</summary>
    TBuilder Zero(Expression<Func<T, double>> selector);

    /// <summary>Validates that the selected <see cref="double"/> does not equal zero.</summary>
    TBuilder NotZero(Expression<Func<T, double>> selector);

    /// <summary>Validates that the selected <see cref="double"/> is greater than zero.</summary>
    TBuilder Positive(Expression<Func<T, double>> selector);

    /// <summary>Validates that the selected <see cref="double"/> is less than zero.</summary>
    TBuilder Negative(Expression<Func<T, double>> selector);

    /// <summary>Validates that the selected <see cref="double"/> is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, double>> selector, double value);

    /// <summary>Validates that the selected <see cref="double"/> is greater than or equal to <paramref name="value"/>.</summary>
    TBuilder GreaterThanOrEqualTo(Expression<Func<T, double>> selector, double value);

    /// <summary>Validates that the selected <see cref="double"/> is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, double>> selector, double value);

    /// <summary>Validates that the selected <see cref="double"/> is less than or equal to <paramref name="value"/>.</summary>
    TBuilder LessThanOrEqualTo(Expression<Func<T, double>> selector, double value);

    /// <summary>Validates that the selected <see cref="double"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    TBuilder MinValue(Expression<Func<T, double>> selector, double minValue);

    /// <summary>Validates that the selected <see cref="double"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    TBuilder MaxValue(Expression<Func<T, double>> selector, double maxValue);

    /// <summary>Validates that the selected <see cref="double"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, double>> selector, double min, double max);

    /// <summary>Validates that the selected <see cref="double"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    TBuilder InRange(Expression<Func<T, double>> selector,
        Expression<Func<T, double>> minSelector, Expression<Func<T, double>> maxSelector);

    // ── decimal ───────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="decimal"/> equals zero.</summary>
    TBuilder Zero(Expression<Func<T, decimal>> selector);

    /// <summary>Validates that the selected <see cref="decimal"/> does not equal zero.</summary>
    TBuilder NotZero(Expression<Func<T, decimal>> selector);

    /// <summary>Validates that the selected <see cref="decimal"/> is greater than zero.</summary>
    TBuilder Positive(Expression<Func<T, decimal>> selector);

    /// <summary>Validates that the selected <see cref="decimal"/> is less than zero.</summary>
    TBuilder Negative(Expression<Func<T, decimal>> selector);

    /// <summary>Validates that the selected <see cref="decimal"/> is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, decimal>> selector, decimal value);

    /// <summary>Validates that the selected <see cref="decimal"/> is greater than or equal to <paramref name="value"/>.</summary>
    TBuilder GreaterThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value);

    /// <summary>Validates that the selected <see cref="decimal"/> is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, decimal>> selector, decimal value);

    /// <summary>Validates that the selected <see cref="decimal"/> is less than or equal to <paramref name="value"/>.</summary>
    TBuilder LessThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value);

    /// <summary>Validates that the selected <see cref="decimal"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    TBuilder MinValue(Expression<Func<T, decimal>> selector, decimal minValue);

    /// <summary>Validates that the selected <see cref="decimal"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    TBuilder MaxValue(Expression<Func<T, decimal>> selector, decimal maxValue);

    /// <summary>Validates that the selected <see cref="decimal"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, decimal>> selector, decimal min, decimal max);

    /// <summary>Validates that the selected <see cref="decimal"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    TBuilder InRange(Expression<Func<T, decimal>> selector,
        Expression<Func<T, decimal>> minSelector, Expression<Func<T, decimal>> maxSelector);

    // ── float ─────────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="float"/> equals zero.</summary>
    TBuilder Zero(Expression<Func<T, float>> selector);

    /// <summary>Validates that the selected <see cref="float"/> does not equal zero.</summary>
    TBuilder NotZero(Expression<Func<T, float>> selector);

    /// <summary>Validates that the selected <see cref="float"/> is greater than zero.</summary>
    TBuilder Positive(Expression<Func<T, float>> selector);

    /// <summary>Validates that the selected <see cref="float"/> is less than zero.</summary>
    TBuilder Negative(Expression<Func<T, float>> selector);

    /// <summary>Validates that the selected <see cref="float"/> is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, float>> selector, float value);

    /// <summary>Validates that the selected <see cref="float"/> is greater than or equal to <paramref name="value"/>.</summary>
    TBuilder GreaterThanOrEqualTo(Expression<Func<T, float>> selector, float value);

    /// <summary>Validates that the selected <see cref="float"/> is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, float>> selector, float value);

    /// <summary>Validates that the selected <see cref="float"/> is less than or equal to <paramref name="value"/>.</summary>
    TBuilder LessThanOrEqualTo(Expression<Func<T, float>> selector, float value);

    /// <summary>Validates that the selected <see cref="float"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    TBuilder MinValue(Expression<Func<T, float>> selector, float minValue);

    /// <summary>Validates that the selected <see cref="float"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    TBuilder MaxValue(Expression<Func<T, float>> selector, float maxValue);

    /// <summary>Validates that the selected <see cref="float"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, float>> selector, float min, float max);

    /// <summary>Validates that the selected <see cref="float"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    TBuilder InRange(Expression<Func<T, float>> selector,
        Expression<Func<T, float>> minSelector, Expression<Func<T, float>> maxSelector);

    // ── short ─────────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="short"/> equals zero.</summary>
    TBuilder Zero(Expression<Func<T, short>> selector);

    /// <summary>Validates that the selected <see cref="short"/> does not equal zero.</summary>
    TBuilder NotZero(Expression<Func<T, short>> selector);

    /// <summary>Validates that the selected <see cref="short"/> is greater than zero.</summary>
    TBuilder Positive(Expression<Func<T, short>> selector);

    /// <summary>Validates that the selected <see cref="short"/> is less than zero.</summary>
    TBuilder Negative(Expression<Func<T, short>> selector);

    /// <summary>Validates that the selected <see cref="short"/> is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, short>> selector, short value);

    /// <summary>Validates that the selected <see cref="short"/> is greater than or equal to <paramref name="value"/>.</summary>
    TBuilder GreaterThanOrEqualTo(Expression<Func<T, short>> selector, short value);

    /// <summary>Validates that the selected <see cref="short"/> is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, short>> selector, short value);

    /// <summary>Validates that the selected <see cref="short"/> is less than or equal to <paramref name="value"/>.</summary>
    TBuilder LessThanOrEqualTo(Expression<Func<T, short>> selector, short value);

    /// <summary>Validates that the selected <see cref="short"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    TBuilder MinValue(Expression<Func<T, short>> selector, short minValue);

    /// <summary>Validates that the selected <see cref="short"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    TBuilder MaxValue(Expression<Func<T, short>> selector, short maxValue);

    /// <summary>Validates that the selected <see cref="short"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, short>> selector, short min, short max);

    /// <summary>Validates that the selected <see cref="short"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    TBuilder InRange(Expression<Func<T, short>> selector,
        Expression<Func<T, short>> minSelector, Expression<Func<T, short>> maxSelector);

    // ── Nullable numeric ──────────────────────────────────────────────────────

    /// <summary>Validates that the selected nullable <see cref="int"/> is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, int?>> selector);

    /// <summary>Validates that the selected nullable <see cref="long"/> is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, long?>> selector);

    /// <summary>Validates that the selected nullable <see cref="decimal"/> is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, decimal?>> selector);

    /// <summary>Validates that the selected nullable <see cref="double"/> is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, double?>> selector);

    /// <summary>Validates that the selected nullable <see cref="float"/> is null or equal to zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, float?>> selector);

    /// <summary>Validates that the selected nullable <see cref="int"/> has a value.</summary>
    TBuilder HasValue(Expression<Func<T, int?>> selector);

    /// <summary>Validates that the selected nullable <see cref="long"/> has a value.</summary>
    TBuilder HasValue(Expression<Func<T, long?>> selector);

    /// <summary>Validates that the selected nullable <see cref="decimal"/> has a value.</summary>
    TBuilder HasValue(Expression<Func<T, decimal?>> selector);

    /// <summary>Validates that the selected nullable <see cref="double"/> has a value.</summary>
    TBuilder HasValue(Expression<Func<T, double?>> selector);

    /// <summary>Validates that the selected nullable <see cref="float"/> has a value (is not null).</summary>
    TBuilder HasValue(Expression<Func<T, float?>> selector);

    /// <summary>Validates that the selected nullable <see cref="int"/> has a value greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, int?>> selector, int value);

    /// <summary>Validates that the selected nullable <see cref="long"/> has a value greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, long?>> selector, long value);

    /// <summary>Validates that the selected nullable <see cref="decimal"/> has a value greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, decimal?>> selector, decimal value);

    /// <summary>Validates that the selected nullable <see cref="int"/> has a value less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, int?>> selector, int value);

    /// <summary>Validates that the selected nullable <see cref="long"/> has a value less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, long?>> selector, long value);

    /// <summary>Validates that the selected nullable <see cref="decimal"/> has a value less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, decimal?>> selector, decimal value);

    /// <summary>Validates that the selected nullable <see cref="int"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, int?>> selector, int min, int max);

    /// <summary>Validates that the selected nullable <see cref="long"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, long?>> selector, long min, long max);

    /// <summary>Validates that the selected nullable <see cref="decimal"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, decimal?>> selector, decimal min, decimal max);

    /// <summary>Validates that the selected nullable <see cref="double"/> has a value greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, double?>> selector, double value);

    /// <summary>Validates that the selected nullable <see cref="double"/> has a value less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, double?>> selector, double value);

    /// <summary>Validates that the selected nullable <see cref="double"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, double?>> selector, double min, double max);

    /// <summary>Validates that the selected nullable <see cref="float"/> has a value greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, float?>> selector, float value);

    /// <summary>Validates that the selected nullable <see cref="float"/> has a value less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, float?>> selector, float value);

    /// <summary>Validates that the selected nullable <see cref="float"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, float?>> selector, float min, float max);

    /// <summary>Validates that the selected nullable <see cref="short"/> is null or equal to zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, short?>> selector);

    /// <summary>Validates that the selected nullable <see cref="short"/> has a value (is not null).</summary>
    TBuilder HasValue(Expression<Func<T, short?>> selector);

    /// <summary>Validates that the selected nullable <see cref="short"/> has a value greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, short?>> selector, short value);

    /// <summary>Validates that the selected nullable <see cref="short"/> has a value less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, short?>> selector, short value);

    /// <summary>Validates that the selected nullable <see cref="short"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, short?>> selector, short min, short max);
}
