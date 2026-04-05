using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;

namespace Vali_Flow.Core.Classes.Types;

public class DateTimeOffsetExpression<TBuilder, T> : IDateTimeOffsetExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, IDateTimeOffsetExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public DateTimeOffsetExpression(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is in the future.</summary>
    /// <remarks>
    /// <c>DateTimeOffset.UtcNow</c> is accessed as a property in the expression tree and is evaluated
    /// fresh each time the predicate executes. The comparison is always against the current UTC time.
    /// </remarks>
    public TBuilder FutureDate(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> predicate = val => val > DateTimeOffset.UtcNow;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is in the past.</summary>
    /// <remarks>
    /// <c>DateTimeOffset.UtcNow</c> is accessed as a property in the expression tree and is evaluated
    /// fresh each time the predicate executes. The comparison is always against the current UTC time.
    /// </remarks>
    public TBuilder PastDate(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> predicate = val => val < DateTimeOffset.UtcNow;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on today's UTC date.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>.UtcDateTime.Date</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsToday(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> predicate = val => val.UtcDateTime.Date == DateTimeOffset.UtcNow.UtcDateTime.Date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on yesterday's UTC date.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>.UtcDateTime.Date</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsYesterday(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> predicate = val => val.UtcDateTime.Date == DateTimeOffset.UtcNow.UtcDateTime.Date.AddDays(-1);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on tomorrow's UTC date.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>.UtcDateTime.Date</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsTomorrow(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> predicate = val => val.UtcDateTime.Date == DateTimeOffset.UtcNow.UtcDateTime.Date.AddDays(1);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on a weekend (Saturday or Sunday).</summary>
    /// <remarks>
    /// Uses <c>DateTimeOffset.DayOfWeek</c>, which reflects the day of week in the <b>stored offset</b>,
    /// not in UTC. If your values are stored in UTC (offset +00:00), this is equivalent to UTC day of week.
    /// If offsets vary, consider normalizing to UTC via <c>.UtcDateTime.DayOfWeek</c> before applying this check.
    /// </remarks>
    public TBuilder IsWeekend(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> predicate = val =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on a weekday (Monday–Friday).</summary>
    /// <remarks>
    /// Uses <c>DateTimeOffset.DayOfWeek</c>, which reflects the day of week in the <b>stored offset</b>,
    /// not in UTC. If your values are stored in UTC (offset +00:00), this is equivalent to UTC day of week.
    /// If offsets vary, consider normalizing to UTC via <c>.UtcDateTime.DayOfWeek</c> before applying this check.
    /// </remarks>
    public TBuilder IsWeekday(Expression<Func<T, DateTimeOffset>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> predicate = val =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the specified <paramref name="dayOfWeek"/>.</summary>
    /// <remarks>
    /// Uses <c>DateTimeOffset.DayOfWeek</c>, which reflects the day of week in the <b>stored offset</b>,
    /// not in UTC. If your values are stored in UTC (offset +00:00), this is equivalent to UTC day of week.
    /// If offsets vary, consider normalizing to UTC via <c>.UtcDateTime.DayOfWeek</c> before applying this check.
    /// </remarks>
    public TBuilder IsDayOfWeek(Expression<Func<T, DateTimeOffset>> selector, DayOfWeek dayOfWeek)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> predicate = val => val.DayOfWeek == dayOfWeek;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified month.</summary>
    /// <remarks>
    /// Uses <c>DateTimeOffset.Month</c>, which reflects the month in the <b>stored offset</b>, not in UTC.
    /// If your values are stored in UTC (offset +00:00), this is equivalent to the UTC month.
    /// If offsets vary, consider normalizing to UTC via <c>.UtcDateTime.Month</c> before applying this check.
    /// </remarks>
    public TBuilder IsInMonth(Expression<Func<T, DateTimeOffset>> selector, int month)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "month must be between 1 and 12.");
        Expression<Func<DateTimeOffset, bool>> predicate = val => val.Month == month;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified year.</summary>
    /// <remarks>
    /// Uses <c>DateTimeOffset.Year</c>, which reflects the year in the <b>stored offset</b>, not in UTC.
    /// If your values are stored in UTC (offset +00:00), this is equivalent to the UTC year.
    /// If offsets vary, consider normalizing to UTC via <c>.UtcDateTime.Year</c> before applying this check.
    /// </remarks>
    public TBuilder IsInYear(Expression<Func<T, DateTimeOffset>> selector, int year)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
        Expression<Func<DateTimeOffset, bool>> predicate = val => val.Year == year;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is before <paramref name="date"/> (full timestamp comparison, offset-aware).</summary>
    public TBuilder IsBefore(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> predicate = val => val < date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is after <paramref name="date"/> (full timestamp comparison, offset-aware).</summary>
    public TBuilder IsAfter(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTimeOffset, bool>> predicate = val => val > date;
        return _builder.Add(selector, predicate);
    }

    public TBuilder BetweenDates(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset from, DateTimeOffset to)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be greater than or equal to from.");
        Expression<Func<DateTimeOffset, bool>> predicate = val => val >= from && val <= to;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the same UTC calendar date as <paramref name="date"/>.</summary>
    /// <remarks>
    /// Compares <c>.UtcDateTime.Date</c> on both values.
    /// <b>EF Core:</b> <c>.UtcDateTime.Date</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder ExactDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var datePart = date.UtcDateTime.Date;
        Expression<Func<DateTimeOffset, bool>> predicate = val => val.UtcDateTime.Date == datePart;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls before the UTC calendar date of <paramref name="date"/> (date-only comparison, time ignored).</summary>
    /// <remarks>
    /// Compares <c>.UtcDateTime.Date</c> on both values.
    /// <b>EF Core:</b> <c>.UtcDateTime.Date</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder BeforeDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var datePart = date.UtcDateTime.Date;
        Expression<Func<DateTimeOffset, bool>> predicate = val => val.UtcDateTime.Date < datePart;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls after the UTC calendar date of <paramref name="date"/> (date-only comparison, time ignored).</summary>
    /// <remarks>
    /// Compares <c>.UtcDateTime.Date</c> on both values.
    /// <b>EF Core:</b> <c>.UtcDateTime.Date</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder AfterDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var datePart = date.UtcDateTime.Date;
        Expression<Func<DateTimeOffset, bool>> predicate = val => val.UtcDateTime.Date > datePart;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the same calendar year and month as <paramref name="date"/>.</summary>
    /// <remarks>
    /// Uses <c>DateTimeOffset.Month</c> and <c>DateTimeOffset.Year</c>, which reflect the <b>stored offset</b>, not UTC.
    /// If offsets vary, consider normalizing to UTC before applying this check.
    /// </remarks>
    public TBuilder SameMonthAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var month = date.Month;
        var year = date.Year;
        Expression<Func<DateTimeOffset, bool>> predicate = val => val.Month == month && val.Year == year;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    /// <remarks>
    /// Uses <c>DateTimeOffset.Year</c>, which reflects the <b>stored offset</b>, not UTC.
    /// If offsets vary, consider normalizing to UTC before applying this check.
    /// </remarks>
    public TBuilder SameYearAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var year = date.Year;
        Expression<Func<DateTimeOffset, bool>> predicate = val => val.Year == year;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the last <paramref name="days"/> days.</summary>
    /// <remarks>
    /// <c>DateTimeOffset.UtcNow</c> is accessed as a property in the expression tree and is evaluated
    /// fresh each time the predicate executes. The boundary shifts with the current UTC time.
    /// <b>EF Core:</b> <c>.UtcDateTime.Date</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder InLastDays(Expression<Func<T, DateTimeOffset>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0)
            throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        Expression<Func<DateTimeOffset, bool>> predicate =
            val => val.UtcDateTime.Date >= DateTimeOffset.UtcNow.UtcDateTime.Date.AddDays(-days)
                && val.UtcDateTime.Date < DateTimeOffset.UtcNow.UtcDateTime.Date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the N days following today (today excluded).
    /// Matches UTC dates in the half-open range (UtcNow.Date, UtcNow.Date+N].</summary>
    /// <remarks>
    /// <c>DateTimeOffset.UtcNow</c> is accessed as a property in the expression tree and is evaluated
    /// fresh each time the predicate executes. The boundary shifts with the current UTC time.
    /// <b>EF Core:</b> <c>.UtcDateTime.Date</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder InNextDays(Expression<Func<T, DateTimeOffset>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0)
            throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        Expression<Func<DateTimeOffset, bool>> predicate =
            val => val.UtcDateTime.Date > DateTimeOffset.UtcNow.UtcDateTime.Date
                && val.UtcDateTime.Date <= DateTimeOffset.UtcNow.UtcDateTime.Date.AddDays(days);
        return _builder.Add(selector, predicate);
    }
}
