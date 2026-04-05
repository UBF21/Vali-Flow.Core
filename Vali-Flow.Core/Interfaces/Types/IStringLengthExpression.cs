using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Fluent builder methods for string length validations.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder that supports method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being validated.</typeparam>
public interface IStringLengthExpression<out TBuilder, T>
{
    /// <summary>Ensures that the selected string has a minimum length.</summary>
    TBuilder MinLength(Expression<Func<T, string?>> selector, int minLength);

    /// <summary>Ensures that the selected string has a maximum length.</summary>
    TBuilder MaxLength(Expression<Func<T, string?>> selector, int maxLength);

    /// <summary>Ensures that the selected string has an exact length.</summary>
    TBuilder ExactLength(Expression<Func<T, string?>> selector, int length);

    /// <summary>Validates that the selected property length is between <paramref name="min"/> and <paramref name="max"/> (inclusive).</summary>
    TBuilder LengthBetween(Expression<Func<T, string?>> selector, int min, int max);
}
