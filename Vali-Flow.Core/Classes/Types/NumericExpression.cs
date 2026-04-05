using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

public class NumericExpression<TBuilder, T> : INumericExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, INumericExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public NumericExpression(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Validates that the selected value is zero.</summary>
    public TBuilder Zero(Expression<Func<T, int>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val == 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is zero.</summary>
    public TBuilder Zero(Expression<Func<T, long>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val == 0L;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is zero.</summary>
    public TBuilder Zero(Expression<Func<T, float>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float, bool>> predicate = val => val == 0f;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is zero.</summary>
    public TBuilder Zero(Expression<Func<T, double>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double, bool>> predicate = val => val == 0.0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is zero.</summary>
    public TBuilder Zero(Expression<Func<T, decimal>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal, bool>> predicate = val => val == 0m;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is zero.</summary>
    public TBuilder Zero(Expression<Func<T, short>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short, bool>> predicate = val => val == 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is not zero.</summary>
    public TBuilder NotZero(Expression<Func<T, int>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val != 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is not zero.</summary>
    public TBuilder NotZero(Expression<Func<T, long>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val != 0L;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is not zero.</summary>
    public TBuilder NotZero(Expression<Func<T, float>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float, bool>> predicate = val => val != 0f;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is not zero.</summary>
    public TBuilder NotZero(Expression<Func<T, double>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double, bool>> predicate = val => val != 0.0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is not zero.</summary>
    public TBuilder NotZero(Expression<Func<T, decimal>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal, bool>> predicate = val => val != 0m;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is not zero.</summary>
    public TBuilder NotZero(Expression<Func<T, short>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short, bool>> predicate = val => val != 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, int>> selector, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min)
            throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min.");
        Expression<Func<int, bool>> predicate = val => val >= min && val <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, long>> selector, long min, long max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min)
            throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min.");
        Expression<Func<long, bool>> predicate = val => val >= min && val <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, float>> selector, float min, float max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min)
            throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min.");
        Expression<Func<float, bool>> predicate = val => val >= min && val <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, double>> selector, double min, double max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min)
            throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min.");
        Expression<Func<double, bool>> predicate = val => val >= min && val <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, decimal>> selector, decimal min, decimal max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min)
            throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min.");
        Expression<Func<decimal, bool>> predicate = val => val >= min && val <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, short>> selector, short min, short max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min)
            throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min.");
        Expression<Func<short, bool>> predicate = val => val >= min && val <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value falls within the range returned by <paramref name="minSelector"/> and <paramref name="maxSelector"/> (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, int>> selector, Expression<Func<T, int>> minSelector,
        Expression<Func<T, int>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    /// <summary>Validates that the selected value falls within the range returned by <paramref name="minSelector"/> and <paramref name="maxSelector"/> (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, long>> selector, Expression<Func<T, long>> minSelector,
        Expression<Func<T, long>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    /// <summary>Validates that the selected value falls within the range returned by <paramref name="minSelector"/> and <paramref name="maxSelector"/> (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, float>> selector, Expression<Func<T, float>> minSelector,
        Expression<Func<T, float>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    /// <summary>Validates that the selected value falls within the range returned by <paramref name="minSelector"/> and <paramref name="maxSelector"/> (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, double>> selector, Expression<Func<T, double>> minSelector,
        Expression<Func<T, double>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    /// <summary>Validates that the selected value falls within the range returned by <paramref name="minSelector"/> and <paramref name="maxSelector"/> (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, decimal>> selector, Expression<Func<T, decimal>> minSelector,
        Expression<Func<T, decimal>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    /// <summary>Validates that the selected value falls within the range returned by <paramref name="minSelector"/> and <paramref name="maxSelector"/> (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, short>> selector, Expression<Func<T, short>> minSelector,
        Expression<Func<T, short>> maxSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    /// <summary>Validates that the selected value is greater than <paramref name="value"/>.</summary>
    public TBuilder GreaterThan(Expression<Func<T, int>> selector, int value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than <paramref name="value"/>.</summary>
    public TBuilder GreaterThan(Expression<Func<T, long>> selector, long value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than <paramref name="value"/>.</summary>
    public TBuilder GreaterThan(Expression<Func<T, float>> selector, float value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float, bool>> predicate = val => val > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than <paramref name="value"/>.</summary>
    public TBuilder GreaterThan(Expression<Func<T, double>> selector, double value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double, bool>> predicate = val => val > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than <paramref name="value"/>.</summary>
    public TBuilder GreaterThan(Expression<Func<T, decimal>> selector, decimal value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal, bool>> predicate = val => val > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than <paramref name="value"/>.</summary>
    public TBuilder GreaterThan(Expression<Func<T, short>> selector, short value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short, bool>> predicate = val => val > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than or equal to <paramref name="value"/>.</summary>
    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, int>> selector, int value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val >= value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than or equal to <paramref name="value"/>.</summary>
    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, long>> selector, long value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val >= value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than or equal to <paramref name="value"/>.</summary>
    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, float>> selector, float value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float, bool>> predicate = val => val >= value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than or equal to <paramref name="value"/>.</summary>
    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, double>> selector, double value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double, bool>> predicate = val => val >= value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than or equal to <paramref name="value"/>.</summary>
    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal, bool>> predicate = val => val >= value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than or equal to <paramref name="value"/>.</summary>
    public TBuilder GreaterThanOrEqualTo(Expression<Func<T, short>> selector, short value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short, bool>> predicate = val => val >= value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than <paramref name="value"/>.</summary>
    public TBuilder LessThan(Expression<Func<T, int>> selector, int value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than <paramref name="value"/>.</summary>
    public TBuilder LessThan(Expression<Func<T, long>> selector, long value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than <paramref name="value"/>.</summary>
    public TBuilder LessThan(Expression<Func<T, float>> selector, float value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float, bool>> predicate = val => val < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than <paramref name="value"/>.</summary>
    public TBuilder LessThan(Expression<Func<T, double>> selector, double value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double, bool>> predicate = val => val < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than <paramref name="value"/>.</summary>
    public TBuilder LessThan(Expression<Func<T, decimal>> selector, decimal value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal, bool>> predicate = val => val < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than <paramref name="value"/>.</summary>
    public TBuilder LessThan(Expression<Func<T, short>> selector, short value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short, bool>> predicate = val => val < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than or equal to <paramref name="value"/>.</summary>
    public TBuilder LessThanOrEqualTo(Expression<Func<T, int>> selector, int value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val <= value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than or equal to <paramref name="value"/>.</summary>
    public TBuilder LessThanOrEqualTo(Expression<Func<T, long>> selector, long value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val <= value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than or equal to <paramref name="value"/>.</summary>
    public TBuilder LessThanOrEqualTo(Expression<Func<T, float>> selector, float value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float, bool>> predicate = val => val <= value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than or equal to <paramref name="value"/>.</summary>
    public TBuilder LessThanOrEqualTo(Expression<Func<T, double>> selector, double value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double, bool>> predicate = val => val <= value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than or equal to <paramref name="value"/>.</summary>
    public TBuilder LessThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal, bool>> predicate = val => val <= value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than or equal to <paramref name="value"/>.</summary>
    public TBuilder LessThanOrEqualTo(Expression<Func<T, short>> selector, short value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short, bool>> predicate = val => val <= value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is positive (> 0).</summary>
    public TBuilder Positive(Expression<Func<T, int>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val > 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is positive (> 0).</summary>
    public TBuilder Positive(Expression<Func<T, long>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val > 0L;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is positive (> 0).</summary>
    public TBuilder Positive(Expression<Func<T, float>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float, bool>> predicate = val => val > 0f;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is positive (> 0).</summary>
    public TBuilder Positive(Expression<Func<T, double>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double, bool>> predicate = val => val > 0.0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is positive (> 0).</summary>
    public TBuilder Positive(Expression<Func<T, decimal>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal, bool>> predicate = val => val > 0m;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is positive (> 0).</summary>
    public TBuilder Positive(Expression<Func<T, short>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short, bool>> predicate = val => val > 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is negative (< 0).</summary>
    public TBuilder Negative(Expression<Func<T, int>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val < 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is negative (< 0).</summary>
    public TBuilder Negative(Expression<Func<T, long>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val < 0L;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is negative (< 0).</summary>
    public TBuilder Negative(Expression<Func<T, float>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float, bool>> predicate = val => val < 0f;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is negative (< 0).</summary>
    public TBuilder Negative(Expression<Func<T, double>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double, bool>> predicate = val => val < 0.0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is negative (< 0).</summary>
    public TBuilder Negative(Expression<Func<T, decimal>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal, bool>> predicate = val => val < 0m;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is negative (< 0).</summary>
    public TBuilder Negative(Expression<Func<T, short>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short, bool>> predicate = val => val < 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than or equal to <paramref name="minValue"/>.</summary>
    public TBuilder MinValue(Expression<Func<T, int>> selector, int minValue)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val >= minValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than or equal to <paramref name="minValue"/>.</summary>
    public TBuilder MinValue(Expression<Func<T, long>> selector, long minValue)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val >= minValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than or equal to <paramref name="minValue"/>.</summary>
    public TBuilder MinValue(Expression<Func<T, float>> selector, float minValue)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float, bool>> predicate = val => val >= minValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than or equal to <paramref name="minValue"/>.</summary>
    public TBuilder MinValue(Expression<Func<T, double>> selector, double minValue)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double, bool>> predicate = val => val >= minValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than or equal to <paramref name="minValue"/>.</summary>
    public TBuilder MinValue(Expression<Func<T, decimal>> selector, decimal minValue)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal, bool>> predicate = val => val >= minValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than or equal to <paramref name="minValue"/>.</summary>
    public TBuilder MinValue(Expression<Func<T, short>> selector, short minValue)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short, bool>> predicate = val => val >= minValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than or equal to <paramref name="maxValue"/>.</summary>
    public TBuilder MaxValue(Expression<Func<T, int>> selector, int maxValue)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val <= maxValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than or equal to <paramref name="maxValue"/>.</summary>
    public TBuilder MaxValue(Expression<Func<T, long>> selector, long maxValue)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val <= maxValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than or equal to <paramref name="maxValue"/>.</summary>
    public TBuilder MaxValue(Expression<Func<T, float>> selector, float maxValue)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float, bool>> predicate = val => val <= maxValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than or equal to <paramref name="maxValue"/>.</summary>
    public TBuilder MaxValue(Expression<Func<T, double>> selector, double maxValue)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double, bool>> predicate = val => val <= maxValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than or equal to <paramref name="maxValue"/>.</summary>
    public TBuilder MaxValue(Expression<Func<T, decimal>> selector, decimal maxValue)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal, bool>> predicate = val => val <= maxValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than or equal to <paramref name="maxValue"/>.</summary>
    public TBuilder MaxValue(Expression<Func<T, short>> selector, short maxValue)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short, bool>> predicate = val => val <= maxValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected integer value is even.</summary>
    /// <remarks>
    /// <b>EF Core:</b> Uses the <c>%</c> (modulo) operator, which is not translatable to SQL by all EF Core providers.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsEven(Expression<Func<T, int>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val % 2 == 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected long value is even.</summary>
    /// <remarks>
    /// <b>EF Core:</b> Uses the <c>%</c> (modulo) operator, which is not translatable to SQL by all EF Core providers.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsEven(Expression<Func<T, long>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val % 2 == 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected integer value is odd.</summary>
    /// <remarks>
    /// <b>EF Core:</b> Uses the <c>%</c> (modulo) operator, which is not translatable to SQL by all EF Core providers.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsOdd(Expression<Func<T, int>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int, bool>> predicate = val => val % 2 != 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected long value is odd.</summary>
    /// <remarks>
    /// <b>EF Core:</b> Uses the <c>%</c> (modulo) operator, which is not translatable to SQL by all EF Core providers.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsOdd(Expression<Func<T, long>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long, bool>> predicate = val => val % 2 != 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected integer is a multiple of <paramref name="divisor"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> Uses the <c>%</c> (modulo) operator, which is not translatable to SQL by all EF Core providers.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsMultipleOf(Expression<Func<T, int>> selector, int divisor)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (divisor == 0) throw new ArgumentException("Divisor cannot be zero.", nameof(divisor));
        Expression<Func<int, bool>> predicate = val => val % divisor == 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected long is a multiple of <paramref name="divisor"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> Uses the <c>%</c> (modulo) operator, which is not translatable to SQL by all EF Core providers.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsMultipleOf(Expression<Func<T, long>> selector, long divisor)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (divisor == 0) throw new ArgumentException("Divisor cannot be zero.", nameof(divisor));
        Expression<Func<long, bool>> predicate = val => val % divisor == 0;
        return _builder.Add(selector, predicate);
    }

    // ── IsBetweenExclusive ─────────────────────────────────────────────────────

    /// <summary>Validates that the selected integer is strictly between <paramref name="min"/> and <paramref name="max"/> (both exclusive).</summary>
    public TBuilder IsBetweenExclusive(Expression<Func<T, int>> selector, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max <= min) throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than min.");
        Expression<Func<int, bool>> predicate = val => val > min && val < max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected long is strictly between <paramref name="min"/> and <paramref name="max"/> (both exclusive).</summary>
    public TBuilder IsBetweenExclusive(Expression<Func<T, long>> selector, long min, long max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max <= min) throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than min.");
        Expression<Func<long, bool>> predicate = val => val > min && val < max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected float is strictly between <paramref name="min"/> and <paramref name="max"/> (both exclusive).</summary>
    public TBuilder IsBetweenExclusive(Expression<Func<T, float>> selector, float min, float max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max <= min) throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than min.");
        Expression<Func<float, bool>> predicate = val => val > min && val < max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected double is strictly between <paramref name="min"/> and <paramref name="max"/> (both exclusive).</summary>
    public TBuilder IsBetweenExclusive(Expression<Func<T, double>> selector, double min, double max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max <= min) throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than min.");
        Expression<Func<double, bool>> predicate = val => val > min && val < max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected decimal is strictly between <paramref name="min"/> and <paramref name="max"/> (both exclusive).</summary>
    public TBuilder IsBetweenExclusive(Expression<Func<T, decimal>> selector, decimal min, decimal max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max <= min) throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than min.");
        Expression<Func<decimal, bool>> predicate = val => val > min && val < max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected short is strictly between <paramref name="min"/> and <paramref name="max"/> (both exclusive).</summary>
    public TBuilder IsBetweenExclusive(Expression<Func<T, short>> selector, short min, short max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max <= min) throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than min.");
        Expression<Func<short, bool>> predicate = val => val > min && val < max;
        return _builder.Add(selector, predicate);
    }

    // ── IsCloseTo ──────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected double value is within <paramref name="tolerance"/> of <paramref name="value"/> (i.e. |val - value| ≤ tolerance).</summary>
    public TBuilder IsCloseTo(Expression<Func<T, double>> selector, double value, double tolerance)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (tolerance < 0) throw new ArgumentOutOfRangeException(nameof(tolerance), "tolerance must be non-negative.");
        Expression<Func<double, bool>> predicate = val => Math.Abs(val - value) <= tolerance;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected float value is within <paramref name="tolerance"/> of <paramref name="value"/> (i.e. |val - value| ≤ tolerance).</summary>
    public TBuilder IsCloseTo(Expression<Func<T, float>> selector, float value, float tolerance)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (tolerance < 0) throw new ArgumentOutOfRangeException(nameof(tolerance), "tolerance must be non-negative.");
        Expression<Func<float, bool>> predicate = val => Math.Abs(val - value) <= tolerance;
        return _builder.Add(selector, predicate);
    }

    // ── IComparable<TValue> generic overloads ──────────────────────────────────

    /// <summary>Validates that the selected value is greater than <paramref name="value"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> This overload uses <c>CompareTo()</c> which is not translatable to SQL.
    /// For EF Core queries, use the typed overloads that accept concrete value types (int, decimal, DateTime, etc.)
    /// which produce SQL-compatible comparison operators.
    /// </remarks>
    public TBuilder GreaterThan<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(value) > 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is greater than or equal to <paramref name="value"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> This overload uses <c>CompareTo()</c> which is not translatable to SQL.
    /// For EF Core queries, use the typed overloads that accept concrete value types (int, decimal, DateTime, etc.)
    /// which produce SQL-compatible comparison operators.
    /// </remarks>
    public TBuilder GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(value) >= 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than <paramref name="value"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> This overload uses <c>CompareTo()</c> which is not translatable to SQL.
    /// For EF Core queries, use the typed overloads that accept concrete value types (int, decimal, DateTime, etc.)
    /// which produce SQL-compatible comparison operators.
    /// </remarks>
    public TBuilder LessThan<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(value) < 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is less than or equal to <paramref name="value"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> This overload uses <c>CompareTo()</c> which is not translatable to SQL.
    /// For EF Core queries, use the typed overloads that accept concrete value types (int, decimal, DateTime, etc.)
    /// which produce SQL-compatible comparison operators.
    /// </remarks>
    public TBuilder LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(value) <= 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <remarks>
    /// <b>EF Core:</b> This overload uses <c>CompareTo()</c> which is not translatable to SQL.
    /// For EF Core queries, use the typed overloads that accept concrete value types (int, decimal, DateTime, etc.)
    /// which produce SQL-compatible comparison operators.
    /// </remarks>
    public TBuilder InRange<TValue>(Expression<Func<T, TValue>> selector, TValue min, TValue max)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(min);
        ArgumentNullException.ThrowIfNull(max);
        if (min.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(min) >= 0 && val.CompareTo(max) <= 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is equal to <paramref name="value"/>.</summary>
    /// <remarks>
    /// Uses <c>Expression.Equal</c> internally, which is EF Core SQL-translatable for all scalar types.
    /// </remarks>
    public TBuilder EqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (value == null) throw new ArgumentNullException(nameof(value), "Use Null() to check for null values.");
        var param = Expression.Parameter(typeof(TValue), "v");
        var body = Expression.Equal(param, Expression.Constant(value, typeof(TValue)));
        Expression<Func<TValue, bool>> predicate = Expression.Lambda<Func<TValue, bool>>(body, param);
        return _builder.Add(selector, predicate);
    }

    // ── Cross-property comparisons ─────────────────────────────────────────────

    /// <summary>Validates that the selected value is greater than the value selected by <paramref name="otherSelector"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> This overload uses <c>IComparable&lt;T&gt;.CompareTo()</c>, which is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder GreaterThan<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(otherSelector);
        var param = selector.Parameters[0];
        var otherBody = new ParameterReplacer(otherSelector.Parameters[0], param).Visit(otherSelector.Body)!;
        var compareToMethod = typeof(IComparable<TValue>).GetMethod(nameof(IComparable<TValue>.CompareTo))!;
        var selectorBodyForCall = new ForceCloneVisitor().Visit(selector.Body)!;
        var callExpr = Expression.Call(selectorBodyForCall, compareToMethod, otherBody);
        Expression body;
        if (typeof(TValue).IsValueType)
        {
            body = Expression.GreaterThan(callExpr, Expression.Constant(0));
        }
        else
        {
            Expression freshBody = new ForceCloneVisitor().Visit(selector.Body)!;
            var nullCheck = Expression.NotEqual(freshBody, Expression.Constant(null, typeof(TValue)));
            body = Expression.AndAlso(nullCheck, Expression.GreaterThan(callExpr, Expression.Constant(0)));
        }
        return _builder.Add(Expression.Lambda<Func<T, bool>>(body, param));
    }

    /// <summary>Validates that the selected value is greater than or equal to the value selected by <paramref name="otherSelector"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> This overload uses <c>IComparable&lt;T&gt;.CompareTo()</c>, which is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(otherSelector);
        var param = selector.Parameters[0];
        var otherBody = new ParameterReplacer(otherSelector.Parameters[0], param).Visit(otherSelector.Body)!;
        var compareToMethod = typeof(IComparable<TValue>).GetMethod(nameof(IComparable<TValue>.CompareTo))!;
        var selectorBodyForCall = new ForceCloneVisitor().Visit(selector.Body)!;
        var callExpr = Expression.Call(selectorBodyForCall, compareToMethod, otherBody);
        Expression body;
        if (typeof(TValue).IsValueType)
        {
            body = Expression.GreaterThanOrEqual(callExpr, Expression.Constant(0));
        }
        else
        {
            Expression freshBody = new ForceCloneVisitor().Visit(selector.Body)!;
            var nullCheck = Expression.NotEqual(freshBody, Expression.Constant(null, typeof(TValue)));
            body = Expression.AndAlso(nullCheck, Expression.GreaterThanOrEqual(callExpr, Expression.Constant(0)));
        }
        return _builder.Add(Expression.Lambda<Func<T, bool>>(body, param));
    }

    /// <summary>Validates that the selected value is less than the value selected by <paramref name="otherSelector"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> This overload uses <c>IComparable&lt;T&gt;.CompareTo()</c>, which is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder LessThan<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(otherSelector);
        var param = selector.Parameters[0];
        var otherBody = new ParameterReplacer(otherSelector.Parameters[0], param).Visit(otherSelector.Body)!;
        var compareToMethod = typeof(IComparable<TValue>).GetMethod(nameof(IComparable<TValue>.CompareTo))!;
        var selectorBodyForCall = new ForceCloneVisitor().Visit(selector.Body)!;
        var callExpr = Expression.Call(selectorBodyForCall, compareToMethod, otherBody);
        Expression body;
        if (typeof(TValue).IsValueType)
        {
            body = Expression.LessThan(callExpr, Expression.Constant(0));
        }
        else
        {
            Expression freshBody = new ForceCloneVisitor().Visit(selector.Body)!;
            var nullCheck = Expression.NotEqual(freshBody, Expression.Constant(null, typeof(TValue)));
            body = Expression.AndAlso(nullCheck, Expression.LessThan(callExpr, Expression.Constant(0)));
        }
        return _builder.Add(Expression.Lambda<Func<T, bool>>(body, param));
    }

    /// <summary>Validates that the selected value is less than or equal to the value selected by <paramref name="otherSelector"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> This overload uses <c>IComparable&lt;T&gt;.CompareTo()</c>, which is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(otherSelector);
        var param = selector.Parameters[0];
        var otherBody = new ParameterReplacer(otherSelector.Parameters[0], param).Visit(otherSelector.Body)!;
        var compareToMethod = typeof(IComparable<TValue>).GetMethod(nameof(IComparable<TValue>.CompareTo))!;
        var selectorBodyForCall = new ForceCloneVisitor().Visit(selector.Body)!;
        var callExpr = Expression.Call(selectorBodyForCall, compareToMethod, otherBody);
        Expression body;
        if (typeof(TValue).IsValueType)
        {
            body = Expression.LessThanOrEqual(callExpr, Expression.Constant(0));
        }
        else
        {
            Expression freshBody = new ForceCloneVisitor().Visit(selector.Body)!;
            var nullCheck = Expression.NotEqual(freshBody, Expression.Constant(null, typeof(TValue)));
            body = Expression.AndAlso(nullCheck, Expression.LessThanOrEqual(callExpr, Expression.Constant(0)));
        }
        return _builder.Add(Expression.Lambda<Func<T, bool>>(body, param));
    }

    // ── Nullable numeric ───────────────────────────────────────────────────────

    /// <summary>Validates that the selected nullable value is null or equal to zero.</summary>
    public TBuilder IsNullOrZero(Expression<Func<T, int?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int?, bool>> predicate = val => !val.HasValue || val.Value == 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value is null or equal to zero.</summary>
    public TBuilder IsNullOrZero(Expression<Func<T, decimal?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal?, bool>> predicate = val => !val.HasValue || val.Value == 0m;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value is null or equal to zero.</summary>
    public TBuilder IsNullOrZero(Expression<Func<T, long?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long?, bool>> predicate = val => !val.HasValue || val.Value == 0L;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value is null or equal to zero.</summary>
    public TBuilder IsNullOrZero(Expression<Func<T, double?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double?, bool>> predicate = val => !val.HasValue || val.Value == 0.0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable <see cref="float"/> is null or equal to zero.</summary>
    public TBuilder IsNullOrZero(Expression<Func<T, float?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float?, bool>> predicate = val => !val.HasValue || val.Value == 0f;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a non-null value.</summary>
    public TBuilder HasValue(Expression<Func<T, int?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int?, bool>> predicate = val => val.HasValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a non-null value.</summary>
    public TBuilder HasValue(Expression<Func<T, decimal?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal?, bool>> predicate = val => val.HasValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a non-null value.</summary>
    public TBuilder HasValue(Expression<Func<T, long?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long?, bool>> predicate = val => val.HasValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a non-null value.</summary>
    public TBuilder HasValue(Expression<Func<T, double?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double?, bool>> predicate = val => val.HasValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a non-null value.</summary>
    public TBuilder HasValue(Expression<Func<T, float?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float?, bool>> predicate = val => val.HasValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and is greater than <paramref name="value"/>.</summary>
    public TBuilder GreaterThan(Expression<Func<T, int?>> selector, int value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int?, bool>> predicate = val => val.HasValue && val.Value > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and is greater than <paramref name="value"/>.</summary>
    public TBuilder GreaterThan(Expression<Func<T, decimal?>> selector, decimal value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal?, bool>> predicate = val => val.HasValue && val.Value > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and is greater than <paramref name="value"/>.</summary>
    public TBuilder GreaterThan(Expression<Func<T, long?>> selector, long value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long?, bool>> predicate = val => val.HasValue && val.Value > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and is greater than <paramref name="value"/>.</summary>
    public TBuilder GreaterThan(Expression<Func<T, double?>> selector, double value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double?, bool>> predicate = val => val.HasValue && val.Value > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and is greater than <paramref name="value"/>.</summary>
    public TBuilder GreaterThan(Expression<Func<T, float?>> selector, float value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float?, bool>> predicate = val => val.HasValue && val.Value > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and is greater than <paramref name="value"/>.</summary>
    public TBuilder GreaterThan(Expression<Func<T, short?>> selector, short value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short?, bool>> predicate = val => val.HasValue && val.Value > value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and is less than <paramref name="value"/>.</summary>
    public TBuilder LessThan(Expression<Func<T, int?>> selector, int value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<int?, bool>> predicate = val => val.HasValue && val.Value < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and is less than <paramref name="value"/>.</summary>
    public TBuilder LessThan(Expression<Func<T, decimal?>> selector, decimal value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<decimal?, bool>> predicate = val => val.HasValue && val.Value < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and is less than <paramref name="value"/>.</summary>
    public TBuilder LessThan(Expression<Func<T, long?>> selector, long value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<long?, bool>> predicate = val => val.HasValue && val.Value < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and is less than <paramref name="value"/>.</summary>
    public TBuilder LessThan(Expression<Func<T, double?>> selector, double value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<double?, bool>> predicate = val => val.HasValue && val.Value < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and is less than <paramref name="value"/>.</summary>
    public TBuilder LessThan(Expression<Func<T, float?>> selector, float value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<float?, bool>> predicate = val => val.HasValue && val.Value < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and is less than <paramref name="value"/>.</summary>
    public TBuilder LessThan(Expression<Func<T, short?>> selector, short value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short?, bool>> predicate = val => val.HasValue && val.Value < value;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value is null or equal to zero.</summary>
    public TBuilder IsNullOrZero(Expression<Func<T, short?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short?, bool>> predicate = val => !val.HasValue || val.Value == 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a non-null value.</summary>
    public TBuilder HasValue(Expression<Func<T, short?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<short?, bool>> predicate = val => val.HasValue;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, int?>> selector, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<int?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, decimal?>> selector, decimal min, decimal max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<decimal?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, long?>> selector, long min, long max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<long?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, double?>> selector, double min, double max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<double?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    public TBuilder InRange(Expression<Func<T, float?>> selector, float min, float max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<float?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable value has a value and falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
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