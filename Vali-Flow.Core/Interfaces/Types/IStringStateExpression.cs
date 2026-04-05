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
    TBuilder IsNullOrEmpty(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string is not null or empty.</summary>
    TBuilder IsNotNullOrEmpty(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property is null or whitespace.</summary>
    /// <remarks><b>EF Core:</b> <c>string.IsNullOrWhiteSpace</c> is not reliably translatable across all EF Core providers. Use with in-memory collections or verify provider support.</remarks>
    TBuilder IsNullOrWhiteSpace(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property is not null and not whitespace.</summary>
    /// <remarks><b>EF Core:</b> <c>string.IsNullOrWhiteSpace</c> is not reliably translatable across all EF Core providers. Use with in-memory collections or verify provider support.</remarks>
    TBuilder IsNotNullOrWhiteSpace(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string has no leading or trailing spaces.</summary>
    /// <remarks><b>EF Core:</b> <c>string.Trim()</c> is not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder IsTrimmed(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property contains only lowercase letters.</summary>
    /// <remarks><b>EF Core:</b> <c>char.IsLower</c> and <c>Enumerable.All</c> over characters are not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder IsLowerCase(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property contains only uppercase letters.</summary>
    /// <remarks><b>EF Core:</b> <c>char.IsUpper</c> and <c>Enumerable.All</c> over characters are not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder IsUpperCase(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string contains only digits.</summary>
    /// <remarks><b>EF Core:</b> <c>char.IsDigit</c> and <c>Enumerable.All</c> over characters are not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder HasOnlyDigits(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string contains only letters.</summary>
    /// <remarks><b>EF Core:</b> <c>char.IsLetter</c> and <c>Enumerable.All</c> over characters are not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder HasOnlyLetters(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string contains both letters and numbers.</summary>
    /// <remarks><b>EF Core:</b> <c>char.IsLetter</c>/<c>char.IsDigit</c> and <c>Enumerable.Any</c> over characters are not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder HasLettersAndNumbers(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string contains special characters.</summary>
    /// <remarks><b>EF Core:</b> <c>char.IsLetterOrDigit</c> and <c>Enumerable.Any</c> over characters are not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder HasSpecialCharacters(Expression<Func<T, string?>> selector);
}
