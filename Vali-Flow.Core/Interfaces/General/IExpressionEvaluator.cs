using Vali_Flow.Core.Models;

namespace Vali_Flow.Core.Interfaces.General;

/// <summary>
/// Defines in-memory evaluation operations over a built expression targeting <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The entity type being evaluated.</typeparam>
public interface IExpressionEvaluator<T>
{
    /// <summary>
    /// Returns <c>true</c> if <paramref name="instance"/> satisfies all conditions.
    /// </summary>
    /// <param name="instance">The entity instance to evaluate.</param>
    /// <returns><c>true</c> when all accumulated conditions pass; otherwise <c>false</c>.</returns>
    bool IsValid(T instance);

    /// <summary>
    /// Returns <c>true</c> if <paramref name="instance"/> does NOT satisfy the built conditions.
    /// </summary>
    /// <param name="instance">The entity instance to evaluate.</param>
    /// <returns><c>true</c> when at least one condition fails; otherwise <c>false</c>.</returns>
    bool IsNotValid(T instance);

    /// <summary>
    /// Evaluates each annotated condition individually and returns a <see cref="ValidationResult"/> describing all failures.
    /// </summary>
    /// <param name="instance">The entity instance to evaluate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing the list of conditions that failed and their associated metadata.</returns>
    ValidationResult Validate(T instance);

    /// <summary>
    /// Runs <see cref="Validate"/> for every item in <paramref name="instances"/> and returns a result per item.
    /// </summary>
    /// <param name="instances">The collection of entity instances to evaluate.</param>
    /// <returns>A sequence of tuples pairing each item with its <see cref="ValidationResult"/>.</returns>
    IEnumerable<(T Item, ValidationResult Result)> ValidateAll(IEnumerable<T> instances);

    /// <summary>
    /// Returns a human-readable description of the built expression tree, useful for debugging.
    /// </summary>
    /// <returns>A string representation of all accumulated conditions and their logical operators.</returns>
    string Explain();
}
