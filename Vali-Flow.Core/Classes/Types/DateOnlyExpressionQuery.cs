using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

/// <summary>
/// EF Core-safe <see cref="DateOnly"/> validation conditions for the fluent builder.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type returned by each fluent method.</typeparam>
/// <typeparam name="T">The entity type being validated.</typeparam>
/// <remarks>
/// Boundary dates are captured at the moment the condition method is called using
/// <see cref="DateTime.UtcNow"/> and are baked into the expression as constants.
/// </remarks>
public class DateOnlyExpressionQuery<TBuilder, T> : IDateOnlyExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    /// <summary>The underlying builder that accumulates conditions.</summary>
    private readonly BaseExpression<TBuilder, T> _builder;

    /// <summary>
    /// Initializes a new instance of <see cref="DateOnlyExpressionQuery{TBuilder,T}"/>.
    /// </summary>
    /// <param name="builder">The parent builder that owns the condition list.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public DateOnlyExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> is strictly before <paramref name="date"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <param name="date">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsBefore(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> p = val => val < date;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> is strictly after <paramref name="date"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <param name="date">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsAfter(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> p = val => val > date;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> falls within [<paramref name="from"/>, <paramref name="to"/>] (inclusive).</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <param name="from">The inclusive lower bound.</param>
    /// <param name="to">The inclusive upper bound; must be &gt;= <paramref name="from"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="to"/> is before <paramref name="from"/>.</exception>
    public TBuilder BetweenDates(Expression<Func<T, DateOnly>> selector, DateOnly from, DateOnly to)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be >= from.");
        Expression<Func<DateOnly, bool>> p = val => val >= from && val <= to;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> falls between two date properties of the same entity (inclusive).</summary>
    /// <param name="selector">Property selector for the value to test.</param>
    /// <param name="fromSelector">Property selector for the inclusive lower bound.</param>
    /// <param name="toSelector">Property selector for the inclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any selector is <see langword="null"/>.</exception>
    /// <remarks>
    /// Uses <see cref="ForceCloneVisitor"/> to avoid sharing the same expression node on both sides of the <c>AndAlso</c>.
    /// </remarks>
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
            Expression.AndAlso(Expression.GreaterThanOrEqual(valBody, fromBody), Expression.LessThanOrEqual(valBodyClone, toBody)),
            param));
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> falls in calendar month <paramref name="month"/> (any year).</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <param name="month">Month number (1–12).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="month"/> is outside 1–12.</exception>
    public TBuilder IsInMonth(Expression<Func<T, DateOnly>> selector, int month)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "month must be between 1 and 12.");
        Expression<Func<DateOnly, bool>> p = val => val.Month == month;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> falls in the specified calendar <paramref name="year"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <param name="year">The calendar year (1–9999).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="year"/> is outside 1–9999.</exception>
    public TBuilder IsInYear(Expression<Func<T, DateOnly>> selector, int year)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
        Expression<Func<DateOnly, bool>> p = val => val.Year == year;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> is the first day of its month (day == 1).</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsFirstDayOfMonth(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> p = val => val.Day == 1;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> is the last day of its month.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <remarks>Detected by checking whether adding one day changes the month.</remarks>
    public TBuilder IsLastDayOfMonth(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> p = val => val.AddDays(1).Month != val.Month;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> is strictly after today's UTC date.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder FutureDate(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Expression<Func<DateOnly, bool>> p = val => val > today;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> is strictly before today's UTC date.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder PastDate(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Expression<Func<DateOnly, bool>> p = val => val < today;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> equals today's UTC date.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsToday(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Expression<Func<DateOnly, bool>> p = val => val == today;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> equals <paramref name="date"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <param name="date">The target date.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder ExactDate(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> p = val => val == date;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> equals yesterday's UTC date.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsYesterday(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        Expression<Func<DateOnly, bool>> p = val => val == yesterday;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> equals tomorrow's UTC date.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsTomorrow(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        Expression<Func<DateOnly, bool>> p = val => val == tomorrow;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> falls within the last <paramref name="days"/> days before today (today excluded).</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <param name="days">Number of past days to include; must be &gt; 0.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is not positive.</exception>
    public TBuilder InLastDays(Expression<Func<T, DateOnly>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Expression<Func<DateOnly, bool>> p = val => val >= from && val < today;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> falls within the next <paramref name="days"/> days after today (today excluded, last day inclusive).</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <param name="days">Number of future days to include; must be &gt; 0.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is not positive.</exception>
    public TBuilder InNextDays(Expression<Func<T, DateOnly>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0) throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var until = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));
        Expression<Func<DateOnly, bool>> p = val => val >= tomorrow && val <= until;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> falls in the same year and month as <paramref name="date"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <param name="date">The reference date.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder SameMonthAs(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var month = date.Month;
        var year = date.Year;
        Expression<Func<DateOnly, bool>> p = val => val.Month == month && val.Year == year;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> falls in the same year as <paramref name="date"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <param name="date">The reference date.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder SameYearAs(Expression<Func<T, DateOnly>> selector, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var year = date.Year;
        Expression<Func<DateOnly, bool>> p = val => val.Year == year;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> falls on a Saturday or Sunday.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsWeekend(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> p = val =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> falls on Monday through Friday.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsWeekday(Expression<Func<T, DateOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> p = val =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> falls on the specified <paramref name="dayOfWeek"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <param name="dayOfWeek">The required day of the week.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsDayOfWeek(Expression<Func<T, DateOnly>> selector, DayOfWeek dayOfWeek)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateOnly, bool>> p = val => val.DayOfWeek == dayOfWeek;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="DateOnly"/> falls in the specified fiscal <paramref name="quarter"/> (calendar-based: Q1=Jan–Mar, Q2=Apr–Jun, Q3=Jul–Sep, Q4=Oct–Dec).</summary>
    /// <param name="selector">Property selector for the <see cref="DateOnly"/> member.</param>
    /// <param name="quarter">The quarter number (1–4).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quarter"/> is outside 1–4.</exception>
    public TBuilder IsInQuarter(Expression<Func<T, DateOnly>> selector, int quarter)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (quarter < 1 || quarter > 4)
            throw new ArgumentOutOfRangeException(nameof(quarter), "quarter must be between 1 and 4.");
        var firstMonth = (quarter - 1) * 3 + 1;
        var lastMonth = firstMonth + 2;
        Expression<Func<DateOnly, bool>> p = val => val.Month >= firstMonth && val.Month <= lastMonth;
        return _builder.Add(selector, p);
    }
}
