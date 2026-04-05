using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Classes.General;
using Vali_Flow.Core.Classes.Types;
using Vali_Flow.Core.Models;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Builder;

/// <summary>
/// A fluent builder for constructing <see cref="Expression{TDelegate}"/> trees that are safe to pass
/// to EF Core <c>DbContext.Where()</c> and other IQueryable providers.
/// </summary>
/// <remarks>
/// <para>
/// <b>Use this builder for EF Core queries.</b> Only methods whose predicates are translatable by the major EF Core relational providers
/// (SQL Server, PostgreSQL, MySQL/Pomelo 5.0+, Oracle EF Core 7.0+) are exposed.
/// <b>SQLite note:</b> Methods that access <c>.DayOfWeek</c> are not translated by the SQLite EF Core provider —
/// use in-memory filtering via <see cref="ValiFlow{T}"/> or a raw SQL workaround for SQLite.
/// Some methods may not translate on less common providers or when used with non-navigation collection properties.
/// Methods that rely on in-memory delegates (e.g. <c>Regex.IsMatch</c>,
/// <c>char.IsDigit</c>, <c>DateTime.Today</c>, <c>%</c> modulo, <c>Enumerable.All/Any</c> with lambdas)
/// are intentionally absent.
/// </para>
/// <para>
/// For in-memory validation where all methods are available, use <see cref="ValiFlow{T}"/> instead.
/// </para>
/// </remarks>
/// <typeparam name="T">The entity type being validated or filtered.</typeparam>
public sealed class ValiFlowQuery<T> : BaseExpression<ValiFlowQuery<T>, T>
{
    private readonly BooleanExpressionQuery<ValiFlowQuery<T>, T> _boolean;
    private readonly ComparisonExpression<ValiFlowQuery<T>, T> _comparison;
    private readonly StringExpressionQuery<ValiFlowQuery<T>, T> _string;
    private readonly CollectionExpressionQuery<ValiFlowQuery<T>, T> _collection;
    private readonly NumericExpressionQuery<ValiFlowQuery<T>, T> _numeric;
    private readonly DateTimeExpressionQuery<ValiFlowQuery<T>, T> _dateTime;
    private readonly DateTimeOffsetExpressionQuery<ValiFlowQuery<T>, T> _dateTimeOffset;
    private readonly DateOnlyExpressionQuery<ValiFlowQuery<T>, T> _dateOnly;
    private readonly TimeOnlyExpressionQuery<ValiFlowQuery<T>, T> _timeOnly;

    public ValiFlowQuery()
    {
        _boolean = new BooleanExpressionQuery<ValiFlowQuery<T>, T>(this);
        _comparison = new ComparisonExpression<ValiFlowQuery<T>, T>(this);
        _string = new StringExpressionQuery<ValiFlowQuery<T>, T>(this);
        _collection = new CollectionExpressionQuery<ValiFlowQuery<T>, T>(this);
        _numeric = new NumericExpressionQuery<ValiFlowQuery<T>, T>(this);
        _dateTime = new DateTimeExpressionQuery<ValiFlowQuery<T>, T>(this);
        _dateTimeOffset = new DateTimeOffsetExpressionQuery<ValiFlowQuery<T>, T>(this);
        _dateOnly = new DateOnlyExpressionQuery<ValiFlowQuery<T>, T>(this);
        _timeOnly = new TimeOnlyExpressionQuery<ValiFlowQuery<T>, T>(this);
    }

    // ── Boolean ───────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected bool property is <see langword="true"/>.</summary>
    public ValiFlowQuery<T> IsTrue(Expression<Func<T, bool>> selector) => _boolean.IsTrue(selector);

    /// <summary>Validates that the selected bool property is <see langword="false"/>.</summary>
    public ValiFlowQuery<T> IsFalse(Expression<Func<T, bool>> selector) => _boolean.IsFalse(selector);

    // ── Comparison ────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected property is not null.</summary>
    public ValiFlowQuery<T> NotNull<TProperty>(Expression<Func<T, TProperty?>> selector) => _comparison.NotNull(selector);

    /// <summary>Validates that the selected property is null.</summary>
    public ValiFlowQuery<T> Null<TProperty>(Expression<Func<T, TProperty?>> selector) => _comparison.Null(selector);

    /// <summary>Validates that the selected property equals <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> EqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IEquatable<TValue> => _comparison.EqualTo(selector, value);

    /// <summary>Validates that the selected property does not equal <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> NotEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IEquatable<TValue> => _comparison.NotEqualTo(selector, value);

    // ── String ────────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected string has a minimum length.</summary>
    public ValiFlowQuery<T> MinLength(Expression<Func<T, string?>> selector, int minLength) => _string.MinLength(selector, minLength);

    /// <summary>Validates that the selected string does not exceed a maximum length. Null values pass.</summary>
    public ValiFlowQuery<T> MaxLength(Expression<Func<T, string?>> selector, int maxLength) => _string.MaxLength(selector, maxLength);

    /// <summary>Validates that the selected string has an exact length.</summary>
    public ValiFlowQuery<T> ExactLength(Expression<Func<T, string?>> selector, int length) => _string.ExactLength(selector, length);

    /// <summary>Validates that the selected string length is between <paramref name="min"/> and <paramref name="max"/> (inclusive).</summary>
    public ValiFlowQuery<T> LengthBetween(Expression<Func<T, string?>> selector, int min, int max) => _string.LengthBetween(selector, min, max);

    /// <summary>Validates that the selected string is null or empty.</summary>
    public ValiFlowQuery<T> IsNullOrEmpty(Expression<Func<T, string?>> selector) => _string.IsNullOrEmpty(selector);

    /// <summary>Validates that the selected string is not null and not empty.</summary>
    public ValiFlowQuery<T> IsNotNullOrEmpty(Expression<Func<T, string?>> selector) => _string.IsNotNullOrEmpty(selector);

    /// <summary>Validates that the selected string is null or contains only whitespace.</summary>
    public ValiFlowQuery<T> IsNullOrWhiteSpace(Expression<Func<T, string?>> selector) => _string.IsNullOrWhiteSpace(selector);

    /// <summary>Validates that the selected string is not null and contains at least one non-whitespace character.</summary>
    public ValiFlowQuery<T> IsNotNullOrWhiteSpace(Expression<Func<T, string?>> selector) => _string.IsNotNullOrWhiteSpace(selector);

    /// <summary>Validates that the selected string starts with <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> StartsWith(Expression<Func<T, string?>> selector, string value) => _string.StartsWith(selector, value);

