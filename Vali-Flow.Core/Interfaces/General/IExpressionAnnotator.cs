using Vali_Flow.Core.Models;

namespace Vali_Flow.Core.Interfaces.General;

/// <summary>
/// Defines operations for annotating the most recently added condition with
/// human-readable messages, error codes, property paths, and severity levels.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type (fluent return type).</typeparam>
public interface IExpressionAnnotator<out TBuilder>
{
    /// <summary>Attaches a human-readable message to the most recently added condition.</summary>
    TBuilder WithMessage(string message);

    /// <summary>Attaches an error code and message to the most recently added condition.</summary>
    TBuilder WithError(string errorCode, string message);

    /// <summary>Attaches an error code, message, and severity to the most recently added condition.</summary>
    TBuilder WithError(string errorCode, string message, Severity severity);

    /// <summary>Attaches an error code, message, and property path to the most recently added condition.</summary>
    TBuilder WithError(string errorCode, string message, string propertyPath);

    /// <summary>Attaches an error code, message, property path, and severity to the most recently added condition.</summary>
    TBuilder WithError(string errorCode, string message, string propertyPath, Severity severity);

    /// <summary>Sets the severity of the most recently added condition's error metadata.</summary>
    TBuilder WithSeverity(Severity severity);
}
