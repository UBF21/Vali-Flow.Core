using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for EF Core-safe <see cref="DateOnly"/> expression conditions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder implementing the interface.</typeparam>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface IDateOnlyExpressionQuery<out TBuilder, T>
{
    /// <summary>Validates that the selected <see cref="DateOnly"/> is before <paramref name="date"/>.</summary>
    TBuilder IsBefore(Expression<Func<T, DateOnly>> selector, DateOnly date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is after <paramref name="date"/>.</summary>
    TBuilder IsAfter(Expression<Func<T, DateOnly>> selector, DateOnly date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is within the inclusive range [<paramref name="from"/>, <paramref name="to"/>].</summary>
    TBuilder BetweenDates(Expression<Func<T, DateOnly>> selector, DateOnly from, DateOnly to);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls between two entity-bound date selectors (inclusive).</summary>
    TBuilder BetweenDates(Expression<Func<T, DateOnly>> selector,
        Expression<Func<T, DateOnly>> fromSelector, Expression<Func<T, DateOnly>> toSelector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the specified calendar month (1–12), regardless of year.</summary>
    TBuilder IsInMonth(Expression<Func<T, DateOnly>> selector, int month);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the specified calendar year.</summary>
    TBuilder IsInYear(Expression<Func<T, DateOnly>> selector, int year);

    /// <summary>Validates that the selected date is the first day of its month.</summary>
    TBuilder IsFirstDayOfMonth(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is the last day of its month.</summary>
    TBuilder IsLastDayOfMonth(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the future (after today's UTC date).</summary>
    TBuilder FutureDate(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> is in the past (before today's UTC date).</summary>
    TBuilder PastDate(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals today's UTC date.</summary>
    TBuilder IsToday(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals <paramref name="date"/>.</summary>
    TBuilder ExactDate(Expression<Func<T, DateOnly>> selector, DateOnly date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals yesterday's UTC date.</summary>
    TBuilder IsYesterday(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> equals tomorrow's UTC date.</summary>
    TBuilder IsTomorrow(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the last <paramref name="days"/> UTC days (today excluded).</summary>
    TBuilder InLastDays(Expression<Func<T, DateOnly>> selector, int days);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the next <paramref name="days"/> UTC days (today excluded).</summary>
    TBuilder InNextDays(Expression<Func<T, DateOnly>> selector, int days);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the same calendar year and month as <paramref name="date"/>.</summary>
    TBuilder SameMonthAs(Expression<Func<T, DateOnly>> selector, DateOnly date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    TBuilder SameYearAs(Expression<Func<T, DateOnly>> selector, DateOnly date);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on a Saturday or Sunday.</summary>
    TBuilder IsWeekend(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on a weekday (Monday–Friday).</summary>
    TBuilder IsWeekday(Expression<Func<T, DateOnly>> selector);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls on the specified <paramref name="dayOfWeek"/>.</summary>
    TBuilder IsDayOfWeek(Expression<Func<T, DateOnly>> selector, DayOfWeek dayOfWeek);

    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the specified calendar quarter (1–4).</summary>
    TBuilder IsInQuarter(Expression<Func<T, DateOnly>> selector, int quarter);
}
