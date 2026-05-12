using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;

namespace Vali_Flow.Core.Classes.Types;

/// <summary>
/// EF Core-safe <see cref="TimeOnly"/> validation conditions for the fluent builder.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type returned by each fluent method.</typeparam>
/// <typeparam name="T">The entity type being validated.</typeparam>
public class TimeOnlyExpressionQuery<TBuilder, T> : ITimeOnlyExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    /// <summary>The underlying builder that accumulates conditions.</summary>
    private readonly BaseExpression<TBuilder, T> _builder;

    /// <summary>
    /// Initializes a new instance of <see cref="TimeOnlyExpressionQuery{TBuilder,T}"/>.
    /// </summary>
    /// <param name="builder">The parent builder that owns the condition list.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public TimeOnlyExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Adds a condition that the selected <see cref="TimeOnly"/> is strictly before <paramref name="time"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="TimeOnly"/> member.</param>
    /// <param name="time">The exclusive upper bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsBefore(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<TimeOnly, bool>> p = val => val < time;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="TimeOnly"/> is strictly after <paramref name="time"/>.</summary>
    /// <param name="selector">Property selector for the <see cref="TimeOnly"/> member.</param>
    /// <param name="time">The exclusive lower bound.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsAfter(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<TimeOnly, bool>> p = val => val > time;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="TimeOnly"/> falls within [<paramref name="from"/>, <paramref name="to"/>] (inclusive).</summary>
    /// <param name="selector">Property selector for the <see cref="TimeOnly"/> member.</param>
    /// <param name="from">The inclusive lower bound.</param>
    /// <param name="to">The inclusive upper bound; must be &gt;= <paramref name="from"/>.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="to"/> is before <paramref name="from"/>.</exception>
    /// <remarks>Midnight-crossing ranges (e.g. 22:00–02:00) are not supported; use <c>Or()</c> to combine two conditions.</remarks>
    public TBuilder IsBetween(Expression<Func<T, TimeOnly>> selector, TimeOnly from, TimeOnly to)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (to < from)
            throw new ArgumentOutOfRangeException(nameof(to), "to must be >= from. Midnight-crossing ranges are not supported; use Or() to combine two separate conditions.");
        Expression<Func<TimeOnly, bool>> p = val => val >= from && val <= to;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="TimeOnly"/> is in the AM period (hour &lt; 12).</summary>
    /// <param name="selector">Property selector for the <see cref="TimeOnly"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsAM(Expression<Func<T, TimeOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<TimeOnly, bool>> p = val => val.Hour < 12;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="TimeOnly"/> is in the PM period (hour &gt;= 12).</summary>
    /// <param name="selector">Property selector for the <see cref="TimeOnly"/> member.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsPM(Expression<Func<T, TimeOnly>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<TimeOnly, bool>> p = val => val.Hour >= 12;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="TimeOnly"/> equals <paramref name="time"/> exactly.</summary>
    /// <param name="selector">Property selector for the <see cref="TimeOnly"/> member.</param>
    /// <param name="time">The exact time to match.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    public TBuilder IsExactTime(Expression<Func<T, TimeOnly>> selector, TimeOnly time)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<TimeOnly, bool>> p = val => val == time;
        return _builder.Add(selector, p);
    }

    /// <summary>Adds a condition that the selected <see cref="TimeOnly"/> falls within the specified <paramref name="hour"/> (0–23).</summary>
    /// <param name="selector">Property selector for the <see cref="TimeOnly"/> member.</param>
    /// <param name="hour">The hour to match (0–23).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="hour"/> is outside 0–23.</exception>
    public TBuilder IsInHour(Expression<Func<T, TimeOnly>> selector, int hour)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (hour < 0 || hour > 23)
            throw new ArgumentOutOfRangeException(nameof(hour), "hour must be between 0 and 23.");
        Expression<Func<TimeOnly, bool>> p = val => val.Hour == hour;
        return _builder.Add(selector, p);
    }
}
