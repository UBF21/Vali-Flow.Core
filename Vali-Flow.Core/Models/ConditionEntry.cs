using System.Linq.Expressions;

namespace Vali_Flow.Core.Models;

/// <summary>
/// Represents a single condition entry stored in the builder's condition list.
/// Holds the predicate expression, its combinatory operator (AND/OR), optional error metadata,
/// and a lazily compiled delegate for efficient repeated evaluation.
/// </summary>
/// <typeparam name="T">The type of the object being validated.</typeparam>
internal sealed record ConditionEntry<T>
{
    /// <summary>The predicate expression that represents this condition.</summary>
    public Expression<Func<T, bool>> Condition { get; init; }

    /// <summary>
    /// <see langword="true"/> if this condition should be combined with the previous one using AND;
    /// <see langword="false"/> if it starts a new OR-group.
    /// </summary>
    public bool IsAnd { get; init; }

    /// <summary>Optional machine-readable error code surfaced in <see cref="ValidationError"/> when this condition fails.</summary>
    public string? ErrorCode { get; init; }

    /// <summary>Optional human-readable message surfaced in <see cref="ValidationError"/> when this condition fails.</summary>
    public string? Message { get; init; }

    /// <summary>Optional factory that produces the validation message at evaluation time, enabling lazy localization.</summary>
    public Func<string>? MessageFactory { get; init; }

    /// <summary>Optional property path associated with this condition, included in the resulting <see cref="ValidationError"/>.</summary>
    public string? PropertyPath { get; init; }

    /// <summary>The severity level assigned to a failure of this condition.</summary>
    public Severity Severity { get; init; }

    /// <summary>
    /// Lazily compiled delegate for this condition's predicate.
    /// Thread-safe by default (<see cref="LazyThreadSafetyMode.ExecutionAndPublication"/>).
    /// Compilation is deferred until the first call to <see cref="BaseExpression{TBuilder,T}.Validate"/>.
    /// </summary>
    // Lazy compilation — thread-safe by Lazy<T> default (LazyThreadSafetyMode.ExecutionAndPublication)
    public Lazy<Func<T, bool>> CompiledFunc { get; init; }

    /// <summary>
    /// Initializes a new <see cref="ConditionEntry{T}"/> with full error metadata.
    /// </summary>
    /// <param name="condition">The predicate expression for this condition.</param>
    /// <param name="isAnd"><see langword="true"/> to AND with the previous condition; <see langword="false"/> to start a new OR-group.</param>
    /// <param name="errorCode">Optional machine-readable error code.</param>
    /// <param name="message">Optional human-readable validation message.</param>
    /// <param name="messageFactory">Optional factory that resolves the message at evaluation time.</param>
    /// <param name="propertyPath">Optional property path associated with the validation failure.</param>
    /// <param name="severity">The severity level of a failure of this condition.</param>
    public ConditionEntry(
        Expression<Func<T, bool>> condition,
        bool isAnd,
        string? errorCode,
        string? message,
        Func<string>? messageFactory,
        string? propertyPath,
        Severity severity)
    {
        Condition = condition;
        IsAnd = isAnd;
        ErrorCode = errorCode;
        Message = message;
        MessageFactory = messageFactory;
        PropertyPath = propertyPath;
        Severity = severity;
        CompiledFunc = new Lazy<Func<T, bool>>(() => condition.Compile());
    }

    /// <summary>
    /// Factory for the common case: a bare condition with no error metadata.
    /// Avoids passing four explicit <see langword="null"/> arguments at every call site.
    /// </summary>
    /// <param name="condition">The predicate expression for this condition.</param>
    /// <param name="isAnd"><see langword="true"/> to AND with the previous condition; <see langword="false"/> to start a new OR-group.</param>
    /// <param name="severity">The severity level; defaults to <see cref="Severity.Error"/>.</param>
    /// <returns>A new <see cref="ConditionEntry{T}"/> with no error metadata.</returns>
    internal static ConditionEntry<T> Create(
        Expression<Func<T, bool>> condition,
        bool isAnd,
        Severity severity = Severity.Error)
        => new(condition, isAnd, null, null, null, null, severity);
}
