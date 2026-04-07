using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.General;

namespace Vali_Flow.Core.Classes.General;

public class BooleanExpressionQuery<TBuilder, T> : IBooleanExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public BooleanExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public TBuilder IsTrue(Expression<Func<T, bool>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return _builder.Add(selector);
    }

    public TBuilder IsFalse(Expression<Func<T, bool>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<T, bool>> negated = Expression.Lambda<Func<T, bool>>(
            Expression.Not(selector.Body), selector.Parameters);
        return _builder.Add(negated);
    }
}
