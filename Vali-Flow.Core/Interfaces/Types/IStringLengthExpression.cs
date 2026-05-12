using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Fluent builder methods for string length validations.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder that supports method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being validated.</typeparam>
public interface IStringLengthExpression<out TBuilder, T>
{
    /// <summary>Ensures that the selected string has at least <paramref name="minLength"/> characters.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="minLength">The minimum number of characters required.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder MinLength(Expression<Func<T, string?>> selector, int minLength);

    /// <summary>Ensures that the selected string has at most <paramref name="maxLength"/> characters. A <c>null</c> value passes.</summary>
    /// <param name="selector">Expression selecting the string property.</param>
    /// <param name="maxLength">The maximum number of characters allowed.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder MaxLength(Expression<Func<T, string?>> selector, int maxLength);

    /// <summary>Ensures that the selected string has exactly <paramref name="length"/> characters.</summary>
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
}
