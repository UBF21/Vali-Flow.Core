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
    /// <param name="expression">The predicate expression to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder Add(Expression<Func<T, bool>> expression);

    /// <summary>Adds a selector + predicate pair as a condition.</summary>
    /// <typeparam name="TValue">The type of the selected property.</typeparam>
    /// <param name="selector">Expression selecting the property to evaluate.</param>
    /// <param name="predicate">Predicate applied to the selected value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder Add<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<TValue, bool>> predicate);

    /// <summary>Adds a grouped sub-expression (parenthesised block).</summary>
    /// <param name="group">A delegate that configures the sub-expression using a nested builder.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>The conditions configured inside <paramref name="group"/> are combined and wrapped in a single parenthesised sub-expression, equivalent to <c>And().Add(subExpr)</c>.</remarks>
    TBuilder AddSubGroup(Action<IExpression<TBuilder, T>> group);

    /// <summary>Inserts a logical AND operator between the preceding and following conditions.</summary>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder And();

    /// <summary>Inserts a logical OR operator between the preceding and following conditions.</summary>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder Or();

    /// <summary>Conditionally adds a boolean expression when <paramref name="condition"/> is <c>true</c> at builder-configuration time.</summary>
    /// <param name="condition">A static flag evaluated when the builder is configured, not when the expression is executed.</param>
    /// <param name="expression">The predicate expression to add if <paramref name="condition"/> is <c>true</c>.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder AddIf(bool condition, Expression<Func<T, bool>> expression);

    /// <summary>Conditionally adds a selector + predicate pair when <paramref name="condition"/> is <c>true</c> at builder-configuration time.</summary>
    /// <typeparam name="TValue">The type of the selected property.</typeparam>
    /// <param name="condition">A static flag evaluated when the builder is configured.</param>
    /// <param name="selector">Expression selecting the property to evaluate.</param>
    /// <param name="predicate">Predicate applied to the selected value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder AddIf<TValue>(bool condition, Expression<Func<T, TValue>> selector, Expression<Func<TValue, bool>> predicate);

    /// <summary>Adds conditions from <paramref name="then"/> only when <paramref name="condition"/> evaluates to <c>true</c> at entity-evaluation time.</summary>
    /// <param name="condition">A predicate evaluated against each entity instance at runtime.</param>
    /// <param name="then">A delegate that configures the conditions to apply when <paramref name="condition"/> is met.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder When(Expression<Func<T, bool>> condition, Action<IExpression<TBuilder, T>> then);

    /// <summary>Adds conditions from <paramref name="unless"/> only when <paramref name="condition"/> evaluates to <c>false</c> at entity-evaluation time.</summary>
    /// <param name="condition">A predicate evaluated against each entity instance at runtime.</param>
    /// <param name="unless">A delegate that configures the conditions to apply when <paramref name="condition"/> is NOT met.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder Unless(Expression<Func<T, bool>> condition, Action<IExpression<TBuilder, T>> unless);

}
