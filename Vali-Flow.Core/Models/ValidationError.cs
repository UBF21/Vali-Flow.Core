namespace Vali_Flow.Core.Models;

public sealed class ValidationError
{
    public string? ErrorCode { get; }
    public string Message { get; }
    public string? PropertyPath { get; }
    public Severity Severity { get; }

    public ValidationError(
        string message,
        string? errorCode = null,
        string? propertyPath = null,
        Severity severity = Severity.Error)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        ErrorCode = errorCode;
        PropertyPath = propertyPath;
        Severity = severity;
    }

    public override string ToString()
    {
        // Only prepend severity tag when it differs from the default (Error),
        // preserving backwards-compatible output for standard errors.
        var prefix = Severity != Severity.Error ? $"[{Severity}] " : string.Empty;

        if (PropertyPath != null && ErrorCode != null)
        {
            return $"{prefix}[{PropertyPath}] [{ErrorCode}] {Message}";
        }

        if (PropertyPath != null)
        {
            return $"{prefix}[{PropertyPath}] {Message}";
        }

        if (ErrorCode != null)
        {
            return $"{prefix}[{ErrorCode}] {Message}";
        }

        return $"{prefix}{Message}";
    }
}
