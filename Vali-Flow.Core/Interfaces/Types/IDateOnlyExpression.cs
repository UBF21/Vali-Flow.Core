using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for constructing Vali-Flow.Core conditions based on <see cref="DateOnly"/> property evaluations.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder that allows method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being validated.</typeparam>
public interface IDateOnlyExpression<out TBuilder, T>
{
    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the future (after today's UTC date).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder FutureDate(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the past (before today's UTC date).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder PastDate(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals today's UTC date.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsToday(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on a Saturday or Sunday.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsWeekend(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on a weekday (Monday–Friday).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsWeekday(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on the specified <paramref name="dayOfWeek"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <param name="dayOfWeek">The day of the week to match.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsDayOfWeek(Expression<Func<T, DateOnly>> selector, DayOfWeek dayOfWeek);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the specified calendar month (1–12), regardless of year.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <param name="month">The calendar month number (1 = January, 12 = December).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsInMonth(Expression<Func<T, DateOnly>> selector, int month);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the specified calendar year.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <param name="year">The four-digit calendar year.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsInYear(Expression<Func<T, DateOnly>> selector, int year);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly before <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <param name="date">The exclusive upper bound.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsBefore(Expression<Func<T, DateOnly>> selector, DateOnly date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly after <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <param name="date">The exclusive lower bound.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsAfter(Expression<Func<T, DateOnly>> selector, DateOnly date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the inclusive range [<paramref name="from"/>, <paramref name="to"/>].</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <param name="from">The inclusive start of the date range.</param>
    /// <param name="to">The inclusive end of the date range.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder BetweenDates(Expression<Func<T, DateOnly>> selector, DateOnly from, DateOnly to);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls between two entity-bound date selectors (inclusive).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property to evaluate.</param>
    /// <param name="fromSelector">Expression selecting the inclusive start date from the entity.</param>
    /// <param name="toSelector">Expression selecting the inclusive end date from the entity.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder BetweenDates(Expression<Func<T, DateOnly>> selector, Expression<Func<T, DateOnly>> fromSelector, Expression<Func<T, DateOnly>> toSelector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is the first day of its month.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsFirstDayOfMonth(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is the last day of its month.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsLastDayOfMonth(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <param name="date">The exact date to match.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder ExactDate(Expression<Func<T, DateOnly>> selector, DateOnly date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals yesterday's UTC date.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsYesterday(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals tomorrow's UTC date.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsTomorrow(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the last <paramref name="days"/> UTC days (today excluded).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <param name="days">The number of past days to include (today not counted).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder InLastDays(Expression<Func<T, DateOnly>> selector, int days);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the next <paramref name="days"/> UTC days (today excluded).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <param name="days">The number of future days to include (today not counted).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder InNextDays(Expression<Func<T, DateOnly>> selector, int days);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the same calendar year and month as <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <param name="date">The reference date whose year and month are matched.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder SameMonthAs(Expression<Func<T, DateOnly>> selector, DateOnly date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateOnly"/> property.</param>
    /// <param name="date">The reference date whose year is matched.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder SameYearAs(Expression<Func<T, DateOnly>> selector, DateOnly date);
}
