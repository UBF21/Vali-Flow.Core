using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;

namespace Vali_Flow.Core.Classes.Types;

public class StringExpressionQuery<TBuilder, T> : IStringExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public StringExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public TBuilder MinLength(Expression<Func<T, string?>> selector, int minLength)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (minLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(minLength), "minLength must be at least 1. Use IsNotNullOrEmpty() to require a non-empty string.");
        Expression<Func<string?, bool>> predicate = val => !string.IsNullOrEmpty(val) && val.Length >= minLength;
        return _builder.Add(selector, predicate);
    }

    public TBuilder MaxLength(Expression<Func<T, string?>> selector, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (maxLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "maxLength must be at least 1. Use IsNullOrEmpty() to require an empty or null string.");
        Expression<Func<string?, bool>> predicate = val => val == null || val.Length <= maxLength;
        return _builder.Add(selector, predicate);
    }

    public TBuilder ExactLength(Expression<Func<T, string?>> selector, int length)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "length must be >= 0.");
        Expression<Func<string?, bool>> predicate = val => val != null && val.Length == length;
        return _builder.Add(selector, predicate);
    }

    public TBuilder LengthBetween(Expression<Func<T, string?>> selector, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "min must be >= 0.");
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<string?, bool>> predicate = val => val != null && val.Length >= min && val.Length <= max;
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsNullOrEmpty(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => string.IsNullOrEmpty(val);
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsNotNullOrEmpty(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => !string.IsNullOrEmpty(val);
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsNullOrWhiteSpace(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => val == null || val.Trim() == "";
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsNotNullOrWhiteSpace(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => val != null && val.Trim() != "";
        return _builder.Add(selector, predicate);
    }

    public TBuilder StartsWith(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val != null && val.StartsWith(value);
        return _builder.Add(selector, predicate);
    }

    public TBuilder EndsWith(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val != null && val.EndsWith(value);
        return _builder.Add(selector, predicate);
    }

    public TBuilder Contains(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length == 0)
            throw new ArgumentException("value cannot be empty for Contains(). An empty string would match every non-null value.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val != null && val.Contains(value);
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsTrimmed(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => val != null && val == val.Trim();
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsLowerCase(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => val != null && val == val.ToLower();
        return _builder.Add(selector, predicate);
    }

    public TBuilder IsUpperCase(Expression<Func<T, string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<string?, bool>> predicate = val => val != null && val == val.ToUpper();
        return _builder.Add(selector, predicate);
    }

    public TBuilder EqualToIgnoreCase(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(value);
        var lower = value.ToLowerInvariant();
        Expression<Func<string?, bool>> predicate = val => val != null && val.ToLower() == lower;
        return _builder.Add(selector, predicate);
    }

    public TBuilder StartsWithIgnoreCase(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        var lower = value.ToLowerInvariant();
        Expression<Func<string?, bool>> predicate = val => val != null && val.ToLower().StartsWith(lower);
        return _builder.Add(selector, predicate);
    }

    public TBuilder EndsWithIgnoreCase(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        var lower = value.ToLowerInvariant();
        Expression<Func<string?, bool>> predicate = val => val != null && val.ToLower().EndsWith(lower);
        return _builder.Add(selector, predicate);
    }

    public TBuilder ContainsIgnoreCase(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        var lower = value.ToLowerInvariant();
        Expression<Func<string?, bool>> predicate = val => val != null && val.ToLower().Contains(lower);
        return _builder.Add(selector, predicate);
    }

    public TBuilder NotContains(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val == null || !val.Contains(value);
        return _builder.Add(selector, predicate);
    }

    public TBuilder NotStartsWith(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val == null || !val.StartsWith(value);
        return _builder.Add(selector, predicate);
    }

    public TBuilder NotEndsWith(Expression<Func<T, string?>> selector, string value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value must not be null or empty.", nameof(value));
        Expression<Func<string?, bool>> predicate = val => val == null || !val.EndsWith(value);
        return _builder.Add(selector, predicate);
    }
}
