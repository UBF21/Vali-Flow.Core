using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for constructing Vali-Flow.Core conditions based on string property evaluations.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder that supports method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being validated.</typeparam>
public interface IStringExpression<out TBuilder, T>
{
     /// <summary>
    /// Ensures that the selected string has a minimum length.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <param name="minLength">The minimum length allowed.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder MinLength(Expression<Func<T, string?>> selector, int minLength);

    /// <summary>
    /// Ensures that the selected string has a maximum length.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <param name="maxLength">The maximum length allowed.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder MaxLength(Expression<Func<T, string?>> selector, int maxLength);

    /// <summary>
    /// Ensures that the selected string matches the given regular expression pattern.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <param name="pattern">The regular expression pattern.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder RegexMatch(Expression<Func<T, string?>> selector, string pattern);

    /// <summary>
    /// Ensures that the selected string is empty.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsNullOrEmpty(Expression<Func<T, string?>> selector);

    /// <summary>
    /// Ensures that the selected string is not empty.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsNotNullOrEmpty(Expression<Func<T, string?>> selector);

    /// <summary>
    /// Ensures that the selected string is a valid email address.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsEmail(Expression<Func<T, string?>> selector);

    /// <summary>
    /// Ensures that the selected string ends with the specified value.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <param name="value">The value that the string should end with.</param>
    /// <param name="comparison">The string comparison rule to use. Defaults to <see cref="StringComparison.OrdinalIgnoreCase"/>.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder EndsWith(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Ensures that the selected string starts with the specified value.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <param name="value">The value that the string should start with.</param>
    /// <param name="comparison">The string comparison rule to use. Defaults to <see cref="StringComparison.OrdinalIgnoreCase"/>.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder StartsWith(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Ensures that the selected string contains the specified value.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <param name="value">The value that the string should contain.</param>
    /// <param name="comparison">Allow values to be case sensitive or case insensitive</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder Contains(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Ensures that the selected string has an exact length.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <param name="length">The exact length required.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder ExactLength(Expression<Func<T, string?>> selector, int length);

    /// <summary>
    /// Ensures that the selected string is equal to the given value, ignoring case.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder EqualToIgnoreCase(Expression<Func<T, string?>> selector, string? value);

    /// <summary>
    /// Ensures that the selected string has no leading or trailing spaces.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsTrimmed(Expression<Func<T, string?>> selector);

    /// <summary>
    /// Ensures that the selected string contains only digits.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder HasOnlyDigits(Expression<Func<T, string?>> selector);

    /// <summary>
    /// Ensures that the selected string contains only letters.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder HasOnlyLetters(Expression<Func<T, string?>> selector);

    /// <summary>
    /// Ensures that the selected string contains both letters and numbers.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder HasLettersAndNumbers(Expression<Func<T, string?>> selector);

    /// <summary>
    /// Ensures that the selected string contains special characters.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder HasSpecialCharacters(Expression<Func<T, string?>> selector);

    /// <summary>
    /// Ensures that the selected string is a valid JSON format.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// <b>EF Core:</b> JSON parsing is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    TBuilder IsJson(Expression<Func<T, string?>> selector);

    /// <summary>
    /// Ensures that the selected string is a valid Base64 encoded string.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// <b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    TBuilder IsBase64(Expression<Func<T, string?>> selector);

    /// <summary>
    /// Ensures that the selected string is not a valid JSON format.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// <b>EF Core:</b> JSON parsing is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    TBuilder NotJson(Expression<Func<T, string?>> selector);

    /// <summary>
    /// Ensures that the selected string is not a valid Base64 encoded string.
    /// </summary>
    /// <param name="selector">Expression to select the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// <b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    TBuilder NotBase64(Expression<Func<T, string?>> selector);

    /// <summary>
    /// Adds a validation rule to check if any of the specified string properties contain at least one of the given search terms.
    /// </summary>
    /// <param name="value">The search string containing one or more words.</param>
    /// <param name="selectors">The expressions selecting the string properties to search within.</param>
    /// <param name="comparison">Allow values to be case sensitive or case insensitive</param>
    /// <returns>The current builder instance with the applied validation rule.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="value"/> is null, empty, or contains only whitespace.
    /// Thrown if <paramref name="selectors"/> is empty.
    /// </exception>
    /// <remarks>
    /// - The search string is split into multiple terms using spaces as delimiters.
    /// - The validation succeeds if at least one term is found in any of the specified properties.
    /// - The comparison is case-insensitive.
    /// </remarks>
    public TBuilder Contains(string value, List<Expression<Func<T, string?>>> selectors,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    /// <summary>Validates that the selected property is a valid URL (http/https).</summary>
    TBuilder IsUrl(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property is a valid E.164 phone number.</summary>
    TBuilder IsPhoneNumber(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property is a valid GUID.</summary>
    TBuilder IsGuid(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property contains only uppercase letters.</summary>
    TBuilder IsUpperCase(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property contains only lowercase letters.</summary>
    TBuilder IsLowerCase(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property length is between <paramref name="min"/> and <paramref name="max"/> (inclusive).</summary>
    TBuilder LengthBetween(Expression<Func<T, string?>> selector, int min, int max);

    /// <summary>Validates that the selected property is null or whitespace.</summary>
    TBuilder IsNullOrWhiteSpace(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property is not null and not whitespace.</summary>
    TBuilder IsNotNullOrWhiteSpace(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is a valid credit card number format.</summary>
    TBuilder IsCreditCard(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is a valid IPv4 address.</summary>
    TBuilder IsIPv4(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is a valid IPv6 address.</summary>
    TBuilder IsIPv6(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is a valid CSS hex color code (#RGB or #RRGGBB).</summary>
    TBuilder IsHexColor(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is a valid URL slug.</summary>
    TBuilder IsSlug(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string matches the wildcard <paramref name="pattern"/>.
    /// Supports '*' (zero or more characters) and '?' (exactly one character). Case-insensitive by default.</summary>
    /// <remarks>In-memory only — not EF Core translatable.</remarks>
    TBuilder MatchesWildcard(Expression<Func<T, string?>> selector, string pattern);

    /// <summary>Validates that the selected string is one of the allowed <paramref name="values"/> (exact match).</summary>
    /// <remarks>In-memory only — not EF Core translatable due to StringComparison parameter.</remarks>
    TBuilder IsOneOf(Expression<Func<T, string?>> selector, IReadOnlyCollection<string> values,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);

}