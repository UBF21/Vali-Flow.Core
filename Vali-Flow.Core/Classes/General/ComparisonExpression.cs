using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.General;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.General;

/// <summary>
/// Implements <see cref="IComparisonExpression{TBuilder,T}"/> — null checks, equality, enum membership,
/// and default-value guards — by composing condition lambdas into the owning <typeparamref name="TBuilder"/>.
/// </summary>
/// <typeparam name="TBuilder">The concrete fluent builder type that owns this expression component.</typeparam>
/// <typeparam name="T">The entity type being validated or filtered.</typeparam>
public class ComparisonExpression<TBuilder,T> : IComparisonExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    /// <summary>The owning builder to which condition lambdas are delegated via <c>Add</c>.</summary>
    private readonly BaseExpression<TBuilder, T> _builder;

    /// <summary>Initializes the component with the owning builder instance.</summary>
    /// <param name="builder">The fluent builder that receives the generated condition lambdas.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    public ComparisonExpression(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }
    
    /// <summary>Validates that the selected value is not null.</summary>
    /// <typeparam name="TValue">The type of the property being compared.</typeparam>
    /// <remarks>
    /// For a <typeparamref name="TValue"/> that closes over a non-nullable value type
    /// (e.g. <c>int</c>, <c>DateTime</c>, <c>bool</c>, <c>Guid</c>, <c>decimal</c>, any <c>enum</c>),
    /// the value can never be <see langword="null"/> — the condition is trivially <see langword="true"/>.
    /// </remarks>
    public TBuilder NotNull<TValue>(Expression<Func<T, TValue?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var closedType = typeof(TValue?);
        if (closedType.IsValueType && Nullable.GetUnderlyingType(closedType) == null)
        {
            // Unconstrained TValue? erases to TValue at runtime for non-nullable value types
            // (no Nullable<T> wrapper is produced) — the value can never be null.
            // Note: a C# `true` literal lambda is constant-folded by Roslyn into a bare
            // ConstantExpression, which BaseExpression's constant-condition guard
            // (ValidateExpressionBody) rejects — so the body is built via the Expression API
            // (1 == 1 as two runtime ConstantExpression nodes) to avoid that fold.
            var trueParam = Expression.Parameter(typeof(T), "_");
            var trueBody = Expression.Equal(Expression.Constant(1), Expression.Constant(1));
            Expression<Func<T, bool>> alwaysTrue = Expression.Lambda<Func<T, bool>>(trueBody, trueParam);
            return _builder.Add(alwaysTrue);
        }

        var param = Expression.Parameter(closedType, "value");
        var body = Expression.NotEqual(param, Expression.Constant(null, closedType));
        Expression<Func<TValue?, bool>> predicate = Expression.Lambda<Func<TValue?, bool>>(body, param);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is null.</summary>
    /// <typeparam name="TValue">The type of the property being compared.</typeparam>
    /// <remarks>
    /// For a <typeparamref name="TValue"/> that closes over a non-nullable value type,
    /// the value can never be <see langword="null"/> — the condition is trivially <see langword="false"/>.
    /// </remarks>
    public TBuilder Null<TValue>(Expression<Func<T, TValue?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var closedType = typeof(TValue?);
        if (closedType.IsValueType && Nullable.GetUnderlyingType(closedType) == null)
        {
            // See NotNull's comment above re: the Roslyn constant-fold / constant-condition guard.
            var falseParam = Expression.Parameter(typeof(T), "_");
            var falseBody = Expression.NotEqual(Expression.Constant(1), Expression.Constant(1));
            Expression<Func<T, bool>> alwaysFalse = Expression.Lambda<Func<T, bool>>(falseBody, falseParam);
            return _builder.Add(alwaysFalse);
        }

        var param = Expression.Parameter(closedType, "value");
        var body = Expression.Equal(param, Expression.Constant(null, closedType));
        Expression<Func<TValue?, bool>> predicate = Expression.Lambda<Func<TValue?, bool>>(body, param);
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