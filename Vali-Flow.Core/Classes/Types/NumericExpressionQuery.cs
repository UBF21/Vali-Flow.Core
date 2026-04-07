using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

public class NumericExpressionQuery<TBuilder, T> : INumericExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public NumericExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    // ── int ───────────────────────────────────────────────────────────────────

    public TBuilder Zero(Expression<Func<T, int>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val == 0; return _builder.Add(selector, p); }

    public TBuilder NotZero(Expression<Func<T, int>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val != 0; return _builder.Add(selector, p); }

    public TBuilder Positive(Expression<Func<T, int>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val > 0; return _builder.Add(selector, p); }

    public TBuilder Negative(Expression<Func<T, int>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val < 0; return _builder.Add(selector, p); }

    public TBuilder GreaterThan(Expression<Func<T, int>> selector, int value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val > value; return _builder.Add(selector, p); }

    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, int>> selector, int value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val >= value; return _builder.Add(selector, p); }

    public TBuilder LessThan(Expression<Func<T, int>> selector, int value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val < value; return _builder.Add(selector, p); }

    public TBuilder LessThanOrEqualTo(Expression<Func<T, int>> selector, int value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val <= value; return _builder.Add(selector, p); }

    public TBuilder MinValue(Expression<Func<T, int>> selector, int minValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val >= minValue; return _builder.Add(selector, p); }

    public TBuilder MaxValue(Expression<Func<T, int>> selector, int maxValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<int, bool>> p = val => val <= maxValue; return _builder.Add(selector, p); }

    public TBuilder InRange(Expression<Func<T, int>> selector, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<int, bool>> p = val => val >= min && val <= max;
        return _builder.Add(selector, p);
    }

    public TBuilder InRange(Expression<Func<T, int>> selector,
        Expression<Func<T, int>> minSelector, Expression<Func<T, int>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    public TBuilder IsEven(Expression<Func<T, int>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val % 2 == 0;
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsOdd(Expression<Func<T, int>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val % 2 != 0;
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsMultipleOf(Expression<Func<T, int>> selector, int divisor)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (divisor == 0) throw new ArgumentOutOfRangeException(nameof(divisor), "divisor must not be zero.");
        Expression<Func<int, bool>> predicate = val => val % divisor == 0;
        return _builder.Add(selector, predicate);
    }

    // ── long ──────────────────────────────────────────────────────────────────

    public TBuilder Zero(Expression<Func<T, long>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val == 0L; return _builder.Add(selector, p); }

    public TBuilder NotZero(Expression<Func<T, long>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val != 0L; return _builder.Add(selector, p); }

    public TBuilder Positive(Expression<Func<T, long>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val > 0L; return _builder.Add(selector, p); }

    public TBuilder Negative(Expression<Func<T, long>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val < 0L; return _builder.Add(selector, p); }

    public TBuilder GreaterThan(Expression<Func<T, long>> selector, long value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val > value; return _builder.Add(selector, p); }

    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, long>> selector, long value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val >= value; return _builder.Add(selector, p); }

    public TBuilder LessThan(Expression<Func<T, long>> selector, long value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val < value; return _builder.Add(selector, p); }

    public TBuilder LessThanOrEqualTo(Expression<Func<T, long>> selector, long value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val <= value; return _builder.Add(selector, p); }

    public TBuilder MinValue(Expression<Func<T, long>> selector, long minValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val >= minValue; return _builder.Add(selector, p); }

    public TBuilder MaxValue(Expression<Func<T, long>> selector, long maxValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<long, bool>> p = val => val <= maxValue; return _builder.Add(selector, p); }

    public TBuilder InRange(Expression<Func<T, long>> selector, long min, long max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<long, bool>> p = val => val >= min && val <= max;
        return _builder.Add(selector, p);
    }

    public TBuilder InRange(Expression<Func<T, long>> selector,
        Expression<Func<T, long>> minSelector, Expression<Func<T, long>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    public TBuilder IsEven(Expression<Func<T, long>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val % 2 == 0;
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsOdd(Expression<Func<T, long>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val % 2 != 0;
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsMultipleOf(Expression<Func<T, long>> selector, long divisor)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (divisor == 0) throw new ArgumentOutOfRangeException(nameof(divisor), "divisor must not be zero.");
        Expression<Func<long, bool>> predicate = val => val % divisor == 0;
        return _builder.Add(selector, predicate);
    }

    // ── double ────────────────────────────────────────────────────────────────

    public TBuilder Zero(Expression<Func<T, double>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val == 0.0; return _builder.Add(selector, p); }

    public TBuilder NotZero(Expression<Func<T, double>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val != 0.0; return _builder.Add(selector, p); }

    public TBuilder Positive(Expression<Func<T, double>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val > 0.0; return _builder.Add(selector, p); }

    public TBuilder Negative(Expression<Func<T, double>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val < 0.0; return _builder.Add(selector, p); }

    public TBuilder GreaterThan(Expression<Func<T, double>> selector, double value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val > value; return _builder.Add(selector, p); }

    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, double>> selector, double value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val >= value; return _builder.Add(selector, p); }

    public TBuilder LessThan(Expression<Func<T, double>> selector, double value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val < value; return _builder.Add(selector, p); }

    public TBuilder LessThanOrEqualTo(Expression<Func<T, double>> selector, double value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val <= value; return _builder.Add(selector, p); }

    public TBuilder MinValue(Expression<Func<T, double>> selector, double minValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val >= minValue; return _builder.Add(selector, p); }

    public TBuilder MaxValue(Expression<Func<T, double>> selector, double maxValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<double, bool>> p = val => val <= maxValue; return _builder.Add(selector, p); }

    public TBuilder InRange(Expression<Func<T, double>> selector, double min, double max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<double, bool>> p = val => val >= min && val <= max;
        return _builder.Add(selector, p);
    }

    public TBuilder InRange(Expression<Func<T, double>> selector,
        Expression<Func<T, double>> minSelector, Expression<Func<T, double>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    // ── decimal ───────────────────────────────────────────────────────────────

    public TBuilder Zero(Expression<Func<T, decimal>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val == 0m; return _builder.Add(selector, p); }

    public TBuilder NotZero(Expression<Func<T, decimal>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val != 0m; return _builder.Add(selector, p); }

    public TBuilder Positive(Expression<Func<T, decimal>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val > 0m; return _builder.Add(selector, p); }

    public TBuilder Negative(Expression<Func<T, decimal>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val < 0m; return _builder.Add(selector, p); }

    public TBuilder GreaterThan(Expression<Func<T, decimal>> selector, decimal value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val > value; return _builder.Add(selector, p); }

    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val >= value; return _builder.Add(selector, p); }

    public TBuilder LessThan(Expression<Func<T, decimal>> selector, decimal value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val < value; return _builder.Add(selector, p); }

    public TBuilder LessThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val <= value; return _builder.Add(selector, p); }

    public TBuilder MinValue(Expression<Func<T, decimal>> selector, decimal minValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val >= minValue; return _builder.Add(selector, p); }

    public TBuilder MaxValue(Expression<Func<T, decimal>> selector, decimal maxValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<decimal, bool>> p = val => val <= maxValue; return _builder.Add(selector, p); }

    public TBuilder InRange(Expression<Func<T, decimal>> selector, decimal min, decimal max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<decimal, bool>> p = val => val >= min && val <= max;
        return _builder.Add(selector, p);
    }

    public TBuilder InRange(Expression<Func<T, decimal>> selector,
        Expression<Func<T, decimal>> minSelector, Expression<Func<T, decimal>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    // ── float ─────────────────────────────────────────────────────────────────

    public TBuilder Zero(Expression<Func<T, float>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val == 0f; return _builder.Add(selector, p); }

    public TBuilder NotZero(Expression<Func<T, float>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val != 0f; return _builder.Add(selector, p); }

    public TBuilder Positive(Expression<Func<T, float>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val > 0f; return _builder.Add(selector, p); }

    public TBuilder Negative(Expression<Func<T, float>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val < 0f; return _builder.Add(selector, p); }

    public TBuilder GreaterThan(Expression<Func<T, float>> selector, float value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val > value; return _builder.Add(selector, p); }

    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, float>> selector, float value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val >= value; return _builder.Add(selector, p); }

    public TBuilder LessThan(Expression<Func<T, float>> selector, float value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val < value; return _builder.Add(selector, p); }

    public TBuilder LessThanOrEqualTo(Expression<Func<T, float>> selector, float value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val <= value; return _builder.Add(selector, p); }

    public TBuilder MinValue(Expression<Func<T, float>> selector, float minValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val >= minValue; return _builder.Add(selector, p); }

    public TBuilder MaxValue(Expression<Func<T, float>> selector, float maxValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<float, bool>> p = val => val <= maxValue; return _builder.Add(selector, p); }

    public TBuilder InRange(Expression<Func<T, float>> selector, float min, float max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<float, bool>> p = val => val >= min && val <= max;
        return _builder.Add(selector, p);
    }

    public TBuilder InRange(Expression<Func<T, float>> selector,
        Expression<Func<T, float>> minSelector, Expression<Func<T, float>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    // ── short ─────────────────────────────────────────────────────────────────

    public TBuilder Zero(Expression<Func<T, short>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val == 0; return _builder.Add(selector, p); }

    public TBuilder NotZero(Expression<Func<T, short>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val != 0; return _builder.Add(selector, p); }

    public TBuilder Positive(Expression<Func<T, short>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val > 0; return _builder.Add(selector, p); }

    public TBuilder Negative(Expression<Func<T, short>> selector)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val < 0; return _builder.Add(selector, p); }

    public TBuilder GreaterThan(Expression<Func<T, short>> selector, short value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val > value; return _builder.Add(selector, p); }

    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, short>> selector, short value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val >= value; return _builder.Add(selector, p); }

    public TBuilder LessThan(Expression<Func<T, short>> selector, short value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val < value; return _builder.Add(selector, p); }

    public TBuilder LessThanOrEqualTo(Expression<Func<T, short>> selector, short value)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val <= value; return _builder.Add(selector, p); }

    public TBuilder MinValue(Expression<Func<T, short>> selector, short minValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val >= minValue; return _builder.Add(selector, p); }

    public TBuilder MaxValue(Expression<Func<T, short>> selector, short maxValue)
    { ArgumentNullException.ThrowIfNull(selector); Expression<Func<short, bool>> p = val => val <= maxValue; return _builder.Add(selector, p); }

    public TBuilder InRange(Expression<Func<T, short>> selector, short min, short max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<short, bool>> p = val => val >= min && val <= max;
        return _builder.Add(selector, p);
    }

    public TBuilder InRange(Expression<Func<T, short>> selector,
        Expression<Func<T, short>> minSelector, Expression<Func<T, short>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    // ── Nullable numeric ──────────────────────────────────────────────────────

    public TBuilder IsNullOrZero(Expression<Func<T, int?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int?, bool>> p = val => !val.HasValue || val.Value == 0; return _builder.Add(selector, p);
    }

    public TBuilder IsNullOrZero(Expression<Func<T, long?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long?, bool>> p = val => !val.HasValue || val.Value == 0L; return _builder.Add(selector, p);
    }

    public TBuilder IsNullOrZero(Expression<Func<T, decimal?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal?, bool>> p = val => !val.HasValue || val.Value == 0m; return _builder.Add(selector, p);
    }

    public TBuilder IsNullOrZero(Expression<Func<T, double?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double?, bool>> p = val => !val.HasValue || val.Value == 0.0; return _builder.Add(selector, p);
    }

    public TBuilder IsNullOrZero(Expression<Func<T, float?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float?, bool>> predicate = val => !val.HasValue || val.Value == 0f;
        return _builder.Add(selector, predicate);
    }

    public TBuilder HasValue(Expression<Func<T, int?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int?, bool>> p = val => val.HasValue; return _builder.Add(selector, p);
    }

    public TBuilder HasValue(Expression<Func<T, long?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long?, bool>> p = val => val.HasValue; return _builder.Add(selector, p);
    }

    public TBuilder HasValue(Expression<Func<T, decimal?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal?, bool>> p = val => val.HasValue; return _builder.Add(selector, p);
    }

    public TBuilder HasValue(Expression<Func<T, double?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double?, bool>> p = val => val.HasValue; return _builder.Add(selector, p);
    }

    public TBuilder HasValue(Expression<Func<T, float?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float?, bool>> predicate = val => val.HasValue;
        return _builder.Add(selector, predicate);
    }

    public TBuilder GreaterThan(Expression<Func<T, int?>> selector, int value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int?, bool>> p = val => val.HasValue && val.Value > value; return _builder.Add(selector, p);
    }

    public TBuilder GreaterThan(Expression<Func<T, long?>> selector, long value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long?, bool>> p = val => val.HasValue && val.Value > value; return _builder.Add(selector, p);
    }

    public TBuilder GreaterThan(Expression<Func<T, decimal?>> selector, decimal value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal?, bool>> p = val => val.HasValue && val.Value > value; return _builder.Add(selector, p);
    }

    public TBuilder LessThan(Expression<Func<T, int?>> selector, int value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int?, bool>> p = val => val.HasValue && val.Value < value; return _builder.Add(selector, p);
    }

    public TBuilder LessThan(Expression<Func<T, long?>> selector, long value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long?, bool>> p = val => val.HasValue && val.Value < value; return _builder.Add(selector, p);
    }

    public TBuilder LessThan(Expression<Func<T, decimal?>> selector, decimal value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal?, bool>> p = val => val.HasValue && val.Value < value; return _builder.Add(selector, p);
    }

    public TBuilder InRange(Expression<Func<T, int?>> selector, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<int?, bool>> p = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, p);
    }

    public TBuilder InRange(Expression<Func<T, long?>> selector, long min, long max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<long?, bool>> p = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, p);
    }

    public TBuilder InRange(Expression<Func<T, decimal?>> selector, decimal min, decimal max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<decimal?, bool>> p = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, p);
    }

    public TBuilder GreaterThan(Expression<Func<T, double?>> selector, double value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double?, bool>> predicate = val => val.HasValue && val.Value > value;
        return _builder.Add(selector, predicate);
    }

    public TBuilder LessThan(Expression<Func<T, double?>> selector, double value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double?, bool>> predicate = val => val.HasValue && val.Value < value;
        return _builder.Add(selector, predicate);
    }

    public TBuilder InRange(Expression<Func<T, double?>> selector, double min, double max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<double?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, predicate);
    }

    public TBuilder GreaterThan(Expression<Func<T, float?>> selector, float value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float?, bool>> predicate = val => val.HasValue && val.Value > value;
        return _builder.Add(selector, predicate);
    }

    public TBuilder LessThan(Expression<Func<T, float?>> selector, float value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float?, bool>> predicate = val => val.HasValue && val.Value < value;
        return _builder.Add(selector, predicate);
    }

    public TBuilder InRange(Expression<Func<T, float?>> selector, float min, float max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<float?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsNullOrZero(Expression<Func<T, short?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short?, bool>> predicate = val => !val.HasValue || val.Value == 0;
        return _builder.Add(selector, predicate);
    }

    public TBuilder HasValue(Expression<Func<T, short?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short?, bool>> predicate = val => val.HasValue;
        return _builder.Add(selector, predicate);
    }

    public TBuilder GreaterThan(Expression<Func<T, short?>> selector, short value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short?, bool>> predicate = val => val.HasValue && val.Value > value;
        return _builder.Add(selector, predicate);
    }

    public TBuilder LessThan(Expression<Func<T, short?>> selector, short value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short?, bool>> predicate = val => val.HasValue && val.Value < value;
        return _builder.Add(selector, predicate);
    }

    public TBuilder InRange(Expression<Func<T, short?>> selector, short min, short max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<short?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, predicate);
    }

    // ── Cross-property range ──────────────────────────────────────────────────

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
