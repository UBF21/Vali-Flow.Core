using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;

namespace Vali_Flow.Core.Classes.Types;

/// <summary>
/// EF Core-safe <see cref="DateTimeOffset"/> validation conditions for the fluent builder.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type returned by each fluent method.</typeparam>
/// <typeparam name="T">The entity type being validated.</typeparam>
/// <remarks>
/// Day-boundary timestamps use UTC with offset zero (<c>TimeSpan.Zero</c>).
/// Comparisons are performed in UTC; stored values with non-zero offsets are normalized
/// by the database provider before comparison.
/// </remarks>
public class DateTimeOffsetExpressionQuery<TBuilder, T> : IDateTimeOffsetExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    /// <summary>The underlying builder that accumulates conditions.</summary>
    private readonly BaseExpression<TBuilder, T> _builder;

    /// <summary>
    /// Initializes a new instance of <see cref="DateTimeOffsetExpressionQuery{TBuilder,T}"/>.
    /// </summary>
    /// <param name="builder">The parent builder that owns the condition list.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public DateTimeOffsetExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> is strictly after <see cref="DateTimeOffset.UtcNow"/> at condition-build time.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder FutureDate(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val > DateTimeOffset.UtcNow;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> is strictly before <see cref="DateTimeOffset.UtcNow"/> at condition-build time.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder PastDate(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val < DateTimeOffset.UtcNow;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> is strictly before <paramref name="date"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <param name="date">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsBefore(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val < date;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> is strictly after <paramref name="date"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <param name="date">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsAfter(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val > date;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls within [<paramref name="from"/>, <paramref name="to"/>] (inclusive).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <param name="from">The inclusive lower bound.</param>
    /// <param name="to">The inclusive upper bound; must be &gt;= <paramref name="from"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="to"/> is before <paramref name="from"/>.</exception>
    public TBuilder BetweenDates(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset from, DateTimeOffset to)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be >= from.");
        Expression<Func<DateTimeOffset, bool>> p = val => val >= from && val <= to;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls in calendar month <paramref name="month"/> (any year).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <param name="month">Month number (1–12).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="month"/> is outside 1–12.</exception>
    public TBuilder IsInMonth(Expression<Func<T, DateTimeOffset>> selector, int month)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "month must be between 1 and 12.");
        Expression<Func<DateTimeOffset, bool>> p = val => val.Month == month;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls in the specified calendar <paramref name="year"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <param name="year">The calendar year (1–9999).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="year"/> is outside 1–9999.</exception>
    public TBuilder IsInYear(Expression<Func<T, DateTimeOffset>> selector, int year)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
        Expression<Func<DateTimeOffset, bool>> p = val => val.Year == year;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls on today's UTC date (midnight UTC-inclusive, next-midnight-exclusive).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsToday(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var todayEnd = todayStart.AddDays(1);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= todayStart && val < todayEnd;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls on yesterday's UTC date.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsYesterday(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var yesterdayStart = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(-1), TimeSpan.Zero);
        var yesterdayEnd = yesterdayStart.AddDays(1);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= yesterdayStart && val < yesterdayEnd;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls on tomorrow's UTC date.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsTomorrow(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var tomorrowStart = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1), TimeSpan.Zero);
        var tomorrowEnd = tomorrowStart.AddDays(1);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= tomorrowStart && val < tomorrowEnd;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls on the calendar date of <paramref name="date"/> in UTC (midnight UTC-inclusive, next-midnight-exclusive).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <param name="date">The target date; time and offset components are normalized to UTC date.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder ExactDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var dayStart = new DateTimeOffset(date.UtcDateTime.Date, TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= dayStart && val < dayEnd;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls in the same year and month as <paramref name="date"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <param name="date">The reference date.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder SameMonthAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var month = date.Month;
        var year = date.Year;
        Expression<Func<DateTimeOffset, bool>> p = val => val.Month == month && val.Year == year;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls in the same year as <paramref name="date"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <param name="date">The reference date.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder SameYearAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var year = date.Year;
        Expression<Func<DateTimeOffset, bool>> p = val => val.Year == year;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls within the last <paramref name="days"/> full UTC days (today excluded).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <param name="days">Number of past days to include; must be &gt; 0.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is not positive.</exception>
    public TBuilder InLastDays(Expression<Func<T, DateTimeOffset>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var fromStart = todayStart.AddDays(-days);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= fromStart && val < todayStart;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls within the next <paramref name="days"/> full UTC days (today excluded).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <param name="days">Number of future days to include; must be &gt; 0.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is not positive.</exception>
    public TBuilder InNextDays(Expression<Func<T, DateTimeOffset>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var tomorrowStart = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1), TimeSpan.Zero);
        var untilEnd = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(days + 1), TimeSpan.Zero);
        Expression<Func<DateTimeOffset, bool>> p = val => val >= tomorrowStart && val < untilEnd;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls on a Saturday or Sunday.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsWeekend(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls on Monday through Friday.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsWeekday(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls on the specified <paramref name="day"/> of the week.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <param name="day">The required day of the week.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsDayOfWeek(Expression<Func<T, DateTimeOffset>> selector, DayOfWeek day)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val.DayOfWeek == day;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> is the first day of its month (day == 1).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsFirstDayOfMonth(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val.Day == 1;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> is the last day of its month.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsLastDayOfMonth(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> p = val => val.Day == DateTime.DaysInMonth(val.Year, val.Month);
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTimeOffset"/> falls in the specified fiscal <paramref name="quarter"/> (calendar-based: Q1=Jan–Mar, Q2=Apr–Jun, Q3=Jul–Sep, Q4=Oct–Dec).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTimeOffset"/> member.</param>
    /// <param name="quarter">The quarter number (1–4).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quarter"/> is outside 1–4.</exception>
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
