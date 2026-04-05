using System.Linq.Expressions;
using Vali_Flow.Core.Builder;
using Vali_Flow.Core.Models;

namespace Vali_Flow.Core.Interfaces.General;

/// <summary>
/// Defines condition-building operations for an expression targeting <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type (fluent return type).</typeparam>
/// <typeparam name="T">The entity type being validated.</typeparam>
public interface IExpressionBuilder<out TBuilder, T>
{
    /// <summary>Adds a raw boolean expression as a condition.</summary>
    TBuilder Add(Expression<Func<T, bool>> expression);

    /// <summary>Adds a selector + predicate pair as a condition.</summary>
    TBuilder Add<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<TValue, bool>> predicate);

    /// <summary>Adds a grouped sub-expression (parenthesised block).</summary>
    TBuilder AddSubGroup(Action<IExpression<TBuilder, T>> group);

    /// <summary>Inserts a logical AND operator between the preceding and following conditions.</summary>
    TBuilder And();

    /// <summary>Inserts a logical OR operator between the preceding and following conditions.</summary>
    TBuilder Or();

    /// <summary>Conditionally adds a boolean expression.</summary>
    TBuilder AddIf(bool condition, Expression<Func<T, bool>> expression);

    /// <summary>Conditionally adds a selector + predicate pair.</summary>
    TBuilder AddIf<TValue>(bool condition, Expression<Func<T, TValue>> selector, Expression<Func<TValue, bool>> predicate);

    /// <summary>Adds conditions from <paramref name="then"/> only when <paramref name="condition"/> is <c>true</c> at runtime.</summary>
    TBuilder When(Expression<Func<T, bool>> condition, Action<IExpression<TBuilder, T>> then);

    /// <summary>Adds conditions from <paramref name="unless"/> only when <paramref name="condition"/> is <c>false</c> at runtime.</summary>
    TBuilder Unless(Expression<Func<T, bool>> condition, Action<IExpression<TBuilder, T>> unless);

    /// <summary>Validates a nested reference-type property using an inner <see cref="ValiFlow{TProperty}"/>.</summary>
    TBuilder ValidateNested<TProperty>(
        Expression<Func<T, TProperty>> selector,
        Action<ValiFlow<TProperty>> configure)
        where TProperty : class;

    /// <summary>Attaches a human-readable message to the most recently added condition.</summary>
    TBuilder WithMessage(string message);

    /// <summary>Attaches an error code and message to the most recently added condition.</summary>
    TBuilder WithError(string errorCode, string message);

    /// <summary>Attaches an error code, message, and severity to the most recently added condition.</summary>
    TBuilder WithError(string errorCode, string message, Severity severity);

    /// <summary>Attaches an error code, message, and property path to the most recently added condition.</summary>
    TBuilder WithError(string errorCode, string message, string propertyPath);

    /// <summary>Attaches an error code, message, property path, and severity to the most recently added condition.</summary>
    TBuilder WithError(string errorCode, string message, string propertyPath, Severity severity);

    /// <summary>Sets the severity of the most recently added condition's error metadata.</summary>
    TBuilder WithSeverity(Severity severity);
}
