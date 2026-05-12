using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for EF Core-safe <see cref="DateTime"/> expression conditions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder implementing the interface.</typeparam>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface IDateTimeExpressionQuery<out TBuilder, T>
{
    /// <summary>Validates that the selected <see cref="DateTime"/> falls on <paramref name="date"/> (date-only, ignoring time-of-day).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <param name="date">The exact date to match (time component is ignored).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder ExactDate(Expression<Func<T, DateTime>> selector, DateTime date);

    /// <summary>Validates that the selected <see cref="DateTime"/> is strictly before <paramref name="date"/> (full DateTime comparison including time-of-day).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <param name="date">The exclusive upper bound.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsBefore(Expression<Func<T, DateTime>> selector, DateTime date);

    /// <summary>Validates that the selected <see cref="DateTime"/> is strictly after <paramref name="date"/> (full DateTime comparison including time-of-day).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <param name="date">The exclusive lower bound.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsAfter(Expression<Func<T, DateTime>> selector, DateTime date);

    /// <summary>Validates that the selected <see cref="DateTime"/> is on a date between <paramref name="startDate"/> and <paramref name="endDate"/> (inclusive, date-only).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <param name="startDate">The inclusive start of the date range.</param>
    /// <param name="endDate">The inclusive end of the date range.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder BetweenDates(Expression<Func<T, DateTime>> selector, DateTime startDate, DateTime endDate);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls between two entity-bound date selectors (inclusive, full DateTime comparison).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <param name="startDateSelector">Expression selecting the inclusive start date from the entity.</param>
    /// <param name="endDateSelector">Expression selecting the inclusive end date from the entity.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder BetweenDates(Expression<Func<T, DateTime>> selector,
        Expression<Func<T, DateTime>> startDateSelector, Expression<Func<T, DateTime>> endDateSelector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the same calendar month and year as <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <param name="date">The reference date whose year and month are matched.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder SameMonthAs(Expression<Func<T, DateTime>> selector, DateTime date);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <param name="date">The reference date whose year is matched.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder SameYearAs(Expression<Func<T, DateTime>> selector, DateTime date);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the specified calendar month (1–12), regardless of year.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <param name="month">The calendar month number (1 = January, 12 = December).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsInMonth(Expression<Func<T, DateTime>> selector, int month);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the specified calendar year.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <param name="year">The four-digit calendar year.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsInYear(Expression<Func<T, DateTime>> selector, int year);

    /// <summary>Validates that the selected <see cref="DateTime"/> is in the future (greater than <see cref="DateTime.UtcNow"/>).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder FutureDate(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> is in the past (less than <see cref="DateTime.UtcNow"/>).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder PastDate(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on a Saturday or Sunday.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsWeekend(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on a weekday (Monday–Friday).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsWeekday(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on the specified <paramref name="day"/> of the week.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <param name="day">The day of the week to match.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsDayOfWeek(Expression<Func<T, DateTime>> selector, DayOfWeek day);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on today's UTC date.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsToday(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on yesterday's UTC date.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsYesterday(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on tomorrow's UTC date.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsTomorrow(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls within the last <paramref name="days"/> UTC calendar days (today excluded).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <param name="days">The number of past days to include (today not counted).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder InLastDays(Expression<Func<T, DateTime>> selector, int days);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls within the next <paramref name="days"/> UTC calendar days (today excluded).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <param name="days">The number of future days to include (today not counted).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder InNextDays(Expression<Func<T, DateTime>> selector, int days);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on the first day of its month.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsFirstDayOfMonth(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on the last day of its month.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsLastDayOfMonth(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the specified calendar quarter (1–4).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property.</param>
    /// <param name="quarter">The calendar quarter (1 = Jan–Mar, 2 = Apr–Jun, 3 = Jul–Sep, 4 = Oct–Dec).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsInQuarter(Expression<Func<T, DateTime>> selector, int quarter);
}
