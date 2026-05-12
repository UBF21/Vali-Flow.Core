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
    /// <param name="message">The static message string to attach.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder WithMessage(string message);

    /// <summary>Attaches a lazily-evaluated message factory to the most recently added condition.
    /// The factory is called only when the error message is actually retrieved, enabling deferred
    /// localization (e.g., <c>() =&gt; localizer["Validation.NameRequired"]</c>).</summary>
    /// <param name="messageFactory">A zero-argument factory invoked on demand to produce the message.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder WithMessage(Func<string> messageFactory);

    /// <summary>Attaches an error code and message to the most recently added condition.</summary>
    /// <param name="errorCode">A short machine-readable code identifying the validation rule.</param>
    /// <param name="message">A human-readable description of the failure.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder WithError(string errorCode, string message);

    /// <summary>Attaches an error code, message, and severity to the most recently added condition.</summary>
    /// <param name="errorCode">A short machine-readable code identifying the validation rule.</param>
    /// <param name="message">A human-readable description of the failure.</param>
    /// <param name="severity">The <see cref="Severity"/> level of the validation failure.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder WithError(string errorCode, string message, Severity severity);

    /// <summary>Attaches an error code, message, and property path to the most recently added condition.</summary>
    /// <param name="errorCode">A short machine-readable code identifying the validation rule.</param>
    /// <param name="message">A human-readable description of the failure.</param>
    /// <param name="propertyPath">The dot-separated path to the property that failed (e.g., <c>"Address.City"</c>).</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder WithError(string errorCode, string message, string propertyPath);

    /// <summary>Attaches an error code, message, property path, and severity to the most recently added condition.</summary>
    /// <param name="errorCode">A short machine-readable code identifying the validation rule.</param>
    /// <param name="message">A human-readable description of the failure.</param>
    /// <param name="propertyPath">The dot-separated path to the property that failed.</param>
    /// <param name="severity">The <see cref="Severity"/> level of the validation failure.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder WithError(string errorCode, string message, string propertyPath, Severity severity);

    /// <summary>Sets the severity of the most recently added condition's error metadata.</summary>
    /// <param name="severity">The <see cref="Severity"/> level to assign.</param>
    /// <returns>The builder instance for method chaining.</returns>
    TBuilder WithSeverity(Severity severity);
}
