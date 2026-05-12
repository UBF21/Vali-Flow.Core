using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Fluent builder methods for string state/character-class validations.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder that supports method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being validated.</typeparam>
public interface IStringStateExpression<out TBuilder, T>
{
    /// <summary>Ensures that the selected string is null or empty.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsNullOrEmpty(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string is not null or empty.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsNotNullOrEmpty(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is null or contains only whitespace.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>string.IsNullOrWhiteSpace</c> is not reliably translatable across all EF Core providers. Use with in-memory collections or verify provider support.</remarks>
    TBuilder IsNullOrWhiteSpace(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected string is not null and contains at least one non-whitespace character.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>string.IsNullOrWhiteSpace</c> is not reliably translatable across all EF Core providers. Use with in-memory collections or verify provider support.</remarks>
    TBuilder IsNotNullOrWhiteSpace(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string has no leading or trailing whitespace characters.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>string.Trim()</c> is not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder IsTrimmed(Expression<Func<T, string?>> selector);

    /// <summary>Validates that every character in the selected string is a lowercase letter.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>char.IsLower</c> and <c>Enumerable.All</c> over characters are not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder IsLowerCase(Expression<Func<T, string?>> selector);

    /// <summary>Validates that every character in the selected string is an uppercase letter.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>char.IsUpper</c> and <c>Enumerable.All</c> over characters are not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder IsUpperCase(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string contains only digit characters (0–9).</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>char.IsDigit</c> and <c>Enumerable.All</c> over characters are not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder HasOnlyDigits(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string contains only letter characters.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>char.IsLetter</c> and <c>Enumerable.All</c> over characters are not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder HasOnlyLetters(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string contains at least one letter and at least one digit.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>char.IsLetter</c>/<c>char.IsDigit</c> and <c>Enumerable.Any</c> over characters are not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder HasLettersAndNumbers(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string contains at least one non-alphanumeric (special) character.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks><b>EF Core:</b> <c>char.IsLetterOrDigit</c> and <c>Enumerable.Any</c> over characters are not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder HasSpecialCharacters(Expression<Func<T, string?>> selector);
}
