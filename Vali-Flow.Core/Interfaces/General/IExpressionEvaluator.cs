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
    bool IsValid(T instance);

    /// <summary>
    /// Returns <c>true</c> if <paramref name="instance"/> does NOT satisfy the built conditions.
    /// </summary>
    bool IsNotValid(T instance);

    /// <summary>
    /// Evaluates each annotated condition and returns a <see cref="ValidationResult"/> with failures.
    /// </summary>
    ValidationResult Validate(T instance);

    /// <summary>
    /// Runs <see cref="Validate"/> for every item in <paramref name="instances"/>.
    /// </summary>
    IEnumerable<(T Item, ValidationResult Result)> ValidateAll(IEnumerable<T> instances);

    /// <summary>
    /// Returns a human-readable description of the built expression tree.
    /// </summary>
    string Explain();
}
