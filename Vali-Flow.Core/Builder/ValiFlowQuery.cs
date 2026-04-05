using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
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
    // ── Boolean ───────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected bool property is <see langword="true"/>.</summary>
    public ValiFlowQuery<T> IsTrue(Expression<Func<T, bool>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        return Add(selector);
    }

    /// <summary>Validates that the selected bool property is <see langword="false"/>.</summary>
    public ValiFlowQuery<T> IsFalse(Expression<Func<T, bool>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<T, bool>> negated = Expression.Lambda<Func<T, bool>>(
            Expression.Not(selector.Body), selector.Parameters);
        return Add(negated);
    }

    // ── Comparison ────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected property is not null.</summary>
    public ValiFlowQuery<T> NotNull<TProperty>(Expression<Func<T, TProperty?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TProperty?, bool>> predicate = value => value != null;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected property is null.</summary>
    public ValiFlowQuery<T> Null<TProperty>(Expression<Func<T, TProperty?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TProperty?, bool>> predicate = value => value == null;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected property equals <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> EqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IEquatable<TValue>
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var param = Expression.Parameter(typeof(TValue), "v");
        var body = Expression.Equal(param, Expression.Constant(value, typeof(TValue)));
        Expression<Func<TValue, bool>> predicate = Expression.Lambda<Func<TValue, bool>>(body, param);
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected property does not equal <paramref name="value"/>.</summary>
    public ValiFlowQuery<T> NotEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IEquatable<TValue>
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var param = Expression.Parameter(typeof(TValue), "v");
        var body = Expression.NotEqual(param, Expression.Constant(value, typeof(TValue)));
        Expression<Func<TValue, bool>> predicate = Expression.Lambda<Func<TValue, bool>>(body, param);
        return Add(selector, predicate);
    }

    // ── String ────────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected string has a minimum length.</summary>
    public ValiFlowQuery<T> MinLength(Expression<Func<T, string?>> selector, int minLength)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (minLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(minLength), "minLength must be at least 1. Use IsNotNullOrEmpty() to require a non-empty string.");
        Expression<Func<string?, bool>> predicate = val => !string.IsNullOrEmpty(val) && val.Length >= minLength;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string does not exceed a maximum length. Null values pass.</summary>
    public ValiFlowQuery<T> MaxLength(Expression<Func<T, string?>> selector, int maxLength)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (maxLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "maxLength must be at least 1. Use IsNullOrEmpty() to require an empty or null string.");
        Expression<Func<string?, bool>> predicate = val => val == null || val.Length <= maxLength;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string has an exact length.</summary>
    public ValiFlowQuery<T> ExactLength(Expression<Func<T, string?>> selector, int length)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "length must be >= 0.");
        Expression<Func<string?, bool>> predicate = val => val != null && val.Length == length;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string length is between <paramref name="min"/> and <paramref name="max"/> (inclusive).</summary>
    public ValiFlowQuery<T> LengthBetween(Expression<Func<T, string?>> selector, int min, int max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "min must be >= 0.");
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<string?, bool>> predicate = val => val != null && val.Length >= min && val.Length <= max;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string is null or empty.</summary>
    public ValiFlowQuery<T> IsNullOrEmpty(Expression<Func<T, string?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<string?, bool>> predicate = val => string.IsNullOrEmpty(val);
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string is not null and not empty.</summary>
    public ValiFlowQuery<T> IsNotNullOrEmpty(Expression<Func<T, string?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<string?, bool>> predicate = val => !string.IsNullOrEmpty(val);
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string is null or contains only whitespace.</summary>
    /// <remarks>
    /// Uses <c>string.Trim()</c> internally. EF Core translates <c>Trim()</c> to:
    /// <list type="bullet">
    /// <item>SQL Server: <c>LTRIM(RTRIM(x))</c></item>
    /// <item>PostgreSQL / SQLite: <c>trim(x)</c></item>
    /// <item>MySQL (Pomelo 5.0+): <c>TRIM(x)</c></item>
    /// <item>Oracle EF Core 7.0+: <c>TRIM(x)</c></item>
    /// </list>
    /// <b>Not supported</b> on Pomelo MySQL &lt; 5.0 or EF Core providers that do not translate <c>string.Trim()</c>.
    /// On unsupported providers, a runtime <see cref="InvalidOperationException"/> will be thrown at query-translation time.
    /// For maximum provider compatibility, use <see cref="IsNullOrEmpty"/> combined with a manual check instead.
    /// </remarks>
    public ValiFlowQuery<T> IsNullOrWhiteSpace(Expression<Func<T, string?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<string?, bool>> predicate = val => val == null || val.Trim() == "";
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string is not null and contains at least one non-whitespace character.</summary>
    /// <remarks>
    /// Uses <c>string.Trim()</c> internally. EF Core translates <c>Trim()</c> to:
    /// <list type="bullet">
    /// <item>SQL Server: <c>LTRIM(RTRIM(x))</c></item>
    /// <item>PostgreSQL / SQLite: <c>trim(x)</c></item>
    /// <item>MySQL (Pomelo 5.0+): <c>TRIM(x)</c></item>
    /// <item>Oracle EF Core 7.0+: <c>TRIM(x)</c></item>
    /// </list>
    /// <b>Not supported</b> on Pomelo MySQL &lt; 5.0 or EF Core providers that do not translate <c>string.Trim()</c>.
    /// On unsupported providers, a runtime <see cref="InvalidOperationException"/> will be thrown at query-translation time.
    /// For maximum provider compatibility, use <see cref="IsNotNullOrEmpty"/> combined with a manual check instead.
    /// </remarks>
    public ValiFlowQuery<T> IsNotNullOrWhiteSpace(Expression<Func<T, string?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<string?, bool>> predicate = val => val != null && val.Trim() != "";
        return Add(selector, predicate);
    }

    /// <summary>
    /// Validates that the selected string starts with <paramref name="value"/>.
    /// </summary>
    /// <remarks>
    /// Uses the single-argument <c>string.StartsWith(string)</c> overload which EF Core translates
    /// to SQL <c>LIKE 'value%'</c>. The comparison is culture-invariant and case-sensitive at the
    /// database level (depends on the column collation).
    /// </remarks>
    public ValiFlowQuery<T> StartsWith(Expression<Func<T, string?>> selector, string value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val != null && val.StartsWith(value);
        return Add(selector, predicate);
    }

    /// <summary>
    /// Validates that the selected string ends with <paramref name="value"/>.
    /// </summary>
    /// <remarks>
    /// Uses the single-argument <c>string.EndsWith(string)</c> overload which EF Core translates
    /// to SQL <c>LIKE '%value'</c>. The comparison is culture-invariant and case-sensitive at the
    /// database level (depends on the column collation).
    /// </remarks>
    public ValiFlowQuery<T> EndsWith(Expression<Func<T, string?>> selector, string value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val != null && val.EndsWith(value);
        return Add(selector, predicate);
    }

    /// <summary>
    /// Validates that the selected string contains <paramref name="value"/>.
    /// </summary>
    /// <remarks>
    /// Uses the single-argument <c>string.Contains(string)</c> overload which EF Core translates
    /// to SQL <c>LIKE '%value%'</c>. The comparison is culture-invariant and case-sensitive at the
    /// database level (depends on the column collation).
    /// </remarks>
    public ValiFlowQuery<T> Contains(Expression<Func<T, string?>> selector, string value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (value.Length == 0)
            throw new ArgumentException("value cannot be empty for Contains(). An empty string would match every non-null value.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val != null && val.Contains(value);
        return Add(selector, predicate);
    }

    // ── Collection ────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected collection is not null and has at least one element.</summary>
    /// <remarks>
    /// EF Core translates <c>val.Any()</c> as <c>EXISTS (SELECT...)</c> for mapped navigation collection properties.
    /// This method will NOT translate for computed or JSON-mapped collections — use with in-memory collections in those cases.
    /// </remarks>
    public ValiFlowQuery<T> NotEmpty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Any();
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected collection is not null and has no elements.</summary>
    /// <remarks>
    /// EF Core translates <c>val.Any()</c> as <c>EXISTS (SELECT...)</c> for mapped navigation collection properties.
    /// This method will NOT translate for computed or JSON-mapped collections — use with in-memory collections in those cases.
    /// </remarks>
    public ValiFlowQuery<T> Empty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && !val.Any();
        return Add(selector, predicate);
    }

    /// <summary>
    /// Validates that the selected scalar value is contained in <paramref name="values"/>.
    /// </summary>
    /// <remarks>
    /// EF Core translates <c>list.Contains(val)</c> to SQL <c>IN (...)</c> for scalar types
    /// (string, int, Guid, enum, etc.).
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty — an empty
    /// <c>IN</c> list would silently filter out every row.</exception>
    public ValiFlowQuery<T> In<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (values == null) throw new ArgumentNullException(nameof(values));
        List<TValue> valueList = values.ToList();
        if (valueList.Count == 0)
            throw new ArgumentException("values must not be empty for In(). An empty IN list would silently filter out every row.", nameof(values));
        Expression<Func<TValue, bool>> predicate = val => valueList.Contains(val);
        return Add(selector, predicate);
    }

    /// <summary>
    /// Validates that the selected scalar value is NOT contained in <paramref name="values"/>.
    /// </summary>
    /// <remarks>
    /// EF Core translates <c>!list.Contains(val)</c> to SQL <c>NOT IN (...)</c> for scalar types.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty — an empty
    /// <c>NOT IN</c> list would silently pass every row.</exception>
    public ValiFlowQuery<T> NotIn<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (values == null) throw new ArgumentNullException(nameof(values));
        List<TValue> valueList = values.ToList();
        if (valueList.Count == 0)
            throw new ArgumentException("values must not be empty for NotIn(). An empty NOT IN list would silently pass every row.", nameof(values));
        Expression<Func<TValue, bool>> predicate = val => !valueList.Contains(val);
        return Add(selector, predicate);
    }

    /// <summary>Validates that the collection has exactly <paramref name="count"/> elements.</summary>
    /// <remarks>
    /// EF Core translates <c>val.Count()</c> as <c>(SELECT COUNT(*)...)</c> for mapped navigation collection properties.
    /// This method will NOT translate for computed or JSON-mapped collections — use with in-memory collections in those cases.
    /// </remarks>
    public ValiFlowQuery<T> Count<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int count)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "count must be >= 0.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() == count;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the collection has at least <paramref name="min"/> elements.</summary>
    /// <remarks>
    /// EF Core translates <c>val.Count()</c> as <c>(SELECT COUNT(*)...)</c> for mapped navigation collection properties.
    /// This method will NOT translate for computed or JSON-mapped collections — use with in-memory collections in those cases.
    /// </remarks>
    public ValiFlowQuery<T> MinCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "min must be >= 0.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() >= min;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the collection has at most <paramref name="max"/> elements. A <c>null</c> collection fails validation.</summary>
    /// <remarks>
    /// EF Core translates <c>val.Count()</c> as <c>(SELECT COUNT(*)...)</c> for mapped navigation collection properties.
    /// This method will NOT translate for computed or JSON-mapped collections — use with in-memory collections in those cases.
    /// </remarks>
    public ValiFlowQuery<T> MaxCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (max < 0) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= 0.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() <= max;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected collection has between <paramref name="min"/> and <paramref name="max"/> elements (inclusive).</summary>
    /// <remarks>
    /// EF Core translates <c>val.Count()</c> for mapped navigation collections as <c>SELECT COUNT(*)</c>.
    /// <b>Note:</b> The count is evaluated twice in the expression tree — once for the lower bound and once for the upper bound.
    /// For most navigation collection scenarios this is acceptable. For non-mapped or computed IEnumerable properties, use with in-memory collections only.
    /// </remarks>
    public ValiFlowQuery<T> CountBetween<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min, int max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "min must be >= 0.");
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() >= min && val.Count() <= max;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string has no leading or trailing whitespace (<c>val == val.Trim()</c>).</summary>
    /// <remarks>
    /// EF Core translates <c>string.Trim()</c> to:
    /// <list type="bullet">
    /// <item>SQL Server: <c>LTRIM(RTRIM(x))</c></item>
    /// <item>PostgreSQL / SQLite: <c>trim(x)</c></item>
    /// <item>MySQL (Pomelo 5.0+): <c>TRIM(x)</c></item>
    /// </list>
    /// A <c>null</c> value fails validation.
    /// </remarks>
    public ValiFlowQuery<T> IsTrimmed(Expression<Func<T, string?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<string?, bool>> predicate = val => val != null && val == val.Trim();
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string equals its lowercase form (<c>val == val.ToLower()</c>).</summary>
    /// <remarks>
    /// EF Core translates <c>string.ToLower()</c> to <c>LOWER(x)</c> on all major providers.
    /// A <c>null</c> value fails validation.
    /// <b>Note:</b> Digits and punctuation are considered "lower-case" by this check
    /// (e.g. <c>"hello123"</c> passes). Use <see cref="ValiFlow{T}.HasOnlyLetters"/> for letter-only validation.
    /// </remarks>
    public ValiFlowQuery<T> IsLowerCase(Expression<Func<T, string?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<string?, bool>> predicate = val => val != null && val == val.ToLower();
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string equals its uppercase form (<c>val == val.ToUpper()</c>).</summary>
    /// <remarks>
    /// EF Core translates <c>string.ToUpper()</c> to <c>UPPER(x)</c> on all major providers.
    /// A <c>null</c> value fails validation.
    /// <b>Note:</b> Digits and punctuation are considered "upper-case" by this check
    /// (e.g. <c>"HELLO123"</c> passes). Use <see cref="ValiFlow{T}.HasOnlyLetters"/> for letter-only validation.
    /// </remarks>
    public ValiFlowQuery<T> IsUpperCase(Expression<Func<T, string?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<string?, bool>> predicate = val => val != null && val == val.ToUpper();
        return Add(selector, predicate);
    }

    // ── String — case-insensitive comparisons ─────────────────────────────────

    /// <summary>Validates that the selected string equals <paramref name="value"/> (case-insensitive, ordinal).</summary>
    /// <remarks>
    /// Implemented as <c>val.ToLower() == value.ToLower()</c>. EF Core translates <c>ToLower()</c> to <c>LOWER(x)</c> on all major providers.
    /// A <c>null</c> value fails validation.
    /// </remarks>
    public ValiFlowQuery<T> EqualToIgnoreCase(Expression<Func<T, string?>> selector, string value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (value == null) throw new ArgumentNullException(nameof(value));
        var lower = value.ToLowerInvariant();
        Expression<Func<string?, bool>> predicate = val => val != null && val.ToLower() == lower;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string starts with <paramref name="value"/> (case-insensitive, ordinal).</summary>
    /// <remarks>
    /// Implemented as <c>val.ToLower().StartsWith(value.ToLower())</c>. EF Core translates to <c>LOWER(x) LIKE 'value%'</c>.
    /// A <c>null</c> value fails validation.
    /// </remarks>
    public ValiFlowQuery<T> StartsWithIgnoreCase(Expression<Func<T, string?>> selector, string value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        var lower = value.ToLowerInvariant();
        Expression<Func<string?, bool>> predicate = val => val != null && val.ToLower().StartsWith(lower);
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string ends with <paramref name="value"/> (case-insensitive, ordinal).</summary>
    /// <remarks>
    /// Implemented as <c>val.ToLower().EndsWith(value.ToLower())</c>. EF Core translates to <c>LOWER(x) LIKE '%value'</c>.
    /// A <c>null</c> value fails validation.
    /// </remarks>
    public ValiFlowQuery<T> EndsWithIgnoreCase(Expression<Func<T, string?>> selector, string value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        var lower = value.ToLowerInvariant();
        Expression<Func<string?, bool>> predicate = val => val != null && val.ToLower().EndsWith(lower);
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string contains <paramref name="value"/> (case-insensitive, ordinal).</summary>
    /// <remarks>
    /// Implemented as <c>val.ToLower().Contains(value.ToLower())</c>. EF Core translates to <c>LOWER(x) LIKE '%value%'</c>.
    /// A <c>null</c> value fails validation.
    /// </remarks>
    public ValiFlowQuery<T> ContainsIgnoreCase(Expression<Func<T, string?>> selector, string value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        var lower = value.ToLowerInvariant();
        Expression<Func<string?, bool>> predicate = val => val != null && val.ToLower().Contains(lower);
        return Add(selector, predicate);
    }

    // ── String — negation ─────────────────────────────────────────────────────

    /// <summary>Validates that the selected string does <b>not</b> contain <paramref name="value"/>. A <c>null</c> value passes.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>!string.Contains(x)</c>.</remarks>
    public ValiFlowQuery<T> NotContains(Expression<Func<T, string?>> selector, string value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val == null || !val.Contains(value);
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string does <b>not</b> start with <paramref name="value"/>. A <c>null</c> value passes.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>!string.StartsWith(x)</c>.</remarks>
    public ValiFlowQuery<T> NotStartsWith(Expression<Func<T, string?>> selector, string value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val == null || !val.StartsWith(value);
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected string does <b>not</b> end with <paramref name="value"/>. A <c>null</c> value passes.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>!string.EndsWith(x)</c>.</remarks>
    public ValiFlowQuery<T> NotEndsWith(Expression<Func<T, string?>> selector, string value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val == null || !val.EndsWith(value);
        return Add(selector, predicate);
    }

    // ── Numeric — int ─────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="int"/> equals zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Zero(Expression<Func<T, int>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<int, bool>> p = val => val == 0; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="int"/> is not zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> NotZero(Expression<Func<T, int>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<int, bool>> p = val => val != 0; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="int"/> is greater than zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Positive(Expression<Func<T, int>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<int, bool>> p = val => val > 0; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="int"/> is less than zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Negative(Expression<Func<T, int>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<int, bool>> p = val => val < 0; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="int"/> is greater than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, int>> selector, int value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<int, bool>> p = val => val > value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="int"/> is greater than or equal to <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> GreaterThanOrEqualTo(Expression<Func<T, int>> selector, int value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<int, bool>> p = val => val >= value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="int"/> is less than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, int>> selector, int value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<int, bool>> p = val => val < value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="int"/> is less than or equal to <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> LessThanOrEqualTo(Expression<Func<T, int>> selector, int value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<int, bool>> p = val => val <= value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="int"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> MinValue(Expression<Func<T, int>> selector, int minValue)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<int, bool>> p = val => val >= minValue; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="int"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> MaxValue(Expression<Func<T, int>> selector, int maxValue)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<int, bool>> p = val => val <= maxValue; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="int"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, int>> selector, int min, int max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<int, bool>> p = val => val >= min && val <= max;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="int"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, int>> selector,
        Expression<Func<T, int>> minSelector, Expression<Func<T, int>> maxSelector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (minSelector == null) throw new ArgumentNullException(nameof(minSelector));
        if (maxSelector == null) throw new ArgumentNullException(nameof(maxSelector));
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    /// <summary>Validates that the selected <see cref="int"/> is even (<c>val % 2 == 0</c>).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via modulo operator.</remarks>
    public ValiFlowQuery<T> IsEven(Expression<Func<T, int>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<int, bool>> predicate = val => val % 2 == 0;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="int"/> is odd (<c>val % 2 != 0</c>).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via modulo operator.</remarks>
    public ValiFlowQuery<T> IsOdd(Expression<Func<T, int>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<int, bool>> predicate = val => val % 2 != 0;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="int"/> is a multiple of <paramref name="divisor"/> (<c>val % divisor == 0</c>).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via modulo operator.</remarks>
    public ValiFlowQuery<T> IsMultipleOf(Expression<Func<T, int>> selector, int divisor)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (divisor == 0) throw new ArgumentOutOfRangeException(nameof(divisor), "divisor must not be zero.");
        Expression<Func<int, bool>> predicate = val => val % divisor == 0;
        return Add(selector, predicate);
    }

    // ── Numeric — long ────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="long"/> equals zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Zero(Expression<Func<T, long>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<long, bool>> p = val => val == 0L; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="long"/> does not equal zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> NotZero(Expression<Func<T, long>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<long, bool>> p = val => val != 0L; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="long"/> is greater than zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Positive(Expression<Func<T, long>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<long, bool>> p = val => val > 0L; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="long"/> is less than zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Negative(Expression<Func<T, long>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<long, bool>> p = val => val < 0L; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="long"/> is greater than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, long>> selector, long value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<long, bool>> p = val => val > value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="long"/> is greater than or equal to <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> GreaterThanOrEqualTo(Expression<Func<T, long>> selector, long value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<long, bool>> p = val => val >= value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="long"/> is less than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, long>> selector, long value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<long, bool>> p = val => val < value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="long"/> is less than or equal to <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> LessThanOrEqualTo(Expression<Func<T, long>> selector, long value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<long, bool>> p = val => val <= value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="long"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> MinValue(Expression<Func<T, long>> selector, long minValue)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<long, bool>> p = val => val >= minValue; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="long"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> MaxValue(Expression<Func<T, long>> selector, long maxValue)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<long, bool>> p = val => val <= maxValue; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="long"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, long>> selector, long min, long max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<long, bool>> p = val => val >= min && val <= max;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="long"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, long>> selector,
        Expression<Func<T, long>> minSelector, Expression<Func<T, long>> maxSelector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (minSelector == null) throw new ArgumentNullException(nameof(minSelector));
        if (maxSelector == null) throw new ArgumentNullException(nameof(maxSelector));
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    /// <summary>Validates that the selected <see cref="long"/> is even (<c>val % 2 == 0</c>).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via modulo operator.</remarks>
    public ValiFlowQuery<T> IsEven(Expression<Func<T, long>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<long, bool>> predicate = val => val % 2 == 0;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="long"/> is odd (<c>val % 2 != 0</c>).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via modulo operator.</remarks>
    public ValiFlowQuery<T> IsOdd(Expression<Func<T, long>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<long, bool>> predicate = val => val % 2 != 0;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="long"/> is a multiple of <paramref name="divisor"/> (<c>val % divisor == 0</c>).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via modulo operator.</remarks>
    public ValiFlowQuery<T> IsMultipleOf(Expression<Func<T, long>> selector, long divisor)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (divisor == 0) throw new ArgumentOutOfRangeException(nameof(divisor), "divisor must not be zero.");
        Expression<Func<long, bool>> predicate = val => val % divisor == 0;
        return Add(selector, predicate);
    }

    // ── Numeric — double ──────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="double"/> equals zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Zero(Expression<Func<T, double>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<double, bool>> p = val => val == 0.0; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="double"/> does not equal zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> NotZero(Expression<Func<T, double>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<double, bool>> p = val => val != 0.0; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="double"/> is greater than zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Positive(Expression<Func<T, double>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<double, bool>> p = val => val > 0.0; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="double"/> is less than zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Negative(Expression<Func<T, double>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<double, bool>> p = val => val < 0.0; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="double"/> is greater than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, double>> selector, double value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<double, bool>> p = val => val > value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="double"/> is greater than or equal to <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> GreaterThanOrEqualTo(Expression<Func<T, double>> selector, double value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<double, bool>> p = val => val >= value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="double"/> is less than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, double>> selector, double value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<double, bool>> p = val => val < value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="double"/> is less than or equal to <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> LessThanOrEqualTo(Expression<Func<T, double>> selector, double value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<double, bool>> p = val => val <= value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="double"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> MinValue(Expression<Func<T, double>> selector, double minValue)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<double, bool>> p = val => val >= minValue; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="double"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> MaxValue(Expression<Func<T, double>> selector, double maxValue)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<double, bool>> p = val => val <= maxValue; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="double"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, double>> selector, double min, double max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<double, bool>> p = val => val >= min && val <= max;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="double"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, double>> selector,
        Expression<Func<T, double>> minSelector, Expression<Func<T, double>> maxSelector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (minSelector == null) throw new ArgumentNullException(nameof(minSelector));
        if (maxSelector == null) throw new ArgumentNullException(nameof(maxSelector));
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    // ── Numeric — decimal ─────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="decimal"/> equals zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Zero(Expression<Func<T, decimal>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<decimal, bool>> p = val => val == 0m; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="decimal"/> does not equal zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> NotZero(Expression<Func<T, decimal>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<decimal, bool>> p = val => val != 0m; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="decimal"/> is greater than zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Positive(Expression<Func<T, decimal>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<decimal, bool>> p = val => val > 0m; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="decimal"/> is less than zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Negative(Expression<Func<T, decimal>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<decimal, bool>> p = val => val < 0m; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="decimal"/> is greater than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, decimal>> selector, decimal value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<decimal, bool>> p = val => val > value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="decimal"/> is greater than or equal to <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> GreaterThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<decimal, bool>> p = val => val >= value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="decimal"/> is less than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, decimal>> selector, decimal value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<decimal, bool>> p = val => val < value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="decimal"/> is less than or equal to <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> LessThanOrEqualTo(Expression<Func<T, decimal>> selector, decimal value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<decimal, bool>> p = val => val <= value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="decimal"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> MinValue(Expression<Func<T, decimal>> selector, decimal minValue)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<decimal, bool>> p = val => val >= minValue; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="decimal"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> MaxValue(Expression<Func<T, decimal>> selector, decimal maxValue)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<decimal, bool>> p = val => val <= maxValue; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="decimal"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, decimal>> selector, decimal min, decimal max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<decimal, bool>> p = val => val >= min && val <= max;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="decimal"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, decimal>> selector,
        Expression<Func<T, decimal>> minSelector, Expression<Func<T, decimal>> maxSelector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (minSelector == null) throw new ArgumentNullException(nameof(minSelector));
        if (maxSelector == null) throw new ArgumentNullException(nameof(maxSelector));
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    // ── Numeric — float ───────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="float"/> equals zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Zero(Expression<Func<T, float>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<float, bool>> p = val => val == 0f; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="float"/> does not equal zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> NotZero(Expression<Func<T, float>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<float, bool>> p = val => val != 0f; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="float"/> is greater than zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Positive(Expression<Func<T, float>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<float, bool>> p = val => val > 0f; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="float"/> is less than zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Negative(Expression<Func<T, float>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<float, bool>> p = val => val < 0f; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="float"/> is greater than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, float>> selector, float value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<float, bool>> p = val => val > value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="float"/> is greater than or equal to <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> GreaterThanOrEqualTo(Expression<Func<T, float>> selector, float value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<float, bool>> p = val => val >= value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="float"/> is less than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, float>> selector, float value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<float, bool>> p = val => val < value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="float"/> is less than or equal to <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> LessThanOrEqualTo(Expression<Func<T, float>> selector, float value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<float, bool>> p = val => val <= value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="float"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> MinValue(Expression<Func<T, float>> selector, float minValue)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<float, bool>> p = val => val >= minValue; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="float"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> MaxValue(Expression<Func<T, float>> selector, float maxValue)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<float, bool>> p = val => val <= maxValue; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="float"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, float>> selector, float min, float max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<float, bool>> p = val => val >= min && val <= max;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="float"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, float>> selector,
        Expression<Func<T, float>> minSelector, Expression<Func<T, float>> maxSelector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (minSelector == null) throw new ArgumentNullException(nameof(minSelector));
        if (maxSelector == null) throw new ArgumentNullException(nameof(maxSelector));
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    // ── Numeric — short ───────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="short"/> equals zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Zero(Expression<Func<T, short>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<short, bool>> p = val => val == 0; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="short"/> does not equal zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> NotZero(Expression<Func<T, short>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<short, bool>> p = val => val != 0; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="short"/> is greater than zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Positive(Expression<Func<T, short>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<short, bool>> p = val => val > 0; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="short"/> is less than zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> Negative(Expression<Func<T, short>> selector)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<short, bool>> p = val => val < 0; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="short"/> is greater than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, short>> selector, short value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<short, bool>> p = val => val > value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="short"/> is greater than or equal to <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> GreaterThanOrEqualTo(Expression<Func<T, short>> selector, short value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<short, bool>> p = val => val >= value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="short"/> is less than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, short>> selector, short value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<short, bool>> p = val => val < value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="short"/> is less than or equal to <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> LessThanOrEqualTo(Expression<Func<T, short>> selector, short value)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<short, bool>> p = val => val <= value; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="short"/> is greater than or equal to <paramref name="minValue"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> MinValue(Expression<Func<T, short>> selector, short minValue)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<short, bool>> p = val => val >= minValue; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="short"/> is less than or equal to <paramref name="maxValue"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> MaxValue(Expression<Func<T, short>> selector, short maxValue)
    { if (selector == null) throw new ArgumentNullException(nameof(selector)); Expression<Func<short, bool>> p = val => val <= maxValue; return Add(selector, p); }

    /// <summary>Validates that the selected <see cref="short"/> is within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, short>> selector, short min, short max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<short, bool>> p = val => val >= min && val <= max;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="short"/> is within the inclusive range defined by two entity-bound selectors.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, short>> selector,
        Expression<Func<T, short>> minSelector, Expression<Func<T, short>> maxSelector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (minSelector == null) throw new ArgumentNullException(nameof(minSelector));
        if (maxSelector == null) throw new ArgumentNullException(nameof(maxSelector));
        return AddCrossPropertyRange(selector, minSelector, maxSelector);
    }

    // ── Nullable numeric ──────────────────────────────────────────────────────

    /// <summary>Validates that the selected nullable <see cref="int"/> is null or zero.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> IsNullOrZero(Expression<Func<T, int?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<int?, bool>> p = val => !val.HasValue || val.Value == 0; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="long"/> is null or zero.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> IsNullOrZero(Expression<Func<T, long?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<long?, bool>> p = val => !val.HasValue || val.Value == 0L; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="decimal"/> is null or zero.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> IsNullOrZero(Expression<Func<T, decimal?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<decimal?, bool>> p = val => !val.HasValue || val.Value == 0m; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="double"/> is null or zero.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> IsNullOrZero(Expression<Func<T, double?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<double?, bool>> p = val => !val.HasValue || val.Value == 0.0; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="float"/> is null or equal to zero.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;float&gt;.HasValue</c> and <c>Nullable&lt;float&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> IsNullOrZero(Expression<Func<T, float?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<float?, bool>> predicate = val => !val.HasValue || val.Value == 0f;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable <see cref="int"/> has a value.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> HasValue(Expression<Func<T, int?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<int?, bool>> p = val => val.HasValue; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="long"/> has a value.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> HasValue(Expression<Func<T, long?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<long?, bool>> p = val => val.HasValue; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="decimal"/> has a value.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> HasValue(Expression<Func<T, decimal?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<decimal?, bool>> p = val => val.HasValue; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="double"/> has a value.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> HasValue(Expression<Func<T, double?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<double?, bool>> p = val => val.HasValue; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="float"/> has a value (is not null).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>Nullable.HasValue</c>.</remarks>
    public ValiFlowQuery<T> HasValue(Expression<Func<T, float?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<float?, bool>> predicate = val => val.HasValue;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable <see cref="int"/> has a value greater than <paramref name="value"/>.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, int?>> selector, int value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<int?, bool>> p = val => val.HasValue && val.Value > value; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="long"/> has a value greater than <paramref name="value"/>.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, long?>> selector, long value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<long?, bool>> p = val => val.HasValue && val.Value > value; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="decimal"/> has a value greater than <paramref name="value"/>.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, decimal?>> selector, decimal value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<decimal?, bool>> p = val => val.HasValue && val.Value > value; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="int"/> has a value less than <paramref name="value"/>.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, int?>> selector, int value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<int?, bool>> p = val => val.HasValue && val.Value < value; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="long"/> has a value less than <paramref name="value"/>.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, long?>> selector, long value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<long?, bool>> p = val => val.HasValue && val.Value < value; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="decimal"/> has a value less than <paramref name="value"/>.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, decimal?>> selector, decimal value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<decimal?, bool>> p = val => val.HasValue && val.Value < value; return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="int"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, int?>> selector, int min, int max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<int?, bool>> p = val => val.HasValue && val.Value >= min && val.Value <= max;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="long"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, long?>> selector, long min, long max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<long?, bool>> p = val => val.HasValue && val.Value >= min && val.Value <= max;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="decimal"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;T&gt;.HasValue</c> and <c>Nullable&lt;T&gt;.Value</c>
    /// to SQL <c>IS NULL</c> / <c>IS NOT NULL</c> and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, decimal?>> selector, decimal min, decimal max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<decimal?, bool>> p = val => val.HasValue && val.Value >= min && val.Value <= max;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected nullable <see cref="double"/> has a value greater than <paramref name="value"/>.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;double&gt;.HasValue</c> and <c>Nullable&lt;double&gt;.Value</c>
    /// to SQL IS NOT NULL and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, double?>> selector, double value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<double?, bool>> predicate = val => val.HasValue && val.Value > value;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable <see cref="double"/> has a value less than <paramref name="value"/>.</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;double&gt;.HasValue</c> and <c>Nullable&lt;double&gt;.Value</c>
    /// to SQL IS NOT NULL and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, double?>> selector, double value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<double?, bool>> predicate = val => val.HasValue && val.Value < value;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable <see cref="double"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <remarks>
    /// EF Core translates <c>Nullable&lt;double&gt;.HasValue</c> and <c>Nullable&lt;double&gt;.Value</c>
    /// to SQL IS NOT NULL and comparison operators on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, double?>> selector, double min, double max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<double?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable <see cref="float"/> has a value greater than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>Nullable.HasValue</c> and comparison operators.</remarks>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, float?>> selector, float value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<float?, bool>> predicate = val => val.HasValue && val.Value > value;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable <see cref="float"/> has a value less than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>Nullable.HasValue</c> and comparison operators.</remarks>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, float?>> selector, float value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<float?, bool>> predicate = val => val.HasValue && val.Value < value;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable <see cref="float"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>Nullable.HasValue</c> and comparison operators.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, float?>> selector, float min, float max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<float?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable <see cref="short"/> is null or equal to zero.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>Nullable.HasValue</c> and comparison operators.</remarks>
    public ValiFlowQuery<T> IsNullOrZero(Expression<Func<T, short?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<short?, bool>> predicate = val => !val.HasValue || val.Value == 0;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable <see cref="short"/> has a value (is not null).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>Nullable.HasValue</c>.</remarks>
    public ValiFlowQuery<T> HasValue(Expression<Func<T, short?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<short?, bool>> predicate = val => val.HasValue;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable <see cref="short"/> has a value greater than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>Nullable.HasValue</c> and comparison operators.</remarks>
    public ValiFlowQuery<T> GreaterThan(Expression<Func<T, short?>> selector, short value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<short?, bool>> predicate = val => val.HasValue && val.Value > value;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable <see cref="short"/> has a value less than <paramref name="value"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>Nullable.HasValue</c> and comparison operators.</remarks>
    public ValiFlowQuery<T> LessThan(Expression<Func<T, short?>> selector, short value)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<short?, bool>> predicate = val => val.HasValue && val.Value < value;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected nullable <see cref="short"/> has a value within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>Nullable.HasValue</c> and comparison operators.</remarks>
    public ValiFlowQuery<T> InRange(Expression<Func<T, short?>> selector, short min, short max)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<short?, bool>> predicate = val => val.HasValue && val.Value >= min && val.Value <= max;
        return Add(selector, predicate);
    }

    // ── DateTime ──────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on <paramref name="date"/> (date-only, ignoring time-of-day).</summary>
    /// <remarks>
    /// Implemented as a half-open range (<c>val &gt;= date.Date &amp;&amp; val &lt; date.Date.AddDays(1)</c>)
    /// to avoid using <c>DateTime.Date</c> on the mapped column, which is not universally translatable by EF Core.
    /// </remarks>
    public ValiFlowQuery<T> ExactDate(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var start = date.Date;
        var end = date.Date.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls before <paramref name="date"/> (date-only comparison, ignores time-of-day).</summary>
    /// <remarks>
    /// The <paramref name="date"/> parameter is normalized to midnight (<c>date.Date</c>) at build time.
    /// A value is considered "before" if its full <c>DateTime</c> is less than midnight on <paramref name="date"/>.
    /// This produces the same date-only semantics as the in-memory <c>ValiFlow&lt;T&gt;.BeforeDate</c> overload.
    /// <b>EF Core:</b> Translatable on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> BeforeDate(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var datePart = date.Date;
        Expression<Func<DateTime, bool>> predicate = val => val < datePart;
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls after <paramref name="date"/> (date-only comparison, ignores time-of-day).</summary>
    /// <remarks>
    /// The <paramref name="date"/> parameter is normalized to midnight (<c>date.Date</c>) at build time.
    /// A value is considered "after" if its full <c>DateTime</c> is greater than or equal to midnight on the day after <paramref name="date"/>.
    /// This produces the same date-only semantics as the in-memory <c>ValiFlow&lt;T&gt;.AfterDate</c> overload.
    /// <b>EF Core:</b> Translatable on all major providers.
    /// </remarks>
    public ValiFlowQuery<T> AfterDate(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var datePart = date.Date;
        Expression<Func<DateTime, bool>> predicate = val => val >= datePart.AddDays(1);
        return Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> is before <paramref name="date"/> (full DateTime comparison including time-of-day).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> IsBefore(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTime, bool>> p = val => val < date;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> is after <paramref name="date"/> (full DateTime comparison including time-of-day).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> IsAfter(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTime, bool>> p = val => val > date;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> is on a date between
    /// <paramref name="startDate"/> and <paramref name="endDate"/> (inclusive, date-only).</summary>
    /// <remarks>
    /// Implemented as a half-open range using the midnight boundaries of each date
    /// (<c>val &gt;= startDate.Date &amp;&amp; val &lt; endDate.Date.AddDays(1)</c>)
    /// to avoid using <c>DateTime.Date</c> on the mapped column, which is not universally translatable by EF Core.
    /// </remarks>
    public ValiFlowQuery<T> BetweenDates(Expression<Func<T, DateTime>> selector, DateTime startDate, DateTime endDate)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (endDate.Date < startDate.Date)
            throw new ArgumentOutOfRangeException(nameof(endDate), "endDate must be >= startDate.");
        var start = startDate.Date;
        var end = endDate.Date.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls between two entity-bound date selectors (inclusive, full DateTime comparison).</summary>
    /// <remarks>
    /// This overload compares full <see cref="DateTime"/> values (including time-of-day) without any date normalization.
    /// Time-of-day is significant: a value of 2026-03-28 14:00 is NOT between 2026-03-28 15:00 and 2026-03-30 00:00.
    /// If you need date-only semantics with cross-property selectors, normalize the selectors to midnight before passing them.
    /// </remarks>
    public ValiFlowQuery<T> BetweenDates(Expression<Func<T, DateTime>> selector,
        Expression<Func<T, DateTime>> startDateSelector, Expression<Func<T, DateTime>> endDateSelector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (startDateSelector == null) throw new ArgumentNullException(nameof(startDateSelector));
        if (endDateSelector == null) throw new ArgumentNullException(nameof(endDateSelector));

        var param = selector.Parameters[0];
        var valBody = selector.Body;
        var valBodyClone = new ForceCloneVisitor().Visit(valBody)!;
        var startBody = new ParameterReplacer(startDateSelector.Parameters[0], param).Visit(startDateSelector.Body)!;
        var endBody = new ParameterReplacer(endDateSelector.Parameters[0], param).Visit(endDateSelector.Body)!;

        return Add(Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(Expression.GreaterThanOrEqual(valBody, startBody), Expression.LessThanOrEqual(valBodyClone, endBody)),
            param));
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the same calendar month and year as <paramref name="date"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>.Year</c> and <c>.Month</c> property access.</remarks>
    public ValiFlowQuery<T> SameMonthAs(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTime, bool>> p = val => val.Year == date.Year && val.Month == date.Month;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>.Year</c> property access.</remarks>
    public ValiFlowQuery<T> SameYearAs(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTime, bool>> p = val => val.Year == date.Year;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the specified calendar month (1–12), regardless of year.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>.Month</c> property access.</remarks>
    public ValiFlowQuery<T> IsInMonth(Expression<Func<T, DateTime>> selector, int month)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "month must be between 1 and 12.");
        Expression<Func<DateTime, bool>> p = val => val.Month == month;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the specified calendar year.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>.Year</c> property access.</remarks>
    public ValiFlowQuery<T> IsInYear(Expression<Func<T, DateTime>> selector, int year)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
        Expression<Func<DateTime, bool>> p = val => val.Year == year;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> is in the future (greater than <see cref="DateTime.UtcNow"/>).</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>DateTime.UtcNow</c> translates to <c>GETUTCDATE()</c> / <c>UTC_TIMESTAMP()</c> on SQL Server and PostgreSQL.
    /// <b>Provider note:</b> Not guaranteed to translate on MySQL (Pomelo) or SQLite.
    /// </remarks>
    public ValiFlowQuery<T> FutureDate(Expression<Func<T, DateTime>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTime, bool>> p = val => val > DateTime.UtcNow;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> is in the past (less than <see cref="DateTime.UtcNow"/>).</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>DateTime.UtcNow</c> translates to <c>GETUTCDATE()</c> / <c>UTC_TIMESTAMP()</c> on SQL Server and PostgreSQL.
    /// <b>Provider note:</b> Not guaranteed to translate on MySQL (Pomelo) or SQLite.
    /// </remarks>
    public ValiFlowQuery<T> PastDate(Expression<Func<T, DateTime>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTime, bool>> p = val => val < DateTime.UtcNow;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on a Saturday or Sunday.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>.DayOfWeek</c> translates on SQL Server, PostgreSQL, and MySQL (Pomelo 5.0+).
    /// <b>SQLite note:</b> The SQLite EF Core provider does not translate <c>.DayOfWeek</c> — use <see cref="ValiFlow{T}"/> for in-memory filtering on SQLite.
    /// </remarks>
    public ValiFlowQuery<T> IsWeekend(Expression<Func<T, DateTime>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTime, bool>> p = val =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on a weekday (Monday–Friday).</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>.DayOfWeek</c> translates on SQL Server, PostgreSQL, and MySQL (Pomelo 5.0+).
    /// <b>SQLite note:</b> The SQLite EF Core provider does not translate <c>.DayOfWeek</c>.
    /// </remarks>
    public ValiFlowQuery<T> IsWeekday(Expression<Func<T, DateTime>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTime, bool>> p = val =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on the specified <paramref name="day"/> of the week.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>.DayOfWeek</c> translates on SQL Server, PostgreSQL, and MySQL (Pomelo 5.0+).
    /// <b>SQLite note:</b> The SQLite EF Core provider does not translate <c>.DayOfWeek</c>.
    /// </remarks>
    public ValiFlowQuery<T> IsDayOfWeek(Expression<Func<T, DateTime>> selector, DayOfWeek day)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTime, bool>> p = val => val.DayOfWeek == day;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on today's UTC date.</summary>
    /// <remarks>Boundaries captured at build time (UTC midnight). EF Core safe — range comparison against two constants.</remarks>
    public ValiFlowQuery<T> IsToday(Expression<Func<T, DateTime>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on yesterday's UTC date.</summary>
    /// <remarks>Boundaries captured at build time (UTC midnight). EF Core safe — range comparison against two constants.</remarks>
    public ValiFlowQuery<T> IsYesterday(Expression<Func<T, DateTime>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var start = DateTime.UtcNow.Date.AddDays(-1);
        var end = start.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on tomorrow's UTC date.</summary>
    /// <remarks>Boundaries captured at build time (UTC midnight). EF Core safe — range comparison against two constants.</remarks>
    public ValiFlowQuery<T> IsTomorrow(Expression<Func<T, DateTime>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var start = DateTime.UtcNow.Date.AddDays(1);
        var end = start.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls within the last <paramref name="days"/> UTC calendar days (today excluded).</summary>
    /// <remarks>Boundaries captured at build time. EF Core safe — direct <c>DateTime</c> range comparison.</remarks>
    public ValiFlowQuery<T> InLastDays(Expression<Func<T, DateTime>> selector, int days)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var todayStart = DateTime.UtcNow.Date;
        var fromStart = todayStart.AddDays(-days);
        Expression<Func<DateTime, bool>> p = val => val >= fromStart && val < todayStart;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls within the next <paramref name="days"/> UTC calendar days (today excluded).</summary>
    /// <remarks>Boundaries captured at build time. EF Core safe — direct <c>DateTime</c> range comparison.</remarks>
    public ValiFlowQuery<T> InNextDays(Expression<Func<T, DateTime>> selector, int days)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var tomorrowStart = DateTime.UtcNow.Date.AddDays(1);
        var untilEnd = tomorrowStart.AddDays(days);
        Expression<Func<DateTime, bool>> p = val => val >= tomorrowStart && val < untilEnd;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on the first day of its month (<c>val.Day == 1</c>).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>.Day</c> property access.</remarks>
    public ValiFlowQuery<T> IsFirstDayOfMonth(Expression<Func<T, DateTime>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTime, bool>> p = val => val.Day == 1;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on the last day of its month.</summary>
    /// <remarks>
    /// Implemented by comparing <c>val.Day == DateTime.DaysInMonth(val.Year, val.Month)</c>.
    /// <b>EF Core:</b> <c>DateTime.DaysInMonth</c> is not translatable. The boundary is captured at build time only if
    /// you know the month; for dynamic evaluation use the in-memory <see cref="ValiFlow{T}"/> equivalent.
    /// For a fully EF Core-safe version, use <c>BetweenDates</c> with explicit start/end.
    /// </remarks>
    public ValiFlowQuery<T> IsLastDayOfMonth(Expression<Func<T, DateTime>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTime, bool>> p = val => val.Day == DateTime.DaysInMonth(val.Year, val.Month);
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the specified calendar quarter (1–4).</summary>
    /// <remarks>
    /// Quarter 1 = Jan–Mar, Quarter 2 = Apr–Jun, Quarter 3 = Jul–Sep, Quarter 4 = Oct–Dec.
    /// <b>EF Core:</b> Translatable on all major providers via <c>.Month</c> property access.
    /// </remarks>
    public ValiFlowQuery<T> IsInQuarter(Expression<Func<T, DateTime>> selector, int quarter)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (quarter < 1 || quarter > 4)
            throw new ArgumentOutOfRangeException(nameof(quarter), "quarter must be between 1 and 4.");
        var firstMonth = (quarter - 1) * 3 + 1;
        var lastMonth = firstMonth + 2;
        Expression<Func<DateTime, bool>> p = val => val.Month >= firstMonth && val.Month <= lastMonth;
        return Add(selector, p);
    }

    // ── DateTimeOffset ────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is in the future (greater than <see cref="DateTimeOffset.UtcNow"/>).</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>DateTimeOffset.UtcNow</c> translates to <c>GETUTCDATE()</c> / <c>UTC_TIMESTAMP()</c> on SQL Server and PostgreSQL.
    /// <b>Provider note:</b> Not guaranteed to translate on MySQL (Pomelo) or SQLite. For portable support across all providers,
    /// capture the reference date outside the expression: <c>var now = DateTimeOffset.UtcNow;</c> and use a manual <c>Add()</c> condition instead.
    /// </remarks>
    public ValiFlowQuery<T> FutureDate(Expression<Func<T, DateTimeOffset>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTimeOffset, bool>> p = val => val > DateTimeOffset.UtcNow;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is in the past (less than <see cref="DateTimeOffset.UtcNow"/>).</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>DateTimeOffset.UtcNow</c> translates to <c>GETUTCDATE()</c> / <c>UTC_TIMESTAMP()</c> on SQL Server and PostgreSQL.
    /// <b>Provider note:</b> Not guaranteed to translate on MySQL (Pomelo) or SQLite. For portable support across all providers,
    /// capture the reference date outside the expression: <c>var now = DateTimeOffset.UtcNow;</c> and use a manual <c>Add()</c> condition instead.
    /// </remarks>
    public ValiFlowQuery<T> PastDate(Expression<Func<T, DateTimeOffset>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTimeOffset, bool>> p = val => val < DateTimeOffset.UtcNow;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is before <paramref name="date"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> IsBefore(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTimeOffset, bool>> p = val => val < date;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is after <paramref name="date"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> IsAfter(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTimeOffset, bool>> p = val => val > date;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the inclusive range [<paramref name="from"/>, <paramref name="to"/>].</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> BetweenDates(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset from, DateTimeOffset to)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be >= from.");
        Expression<Func<DateTimeOffset, bool>> p = val => val >= from && val <= to;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified month.</summary>
    /// <remarks>
    /// <b>EF Core semantics divergence:</b> In-memory, <c>DateTimeOffset.Month</c> reflects the stored offset's local month.
    /// EF Core providers (SQL Server, PostgreSQL) translate this to the UTC month, which may differ for values
    /// near month boundaries with non-UTC offsets (e.g., <c>2026-03-31 23:00 +01:00</c> is UTC April but local March).
    /// For UTC-consistent behavior, project to UTC before filtering or use a raw SQL query.
    /// </remarks>
    public ValiFlowQuery<T> IsInMonth(Expression<Func<T, DateTimeOffset>> selector, int month)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "month must be between 1 and 12.");
        Expression<Func<DateTimeOffset, bool>> p = val => val.Month == month;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified year.</summary>
    /// <remarks>
    /// <b>EF Core semantics divergence:</b> In-memory, <c>DateTimeOffset.Year</c> reflects the stored offset's local year.
    /// EF Core providers (SQL Server, PostgreSQL) translate this to the UTC year, which may differ for values
    /// near year boundaries with non-UTC offsets (e.g., <c>2025-12-31 23:00 +02:00</c> is UTC 2026 but local 2025).
    /// For UTC-consistent behavior, project to UTC before filtering or use a raw SQL query.
    /// </remarks>
    public ValiFlowQuery<T> IsInYear(Expression<Func<T, DateTimeOffset>> selector, int year)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
        Expression<Func<DateTimeOffset, bool>> p = val => val.Year == year;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on today's UTC calendar date.</summary>
    /// <remarks>
    /// Boundaries are captured at build time (UTC midnight). EF Core translates this as a range comparison
    /// against two parameter values — compatible with SQL Server, PostgreSQL, MySQL (Pomelo 5+), and SQLite.
    /// </remarks>
    public ValiFlowQuery<T> IsToday(Expression<Func<T, DateTimeOffset>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var todayEnd = todayStart.AddDays(1);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= todayStart && val < todayEnd;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on yesterday's UTC calendar date.</summary>
    /// <remarks>Boundaries are captured at build time (UTC midnight). EF Core safe — range comparison against two parameters.</remarks>
    public ValiFlowQuery<T> IsYesterday(Expression<Func<T, DateTimeOffset>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var yesterdayStart = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(-1), TimeSpan.Zero);
        var yesterdayEnd = yesterdayStart.AddDays(1);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= yesterdayStart && val < yesterdayEnd;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on tomorrow's UTC calendar date.</summary>
    /// <remarks>Boundaries are captured at build time (UTC midnight). EF Core safe — range comparison against two parameters.</remarks>
    public ValiFlowQuery<T> IsTomorrow(Expression<Func<T, DateTimeOffset>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var tomorrowStart = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1), TimeSpan.Zero);
        var tomorrowEnd = tomorrowStart.AddDays(1);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= tomorrowStart && val < tomorrowEnd;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the same UTC calendar date as <paramref name="date"/>.</summary>
    /// <remarks>Boundaries are captured at build time. EF Core safe — range comparison against two parameters.</remarks>
    public ValiFlowQuery<T> ExactDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var dayStart = new DateTimeOffset(date.UtcDateTime.Date, TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= dayStart && val < dayEnd;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is before the UTC calendar date of <paramref name="date"/> (date-only comparison, time ignored).</summary>
    /// <remarks>Boundary captured at build time (UTC midnight of <paramref name="date"/>). EF Core safe.</remarks>
    public ValiFlowQuery<T> BeforeDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var dayStart = new DateTimeOffset(date.UtcDateTime.Date, TimeSpan.Zero);
        Expression<Func<DateTimeOffset, bool>> p = val => val < dayStart;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is after the UTC calendar date of <paramref name="date"/> (date-only comparison, time ignored).</summary>
    /// <remarks>Boundary captured at build time (end of UTC day of <paramref name="date"/>). EF Core safe.</remarks>
    public ValiFlowQuery<T> AfterDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var dayEnd = new DateTimeOffset(date.UtcDateTime.Date.AddDays(1), TimeSpan.Zero);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= dayEnd;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the same calendar year and month as <paramref name="date"/>.</summary>
    /// <remarks><c>.Month</c> and <c>.Year</c> translate to SQL <c>MONTH()</c>/<c>YEAR()</c> — EF Core safe on all major providers.</remarks>
    public ValiFlowQuery<T> SameMonthAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var month = date.Month;
        var year = date.Year;
        Expression<Func<DateTimeOffset, bool>> p = val => val.Month == month && val.Year == year;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    /// <remarks><c>.Year</c> translates to SQL <c>YEAR()</c> — EF Core safe on all major providers.</remarks>
    public ValiFlowQuery<T> SameYearAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var year = date.Year;
        Expression<Func<DateTimeOffset, bool>> p = val => val.Year == year;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the last <paramref name="days"/> UTC calendar days (today excluded).</summary>
    /// <remarks>Boundaries captured at build time (UTC midnight). EF Core safe — range comparison against two parameters.</remarks>
    public ValiFlowQuery<T> InLastDays(Expression<Func<T, DateTimeOffset>> selector, int days)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var fromStart = todayStart.AddDays(-days);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= fromStart && val < todayStart;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the next <paramref name="days"/> UTC calendar days (today excluded).</summary>
    /// <remarks>Boundaries captured at build time (UTC midnight). EF Core safe — range comparison against two parameters.</remarks>
    public ValiFlowQuery<T> InNextDays(Expression<Func<T, DateTimeOffset>> selector, int days)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var tomorrowStart = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1), TimeSpan.Zero);
        var untilEnd = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(days + 1), TimeSpan.Zero);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= tomorrowStart && val < untilEnd;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on a Saturday or Sunday.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>.DayOfWeek</c> translates on SQL Server, PostgreSQL, and MySQL (Pomelo 5.0+).
    /// <b>SQLite note:</b> The SQLite EF Core provider does not translate <c>.DayOfWeek</c>.
    /// Reflects the day of week in the stored offset, not UTC.
    /// </remarks>
    public ValiFlowQuery<T> IsWeekend(Expression<Func<T, DateTimeOffset>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTimeOffset, bool>> p = val =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on a weekday (Monday–Friday).</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>.DayOfWeek</c> translates on SQL Server, PostgreSQL, and MySQL (Pomelo 5.0+).
    /// <b>SQLite note:</b> The SQLite EF Core provider does not translate <c>.DayOfWeek</c>.
    /// </remarks>
    public ValiFlowQuery<T> IsWeekday(Expression<Func<T, DateTimeOffset>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTimeOffset, bool>> p = val =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the specified <paramref name="day"/> of the week.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>.DayOfWeek</c> translates on SQL Server, PostgreSQL, and MySQL (Pomelo 5.0+).
    /// <b>SQLite note:</b> The SQLite EF Core provider does not translate <c>.DayOfWeek</c>.
    /// </remarks>
    public ValiFlowQuery<T> IsDayOfWeek(Expression<Func<T, DateTimeOffset>> selector, DayOfWeek day)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTimeOffset, bool>> p = val => val.DayOfWeek == day;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the first day of its month (<c>val.Day == 1</c>).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>.Day</c> property access.</remarks>
    public ValiFlowQuery<T> IsFirstDayOfMonth(Expression<Func<T, DateTimeOffset>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTimeOffset, bool>> p = val => val.Day == 1;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the last day of its month.</summary>
    /// <remarks>
    /// <c>DateTime.DaysInMonth</c> is not EF Core translatable. For a fully EF Core-safe version, use <c>BetweenDates</c> with explicit start/end.
    /// </remarks>
    public ValiFlowQuery<T> IsLastDayOfMonth(Expression<Func<T, DateTimeOffset>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateTimeOffset, bool>> p = val => val.Day == DateTime.DaysInMonth(val.Year, val.Month);
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified calendar quarter (1–4).</summary>
    /// <remarks>
    /// Quarter 1 = Jan–Mar, Quarter 2 = Apr–Jun, Quarter 3 = Jul–Sep, Quarter 4 = Oct–Dec.
    /// <b>EF Core:</b> Translatable on all major providers via <c>.Month</c> property access.
    /// </remarks>
    public ValiFlowQuery<T> IsInQuarter(Expression<Func<T, DateTimeOffset>> selector, int quarter)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (quarter < 1 || quarter > 4)
            throw new ArgumentOutOfRangeException(nameof(quarter), "quarter must be between 1 and 4.");
        var firstMonth = (quarter - 1) * 3 + 1;
        var lastMonth = firstMonth + 2;
        Expression<Func<DateTimeOffset, bool>> p = val => val.Month >= firstMonth && val.Month <= lastMonth;
        return Add(selector, p);
    }

    // ── DateOnly ──────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="DateOnly"/> is before <paramref name="date"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> IsBefore(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val < date;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is after <paramref name="date"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> IsAfter(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val > date;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is within the inclusive range [<paramref name="from"/>, <paramref name="to"/>].</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> BetweenDates(Expression<Func<T, DateOnly>> selector, DateOnly from, DateOnly to)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be >= from.");
        Expression<Func<DateOnly, bool>> p = val => val >= from && val <= to;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the specified calendar month (1–12), regardless of year.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>.Month</c> property access.</remarks>
    public ValiFlowQuery<T> IsInMonth(Expression<Func<T, DateOnly>> selector, int month)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "month must be between 1 and 12.");
        Expression<Func<DateOnly, bool>> p = val => val.Month == month;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the specified calendar year.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>.Year</c> property access.</remarks>
    public ValiFlowQuery<T> IsInYear(Expression<Func<T, DateOnly>> selector, int year)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
        Expression<Func<DateOnly, bool>> p = val => val.Year == year;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected date is the first day of its month.</summary>
    public ValiFlowQuery<T> IsFirstDayOfMonth(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val.Day == 1;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is the last day of its month.</summary>
    /// <remarks>
    /// Implemented as <c>AddDays(1).Month != Month</c> — avoids <c>DateTime.DaysInMonth</c> which is not EF Core translatable.
    /// Translates to <c>MONTH(DATEADD(day,1,col)) &lt;&gt; MONTH(col)</c> on SQL Server,
    /// <c>EXTRACT(MONTH FROM col + INTERVAL '1 day') &lt;&gt; EXTRACT(MONTH FROM col)</c> on PostgreSQL,
    /// and equivalent expressions on MySQL and SQLite.
    /// </remarks>
    public ValiFlowQuery<T> IsLastDayOfMonth(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val.AddDays(1).Month != val.Month;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the future (after today's UTC date).</summary>
    /// <remarks>Today is captured at build time. EF Core safe — direct <c>DateOnly</c> comparison.</remarks>
    public ValiFlowQuery<T> FutureDate(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Expression<Func<DateOnly, bool>> p = val => val > today;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the past (before today's UTC date).</summary>
    /// <remarks>Today is captured at build time. EF Core safe — direct <c>DateOnly</c> comparison.</remarks>
    public ValiFlowQuery<T> PastDate(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Expression<Func<DateOnly, bool>> p = val => val < today;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals today's UTC date.</summary>
    /// <remarks>Today is captured at build time. EF Core safe — direct <c>DateOnly</c> equality comparison.</remarks>
    public ValiFlowQuery<T> IsToday(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Expression<Func<DateOnly, bool>> p = val => val == today;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals <paramref name="date"/>.</summary>
    /// <remarks>EF Core safe — direct <c>DateOnly</c> equality.</remarks>
    public ValiFlowQuery<T> ExactDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val == date;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly before <paramref name="date"/>.</summary>
    /// <remarks>EF Core safe — direct <c>DateOnly</c> comparison.</remarks>
    public ValiFlowQuery<T> BeforeDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val < date;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly after <paramref name="date"/>.</summary>
    /// <remarks>EF Core safe — direct <c>DateOnly</c> comparison.</remarks>
    public ValiFlowQuery<T> AfterDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val > date;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals yesterday's UTC date.</summary>
    /// <remarks>Yesterday is captured at build time. EF Core safe — direct <c>DateOnly</c> equality.</remarks>
    public ValiFlowQuery<T> IsYesterday(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        Expression<Func<DateOnly, bool>> p = val => val == yesterday;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals tomorrow's UTC date.</summary>
    /// <remarks>Tomorrow is captured at build time. EF Core safe — direct <c>DateOnly</c> equality.</remarks>
    public ValiFlowQuery<T> IsTomorrow(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        Expression<Func<DateOnly, bool>> p = val => val == tomorrow;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the last <paramref name="days"/> UTC days (today excluded).</summary>
    /// <remarks>Boundaries captured at build time. EF Core safe — direct <c>DateOnly</c> range comparison.</remarks>
    public ValiFlowQuery<T> InLastDays(Expression<Func<T, DateOnly>> selector, int days)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Expression<Func<DateOnly, bool>> p = val => val >= from && val < today;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the next <paramref name="days"/> UTC days (today excluded).</summary>
    /// <remarks>Boundaries captured at build time. EF Core safe — direct <c>DateOnly</c> range comparison.</remarks>
    public ValiFlowQuery<T> InNextDays(Expression<Func<T, DateOnly>> selector, int days)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var until = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));
        Expression<Func<DateOnly, bool>> p = val => val >= tomorrow && val <= until;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the same calendar year and month as <paramref name="date"/>.</summary>
    /// <remarks><c>.Month</c> and <c>.Year</c> translate to SQL <c>MONTH()</c>/<c>YEAR()</c> — EF Core safe.</remarks>
    public ValiFlowQuery<T> SameMonthAs(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var month = date.Month;
        var year = date.Year;
        Expression<Func<DateOnly, bool>> p = val => val.Month == month && val.Year == year;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    /// <remarks><c>.Year</c> translates to SQL <c>YEAR()</c> — EF Core safe.</remarks>
    public ValiFlowQuery<T> SameYearAs(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var year = date.Year;
        Expression<Func<DateOnly, bool>> p = val => val.Year == year;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on a Saturday or Sunday.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>.DayOfWeek</c> translates on SQL Server, PostgreSQL, and MySQL (Pomelo 5.0+).
    /// <b>SQLite note:</b> The SQLite EF Core provider does not translate <c>.DayOfWeek</c>.
    /// </remarks>
    public ValiFlowQuery<T> IsWeekend(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on a weekday (Monday–Friday).</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>.DayOfWeek</c> translates on SQL Server, PostgreSQL, and MySQL (Pomelo 5.0+).
    /// <b>SQLite note:</b> The SQLite EF Core provider does not translate <c>.DayOfWeek</c>.
    /// </remarks>
    public ValiFlowQuery<T> IsWeekday(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on the specified <paramref name="dayOfWeek"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>.DayOfWeek</c> translates on SQL Server, PostgreSQL, and MySQL (Pomelo 5.0+).
    /// <b>SQLite note:</b> The SQLite EF Core provider does not translate <c>.DayOfWeek</c>.
    /// </remarks>
    public ValiFlowQuery<T> IsDayOfWeek(Expression<Func<T, DateOnly>> selector, DayOfWeek dayOfWeek)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val.DayOfWeek == dayOfWeek;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the specified calendar quarter (1–4).</summary>
    /// <remarks>
    /// Quarter 1 = Jan–Mar, Quarter 2 = Apr–Jun, Quarter 3 = Jul–Sep, Quarter 4 = Oct–Dec.
    /// <b>EF Core:</b> Translatable on all major providers via <c>.Month</c> property access.
    /// </remarks>
    public ValiFlowQuery<T> IsInQuarter(Expression<Func<T, DateOnly>> selector, int quarter)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (quarter < 1 || quarter > 4)
            throw new ArgumentOutOfRangeException(nameof(quarter), "quarter must be between 1 and 4.");
        var firstMonth = (quarter - 1) * 3 + 1;
        var lastMonth = firstMonth + 2;
        Expression<Func<DateOnly, bool>> p = val => val.Month >= firstMonth && val.Month <= lastMonth;
        return Add(selector, p);
    }

    // ── TimeOnly ──────────────────────────────────────────────────────────────

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is before <paramref name="time"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> IsBefore(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> p = val => val < time;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is after <paramref name="time"/>.</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> IsAfter(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> p = val => val > time;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected time falls within [<paramref name="from"/>, <paramref name="to"/>] (inclusive).</summary>
    /// <remarks>
    /// Midnight-crossing ranges are not supported. For overnight ranges (e.g. 22:00–02:00),
    /// use two separate conditions combined with <c>Or()</c>.
    /// Passing <paramref name="to"/> &lt; <paramref name="from"/> throws <see cref="ArgumentOutOfRangeException"/>.
    /// </remarks>
    public ValiFlowQuery<T> IsBetween(Expression<Func<T, TimeOnly>> selector, TimeOnly from, TimeOnly to)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be >= from. Midnight-crossing ranges are not supported; use Or() to combine two separate conditions.");
        Expression<Func<TimeOnly, bool>> p = val => val >= from && val <= to;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is in the AM (hour &lt; 12).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>.Hour</c> property access.</remarks>
    public ValiFlowQuery<T> IsAM(Expression<Func<T, TimeOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> p = val => val.Hour < 12;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is in the PM (hour &gt;= 12).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>.Hour</c> property access.</remarks>
    public ValiFlowQuery<T> IsPM(Expression<Func<T, TimeOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> p = val => val.Hour >= 12;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="TimeOnly"/> equals <paramref name="time"/> exactly (to the tick).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers.</remarks>
    public ValiFlowQuery<T> IsExactTime(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> p = val => val == time;
        return Add(selector, p);
    }

    /// <summary>Validates that the selected <see cref="TimeOnly"/> falls within the specified hour (0–23).</summary>
    /// <remarks><b>EF Core:</b> Translatable on all major providers via <c>.Hour</c> property access.</remarks>
    public ValiFlowQuery<T> IsInHour(Expression<Func<T, TimeOnly>> selector, int hour)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (hour < 0 || hour > 23)
            throw new ArgumentOutOfRangeException(nameof(hour), "hour must be between 0 and 23.");
        Expression<Func<TimeOnly, bool>> p = val => val.Hour == hour;
        return Add(selector, p);
    }

    // ── DateOnly cross-property BetweenDates ──────────────────────────────────

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls between two entity-bound date selectors (inclusive).</summary>
    public ValiFlowQuery<T> BetweenDates(Expression<Func<T, DateOnly>> selector,
        Expression<Func<T, DateOnly>> fromSelector, Expression<Func<T, DateOnly>> toSelector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (fromSelector == null) throw new ArgumentNullException(nameof(fromSelector));
        if (toSelector == null) throw new ArgumentNullException(nameof(toSelector));
        var param = selector.Parameters[0];
        var valBody = selector.Body;
        var valBodyClone = new ForceCloneVisitor().Visit(valBody)!;
        var fromBody = new ParameterReplacer(fromSelector.Parameters[0], param).Visit(fromSelector.Body)!;
        var toBody = new ParameterReplacer(toSelector.Parameters[0], param).Visit(toSelector.Body)!;
        return Add(Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(Expression.GreaterThanOrEqual(valBody, fromBody), Expression.LessThanOrEqual(valBodyClone, toBody)),
            param));
    }

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
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        var nestedBuilder = new ValiFlowQuery<TProperty>();
        configure(nestedBuilder);
        var nestedExpr = nestedBuilder.Build();
        if (nestedExpr.Body is ConstantExpression { Value: true })
            throw new ArgumentException("The configure action must add at least one condition.", nameof(configure));
        var param = selector.Parameters[0];
        var selectorBody = selector.Body;
        var nestedBody = new ParameterReplacer(nestedExpr.Parameters[0], selectorBody).Visit(nestedExpr.Body)!;
        var selectorBodyForNullCheck = new ForceCloneVisitor().Visit(selectorBody);
        var nullCheck = Expression.NotEqual(selectorBodyForNullCheck, Expression.Constant(null, typeof(TProperty)));
        var combined = Expression.AndAlso(nullCheck, nestedBody);
        return Add(Expression.Lambda<Func<T, bool>>(combined, param));
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
        if (left == null) throw new ArgumentNullException(nameof(left));
        if (right == null) throw new ArgumentNullException(nameof(right));
        var l = left.Build();
        var r = right.Build();
        if (l.Body is ConstantExpression { Value: true }) return r;
        if (r.Body is ConstantExpression { Value: true }) return l;
        var param = l.Parameters[0];
        var rBody = new ParameterReplacer(r.Parameters[0], param).Visit(r.Body)!;
        var body = and ? Expression.AndAlso(l.Body, rBody) : Expression.OrElse(l.Body, rBody);
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    public static Expression<Func<T, bool>> operator &(ValiFlowQuery<T> left, ValiFlowQuery<T> right)
        => Combine(left, right, and: true);

    public static Expression<Func<T, bool>> operator |(ValiFlowQuery<T> left, ValiFlowQuery<T> right)
        => Combine(left, right, and: false);

    public static Expression<Func<T, bool>> operator !(ValiFlowQuery<T> flow)
    {
        if (flow == null) throw new ArgumentNullException(nameof(flow));
        return flow.BuildNegated();
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Builds an inclusive range expression tree from three entity-bound selectors.
    /// Used by all numeric <c>InRange(selector, minSelector, maxSelector)</c> overloads.
    /// </summary>
    private ValiFlowQuery<T> AddCrossPropertyRange<TNum>(
        Expression<Func<T, TNum>> selector,
        Expression<Func<T, TNum>> minSelector,
        Expression<Func<T, TNum>> maxSelector)
    {
        var param = selector.Parameters[0];
        var val = selector.Body;
        var valClone = new ForceCloneVisitor().Visit(val)!;
        var minBody = new ParameterReplacer(minSelector.Parameters[0], param).Visit(minSelector.Body)!;
        var maxBody = new ParameterReplacer(maxSelector.Parameters[0], param).Visit(maxSelector.Body)!;
        return Add(Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(
                Expression.GreaterThanOrEqual(val, minBody),
                Expression.LessThanOrEqual(valClone, maxBody)),
            param));
    }

}
