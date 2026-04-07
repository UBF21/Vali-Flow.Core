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
    TBuilder ExactDate(Expression<Func<T, DateTime>> selector, DateTime date);

    /// <summary>Validates that the selected <see cref="DateTime"/> is before <paramref name="date"/> (full DateTime comparison including time-of-day).</summary>
    TBuilder IsBefore(Expression<Func<T, DateTime>> selector, DateTime date);

    /// <summary>Validates that the selected <see cref="DateTime"/> is after <paramref name="date"/> (full DateTime comparison including time-of-day).</summary>
    TBuilder IsAfter(Expression<Func<T, DateTime>> selector, DateTime date);

    /// <summary>Validates that the selected <see cref="DateTime"/> is on a date between <paramref name="startDate"/> and <paramref name="endDate"/> (inclusive, date-only).</summary>
    TBuilder BetweenDates(Expression<Func<T, DateTime>> selector, DateTime startDate, DateTime endDate);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls between two entity-bound date selectors (inclusive, full DateTime comparison).</summary>
    TBuilder BetweenDates(Expression<Func<T, DateTime>> selector,
        Expression<Func<T, DateTime>> startDateSelector, Expression<Func<T, DateTime>> endDateSelector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the same calendar month and year as <paramref name="date"/>.</summary>
    TBuilder SameMonthAs(Expression<Func<T, DateTime>> selector, DateTime date);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    TBuilder SameYearAs(Expression<Func<T, DateTime>> selector, DateTime date);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the specified calendar month (1–12), regardless of year.</summary>
    TBuilder IsInMonth(Expression<Func<T, DateTime>> selector, int month);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the specified calendar year.</summary>
    TBuilder IsInYear(Expression<Func<T, DateTime>> selector, int year);

    /// <summary>Validates that the selected <see cref="DateTime"/> is in the future (greater than <see cref="DateTime.UtcNow"/>).</summary>
    TBuilder FutureDate(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> is in the past (less than <see cref="DateTime.UtcNow"/>).</summary>
    TBuilder PastDate(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on a Saturday or Sunday.</summary>
    TBuilder IsWeekend(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on a weekday (Monday–Friday).</summary>
    TBuilder IsWeekday(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on the specified <paramref name="day"/> of the week.</summary>
    TBuilder IsDayOfWeek(Expression<Func<T, DateTime>> selector, DayOfWeek day);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on today's UTC date.</summary>
    TBuilder IsToday(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on yesterday's UTC date.</summary>
    TBuilder IsYesterday(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on tomorrow's UTC date.</summary>
    TBuilder IsTomorrow(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls within the last <paramref name="days"/> UTC calendar days (today excluded).</summary>
    TBuilder InLastDays(Expression<Func<T, DateTime>> selector, int days);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls within the next <paramref name="days"/> UTC calendar days (today excluded).</summary>
    TBuilder InNextDays(Expression<Func<T, DateTime>> selector, int days);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on the first day of its month.</summary>
    TBuilder IsFirstDayOfMonth(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on the last day of its month.</summary>
    TBuilder IsLastDayOfMonth(Expression<Func<T, DateTime>> selector);

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the specified calendar quarter (1–4).</summary>
    TBuilder IsInQuarter(Expression<Func<T, DateTime>> selector, int quarter);
}