    /// <summary>Validates that the selected string ends with <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> EndsWith(Expression<Func<T, string?>> selector, string value) => _string.EndsWith(selector, value);

    /// <summary>Validates that the selected string contains <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> Contains(Expression<Func<T, string?>> selector, string value) => _string.Contains(selector, value);

    /// <summary>Validates that the selected string has no leading or trailing whitespace.</summary>
    public ValiFlowQuery<T> IsTrimmed(Expression<Func<T, string?>> selector) => _string.IsTrimmed(selector);

    /// <summary>Validates that the selected string equals its lowercase form.</summary>
    public ValiFlowQuery<T> IsLowerCase(Expression<Func<T, string?>> selector) => _string.IsLowerCase(selector);

    /// <summary>Validates that the selected string equals its uppercase form.</summary>
    public ValiFlowQuery<T> IsUpperCase(Expression<Func<T, string?>> selector) => _string.IsUpperCase(selector);

    /// <summary>Validates that the selected string equals <paramref name="value"/> (case-insensitive, ordinal).</summary>
    public ValiFlowQuery<T> EqualToIgnoreCase(Expression<Func<T, string?>> selector, string value) => _string.EqualToIgnoreCase(selector, value);

    /// <summary>Validates that the selected string starts with <paramref name="value"/> (case-insensitive, ordinal).</summary>
    public ValiFlowQuery<T> StartsWithIgnoreCase(Expression<Func<T, string?>> selector, string value) => _string.StartsWithIgnoreCase(selector, value);

    /// <summary>Validates that the selected string ends with <paramref name="value"/> (case-insensitive, ordinal).</summary>
    public ValiFlowQuery<T> EndsWithIgnoreCase(Expression<Func<T, string?>> selector, string value) => _string.EndsWithIgnoreCase(selector, value);

    /// <summary>Validates that the selected string contains <paramref name="value"/> (case-insensitive, ordinal).</summary>
    public ValiFlowQuery<T> ContainsIgnoreCase(Expression<Func<T, string?>> selector, string value) => _string.ContainsIgnoreCase(selector, value);

    /// <summary>Validates that the selected string does <b>not</b> contain <paramref name="value"/>. A <c>null</c> value passes.</summary>
    public ValiFlowQuery<T> NotContains(Expression<Func<T, string?>> selector, string value) => _string.NotContains(selector, value);

    /// <summary>Validates that the selected string does <b>not</b> start with <paramref name="value"/>. A <c>null</c> value passes.</summary>
    public ValiFlowQuery<T> NotStartsWith(Expression<Func<T, string?>> selector, string value) => _string.NotStartsWith(selector, value);

    /// <summary>Validates that the selected string does <b>not</b> end with <paramref name="value"/>. A <c>null</c> value passes.</summary>
    public ValiFlowQuery<T> NotEndsWith(Expression<Func<T, string?>> selector, string value) => _string.NotEndsWith(selector, value);

