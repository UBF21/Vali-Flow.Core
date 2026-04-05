using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

public interface IDateOnlyExpression<out TBuilder, T>
{
    TBuilder FutureDate(Expression<Func<T, DateOnly>> selector);
    TBuilder PastDate(Expression<Func<T, DateOnly>> selector);
    TBuilder IsToday(Expression<Func<T, DateOnly>> selector);
    TBuilder IsWeekend(Expression<Func<T, DateOnly>> selector);
    TBuilder IsWeekday(Expression<Func<T, DateOnly>> selector);
    TBuilder IsDayOfWeek(Expression<Func<T, DateOnly>> selector, DayOfWeek dayOfWeek);
    TBuilder IsInMonth(Expression<Func<T, DateOnly>> selector, int month);
    TBuilder IsInYear(Expression<Func<T, DateOnly>> selector, int year);
    TBuilder IsBefore(Expression<Func<T, DateOnly>> selector, DateOnly date);
    TBuilder IsAfter(Expression<Func<T, DateOnly>> selector, DateOnly date);
    TBuilder BetweenDates(Expression<Func<T, DateOnly>> selector, DateOnly from, DateOnly to);
    TBuilder BetweenDates(Expression<Func<T, DateOnly>> selector, Expression<Func<T, DateOnly>> fromSelector, Expression<Func<T, DateOnly>> toSelector);
    TBuilder IsFirstDayOfMonth(Expression<Func<T, DateOnly>> selector);
    TBuilder IsLastDayOfMonth(Expression<Func<T, DateOnly>> selector);
    /// <summary>Validates that the selected <see cref="DateOnly"/> equals <paramref name="date"/>.</summary>
    TBuilder ExactDate(Expression<Func<T, DateOnly>> selector, DateOnly date);
    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly before <paramref name="date"/>.</summary>
    TBuilder BeforeDate(Expression<Func<T, DateOnly>> selector, DateOnly date);
    /// <summary>Validates that the selected <see cref="DateOnly"/> is strictly after <paramref name="date"/>.</summary>
    TBuilder AfterDate(Expression<Func<T, DateOnly>> selector, DateOnly date);
    /// <summary>Validates that the selected <see cref="DateOnly"/> equals yesterday's UTC date.</summary>
    TBuilder IsYesterday(Expression<Func<T, DateOnly>> selector);
    /// <summary>Validates that the selected <see cref="DateOnly"/> equals tomorrow's UTC date.</summary>
    TBuilder IsTomorrow(Expression<Func<T, DateOnly>> selector);
    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the last <paramref name="days"/> days (today excluded).</summary>
    TBuilder InLastDays(Expression<Func<T, DateOnly>> selector, int days);
    /// <summary>Validates that the selected <see cref="DateOnly"/> falls within the next <paramref name="days"/> days (today excluded).</summary>
    TBuilder InNextDays(Expression<Func<T, DateOnly>> selector, int days);
    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the same calendar year and month as <paramref name="date"/>.</summary>
    TBuilder SameMonthAs(Expression<Func<T, DateOnly>> selector, DateOnly date);
    /// <summary>Validates that the selected <see cref="DateOnly"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    TBuilder SameYearAs(Expression<Func<T, DateOnly>> selector, DateOnly date);
}
