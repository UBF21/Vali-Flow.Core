using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Classes.General;
using Vali_Flow.Core.Classes.Types;
using Vali_Flow.Core.Interfaces.General;
using Vali_Flow.Core.Interfaces.Types;
using Vali_Flow.Core.Models;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Builder;

/// <summary>
/// The main public fluent builder for constructing <see cref="Expression{TDelegate}"/> trees
/// for in-memory validation and general LINQ expression use.
/// All condition types (string, numeric, collection, DateTime, etc.) are available.
/// For EF Core <c>IQueryable</c> use, prefer <see cref="ValiFlowQuery{T}"/> which only
/// exposes provider-translatable predicates.
/// </summary>
/// <example>
/// <code>
/// var rule = new ValiFlow&lt;Order&gt;()
///     .NotNull(o => o.CustomerId)
///     .GreaterThan(o => o.Total, 0m);
///
/// bool ok  = rule.IsValid(order);
/// var expr = rule.Build(); // Expression&lt;Func&lt;Order, bool&gt;&gt;
/// </code>
/// </example>
/// <typeparam name="T">The entity type being validated or filtered.</typeparam>
public class ValiFlow<T> : BaseExpression<ValiFlow<T>, T>,
    IBooleanExpression<ValiFlow<T>, T>, IComparisonExpression<ValiFlow<T>, T>,
    ICollectionExpression<ValiFlow<T>, T>, IStringExpression<ValiFlow<T>, T>,
    INumericExpression<ValiFlow<T>, T>, IDateTimeExpression<ValiFlow<T>, T>,
    IDateTimeOffsetExpression<ValiFlow<T>, T>, IDateOnlyExpression<ValiFlow<T>, T>,
    ITimeOnlyExpression<ValiFlow<T>, T>
{
    private readonly IBooleanExpression<ValiFlow<T>, T> _booleanExpression;
    private readonly ICollectionExpression<ValiFlow<T>, T> _collectionExpression;
    private readonly IComparisonExpression<ValiFlow<T>, T> _comparisonExpression;
    private readonly IStringExpression<ValiFlow<T>, T> _stringExpression;
    private readonly INumericExpression<ValiFlow<T>, T> _numericExpression;
    private readonly IDateTimeExpression<ValiFlow<T>, T> _dateTimeExpression;
    private readonly IDateTimeOffsetExpression<ValiFlow<T>, T> _dateTimeOffsetExpression;
    private readonly IDateOnlyExpression<ValiFlow<T>, T> _dateOnlyExpression;
    private readonly ITimeOnlyExpression<ValiFlow<T>, T> _timeOnlyExpression;

    public ValiFlow()
    {
        _booleanExpression = new BooleanExpression<ValiFlow<T>, T>(this);
        _collectionExpression = new CollectionExpression<ValiFlow<T>, T>(this);
        _comparisonExpression = new ComparisonExpression<ValiFlow<T>, T>(this);
        _stringExpression = new StringExpression<ValiFlow<T>, T>(this);
        _numericExpression = new NumericExpression<ValiFlow<T>, T>(this);
        _dateTimeExpression = new DateTimeExpression<ValiFlow<T>, T>(this);
        _dateTimeOffsetExpression = new DateTimeOffsetExpression<ValiFlow<T>, T>(this);
        _dateOnlyExpression = new DateOnlyExpression<ValiFlow<T>, T>(this);
        _timeOnlyExpression = new TimeOnlyExpression<ValiFlow<T>, T>(this);
    }

    public ValiFlow<T> IsTrue(Expression<Func<T, bool>> selector) => _booleanExpression.IsTrue(selector);

    public ValiFlow<T> IsFalse(Expression<Func<T, bool>> selector) => _booleanExpression.IsFalse(selector);

    public ValiFlow<T> NotNull<TProperty>(Expression<Func<T, TProperty?>> selector) =>
        _comparisonExpression.NotNull(selector);

    public ValiFlow<T> Null<TProperty>(Expression<Func<T, TProperty?>> selector) =>
        _comparisonExpression.Null(selector);

    public ValiFlow<T> EqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IEquatable<TValue> => _comparisonExpression.EqualTo(selector, value);

    public ValiFlow<T> NotEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IEquatable<TValue> => _comparisonExpression.NotEqualTo(selector, value);

    public ValiFlow<T> IsInEnum<TEnum>(Expression<Func<T, TEnum>> selector) where TEnum : struct, Enum
        => _comparisonExpression.IsInEnum(selector);

    public ValiFlow<T> IsDefault<TValue>(Expression<Func<T, TValue>> selector)
        => _comparisonExpression.IsDefault(selector);

    public ValiFlow<T> IsNotDefault<TValue>(Expression<Func<T, TValue>> selector)
        => _comparisonExpression.IsNotDefault(selector);

    public ValiFlow<T> IsNull<TValue>(Expression<Func<T, TValue?>> selector)
        => _comparisonExpression.IsNull(selector);

    public ValiFlow<T> IsNotNull<TValue>(Expression<Func<T, TValue?>> selector)
        => _comparisonExpression.IsNotNull(selector);

    public ValiFlow<T> NotEmpty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector) =>
        _collectionExpression.NotEmpty(selector);

    public ValiFlow<T> In<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values)
        => _collectionExpression.In(selector, values);

    public ValiFlow<T> NotIn<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values)
        => _collectionExpression.NotIn(selector, values);

    public ValiFlow<T> Count<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int count)
        => _collectionExpression.Count(selector, count);

    public ValiFlow<T> CountBetween<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector, int min, int max)
        => _collectionExpression.CountBetween(selector, min, max);

    public ValiFlow<T> All<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector,
        Expression<Func<TValue, bool>> predicate)
        => _collectionExpression.All(selector, predicate);

    public ValiFlow<T> Any<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector,
        Expression<Func<TValue, bool>> predicate)
        => _collectionExpression.Any(selector, predicate);

    public ValiFlow<T> Contains<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector, TValue value)
        => _collectionExpression.Contains(selector, value);

    public ValiFlow<T> DistinctCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int count)
        => _collectionExpression.DistinctCount(selector, count);

    public ValiFlow<T> None<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector,
        Expression<Func<TValue, bool>> predicate)
        => _collectionExpression.None(selector, predicate);

    public ValiFlow<T> MinLength(Expression<Func<T, string?>> selector, int minLength)
        => _stringExpression.MinLength(selector, minLength);

    public ValiFlow<T> MaxLength(Expression<Func<T, string?>> selector, int maxLength)
        => _stringExpression.MaxLength(selector, maxLength);

    public ValiFlow<T> RegexMatch(Expression<Func<T, string?>> selector, string pattern)
        => _stringExpression.RegexMatch(selector, pattern);

    public ValiFlow<T> IsNullOrEmpty(Expression<Func<T, string?>> selector)
        => _stringExpression.IsNullOrEmpty(selector);

    public ValiFlow<T> IsNotNullOrEmpty(Expression<Func<T, string?>> selector)
        => _stringExpression.IsNotNullOrEmpty(selector);

    public ValiFlow<T> IsEmail(Expression<Func<T, string?>> selector)
        => _stringExpression.IsEmail(selector);

    public ValiFlow<T> EndsWith(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        => _stringExpression.EndsWith(selector, value, comparison);

    public ValiFlow<T> StartsWith(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        => _stringExpression.StartsWith(selector, value, comparison);

    public ValiFlow<T> Contains(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        => _stringExpression.Contains(selector, value, comparison);

    public ValiFlow<T> ExactLength(Expression<Func<T, string?>> selector, int length)
        => _stringExpression.ExactLength(selector, length);

    public ValiFlow<T> EqualsIgnoreCase(Expression<Func<T, string?>> selector, string? value)
        => _stringExpression.EqualsIgnoreCase(selector, value);

    public ValiFlow<T> IsTrimmed(Expression<Func<T, string?>> selector)
        => _stringExpression.IsTrimmed(selector);

    public ValiFlow<T> HasOnlyDigits(Expression<Func<T, string?>> selector)
        => _stringExpression.HasOnlyDigits(selector);

    public ValiFlow<T> HasOnlyLetters(Expression<Func<T, string?>> selector)
        => _stringExpression.HasOnlyLetters(selector);

    public ValiFlow<T> HasLettersAndNumbers(Expression<Func<T, string?>> selector)
        => _stringExpression.HasLettersAndNumbers(selector);

    public ValiFlow<T> HasSpecialCharacters(Expression<Func<T, string?>> selector)
        => _stringExpression.HasSpecialCharacters(selector);

    public ValiFlow<T> IsJson(Expression<Func<T, string?>> selector)
        => _stringExpression.IsJson(selector);

    public ValiFlow<T> IsBase64(Expression<Func<T, string?>> selector)
        => _stringExpression.IsBase64(selector);

    public ValiFlow<T> NotJson(Expression<Func<T, string?>> selector)
        => _stringExpression.NotJson(selector);

    public ValiFlow<T> NotBase64(Expression<Func<T, string?>> selector)
        => _stringExpression.NotBase64(selector);

    public ValiFlow<T> Contains(string value, List<Expression<Func<T, string?>>> selectors,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        => _stringExpression.Contains(value, selectors, comparison);

    public ValiFlow<T> IsUrl(Expression<Func<T, string?>> selector)
        => _stringExpression.IsUrl(selector);

    public ValiFlow<T> IsPhoneNumber(Expression<Func<T, string?>> selector)
        => _stringExpression.IsPhoneNumber(selector);

    public ValiFlow<T> IsGuid(Expression<Func<T, string?>> selector)
        => _stringExpression.IsGuid(selector);

    public ValiFlow<T> IsUpperCase(Expression<Func<T, string?>> selector)
        => _stringExpression.IsUpperCase(selector);

    public ValiFlow<T> IsLowerCase(Expression<Func<T, string?>> selector)
        => _stringExpression.IsLowerCase(selector);

    public ValiFlow<T> LengthBetween(Expression<Func<T, string?>> selector, int min, int max)
        => _stringExpression.LengthBetween(selector, min, max);

    public ValiFlow<T> IsNullOrWhiteSpace(Expression<Func<T, string?>> selector)
        => _stringExpression.IsNullOrWhiteSpace(selector);

    public ValiFlow<T> IsNotNullOrWhiteSpace(Expression<Func<T, string?>> selector)
        => _stringExpression.IsNotNullOrWhiteSpace(selector);

    public ValiFlow<T> IsCreditCard(Expression<Func<T, string?>> selector) => _stringExpression.IsCreditCard(selector);
    public ValiFlow<T> IsIPv4(Expression<Func<T, string?>> selector) => _stringExpression.IsIPv4(selector);
    public ValiFlow<T> IsIPv6(Expression<Func<T, string?>> selector) => _stringExpression.IsIPv6(selector);
    public ValiFlow<T> IsHexColor(Expression<Func<T, string?>> selector) => _stringExpression.IsHexColor(selector);
    public ValiFlow<T> IsSlug(Expression<Func<T, string?>> selector) => _stringExpression.IsSlug(selector);
    public ValiFlow<T> MatchesWildcard(Expression<Func<T, string?>> selector, string pattern) => _stringExpression.MatchesWildcard(selector, pattern);
    public ValiFlow<T> IsOneOf(Expression<Func<T, string?>> selector, IReadOnlyCollection<string> values, StringComparison comparison = StringComparison.OrdinalIgnoreCase) => _stringExpression.IsOneOf(selector, values, comparison);

    public ValiFlow<T> Zero(Expression<Func<T, int>> selector)
        => _numericExpression.Zero(selector);

    public ValiFlow<T> Zero(Expression<Func<T, long>> selector)
        => _numericExpression.Zero(selector);

    public ValiFlow<T> Zero(Expression<Func<T, float>> selector)
        => _numericExpression.Zero(selector);

    public ValiFlow<T> Zero(Expression<Func<T, double>> selector)
        => _numericExpression.Zero(selector);

    public ValiFlow<T> Zero(Expression<Func<T, decimal>> selector)
        => _numericExpression.Zero(selector);

    public ValiFlow<T> Zero(Expression<Func<T, short>> selector)
        => _numericExpression.Zero(selector);

    public ValiFlow<T> NotZero(Expression<Func<T, int>> selector)
        => _numericExpression.NotZero(selector);

    public ValiFlow<T> NotZero(Expression<Func<T, long>> selector)
        => _numericExpression.NotZero(selector);

    public ValiFlow<T> NotZero(Expression<Func<T, float>> selector)
        => _numericExpression.NotZero(selector);

    public ValiFlow<T> NotZero(Expression<Func<T, double>> selector)
        => _numericExpression.NotZero(selector);

    public ValiFlow<T> NotZero(Expression<Func<T, decimal>> selector)
        => _numericExpression.NotZero(selector);

    public ValiFlow<T> NotZero(Expression<Func<T, short>> selector)
        => _numericExpression.NotZero(selector);

    public ValiFlow<T> InRange(Expression<Func<T, int>> selector, int min, int max)
        => _numericExpression.InRange(selector, min, max);

    public ValiFlow<T> InRange(Expression<Func<T, long>> selector, long min, long max)
        => _numericExpression.InRange(selector, min, max);

    public ValiFlow<T> InRange(Expression<Func<T, float>> selector, float min, float max)
        => _numericExpression.InRange(selector, min, max);

    public ValiFlow<T> InRange(Expression<Func<T, double>> selector, double min, double max)
        => _numericExpression.InRange(selector, min, max);

    public ValiFlow<T> InRange(Expression<Func<T, decimal>> selector, decimal min, decimal max)
        => _numericExpression.InRange(selector, min, max);

    public ValiFlow<T> InRange(Expression<Func<T, short>> selector, short min, short max)
        => _numericExpression.InRange(selector, min, max);

    public ValiFlow<T> InRange(Expression<Func<T, int>> selector, Expression<Func<T, int>> minSelector,
        Expression<Func<T, int>> maxSelector)
        => _numericExpression.InRange(selector, minSelector, maxSelector);

    public ValiFlow<T> InRange(Expression<Func<T, long>> selector, Expression<Func<T, long>> minSelector,
        Expression<Func<T, long>> maxSelector)
        => _numericExpression.InRange(selector, minSelector, maxSelector);

    public ValiFlow<T> InRange(Expression<Func<T, float>> selector, Expression<Func<T, float>> minSelector,
        Expression<Func<T, float>> maxSelector)
        => _numericExpression.InRange(selector, minSelector, maxSelector);

    public ValiFlow<T> InRange(Expression<Func<T, double>> selector, Expression<Func<T, double>> minSelector,
        Expression<Func<T, double>> maxSelector)
        => _numericExpression.InRange(selector, minSelector, maxSelector);

    public ValiFlow<T> InRange(Expression<Func<T, decimal>> selector, Expression<Func<T, decimal>> minSelector,
        Expression<Func<T, decimal>> maxSelector)
        => _numericExpression.InRange(selector, minSelector, maxSelector);

    public ValiFlow<T> InRange(Expression<Func<T, short>> selector, Expression<Func<T, short>> minSelector,
        Expression<Func<T, short>> maxSelector)
        => _numericExpression.InRange(selector, minSelector, maxSelector);

    public ValiFlow<T> GreaterThan(Expression<Func<T, int>> selector, int value)
        => _numericExpression.GreaterThan(selector, value);

    public ValiFlow<T> GreaterThan(Expression<Func<T, long>> selector, long value)
        => _numericExpression.GreaterThan(selector, value);

    public ValiFlow<T> GreaterThan(Expression<Func<T, float>> selector, float value)
        => _numericExpression.GreaterThan(selector, value);

    public ValiFlow<T> GreaterThan(Expression<Func<T, double>> selector, double value)
        => _numericExpression.GreaterThan(selector, value);

    public ValiFlow<T> GreaterThan(Expression<Func<T, decimal>> selector, decimal value)
        => _numericExpression.GreaterThan(selector, value);

    public ValiFlow<T> GreaterThan(Expression<Func<T, short>> selector, short value)
        => _numericExpression.GreaterThan(selector, value);

    public ValiFlow<T> GreaterThanOrEqualTo(Expression<Func<T, int>> selector, int value)
        => _numericExpression.GreaterThanOrEqualTo(selector, value);

    public ValiFlow<T> GreaterThanOrEqualTo(Expression<Func<T, long>> selector, long value)
        => _numericExpression.GreaterThanOrEqualTo(selector, value);

    public ValiFlow<T> GreaterThanOrEqualTo(Expression<Func<T, float>> selector, float value)
        => _numericExpression.GreaterThanOrEqualTo(selector, value);

    public ValiFlow<T> GreaterThanOrEqualTo(Expression<Func<T, double>> selector, double value)
        => _numericExpression.GreaterThanOrEqualTo(selector, value);

    public ValiFlow<T> GreaterThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value)
        => _numericExpression.GreaterThanOrEqualTo(selector, value);

    public ValiFlow<T> GreaterThanOrEqualTo(Expression<Func<T, short>> selector, short value)
        => _numericExpression.GreaterThanOrEqualTo(selector, value);

    public ValiFlow<T> LessThan(Expression<Func<T, int>> selector, int value)
        => _numericExpression.LessThan(selector, value);

    public ValiFlow<T> LessThan(Expression<Func<T, long>> selector, long value)
        => _numericExpression.LessThan(selector, value);

    public ValiFlow<T> LessThan(Expression<Func<T, float>> selector, float value)
        => _numericExpression.LessThan(selector, value);

    public ValiFlow<T> LessThan(Expression<Func<T, double>> selector, double value)
        => _numericExpression.LessThan(selector, value);

    public ValiFlow<T> LessThan(Expression<Func<T, decimal>> selector, decimal value)
        => _numericExpression.LessThan(selector, value);

    public ValiFlow<T> LessThan(Expression<Func<T, short>> selector, short value)
        => _numericExpression.LessThan(selector, value);

    public ValiFlow<T> LessThanOrEqualTo(Expression<Func<T, int>> selector, int value)
        => _numericExpression.LessThanOrEqualTo(selector, value);

    public ValiFlow<T> LessThanOrEqualTo(Expression<Func<T, long>> selector, long value)
        => _numericExpression.LessThanOrEqualTo(selector, value);

    public ValiFlow<T> LessThanOrEqualTo(Expression<Func<T, float>> selector, float value)
        => _numericExpression.LessThanOrEqualTo(selector, value);

    public ValiFlow<T> LessThanOrEqualTo(Expression<Func<T, double>> selector, double value)
        => _numericExpression.LessThanOrEqualTo(selector, value);

    public ValiFlow<T> LessThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value)
        => _numericExpression.LessThanOrEqualTo(selector, value);

    public ValiFlow<T> LessThanOrEqualTo(Expression<Func<T, short>> selector, short value)
        => _numericExpression.LessThanOrEqualTo(selector, value);

    public ValiFlow<T> Positive(Expression<Func<T, int>> selector)
        => _numericExpression.Positive(selector);

    public ValiFlow<T> Positive(Expression<Func<T, long>> selector)
        => _numericExpression.Positive(selector);

    public ValiFlow<T> Positive(Expression<Func<T, float>> selector)
        => _numericExpression.Positive(selector);

    public ValiFlow<T> Positive(Expression<Func<T, double>> selector)
        => _numericExpression.Positive(selector);

    public ValiFlow<T> Positive(Expression<Func<T, decimal>> selector)
        => _numericExpression.Positive(selector);

    public ValiFlow<T> Positive(Expression<Func<T, short>> selector)
        => _numericExpression.Positive(selector);

    public ValiFlow<T> Negative(Expression<Func<T, int>> selector)
        => _numericExpression.Negative(selector);

    public ValiFlow<T> Negative(Expression<Func<T, long>> selector)
        => _numericExpression.Negative(selector);

    public ValiFlow<T> Negative(Expression<Func<T, float>> selector)
        => _numericExpression.Negative(selector);

    public ValiFlow<T> Negative(Expression<Func<T, double>> selector)
        => _numericExpression.Negative(selector);

    public ValiFlow<T> Negative(Expression<Func<T, decimal>> selector)
        => _numericExpression.Negative(selector);

    public ValiFlow<T> Negative(Expression<Func<T, short>> selector)
        => _numericExpression.Negative(selector);

    public ValiFlow<T> MinValue(Expression<Func<T, int>> selector, int minValue)
        => _numericExpression.MinValue(selector, minValue);

    public ValiFlow<T> MinValue(Expression<Func<T, long>> selector, long minValue)
        => _numericExpression.MinValue(selector, minValue);

    public ValiFlow<T> MinValue(Expression<Func<T, float>> selector, float minValue)
        => _numericExpression.MinValue(selector, minValue);

    public ValiFlow<T> MinValue(Expression<Func<T, double>> selector, double minValue)
        => _numericExpression.MinValue(selector, minValue);

    public ValiFlow<T> MinValue(Expression<Func<T, decimal>> selector, decimal minValue)
        => _numericExpression.MinValue(selector, minValue);

    public ValiFlow<T> MinValue(Expression<Func<T, short>> selector, short minValue)
        => _numericExpression.MinValue(selector, minValue);

    public ValiFlow<T> MaxValue(Expression<Func<T, int>> selector, int maxValue)
        => _numericExpression.MaxValue(selector, maxValue);

    public ValiFlow<T> MaxValue(Expression<Func<T, long>> selector, long maxValue)
        => _numericExpression.MaxValue(selector, maxValue);

    public ValiFlow<T> MaxValue(Expression<Func<T, float>> selector, float maxValue)
        => _numericExpression.MaxValue(selector, maxValue);

    public ValiFlow<T> MaxValue(Expression<Func<T, double>> selector, double maxValue)
        => _numericExpression.MaxValue(selector, maxValue);

    public ValiFlow<T> MaxValue(Expression<Func<T, decimal>> selector, decimal maxValue)
        => _numericExpression.MaxValue(selector, maxValue);

    public ValiFlow<T> MaxValue(Expression<Func<T, short>> selector, short maxValue)
        => _numericExpression.MaxValue(selector, maxValue);

    public ValiFlow<T> IsEven(Expression<Func<T, int>> selector)
        => _numericExpression.IsEven(selector);

    public ValiFlow<T> IsEven(Expression<Func<T, long>> selector)
        => _numericExpression.IsEven(selector);

    public ValiFlow<T> IsOdd(Expression<Func<T, int>> selector)
        => _numericExpression.IsOdd(selector);

    public ValiFlow<T> IsOdd(Expression<Func<T, long>> selector)
        => _numericExpression.IsOdd(selector);

    public ValiFlow<T> IsMultipleOf(Expression<Func<T, int>> selector, int divisor)
        => _numericExpression.IsMultipleOf(selector, divisor);

    public ValiFlow<T> IsMultipleOf(Expression<Func<T, long>> selector, long divisor)
        => _numericExpression.IsMultipleOf(selector, divisor);

    public ValiFlow<T> IsBetweenExclusive(Expression<Func<T, int>> selector, int min, int max)
        => _numericExpression.IsBetweenExclusive(selector, min, max);

    public ValiFlow<T> IsBetweenExclusive(Expression<Func<T, long>> selector, long min, long max)
        => _numericExpression.IsBetweenExclusive(selector, min, max);

    public ValiFlow<T> IsBetweenExclusive(Expression<Func<T, float>> selector, float min, float max)
        => _numericExpression.IsBetweenExclusive(selector, min, max);

    public ValiFlow<T> IsBetweenExclusive(Expression<Func<T, double>> selector, double min, double max)
        => _numericExpression.IsBetweenExclusive(selector, min, max);

    public ValiFlow<T> IsBetweenExclusive(Expression<Func<T, decimal>> selector, decimal min, decimal max)
        => _numericExpression.IsBetweenExclusive(selector, min, max);

    public ValiFlow<T> IsBetweenExclusive(Expression<Func<T, short>> selector, short min, short max)
        => _numericExpression.IsBetweenExclusive(selector, min, max);

    public ValiFlow<T> IsCloseTo(Expression<Func<T, double>> selector, double value, double tolerance)
        => _numericExpression.IsCloseTo(selector, value, tolerance);

    public ValiFlow<T> IsCloseTo(Expression<Func<T, float>> selector, float value, float tolerance)
        => _numericExpression.IsCloseTo(selector, value, tolerance);

    // ── IComparable<TValue> generic overloads ──────────────────────────────────

    public ValiFlow<T> GreaterThan<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IComparable<TValue>
        => _numericExpression.GreaterThan(selector, value);

    public ValiFlow<T> GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IComparable<TValue>
        => _numericExpression.GreaterThanOrEqualTo(selector, value);

    public ValiFlow<T> LessThan<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IComparable<TValue>
        => _numericExpression.LessThan(selector, value);

    public ValiFlow<T> LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IComparable<TValue>
        => _numericExpression.LessThanOrEqualTo(selector, value);

    public ValiFlow<T> InRange<TValue>(Expression<Func<T, TValue>> selector, TValue min, TValue max)
        where TValue : IComparable<TValue>
        => _numericExpression.InRange(selector, min, max);

    ValiFlow<T> INumericExpression<ValiFlow<T>, T>.EqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        => _numericExpression.EqualTo(selector, value);

    // ── Cross-property comparisons ─────────────────────────────────────────────

    public ValiFlow<T> GreaterThan<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        where TValue : IComparable<TValue>
        => _numericExpression.GreaterThan(selector, otherSelector);

    public ValiFlow<T> GreaterThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        where TValue : IComparable<TValue>
        => _numericExpression.GreaterThanOrEqualTo(selector, otherSelector);

    public ValiFlow<T> LessThan<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        where TValue : IComparable<TValue>
        => _numericExpression.LessThan(selector, otherSelector);

    public ValiFlow<T> LessThanOrEqualTo<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        where TValue : IComparable<TValue>
        => _numericExpression.LessThanOrEqualTo(selector, otherSelector);

    // ── Nullable numeric ───────────────────────────────────────────────────────

    public ValiFlow<T> IsNullOrZero(Expression<Func<T, int?>> selector)
        => _numericExpression.IsNullOrZero(selector);

    public ValiFlow<T> IsNullOrZero(Expression<Func<T, decimal?>> selector)
        => _numericExpression.IsNullOrZero(selector);

    public ValiFlow<T> IsNullOrZero(Expression<Func<T, long?>> selector)
        => _numericExpression.IsNullOrZero(selector);

    public ValiFlow<T> IsNullOrZero(Expression<Func<T, double?>> selector)
        => _numericExpression.IsNullOrZero(selector);

    public ValiFlow<T> IsNullOrZero(Expression<Func<T, float?>> selector)
        => _numericExpression.IsNullOrZero(selector);

    public ValiFlow<T> IsNullOrZero(Expression<Func<T, short?>> selector)
        => _numericExpression.IsNullOrZero(selector);

    public ValiFlow<T> HasValue(Expression<Func<T, int?>> selector)
        => _numericExpression.HasValue(selector);

    public ValiFlow<T> HasValue(Expression<Func<T, decimal?>> selector)
        => _numericExpression.HasValue(selector);

    public ValiFlow<T> HasValue(Expression<Func<T, long?>> selector)
        => _numericExpression.HasValue(selector);

    public ValiFlow<T> HasValue(Expression<Func<T, double?>> selector)
        => _numericExpression.HasValue(selector);

    public ValiFlow<T> HasValue(Expression<Func<T, float?>> selector)
        => _numericExpression.HasValue(selector);

    public ValiFlow<T> HasValue(Expression<Func<T, short?>> selector)
        => _numericExpression.HasValue(selector);

    public ValiFlow<T> GreaterThan(Expression<Func<T, int?>> selector, int value)
        => _numericExpression.GreaterThan(selector, value);

    public ValiFlow<T> GreaterThan(Expression<Func<T, decimal?>> selector, decimal value)
        => _numericExpression.GreaterThan(selector, value);

    public ValiFlow<T> GreaterThan(Expression<Func<T, long?>> selector, long value)
        => _numericExpression.GreaterThan(selector, value);

    public ValiFlow<T> GreaterThan(Expression<Func<T, double?>> selector, double value)
        => _numericExpression.GreaterThan(selector, value);

    public ValiFlow<T> GreaterThan(Expression<Func<T, float?>> selector, float value)
        => _numericExpression.GreaterThan(selector, value);

    public ValiFlow<T> GreaterThan(Expression<Func<T, short?>> selector, short value)
        => _numericExpression.GreaterThan(selector, value);

    public ValiFlow<T> LessThan(Expression<Func<T, int?>> selector, int value)
        => _numericExpression.LessThan(selector, value);

    public ValiFlow<T> LessThan(Expression<Func<T, decimal?>> selector, decimal value)
        => _numericExpression.LessThan(selector, value);

    public ValiFlow<T> LessThan(Expression<Func<T, long?>> selector, long value)
        => _numericExpression.LessThan(selector, value);

    public ValiFlow<T> LessThan(Expression<Func<T, double?>> selector, double value)
        => _numericExpression.LessThan(selector, value);

    public ValiFlow<T> LessThan(Expression<Func<T, float?>> selector, float value)
        => _numericExpression.LessThan(selector, value);

    public ValiFlow<T> LessThan(Expression<Func<T, short?>> selector, short value)
        => _numericExpression.LessThan(selector, value);

    public ValiFlow<T> InRange(Expression<Func<T, int?>> selector, int min, int max)
        => _numericExpression.InRange(selector, min, max);

    public ValiFlow<T> InRange(Expression<Func<T, decimal?>> selector, decimal min, decimal max)
        => _numericExpression.InRange(selector, min, max);

    public ValiFlow<T> InRange(Expression<Func<T, long?>> selector, long min, long max)
        => _numericExpression.InRange(selector, min, max);

    public ValiFlow<T> InRange(Expression<Func<T, double?>> selector, double min, double max)
        => _numericExpression.InRange(selector, min, max);

    public ValiFlow<T> InRange(Expression<Func<T, float?>> selector, float min, float max)
        => _numericExpression.InRange(selector, min, max);

    public ValiFlow<T> InRange(Expression<Func<T, short?>> selector, short min, short max)
        => _numericExpression.InRange(selector, min, max);

    public ValiFlow<T> Empty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector)
        => _collectionExpression.Empty(selector);

    public ValiFlow<T> MinCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min)
        => _collectionExpression.MinCount(selector, min);

    public ValiFlow<T> MaxCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int max)
        => _collectionExpression.MaxCount(selector, max);

    public ValiFlow<T> HasDuplicates<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector)
        => _collectionExpression.HasDuplicates(selector);

    public ValiFlow<T> EachItem<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector, Action<ValiFlow<TValue>> configure)
        => _collectionExpression.EachItem(selector, configure);

    public ValiFlow<T> AnyItem<TValue>(
        Expression<Func<T, IEnumerable<TValue>>> selector,
        Action<ValiFlow<TValue>> configure)
        => _collectionExpression.AnyItem(selector, configure);

    public ValiFlow<T> AllMatch<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector, ValiFlow<TValue> filter)
        => _collectionExpression.AllMatch(selector, filter);

    public ValiFlow<T> CountEquals<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int count)
        => _collectionExpression.CountEquals<TValue>(selector, count);

    public ValiFlow<T> FutureDate(Expression<Func<T, DateTime>> selector)
        => _dateTimeExpression.FutureDate(selector);

    public ValiFlow<T> PastDate(Expression<Func<T, DateTime>> selector)
        => _dateTimeExpression.PastDate(selector);

    public ValiFlow<T> BetweenDates(Expression<Func<T, DateTime>> selector, DateTime startDate, DateTime endDate)
        => _dateTimeExpression.BetweenDates(selector, startDate, endDate);

    public ValiFlow<T> BetweenDates(Expression<Func<T, DateTime>> selector,
        Expression<Func<T, DateTime>> startDateSelector, Expression<Func<T, DateTime>> endDateSelector)
        => _dateTimeExpression.BetweenDates(selector, startDateSelector, endDateSelector);

    public ValiFlow<T> ExactDate(Expression<Func<T, DateTime>> selector, DateTime date)
        => _dateTimeExpression.ExactDate(selector, date);

    public ValiFlow<T> BeforeDate(Expression<Func<T, DateTime>> selector, DateTime date)
        => _dateTimeExpression.BeforeDate(selector, date);

    public ValiFlow<T> AfterDate(Expression<Func<T, DateTime>> selector, DateTime date)
        => _dateTimeExpression.AfterDate(selector, date);

    public ValiFlow<T> IsToday(Expression<Func<T, DateTime>> selector)
        => _dateTimeExpression.IsToday(selector);

    public ValiFlow<T> IsYesterday(Expression<Func<T, DateTime>> selector)
        => _dateTimeExpression.IsYesterday(selector);

    public ValiFlow<T> IsTomorrow(Expression<Func<T, DateTime>> selector)
        => _dateTimeExpression.IsTomorrow(selector);

    public ValiFlow<T> InLastDays(Expression<Func<T, DateTime>> selector, int days)
        => _dateTimeExpression.InLastDays(selector, days);

    public ValiFlow<T> InNextDays(Expression<Func<T, DateTime>> selector, int days)
        => _dateTimeExpression.InNextDays(selector, days);

    public ValiFlow<T> IsWeekend(Expression<Func<T, DateTime>> selector)
        => _dateTimeExpression.IsWeekend(selector);

    public ValiFlow<T> IsWeekday(Expression<Func<T, DateTime>> selector)
        => _dateTimeExpression.IsWeekday(selector);

    public ValiFlow<T> IsLeapYear(Expression<Func<T, DateTime>> selector)
        => _dateTimeExpression.IsLeapYear(selector);

    public ValiFlow<T> SameMonthAs(Expression<Func<T, DateTime>> selector, DateTime date)
        => _dateTimeExpression.SameMonthAs(selector, date);

    public ValiFlow<T> SameYearAs(Expression<Func<T, DateTime>> selector, DateTime date)
        => _dateTimeExpression.SameYearAs(selector, date);

    public ValiFlow<T> IsDayOfWeek(Expression<Func<T, DateTime>> selector, DayOfWeek day)
        => _dateTimeExpression.IsDayOfWeek(selector, day);

    public ValiFlow<T> IsInMonth(Expression<Func<T, DateTime>> selector, int month)
        => _dateTimeExpression.IsInMonth(selector, month);

    public ValiFlow<T> IsInYear(Expression<Func<T, DateTime>> selector, int year)
        => _dateTimeExpression.IsInYear(selector, year);

    public ValiFlow<T> IsBefore(Expression<Func<T, DateTime>> selector, DateTime date)
        => _dateTimeExpression.IsBefore(selector, date);

    public ValiFlow<T> IsAfter(Expression<Func<T, DateTime>> selector, DateTime date)
        => _dateTimeExpression.IsAfter(selector, date);

    // ── DateTimeOffset ────────────────────────────────────────────────────────

    public ValiFlow<T> FutureDate(Expression<Func<T, DateTimeOffset>> selector)
        => _dateTimeOffsetExpression.FutureDate(selector);

    public ValiFlow<T> PastDate(Expression<Func<T, DateTimeOffset>> selector)
        => _dateTimeOffsetExpression.PastDate(selector);

    public ValiFlow<T> IsToday(Expression<Func<T, DateTimeOffset>> selector)
        => _dateTimeOffsetExpression.IsToday(selector);

    public ValiFlow<T> IsWeekend(Expression<Func<T, DateTimeOffset>> selector)
        => _dateTimeOffsetExpression.IsWeekend(selector);

    public ValiFlow<T> IsWeekday(Expression<Func<T, DateTimeOffset>> selector)
        => _dateTimeOffsetExpression.IsWeekday(selector);

    public ValiFlow<T> IsDayOfWeek(Expression<Func<T, DateTimeOffset>> selector, DayOfWeek dayOfWeek)
        => _dateTimeOffsetExpression.IsDayOfWeek(selector, dayOfWeek);

    public ValiFlow<T> IsInMonth(Expression<Func<T, DateTimeOffset>> selector, int month)
        => _dateTimeOffsetExpression.IsInMonth(selector, month);

    public ValiFlow<T> IsInYear(Expression<Func<T, DateTimeOffset>> selector, int year)
        => _dateTimeOffsetExpression.IsInYear(selector, year);

    public ValiFlow<T> IsBefore(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
        => _dateTimeOffsetExpression.IsBefore(selector, date);

    public ValiFlow<T> IsAfter(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
        => _dateTimeOffsetExpression.IsAfter(selector, date);

    public ValiFlow<T> BetweenDates(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset from, DateTimeOffset to)
        => _dateTimeOffsetExpression.BetweenDates(selector, from, to);

    public ValiFlow<T> ExactDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
        => _dateTimeOffsetExpression.ExactDate(selector, date);

    public ValiFlow<T> BeforeDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
        => _dateTimeOffsetExpression.BeforeDate(selector, date);

    public ValiFlow<T> AfterDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
        => _dateTimeOffsetExpression.AfterDate(selector, date);

    public ValiFlow<T> SameMonthAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
        => _dateTimeOffsetExpression.SameMonthAs(selector, date);

    public ValiFlow<T> SameYearAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
        => _dateTimeOffsetExpression.SameYearAs(selector, date);

    public ValiFlow<T> InLastDays(Expression<Func<T, DateTimeOffset>> selector, int days)
        => _dateTimeOffsetExpression.InLastDays(selector, days);

    public ValiFlow<T> InNextDays(Expression<Func<T, DateTimeOffset>> selector, int days)
        => _dateTimeOffsetExpression.InNextDays(selector, days);

    public ValiFlow<T> IsYesterday(Expression<Func<T, DateTimeOffset>> selector)
        => _dateTimeOffsetExpression.IsYesterday(selector);

    public ValiFlow<T> IsTomorrow(Expression<Func<T, DateTimeOffset>> selector)
        => _dateTimeOffsetExpression.IsTomorrow(selector);

    // ── DateOnly ──────────────────────────────────────────────────────────────

    public ValiFlow<T> FutureDate(Expression<Func<T, DateOnly>> selector)
        => _dateOnlyExpression.FutureDate(selector);

    public ValiFlow<T> PastDate(Expression<Func<T, DateOnly>> selector)
        => _dateOnlyExpression.PastDate(selector);

    public ValiFlow<T> IsToday(Expression<Func<T, DateOnly>> selector)
        => _dateOnlyExpression.IsToday(selector);

    public ValiFlow<T> IsWeekend(Expression<Func<T, DateOnly>> selector)
        => _dateOnlyExpression.IsWeekend(selector);

    public ValiFlow<T> IsWeekday(Expression<Func<T, DateOnly>> selector)
        => _dateOnlyExpression.IsWeekday(selector);

    public ValiFlow<T> IsDayOfWeek(Expression<Func<T, DateOnly>> selector, DayOfWeek dayOfWeek)
        => _dateOnlyExpression.IsDayOfWeek(selector, dayOfWeek);

    public ValiFlow<T> IsInMonth(Expression<Func<T, DateOnly>> selector, int month)
        => _dateOnlyExpression.IsInMonth(selector, month);

    public ValiFlow<T> IsInYear(Expression<Func<T, DateOnly>> selector, int year)
        => _dateOnlyExpression.IsInYear(selector, year);

    public ValiFlow<T> IsBefore(Expression<Func<T, DateOnly>> selector, DateOnly date)
        => _dateOnlyExpression.IsBefore(selector, date);

    public ValiFlow<T> IsAfter(Expression<Func<T, DateOnly>> selector, DateOnly date)
        => _dateOnlyExpression.IsAfter(selector, date);

    public ValiFlow<T> BetweenDates(Expression<Func<T, DateOnly>> selector, DateOnly from, DateOnly to)
        => _dateOnlyExpression.BetweenDates(selector, from, to);

    public ValiFlow<T> BetweenDates(Expression<Func<T, DateOnly>> selector,
        Expression<Func<T, DateOnly>> fromSelector, Expression<Func<T, DateOnly>> toSelector)
        => _dateOnlyExpression.BetweenDates(selector, fromSelector, toSelector);

    public ValiFlow<T> IsFirstDayOfMonth(Expression<Func<T, DateOnly>> selector)
        => _dateOnlyExpression.IsFirstDayOfMonth(selector);

    public ValiFlow<T> IsLastDayOfMonth(Expression<Func<T, DateOnly>> selector)
        => _dateOnlyExpression.IsLastDayOfMonth(selector);

    public ValiFlow<T> ExactDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
        => _dateOnlyExpression.ExactDate(selector, date);

    public ValiFlow<T> BeforeDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
        => _dateOnlyExpression.BeforeDate(selector, date);

    public ValiFlow<T> AfterDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
        => _dateOnlyExpression.AfterDate(selector, date);

    public ValiFlow<T> IsYesterday(Expression<Func<T, DateOnly>> selector)
        => _dateOnlyExpression.IsYesterday(selector);

    public ValiFlow<T> IsTomorrow(Expression<Func<T, DateOnly>> selector)
        => _dateOnlyExpression.IsTomorrow(selector);

    public ValiFlow<T> InLastDays(Expression<Func<T, DateOnly>> selector, int days)
        => _dateOnlyExpression.InLastDays(selector, days);

    public ValiFlow<T> InNextDays(Expression<Func<T, DateOnly>> selector, int days)
        => _dateOnlyExpression.InNextDays(selector, days);

    public ValiFlow<T> SameMonthAs(Expression<Func<T, DateOnly>> selector, DateOnly date)
        => _dateOnlyExpression.SameMonthAs(selector, date);

    public ValiFlow<T> SameYearAs(Expression<Func<T, DateOnly>> selector, DateOnly date)
        => _dateOnlyExpression.SameYearAs(selector, date);

    // ── TimeOnly ──────────────────────────────────────────────────────────────

    public ValiFlow<T> IsBefore(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
        => _timeOnlyExpression.IsBefore(selector, time);

    public ValiFlow<T> IsAfter(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
        => _timeOnlyExpression.IsAfter(selector, time);

    public ValiFlow<T> IsBetween(Expression<Func<T, TimeOnly>> selector, TimeOnly from, TimeOnly to)
        => _timeOnlyExpression.IsBetween(selector, from, to);

    public ValiFlow<T> IsAM(Expression<Func<T, TimeOnly>> selector)
        => _timeOnlyExpression.IsAM(selector);

    public ValiFlow<T> IsPM(Expression<Func<T, TimeOnly>> selector)
        => _timeOnlyExpression.IsPM(selector);

    public ValiFlow<T> IsExactTime(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
        => _timeOnlyExpression.IsExactTime(selector, time);

    public ValiFlow<T> IsInHour(Expression<Func<T, TimeOnly>> selector, int hour)
        => _timeOnlyExpression.IsInHour(selector, hour);

    // ── Nested validation ─────────────────────────────────────────────────────

    public new ValiFlow<T> ValidateNested<TProperty>(
        Expression<Func<T, TProperty>> selector,
        Action<ValiFlow<TProperty>> configure)
        where TProperty : class
        => base.ValidateNested(selector, configure);

    // ── Cached build ──────────────────────────────────────────────────────────

    public new Func<T, bool> BuildCached() => base.BuildCached();

    // ── WithError / WithSeverity overloads ────────────────────────────────────

    public new ValiFlow<T> WithError(string errorCode, string message, string propertyPath)
        => (ValiFlow<T>)base.WithError(errorCode, message, propertyPath);

    public new ValiFlow<T> WithError(string errorCode, string message, Severity severity)
        => (ValiFlow<T>)base.WithError(errorCode, message, severity);

    public new ValiFlow<T> WithError(string errorCode, string message, string propertyPath, Severity severity)
        => (ValiFlow<T>)base.WithError(errorCode, message, propertyPath, severity);

    public new ValiFlow<T> WithSeverity(Severity severity)
        => (ValiFlow<T>)base.WithSeverity(severity);

    // ── Builder combining ─────────────────────────────────────────────────────

    public static Expression<Func<T, bool>> Combine(ValiFlow<T> left, ValiFlow<T> right, bool and = true)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return CombineExpressions(left.Build(), right.Build(), and);
    }

    public static Expression<Func<T, bool>> operator &(ValiFlow<T> left, ValiFlow<T> right) => Combine(left, right, and: true);
    public static Expression<Func<T, bool>> operator |(ValiFlow<T> left, ValiFlow<T> right) => Combine(left, right, and: false);
    public static Expression<Func<T, bool>> operator !(ValiFlow<T> flow)
    {
        ArgumentNullException.ThrowIfNull(flow);
        return flow.BuildNegated();
    }

}