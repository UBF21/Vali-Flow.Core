using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

/// <summary>
/// EF Core-safe <see cref="DateTime"/> validation conditions for the fluent builder.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type returned by each fluent method.</typeparam>
/// <typeparam name="T">The entity type being validated.</typeparam>
/// <remarks>
/// Boundary timestamps are captured at the moment the condition method is called and baked
/// into the expression as constants. They are not re-evaluated on each predicate invocation.
/// </remarks>
public class DateTimeExpressionQuery<TBuilder, T> : IDateTimeExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    /// <summary>The underlying builder that accumulates conditions.</summary>
    private readonly BaseExpression<TBuilder, T> _builder;

    /// <summary>
    /// Initializes a new instance of <see cref="DateTimeExpressionQuery{TBuilder,T}"/>.
    /// </summary>
    /// <param name="builder">The parent builder that owns the condition list.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public DateTimeExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls on the calendar date of <paramref name="date"/> (midnight-inclusive, next-midnight-exclusive).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <param name="date">The target date; time component is ignored.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder ExactDate(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var start = date.Date;
        var end = date.Date.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> is strictly before <paramref name="date"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <param name="date">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsBefore(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val < date;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> is strictly after <paramref name="date"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <param name="date">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsAfter(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val > date;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls within the date range [<paramref name="startDate"/>, <paramref name="endDate"/>] (day-inclusive).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <param name="startDate">The inclusive start date; time component is ignored.</param>
    /// <param name="endDate">The inclusive end date; time component is ignored.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="endDate"/> is before <paramref name="startDate"/>.</exception>
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

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls between two date properties of the same entity (inclusive).</summary>
    /// <param name="selector">Property selector for the value to test.</param>
    /// <param name="startDateSelector">Property selector for the inclusive lower bound.</param>
    /// <param name="endDateSelector">Property selector for the inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any selector is <see langword="null"/>.</exception>
    /// <remarks>
    /// Uses <see cref="ForceCloneVisitor"/> to avoid sharing the same expression node on both sides of the <c>AndAlso</c>.
    /// </remarks>
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

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls in the same year and month as <paramref name="date"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <param name="date">The reference date.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder SameMonthAs(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val.Year == date.Year && val.Month == date.Month;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls in the same year as <paramref name="date"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <param name="date">The reference date.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder SameYearAs(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val.Year == date.Year;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls in calendar month <paramref name="month"/> (any year).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <param name="month">Month number (1–12).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="month"/> is outside 1–12.</exception>
    public TBuilder IsInMonth(Expression<Func<T, DateTime>> selector, int month)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "month must be between 1 and 12.");
        Expression<Func<DateTime, bool>> p = val => val.Month == month;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls in the specified calendar <paramref name="year"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <param name="year">The calendar year (1–9999).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="year"/> is outside 1–9999.</exception>
    public TBuilder IsInYear(Expression<Func<T, DateTime>> selector, int year)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
        Expression<Func<DateTime, bool>> p = val => val.Year == year;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> is strictly after <see cref="DateTime.UtcNow"/> at condition-build time.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder FutureDate(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val > DateTime.UtcNow;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> is strictly before <see cref="DateTime.UtcNow"/> at condition-build time.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder PastDate(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val < DateTime.UtcNow;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls on a Saturday or Sunday.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsWeekend(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls on Monday through Friday.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsWeekday(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls on the specified <paramref name="day"/> of the week.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <param name="day">The required day of the week.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsDayOfWeek(Expression<Func<T, DateTime>> selector, DayOfWeek day)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val.DayOfWeek == day;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls on today's UTC date (midnight-inclusive, next-midnight-exclusive).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsToday(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls on yesterday's UTC date.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsYesterday(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var start = DateTime.UtcNow.Date.AddDays(-1);
        var end = start.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls on tomorrow's UTC date.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsTomorrow(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var start = DateTime.UtcNow.Date.AddDays(1);
        var end = start.AddDays(1);
        Expression<Func<DateTime, bool>> p = val => val >= start && val < end;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls within the last <paramref name="days"/> full UTC days (today excluded).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <param name="days">Number of past days to include; must be &gt; 0.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is not positive.</exception>
    public TBuilder InLastDays(Expression<Func<T, DateTime>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var todayStart = DateTime.UtcNow.Date;
        var fromStart = todayStart.AddDays(-days);
        Expression<Func<DateTime, bool>> p = val => val >= fromStart && val < todayStart;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls within the next <paramref name="days"/> full UTC days (today excluded).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <param name="days">Number of future days to include; must be &gt; 0.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is not positive.</exception>
    public TBuilder InNextDays(Expression<Func<T, DateTime>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var tomorrowStart = DateTime.UtcNow.Date.AddDays(1);
        var untilEnd = tomorrowStart.AddDays(days);
        Expression<Func<DateTime, bool>> p = val => val >= tomorrowStart && val < untilEnd;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> is the first day of its month (day == 1).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsFirstDayOfMonth(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val.Day == 1;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> is the last day of its month.</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsLastDayOfMonth(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> p = val => val.Day == DateTime.DaysInMonth(val.Year, val.Month);
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateTime"/> falls in the specified fiscal <paramref name="quarter"/> (quarters are calendar-based: Q1=Jan–Mar, Q2=Apr–Jun, Q3=Jul–Sep, Q4=Oct–Dec).</summary>
    /// <param name="selector">Property selector for the <see cref="DateTime"/> member.</param>
    /// <param name="quarter">The quarter number (1–4).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quarter"/> is outside 1–4.</exception>
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
