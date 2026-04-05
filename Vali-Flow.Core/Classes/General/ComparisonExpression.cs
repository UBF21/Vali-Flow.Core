using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.General;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.General;

public class ComparisonExpression<TBuilder,T> : IComparisonExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public ComparisonExpression(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }
    
    /// <summary>Validates that the selected value is not null.</summary>
    /// <typeparam name="TValue">The type of the property being compared.</typeparam>
    public TBuilder NotNull<TValue>(Expression<Func<T, TValue?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<TValue?, bool>> predicate = value => value != null;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is null.</summary>
    /// <typeparam name="TValue">The type of the property being compared.</typeparam>
    public TBuilder Null<TValue>(Expression<Func<T, TValue?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<TValue?, bool>> predicate = value => value == null;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value equals <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">The type of the property being compared.</typeparam>
    public TBuilder EqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : IEquatable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        var param = Expression.Parameter(typeof(TValue), "v");
        var body = Expression.Equal(param, Expression.Constant(value, typeof(TValue)));
        Expression<Func<TValue, bool>> predicate = Expression.Lambda<Func<TValue, bool>>(body, param);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value does not equal <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">The type of the property being compared.</typeparam>
    public TBuilder NotEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value) where TValue : IEquatable<TValue>
    {
        ArgumentNullException.ThrowIfNull(selector);
        var param = Expression.Parameter(typeof(TValue), "v");
        var body = Expression.NotEqual(param, Expression.Constant(value, typeof(TValue)));
        Expression<Func<TValue, bool>> predicate = Expression.Lambda<Func<TValue, bool>>(body, param);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <typeparamref name="TEnum"/> value is a defined member of its enum type.</summary>
    /// <typeparam name="TEnum">The enum type. Must be a struct and an Enum.</typeparam>
    /// <param name="selector">Selects the enum property to validate.</param>
    public TBuilder IsInEnum<TEnum>(Expression<Func<T, TEnum>> selector) where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<TEnum, bool>> predicate = val => Enum.IsDefined(typeof(TEnum), val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected property equals <c>default(<typeparamref name="TValue"/>)</c>.</summary>
    /// <typeparam name="TValue">The property type.</typeparam>
    /// <param name="selector">Selects the property to validate.</param>
    public TBuilder IsDefault<TValue>(Expression<Func<T, TValue>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var defaultValue = default(TValue);
        Expression<Func<TValue, bool>> predicate = val => EqualityComparer<TValue>.Default.Equals(val, defaultValue);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected property does NOT equal <c>default(<typeparamref name="TValue"/>)</c>.</summary>
    /// <typeparam name="TValue">The property type.</typeparam>
    /// <param name="selector">Selects the property to validate.</param>
    public TBuilder IsNotDefault<TValue>(Expression<Func<T, TValue>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var defaultValue = default(TValue);
        Expression<Func<TValue, bool>> predicate = val => !EqualityComparer<TValue>.Default.Equals(val, defaultValue);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is null. Alias for <see cref="Null{TValue}"/>.</summary>
    public TBuilder IsNull<TValue>(Expression<Func<T, TValue?>> selector) => Null(selector);

    /// <summary>Validates that the selected value is not null. Alias for <see cref="NotNull{TValue}"/>.</summary>
    public TBuilder IsNotNull<TValue>(Expression<Func<T, TValue?>> selector) => NotNull(selector);
}