using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

/// <summary>
/// EF Core-safe numeric validation conditions for the fluent builder,
/// covering <see langword="int"/>, <see langword="long"/>, <see langword="double"/>,
/// <see langword="decimal"/>, <see langword="float"/>, <see langword="short"/>,
/// and their nullable counterparts.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type returned by each fluent method.</typeparam>
/// <typeparam name="T">The entity type being validated.</typeparam>
public class NumericExpressionQuery<TBuilder, T> : INumericExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    /// <summary>The underlying builder that accumulates conditions.</summary>
    private readonly BaseExpression<TBuilder, T> _builder;

    /// <summary>
    /// Initializes a new instance of <see cref="NumericExpressionQuery{TBuilder,T}"/>.
    /// </summary>
    /// <param name="builder">The parent builder that owns the condition list.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public NumericExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    // ── int ───────────────────────────────────────────────────────────────────

    /// <summary>Adds a condition that the selected <see langword="int"/> equals zero.</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Zero(Expression<Func<T, int>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val == 0; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="int"/> is not zero.</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder NotZero(Expression<Func<T, int>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val != 0; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="int"/> is greater than zero.</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Positive(Expression<Func<T, int>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val > 0; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="int"/> is less than zero.</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Negative(Expression<Func<T, int>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val < 0; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="int"/> is greater than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan(Expression<Func<T, int>> selector, int value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val > value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="int"/> is greater than or equal to <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <param name="value">The inclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, int>> selector, int value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val >= value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="int"/> is less than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan(Expression<Func<T, int>> selector, int value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val < value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="int"/> is less than or equal to <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <param name="value">The inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThanOrEqualTo(Expression<Func<T, int>> selector, int value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val <= value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="int"/> is at least <paramref name="minValue"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <param name="minValue">The inclusive minimum value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MinValue(Expression<Func<T, int>> selector, int minValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val >= minValue; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="int"/> is at most <paramref name="maxValue"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <param name="maxValue">The inclusive maximum value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MaxValue(Expression<Func<T, int>> selector, int maxValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val <= maxValue; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="int"/> falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder InRange(Expression<Func<T, int>> selector, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<int, bool>> p = val => val >= min && val <= max;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see langword="int"/> falls within the range defined by two other properties on the same entity.</summary>
    /// <param name="selector">Property selector for the value to test.</param>
    /// <param name="minSelector">Property selector for the inclusive lower bound.</param>
    /// <param name="maxSelector">Property selector for the inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder InRange(Expression<Func<T, int>> selector,
        Expression<Func<T, int>> minSelector, Expression<Func<T, int>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    /// <summary>Adds a condition that the selected <see langword="int"/> is even.</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsEven(Expression<Func<T, int>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val % 2 == 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected <see langword="int"/> is odd.</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsOdd(Expression<Func<T, int>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val % 2 != 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected <see langword="int"/> is a multiple of <paramref name="divisor"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="int"/> member.</param>
    /// <param name="divisor">The divisor; must not be zero.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="divisor"/> is zero.</exception>
    public TBuilder IsMultipleOf(Expression<Func<T, int>> selector, int divisor)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (divisor == 0) throw new ArgumentOutOfRangeException(nameof(divisor), "divisor must not be zero.");
        Expression<Func<int, bool>> predicate = val => val % divisor == 0;
        return _builder.Add(selector, predicate);
    }

    // ── long ──────────────────────────────────────────────────────────────────

    /// <summary>Adds a condition that the selected <see langword="long"/> equals zero.</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Zero(Expression<Func<T, long>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val == 0L; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="long"/> is not zero.</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder NotZero(Expression<Func<T, long>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val != 0L; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="long"/> is greater than zero.</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Positive(Expression<Func<T, long>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val > 0L; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="long"/> is less than zero.</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Negative(Expression<Func<T, long>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val < 0L; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="long"/> is greater than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan(Expression<Func<T, long>> selector, long value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val > value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="long"/> is greater than or equal to <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <param name="value">The inclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, long>> selector, long value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val >= value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="long"/> is less than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan(Expression<Func<T, long>> selector, long value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val < value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="long"/> is less than or equal to <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <param name="value">The inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThanOrEqualTo(Expression<Func<T, long>> selector, long value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val <= value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="long"/> is at least <paramref name="minValue"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <param name="minValue">The inclusive minimum value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MinValue(Expression<Func<T, long>> selector, long minValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val >= minValue; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="long"/> is at most <paramref name="maxValue"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <param name="maxValue">The inclusive maximum value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MaxValue(Expression<Func<T, long>> selector, long maxValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val <= maxValue; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="long"/> falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder InRange(Expression<Func<T, long>> selector, long min, long max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<long, bool>> p = val => val >= min && val <= max;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see langword="long"/> falls within the range defined by two other properties on the same entity.</summary>
    /// <param name="selector">Property selector for the value to test.</param>
    /// <param name="minSelector">Property selector for the inclusive lower bound.</param>
    /// <param name="maxSelector">Property selector for the inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder InRange(Expression<Func<T, long>> selector,
        Expression<Func<T, long>> minSelector, Expression<Func<T, long>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    /// <summary>Adds a condition that the selected <see langword="long"/> is even.</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsEven(Expression<Func<T, long>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val % 2 == 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected <see langword="long"/> is odd.</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsOdd(Expression<Func<T, long>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val % 2 != 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected <see langword="long"/> is a multiple of <paramref name="divisor"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="long"/> member.</param>
    /// <param name="divisor">The divisor; must not be zero.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="divisor"/> is zero.</exception>
    public TBuilder IsMultipleOf(Expression<Func<T, long>> selector, long divisor)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (divisor == 0) throw new ArgumentOutOfRangeException(nameof(divisor), "divisor must not be zero.");
        Expression<Func<long, bool>> predicate = val => val % divisor == 0;
        return _builder.Add(selector, predicate);
    }

    // ── double ────────────────────────────────────────────────────────────────

    /// <summary>Adds a condition that the selected <see langword="double"/> equals zero.</summary>
    /// <param name="selector">Property selector for the <see langword="double"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Zero(Expression<Func<T, double>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val == 0.0; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="double"/> is not zero.</summary>
    /// <param name="selector">Property selector for the <see langword="double"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder NotZero(Expression<Func<T, double>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val != 0.0; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="double"/> is greater than zero.</summary>
    /// <param name="selector">Property selector for the <see langword="double"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Positive(Expression<Func<T, double>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val > 0.0; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="double"/> is less than zero.</summary>
    /// <param name="selector">Property selector for the <see langword="double"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Negative(Expression<Func<T, double>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val < 0.0; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="double"/> is greater than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="double"/> member.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan(Expression<Func<T, double>> selector, double value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val > value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="double"/> is greater than or equal to <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="double"/> member.</param>
    /// <param name="value">The inclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, double>> selector, double value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val >= value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="double"/> is less than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="double"/> member.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan(Expression<Func<T, double>> selector, double value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val < value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="double"/> is less than or equal to <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="double"/> member.</param>
    /// <param name="value">The inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThanOrEqualTo(Expression<Func<T, double>> selector, double value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val <= value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="double"/> is at least <paramref name="minValue"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="double"/> member.</param>
    /// <param name="minValue">The inclusive minimum value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MinValue(Expression<Func<T, double>> selector, double minValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val >= minValue; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="double"/> is at most <paramref name="maxValue"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="double"/> member.</param>
    /// <param name="maxValue">The inclusive maximum value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MaxValue(Expression<Func<T, double>> selector, double maxValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val <= maxValue; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="double"/> falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="selector">Property selector for the <see langword="double"/> member.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder InRange(Expression<Func<T, double>> selector, double min, double max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<double, bool>> p = val => val >= min && val <= max;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see langword="double"/> falls within the range defined by two other properties on the same entity.</summary>
    /// <param name="selector">Property selector for the value to test.</param>
    /// <param name="minSelector">Property selector for the inclusive lower bound.</param>
    /// <param name="maxSelector">Property selector for the inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder InRange(Expression<Func<T, double>> selector,
        Expression<Func<T, double>> minSelector, Expression<Func<T, double>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    // ── decimal ───────────────────────────────────────────────────────────────

    /// <summary>Adds a condition that the selected <see langword="decimal"/> equals zero.</summary>
    /// <param name="selector">Property selector for the <see langword="decimal"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Zero(Expression<Func<T, decimal>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val == 0m; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="decimal"/> is not zero.</summary>
    /// <param name="selector">Property selector for the <see langword="decimal"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder NotZero(Expression<Func<T, decimal>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val != 0m; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="decimal"/> is greater than zero.</summary>
    /// <param name="selector">Property selector for the <see langword="decimal"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Positive(Expression<Func<T, decimal>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val > 0m; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="decimal"/> is less than zero.</summary>
    /// <param name="selector">Property selector for the <see langword="decimal"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Negative(Expression<Func<T, decimal>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val < 0m; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="decimal"/> is greater than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="decimal"/> member.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan(Expression<Func<T, decimal>> selector, decimal value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val > value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="decimal"/> is greater than or equal to <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="decimal"/> member.</param>
    /// <param name="value">The inclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val >= value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="decimal"/> is less than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="decimal"/> member.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan(Expression<Func<T, decimal>> selector, decimal value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val < value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="decimal"/> is less than or equal to <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="decimal"/> member.</param>
    /// <param name="value">The inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val <= value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="decimal"/> is at least <paramref name="minValue"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="decimal"/> member.</param>
    /// <param name="minValue">The inclusive minimum value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MinValue(Expression<Func<T, decimal>> selector, decimal minValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val >= minValue; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="decimal"/> is at most <paramref name="maxValue"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="decimal"/> member.</param>
    /// <param name="maxValue">The inclusive maximum value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MaxValue(Expression<Func<T, decimal>> selector, decimal maxValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val <= maxValue; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="decimal"/> falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="selector">Property selector for the <see langword="decimal"/> member.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder InRange(Expression<Func<T, decimal>> selector, decimal min, decimal max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<decimal, bool>> p = val => val >= min && val <= max;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see langword="decimal"/> falls within the range defined by two other properties on the same entity.</summary>
    /// <param name="selector">Property selector for the value to test.</param>
    /// <param name="minSelector">Property selector for the inclusive lower bound.</param>
    /// <param name="maxSelector">Property selector for the inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder InRange(Expression<Func<T, decimal>> selector,
        Expression<Func<T, decimal>> minSelector, Expression<Func<T, decimal>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    // ── float ─────────────────────────────────────────────────────────────────

    /// <summary>Adds a condition that the selected <see langword="float"/> equals zero.</summary>
    /// <param name="selector">Property selector for the <see langword="float"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Zero(Expression<Func<T, float>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val == 0f; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="float"/> is not zero.</summary>
    /// <param name="selector">Property selector for the <see langword="float"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder NotZero(Expression<Func<T, float>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val != 0f; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="float"/> is greater than zero.</summary>
    /// <param name="selector">Property selector for the <see langword="float"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Positive(Expression<Func<T, float>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val > 0f; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="float"/> is less than zero.</summary>
    /// <param name="selector">Property selector for the <see langword="float"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Negative(Expression<Func<T, float>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val < 0f; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="float"/> is greater than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="float"/> member.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan(Expression<Func<T, float>> selector, float value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val > value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="float"/> is greater than or equal to <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="float"/> member.</param>
    /// <param name="value">The inclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, float>> selector, float value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val >= value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="float"/> is less than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="float"/> member.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan(Expression<Func<T, float>> selector, float value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val < value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="float"/> is less than or equal to <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="float"/> member.</param>
    /// <param name="value">The inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThanOrEqualTo(Expression<Func<T, float>> selector, float value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val <= value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="float"/> is at least <paramref name="minValue"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="float"/> member.</param>
    /// <param name="minValue">The inclusive minimum value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MinValue(Expression<Func<T, float>> selector, float minValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val >= minValue; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="float"/> is at most <paramref name="maxValue"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="float"/> member.</param>
    /// <param name="maxValue">The inclusive maximum value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MaxValue(Expression<Func<T, float>> selector, float maxValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val <= maxValue; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="float"/> falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="selector">Property selector for the <see langword="float"/> member.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder InRange(Expression<Func<T, float>> selector, float min, float max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<float, bool>> p = val => val >= min && val <= max;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see langword="float"/> falls within the range defined by two other properties on the same entity.</summary>
    /// <param name="selector">Property selector for the value to test.</param>
    /// <param name="minSelector">Property selector for the inclusive lower bound.</param>
    /// <param name="maxSelector">Property selector for the inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder InRange(Expression<Func<T, float>> selector,
        Expression<Func<T, float>> minSelector, Expression<Func<T, float>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    // ── short ─────────────────────────────────────────────────────────────────

    /// <summary>Adds a condition that the selected <see langword="short"/> equals zero.</summary>
    /// <param name="selector">Property selector for the <see langword="short"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Zero(Expression<Func<T, short>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val == 0; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="short"/> is not zero.</summary>
    /// <param name="selector">Property selector for the <see langword="short"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder NotZero(Expression<Func<T, short>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val != 0; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="short"/> is greater than zero.</summary>
    /// <param name="selector">Property selector for the <see langword="short"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Positive(Expression<Func<T, short>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val > 0; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="short"/> is less than zero.</summary>
    /// <param name="selector">Property selector for the <see langword="short"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Negative(Expression<Func<T, short>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val < 0; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="short"/> is greater than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="short"/> member.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan(Expression<Func<T, short>> selector, short value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val > value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="short"/> is greater than or equal to <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="short"/> member.</param>
    /// <param name="value">The inclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, short>> selector, short value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val >= value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="short"/> is less than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="short"/> member.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan(Expression<Func<T, short>> selector, short value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val < value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="short"/> is less than or equal to <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="short"/> member.</param>
    /// <param name="value">The inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThanOrEqualTo(Expression<Func<T, short>> selector, short value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val <= value; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="short"/> is at least <paramref name="minValue"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="short"/> member.</param>
    /// <param name="minValue">The inclusive minimum value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MinValue(Expression<Func<T, short>> selector, short minValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val >= minValue; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="short"/> is at most <paramref name="maxValue"/>.</summary>
    /// <param name="selector">Property selector for the <see langword="short"/> member.</param>
    /// <param name="maxValue">The inclusive maximum value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MaxValue(Expression<Func<T, short>> selector, short maxValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val <= maxValue; return _builder.Add(selector, p); }

    /// <summary>Adds a condition that the selected <see langword="short"/> falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="selector">Property selector for the <see langword="short"/> member.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder InRange(Expression<Func<T, short>> selector, short min, short max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<short, bool>> p = val => val >= min && val <= max;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see langword="short"/> falls within the range defined by two other properties on the same entity.</summary>
    /// <param name="selector">Property selector for the value to test.</param>
    /// <param name="minSelector">Property selector for the inclusive lower bound.</param>
    /// <param name="maxSelector">Property selector for the inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder InRange(Expression<Func<T, short>> selector,
        Expression<Func<T, short>> minSelector, Expression<Func<T, short>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    // ── Nullable numeric ──────────────────────────────────────────────────────

    /// <summary>Adds a condition that the selected nullable <see langword="int"/> is <see langword="null"/> or zero.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="int"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsNullOrZero(Expression<Func<T, int?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int?, bool>> p = val => !val.HasValue || val.Value == 0; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="long"/> is <see langword="null"/> or zero.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="long"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsNullOrZero(Expression<Func<T, long?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long?, bool>> p = val => !val.HasValue || val.Value == 0L; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="decimal"/> is <see langword="null"/> or zero.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="decimal"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsNullOrZero(Expression<Func<T, decimal?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal?, bool>> p = val => !val.HasValue || val.Value == 0m; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="double"/> is <see langword="null"/> or zero.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="double"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsNullOrZero(Expression<Func<T, double?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double?, bool>> p = val => !val.HasValue || val.Value == 0.0; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="float"/> is <see langword="null"/> or zero.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="float"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsNullOrZero(Expression<Func<T, float?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float?, bool>> predicate = val => !val.HasValue || val.Value == 0f;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="int"/> has a value (is not <see langword="null"/>).</summary>
    /// <param name="selector">Property selector for the nullable <see langword="int"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder HasValue(Expression<Func<T, int?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int?, bool>> p = val => val.HasValue; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="long"/> has a value (is not <see langword="null"/>).</summary>
    /// <param name="selector">Property selector for the nullable <see langword="long"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder HasValue(Expression<Func<T, long?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long?, bool>> p = val => val.HasValue; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="decimal"/> has a value (is not <see langword="null"/>).</summary>
    /// <param name="selector">Property selector for the nullable <see langword="decimal"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder HasValue(Expression<Func<T, decimal?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal?, bool>> p = val => val.HasValue; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="double"/> has a value (is not <see langword="null"/>).</summary>
    /// <param name="selector">Property selector for the nullable <see langword="double"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder HasValue(Expression<Func<T, double?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double?, bool>> p = val => val.HasValue; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="float"/> has a value (is not <see langword="null"/>).</summary>
    /// <param name="selector">Property selector for the nullable <see langword="float"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder HasValue(Expression<Func<T, float?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float?, bool>> predicate = val => val.HasValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="int"/> has a value and is greater than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="int"/> member.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan(Expression<Func<T, int?>> selector, int value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int?, bool>> p = val => val.HasValue && val.Value > value; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="long"/> has a value and is greater than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="long"/> member.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan(Expression<Func<T, long?>> selector, long value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long?, bool>> p = val => val.HasValue && val.Value > value; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="decimal"/> has a value and is greater than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="decimal"/> member.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan(Expression<Func<T, decimal?>> selector, decimal value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal?, bool>> p = val => val.HasValue && val.Value > value; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="int"/> has a value and is less than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="int"/> member.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan(Expression<Func<T, int?>> selector, int value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int?, bool>> p = val => val.HasValue && val.Value < value; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="long"/> has a value and is less than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="long"/> member.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan(Expression<Func<T, long?>> selector, long value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long?, bool>> p = val => val.HasValue && val.Value < value; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="decimal"/> has a value and is less than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="decimal"/> member.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan(Expression<Func<T, decimal?>> selector, decimal value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal?, bool>> p = val => val.HasValue && val.Value < value; return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="int"/> has a value and falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="selector">Property selector for the nullable <see langword="int"/> member.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder InRange(Expression<Func<T, int?>> selector, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<int?, bool>> p = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="long"/> has a value and falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="selector">Property selector for the nullable <see langword="long"/> member.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder InRange(Expression<Func<T, long?>> selector, long min, long max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<long?, bool>> p = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="decimal"/> has a value and falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="selector">Property selector for the nullable <see langword="decimal"/> member.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder InRange(Expression<Func<T, decimal?>> selector, decimal min, decimal max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<decimal?, bool>> p = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="double"/> has a value and is greater than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="double"/> member.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan(Expression<Func<T, double?>> selector, double value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double?, bool>> predicate = val => val.HasValue && val.Value > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="double"/> has a value and is less than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="double"/> member.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan(Expression<Func<T, double?>> selector, double value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double?, bool>> predicate = val => val.HasValue && val.Value < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="double"/> has a value and falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="selector">Property selector for the nullable <see langword="double"/> member.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder InRange(Expression<Func<T, double?>> selector, double min, double max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<double?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="float"/> has a value and is greater than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="float"/> member.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan(Expression<Func<T, float?>> selector, float value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float?, bool>> predicate = val => val.HasValue && val.Value > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="float"/> has a value and is less than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="float"/> member.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan(Expression<Func<T, float?>> selector, float value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float?, bool>> predicate = val => val.HasValue && val.Value < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="float"/> has a value and falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="selector">Property selector for the nullable <see langword="float"/> member.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder InRange(Expression<Func<T, float?>> selector, float min, float max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<float?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="short"/> is <see langword="null"/> or zero.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="short"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsNullOrZero(Expression<Func<T, short?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short?, bool>> predicate = val => !val.HasValue || val.Value == 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="short"/> has a value (is not <see langword="null"/>).</summary>
    /// <param name="selector">Property selector for the nullable <see langword="short"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder HasValue(Expression<Func<T, short?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short?, bool>> predicate = val => val.HasValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="short"/> has a value and is greater than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="short"/> member.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan(Expression<Func<T, short?>> selector, short value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short?, bool>> predicate = val => val.HasValue && val.Value > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="short"/> has a value and is less than <paramref name="value"/>.</summary>
    /// <param name="selector">Property selector for the nullable <see langword="short"/> member.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan(Expression<Func<T, short?>> selector, short value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short?, bool>> predicate = val => val.HasValue && val.Value < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected nullable <see langword="short"/> has a value and falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="selector">Property selector for the nullable <see langword="short"/> member.</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder InRange(Expression<Func<T, short?>> selector, short min, short max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<short?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, predicate);
    }

    // ── Cross-property range ──────────────────────────────────────────────────

    /// <summary>
    /// Builds a cross-property range condition: <c>selector &gt;= minSelector &amp;&amp; selector &lt;= maxSelector</c>,
    /// where all three selectors reference the same entity parameter.
    /// </summary>
    /// <typeparam name="TNum">The numeric type shared by all three selectors.</typeparam>
    /// <param name="selector">Property selector for the value to test.</param>
    /// <param name="minSelector">Property selector for the inclusive lower bound.</param>
    /// <param name="maxSelector">Property selector for the inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <remarks>
    /// Uses <see cref="ForceCloneVisitor"/> to duplicate the value expression so that the same
    /// expression node is not shared between the left and right side of the <c>AndAlso</c>.
    /// </remarks>
    private TBuilder AddCrossPropertyRange<TNum>(
        Expression<Func<T, TNum>> selector,
        Expression<Func<T, TNum>> minSelector,
        Expression<Func<T, TNum>> maxSelector)
    {
        var param = selector.Parameters[0];
        var val = selector.Body;
        var valClone = new ForceCloneVisitor().Visit(val)!;
        var minBody = new ParameterReplacer(minSelector.Parameters[0], param).Visit(minSelector.Body)!;
        var maxBody = new ParameterReplacer(maxSelector.Parameters[0], param).Visit(maxSelector.Body)!;
        return _builder.Add(Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(
                Expression.GreaterThanOrEqual(val, minBody),
                Expression.LessThanOrEqual(valClone, maxBody)),
            param));
    }
}
