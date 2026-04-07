using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using System.Text.RegularExpressions;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;
using Vali_Flow.Core.RegularExpressions;
using Vali_Flow.Core.Utils;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

public class StringExpression<TBuilder, T> : IStringExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, IStringExpression<TBuilder, T>, new()
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Regex> _regexCache = new();
    private const int RegexCacheMaxEntries = 1000;
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(10);

    private readonly BaseExpression<TBuilder, T> _builder;

    public StringExpression(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Validates that the selected string has at least <paramref name="minLength"/> characters.</summary>
    public TBuilder MinLength(Expression<Func<T, string?>> selector, int minLength)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (minLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(minLength), "minLength must be at least 1. Use IsNotNullOrEmpty() to require a non-empty string.");
        Expression<Func<string?, bool>> predicate = val => !string.IsNullOrEmpty(val) && val.Length >= minLength;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string has at most <paramref name="maxLength"/> characters.</summary>
    public TBuilder MaxLength(Expression<Func<T, string?>> selector, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (maxLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "maxLength must be at least 1. Use IsNullOrEmpty() to require an empty or null string.");
        Expression<Func<string?, bool>> predicate = val => val == null || val.Length <= maxLength;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string matches the given regular expression <paramref name="pattern"/>.</summary>
    /// <remarks>
    /// Compiled <see cref="Regex"/> instances are cached in a static <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}"/>
    /// keyed by pattern string. The cache is capped at <b>1,000 entries</b>; exceeding this limit throws
    /// <see cref="InvalidOperationException"/>. Avoid passing dynamic or per-request patterns to this method.
    /// For high-cardinality pattern sets, pre-compile a <see cref="Regex"/> and supply a raw predicate via
    /// <c>Add(selector, predicate)</c> instead.
    /// <para>
    /// <b>Thread safety:</b> The cap check uses <c>ConcurrentDictionary.Count</c> and is a soft limit —
    /// under high concurrency the cache may temporarily exceed the cap by a small number of entries.
    /// </para>
    /// <b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder RegexMatch(Expression<Func<T, string?>> selector, string pattern)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(pattern)) throw new ArgumentException("Pattern cannot be empty", nameof(pattern));

        var compiled = GetOrCreateRegex(pattern);
        Expression<Func<string?, bool>> predicate = value =>
            !string.IsNullOrEmpty(value) && compiled.IsMatch(value);
        return _builder.Add(selector, predicate);
    }

    private static Regex GetOrCreateRegex(string pattern)
    {
        if (_regexCache.TryGetValue(pattern, out var existing))
            return existing;

        if (_regexCache.Count >= RegexCacheMaxEntries)
            throw new InvalidOperationException(
                $"RegexMatch cache is full ({RegexCacheMaxEntries} patterns). " +
                "Avoid using RegexMatch with unbounded dynamic patterns.");

        var newRegex = new Regex(pattern, RegexOptions.Compiled, RegexTimeout);
        _regexCache.TryAdd(pattern, newRegex);

        // If TryAdd failed, another thread beat us — return what's now in the cache
        return _regexCache.TryGetValue(pattern, out var added) ? added : newRegex;
    }

    /// <summary>Validates that the selected string is null or empty.</summary>
    public TBuilder IsNullOrEmpty(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => string.IsNullOrEmpty(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string is not null and not empty.</summary>
    public TBuilder IsNotNullOrEmpty(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => !string.IsNullOrEmpty(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Ensures that the selected string is a valid email address.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsEmail(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            !string.IsNullOrEmpty(val) && RegularExpression.EmailPattern.IsMatch(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string ends with <paramref name="value"/>.</summary>
    /// <remarks>Uses <paramref name="comparison"/> for culture-aware matching. Not EF Core translatable when a non-ordinal <see cref="StringComparison"/> is used.</remarks>
    public TBuilder EndsWith(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        }
        Expression<Func<string?, bool>> predicate = val => val != null && val.EndsWith(value, comparison);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string starts with <paramref name="value"/>.</summary>
    /// <remarks>Uses <paramref name="comparison"/> for culture-aware matching. Not EF Core translatable when a non-ordinal <see cref="StringComparison"/> is used.</remarks>
    public TBuilder StartsWith(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        }
        Expression<Func<string?, bool>> predicate = val => val != null && val.StartsWith(value, comparison);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string contains <paramref name="value"/>.</summary>
    /// <remarks>
    /// Uses <c>string.Contains(string, StringComparison)</c> which is <b>not translatable to SQL by EF Core</b>
    /// regardless of the <paramref name="comparison"/> value. Use this method only with in-memory collections
    /// (LINQ-to-Objects). For EF Core queries, use the single-argument <c>string.Contains(string)</c>
    /// overload via a raw expression instead.
    /// A null selector value returns <see langword="false"/>.
    /// </remarks>
    public TBuilder Contains(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);

        Expression<Func<string?, bool>> predicate = val => val != null && val.Contains(value, comparison);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string has exactly <paramref name="length"/> characters.</summary>
    public TBuilder ExactLength(Expression<Func<T, string?>> selector, int length)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "length must be >= 0.");
        Expression<Func<string?, bool>> predicate = val => val != null && val.Length == length;
        return _builder.Add(selector, predicate);
    }

    /// <summary>
    /// Ensures that the selected string is equal to <paramref name="value"/>, ignoring case.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="StringComparison.OrdinalIgnoreCase"/> via <c>string.Equals</c> internally.
    /// <b>EF Core:</b> <c>string.Equals</c> with <see cref="StringComparison"/> is not reliably translatable to SQL across all EF Core providers.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder EqualToIgnoreCase(Expression<Func<T, string?>> selector, string? value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (value == null)
            throw new ArgumentNullException(nameof(value),
                "Use IsNull() or IsNullOrEmpty() to check for null values.");

        var capturedValue = value;
        Expression<Func<string?, bool>> predicate = val =>
            val != null && string.Equals(val, capturedValue, StringComparison.OrdinalIgnoreCase);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Ensures that the selected string has no leading or trailing whitespace.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>string.Trim()</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsTrimmed(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => val != null && val == val.Trim();
        return _builder.Add(selector, predicate);
    }

    /// <summary>Ensures that the selected string contains only digit characters.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>char.IsDigit</c> and <c>Enumerable.All</c> over characters are not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder HasOnlyDigits(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => !string.IsNullOrEmpty(val) && val.All(char.IsDigit);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Ensures that the selected string contains only letter characters.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>char.IsLetter</c> and <c>Enumerable.All</c> over characters are not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder HasOnlyLetters(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => !string.IsNullOrEmpty(val) && val.All(char.IsLetter);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Ensures that the selected string contains both letter and digit characters.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>char.IsLetter</c>/<c>char.IsDigit</c> and <c>Enumerable.Any</c> over characters are not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder HasLettersAndNumbers(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            !string.IsNullOrEmpty(val) && val.Any(char.IsLetter) && val.Any(char.IsDigit);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Ensures that the selected string contains at least one non-alphanumeric character.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>char.IsLetterOrDigit</c> and <c>Enumerable.Any</c> over characters are not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder HasSpecialCharacters(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            !string.IsNullOrEmpty(val) && val.Any(c => !char.IsLetterOrDigit(c));
        return _builder.Add(selector, predicate);
    }

    /// <summary>Ensures that the selected string is valid JSON.</summary>
    /// <remarks>
    /// <b>EF Core:</b> JSON validation via <c>JsonDocument.Parse</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// Uses <see cref="System.Text.Json.JsonDocument.Parse(string, System.Text.Json.JsonDocumentOptions)"/> which accepts any valid JSON value,
    /// including bare primitives such as <c>42</c>, <c>true</c>, and <c>null</c>.
    /// If only JSON objects (<c>{...}</c>) or arrays (<c>[...]</c>) should be accepted,
    /// add an additional <c>IsTrue(p =&gt; p.Name.TrimStart()[0] == '{' || p.Name.TrimStart()[0] == '[')</c> condition.
    /// </remarks>
    public TBuilder IsJson(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => JsonValidation.IsValidJson(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Ensures that the selected string is a valid Base64-encoded string.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsBase64(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            !string.IsNullOrEmpty(val) &&
            val.Length % 4 == 0 &&
            RegularExpression.Base64Pattern.IsMatch(val);

        return _builder.Add(selector, predicate);
    }

    /// <summary>Ensures that the selected string is NOT valid JSON.</summary>
    /// <remarks>
    /// <b>EF Core:</b> JSON validation via <c>JsonDocument.Parse</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder NotJson(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => !JsonValidation.IsValidJson(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Ensures that the selected string is NOT a valid Base64-encoded string.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder NotBase64(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            string.IsNullOrEmpty(val) ||
            val.Length % 4 != 0 ||
            !RegularExpression.Base64Pattern.IsMatch(val);

        return _builder.Add(selector, predicate);
    }

    /// <summary>
    /// Validates that any of the strings selected by <paramref name="selectors"/> contains
    /// any word from <paramref name="value"/>. All selectors are combined into a single OR
    /// expression so the caller's preceding And()/Or() state is honoured.
    /// </summary>
    /// <remarks>
    /// <b>EF Core:</b> This method uses <c>ToLower()</c> and <c>string.Contains(string)</c> internally.
    /// <c>string.ToLower()</c> is not translatable to SQL by EF Core. Use this method only with
    /// in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder Contains(string value, List<Expression<Func<T, string?>>> selectors,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value is null, empty, or contains only whitespace.", nameof(value));

        ArgumentNullException.ThrowIfNull(selectors);

        if (!selectors.Any())
            throw new ArgumentException("Selectors list cannot be empty.", nameof(selectors));

        if (selectors.Any(s => s == null))
            throw new ArgumentException("Selectors list must not contain null entries.", nameof(selectors));

        bool ignoreCase = comparison == StringComparison.OrdinalIgnoreCase
                       || comparison == StringComparison.CurrentCultureIgnoreCase
                       || comparison == StringComparison.InvariantCultureIgnoreCase;

        string[] searchTerms = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (searchTerms.Length == 0)
            throw new ArgumentException("Value contains no searchable terms after splitting.", nameof(value));

        string[] matchTerms = ignoreCase
            ? searchTerms.Select(t =>
                comparison == StringComparison.InvariantCultureIgnoreCase
                    ? t.ToLowerInvariant()
                    : t.ToLower()).ToArray()
            : searchTerms;

        // Build one combined OR expression across all selectors so the entire
        // multi-selector check is added as a single condition, preserving the
        // And/Or state set by the caller.
        var param = selectors[0].Parameters[0];
        Expression? combined = null;

        var toLowerMethod = comparison == StringComparison.InvariantCultureIgnoreCase
            ? typeof(string).GetMethod(nameof(string.ToLowerInvariant), Type.EmptyTypes)!
            : typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;

        var deepCloner = new ForceCloneVisitor();

        foreach (var selector in selectors)
        {
            // GetFreshBody() produces a structurally equal but reference-distinct
            // copy of the selector body on every call, ensuring no single Expression
            // node appears as a child of two different parents in the combined tree.
            Expression GetFreshBody()
            {
                var normalized = ReferenceEquals(selector, selectors[0])
                    ? selector.Body
                    : new ParameterReplacer(selector.Parameters[0], param).Visit(selector.Body)!;
                return deepCloner.Visit(normalized)!;
            }

            var notNull = Expression.NotEqual(GetFreshBody(), Expression.Constant(null, typeof(string)));

            Expression? termMatches = null;
            foreach (var term in matchTerms)
            {
                var matchTarget = ignoreCase
                    ? (Expression)Expression.Call(GetFreshBody(), toLowerMethod)
                    : GetFreshBody();
                var termBody = Expression.Call(matchTarget, containsMethod, Expression.Constant(term));
                termMatches = termMatches == null ? termBody : Expression.OrElse(termMatches, termBody);
            }

            Expression selectorExpr = Expression.AndAlso(notNull, termMatches!);
            combined = combined == null ? selectorExpr : Expression.OrElse(combined, selectorExpr);
        }

        return _builder.Add(Expression.Lambda<Func<T, bool>>(combined!, param));
    }

    /// <summary>Validates that the selected property is a valid URL (http/https).</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsUrl(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            !string.IsNullOrEmpty(val) && RegularExpression.UrlPattern.IsMatch(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected property is a valid E.164 phone number.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsPhoneNumber(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            !string.IsNullOrEmpty(val) && RegularExpression.PhonePattern.IsMatch(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected property is a valid GUID.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsGuid(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            !string.IsNullOrEmpty(val) && RegularExpression.GuidPattern.IsMatch(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that every character in the selected string satisfies <see cref="char.IsUpper(char)"/>.</summary>
    /// <remarks>
    /// Every character must satisfy <see cref="char.IsUpper(char)"/>; digits and punctuation cause the check to fail.
    /// For example, <c>"HELLO-123"</c> returns <see langword="false"/> because <c>'-'</c> and digits are not uppercase letters.
    /// <b>EF Core:</b> <c>char.IsUpper</c> and <c>Enumerable.All</c> over characters are not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsUpperCase(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            !string.IsNullOrEmpty(val) && val.All(char.IsUpper);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that every character in the selected string satisfies <see cref="char.IsLower(char)"/>.</summary>
    /// <remarks>
    /// Every character must satisfy <see cref="char.IsLower(char)"/>; digits and punctuation cause the check to fail.
    /// For example, <c>"hello-123"</c> returns <see langword="false"/> because <c>'-'</c> and digits are not lowercase letters.
    /// <b>EF Core:</b> <c>char.IsLower</c> and <c>Enumerable.All</c> over characters are not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsLowerCase(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            !string.IsNullOrEmpty(val) && val.All(char.IsLower);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string length falls within [<paramref name="min"/>, <paramref name="max"/>] (inclusive).</summary>
    public TBuilder LengthBetween(Expression<Func<T, string?>> selector, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "min must be >= 0.");
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<string?, bool>> predicate = val =>
            val != null && val.Length >= min && val.Length <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string is null, empty, or consists only of whitespace characters.</summary>
    public TBuilder IsNullOrWhiteSpace(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => string.IsNullOrWhiteSpace(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string is not null, not empty, and not whitespace-only.</summary>
    public TBuilder IsNotNullOrWhiteSpace(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => !string.IsNullOrWhiteSpace(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string is a valid credit card number format (Visa, Mastercard, Amex, Discover, Diners, JCB).</summary>
    /// <remarks>
    /// Validates number format only — does <b>not</b> perform Luhn checksum validation.
    /// Use this method only with in-memory collections (LINQ-to-Objects) — regex is not EF Core translatable.
    /// </remarks>
    public TBuilder IsCreditCard(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            val != null && RegularExpression.CreditCardPattern.IsMatch(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string is a valid IPv4 address (e.g. <c>192.168.1.1</c>).</summary>
    /// <remarks>Use this method only with in-memory collections (LINQ-to-Objects) — regex is not EF Core translatable.</remarks>
    public TBuilder IsIPv4(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            val != null && RegularExpression.IPv4Pattern.IsMatch(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string is a valid IPv6 address (full and compressed forms).</summary>
    /// <remarks>Use this method only with in-memory collections (LINQ-to-Objects) — regex is not EF Core translatable.</remarks>
    public TBuilder IsIPv6(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            val != null && RegularExpression.IPv6Pattern.IsMatch(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string is a valid CSS hex color code (<c>#RGB</c> or <c>#RRGGBB</c>).</summary>
    /// <remarks>Use this method only with in-memory collections (LINQ-to-Objects) — regex is not EF Core translatable.</remarks>
    public TBuilder IsHexColor(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            val != null && RegularExpression.HexColorPattern.IsMatch(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string is a valid URL slug (lowercase letters, digits, and hyphens; no leading or trailing hyphens).</summary>
    /// <remarks>Use this method only with in-memory collections (LINQ-to-Objects) — regex is not EF Core translatable.</remarks>
    public TBuilder IsSlug(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val =>
            val != null && RegularExpression.SlugPattern.IsMatch(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string matches the wildcard <paramref name="pattern"/>.
    /// Supports '*' (zero or more characters) and '?' (exactly one character). Case-insensitive by default.</summary>
    /// <remarks>In-memory only — not EF Core translatable.</remarks>
    public TBuilder MatchesWildcard(Expression<Func<T, string?>> selector, string pattern)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(pattern);
        var regexPattern = "(?i)^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
        var compiled = GetOrCreateRegex(regexPattern);
        Expression<Func<string?, bool>> predicate = value => value != null && compiled.IsMatch(value);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected string is one of the allowed <paramref name="values"/> (exact match).</summary>
    /// <remarks>In-memory only — not EF Core translatable due to StringComparison parameter.</remarks>
    public TBuilder IsOneOf(Expression<Func<T, string?>> selector, IReadOnlyCollection<string> values,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count == 0) throw new ArgumentException("values must not be empty.", nameof(values));
        var captured = values.ToArray();
        Expression<Func<string?, bool>> predicate = value =>
            value != null && captured.Any(v => string.Equals(value, v, comparison));
        return _builder.Add(selector, predicate);
    }

}
