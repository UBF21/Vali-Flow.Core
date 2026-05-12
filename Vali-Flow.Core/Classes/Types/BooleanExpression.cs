using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;

namespace Vali_Flow.Core.Classes.Types;

/// <summary>
/// Provides boolean validation conditions for a fluent expression builder.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type returned by each fluent method.</typeparam>
/// <typeparam name="T">The entity type being validated.</typeparam>
public class BooleanExpression<TBuilder, T> : IBooleanExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, IBooleanExpression<TBuilder, T>, new()
{
    /// <summary>The parent builder to which each condition is delegated.</summary>
    private readonly BaseExpression<TBuilder, T> _builder;

    /// <summary>Initializes a new instance of <see cref="BooleanExpression{TBuilder,T}"/> with the given parent builder.</summary>
    /// <param name="builder">The parent builder that owns the condition list.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public BooleanExpression(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Validates that the selected boolean value is <c>true</c>.</summary>
    public TBuilder IsTrue(Expression<Func<T, bool>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector);
    }

    /// <summary>Validates that the selected boolean value is <c>false</c>.</summary>
    public TBuilder IsFalse(Expression<Func<T, bool>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<T, bool>> negated = Expression.Lambda<Func<T, bool>>(
            Expression.Not(selector.Body), selector.Parameters);
        return _builder.Add(negated);
    }
}