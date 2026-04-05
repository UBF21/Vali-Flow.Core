namespace Vali_Flow.Core.Models;

/// <summary>
/// Represents the severity level of a validation error.
/// </summary>
public enum Severity
{
    /// <summary>Informational — does not indicate a failure, used for hints.</summary>
    Info = 0,

    /// <summary>Warning — the rule failed but is not blocking.</summary>
    Warning = 1,

    /// <summary>Error — the rule failed and should be treated as a standard error.</summary>
    Error = 2,

    /// <summary>Critical — the rule failed and is blocking; execution should stop.</summary>
    Critical = 3
}
