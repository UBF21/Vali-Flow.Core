namespace Vali_Flow.Core.Models;
#pragma warning disable CS1591 // Missing XML comment — documented on implementation or in API reference

public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors;
    private readonly IReadOnlyList<ValidationError> _warnings;
    private readonly IReadOnlyList<ValidationError> _criticalErrors;

    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();
    public string? FirstError => _errors.Count > 0 ? _errors[0].Message : null;
    public string? FirstErrorCode => _errors.Count > 0 ? _errors[0].ErrorCode : null;

    public IReadOnlyList<ValidationError> Warnings => _warnings;

    public IReadOnlyList<ValidationError> CriticalErrors => _criticalErrors;

    public IReadOnlyList<ValidationError> ErrorsAbove(Severity minSeverity)
        => _errors.Where(e => e.Severity >= minSeverity).ToList();

    public bool HasAnySeverity(Severity minSeverity)
        => _errors.Any(e => e.Severity >= minSeverity);

    internal ValidationResult(List<ValidationError> errors)
    {
        _errors = errors ?? throw new ArgumentNullException(nameof(errors));
        _warnings = _errors.Where(e => e.Severity == Severity.Warning).ToList();
        _criticalErrors = _errors.Where(e => e.Severity == Severity.Critical).ToList();
    }

    public static ValidationResult Ok() => new(new List<ValidationError>());

    public override string ToString() =>
        IsValid ? "Valid" : string.Join("; ", _errors.Select(e => e.ToString()));
}
