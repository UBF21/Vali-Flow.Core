using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.General;

namespace Vali_Flow.Core.Classes.General;

/// <summary>
/// EF Core-safe implementation of <see cref="IBooleanExpressionQuery{TBuilder,T}"/> that adds
/// simple boolean condition predicates (<c>IsTrue</c> / <c>IsFalse</c>) to the owning builder.
/// </summary>
/// <remarks>
/// This class intentionally exposes only the EF Core-translatable subset of boolean checks.
/// For the full set (including in-memory-only predicates), use <see cref="Vali_Flow.Core.Classes.Types.BooleanExpression{TBuilder,T}"/>.
/// </remarks>
/// <typeparam name="TBuilder">The concrete fluent builder type that owns this expression component.</typeparam>
/// <typeparam name="T">The entity type being validated or filtered.</typeparam>
public class BooleanExpressionQuery<TBuilder, T> : IBooleanExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    /// <summary>The owning builder to which condition lambdas are delegated via <c>Add</c>.</summary>
    private readonly BaseExpression<TBuilder, T> _builder;

    /// <summary>Initializes the component with the owning builder instance.</summary>
    /// <param name="builder">The fluent builder that receives the generated condition lambdas.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    public BooleanExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Adds a condition that the property selected by <paramref name="selector"/> evaluates to <c>true</c>.</summary>
    /// <param name="selector">Expression that selects the boolean property to check.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsTrue(Expression<Func<T, bool>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector);
    }

    /// <summary>Adds a condition that the property selected by <paramref name="selector"/> evaluates to <c>false</c>.</summary>
    /// <remarks>Internally wraps the selector body in a <see cref="Expression.Not"/> node.</remarks>
    /// <param name="selector">Expression that selects the boolean property to check.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsFalse(Expression<Func<T, bool>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<T, bool>> negated = Expression.Lambda<Func<T, bool>>(
            Expression.Not(selector.Body), selector.Parameters);
        return _builder.Add(negated);
    }
}
