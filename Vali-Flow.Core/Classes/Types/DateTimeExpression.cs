using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

/// <summary>
/// Provides fluent <see cref="DateTime"/> condition methods for the expression builder pipeline.
/// </summary>
/// <typeparam name="TBuilder">
/// The concrete builder type returned by each method to enable fluent chaining.
/// Must inherit <see cref="BaseExpression{TBuilder,T}"/> and implement <see cref="IDateTimeExpression{TBuilder,T}"/>.
/// </typeparam>
/// <typeparam name="T">The entity type whose properties are being evaluated.</typeparam>
public class DateTimeExpression<TBuilder, T> : IDateTimeExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, IDateTimeExpression<TBuilder, T>, new()
{
    /// <summary>The underlying builder used to accumulate conditions.</summary>
    private readonly BaseExpression<TBuilder, T> _builder;

    /// <summary>
    /// Initializes a new instance of <see cref="DateTimeExpression{TBuilder,T}"/> with the given builder.
    /// </summary>
    /// <param name="builder">The parent expression builder that owns the condition list.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public DateTimeExpression(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>
    /// Adds a condition that the selected <see cref="DateTime"/> value must be in the future.
    /// </summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <remarks>
    /// <c>DateTime.Now</c> is accessed as a property in the expression tree and is evaluated fresh
    /// each time the predicate executes. The comparison is always against the current time at evaluation time.
    /// </remarks>
    public TBuilder FutureDate(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val => val > DateTime.Now;
        return _builder.Add(selector, predicate);
    }

    /// <summary>
    /// Adds a condition that the selected <see cref="DateTime"/> value must be in the past.
    /// </summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <remarks>
    /// <c>DateTime.Now</c> is accessed as a property in the expression tree and is evaluated fresh
    /// each time the predicate executes. The comparison is always against the current time at evaluation time.
    /// </remarks>
    public TBuilder PastDate(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val => val < DateTime.Now;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> date falls within [<paramref name="startDate"/>, <paramref name="endDate"/>] (date-only, inclusive).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <param name="startDate">The inclusive lower bound (date part only).</param>
    /// <param name="endDate">The inclusive upper bound (date part only). Must be &gt;= <paramref name="startDate"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="endDate"/> is earlier than <paramref name="startDate"/>.</exception>
    /// <remarks>
    /// Compares only the date part (<c>val.Date</c>), ignoring time-of-day.
    /// <b>EF Core:</b> <c>val.Date</c> is not universally translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// For EF Core queries, use <see cref="IsBefore"/>/<see cref="IsAfter"/> or a constant-boundary comparison against the full <see cref="DateTime"/>.
    /// </remarks>
    public TBuilder BetweenDates(Expression<Func<T, DateTime>> selector, DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (endDate < startDate)
            throw new ArgumentOutOfRangeException(nameof(endDate), "endDate must be greater than or equal to startDate.");
        Expression<Func<DateTime, bool>> predicate = val => val.Date >= startDate.Date && val.Date <= endDate.Date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>
    /// Validates that the selected <see cref="DateTime"/> value (date part only) is between
    /// two entity-bound date selectors (inclusive). Both boundary values are normalized via
    /// <c>.Date</c> to strip time-of-day, consistent with the constant-boundary overload.
    /// </summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <param name="startDateSelector">Expression selecting the inclusive lower-bound <see cref="DateTime"/> from the same entity.</param>
    /// <param name="endDateSelector">Expression selecting the inclusive upper-bound <see cref="DateTime"/> from the same entity.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder BetweenDates(Expression<Func<T, DateTime>> selector,
        Expression<Func<T, DateTime>> startDateSelector, Expression<Func<T, DateTime>> endDateSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(startDateSelector);
        ArgumentNullException.ThrowIfNull(endDateSelector);

        var param = selector.Parameters[0];
        var dateProp = typeof(DateTime).GetProperty(nameof(DateTime.Date))!;

        var valDate = Expression.Property(selector.Body, dateProp);
        var valDateClone = Expression.Property(new ForceCloneVisitor().Visit(selector.Body)!, dateProp);
        var startBody = new ParameterReplacer(startDateSelector.Parameters[0], param).Visit(startDateSelector.Body)!;
        var endBody = new ParameterReplacer(endDateSelector.Parameters[0], param).Visit(endDateSelector.Body)!;
        var startDate = Expression.Property(startBody, dateProp);
        var endDate = Expression.Property(endBody, dateProp);

        Expression<Func<T, bool>> filter = Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(
                Expression.GreaterThanOrEqual(valDate, startDate),
                Expression.LessThanOrEqual(valDateClone, endDate)
            ),
            param
        );

        return _builder.Add(filter);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> date part equals <paramref name="date"/> (date-only comparison).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <param name="date">The reference date to match (time-of-day is ignored).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <remarks>
    /// Compares only the date part (<c>val.Date == date.Date</c>), ignoring time-of-day.
    /// <b>EF Core:</b> <c>val.Date</c> is not universally translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder ExactDate(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val => val.Date == date.Date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>
    /// Adds a condition that the selected <see cref="DateTime"/> value falls on today's date.
    /// </summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <remarks>
    /// <c>DateTime.Today</c> is accessed as a property in the expression tree and is evaluated fresh
    /// each time the predicate executes. The comparison is always against today's date at evaluation time.
    /// <b>EF Core:</b> <c>val.Date</c> and <c>DateTime.Today</c> are not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsToday(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val => val.Date == DateTime.Today;
        return _builder.Add(selector, predicate);
    }

