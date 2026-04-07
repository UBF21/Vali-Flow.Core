namespace Vali_Flow.Core.Models;
#pragma warning disable CS1591 // Missing XML comment — documented on implementation or in API reference

/// <summary>Represents a single validation failure with an optional error code, property path, and severity.</summary>
public sealed class ValidationError
{
    /// <summary>An optional machine-readable error code (e.g. <c>"ERR_REQUIRED"</c>).</summary>
    public string? ErrorCode { get; }
    /// <summary>The human-readable validation message.</summary>
    public string Message { get; }
    /// <summary>The property path that triggered this error, if provided.</summary>
    public string? PropertyPath { get; }
    /// <summary>The severity level of this error.</summary>
    public Severity Severity { get; }

    /// <summary>Initializes a new <see cref="ValidationError"/>.</summary>
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
