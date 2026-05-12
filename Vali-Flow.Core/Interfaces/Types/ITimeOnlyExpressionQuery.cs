using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for EF Core-safe <see cref="TimeOnly"/> expression conditions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder implementing the interface.</typeparam>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface ITimeOnlyExpressionQuery<out TBuilder, T>
{
    /// <summary>Validates that the selected <see cref="TimeOnly"/> is strictly before <paramref name="time"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="TimeOnly"/> property.</param>
    /// <param name="time">The exclusive upper bound time.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsBefore(Expression<Func<T, TimeOnly>> selector, TimeOnly time);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is strictly after <paramref name="time"/>.</summary>
    /// <param name="selector">Expression selecting the <see cref="TimeOnly"/> property.</param>
    /// <param name="time">The exclusive lower bound time.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsAfter(Expression<Func<T, TimeOnly>> selector, TimeOnly time);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> falls within [<paramref name="from"/>, <paramref name="to"/>] (inclusive).</summary>
    /// <param name="selector">Expression selecting the <see cref="TimeOnly"/> property.</param>
    /// <param name="from">The inclusive start of the time range.</param>
    /// <param name="to">The inclusive end of the time range.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsBetween(Expression<Func<T, TimeOnly>> selector, TimeOnly from, TimeOnly to);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is in the AM period (hour &lt; 12).</summary>
    /// <param name="selector">Expression selecting the <see cref="TimeOnly"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsAM(Expression<Func<T, TimeOnly>> selector);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is in the PM period (hour &gt;= 12).</summary>
    /// <param name="selector">Expression selecting the <see cref="TimeOnly"/> property.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsPM(Expression<Func<T, TimeOnly>> selector);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> equals <paramref name="time"/> exactly (to the tick).</summary>
    /// <param name="selector">Expression selecting the <see cref="TimeOnly"/> property.</param>
    /// <param name="time">The exact time value to match.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsExactTime(Expression<Func<T, TimeOnly>> selector, TimeOnly time);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> falls within the specified hour (0–23).</summary>
    /// <param name="selector">Expression selecting the <see cref="TimeOnly"/> property.</param>
    /// <param name="hour">The hour component to match (0 = midnight, 23 = 11 PM).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder IsInHour(Expression<Func<T, TimeOnly>> selector, int hour);
}
