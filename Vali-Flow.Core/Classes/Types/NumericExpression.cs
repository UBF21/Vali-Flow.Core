using System.Linq.Expressions;
using System.Numerics;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

/// <summary>
/// Provides numeric and comparable validation conditions for a fluent expression builder.
/// Implements both <see cref="INumericExpression{TBuilder,T}"/> (for <see cref="INumber{TSelf}"/>-constrained
/// types) and <see cref="IComparableExpression{TBuilder,T}"/> (for any <see cref="IComparable{T}"/> type).
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type returned by each fluent method.</typeparam>
/// <typeparam name="T">The entity type being validated.</typeparam>
public class NumericExpression<TBuilder, T> : INumericExpression<TBuilder, T>, IComparableExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, INumericExpression<TBuilder, T>, new()
{
    /// <summary>
    /// Per-type cache of the static <c>Abs</c> method resolved via reflection, used by
    /// <see cref="IsCloseTo{TValue}"/> to build expression trees for floating-point types.
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, System.Reflection.MethodInfo> _absMethodCache = new();

    /// <summary>The parent builder to which each condition is delegated.</summary>
    private readonly BaseExpression<TBuilder, T> _builder;

    /// <summary>Initializes a new instance of <see cref="NumericExpression{TBuilder,T}"/> with the given parent builder.</summary>
    /// <param name="builder">The parent builder that owns the condition list.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public NumericExpression(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    // ── Sign checks ────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected numeric value equals zero.</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Zero<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildComparisonToZero<TValue>(ExpressionType.Equal));
    }

    /// <summary>Validates that the selected numeric value is not zero.</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder NotZero<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildComparisonToZero<TValue>(ExpressionType.NotEqual));
    }

    /// <summary>Validates that the selected numeric value is strictly greater than zero.</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Positive<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildComparisonToZero<TValue>(ExpressionType.GreaterThan));
    }

    /// <summary>Validates that the selected numeric value is strictly less than zero.</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder Negative<TValue>(Expression<Func<T, TValue>> selector) where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildComparisonToZero<TValue>(ExpressionType.LessThan));
    }

    /// <summary>Builds a predicate that compares a numeric value to <see cref="INumber{TSelf}.Zero"/> using the given expression type.</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="type">The binary comparison operator to apply (e.g., <see cref="ExpressionType.Equal"/>).</param>
    /// <returns>A compiled predicate expression for the comparison.</returns>
    private static Expression<Func<TValue, bool>> BuildComparisonToZero<TValue>(ExpressionType type)
        where TValue : INumber<TValue>
    {
        var p = Expression.Parameter(typeof(TValue), "val");
        var zero = Expression.Constant(TValue.Zero, typeof(TValue));
        return Expression.Lambda<Func<TValue, bool>>(Expression.MakeBinary(type, p, zero), p);
    }

    // ── Scalar comparisons (INumber<TValue>) ───────────────────────────────────

    /// <summary>Builds a predicate that compares a numeric value to the constant <paramref name="value"/> using the given expression type.</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="value">The constant right-hand side of the comparison.</param>
    /// <param name="type">The binary comparison operator to apply.</param>
    /// <returns>A compiled predicate expression for the comparison.</returns>
    private static Expression<Func<TValue, bool>> BuildScalarComparison<TValue>(TValue value, ExpressionType type)
        where TValue : INumber<TValue>
    {
        var p = Expression.Parameter(typeof(TValue), "val");
        return Expression.Lambda<Func<TValue, bool>>(
            Expression.MakeBinary(type, p, Expression.Constant(value, typeof(TValue))), p);
    }

    /// <summary>Validates that the selected numeric value is strictly greater than <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildScalarComparison(value, ExpressionType.GreaterThan));
    }

    /// <summary>Validates that the selected numeric value is greater than or equal to <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <param name="value">The inclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildScalarComparison(value, ExpressionType.GreaterThanOrEqual));
    }

    /// <summary>Validates that the selected numeric value is strictly less than <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildScalarComparison(value, ExpressionType.LessThan));
    }

    /// <summary>Validates that the selected numeric value is less than or equal to <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <param name="value">The inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildScalarComparison(value, ExpressionType.LessThanOrEqual));
    }

    /// <summary>Validates that the selected numeric value is greater than or equal to <paramref name="minValue"/> (alias for <see cref="GreaterThanOrEqualTo{TValue}(Expression{Func{T,TValue}},TValue)"/>).</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <param name="minValue">The inclusive minimum allowed value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MinValue<TValue>(Expression<Func<T, TValue>> selector, TValue minValue)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildScalarComparison(minValue, ExpressionType.GreaterThanOrEqual));
    }

    /// <summary>Validates that the selected numeric value is less than or equal to <paramref name="maxValue"/> (alias for <see cref="LessThanOrEqualTo{TValue}(Expression{Func{T,TValue}},TValue)"/>).</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <param name="maxValue">The inclusive maximum allowed value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder MaxValue<TValue>(Expression<Func<T, TValue>> selector, TValue maxValue)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector, BuildScalarComparison(maxValue, ExpressionType.LessThanOrEqual));
    }

    // ── Range (INumber<TValue>) ────────────────────────────────────────────────

    /// <summary>Validates that the selected numeric value falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <param name="min">The inclusive lower bound.</param>
    /// <param name="max">The inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
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

    /// <summary>Validates that the selected value falls between the values selected by <paramref name="minSelector"/> and <paramref name="maxSelector"/> (inclusive), evaluated per entity instance.</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <param name="minSelector">Expression selecting the per-entity inclusive lower bound.</param>
    /// <param name="maxSelector">Expression selecting the per-entity inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder InRange<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> minSelector,
        Expression<Func<T, TValue>> maxSelector)
        where TValue : INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(minSelector);
        ArgumentNullException.ThrowIfNull(maxSelector);
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    /// <summary>Validates that the selected numeric value falls within (<paramref name="min"/>, <paramref name="max"/>) — both bounds are exclusive.</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <param name="min">The exclusive lower bound.</param>
    /// <param name="max">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than or equal to <paramref name="min"/>.</exception>
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

    /// <summary>Validates that the selected floating-point value is within <paramref name="tolerance"/> of <paramref name="value"/> (i.e., <c>|selected - value| &lt;= tolerance</c>).</summary>
    /// <typeparam name="TValue">A floating-point type implementing <see cref="IFloatingPointIeee754{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the floating-point property to validate.</param>
    /// <param name="value">The target value to compare against.</param>
    /// <param name="tolerance">The maximum absolute difference allowed. Must be non-negative.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tolerance"/> is negative.</exception>
    /// <exception cref="NotSupportedException">Thrown when <typeparamref name="TValue"/> does not expose a static <c>Abs</c> method.</exception>
    /// <remarks>
    /// The expression tree is built manually via reflection because <c>IFloatingPointIeee754&lt;TValue&gt;.Abs</c>
    /// is a static abstract member that cannot be called directly inside an expression tree (CS8927).
    /// The resolved <see cref="System.Reflection.MethodInfo"/> is cached in <see cref="_absMethodCache"/>.
    /// </remarks>
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

    /// <summary>Validates that the selected numeric value is even (i.e., divisible by 2).</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
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

    /// <summary>Validates that the selected numeric value is odd (i.e., not divisible by 2).</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
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

    /// <summary>Validates that the selected numeric value is evenly divisible by <paramref name="divisor"/>.</summary>
    /// <typeparam name="TValue">A numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the numeric property to validate.</param>
    /// <param name="divisor">The divisor. Must not be zero.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="divisor"/> is zero.</exception>
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

    /// <summary>Validates that the selected nullable numeric value is <see langword="null"/> or equal to zero.</summary>
    /// <typeparam name="TValue">A value type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the nullable numeric property to validate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
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

    /// <summary>Validates that the selected nullable numeric value has a non-null value (i.e., <c>HasValue == true</c>).</summary>
    /// <typeparam name="TValue">A value type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the nullable numeric property to validate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder HasValue<TValue>(Expression<Func<T, TValue?>> selector)
        where TValue : struct, INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        var p = Expression.Parameter(typeof(TValue?), "val");
        var hasValueProp = typeof(TValue?).GetProperty("HasValue")!;
        var predicate = Expression.Lambda<Func<TValue?, bool>>(Expression.Property(p, hasValueProp), p);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable numeric value has a value and that value is strictly greater than <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">A value type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the nullable numeric property to validate.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder GreaterThan<TValue>(Expression<Func<T, TValue?>> selector, TValue value)
        where TValue : struct, INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        var p = Expression.Parameter(typeof(TValue?), "val");
        var valueProp = typeof(TValue?).GetProperty("Value")!;
        var gt = Expression.GreaterThan(Expression.Property(p, valueProp), Expression.Constant(value, typeof(TValue)));
        return _builder.Add(selector, BuildNullableScalarPredicate<TValue>(gt, p));
    }

    /// <summary>Validates that the selected nullable numeric value has a value and that value is strictly less than <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">A value type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the nullable numeric property to validate.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder LessThan<TValue>(Expression<Func<T, TValue?>> selector, TValue value)
        where TValue : struct, INumber<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        var p = Expression.Parameter(typeof(TValue?), "val");
        var valueProp = typeof(TValue?).GetProperty("Value")!;
        var lt = Expression.LessThan(Expression.Property(p, valueProp), Expression.Constant(value, typeof(TValue)));
        return _builder.Add(selector, BuildNullableScalarPredicate<TValue>(lt, p));
    }

    /// <summary>Validates that the selected nullable numeric value has a value and that value falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    /// <typeparam name="TValue">A value type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="selector">Expression selecting the nullable numeric property to validate.</param>
    /// <param name="min">The inclusive lower bound.</param>
    /// <param name="max">The inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
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

    /// <summary>
    /// Wraps <paramref name="innerBody"/> in a null-guard: <c>param.HasValue &amp;&amp; innerBody</c>.
    /// Used by nullable overloads to produce a single, null-safe predicate expression.
    /// </summary>
    /// <typeparam name="TValue">The underlying value type of the nullable.</typeparam>
    /// <param name="innerBody">The expression representing the inner comparison, built against <c>param.Value</c>.</param>
    /// <param name="param">The <see cref="ParameterExpression"/> of type <typeparamref name="TValue"/>?.</param>
    /// <returns>A predicate expression that returns <see langword="false"/> for <see langword="null"/> inputs.</returns>
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

    /// <summary>Validates that the selected comparable value is strictly greater than <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">Any reference type implementing <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="selector">Expression selecting the property to validate.</param>
    /// <param name="value">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    TBuilder IComparableExpression<TBuilder, T>.GreaterThan<TValue>(Expression<Func<T, TValue>> selector, TValue value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(value) > 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected comparable value is greater than or equal to <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">Any reference type implementing <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="selector">Expression selecting the property to validate.</param>
    /// <param name="value">The inclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    TBuilder IComparableExpression<TBuilder, T>.GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(value) >= 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected comparable value is strictly less than <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">Any reference type implementing <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="selector">Expression selecting the property to validate.</param>
    /// <param name="value">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    TBuilder IComparableExpression<TBuilder, T>.LessThan<TValue>(Expression<Func<T, TValue>> selector, TValue value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(value) < 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected comparable value is less than or equal to <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">Any reference type implementing <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="selector">Expression selecting the property to validate.</param>
    /// <param name="value">The inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    TBuilder IComparableExpression<TBuilder, T>.LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        Expression<Func<TValue, bool>> predicate = val => val != null && val.CompareTo(value) <= 0;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected comparable value falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    /// <typeparam name="TValue">Any reference type implementing <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="selector">Expression selecting the property to validate.</param>
    /// <param name="min">The inclusive lower bound.</param>
    /// <param name="max">The inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
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

    /// <summary>Validates that the selected value is exactly equal to <paramref name="value"/> using structural equality.</summary>
    /// <typeparam name="TValue">Any type implementing <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="selector">Expression selecting the property to validate.</param>
    /// <param name="value">The expected value. Must not be <see langword="null"/>; use <c>Null()</c> for null checks.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
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

    /// <summary>Validates that the selected comparable value is strictly greater than the value selected by <paramref name="otherSelector"/>, evaluated per entity instance.</summary>
    /// <typeparam name="TValue">Any reference type implementing <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="selector">Expression selecting the left-hand property.</param>
    /// <param name="otherSelector">Expression selecting the right-hand property to compare against.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    TBuilder IComparableExpression<TBuilder, T>.GreaterThan<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => AddCrossPropertyComparableComparison(selector, otherSelector, ExpressionType.GreaterThan);

    /// <summary>Validates that the selected comparable value is greater than or equal to the value selected by <paramref name="otherSelector"/>, evaluated per entity instance.</summary>
    /// <typeparam name="TValue">Any reference type implementing <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="selector">Expression selecting the left-hand property.</param>
    /// <param name="otherSelector">Expression selecting the right-hand property to compare against.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    TBuilder IComparableExpression<TBuilder, T>.GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => AddCrossPropertyComparableComparison(selector, otherSelector, ExpressionType.GreaterThanOrEqual);

    /// <summary>Validates that the selected comparable value is strictly less than the value selected by <paramref name="otherSelector"/>, evaluated per entity instance.</summary>
    /// <typeparam name="TValue">Any reference type implementing <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="selector">Expression selecting the left-hand property.</param>
    /// <param name="otherSelector">Expression selecting the right-hand property to compare against.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    TBuilder IComparableExpression<TBuilder, T>.LessThan<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => AddCrossPropertyComparableComparison(selector, otherSelector, ExpressionType.LessThan);

    /// <summary>Validates that the selected comparable value is less than or equal to the value selected by <paramref name="otherSelector"/>, evaluated per entity instance.</summary>
    /// <typeparam name="TValue">Any reference type implementing <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="selector">Expression selecting the left-hand property.</param>
    /// <param name="otherSelector">Expression selecting the right-hand property to compare against.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    TBuilder IComparableExpression<TBuilder, T>.LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => AddCrossPropertyComparableComparison(selector, otherSelector, ExpressionType.LessThanOrEqual);

    /// <summary>
    /// Builds and registers a cross-property comparison condition using <see cref="IComparable{T}.CompareTo"/>.
    /// For reference types an additional null-guard on the left-hand selector is prepended.
    /// </summary>
    /// <typeparam name="TValue">Any type implementing <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="selector">Left-hand property selector.</param>
    /// <param name="otherSelector">Right-hand property selector (rebased to the same parameter).</param>
    /// <param name="comparisonType">The binary comparison operator applied to the <c>CompareTo</c> result vs. <c>0</c>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
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

    /// <summary>
    /// Builds and registers a cross-property range condition:
    /// <c>selector &gt;= minSelector &amp;&amp; selector &lt;= maxSelector</c>, all evaluated on the same entity instance.
    /// </summary>
    /// <typeparam name="TNum">The numeric type shared by all three selectors.</typeparam>
    /// <param name="selector">Expression selecting the value to check.</param>
    /// <param name="minSelector">Expression selecting the inclusive lower bound.</param>
    /// <param name="maxSelector">Expression selecting the inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
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
