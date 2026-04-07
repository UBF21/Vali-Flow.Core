using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for EF Core-safe <see cref="DateTimeOffset"/> expression conditions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder implementing the interface.</typeparam>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface IDateTimeOffsetExpressionQuery<out TBuilder, T>
{
    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is in the future.</summary>
    TBuilder FutureDate(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is in the past.</summary>
    TBuilder PastDate(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is before <paramref name="date"/>.</summary>
    TBuilder IsBefore(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is after <paramref name="date"/>.</summary>
    TBuilder IsAfter(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the inclusive range [<paramref name="from"/>, <paramref name="to"/>].</summary>
    TBuilder BetweenDates(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset from, DateTimeOffset to);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified month.</summary>
    TBuilder IsInMonth(Expression<Func<T, DateTimeOffset>> selector, int month);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified year.</summary>
    TBuilder IsInYear(Expression<Func<T, DateTimeOffset>> selector, int year);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on today's UTC calendar date.</summary>
    TBuilder IsToday(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on yesterday's UTC calendar date.</summary>
    TBuilder IsYesterday(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on tomorrow's UTC calendar date.</summary>
    TBuilder IsTomorrow(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the same UTC calendar date as <paramref name="date"/>.</summary>
    TBuilder ExactDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the same calendar year and month as <paramref name="date"/>.</summary>
    TBuilder SameMonthAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    TBuilder SameYearAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the last <paramref name="days"/> UTC calendar days (today excluded).</summary>
    TBuilder InLastDays(Expression<Func<T, DateTimeOffset>> selector, int days);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> is within the next <paramref name="days"/> UTC calendar days (today excluded).</summary>
    TBuilder InNextDays(Expression<Func<T, DateTimeOffset>> selector, int days);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on a Saturday or Sunday.</summary>
    TBuilder IsWeekend(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on a weekday (Monday–Friday).</summary>
    TBuilder IsWeekday(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the specified <paramref name="day"/> of the week.</summary>
    TBuilder IsDayOfWeek(Expression<Func<T, DateTimeOffset>> selector, DayOfWeek day);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the first day of its month.</summary>
    TBuilder IsFirstDayOfMonth(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls on the last day of its month.</summary>
    TBuilder IsLastDayOfMonth(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected <see cref="DateTimeOffset"/> falls in the specified calendar quarter (1–4).</summary>
    TBuilder IsInQuarter(Expression<Func<T, DateTimeOffset>> selector, int quarter);
}
