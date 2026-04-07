using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

public class DateTimeExpressionQuery<TBuilder, T> : IDateTimeExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public DateTimeExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public TBuilder ExactDate(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var start = date.Date;
        var end = date.Date.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return _builder.Add(selector, p);
    }

    public TBuilder IsBefore(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val < date;
        return _builder.Add(selector, p);
    }

    public TBuilder IsAfter(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val > date;
        return _builder.Add(selector, p);
    }

    public TBuilder BetweenDates(Expression<Func<T, DateTime>> selector, DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (endDate.Date < startDate.Date)
            throw new ArgumentOutOfRangeException(nameof(endDate), "endDate must be >= startDate.");
        var start = startDate.Date;
        var end = endDate.Date.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return _builder.Add(selector, p);
    }

    public TBuilder BetweenDates(Expression<Func<T, DateTime>> selector,
        Expression<Func<T, DateTime>> startDateSelector, Expression<Func<T, DateTime>> endDateSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(startDateSelector);
        ArgumentNullException.ThrowIfNull(endDateSelector);

        var param = selector.Parameters[0];
        var valBody = selector.Body;
        var valBodyClone = new ForceCloneVisitor().Visit(valBody)!;
        var startBody = new ParameterReplacer(startDateSelector.Parameters[0], param).Visit(startDateSelector.Body)!;
        var endBody = new ParameterReplacer(endDateSelector.Parameters[0], param).Visit(endDateSelector.Body)!;

        return _builder.Add(Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(Expression.GreaterThanOrEqual(valBody, startBody), Expression.LessThanOrEqual(valBodyClone, endBody)),
            param));
    }

    public TBuilder SameMonthAs(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val.Year == date.Year && val.Month == date.Month;
        return _builder.Add(selector, p);
    }

    public TBuilder SameYearAs(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val.Year == date.Year;
        return _builder.Add(selector, p);
    }

    public TBuilder IsInMonth(Expression<Func<T, DateTime>> selector, int month)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "month must be between 1 and 12.");
        Expression<Func<DateTime, bool>> p = val => val.Month == month;
        return _builder.Add(selector, p);
    }

    public TBuilder IsInYear(Expression<Func<T, DateTime>> selector, int year)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
        Expression<Func<DateTime, bool>> p = val => val.Year == year;
        return _builder.Add(selector, p);
    }

    public TBuilder FutureDate(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val > DateTime.UtcNow;
        return _builder.Add(selector, p);
    }

    public TBuilder PastDate(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val < DateTime.UtcNow;
        return _builder.Add(selector, p);
    }

    public TBuilder IsWeekend(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday;
        return _builder.Add(selector, p);
    }

    public TBuilder IsWeekday(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday;
        return _builder.Add(selector, p);
    }

    public TBuilder IsDayOfWeek(Expression<Func<T, DateTime>> selector, DayOfWeek day)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val.DayOfWeek == day;
        return _builder.Add(selector, p);
    }

    public TBuilder IsToday(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return _builder.Add(selector, p);
    }

    public TBuilder IsYesterday(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var start = DateTime.UtcNow.Date.AddDays(-1);
        var end = start.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return _builder.Add(selector, p);
    }

    public TBuilder IsTomorrow(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var start = DateTime.UtcNow.Date.AddDays(1);
        var end = start.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return _builder.Add(selector, p);
    }

    public TBuilder InLastDays(Expression<Func<T, DateTime>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var todayStart = DateTime.UtcNow.Date;
        var fromStart = todayStart.AddDays(-days);
        Expression<Func<DateTime, bool>> p = val => val >= fromStart && val < todayStart;
        return _builder.Add(selector, p);
    }

    public TBuilder InNextDays(Expression<Func<T, DateTime>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var tomorrowStart = DateTime.UtcNow.Date.AddDays(1);
        var untilEnd = tomorrowStart.AddDays(days);
        Expression<Func<DateTime, bool>> p = val => val >= tomorrowStart && val < untilEnd;
        return _builder.Add(selector, p);
    }

    public TBuilder IsFirstDayOfMonth(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val.Day == 1;
        return _builder.Add(selector, p);
    }

    public TBuilder IsLastDayOfMonth(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val.Day == DateTime.DaysInMonth(val.Year, val.Month);
        return _builder.Add(selector, p);
    }

    public TBuilder IsInQuarter(Expression<Func<T, DateTime>> selector, int quarter)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (quarter < 1 || quarter > 4)
            throw new ArgumentOutOfRangeException(nameof(quarter), "quarter must be between 1 and 4.");
        var firstMonth = (quarter - 1) * 3 + 1;
        var lastMonth = firstMonth + 2;
        Expression<Func<DateTime, bool>> p = val => val.Month >= firstMonth && val.Month <= lastMonth;
        return _builder.Add(selector, p);
    }
}
