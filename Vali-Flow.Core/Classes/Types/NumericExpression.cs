using System.Linq.Expressions;
using System.Numerics;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

public class NumericExpression<TBuilder, T> : INumericExpression<TBuilder, T>, IComparableExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, INumericExpression<TBuilder, T>, new()
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, System.Reflection.MethodInfo> _absMethodCache = new();
    private readonly BaseExpression<TBuilder, T> _builder;

    public NumericExpression(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    // ── Sign checks ────────────────────────────────────────────────────────────

    public TBuilder Zero<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildComparisonToZero<TValue>(ExpressionType.Equal));
    }

    public TBuilder NotZero<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildComparisonToZero<TValue>(ExpressionType.NotEqual));
    }

    public TBuilder Positive<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildComparisonToZero<TValue>(ExpressionType.GreaterThan));
    }

    public TBuilder Negative<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildComparisonToZero<TValue>(ExpressionType.LessThan));
    }

    private static Expression<Func<TValue, bool>> BuildComparisonToZero<TValue>(ExpressionType type)
        where TValue : INumber<TValue>
    {
        var p = Expression.Parameter(typeof(TValue), "val");
        var zero = Expression.Constant(TValue.Zero, typeof(TValue));
        return Expression.Lambda<Func<TValue, bool>>(Expression.MakeBinary(type, p, zero), p);
    }

    // ── Scalar comparisons (INumber<TValue>) ───────────────────────────────────

    private static Expression<Func<TValue, bool>> BuildScalarComparison<TValue>(TValue value, ExpressionType type)
        where TValue : INumber<TValue>
    {
        var p = Expression.Parameter(typeof(TValue), "val");
        return Expression.Lambda<Func<TValue, bool>>(
            Expression.MakeBinary(type, p, Expression.Constant(value, typeof(TValue))), p);
    }

    public TBuilder GreaterThan<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildScalarComparison(value, ExpressionType.GreaterThan));
    }

    public TBuilder GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildScalarComparison(value, ExpressionType.GreaterThanOrEqual));
    }

    public TBuilder LessThan<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildScalarComparison(value, ExpressionType.LessThan));
    }

    public TBuilder LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildScalarComparison(value, ExpressionType.LessThanOrEqual));
    }

    public TBuilder MinValue<TValue>(Expression<Func<T, TValue>> selector, TValue minValue)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildScalarComparison(minValue, ExpressionType.GreaterThanOrEqual));
    }

    public TBuilder MaxValue<TValue>(Expression<Func<T, TValue>> selector, TValue maxValue)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildScalarComparison(maxValue, ExpressionType.LessThanOrEqual));
    }

    // ── Range (INumber<TValue>) ────────────────────────────────────────────────

    public TBuilder InRange<TValue>(Expression<Func<T, TValue>> selector, TValue min, TValue max)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (TValue.CreateChecked(max).CompareTo(TValue.CreateChecked(min)) < 0)
            throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min.");
        var p = Expression.Parameter(typeof(TValue), "val");
        var minConst = Expression.Constant(min, typeof(TValue));
        var maxConst = Expression.Constant(max, typeof(TValue));
        var gte = Expression.GreaterThanOrEqual(p, minConst);
        var lte = Expression.LessThanOrEqual(p, maxConst);
        var predicate = Expression.Lambda<Func<TValue, bool>>(Expression.AndAlso(gte, lte), p);
        return _builder.Add(selector, predicate);
    }

    public TBuilder InRange<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> minSelector,
        Expression<Func<T, TValue>> maxSelector)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    public TBuilder IsBetweenExclusive<TValue>(Expression<Func<T, TValue>> selector, TValue min, TValue max)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (TValue.CreateChecked(max).CompareTo(TValue.CreateChecked(min)) <= 0)
            throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than min.");
        var p = Expression.Parameter(typeof(TValue), "val");
        var minConst = Expression.Constant(min, typeof(TValue));
        var maxConst = Expression.Constant(max, typeof(TValue));
        var gt = Expression.GreaterThan(p, minConst);
        var lt = Expression.LessThan(p, maxConst);
        var predicate = Expression.Lambda<Func<TValue, bool>>(Expression.AndAlso(gt, lt), p);
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsCloseTo<TValue>(Expression<Func<T, TValue>> selector, TValue value, TValue tolerance)
        where TValue : IFloatingPointIeee754<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (tolerance.CompareTo(TValue.Zero) < 0)
            throw new ArgumentOutOfRangeException(nameof(tolerance), "tolerance must be non-negative.");

        // IFloatingPointIeee754<TValue>.Abs is a static abstract member — cannot be used directly
        // in expression trees (CS8927). Build the expression tree manually via reflection.
        var p = Expression.Parameter(typeof(TValue), "val");
        var valueConst = Expression.Constant(value, typeof(TValue));
        var toleranceConst = Expression.Constant(tolerance, typeof(TValue));
        var absMethod = _absMethodCache.GetOrAdd(typeof(TValue), t =>
            t.GetMethod("Abs", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                ?? throw new NotSupportedException($"Type {t.Name} does not expose a static Abs method. Use a floating-point type such as float, double, or decimal."));
        var diff = Expression.Subtract(p, valueConst);
        var absExpr = Expression.Call(absMethod, diff);
        var lte = Expression.LessThanOrEqual(absExpr, toleranceConst);
        var predicate = Expression.Lambda<Func<TValue, bool>>(lte, p);
        return _builder.Add(selector, predicate);
    }

    // ── Parity (INumber<TValue>) ───────────────────────────────────────────────

    public TBuilder IsEven<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        var p = Expression.Parameter(typeof(TValue), "val");
        var two = Expression.Constant(TValue.CreateChecked(2), typeof(TValue));
        var zero = Expression.Constant(TValue.Zero, typeof(TValue));
        var mod = Expression.Modulo(p, two);
        var predicate = Expression.Lambda<Func<TValue, bool>>(Expression.Equal(mod, zero), p);
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsOdd<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        var p = Expression.Parameter(typeof(TValue), "val");
        var two = Expression.Constant(TValue.CreateChecked(2), typeof(TValue));
        var zero = Expression.Constant(TValue.Zero, typeof(TValue));
        var mod = Expression.Modulo(p, two);
        var predicate = Expression.Lambda<Func<TValue, bool>>(Expression.NotEqual(mod, zero), p);
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsMultipleOf<TValue>(Expression<Func<T, TValue>> selector, TValue divisor)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (divisor == TValue.Zero)
            throw new ArgumentException("Divisor cannot be zero.", nameof(divisor));
        var p = Expression.Parameter(typeof(TValue), "val");
        var div = Expression.Constant(divisor, typeof(TValue));
        var zero = Expression.Constant(TValue.Zero, typeof(TValue));
        var mod = Expression.Modulo(p, div);
        var predicate = Expression.Lambda<Func<TValue, bool>>(Expression.Equal(mod, zero), p);
        return _builder.Add(selector, predicate);
    }

    // ── Nullable numeric (struct, INumber<TValue>) ─────────────────────────────

    public TBuilder IsNullOrZero<TValue>(Expression<Func<T, TValue?>> selector)
        where TValue : struct, INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        var p = Expression.Parameter(typeof(TValue?), "val");
        var hasValueProp = typeof(TValue?).GetProperty("HasValue")!;
        var valueProp = typeof(TValue?).GetProperty("Value")!;
        var zero = Expression.Constant(TValue.Zero, typeof(TValue));
        var notHasValue = Expression.Not(Expression.Property(p, hasValueProp));
        var valueEqZero = Expression.Equal(Expression.Property(p, valueProp), zero);
        var body = Expression.OrElse(notHasValue, valueEqZero);
        var predicate = Expression.Lambda<Func<TValue?, bool>>(body, p);
        return _builder.Add(selector, predicate);
    }

    public TBuilder HasValue<TValue>(Expression<Func<T, TValue?>> selector)
        where TValue : struct, INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        var p = Expression.Parameter(typeof(TValue?), "val");
        var hasValueProp = typeof(TValue?).GetProperty("HasValue")!;
        var predicate = Expression.Lambda<Func<TValue?, bool>>(Expression.Property(p, hasValueProp), p);
        return _builder.Add(selector, predicate);
    }

    public TBuilder GreaterThan<TValue>(Expression<Func<T, TValue?>> selector, TValue value)
        where TValue : struct, INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        var p = Expression.Parameter(typeof(TValue?), "val");
        var valueProp = typeof(TValue?).GetProperty("Value")!;
        var gt = Expression.GreaterThan(Expression.Property(p, valueProp), Expression.Constant(value, typeof(TValue)));
        return _builder.Add(selector, BuildNullableScalarPredicate<TValue>(gt, p));
    }

    public TBuilder LessThan<TValue>(Expression<Func<T, TValue?>> selector, TValue value)
        where TValue : struct, INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        var p = Expression.Parameter(typeof(TValue?), "val");
        var valueProp = typeof(TValue?).GetProperty("Value")!;
        var lt = Expression.LessThan(Expression.Property(p, valueProp), Expression.Constant(value, typeof(TValue)));
        return _builder.Add(selector, BuildNullableScalarPredicate<TValue>(lt, p));
    }

    public TBuilder InRange<TValue>(Expression<Func<T, TValue?>> selector, TValue min, TValue max)
        where TValue : struct, INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (TValue.CreateChecked(max).CompareTo(TValue.CreateChecked(min)) < 0)
            throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        var p = Expression.Parameter(typeof(TValue?), "val");
        var valueProp = typeof(TValue?).GetProperty("Value")!;
        var innerVal = Expression.Property(p, valueProp);
        var rangeBody = Expression.AndAlso(
            Expression.GreaterThanOrEqual(innerVal, Expression.Constant(min, typeof(TValue))),
            Expression.LessThanOrEqual(innerVal, Expression.Constant(max, typeof(TValue))));
        return _builder.Add(selector, BuildNullableScalarPredicate<TValue>(rangeBody, p));
    }

    private static Expression<Func<TValue?, bool>> BuildNullableScalarPredicate<TValue>(
        Expression innerBody,
        ParameterExpression param)
        where TValue : struct
    {
        var hasValueProp = typeof(TValue?).GetProperty("HasValue")!;
        var hasValue = Expression.Property(param, hasValueProp);
        return Expression.Lambda<Func<TValue?, bool>>(
            Expression.AndAlso(hasValue, innerBody), param);
    }

    // ── IComparableExpression (generic, IComparable<TValue>) ──────────────────

    TBuilder IComparableExpression<TBuilder, T>.GreaterThan<TValue>(Expression<Func<T, TValue>> selector, TValue value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(value) > 0;
        return _builder.Add(selector, predicate);
    }

    TBuilder IComparableExpression<TBuilder, T>.GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(value) >= 0;
        return _builder.Add(selector, predicate);
    }

    TBuilder IComparableExpression<TBuilder, T>.LessThan<TValue>(Expression<Func<T, TValue>> selector, TValue value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(value) < 0;
        return _builder.Add(selector, predicate);
    }

    TBuilder IComparableExpression<TBuilder, T>.LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(value) <= 0;
        return _builder.Add(selector, predicate);
    }

    TBuilder IComparableExpression<TBuilder, T>.InRange<TValue>(Expression<Func<T, TValue>> selector, TValue min, TValue max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(min);
        ArgumentNullException.ThrowIfNull(max);
        if (min.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(nameof(min), "min must be less than or equal to max.");
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(min) >= 0 && val.CompareTo(max) <= 0;
        return _builder.Add(selector, predicate);
    }

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

    TBuilder IComparableExpression<TBuilder, T>.GreaterThan<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => AddCrossPropertyComparableComparison(selector, otherSelector, ExpressionType.GreaterThan);

    TBuilder IComparableExpression<TBuilder, T>.GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => AddCrossPropertyComparableComparison(selector, otherSelector, ExpressionType.GreaterThanOrEqual);

    TBuilder IComparableExpression<TBuilder, T>.LessThan<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => AddCrossPropertyComparableComparison(selector, otherSelector, ExpressionType.LessThan);

    TBuilder IComparableExpression<TBuilder, T>.LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => AddCrossPropertyComparableComparison(selector, otherSelector, ExpressionType.LessThanOrEqual);

    private TBuilder AddCrossPropertyComparableComparison<TValue>(
        Expression<Func<T, TValue>> selector,
        Expression<Func<T, TValue>> otherSelector,
        ExpressionType comparisonType)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(otherSelector);
        var param = selector.Parameters[0];
        var otherBody = new ParameterReplacer(otherSelector.Parameters[0], param).Visit(otherSelector.Body)!;
        var compareToMethod = typeof(IComparable<TValue>).GetMethod(nameof(IComparable<TValue>.CompareTo))!;
        var selectorBodyForCall = new ForceCloneVisitor().Visit(selector.Body)!;
        var callExpr = Expression.Call(selectorBodyForCall, compareToMethod, otherBody);
        var compareResult = Expression.MakeBinary(comparisonType, callExpr, Expression.Constant(0));
        Expression body;
        if (typeof(TValue).IsValueType)
        {
            body = compareResult;
        }
        else
        {
            Expression freshBody = new ForceCloneVisitor().Visit(selector.Body)!;
            var nullCheck = Expression.NotEqual(freshBody, Expression.Constant(null, typeof(TValue)));
            body = Expression.AndAlso(nullCheck, compareResult);
        }
        return _builder.Add(Expression.Lambda<Func<T, bool>>(body, param));
    }

    // ── Cross-property range ───────────────────────────────────────────────────

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
