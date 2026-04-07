using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for EF Core-safe string expression conditions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder implementing the interface.</typeparam>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface IStringExpressionQuery<out TBuilder, T>
{
    /// <summary>Validates that the selected string has a minimum length.</summary>
    TBuilder MinLength(Expression<Func<T, string?>> selector, int minLength);

    /// <summary>Validates that the selected string does not exceed a maximum length. Null values pass.</summary>
    TBuilder MaxLength(Expression<Func<T, string?>> selector, int maxLength);

    /// <summary>Validates that the selected string has an exact length.</summary>
    TBuilder ExactLength(Expression<Func<T, string?>> selector, int length);

    /// <summary>Validates that the selected string length is between <paramref name="min"/> and <paramref name="max"/> (inclusive).</summary>
    TBuilder LengthBetween(Expression<Func<T, string?>> selector, int min, int max);

    /// <summary>Validates that the selected string is null or empty.</summary>
    TBuilder IsNullOrEmpty(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is not null and not empty.</summary>
    TBuilder IsNotNullOrEmpty(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is null or contains only whitespace.</summary>
    TBuilder IsNullOrWhiteSpace(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is not null and contains at least one non-whitespace character.</summary>
    TBuilder IsNotNullOrWhiteSpace(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string starts with <paramref name="value"/>.</summary>
    TBuilder StartsWith(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string ends with <paramref name="value"/>.</summary>
    TBuilder EndsWith(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string contains <paramref name="value"/>.</summary>
    TBuilder Contains(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string has no leading or trailing whitespace.</summary>
    TBuilder IsTrimmed(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string equals its lowercase form.</summary>
    TBuilder IsLowerCase(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string equals its uppercase form.</summary>
    TBuilder IsUpperCase(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string equals <paramref name="value"/> (case-insensitive, ordinal).</summary>
    TBuilder EqualToIgnoreCase(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string starts with <paramref name="value"/> (case-insensitive, ordinal).</summary>
    TBuilder StartsWithIgnoreCase(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string ends with <paramref name="value"/> (case-insensitive, ordinal).</summary>
    TBuilder EndsWithIgnoreCase(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string contains <paramref name="value"/> (case-insensitive, ordinal).</summary>
    TBuilder ContainsIgnoreCase(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string does <b>not</b> contain <paramref name="value"/>. A <c>null</c> value passes.</summary>
    TBuilder NotContains(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string does <b>not</b> start with <paramref name="value"/>. A <c>null</c> value passes.</summary>
    TBuilder NotStartsWith(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string does <b>not</b> end with <paramref name="value"/>. A <c>null</c> value passes.</summary>
    TBuilder NotEndsWith(Expression<Func<T, string?>> selector, string value);
}
