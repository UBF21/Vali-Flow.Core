namespace Vali_Flow.Core.Models;
#pragma warning disable CS1591 // Missing XML comment — documented on implementation or in API reference

public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors;

    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();
    public string? FirstError => _errors.Count > 0 ? _errors[0].Message : null;
    public string? FirstErrorCode => _errors.Count > 0 ? _errors[0].ErrorCode : null;

    public IReadOnlyList<ValidationError> Warnings
        => _errors.Where(e => e.Severity == Severity.Warning).ToList();

    public IReadOnlyList<ValidationError> CriticalErrors
        => _errors.Where(e => e.Severity == Severity.Critical).ToList();

    public IReadOnlyList<ValidationError> ErrorsAbove(Severity minSeverity)
        => _errors.Where(e => e.Severity >= minSeverity).ToList();

    public bool HasAnySeverity(Severity minSeverity)
        => _errors.Any(e => e.Severity >= minSeverity);

    internal ValidationResult(List<ValidationError> errors)
    {
        _errors = errors ?? throw new ArgumentNullException(nameof(errors));
    }

    public static ValidationResult Ok() => new(new List<ValidationError>());

    public override string ToString() =>
        IsValid ? "Valid" : string.Join("; ", _errors.Select(e => e.ToString()));
}
