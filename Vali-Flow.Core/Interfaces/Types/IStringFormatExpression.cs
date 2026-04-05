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
    TBuilder IsEmail(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property is a valid URL (http/https).</summary>
    TBuilder IsUrl(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property is a valid E.164 phone number.</summary>
    TBuilder IsPhoneNumber(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property is a valid GUID.</summary>
    TBuilder IsGuid(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string is a valid JSON format.</summary>
    /// <remarks><b>EF Core:</b> JSON parsing is not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder IsJson(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string is not a valid JSON format.</summary>
    /// <remarks><b>EF Core:</b> JSON parsing is not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder NotJson(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string is a valid Base64 encoded string.</summary>
    /// <remarks><b>EF Core:</b> Not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder IsBase64(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string is not a valid Base64 encoded string.</summary>
    /// <remarks><b>EF Core:</b> Not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder NotBase64(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string matches the given regular expression pattern.</summary>
    TBuilder RegexMatch(Expression<Func<T, string?>> selector, string pattern);

    /// <summary>Validates that the selected string matches the wildcard <paramref name="pattern"/>.
    /// Supports '*' (zero or more characters) and '?' (exactly one character). Case-insensitive by default.</summary>
    /// <remarks>In-memory only — not EF Core translatable.</remarks>
    TBuilder MatchesWildcard(Expression<Func<T, string?>> selector, string pattern);

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
}
