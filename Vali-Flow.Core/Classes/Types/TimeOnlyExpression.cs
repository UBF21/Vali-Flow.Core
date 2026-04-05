using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;

namespace Vali_Flow.Core.Classes.Types;

public class TimeOnlyExpression<TBuilder, T> : ITimeOnlyExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, ITimeOnlyExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public TimeOnlyExpression(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is strictly before <paramref name="time"/>.</summary>
    public TBuilder IsBefore(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> predicate = val => val < time;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is strictly after <paramref name="time"/>.</summary>
    public TBuilder IsAfter(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> predicate = val => val > time;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="TimeOnly"/> falls within the range [<paramref name="from"/>, <paramref name="to"/>] (inclusive).</summary>
    /// <remarks>
    /// <b>Midnight-crossing ranges are not supported.</b> This method only handles forward ranges where
    /// <paramref name="from"/> &lt;= <paramref name="to"/>. For overnight ranges such as 22:00–02:00,
    /// use two separate conditions combined with <c>Or()</c>:
    /// <code>
    /// builder.IsAfter(x => x.Time, new TimeOnly(22, 0))   // exclusive: > 22:00
    ///        .Or()
    ///        .IsBefore(x => x.Time, new TimeOnly(2, 0))   // exclusive: &lt; 02:00
    /// </code>
    /// Note: <see cref="IsAfter"/> and <see cref="IsBefore"/> use strict comparisons (<c>&gt;</c> / <c>&lt;</c>),
    /// so the boundary values 22:00 and 02:00 themselves are excluded. Shift the boundary by one second if
    /// inclusive endpoints are required (e.g. <c>new TimeOnly(21, 59, 59)</c> / <c>new TimeOnly(2, 0, 1)</c>).
    /// Passing <paramref name="to"/> &lt; <paramref name="from"/> throws <see cref="ArgumentOutOfRangeException"/>.
    /// </remarks>
    public TBuilder IsBetween(Expression<Func<T, TimeOnly>> selector, TimeOnly from, TimeOnly to)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be >= from. Midnight-crossing ranges are not supported; use Or() to combine two separate conditions.");
        Expression<Func<TimeOnly, bool>> predicate = val => val >= from && val <= to;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="TimeOnly"/> falls in the AM half of the day (hour &lt; 12).</summary>
    public TBuilder IsAM(Expression<Func<T, TimeOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> predicate = val => val.Hour < 12;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="TimeOnly"/> falls in the PM half of the day (hour &gt;= 12).</summary>
    public TBuilder IsPM(Expression<Func<T, TimeOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> predicate = val => val.Hour >= 12;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="TimeOnly"/> equals exactly <paramref name="time"/>.</summary>
    public TBuilder IsExactTime(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<TimeOnly, bool>> predicate = val => val == time;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="TimeOnly"/> falls within the specified <paramref name="hour"/> (0–23).</summary>
    public TBuilder IsInHour(Expression<Func<T, TimeOnly>> selector, int hour)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (hour < 0 || hour > 23)
            throw new ArgumentOutOfRangeException(nameof(hour), "hour must be between 0 and 23.");
        Expression<Func<TimeOnly, bool>> predicate = val => val.Hour == hour;
        return _builder.Add(selector, predicate);
    }
}
