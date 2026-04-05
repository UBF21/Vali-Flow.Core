using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;

namespace Vali_Flow.Core.Classes.Types;

public class DateTimeOffsetExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public DateTimeOffsetExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public TBuilder FutureDate(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val > DateTimeOffset.UtcNow;
        return _builder.Add(selector, p);
    }

    public TBuilder PastDate(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val < DateTimeOffset.UtcNow;
        return _builder.Add(selector, p);
    }

    public TBuilder IsBefore(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val < date;
        return _builder.Add(selector, p);
    }

    public TBuilder IsAfter(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val > date;
        return _builder.Add(selector, p);
    }

    public TBuilder BetweenDates(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset from, DateTimeOffset to)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be >= from.");
        Expression<Func<DateTimeOffset, bool>> p = val => val >= from && val <= to;
        return _builder.Add(selector, p);
    }

    public TBuilder IsInMonth(Expression<Func<T, DateTimeOffset>> selector, int month)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "month must be between 1 and 12.");
        Expression<Func<DateTimeOffset, bool>> p = val => val.Month == month;
        return _builder.Add(selector, p);
    }

    public TBuilder IsInYear(Expression<Func<T, DateTimeOffset>> selector, int year)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
        Expression<Func<DateTimeOffset, bool>> p = val => val.Year == year;
        return _builder.Add(selector, p);
    }

    public TBuilder IsToday(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var todayEnd = todayStart.AddDays(1);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= todayStart && val < todayEnd;
        return _builder.Add(selector, p);
    }

    public TBuilder IsYesterday(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var yesterdayStart = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(-1), TimeSpan.Zero);
        var yesterdayEnd = yesterdayStart.AddDays(1);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= yesterdayStart && val < yesterdayEnd;
        return _builder.Add(selector, p);
    }

    public TBuilder IsTomorrow(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var tomorrowStart = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1), TimeSpan.Zero);
        var tomorrowEnd = tomorrowStart.AddDays(1);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= tomorrowStart && val < tomorrowEnd;
        return _builder.Add(selector, p);
    }

    public TBuilder ExactDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var dayStart = new DateTimeOffset(date.UtcDateTime.Date, TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= dayStart && val < dayEnd;
        return _builder.Add(selector, p);
    }

    [Obsolete("BeforeDate normalizes the boundary to UTC day start (val < date.UtcDateTime.Date at 00:00Z), ignoring time-of-day. IsBefore compares the full DateTimeOffset including time and offset. They are NOT equivalent — choose based on your use case. BeforeDate will be removed in a future version.")]
    public TBuilder BeforeDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var dayStart = new DateTimeOffset(date.UtcDateTime.Date, TimeSpan.Zero);
        Expression<Func<DateTimeOffset, bool>> p = val => val < dayStart;
        return _builder.Add(selector, p);
    }

    [Obsolete("AfterDate normalizes the boundary to UTC next day start (val >= date.UtcDateTime.Date.AddDays(1) at 00:00Z), not val > date. IsAfter compares the full DateTimeOffset including time and offset. They are NOT equivalent — choose based on your use case. AfterDate will be removed in a future version.")]
    public TBuilder AfterDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var dayEnd = new DateTimeOffset(date.UtcDateTime.Date.AddDays(1), TimeSpan.Zero);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= dayEnd;
        return _builder.Add(selector, p);
    }

    public TBuilder SameMonthAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var month = date.Month;
        var year = date.Year;
        Expression<Func<DateTimeOffset, bool>> p = val => val.Month == month && val.Year == year;
        return _builder.Add(selector, p);
    }

    public TBuilder SameYearAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var year = date.Year;
        Expression<Func<DateTimeOffset, bool>> p = val => val.Year == year;
        return _builder.Add(selector, p);
    }

    public TBuilder InLastDays(Expression<Func<T, DateTimeOffset>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var fromStart = todayStart.AddDays(-days);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= fromStart && val < todayStart;
        return _builder.Add(selector, p);
    }

    public TBuilder InNextDays(Expression<Func<T, DateTimeOffset>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var tomorrowStart = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1), TimeSpan.Zero);
        var untilEnd = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(days + 1), TimeSpan.Zero);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= tomorrowStart && val < untilEnd;
        return _builder.Add(selector, p);
    }

    public TBuilder IsWeekend(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday;
        return _builder.Add(selector, p);
    }

    public TBuilder IsWeekday(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday;
        return _builder.Add(selector, p);
    }

    public TBuilder IsDayOfWeek(Expression<Func<T, DateTimeOffset>> selector, DayOfWeek day)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val.DayOfWeek == day;
        return _builder.Add(selector, p);
    }

    public TBuilder IsFirstDayOfMonth(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val.Day == 1;
        return _builder.Add(selector, p);
    }

    public TBuilder IsLastDayOfMonth(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val.Day == DateTime.DaysInMonth(val.Year, val.Month);
        return _builder.Add(selector, p);
    }

    public TBuilder IsInQuarter(Expression<Func<T, DateTimeOffset>> selector, int quarter)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (quarter < 1 || quarter > 4)
            throw new ArgumentOutOfRangeException(nameof(quarter), "quarter must be between 1 and 4.");
        var firstMonth = (quarter - 1) * 3 + 1;
        var lastMonth = firstMonth + 2;
        Expression<Func<DateTimeOffset, bool>> p = val => val.Month >= firstMonth && val.Month <= lastMonth;
        return _builder.Add(selector, p);
    }
}