    // ── Collection ────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected collection is not null and has at least one element.</summary>
    public ValiFlowQuery<T> NotEmpty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector) => _collection.NotEmpty(selector);

    /// <summary>Validates that the selected collection is not null and has no elements.</summary>
    public ValiFlowQuery<T> Empty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector) => _collection.Empty(selector);

    /// <summary>Validates that the selected scalar value is contained in <paramref name="values"/>.</summary>
    public ValiFlowQuery<T> In<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values) => _collection.In(selector, values);

    /// <summary>Validates that the selected scalar value is NOT contained in <paramref name="values"/>.</summary>
    public ValiFlowQuery<T> NotIn<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values) => _collection.NotIn(selector, values);

    /// <summary>Validates that the collection has exactly <paramref name="count"/> elements.</summary>
    public ValiFlowQuery<T> Count<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int count) => _collection.Count(selector, count);

    /// <summary>Validates that the collection has at least <paramref name="min"/> elements.</summary>
    public ValiFlowQuery<T> MinCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min) => _collection.MinCount(selector, min);

    /// <summary>Validates that the collection has at most <paramref name="max"/> elements.</summary>
    public ValiFlowQuery<T> MaxCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int max) => _collection.MaxCount(selector, max);

    /// <summary>Validates that the selected collection has between <paramref name="min"/> and <paramref name="max"/> elements (inclusive).</summary>
    public ValiFlowQuery<T> CountBetween<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min, int max) => _collection.CountBetween(selector, min, max);

    // ── Numeric — int ─────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="int"/> equals zero.</summary>
    public ValiFlowQuery<T> Zero(Expression<Func<T, int>> selector) => _numeric.Zero(selector);

    /// <summary>Validates that the selected <see cref="int"/> is not zero.</summary>
    public ValiFlowQuery<T> NotZero(Expression<Func<T, int>> selector) => _numeric.NotZero(selector);

    /// <summary>Validates that the selected <see cref="int"/> is greater than zero.</summary>
    public ValiFlowQuery<T> Positive(Expression<Func<T, int>> selector) => _numeric.Positive(selector);

    /// <summary>Validates that the selected <see cref="int"/> is less than zero.</summary>
    public ValiFlowQuery<T> Negative(Expression<Func<T, int>> selector) => _numeric.Negative(selector);

    /// <summary>Validates that the selected <see cref="int"/> is greater than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, int>> selector, int value) => _numeric.GreaterThan(selector, value);

    /// <summary>Validates that the selected <see cref="int"/> is greater than or equal to <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThanOrEqualTo(Expression<Func<T, int>> selector, int value) => _numeric.GreaterThanOrEqualTo(selector, value);

    /// <summary>Validates that the selected <see cref="int"/> is less than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, int>> selector, int value) => _numeric.LessThan(selector, value);

    /// <summary>Validates that the selected <see cref="int"/> is less than or equal to <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThanOrEqualTo(Expression<Func<T, int>> selector, int value) => _numeric.LessThanOrEqualTo(selector, value);

    /// <summary>Validates that the selected <see cref="int"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    public ValiFlowQuery<T> MinValue(Expression<Func<T, int>> selector, int minValue) => _numeric.MinValue(selector, minValue);

    /// <summary>Validates that the selected <see cref="int"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    public ValiFlowQuery<T> MaxValue(Expression<Func<T, int>> selector, int maxValue) => _numeric.MaxValue(selector, maxValue);

    /// <summary>Validates that the selected <see cref="int"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, int>> selector, int min, int max) => _numeric.InRange(selector, min, max);

    /// <summary>Validates that the selected <see cref="int"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, int>> selector,
        Expression<Func<T, int>> minSelector, Expression<Func<T, int>> maxSelector)
        => _numeric.InRange(selector, minSelector, maxSelector);

    /// <summary>Validates that the selected <see cref="int"/> is even.</summary>
    public ValiFlowQuery<T> IsEven(Expression<Func<T, int>> selector) => _numeric.IsEven(selector);

    /// <summary>Validates that the selected <see cref="int"/> is odd.</summary>
    public ValiFlowQuery<T> IsOdd(Expression<Func<T, int>> selector) => _numeric.IsOdd(selector);

    /// <summary>Validates that the selected <see cref="int"/> is a multiple of <paramref name="divisor"/>.</summary>
    public ValiFlowQuery<T> IsMultipleOf(Expression<Func<T, int>> selector, int divisor) => _numeric.IsMultipleOf(selector, divisor);

    // ── Numeric — long ────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="long"/> equals zero.</summary>
    public ValiFlowQuery<T> Zero(Expression<Func<T, long>> selector) => _numeric.Zero(selector);

    /// <summary>Validates that the selected <see cref="long"/> does not equal zero.</summary>
    public ValiFlowQuery<T> NotZero(Expression<Func<T, long>> selector) => _numeric.NotZero(selector);

    /// <summary>Validates that the selected <see cref="long"/> is greater than zero.</summary>
    public ValiFlowQuery<T> Positive(Expression<Func<T, long>> selector) => _numeric.Positive(selector);

    /// <summary>Validates that the selected <see cref="long"/> is less than zero.</summary>
    public ValiFlowQuery<T> Negative(Expression<Func<T, long>> selector) => _numeric.Negative(selector);

    /// <summary>Validates that the selected <see cref="long"/> is greater than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, long>> selector, long value) => _numeric.GreaterThan(selector, value);

    /// <summary>Validates that the selected <see cref="long"/> is greater than or equal to <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThanOrEqualTo(Expression<Func<T, long>> selector, long value) => _numeric.GreaterThanOrEqualTo(selector, value);

    /// <summary>Validates that the selected <see cref="long"/> is less than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, long>> selector, long value) => _numeric.LessThan(selector, value);

    /// <summary>Validates that the selected <see cref="long"/> is less than or equal to <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThanOrEqualTo(Expression<Func<T, long>> selector, long value) => _numeric.LessThanOrEqualTo(selector, value);

    /// <summary>Validates that the selected <see cref="long"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    public ValiFlowQuery<T> MinValue(Expression<Func<T, long>> selector, long minValue) => _numeric.MinValue(selector, minValue);

    /// <summary>Validates that the selected <see cref="long"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    public ValiFlowQuery<T> MaxValue(Expression<Func<T, long>> selector, long maxValue) => _numeric.MaxValue(selector, maxValue);

    /// <summary>Validates that the selected <see cref="long"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, long>> selector, long min, long max) => _numeric.InRange(selector, min, max);

    /// <summary>Validates that the selected <see cref="long"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, long>> selector,
        Expression<Func<T, long>> minSelector, Expression<Func<T, long>> maxSelector)
        => _numeric.InRange(selector, minSelector, maxSelector);

    /// <summary>Validates that the selected <see cref="long"/> is even.</summary>
    public ValiFlowQuery<T> IsEven(Expression<Func<T, long>> selector) => _numeric.IsEven(selector);

    /// <summary>Validates that the selected <see cref="long"/> is odd.</summary>
    public ValiFlowQuery<T> IsOdd(Expression<Func<T, long>> selector) => _numeric.IsOdd(selector);

    /// <summary>Validates that the selected <see cref="long"/> is a multiple of <paramref name="divisor"/>.</summary>
    public ValiFlowQuery<T> IsMultipleOf(Expression<Func<T, long>> selector, long divisor) => _numeric.IsMultipleOf(selector, divisor);

    // ── Numeric — double ──────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="double"/> equals zero.</summary>
    public ValiFlowQuery<T> Zero(Expression<Func<T, double>> selector) => _numeric.Zero(selector);

    /// <summary>Validates that the selected <see cref="double"/> does not equal zero.</summary>
    public ValiFlowQuery<T> NotZero(Expression<Func<T, double>> selector) => _numeric.NotZero(selector);

    /// <summary>Validates that the selected <see cref="double"/> is greater than zero.</summary>
    public ValiFlowQuery<T> Positive(Expression<Func<T, double>> selector) => _numeric.Positive(selector);

    /// <summary>Validates that the selected <see cref="double"/> is less than zero.</summary>
    public ValiFlowQuery<T> Negative(Expression<Func<T, double>> selector) => _numeric.Negative(selector);

    /// <summary>Validates that the selected <see cref="double"/> is greater than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, double>> selector, double value) => _numeric.GreaterThan(selector, value);

    /// <summary>Validates that the selected <see cref="double"/> is greater than or equal to <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThanOrEqualTo(Expression<Func<T, double>> selector, double value) => _numeric.GreaterThanOrEqualTo(selector, value);

    /// <summary>Validates that the selected <see cref="double"/> is less than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, double>> selector, double value) => _numeric.LessThan(selector, value);

    /// <summary>Validates that the selected <see cref="double"/> is less than or equal to <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThanOrEqualTo(Expression<Func<T, double>> selector, double value) => _numeric.LessThanOrEqualTo(selector, value);

    /// <summary>Validates that the selected <see cref="double"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    public ValiFlowQuery<T> MinValue(Expression<Func<T, double>> selector, double minValue) => _numeric.MinValue(selector, minValue);

    /// <summary>Validates that the selected <see cref="double"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    public ValiFlowQuery<T> MaxValue(Expression<Func<T, double>> selector, double maxValue) => _numeric.MaxValue(selector, maxValue);

    /// <summary>Validates that the selected <see cref="double"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, double>> selector, double min, double max) => _numeric.InRange(selector, min, max);

    /// <summary>Validates that the selected <see cref="double"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, double>> selector,
        Expression<Func<T, double>> minSelector, Expression<Func<T, double>> maxSelector)
        => _numeric.InRange(selector, minSelector, maxSelector);

    // ── Numeric — decimal ─────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="decimal"/> equals zero.</summary>
    public ValiFlowQuery<T> Zero(Expression<Func<T, decimal>> selector) => _numeric.Zero(selector);

    /// <summary>Validates that the selected <see cref="decimal"/> does not equal zero.</summary>
    public ValiFlowQuery<T> NotZero(Expression<Func<T, decimal>> selector) => _numeric.NotZero(selector);

    /// <summary>Validates that the selected <see cref="decimal"/> is greater than zero.</summary>
    public ValiFlowQuery<T> Positive(Expression<Func<T, decimal>> selector) => _numeric.Positive(selector);

    /// <summary>Validates that the selected <see cref="decimal"/> is less than zero.</summary>
    public ValiFlowQuery<T> Negative(Expression<Func<T, decimal>> selector) => _numeric.Negative(selector);

    /// <summary>Validates that the selected <see cref="decimal"/> is greater than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, decimal>> selector, decimal value) => _numeric.GreaterThan(selector, value);

    /// <summary>Validates that the selected <see cref="decimal"/> is greater than or equal to <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value) => _numeric.GreaterThanOrEqualTo(selector, value);

    /// <summary>Validates that the selected <see cref="decimal"/> is less than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, decimal>> selector, decimal value) => _numeric.LessThan(selector, value);

    /// <summary>Validates that the selected <see cref="decimal"/> is less than or equal to <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value) => _numeric.LessThanOrEqualTo(selector, value);

    /// <summary>Validates that the selected <see cref="decimal"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    public ValiFlowQuery<T> MinValue(Expression<Func<T, decimal>> selector, decimal minValue) => _numeric.MinValue(selector, minValue);

    /// <summary>Validates that the selected <see cref="decimal"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    public ValiFlowQuery<T> MaxValue(Expression<Func<T, decimal>> selector, decimal maxValue) => _numeric.MaxValue(selector, maxValue);

    /// <summary>Validates that the selected <see cref="decimal"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, decimal>> selector, decimal min, decimal max) => _numeric.InRange(selector, min, max);

    /// <summary>Validates that the selected <see cref="decimal"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, decimal>> selector,
        Expression<Func<T, decimal>> minSelector, Expression<Func<T, decimal>> maxSelector)
        => _numeric.InRange(selector, minSelector, maxSelector);

    // ── Numeric — float ───────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="float"/> equals zero.</summary>
    public ValiFlowQuery<T> Zero(Expression<Func<T, float>> selector) => _numeric.Zero(selector);

    /// <summary>Validates that the selected <see cref="float"/> does not equal zero.</summary>
    public ValiFlowQuery<T> NotZero(Expression<Func<T, float>> selector) => _numeric.NotZero(selector);

    /// <summary>Validates that the selected <see cref="float"/> is greater than zero.</summary>
    public ValiFlowQuery<T> Positive(Expression<Func<T, float>> selector) => _numeric.Positive(selector);

    /// <summary>Validates that the selected <see cref="float"/> is less than zero.</summary>
    public ValiFlowQuery<T> Negative(Expression<Func<T, float>> selector) => _numeric.Negative(selector);

    /// <summary>Validates that the selected <see cref="float"/> is greater than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, float>> selector, float value) => _numeric.GreaterThan(selector, value);

    /// <summary>Validates that the selected <see cref="float"/> is greater than or equal to <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThanOrEqualTo(Expression<Func<T, float>> selector, float value) => _numeric.GreaterThanOrEqualTo(selector, value);

    /// <summary>Validates that the selected <see cref="float"/> is less than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, float>> selector, float value) => _numeric.LessThan(selector, value);

    /// <summary>Validates that the selected <see cref="float"/> is less than or equal to <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThanOrEqualTo(Expression<Func<T, float>> selector, float value) => _numeric.LessThanOrEqualTo(selector, value);

    /// <summary>Validates that the selected <see cref="float"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    public ValiFlowQuery<T> MinValue(Expression<Func<T, float>> selector, float minValue) => _numeric.MinValue(selector, minValue);

    /// <summary>Validates that the selected <see cref="float"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    public ValiFlowQuery<T> MaxValue(Expression<Func<T, float>> selector, float maxValue) => _numeric.MaxValue(selector, maxValue);

    /// <summary>Validates that the selected <see cref="float"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, float>> selector, float min, float max) => _numeric.InRange(selector, min, max);

    /// <summary>Validates that the selected <see cref="float"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, float>> selector,
        Expression<Func<T, float>> minSelector, Expression<Func<T, float>> maxSelector)
        => _numeric.InRange(selector, minSelector, maxSelector);

    // ── Numeric — short ───────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="short"/> equals zero.</summary>
    public ValiFlowQuery<T> Zero(Expression<Func<T, short>> selector) => _numeric.Zero(selector);

    /// <summary>Validates that the selected <see cref="short"/> does not equal zero.</summary>
    public ValiFlowQuery<T> NotZero(Expression<Func<T, short>> selector) => _numeric.NotZero(selector);

    /// <summary>Validates that the selected <see cref="short"/> is greater than zero.</summary>
    public ValiFlowQuery<T> Positive(Expression<Func<T, short>> selector) => _numeric.Positive(selector);

    /// <summary>Validates that the selected <see cref="short"/> is less than zero.</summary>
    public ValiFlowQuery<T> Negative(Expression<Func<T, short>> selector) => _numeric.Negative(selector);

    /// <summary>Validates that the selected <see cref="short"/> is greater than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, short>> selector, short value) => _numeric.GreaterThan(selector, value);

    /// <summary>Validates that the selected <see cref="short"/> is greater than or equal to <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThanOrEqualTo(Expression<Func<T, short>> selector, short value) => _numeric.GreaterThanOrEqualTo(selector, value);

    /// <summary>Validates that the selected <see cref="short"/> is less than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, short>> selector, short value) => _numeric.LessThan(selector, value);

    /// <summary>Validates that the selected <see cref="short"/> is less than or equal to <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThanOrEqualTo(Expression<Func<T, short>> selector, short value) => _numeric.LessThanOrEqualTo(selector, value);

    /// <summary>Validates that the selected <see cref="short"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    public ValiFlowQuery<T> MinValue(Expression<Func<T, short>> selector, short minValue) => _numeric.MinValue(selector, minValue);

    /// <summary>Validates that the selected <see cref="short"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    public ValiFlowQuery<T> MaxValue(Expression<Func<T, short>> selector, short maxValue) => _numeric.MaxValue(selector, maxValue);

    /// <summary>Validates that the selected <see cref="short"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, short>> selector, short min, short max) => _numeric.InRange(selector, min, max);

    /// <summary>Validates that the selected <see cref="short"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, short>> selector,
        Expression<Func<T, short>> minSelector, Expression<Func<T, short>> maxSelector)
        => _numeric.InRange(selector, minSelector, maxSelector);

    // ── Nullable numeric ──────────────────────────────────────────────────────

    /// <summary>Validates that the selected nullable <see cref="int"/> is null or zero.</summary>
    public ValiFlowQuery<T> IsNullOrZero(Expression<Func<T, int?>> selector) => _numeric.IsNullOrZero(selector);

    /// <summary>Validates that the selected nullable <see cref="long"/> is null or zero.</summary>
    public ValiFlowQuery<T> IsNullOrZero(Expression<Func<T, long?>> selector) => _numeric.IsNullOrZero(selector);

    /// <summary>Validates that the selected nullable <see cref="decimal"/> is null or zero.</summary>
    public ValiFlowQuery<T> IsNullOrZero(Expression<Func<T, decimal?>> selector) => _numeric.IsNullOrZero(selector);

    /// <summary>Validates that the selected nullable <see cref="double"/> is null or zero.</summary>
    public ValiFlowQuery<T> IsNullOrZero(Expression<Func<T, double?>> selector) => _numeric.IsNullOrZero(selector);

    /// <summary>Validates that the selected nullable <see cref="float"/> is null or equal to zero.</summary>
    public ValiFlowQuery<T> IsNullOrZero(Expression<Func<T, float?>> selector) => _numeric.IsNullOrZero(selector);

    /// <summary>Validates that the selected nullable <see cref="int"/> has a value.</summary>
    public ValiFlowQuery<T> HasValue(Expression<Func<T, int?>> selector) => _numeric.HasValue(selector);

    /// <summary>Validates that the selected nullable <see cref="long"/> has a value.</summary>
    public ValiFlowQuery<T> HasValue(Expression<Func<T, long?>> selector) => _numeric.HasValue(selector);

    /// <summary>Validates that the selected nullable <see cref="decimal"/> has a value.</summary>
    public ValiFlowQuery<T> HasValue(Expression<Func<T, decimal?>> selector) => _numeric.HasValue(selector);

    /// <summary>Validates that the selected nullable <see cref="double"/> has a value.</summary>
    public ValiFlowQuery<T> HasValue(Expression<Func<T, double?>> selector) => _numeric.HasValue(selector);

    /// <summary>Validates that the selected nullable <see cref="float"/> has a value (is not null).</summary>
    public ValiFlowQuery<T> HasValue(Expression<Func<T, float?>> selector) => _numeric.HasValue(selector);

    /// <summary>Validates that the selected nullable <see cref="int"/> has a value greater than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, int?>> selector, int value) => _numeric.GreaterThan(selector, value);

    /// <summary>Validates that the selected nullable <see cref="long"/> has a value greater than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, long?>> selector, long value) => _numeric.GreaterThan(selector, value);

    /// <summary>Validates that the selected nullable <see cref="decimal"/> has a value greater than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, decimal?>> selector, decimal value) => _numeric.GreaterThan(selector, value);

    /// <summary>Validates that the selected nullable <see cref="int"/> has a value less than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, int?>> selector, int value) => _numeric.LessThan(selector, value);

    /// <summary>Validates that the selected nullable <see cref="long"/> has a value less than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, long?>> selector, long value) => _numeric.LessThan(selector, value);

    /// <summary>Validates that the selected nullable <see cref="decimal"/> has a value less than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, decimal?>> selector, decimal value) => _numeric.LessThan(selector, value);

    /// <summary>Validates that the selected nullable <see cref="int"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, int?>> selector, int min, int max) => _numeric.InRange(selector, min, max);

    /// <summary>Validates that the selected nullable <see cref="long"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, long?>> selector, long min, long max) => _numeric.InRange(selector, min, max);

    /// <summary>Validates that the selected nullable <see cref="decimal"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, decimal?>> selector, decimal min, decimal max) => _numeric.InRange(selector, min, max);

    /// <summary>Validates that the selected nullable <see cref="double"/> has a value greater than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, double?>> selector, double value) => _numeric.GreaterThan(selector, value);

    /// <summary>Validates that the selected nullable <see cref="double"/> has a value less than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, double?>> selector, double value) => _numeric.LessThan(selector, value);

    /// <summary>Validates that the selected nullable <see cref="double"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, double?>> selector, double min, double max) => _numeric.InRange(selector, min, max);

    /// <summary>Validates that the selected nullable <see cref="float"/> has a value greater than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, float?>> selector, float value) => _numeric.GreaterThan(selector, value);

    /// <summary>Validates that the selected nullable <see cref="float"/> has a value less than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, float?>> selector, float value) => _numeric.LessThan(selector, value);

    /// <summary>Validates that the selected nullable <see cref="float"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, float?>> selector, float min, float max) => _numeric.InRange(selector, min, max);

    /// <summary>Validates that the selected nullable <see cref="short"/> is null or equal to zero.</summary>
    public ValiFlowQuery<T> IsNullOrZero(Expression<Func<T, short?>> selector) => _numeric.IsNullOrZero(selector);

    /// <summary>Validates that the selected nullable <see cref="short"/> has a value (is not null).</summary>
    public ValiFlowQuery<T> HasValue(Expression<Func<T, short?>> selector) => _numeric.HasValue(selector);

    /// <summary>Validates that the selected nullable <see cref="short"/> has a value greater than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, short?>> selector, short value) => _numeric.GreaterThan(selector, value);

    /// <summary>Validates that the selected nullable <see cref="short"/> has a value less than <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, short?>> selector, short value) => _numeric.LessThan(selector, value);

    /// <summary>Validates that the selected nullable <see cref="short"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public ValiFlowQuery<T> InRange(Expression<Func<T, short?>> selector, short min, short max) => _numeric.InRange(selector, min, max);

    // ── DateTime ──────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on <paramref name="date"/> (date-only, ignoring time-of-day).</summary>
    public ValiFlowQuery<T> ExactDate(Expression<Func<T, DateTime>> selector, DateTime date) => _dateTime.ExactDate(selector, date);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls before <paramref name="date"/> (date-only comparison, ignores time-of-day).</summary>
    [Obsolete("Use IsBefore instead. BeforeDate will be removed in a future version.")]
    public ValiFlowQuery<T> BeforeDate(Expression<Func<T, DateTime>> selector, DateTime date) => _dateTime.BeforeDate(selector, date);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls after <paramref name="date"/> (date-only comparison, ignores time-of-day).</summary>
    [Obsolete("Use IsAfter instead. AfterDate will be removed in a future version.")]
    public ValiFlowQuery<T> AfterDate(Expression<Func<T, DateTime>> selector, DateTime date) => _dateTime.AfterDate(selector, date);

    /// <summary>Validates that the selected <see cref="DateTime"/> is before <paramref name="date"/> (full DateTime comparison including time-of-day).</summary>
    public ValiFlowQuery<T> IsBefore(Expression<Func<T, DateTime>> selector, DateTime date) => _dateTime.IsBefore(selector, date);

    /// <summary>Validates that the selected <see cref="DateTime"/> is after <paramref name="date"/> (full DateTime comparison including time-of-day).</summary>
    public ValiFlowQuery<T> IsAfter(Expression<Func<T, DateTime>> selector, DateTime date) => _dateTime.IsAfter(selector, date);

    /// <summary>Validates that the selected <see cref="DateTime"/> is on a date between <paramref name="startDate"/> and <paramref name="endDate"/> (inclusive, date-only).</summary>
    public ValiFlowQuery<T> BetweenDates(Expression<Func<T, DateTime>> selector, DateTime startDate, DateTime endDate)
        => _dateTime.BetweenDates(selector, startDate, endDate);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls between two entity-bound date selectors (inclusive, full DateTime comparison).</summary>
    public ValiFlowQuery<T> BetweenDates(Expression<Func<T, DateTime>> selector,
        Expression<Func<T, DateTime>> startDateSelector, Expression<Func<T, DateTime>> endDateSelector)
        => _dateTime.BetweenDates(selector, startDateSelector, endDateSelector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the same calendar month and year as <paramref name="date"/>.</summary>
    public ValiFlowQuery<T> SameMonthAs(Expression<Func<T, DateTime>> selector, DateTime date) => _dateTime.SameMonthAs(selector, date);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    public ValiFlowQuery<T> SameYearAs(Expression<Func<T, DateTime>> selector, DateTime date) => _dateTime.SameYearAs(selector, date);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the specified calendar month (1–12), regardless of year.</summary>
    public ValiFlowQuery<T> IsInMonth(Expression<Func<T, DateTime>> selector, int month) => _dateTime.IsInMonth(selector, month);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the specified calendar year.</summary>
    public ValiFlowQuery<T> IsInYear(Expression<Func<T, DateTime>> selector, int year) => _dateTime.IsInYear(selector, year);

    /// <summary>Validates that the selected <see cref="DateTime"/> is in the future (greater than <see cref="DateTime.UtcNow"/>).</summary>
    public ValiFlowQuery<T> FutureDate(Expression<Func<T, DateTime>> selector) => _dateTime.FutureDate(selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> is in the past (less than <see cref="DateTime.UtcNow"/>).</summary>
    public ValiFlowQuery<T> PastDate(Expression<Func<T, DateTime>> selector) => _dateTime.PastDate(selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on a Saturday or Sunday.</summary>
    public ValiFlowQuery<T> IsWeekend(Expression<Func<T, DateTime>> selector) => _dateTime.IsWeekend(selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on a weekday (Monday–Friday).</summary>
    public ValiFlowQuery<T> IsWeekday(Expression<Func<T, DateTime>> selector) => _dateTime.IsWeekday(selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on the specified <paramref name="day"/> of the week.</summary>
    public ValiFlowQuery<T> IsDayOfWeek(Expression<Func<T, DateTime>> selector, DayOfWeek day) => _dateTime.IsDayOfWeek(selector, day);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on today's UTC date.</summary>
    public ValiFlowQuery<T> IsToday(Expression<Func<T, DateTime>> selector) => _dateTime.IsToday(selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on yesterday's UTC date.</summary>
    public ValiFlowQuery<T> IsYesterday(Expression<Func<T, DateTime>> selector) => _dateTime.IsYesterday(selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on tomorrow's UTC date.</summary>
    public ValiFlowQuery<T> IsTomorrow(Expression<Func<T, DateTime>> selector) => _dateTime.IsTomorrow(selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls within the last <paramref name="days"/> UTC calendar days (today excluded).</summary>
    public ValiFlowQuery<T> InLastDays(Expression<Func<T, DateTime>> selector, int days) => _dateTime.InLastDays(selector, days);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls within the next <paramref name="days"/> UTC calendar days (today excluded).</summary>
    public ValiFlowQuery<T> InNextDays(Expression<Func<T, DateTime>> selector, int days) => _dateTime.InNextDays(selector, days);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on the first day of its month.</summary>
    public ValiFlowQuery<T> IsFirstDayOfMonth(Expression<Func<T, DateTime>> selector) => _dateTime.IsFirstDayOfMonth(selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on the last day of its month.</summary>
    public ValiFlowQuery<T> IsLastDayOfMonth(Expression<Func<T, DateTime>> selector) => _dateTime.IsLastDayOfMonth(selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the specified calendar quarter (1–4).</summary>
    public ValiFlowQuery<T> IsInQuarter(Expression<Func<T, DateTime>> selector, int quarter) => _dateTime.IsInQuarter(selector, quarter);

    // ── DateTimeOffset ────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is in the future.</summary>
    public ValiFlowQuery<T> FutureDate(Expression<Func<T, DateTimeOffset>> selector) => _dateTimeOffset.FutureDate(selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is in the past.</summary>
    public ValiFlowQuery<T> PastDate(Expression<Func<T, DateTimeOffset>> selector) => _dateTimeOffset.PastDate(selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is before <paramref name="date"/>.</summary>
    public ValiFlowQuery<T> IsBefore(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date) => _dateTimeOffset.IsBefore(selector, date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is after <paramref name="date"/>.</summary>
    public ValiFlowQuery<T> IsAfter(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date) => _dateTimeOffset.IsAfter(selector, date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the inclusive range [<paramref name="from"/>, <paramref name="to"/>].</summary>
    public ValiFlowQuery<T> BetweenDates(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset from, DateTimeOffset to)
        => _dateTimeOffset.BetweenDates(selector, from, to);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified month.</summary>
    public ValiFlowQuery<T> IsInMonth(Expression<Func<T, DateTimeOffset>> selector, int month) => _dateTimeOffset.IsInMonth(selector, month);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified year.</summary>
    public ValiFlowQuery<T> IsInYear(Expression<Func<T, DateTimeOffset>> selector, int year) => _dateTimeOffset.IsInYear(selector, year);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on today's UTC calendar date.</summary>
    public ValiFlowQuery<T> IsToday(Expression<Func<T, DateTimeOffset>> selector) => _dateTimeOffset.IsToday(selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on yesterday's UTC calendar date.</summary>
    public ValiFlowQuery<T> IsYesterday(Expression<Func<T, DateTimeOffset>> selector) => _dateTimeOffset.IsYesterday(selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on tomorrow's UTC calendar date.</summary>
    public ValiFlowQuery<T> IsTomorrow(Expression<Func<T, DateTimeOffset>> selector) => _dateTimeOffset.IsTomorrow(selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the same UTC calendar date as <paramref name="date"/>.</summary>
    public ValiFlowQuery<T> ExactDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date) => _dateTimeOffset.ExactDate(selector, date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is before the UTC calendar date of <paramref name="date"/> (date-only comparison).</summary>
    [Obsolete("Use IsBefore instead. BeforeDate will be removed in a future version.")]
    public ValiFlowQuery<T> BeforeDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date) => _dateTimeOffset.BeforeDate(selector, date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is after the UTC calendar date of <paramref name="date"/> (date-only comparison).</summary>
    [Obsolete("Use IsAfter instead. AfterDate will be removed in a future version.")]
    public ValiFlowQuery<T> AfterDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date) => _dateTimeOffset.AfterDate(selector, date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the same calendar year and month as <paramref name="date"/>.</summary>
    public ValiFlowQuery<T> SameMonthAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date) => _dateTimeOffset.SameMonthAs(selector, date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    public ValiFlowQuery<T> SameYearAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date) => _dateTimeOffset.SameYearAs(selector, date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the last <paramref name="days"/> UTC calendar days (today excluded).</summary>
    public ValiFlowQuery<T> InLastDays(Expression<Func<T, DateTimeOffset>> selector, int days) => _dateTimeOffset.InLastDays(selector, days);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the next <paramref name="days"/> UTC calendar days (today excluded).</summary>
    public ValiFlowQuery<T> InNextDays(Expression<Func<T, DateTimeOffset>> selector, int days) => _dateTimeOffset.InNextDays(selector, days);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on a Saturday or Sunday.</summary>
    public ValiFlowQuery<T> IsWeekend(Expression<Func<T, DateTimeOffset>> selector) => _dateTimeOffset.IsWeekend(selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on a weekday (Monday–Friday).</summary>
    public ValiFlowQuery<T> IsWeekday(Expression<Func<T, DateTimeOffset>> selector) => _dateTimeOffset.IsWeekday(selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the specified <paramref name="day"/> of the week.</summary>
    public ValiFlowQuery<T> IsDayOfWeek(Expression<Func<T, DateTimeOffset>> selector, DayOfWeek day) => _dateTimeOffset.IsDayOfWeek(selector, day);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the first day of its month.</summary>
    public ValiFlowQuery<T> IsFirstDayOfMonth(Expression<Func<T, DateTimeOffset>> selector) => _dateTimeOffset.IsFirstDayOfMonth(selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the last day of its month.</summary>
    public ValiFlowQuery<T> IsLastDayOfMonth(Expression<Func<T, DateTimeOffset>> selector) => _dateTimeOffset.IsLastDayOfMonth(selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified calendar quarter (1–4).</summary>
    public ValiFlowQuery<T> IsInQuarter(Expression<Func<T, DateTimeOffset>> selector, int quarter) => _dateTimeOffset.IsInQuarter(selector, quarter);

    // ── DateOnly ──────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="DateOnly"/> is before <paramref name="date"/>.</summary>
    public ValiFlowQuery<T> IsBefore(Expression<Func<T, DateOnly>> selector, DateOnly date) => _dateOnly.IsBefore(selector, date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is after <paramref name="date"/>.</summary>
    public ValiFlowQuery<T> IsAfter(Expression<Func<T, DateOnly>> selector, DateOnly date) => _dateOnly.IsAfter(selector, date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is within the inclusive range [<paramref name="from"/>, <paramref name="to"/>].</summary>
    public ValiFlowQuery<T> BetweenDates(Expression<Func<T, DateOnly>> selector, DateOnly from, DateOnly to) => _dateOnly.BetweenDates(selector, from, to);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the specified calendar month (1–12), regardless of year.</summary>
    public ValiFlowQuery<T> IsInMonth(Expression<Func<T, DateOnly>> selector, int month) => _dateOnly.IsInMonth(selector, month);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the specified calendar year.</summary>
    public ValiFlowQuery<T> IsInYear(Expression<Func<T, DateOnly>> selector, int year) => _dateOnly.IsInYear(selector, year);

    /// <summary>Validates that the selected date is the first day of its month.</summary>
    public ValiFlowQuery<T> IsFirstDayOfMonth(Expression<Func<T, DateOnly>> selector) => _dateOnly.IsFirstDayOfMonth(selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is the last day of its month.</summary>
    public ValiFlowQuery<T> IsLastDayOfMonth(Expression<Func<T, DateOnly>> selector) => _dateOnly.IsLastDayOfMonth(selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the future (after today's UTC date).</summary>
    public ValiFlowQuery<T> FutureDate(Expression<Func<T, DateOnly>> selector) => _dateOnly.FutureDate(selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the past (before today's UTC date).</summary>
    public ValiFlowQuery<T> PastDate(Expression<Func<T, DateOnly>> selector) => _dateOnly.PastDate(selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals today's UTC date.</summary>
    public ValiFlowQuery<T> IsToday(Expression<Func<T, DateOnly>> selector) => _dateOnly.IsToday(selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals <paramref name="date"/>.</summary>
    public ValiFlowQuery<T> ExactDate(Expression<Func<T, DateOnly>> selector, DateOnly date) => _dateOnly.ExactDate(selector, date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly before <paramref name="date"/>.</summary>
    [Obsolete("Use IsBefore instead. BeforeDate will be removed in a future version.")]
    public ValiFlowQuery<T> BeforeDate(Expression<Func<T, DateOnly>> selector, DateOnly date) => _dateOnly.BeforeDate(selector, date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly after <paramref name="date"/>.</summary>
    [Obsolete("Use IsAfter instead. AfterDate will be removed in a future version.")]
    public ValiFlowQuery<T> AfterDate(Expression<Func<T, DateOnly>> selector, DateOnly date) => _dateOnly.AfterDate(selector, date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals yesterday's UTC date.</summary>
    public ValiFlowQuery<T> IsYesterday(Expression<Func<T, DateOnly>> selector) => _dateOnly.IsYesterday(selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals tomorrow's UTC date.</summary>
    public ValiFlowQuery<T> IsTomorrow(Expression<Func<T, DateOnly>> selector) => _dateOnly.IsTomorrow(selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the last <paramref name="days"/> UTC days (today excluded).</summary>
    public ValiFlowQuery<T> InLastDays(Expression<Func<T, DateOnly>> selector, int days) => _dateOnly.InLastDays(selector, days);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the next <paramref name="days"/> UTC days (today excluded).</summary>
    public ValiFlowQuery<T> InNextDays(Expression<Func<T, DateOnly>> selector, int days) => _dateOnly.InNextDays(selector, days);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the same calendar year and month as <paramref name="date"/>.</summary>
    public ValiFlowQuery<T> SameMonthAs(Expression<Func<T, DateOnly>> selector, DateOnly date) => _dateOnly.SameMonthAs(selector, date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    public ValiFlowQuery<T> SameYearAs(Expression<Func<T, DateOnly>> selector, DateOnly date) => _dateOnly.SameYearAs(selector, date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on a Saturday or Sunday.</summary>
    public ValiFlowQuery<T> IsWeekend(Expression<Func<T, DateOnly>> selector) => _dateOnly.IsWeekend(selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on a weekday (Monday–Friday).</summary>
    public ValiFlowQuery<T> IsWeekday(Expression<Func<T, DateOnly>> selector) => _dateOnly.IsWeekday(selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on the specified <paramref name="dayOfWeek"/>.</summary>
    public ValiFlowQuery<T> IsDayOfWeek(Expression<Func<T, DateOnly>> selector, DayOfWeek dayOfWeek) => _dateOnly.IsDayOfWeek(selector, dayOfWeek);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the specified calendar quarter (1–4).</summary>
    public ValiFlowQuery<T> IsInQuarter(Expression<Func<T, DateOnly>> selector, int quarter) => _dateOnly.IsInQuarter(selector, quarter);

    // ── DateOnly cross-property BetweenDates ──────────────────────────────────

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls between two entity-bound date selectors (inclusive).</summary>
    public ValiFlowQuery<T> BetweenDates(Expression<Func<T, DateOnly>> selector,
        Expression<Func<T, DateOnly>> fromSelector, Expression<Func<T, DateOnly>> toSelector)
        => _dateOnly.BetweenDates(selector, fromSelector, toSelector);

    // ── TimeOnly ──────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is before <paramref name="time"/>.</summary>
    public ValiFlowQuery<T> IsBefore(Expression<Func<T, TimeOnly>> selector, TimeOnly time) => _timeOnly.IsBefore(selector, time);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is after <paramref name="time"/>.</summary>
    public ValiFlowQuery<T> IsAfter(Expression<Func<T, TimeOnly>> selector, TimeOnly time) => _timeOnly.IsAfter(selector, time);

    /// <summary>Validates that the selected time falls within [<paramref name="from"/>, <paramref name="to"/>] (inclusive).</summary>
    public ValiFlowQuery<T> IsBetween(Expression<Func<T, TimeOnly>> selector, TimeOnly from, TimeOnly to) => _timeOnly.IsBetween(selector, from, to);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is in the AM (hour &lt; 12).</summary>
    public ValiFlowQuery<T> IsAM(Expression<Func<T, TimeOnly>> selector) => _timeOnly.IsAM(selector);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is in the PM (hour &gt;= 12).</summary>
    public ValiFlowQuery<T> IsPM(Expression<Func<T, TimeOnly>> selector) => _timeOnly.IsPM(selector);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> equals <paramref name="time"/> exactly (to the tick).</summary>
    public ValiFlowQuery<T> IsExactTime(Expression<Func<T, TimeOnly>> selector, TimeOnly time) => _timeOnly.IsExactTime(selector, time);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> falls within the specified hour (0–23).</summary>
    public ValiFlowQuery<T> IsInHour(Expression<Func<T, TimeOnly>> selector, int hour) => _timeOnly.IsInHour(selector, hour);

    // ── Nested validation ─────────────────────────────────────────────────────

    /// <summary>
    /// Validates that the navigation property selected by <paramref name="selector"/> is not null
    /// and satisfies all conditions configured in <paramref name="configure"/> using an inner
    /// <see cref="ValiFlowQuery{TProperty}"/> builder that only exposes EF Core-safe methods.
    /// </summary>
    public ValiFlowQuery<T> ValidateNested<TProperty>(
        Expression<Func<T, TProperty>> selector,
        Action<ValiFlowQuery<TProperty>> configure)
        where TProperty : class
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(configure);
        var nestedBuilder = new ValiFlowQuery<TProperty>();
        configure(nestedBuilder);
        var nestedExpr = nestedBuilder.Build();
        if (nestedExpr.Body is ConstantExpression { Value: true })
            throw new ArgumentException("The configure action must add at least one condition.", nameof(configure));
        return Add(BuildNestedExpression(selector, nestedExpr));
    }

    // ── WithError / WithSeverity overloads ────────────────────────────────────

    public new ValiFlowQuery<T> WithError(string errorCode, string message, string propertyPath)
        => (ValiFlowQuery<T>)base.WithError(errorCode, message, propertyPath);

    public new ValiFlowQuery<T> WithError(string errorCode, string message, Severity severity)
        => (ValiFlowQuery<T>)base.WithError(errorCode, message, severity);

    public new ValiFlowQuery<T> WithError(string errorCode, string message, string propertyPath, Severity severity)
        => (ValiFlowQuery<T>)base.WithError(errorCode, message, propertyPath, severity);

    public new ValiFlowQuery<T> WithSeverity(Severity severity)
        => (ValiFlowQuery<T>)base.WithSeverity(severity);

    // ── Builder combining ─────────────────────────────────────────────────────

    public static Expression<Func<T, bool>> Combine(ValiFlowQuery<T> left, ValiFlowQuery<T> right, bool and = true)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return CombineExpressions(left.Build(), right.Build(), and);
    }

    public static Expression<Func<T, bool>> operator &(ValiFlowQuery<T> left, ValiFlowQuery<T> right)
        => Combine(left, right, and: true);

    public static Expression<Func<T, bool>> operator |(ValiFlowQuery<T> left, ValiFlowQuery<T> right)
        => Combine(left, right, and: false);

    public static Expression<Func<T, bool>> operator !(ValiFlowQuery<T> flow)
    {
        ArgumentNullException.ThrowIfNull(flow);
        return flow.BuildNegated();
    }
}
