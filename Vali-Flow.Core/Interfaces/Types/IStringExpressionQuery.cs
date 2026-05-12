using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for EF Core-safe string expression conditions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder implementing the interface.</typeparam>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface IStringExpressionQuery<out TBuilder, T>
{
    /// <summary>Validates that the selected string has at least <paramref name="minLength"/> characters.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="minLength">The minimum number of characters required.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder MinLength(Expression<Func<T, string?>> selector, int minLength);

    /// <summary>Validates that the selected string does not exceed <paramref name="maxLength"/> characters. A <c>null</c> value passes.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="maxLength">The maximum number of characters allowed.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder MaxLength(Expression<Func<T, string?>> selector, int maxLength);

    /// <summary>Validates that the selected string has exactly <paramref name="length"/> characters.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="length">The exact number of characters required.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder ExactLength(Expression<Func<T, string?>> selector, int length);

    /// <summary>Validates that the selected string length is between <paramref name="min"/> and <paramref name="max"/> (inclusive).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="min">The minimum number of characters (inclusive).</param>
    /// <param name="max">The maximum number of characters (inclusive).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder LengthBetween(Expression<Func<T, string?>> selector, int min, int max);

    /// <summary>Validates that the selected string is null or empty.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsNullOrEmpty(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is not null and not empty.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsNotNullOrEmpty(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is null or contains only whitespace.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsNullOrWhiteSpace(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is not null and contains at least one non-whitespace character.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsNotNullOrWhiteSpace(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string starts with <paramref name="value"/> (ordinal, case-sensitive).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="value">The prefix to match.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder StartsWith(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string ends with <paramref name="value"/> (ordinal, case-sensitive).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="value">The suffix to match.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder EndsWith(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string contains <paramref name="value"/> (ordinal, case-sensitive).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="value">The substring to search for.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder Contains(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string has no leading or trailing whitespace.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsTrimmed(Expression<Func<T, string?>> selector);

    /// <summary>Validates that every character in the selected string is a lowercase letter.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsLowerCase(Expression<Func<T, string?>> selector);

    /// <summary>Validates that every character in the selected string is an uppercase letter.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsUpperCase(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string equals <paramref name="value"/> (case-insensitive, ordinal).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="value">The value to compare against.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder EqualToIgnoreCase(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string starts with <paramref name="value"/> (case-insensitive, ordinal).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="value">The prefix to match.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder StartsWithIgnoreCase(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string ends with <paramref name="value"/> (case-insensitive, ordinal).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="value">The suffix to match.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder EndsWithIgnoreCase(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string contains <paramref name="value"/> (case-insensitive, ordinal).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="value">The substring to search for.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder ContainsIgnoreCase(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string does <b>not</b> contain <paramref name="value"/> (ordinal, case-sensitive). A <c>null</c> value passes.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="value">The substring that must not be present.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder NotContains(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string does <b>not</b> start with <paramref name="value"/> (ordinal, case-sensitive). A <c>null</c> value passes.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="value">The prefix that must not be present.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder NotStartsWith(Expression<Func<T, string?>> selector, string value);

    /// <summary>Validates that the selected string does <b>not</b> end with <paramref name="value"/> (ordinal, case-sensitive). A <c>null</c> value passes.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="value">The suffix that must not be present.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder NotEndsWith(Expression<Func<T, string?>> selector, string value);
}
