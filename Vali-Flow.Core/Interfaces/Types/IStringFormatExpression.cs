using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Fluent builder methods for string format and pattern validations.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder that supports method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being validated.</typeparam>
public interface IStringFormatExpression<out TBuilder, T>
{
    /// <summary>Ensures that the selected string is a valid email address.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL. Use with in-memory collections only.</remarks>
    /// <exception cref="System.Text.RegularExpressions.RegexMatchTimeoutException">Thrown when the input causes catastrophic backtracking and exceeds the internal match timeout.</exception>
    TBuilder IsEmail(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is a valid URL (http or https scheme).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL. Use with in-memory collections only.</remarks>
    /// <exception cref="System.Text.RegularExpressions.RegexMatchTimeoutException">Thrown when the input causes catastrophic backtracking and exceeds the internal match timeout.</exception>
    TBuilder IsUrl(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is a valid E.164 phone number (e.g., <c>+15551234567</c>).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL. Use with in-memory collections only.</remarks>
    /// <exception cref="System.Text.RegularExpressions.RegexMatchTimeoutException">Thrown when the input causes catastrophic backtracking and exceeds the internal match timeout.</exception>
    TBuilder IsPhoneNumber(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is a valid GUID (any standard format).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL. Use with in-memory collections only.</remarks>
    /// <exception cref="System.Text.RegularExpressions.RegexMatchTimeoutException">Thrown when the input causes catastrophic backtracking and exceeds the internal match timeout.</exception>
    TBuilder IsGuid(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string is valid JSON.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> JSON parsing is not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder IsJson(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string is NOT valid JSON.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> JSON parsing is not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder NotJson(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string is a valid Base64-encoded string.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> Not translatable to SQL. Use with in-memory collections only.</remarks>
    /// <exception cref="System.Text.RegularExpressions.RegexMatchTimeoutException">Thrown when the input causes catastrophic backtracking and exceeds the internal match timeout.</exception>
    TBuilder IsBase64(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string is NOT a valid Base64-encoded string.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> Not translatable to SQL. Use with in-memory collections only.</remarks>
    /// <exception cref="System.Text.RegularExpressions.RegexMatchTimeoutException">Thrown when the input causes catastrophic backtracking and exceeds the internal match timeout.</exception>
    TBuilder NotBase64(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string matches the given regular expression <paramref name="pattern"/>.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="pattern">The regular expression pattern to match against. Patterns are cached internally.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL. Use with in-memory collections only.</remarks>
    /// <exception cref="System.Text.RegularExpressions.RegexMatchTimeoutException">Thrown when the input causes catastrophic backtracking and exceeds the internal 10-second match timeout.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the internal regex cache has reached its 1,000-pattern limit.</exception>
    TBuilder RegexMatch(Expression<Func<T, string?>> selector, string pattern);

    /// <summary>Validates that the selected string matches the wildcard <paramref name="pattern"/>.
    /// Supports <c>*</c> (zero or more characters) and <c>?</c> (exactly one character). Case-insensitive by default.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="pattern">The wildcard pattern to match against.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>In-memory only — not EF Core translatable.</remarks>
    TBuilder MatchesWildcard(Expression<Func<T, string?>> selector, string pattern);

    /// <summary>Validates that the selected string is a valid credit card number (Luhn-format check).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL. Use with in-memory collections only.</remarks>
    /// <exception cref="System.Text.RegularExpressions.RegexMatchTimeoutException">Thrown when the input causes catastrophic backtracking and exceeds the internal match timeout.</exception>
    TBuilder IsCreditCard(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is a valid IPv4 address (dotted-decimal notation).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL. Use with in-memory collections only.</remarks>
    /// <exception cref="System.Text.RegularExpressions.RegexMatchTimeoutException">Thrown when the input causes catastrophic backtracking and exceeds the internal match timeout.</exception>
    TBuilder IsIPv4(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is a valid IPv6 address.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL. Use with in-memory collections only.</remarks>
    /// <exception cref="System.Text.RegularExpressions.RegexMatchTimeoutException">Thrown when the input causes catastrophic backtracking and exceeds the internal match timeout.</exception>
    TBuilder IsIPv6(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is a valid CSS hex color code (<c>#RGB</c> or <c>#RRGGBB</c>).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL. Use with in-memory collections only.</remarks>
    /// <exception cref="System.Text.RegularExpressions.RegexMatchTimeoutException">Thrown when the input causes catastrophic backtracking and exceeds the internal match timeout.</exception>
    TBuilder IsHexColor(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is a valid URL slug (lowercase letters, digits, and hyphens only).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>Regex.IsMatch</c> is not translatable to SQL. Use with in-memory collections only.</remarks>
    /// <exception cref="System.Text.RegularExpressions.RegexMatchTimeoutException">Thrown when the input causes catastrophic backtracking and exceeds the internal match timeout.</exception>
    TBuilder IsSlug(Expression<Func<T, string?>> selector);
}
