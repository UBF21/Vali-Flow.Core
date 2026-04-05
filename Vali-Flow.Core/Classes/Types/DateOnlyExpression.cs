using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

public class DateOnlyExpression<TBuilder, T> : IDateOnlyExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, IDateOnlyExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public DateOnlyExpression(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the future relative to today's UTC date.</summary>
    /// <remarks>
    /// Compares against <c>DateTime.UtcNow.Date</c>. On servers not running in UTC, the date boundary
    /// shifts at midnight UTC rather than midnight local time. Use <c>DateOnly.FromDateTime(DateTime.Today)</c>
    /// if you need local-time boundaries.
    /// </remarks>
    public TBuilder FutureDate(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val > DateOnly.FromDateTime(DateTime.UtcNow.Date);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the past relative to today's UTC date.</summary>
    /// <remarks>
    /// Compares against <c>DateTime.UtcNow.Date</c>. On servers not running in UTC, the date boundary
    /// shifts at midnight UTC rather than midnight local time. Use <c>DateOnly.FromDateTime(DateTime.Today)</c>
    /// if you need local-time boundaries.
    /// </remarks>
    public TBuilder PastDate(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val < DateOnly.FromDateTime(DateTime.UtcNow.Date);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals today's UTC date.</summary>
    /// <remarks>
    /// Compares against <c>DateTime.UtcNow.Date</c>. On servers not running in UTC, the date boundary
    /// shifts at midnight UTC rather than midnight local time. Use <c>DateOnly.FromDateTime(DateTime.Today)</c>
    /// if you need local-time boundaries.
    /// </remarks>
    public TBuilder IsToday(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val == DateOnly.FromDateTime(DateTime.UtcNow.Date);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on a Saturday or Sunday.</summary>
    public TBuilder IsWeekend(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on a weekday (Monday–Friday).</summary>
    public TBuilder IsWeekday(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on the specified <paramref name="dayOfWeek"/>.</summary>
    public TBuilder IsDayOfWeek(Expression<Func<T, DateOnly>> selector, DayOfWeek dayOfWeek)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val.DayOfWeek == dayOfWeek;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the specified month (1–12).</summary>
    public TBuilder IsInMonth(Expression<Func<T, DateOnly>> selector, int month)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "month must be between 1 and 12.");
        Expression<Func<DateOnly, bool>> predicate = val => val.Month == month;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the specified <paramref name="year"/>.</summary>
    public TBuilder IsInYear(Expression<Func<T, DateOnly>> selector, int year)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
        Expression<Func<DateOnly, bool>> predicate = val => val.Year == year;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly before <paramref name="date"/>.</summary>
    public TBuilder IsBefore(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val < date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly after <paramref name="date"/>.</summary>
    public TBuilder IsAfter(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val > date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls between <paramref name="from"/> and <paramref name="to"/> (inclusive).</summary>
    public TBuilder BetweenDates(Expression<Func<T, DateOnly>> selector, DateOnly from, DateOnly to)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be greater than or equal to from.");
        Expression<Func<DateOnly, bool>> predicate = val => val >= from && val <= to;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls between two entity-bound date selectors (inclusive).</summary>
    public TBuilder BetweenDates(Expression<Func<T, DateOnly>> selector,
        Expression<Func<T, DateOnly>> fromSelector, Expression<Func<T, DateOnly>> toSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(fromSelector);
        ArgumentNullException.ThrowIfNull(toSelector);

        var param = selector.Parameters[0];
        var valBody = selector.Body;
        var valBodyClone = new ForceCloneVisitor().Visit(valBody)!;
        var fromBody = new ParameterReplacer(fromSelector.Parameters[0], param).Visit(fromSelector.Body)!;
        var toBody = new ParameterReplacer(toSelector.Parameters[0], param).Visit(toSelector.Body)!;

        return _builder.Add(Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(
                Expression.GreaterThanOrEqual(valBody, fromBody),
                Expression.LessThanOrEqual(valBodyClone, toBody)),
            param));
    }

    /// <summary>Validates that the selected date is the first day of its month.</summary>
    public TBuilder IsFirstDayOfMonth(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val.Day == 1;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected date is the last day of its month.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>DateTime.DaysInMonth</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsLastDayOfMonth(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val =>
            val.Day == DateTime.DaysInMonth(val.Year, val.Month);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals <paramref name="date"/>.</summary>
    public TBuilder ExactDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val == date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly before <paramref name="date"/>.</summary>
    public TBuilder BeforeDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val < date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly after <paramref name="date"/>.</summary>
    public TBuilder AfterDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val > date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals yesterday's UTC date.</summary>
    public TBuilder IsYesterday(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val == DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals tomorrow's UTC date.</summary>
    public TBuilder IsTomorrow(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val == DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the last <paramref name="days"/> days (today excluded).</summary>
    /// <remarks>Matches dates in the half-open range [Today-N, Today). Today is excluded.</remarks>
    public TBuilder InLastDays(Expression<Func<T, DateOnly>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        Expression<Func<DateOnly, bool>> predicate = val =>
            val >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days))
            && val < DateOnly.FromDateTime(DateTime.UtcNow);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the next <paramref name="days"/> days (today excluded).</summary>
    /// <remarks>Matches dates in the half-open range (Today, Today+N].</remarks>
    public TBuilder InNextDays(Expression<Func<T, DateOnly>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        Expression<Func<DateOnly, bool>> predicate = val =>
            val > DateOnly.FromDateTime(DateTime.UtcNow)
            && val <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the same calendar year and month as <paramref name="date"/>.</summary>
    public TBuilder SameMonthAs(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var month = date.Month;
        var year = date.Year;
        Expression<Func<DateOnly, bool>> predicate = val => val.Month == month && val.Year == year;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    public TBuilder SameYearAs(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var year = date.Year;
        Expression<Func<DateOnly, bool>> predicate = val => val.Year == year;
        return _builder.Add(selector, predicate);
    }

}