    /// <summary>
    /// Adds a condition that the selected <see cref="DateTime"/> value falls on yesterday's date.
    /// </summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <remarks>
    /// <c>DateTime.Today</c> is accessed as a property in the expression tree and is evaluated fresh
    /// each time the predicate executes. The comparison is always against yesterday's date at evaluation time.
    /// <b>EF Core:</b> <c>val.Date</c> and <c>DateTime.Today</c> are not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsYesterday(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val => val.Date == DateTime.Today.AddDays(-1);
        return _builder.Add(selector, predicate);
    }

    /// <summary>
    /// Adds a condition that the selected <see cref="DateTime"/> value falls on tomorrow's date.
    /// </summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <remarks>
    /// <c>DateTime.Today</c> is accessed as a property in the expression tree and is evaluated fresh
    /// each time the predicate executes. The comparison is always against tomorrow's date at evaluation time.
    /// <b>EF Core:</b> <c>val.Date</c> and <c>DateTime.Today</c> are not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsTomorrow(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val => val.Date == DateTime.Today.AddDays(1);
        return _builder.Add(selector, predicate);
    }

    /// <summary>
    /// Adds a condition that the selected <see cref="DateTime"/> value falls within the N days preceding
    /// today (today excluded). Matches dates in the half-open range [Today-N, Today).
    /// </summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <param name="days">Number of past days to include. Must be a positive integer.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is not positive.</exception>
    /// <remarks>
    /// <c>DateTime.Today</c> is accessed as a property in the expression tree and is evaluated fresh
    /// each time the predicate executes. The boundary shifts with the current date at evaluation time.
    /// <b>EF Core:</b> <c>val.Date</c> and <c>DateTime.Today</c> are not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder InLastDays(Expression<Func<T, DateTime>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0)
            throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");

        Expression<Func<DateTime, bool>> predicate = val => val.Date >= DateTime.Today.AddDays(-days) && val.Date < DateTime.Today;
        return _builder.Add(selector, predicate);
    }

    /// <summary>
    /// Adds a condition that the selected <see cref="DateTime"/> value falls within the N days following
    /// today (today excluded). Matches dates in the half-open range (Today, Today+N].
    /// </summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <param name="days">Number of future days to include. Must be a positive integer.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is not positive.</exception>
    /// <remarks>
    /// <c>DateTime.Today</c> is accessed as a property in the expression tree and is evaluated fresh
    /// each time the predicate executes. The boundary shifts with the current date at evaluation time.
    /// <b>EF Core:</b> <c>val.Date</c> and <c>DateTime.Today</c> are not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder InNextDays(Expression<Func<T, DateTime>> selector, int days)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (days <= 0)
            throw new ArgumentOutOfRangeException(nameof(days), "days must be a positive integer.");

        Expression<Func<DateTime, bool>> predicate = val => val.Date > DateTime.Today && val.Date <= DateTime.Today.AddDays(days);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on a Saturday or Sunday.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsWeekend(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val =>
            val.DayOfWeek == DayOfWeek.Saturday || val.DayOfWeek == DayOfWeek.Sunday;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on a weekday (Monday–Friday).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsWeekday(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val =>
            val.DayOfWeek != DayOfWeek.Saturday && val.DayOfWeek != DayOfWeek.Sunday;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in a leap year.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <remarks>
    /// <b>EF Core:</b> <c>DateTime.IsLeapYear</c> is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder IsLeapYear(Expression<Func<T, DateTime>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val => DateTime.IsLeapYear(val.Year);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the same calendar month and year as <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <param name="date">The reference date whose month and year are matched.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <remarks>
    /// Compares <c>.Year</c> and <c>.Month</c> scalar properties — translatable to SQL by EF Core on all major providers.
    /// </remarks>
    public TBuilder SameMonthAs(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val => val.Year == date.Year && val.Month == date.Month;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls in the same calendar year as <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <param name="date">The reference date whose year is matched.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <remarks>
    /// Compares the <c>.Year</c> scalar property — translatable to SQL by EF Core on all major providers.
    /// </remarks>
    public TBuilder SameYearAs(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val => val.Year == date.Year;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> falls on the specified <paramref name="day"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <param name="day">The <see cref="DayOfWeek"/> value to match.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsDayOfWeek(Expression<Func<T, DateTime>> selector, DayOfWeek day)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val => val.DayOfWeek == day;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> is in the specified month (1–12).</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <param name="month">The calendar month number (1–12).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="month"/> is outside [1, 12].</exception>
    public TBuilder IsInMonth(Expression<Func<T, DateTime>> selector, int month)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        Expression<Func<DateTime, bool>> predicate = val => val.Month == month;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> is in the specified <paramref name="year"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <param name="year">The four-digit calendar year (1–9999).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="year"/> is outside [1, 9999].</exception>
    public TBuilder IsInYear(Expression<Func<T, DateTime>> selector, int year)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
        Expression<Func<DateTime, bool>> predicate = val => val.Year == year;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> is strictly before <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <param name="date">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsBefore(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val => val < date;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected <see cref="DateTime"/> is strictly after <paramref name="date"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="DateTime"/> property to evaluate.</param>
    /// <param name="date">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public TBuilder IsAfter(Expression<Func<T, DateTime>> selector, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<DateTime, bool>> predicate = val => val > date;
        return _builder.Add(selector, predicate);
    }

}