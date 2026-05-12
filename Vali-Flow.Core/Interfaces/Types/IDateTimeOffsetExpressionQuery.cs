using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for EF Core-safe <see cref="DateTimeOffset"/> expression conditions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder implementing the interface.</typeparam>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface IDateTimeOffsetExpressionQuery<out TBuilder, T>
{
    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is in the future (after <see cref="DateTimeOffset.UtcNow"/>).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder FutureDate(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is in the past (before <see cref="DateTimeOffset.UtcNow"/>).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder PastDate(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is strictly before <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <param name="date">The exclusive upper bound.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsBefore(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is strictly after <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <param name="date">The exclusive lower bound.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsAfter(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the inclusive range [<paramref name="from"/>, <paramref name="to"/>].</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <param name="from">The inclusive start of the range.</param>
    /// <param name="to">The inclusive end of the range.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder BetweenDates(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset from, DateTimeOffset to);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified calendar month (1–12), regardless of year.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <param name="month">The calendar month number (1 = January, 12 = December).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsInMonth(Expression<Func<T, DateTimeOffset>> selector, int month);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified calendar year.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <param name="year">The four-digit calendar year.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsInYear(Expression<Func<T, DateTimeOffset>> selector, int year);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on today's UTC calendar date.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsToday(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on yesterday's UTC calendar date.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsYesterday(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on tomorrow's UTC calendar date.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsTomorrow(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the same UTC calendar date as <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <param name="date">The reference date to match (time component is ignored).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder ExactDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the same calendar year and month as <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <param name="date">The reference date whose year and month are matched.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder SameMonthAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <param name="date">The reference date whose year is matched.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder SameYearAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the last <paramref name="days"/> UTC calendar days (today excluded).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <param name="days">The number of past days to include (today not counted).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder InLastDays(Expression<Func<T, DateTimeOffset>> selector, int days);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the next <paramref name="days"/> UTC calendar days (today excluded).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <param name="days">The number of future days to include (today not counted).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder InNextDays(Expression<Func<T, DateTimeOffset>> selector, int days);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on a Saturday or Sunday.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsWeekend(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on a weekday (Monday–Friday).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsWeekday(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the specified <paramref name="day"/> of the week.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <param name="day">The day of the week to match.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsDayOfWeek(Expression<Func<T, DateTimeOffset>> selector, DayOfWeek day);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the first day of its month.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsFirstDayOfMonth(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the last day of its month.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsLastDayOfMonth(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified calendar quarter (1–4).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTimeOffset"/> property.</param>
    /// <param name="quarter">The calendar quarter (1 = Jan–Mar, 2 = Apr–Jun, 3 = Jul–Sep, 4 = Oct–Dec).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsInQuarter(Expression<Func<T, DateTimeOffset>> selector, int quarter);
}
