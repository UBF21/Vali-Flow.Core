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
    TBuilder IsNullOrWhiteSpace(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property is not null and not whitespace.</summary>
    TBuilder IsNotNullOrWhiteSpace(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string has no leading or trailing spaces.</summary>
    TBuilder IsTrimmed(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property contains only lowercase letters.</summary>
    TBuilder IsLowerCase(Expression<Func<T, string?>> selector);

    /// <summary>Validates that the selected property contains only uppercase letters.</summary>
    TBuilder IsUpperCase(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string contains only digits.</summary>
    TBuilder HasOnlyDigits(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string contains only letters.</summary>
    TBuilder HasOnlyLetters(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string contains both letters and numbers.</summary>
    TBuilder HasLettersAndNumbers(Expression<Func<T, string?>> selector);

    /// <summary>Ensures that the selected string contains special characters.</summary>
    TBuilder HasSpecialCharacters(Expression<Func<T, string?>> selector);
}
