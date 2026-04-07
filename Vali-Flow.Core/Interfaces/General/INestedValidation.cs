using System.Linq.Expressions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Interfaces.General;

/// <summary>
/// Provides nested validation support, allowing sub-builders to be configured per property.
/// Only implemented by <see cref="ValiFlow{T}"/>; <see cref="ValiFlowQuery{T}"/> does not
/// support nested validation because inner predicates may not be EF Core–translatable.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type (CRTP).</typeparam>
/// <typeparam name="T">The entity type being validated.</typeparam>
public interface INestedValidation<out TBuilder, T>
{
    /// <summary>
    /// Adds a nested validation condition using a sub-builder for a navigation property.
    /// </summary>
    /// <typeparam name="TProperty">The type of the navigation property.</typeparam>
    /// <param name="selector">Selects the navigation property from <typeparamref name="T"/>.</param>
    /// <param name="configure">Configures the sub-builder for <typeparamref name="TProperty"/>.</param>
    /// <returns>The builder for fluent chaining.</returns>
    /// <remarks>Not EF Core translatable — use in-memory validation only.</remarks>
    TBuilder ValidateNested<TProperty>(
        Expression<Func<T, TProperty>> selector,
        Action<ValiFlow<TProperty>> configure)
        where TProperty : class;
}
