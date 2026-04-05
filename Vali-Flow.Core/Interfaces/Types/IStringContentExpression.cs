using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Fluent builder methods for string content/matching validations.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder that supports method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being validated.</typeparam>
public interface IStringContentExpression<out TBuilder, T>
{
    /// <summary>Ensures that the selected string starts with the specified value.</summary>
    TBuilder StartsWith(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    /// <summary>Ensures that the selected string ends with the specified value.</summary>
    TBuilder EndsWith(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    /// <summary>Ensures that the selected string contains the specified value.</summary>
    TBuilder Contains(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    /// <summary>Adds a validation rule to check if any of the specified string properties contain at least one of the given search terms.</summary>
    TBuilder Contains(string value, List<Expression<Func<T, string?>>> selectors,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    /// <summary>Ensures that the selected string is equal to the given value, ignoring case.</summary>
    TBuilder EqualToIgnoreCase(Expression<Func<T, string?>> selector, string? value);

    /// <summary>Validates that the selected string is one of the allowed <paramref name="values"/>.</summary>
    TBuilder IsOneOf(Expression<Func<T, string?>> selector, IReadOnlyCollection<string> values,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);
}
