using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;

namespace Vali_Flow.Core.Classes.Types;

public class TimeOnlyExpressionQuery<TBuilder, T> : ITimeOnlyExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public TimeOnlyExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public TBuilder IsBefore(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<TimeOnly, bool>> p = val => val < time;
        return _builder.Add(selector, p);
    }

    public TBuilder IsAfter(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<TimeOnly, bool>> p = val => val > time;
        return _builder.Add(selector, p);
    }

    public TBuilder IsBetween(Expression<Func<T, TimeOnly>> selector, TimeOnly from, TimeOnly to)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be >= from. Midnight-crossing ranges are not supported; use Or() to combine two separate conditions.");
        Expression<Func<TimeOnly, bool>> p = val => val >= from && val <= to;
        return _builder.Add(selector, p);
    }

    public TBuilder IsAM(Expression<Func<T, TimeOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<TimeOnly, bool>> p = val => val.Hour < 12;
        return _builder.Add(selector, p);
    }

    public TBuilder IsPM(Expression<Func<T, TimeOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<TimeOnly, bool>> p = val => val.Hour >= 12;
        return _builder.Add(selector, p);
    }

    public TBuilder IsExactTime(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<TimeOnly, bool>> p = val => val == time;
        return _builder.Add(selector, p);
    }

    public TBuilder IsInHour(Expression<Func<T, TimeOnly>> selector, int hour)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (hour < 0 || hour > 23)
            throw new ArgumentOutOfRangeException(nameof(hour), "hour must be between 0 and 23.");
        Expression<Func<TimeOnly, bool>> p = val => val.Hour == hour;
        return _builder.Add(selector, p);
    }
}
