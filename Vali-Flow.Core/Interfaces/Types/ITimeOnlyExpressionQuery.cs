using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for EF Core-safe <see cref="TimeOnly"/> expression conditions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder implementing the interface.</typeparam>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface ITimeOnlyExpressionQuery<out TBuilder, T>
{
    /// <summary>Validates that the selected <see cref="TimeOnly"/> is before <paramref name="time"/>.</summary>
    TBuilder IsBefore(Expression<Func<T, TimeOnly>> selector, TimeOnly time);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is after <paramref name="time"/>.</summary>
    TBuilder IsAfter(Expression<Func<T, TimeOnly>> selector, TimeOnly time);

    /// <summary>Validates that the selected time falls within [<paramref name="from"/>, <paramref name="to"/>] (inclusive).</summary>
    TBuilder IsBetween(Expression<Func<T, TimeOnly>> selector, TimeOnly from, TimeOnly to);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is in the AM (hour &lt; 12).</summary>
    TBuilder IsAM(Expression<Func<T, TimeOnly>> selector);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> is in the PM (hour &gt;= 12).</summary>
    TBuilder IsPM(Expression<Func<T, TimeOnly>> selector);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> equals <paramref name="time"/> exactly (to the tick).</summary>
    TBuilder IsExactTime(Expression<Func<T, TimeOnly>> selector, TimeOnly time);

    /// <summary>Validates that the selected <see cref="TimeOnly"/> falls within the specified hour (0–23).</summary>
    TBuilder IsInHour(Expression<Func<T, TimeOnly>> selector, int hour);
}
