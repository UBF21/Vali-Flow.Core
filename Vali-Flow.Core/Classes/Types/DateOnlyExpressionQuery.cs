using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

public class DateOnlyExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public DateOnlyExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public TBuilder IsBefore(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val < date;
        return _builder.Add(selector, p);
    }

    public TBuilder IsAfter(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val > date;
        return _builder.Add(selector, p);
    }

    public TBuilder BetweenDates(Expression<Func<T, DateOnly>> selector, DateOnly from, DateOnly to)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be >= from.");
        Expression<Func<DateOnly, bool>> p = val => val >= from && val <= to;
        return _builder.Add(selector, p);
    }

    public TBuilder BetweenDates(Expression<Func<T, DateOnly>> selector,
        Expression<Func<T, DateOnly>> fromSelector, Expression<Func<T, DateOnly>> toSelector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (fromSelector == null) throw new ArgumentNullException(nameof(fromSelector));
        if (toSelector == null) throw new ArgumentNullException(nameof(toSelector));
        var param = selector.Parameters[0];
        var valBody = selector.Body;
        var valBodyClone = new ForceCloneVisitor().Visit(valBody)!;
        var fromBody = new ParameterReplacer(fromSelector.Parameters[0], param).Visit(fromSelector.Body)!;
        var toBody = new ParameterReplacer(toSelector.Parameters[0], param).Visit(toSelector.Body)!;
        return _builder.Add(Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(Expression.GreaterThanOrEqual(valBody, fromBody), Expression.LessThanOrEqual(valBodyClone, toBody)),
            param));
    }

    public TBuilder IsInMonth(Expression<Func<T, DateOnly>> selector, int month)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "month must be between 1 and 12.");
        Expression<Func<DateOnly, bool>> p = val => val.Month == month;
        return _builder.Add(selector, p);
    }

    public TBuilder IsInYear(Expression<Func<T, DateOnly>> selector, int year)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
        Expression<Func<DateOnly, bool>> p = val => val.Year == year;
        return _builder.Add(selector, p);
    }

    public TBuilder IsFirstDayOfMonth(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val.Day == 1;
        return _builder.Add(selector, p);
    }

    public TBuilder IsLastDayOfMonth(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val.AddDays(1).Month != val.Month;
        return _builder.Add(selector, p);
    }

    public TBuilder FutureDate(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Expression<Func<DateOnly, bool>> p = val => val > today;
        return _builder.Add(selector, p);
    }

    public TBuilder PastDate(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Expression<Func<DateOnly, bool>> p = val => val < today;
        return _builder.Add(selector, p);
    }

    public TBuilder IsToday(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Expression<Func<DateOnly, bool>> p = val => val == today;
        return _builder.Add(selector, p);
    }

    public TBuilder ExactDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val == date;
        return _builder.Add(selector, p);
    }

    public TBuilder BeforeDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val < date;
        return _builder.Add(selector, p);
    }

    public TBuilder AfterDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val > date;
        return _builder.Add(selector, p);
    }

    public TBuilder IsYesterday(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        Expression<Func<DateOnly, bool>> p = val => val == yesterday;
        return _builder.Add(selector, p);
    }

    public TBuilder IsTomorrow(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        Expression<Func<DateOnly, bool>> p = val => val == tomorrow;
        return _builder.Add(selector, p);
    }

    public TBuilder InLastDays(Expression<Func<T, DateOnly>> selector, int days)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Expression<Func<DateOnly, bool>> p = val => val >= from && val < today;
        return _builder.Add(selector, p);
    }

    public TBuilder InNextDays(Expression<Func<T, DateOnly>> selector, int days)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var until = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));
        Expression<Func<DateOnly, bool>> p = val => val >= tomorrow && val <= until;
        return _builder.Add(selector, p);
    }

    public TBuilder SameMonthAs(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var month = date.Month;
        var year = date.Year;
        Expression<Func<DateOnly, bool>> p = val => val.Month == month && val.Year == year;
        return _builder.Add(selector, p);
    }

    public TBuilder SameYearAs(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var year = date.Year;
        Expression<Func<DateOnly, bool>> p = val => val.Year == year;
        return _builder.Add(selector, p);
    }

    public TBuilder IsWeekend(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday;
        return _builder.Add(selector, p);
    }

    public TBuilder IsWeekday(Expression<Func<T, DateOnly>> selector)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday;
        return _builder.Add(selector, p);
    }

    public TBuilder IsDayOfWeek(Expression<Func<T, DateOnly>> selector, DayOfWeek dayOfWeek)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        Expression<Func<DateOnly, bool>> p = val => val.DayOfWeek == dayOfWeek;
        return _builder.Add(selector, p);
    }

    public TBuilder IsInQuarter(Expression<Func<T, DateOnly>> selector, int quarter)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (quarter < 1 || quarter > 4)
            throw new ArgumentOutOfRangeException(nameof(quarter), "quarter must be between 1 and 4.");
        var firstMonth = (quarter - 1) * 3 + 1;
        var lastMonth = firstMonth + 2;
        Expression<Func<DateOnly, bool>> p = val => val.Month >= firstMonth && val.Month <= lastMonth;
        return _builder.Add(selector, p);
    }
}
