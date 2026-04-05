using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for constructing Vali-Flow.Core conditions based on DateTimeOffset property evaluations.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder that allows method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being validated.</typeparam>
public interface IDateTimeOffsetExpression<out TBuilder, T>
{
    /// <summary>Validates that the selected DateTimeOffset is in the future (UTC).</summary>
    TBuilder FutureDate(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected DateTimeOffset is in the past (UTC).</summary>
    TBuilder PastDate(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected DateTimeOffset falls on today's UTC date.</summary>
    TBuilder IsToday(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected DateTimeOffset falls on yesterday's UTC date.</summary>
    TBuilder IsYesterday(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected DateTimeOffset falls on tomorrow's UTC date.</summary>
    TBuilder IsTomorrow(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected DateTimeOffset falls on a weekend (Saturday or Sunday).</summary>
    TBuilder IsWeekend(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected DateTimeOffset falls on a weekday (Monday to Friday).</summary>
    TBuilder IsWeekday(Expression<Func<T, DateTimeOffset>> selector);

    /// <summary>Validates that the selected DateTimeOffset falls on the specified day of the week.</summary>
    TBuilder IsDayOfWeek(Expression<Func<T, DateTimeOffset>> selector, DayOfWeek dayOfWeek);

    /// <summary>Validates that the selected DateTimeOffset is in the specified month (1–12).</summary>
    TBuilder IsInMonth(Expression<Func<T, DateTimeOffset>> selector, int month);

    /// <summary>Validates that the selected DateTimeOffset is in the specified year.</summary>
    TBuilder IsInYear(Expression<Func<T, DateTimeOffset>> selector, int year);

    /// <summary>Validates that the selected DateTimeOffset is strictly before <paramref name="date"/>.</summary>
    TBuilder IsBefore(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected DateTimeOffset is strictly after <paramref name="date"/>.</summary>
    TBuilder IsAfter(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected DateTimeOffset falls within [from, to] inclusive.</summary>
    TBuilder BetweenDates(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset from, DateTimeOffset to);

    /// <summary>Validates that the selected DateTimeOffset falls on the same UTC calendar date as <paramref name="date"/>.</summary>
    TBuilder ExactDate(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected DateTimeOffset falls in the same calendar year and month as <paramref name="date"/>.</summary>
    TBuilder SameMonthAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected DateTimeOffset falls in the same calendar year as <paramref name="date"/>.</summary>
    TBuilder SameYearAs(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date);

    /// <summary>Validates that the selected DateTimeOffset is within the last <paramref name="days"/> days from now (UTC).</summary>
    TBuilder InLastDays(Expression<Func<T, DateTimeOffset>> selector, int days);

    /// <summary>Validates that the selected DateTimeOffset is within the next <paramref name="days"/> days from now (UTC).</summary>
    TBuilder InNextDays(Expression<Func<T, DateTimeOffset>> selector, int days);
}
