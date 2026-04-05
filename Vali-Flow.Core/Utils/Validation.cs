using System.Text.Json;

namespace Vali_Flow.Core.Utils;

/// <summary>
/// Provides helper methods for validations.
/// </summary>
internal static class Validation
{
    /// <summary>
    /// Checks whether a given string is a valid JSON.
    /// </summary>
    /// <param name="val">The string to validate.</param>
    /// <returns><c>true</c> if the string is a valid JSON; otherwise, <c>false</c>.</returns>
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
