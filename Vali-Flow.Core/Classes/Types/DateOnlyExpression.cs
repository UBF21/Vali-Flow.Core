using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

/// <summary>
/// Provides fluent <see cref="DateOnly"/> condition methods for the expression builder pipeline.
/// </summary>
/// <typeparam name="TBuilder">
/// The concrete builder type returned by each method to enable fluent chaining.
/// Must inherit <see cref="BaseExpression{TBuilder,T}"/> and implement <see cref="IDateOnlyExpression{TBuilder,T}"/>.
/// </typeparam>
/// <typeparam name="T">The entity type whose properties are being evaluated.</typeparam>
public class DateOnlyExpression<TBuilder, T> : IDateOnlyExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, IDateOnlyExpression<TBuilder, T>, new()
{
    /// <summary>The underlying builder used to accumulate conditions.</summary>
    private readonly BaseExpression<TBuilder, T> _builder;

    /// <summary>
    /// Initializes a new instance of <see cref="DateOnlyExpression{TBuilder,T}"/> with the given builder.
    /// </summary>
    /// <param name="builder">The parent expression builder that owns the condition list.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public DateOnlyExpression(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the future relative to today's UTC date.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
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
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
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
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
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
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsWeekend(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on a weekday (Monday–Friday).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsWeekday(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on the specified <paramref name="dayOfWeek"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <param name="dayOfWeek">The <see cref="DayOfWeek"/> value to match.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsDayOfWeek(Expression<Func<T, DateOnly>> selector, DayOfWeek dayOfWeek)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val.DayOfWeek == dayOfWeek;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the specified month (1–12).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <param name="month">The calendar month number (1–12).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="month"/> is outside [1, 12].</exception>
    public TBuilder IsInMonth(Expression<Func<T, DateOnly>> selector, int month)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "month must be between 1 and 12.");
        Expression<Func<DateOnly, bool>> predicate = val => val.Month == month;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the specified <paramref name="year"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <param name="year">The four-digit calendar year (1–9999).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="year"/> is outside [1, 9999].</exception>
    public TBuilder IsInYear(Expression<Func<T, DateOnly>> selector, int year)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
        Expression<Func<DateOnly, bool>> predicate = val => val.Year == year;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly before <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <param name="date">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsBefore(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val < date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly after <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <param name="date">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsAfter(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val > date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls between <paramref name="from"/> and <paramref name="to"/> (inclusive).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <param name="from">The inclusive lower bound.</param>
    /// <param name="to">The inclusive upper bound. Must be &gt;= <paramref name="from"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="to"/> is earlier than <paramref name="from"/>.</exception>
    public TBuilder BetweenDates(Expression<Func<T, DateOnly>> selector, DateOnly from, DateOnly to)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be greater than or equal to from.");
        Expression<Func<DateOnly, bool>> predicate = val => val >= from && val <= to;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls between two entity-bound date selectors (inclusive).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <param name="fromSelector">Expression selecting the inclusive lower-bound <see cref="DateOnly"/> from the same entity.</param>
    /// <param name="toSelector">Expression selecting the inclusive upper-bound <see cref="DateOnly"/> from the same entity.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
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
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsFirstDayOfMonth(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val.Day == 1;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected date is the last day of its month.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
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
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <param name="date">The exact date to match.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder ExactDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val == date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals yesterday's UTC date.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsYesterday(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val == DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals tomorrow's UTC date.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsTomorrow(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> predicate = val => val == DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the last <paramref name="days"/> days (today excluded).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <param name="days">Number of past days to include. Must be a positive integer.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is not positive.</exception>
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
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <param name="days">Number of future days to include. Must be a positive integer.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is not positive.</exception>
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
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <param name="date">The reference date whose month and year are matched.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder SameMonthAs(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var month = date.Month;
        var year = date.Year;
        Expression<Func<DateOnly, bool>> predicate = val => val.Month == month && val.Year == year;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <param name="date">The reference date whose year is matched.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder SameYearAs(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var year = date.Year;
        Expression<Func<DateOnly, bool>> predicate = val => val.Year == year;
        return _builder.Add(selector, predicate);
    }

}
