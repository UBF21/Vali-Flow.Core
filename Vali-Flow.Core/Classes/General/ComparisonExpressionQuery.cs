using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;

namespace Vali_Flow.Core.Classes.General;

public class ComparisonExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public ComparisonExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public TBuilder NotNull<TValue>(Expression<Func<T, TValue?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TValue?, bool>> predicate = value => value != null;
        return _builder.Add(selector, predicate);
    }

    public TBuilder Null<TValue>(Expression<Func<T, TValue?>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TValue?, bool>> predicate = value => value == null;
        return _builder.Add(selector, predicate);
    }

    public TBuilder EqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IEquatable<TValue>
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var param = Expression.Parameter(typeof(TValue), "v");
        var body = Expression.Equal(param, Expression.Constant(value, typeof(TValue)));
        Expression<Func<TValue, bool>> predicate = Expression.Lambda<Func<TValue, bool>>(body, param);
        return _builder.Add(selector, predicate);
    }

    public TBuilder NotEqualTo<TValue>(Expression<Func<T, TValue>> selector, TValue value)
        where TValue : IEquatable<TValue>
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var param = Expression.Parameter(typeof(TValue), "v");
        var body = Expression.NotEqual(param, Expression.Constant(value, typeof(TValue)));
        Expression<Func<TValue, bool>> predicate = Expression.Lambda<Func<TValue, bool>>(body, param);
        return _builder.Add(selector, predicate);
    }
}
