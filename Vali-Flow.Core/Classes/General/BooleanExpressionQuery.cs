using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;

namespace Vali_Flow.Core.Classes.General;

public class BooleanExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public BooleanExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public TBuilder IsTrue(Expression<Func<T, bool>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        return _builder.Add(selector);
    }

    public TBuilder IsFalse(Expression<Func<T, bool>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<T, bool>> negated = Expression.Lambda<Func<T, bool>>(
            Expression.Not(selector.Body), selector.Parameters);
        return _builder.Add(negated);
    }
}
