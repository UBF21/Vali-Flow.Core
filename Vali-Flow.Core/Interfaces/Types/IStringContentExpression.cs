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
    /// <remarks><b>EF Core:</b> The <see cref="StringComparison"/> overload is not reliably translatable. Prefer passing <c>StringComparison.Ordinal</c> or use a raw LIKE expression for EF Core queries.</remarks>
    TBuilder StartsWith(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    /// <summary>Ensures that the selected string ends with the specified value.</summary>
    /// <remarks><b>EF Core:</b> The <see cref="StringComparison"/> overload is not reliably translatable. Prefer passing <c>StringComparison.Ordinal</c> or use a raw LIKE expression for EF Core queries.</remarks>
    TBuilder EndsWith(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    /// <summary>Ensures that the selected string contains the specified value.</summary>
    /// <remarks><b>EF Core:</b> <c>string.Contains(string, StringComparison)</c> is not translatable to SQL regardless of the comparison value. Use with in-memory collections only.</remarks>
    TBuilder Contains(Expression<Func<T, string?>> selector, string value,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    /// <summary>Adds a validation rule to check if any of the specified string properties contain at least one of the given search terms.</summary>
    /// <remarks><b>EF Core:</b> Uses <c>ToLower()</c> and <c>string.Contains(string)</c> internally. <c>ToLower()</c> is not translatable to SQL by EF Core. Use with in-memory collections only.</remarks>
    TBuilder Contains(string value, IEnumerable<Expression<Func<T, string?>>> selectors,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    /// <summary>Ensures that the selected string is equal to the given value, ignoring case.</summary>
    /// <remarks><b>EF Core:</b> <c>string.Equals(string, StringComparison)</c> is not reliably translatable across all EF Core providers. Use with in-memory collections only.</remarks>
    TBuilder EqualToIgnoreCase(Expression<Func<T, string?>> selector, string? value);

    /// <summary>Validates that the selected string is one of the allowed <paramref name="values"/>.</summary>
    /// <remarks><b>EF Core:</b> Uses <c>string.Equals</c> with <see cref="StringComparison"/> which is not translatable to SQL. Use with in-memory collections only.</remarks>
    TBuilder IsOneOf(Expression<Func<T, string?>> selector, IReadOnlyCollection<string> values,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase);
}
