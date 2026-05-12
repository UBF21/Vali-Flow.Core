using System.Text.Json;

namespace Vali_Flow.Core.Utils;

/// <summary>
/// Provides internal helper methods for JSON-related validations used by expression builder classes.
/// </summary>
internal static class JsonValidation
{
    /// <summary>
    /// Checks whether a given string is a valid JSON document.
    /// </summary>
    /// <param name="val">The string to validate. A <see langword="null"/> or whitespace-only value always returns <see langword="false"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="val"/> can be parsed as a valid JSON document;
    /// <see langword="false"/> if it is <see langword="null"/>, whitespace, or malformed JSON.
    /// </returns>
    /// <remarks>
    /// Parsing is performed via <see cref="JsonDocument.Parse(string)"/> and any
    /// <see cref="JsonException"/> is caught and treated as a parse failure.
    /// The parsed document is disposed immediately to avoid pinned memory.
    /// </remarks>
    internal static bool IsValidJson(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return false;

        try
        {
            using var _ = JsonDocument.Parse(val);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
