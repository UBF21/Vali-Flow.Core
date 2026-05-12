using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.General;

/// <summary>
/// Defines a composable, chainable sort specification for entities of type <typeparamref name="T"/>.
/// Supports both in-memory (<see cref="IEnumerable{T}"/>) and database-backed (<see cref="IQueryable{T}"/>) sequences.
/// </summary>
/// <typeparam name="T">The type of the entity being sorted.</typeparam>
public interface IValiSort<T>
{
    /// <summary>Adds a primary sort criterion.</summary>
    /// <typeparam name="TKey">The type of the sort key.</typeparam>
    /// <param name="selector">Expression selecting the property to sort by.</param>
    /// <param name="descending">When <c>true</c>, sorts in descending order; otherwise ascending.</param>
    /// <returns>The current <see cref="IValiSort{T}"/> instance for chaining.</returns>
    IValiSort<T> By<TKey>(Expression<Func<T, TKey>> selector, bool descending = false);

    /// <summary>Adds a secondary (or subsequent) sort criterion, applied after the preceding <see cref="By{TKey}"/> or <see cref="ThenBy{TKey}"/> call.</summary>
    /// <typeparam name="TKey">The type of the sort key.</typeparam>
    /// <param name="selector">Expression selecting the property to sort by.</param>
    /// <param name="descending">When <c>true</c>, sorts in descending order; otherwise ascending.</param>
    /// <returns>The current <see cref="IValiSort{T}"/> instance for chaining.</returns>
    IValiSort<T> ThenBy<TKey>(Expression<Func<T, TKey>> selector, bool descending = false);

    /// <summary>Applies the configured sort criteria to a database-backed queryable.</summary>
    /// <param name="query">The <see cref="IQueryable{T}"/> to sort. The sort is translated to SQL by the EF Core provider.</param>
    /// <returns>An <see cref="IOrderedQueryable{T}"/> with all sort criteria applied.</returns>
    IOrderedQueryable<T> Apply(IQueryable<T> query);

    /// <summary>Applies the configured sort criteria to an in-memory sequence.</summary>
    /// <param name="source">The in-memory sequence to sort.</param>
    /// <returns>An <see cref="IOrderedEnumerable{T}"/> with all sort criteria applied.</returns>
    /// <remarks>This overload evaluates the sort in memory. For database queries, use <see cref="Apply(IQueryable{T})"/> instead.</remarks>
    IOrderedEnumerable<T> Apply(IEnumerable<T> source);

    /// <summary>Returns a human-readable description of the configured sort criteria.</summary>
    /// <returns>A string describing the sort order, useful for debugging and logging.</returns>
    string Explain();
}
