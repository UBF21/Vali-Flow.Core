using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;

namespace Vali_Flow.Core.Classes.Types;

/// <summary>
/// EF Core-safe string validation conditions for the fluent builder.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type returned by each fluent method.</typeparam>
/// <typeparam name="T">The entity type being validated.</typeparam>
/// <remarks>
/// All predicates in this class use only plain string operations that EF Core can translate
/// to SQL. Methods that rely on <see cref="StringComparison"/> or <c>ToLower()</c>-based
/// case-insensitive comparisons are included here but may not translate in every provider;
/// check your provider documentation before using them in queries.
/// </remarks>
public class StringExpressionQuery<TBuilder, T> : IStringExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    /// <summary>The underlying builder that accumulates conditions.</summary>
    private readonly BaseExpression<TBuilder, T> _builder;

    /// <summary>
    /// Initializes a new instance of <see cref="StringExpressionQuery{TBuilder,T}"/>.
    /// </summary>
    /// <param name="builder">The parent builder that owns the condition list.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public StringExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Adds a condition that the selected string has at least <paramref name="minLength"/> characters.</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="minLength">Minimum required length; must be &gt;= 1.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minLength"/> is less than 1.</exception>
    public TBuilder MinLength(Expression<Func<T, string?>> selector, int minLength)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (minLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(minLength), "minLength must be at least 1. Use IsNotNullOrEmpty() to require a non-empty string.");
        Expression<Func<string?, bool>> predicate = val => !string.IsNullOrEmpty(val) && val.Length >= minLength;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string has at most <paramref name="maxLength"/> characters (null passes).</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="maxLength">Maximum allowed length; must be &gt;= 1.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is less than 1.</exception>
    public TBuilder MaxLength(Expression<Func<T, string?>> selector, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (maxLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "maxLength must be at least 1. Use IsNullOrEmpty() to require an empty or null string.");
        Expression<Func<string?, bool>> predicate = val => val == null || val.Length <= maxLength;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string has exactly <paramref name="length"/> characters.</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="length">Required exact length; must be &gt;= 0.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is negative.</exception>
    public TBuilder ExactLength(Expression<Func<T, string?>> selector, int length)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "length must be >= 0.");
        Expression<Func<string?, bool>> predicate = val => val != null && val.Length == length;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string length falls within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="min">Minimum length; must be &gt;= 0.</param>
    /// <param name="max">Maximum length; must be &gt;= <paramref name="min"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="min"/> is negative or <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public TBuilder LengthBetween(Expression<Func<T, string?>> selector, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "min must be >= 0.");
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<string?, bool>> predicate = val => val != null && val.Length >= min && val.Length <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string is <see langword="null"/> or empty.</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsNullOrEmpty(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => string.IsNullOrEmpty(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string is neither <see langword="null"/> nor empty.</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsNotNullOrEmpty(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => !string.IsNullOrEmpty(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string is <see langword="null"/>, empty, or consists only of whitespace.</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <remarks>Uses <c>val.Trim() == ""</c> instead of <c>string.IsNullOrWhiteSpace</c> for EF Core compatibility.</remarks>
    public TBuilder IsNullOrWhiteSpace(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => val == null || val.Trim() == "";
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string is not <see langword="null"/> and contains at least one non-whitespace character.</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <remarks>Uses <c>val.Trim() != ""</c> instead of <c>!string.IsNullOrWhiteSpace</c> for EF Core compatibility.</remarks>
    public TBuilder IsNotNullOrWhiteSpace(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => val != null && val.Trim() != "";
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string starts with <paramref name="value"/> (ordinal, case-sensitive).</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="value">The prefix to check; must not be null or empty.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or empty.</exception>
    public TBuilder StartsWith(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val != null && val.StartsWith(value);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string ends with <paramref name="value"/> (ordinal, case-sensitive).</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="value">The suffix to check; must not be null or empty.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or empty.</exception>
    public TBuilder EndsWith(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val != null && val.EndsWith(value);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string contains <paramref name="value"/> (ordinal, case-sensitive).</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="value">The substring to search for; must not be null or empty.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> or <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty.</exception>
    public TBuilder Contains(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length == 0)
            throw new ArgumentException("value cannot be empty for Contains(). An empty string would match every non-null value.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val != null && val.Contains(value);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string has no leading or trailing whitespace.</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsTrimmed(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => val != null && val == val.Trim();
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string is entirely lowercase.</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsLowerCase(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => val != null && val == val.ToLower();
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string is entirely uppercase.</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsUpperCase(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => val != null && val == val.ToUpper();
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string equals <paramref name="value"/>, ignoring case.</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="value">The value to compare against; must not be <see langword="null"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> or <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>Uses <c>ToLower()</c> for comparison; may not translate in all EF Core providers.</remarks>
    public TBuilder EqualToIgnoreCase(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        var lower = value.ToLowerInvariant();
        Expression<Func<string?, bool>> predicate = val => val != null && val.ToLower() == lower;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string starts with <paramref name="value"/>, ignoring case.</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="value">The prefix to check; must not be null or empty.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or empty.</exception>
    /// <remarks>Uses <c>ToLower()</c> for comparison; may not translate in all EF Core providers.</remarks>
    public TBuilder StartsWithIgnoreCase(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        var lower = value.ToLowerInvariant();
        Expression<Func<string?, bool>> predicate = val => val != null && val.ToLower().StartsWith(lower);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string ends with <paramref name="value"/>, ignoring case.</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="value">The suffix to check; must not be null or empty.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or empty.</exception>
    /// <remarks>Uses <c>ToLower()</c> for comparison; may not translate in all EF Core providers.</remarks>
    public TBuilder EndsWithIgnoreCase(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        var lower = value.ToLowerInvariant();
        Expression<Func<string?, bool>> predicate = val => val != null && val.ToLower().EndsWith(lower);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string contains <paramref name="value"/>, ignoring case.</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="value">The substring to search for; must not be null or empty.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or empty.</exception>
    /// <remarks>Uses <c>ToLower()</c> for comparison; may not translate in all EF Core providers.</remarks>
    public TBuilder ContainsIgnoreCase(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        var lower = value.ToLowerInvariant();
        Expression<Func<string?, bool>> predicate = val => val != null && val.ToLower().Contains(lower);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string does not contain <paramref name="value"/> (null passes).</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="value">The substring that must be absent; must not be null or empty.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or empty.</exception>
    public TBuilder NotContains(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val == null || !val.Contains(value);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string does not start with <paramref name="value"/> (null passes).</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="value">The prefix that must be absent; must not be null or empty.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or empty.</exception>
    public TBuilder NotStartsWith(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val == null || !val.StartsWith(value);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Adds a condition that the selected string does not end with <paramref name="value"/> (null passes).</summary>
    /// <param name="selector">Property selector for the string member.</param>
    /// <param name="value">The suffix that must be absent; must not be null or empty.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or empty.</exception>
    public TBuilder NotEndsWith(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val == null || !val.EndsWith(value);
        return _builder.Add(selector, predicate);
    }
}
