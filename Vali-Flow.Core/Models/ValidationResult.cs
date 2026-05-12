namespace Vali_Flow.Core.Models;
#pragma warning disable CS1591 // Missing XML comment — documented on implementation or in API reference

/// <summary>
/// Represents the outcome of a validation run produced by
/// <see cref="Classes.Base.BaseExpression{TBuilder,T}.Validate"/>.
/// Contains the list of <see cref="ValidationError"/> entries for every annotated condition that failed.
/// </summary>
/// <remarks>
/// An instance with <see cref="IsValid"/> equal to <see langword="true"/> (i.e., <see cref="Errors"/> is empty)
/// can be obtained directly via <see cref="Ok"/> without running any conditions.
/// Filtered views such as <see cref="Warnings"/> and <see cref="CriticalErrors"/> are computed lazily
/// on first access and cached for the lifetime of the instance.
/// </remarks>
public sealed class ValidationResult
{
    /// <summary>Backing store for all validation errors collected during the run.</summary>
    private readonly List<ValidationError> _errors;

    /// <summary>Lazily computed subset of <see cref="_errors"/> with <see cref="Severity.Warning"/>.</summary>
    private readonly Lazy<IReadOnlyList<ValidationError>> _warnings;

    /// <summary>Lazily computed subset of <see cref="_errors"/> with <see cref="Severity.Critical"/>.</summary>
    private readonly Lazy<IReadOnlyList<ValidationError>> _criticalErrors;

    /// <summary><see langword="true"/> when no validation errors were collected; otherwise <see langword="false"/>.</summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>All validation errors collected during the run, in the order conditions were evaluated.</summary>
    public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();

    /// <summary>The message of the first collected error, or <see langword="null"/> when <see cref="IsValid"/> is <see langword="true"/>.</summary>
    public string? FirstError => _errors.Count > 0 ? _errors[0].Message : null;

    /// <summary>The error code of the first collected error, or <see langword="null"/> when <see cref="IsValid"/> is <see langword="true"/> or the first error has no code.</summary>
    public string? FirstErrorCode => _errors.Count > 0 ? _errors[0].ErrorCode : null;

    /// <summary>All errors whose <see cref="ValidationError.Severity"/> is <see cref="Severity.Warning"/>. Computed lazily and cached.</summary>
    public IReadOnlyList<ValidationError> Warnings => _warnings.Value;

    /// <summary>All errors whose <see cref="ValidationError.Severity"/> is <see cref="Severity.Critical"/>. Computed lazily and cached.</summary>
    public IReadOnlyList<ValidationError> CriticalErrors => _criticalErrors.Value;

    /// <summary>Returns all errors whose severity is <paramref name="minSeverity"/> or higher (inclusive).</summary>
    /// <param name="minSeverity">The minimum <see cref="Severity"/> threshold.</param>
    /// <returns>A read-only list of matching <see cref="ValidationError"/> entries.</returns>
    public IReadOnlyList<ValidationError> ErrorsAtOrAbove(Severity minSeverity)
        => _errors.Where(e => e.Severity >= minSeverity).ToList().AsReadOnly();

    /// <summary>Returns <see langword="true"/> if at least one collected error has a severity at or above <paramref name="minSeverity"/>.</summary>
    /// <param name="minSeverity">The minimum <see cref="Severity"/> threshold to check.</param>
    public bool HasAnySeverity(Severity minSeverity)
        => _errors.Any(e => e.Severity >= minSeverity);

    /// <summary>
    /// Initializes a new <see cref="ValidationResult"/> with the given error list.
    /// Internal — consumers obtain instances via <see cref="Ok"/> or
    /// <see cref="Classes.Base.BaseExpression{TBuilder,T}.Validate"/>.
    /// </summary>
    /// <param name="errors">The list of collected <see cref="ValidationError"/> entries. Must not be <see langword="null"/>.</param>
    internal ValidationResult(List<ValidationError> errors)
    {
        _errors = errors ?? throw new ArgumentNullException(nameof(errors));
        _warnings      = new Lazy<IReadOnlyList<ValidationError>>(
            () => _errors.Where(e => e.Severity == Severity.Warning).ToList().AsReadOnly());
        _criticalErrors = new Lazy<IReadOnlyList<ValidationError>>(
            () => _errors.Where(e => e.Severity == Severity.Critical).ToList().AsReadOnly());
    }

    /// <summary>Creates a <see cref="ValidationResult"/> that represents a successful (error-free) validation.</summary>
    /// <returns>A <see cref="ValidationResult"/> with an empty error list and <see cref="IsValid"/> equal to <see langword="true"/>.</returns>
    public static ValidationResult Ok() => new(new List<ValidationError>());

    /// <summary>Returns <c>"Valid"</c> when there are no errors; otherwise a semicolon-separated list of all error messages.</summary>
    public override string ToString() =>
        IsValid ? "Valid" : string.Join("; ", _errors.Select(e => e.ToString()));
}
