using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;

namespace Vali_Flow.Core.Classes.Types;

public class TimeOnlyExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public TimeOnlyExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public TBuilder IsBefore(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> p = val => val < time;
        return _builder.Add(selector, p);
    }

    public TBuilder IsAfter(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> p = val => val > time;
        return _builder.Add(selector, p);
    }

    public TBuilder IsBetween(Expression<Func<T, TimeOnly>> selector, TimeOnly from, TimeOnly to)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be >= from. Midnight-crossing ranges are not supported; use Or() to combine two separate conditions.");
        Expression<Func<TimeOnly, bool>> p = val => val >= from && val <= to;
        return _builder.Add(selector, p);
    }

    public TBuilder IsAM(Expression<Func<T, TimeOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> p = val => val.Hour < 12;
        return _builder.Add(selector, p);
    }

    public TBuilder IsPM(Expression<Func<T, TimeOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> p = val => val.Hour >= 12;
        return _builder.Add(selector, p);
    }

    public TBuilder IsExactTime(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> p = val => val == time;
        return _builder.Add(selector, p);
    }

    public TBuilder IsInHour(Expression<Func<T, TimeOnly>> selector, int hour)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (hour < 0 || hour > 23)
            throw new ArgumentOutOfRangeException(nameof(hour), "hour must be between 0 and 23.");
        Expression<Func<TimeOnly, bool>> p = val => val.Hour == hour;
        return _builder.Add(selector, p);
    }
}
