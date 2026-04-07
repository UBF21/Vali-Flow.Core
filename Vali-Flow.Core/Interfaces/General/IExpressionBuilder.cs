using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.General;

/// <summary>
/// Defines condition-building operations for an expression targeting <typeparamref name="T"/>.
/// Inherits from <see cref="IExpressionAnnotator{TBuilder}"/> for error metadata methods.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type (fluent return type).</typeparam>
/// <typeparam name="T">The entity type being validated.</typeparam>
public interface IExpressionBuilder<out TBuilder, T> : IExpressionAnnotator<TBuilder>
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

}
