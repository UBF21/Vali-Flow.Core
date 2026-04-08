using System.Text.RegularExpressions;

namespace Vali_Flow.Core.RegularExpressions;

/// <summary>
/// Provides regular expressions for validating different formats.
/// </summary>
public static class RegularExpression
{
    /// <summary>
    /// Regular expression for validating email addresses.
    /// </summary>
    /// <remarks>
    /// Accepts email addresses containing alphanumeric characters, dots, hyphens, and the '@' symbol, 
    /// followed by a valid domain.
    /// </remarks>
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Compiled regex for validating email addresses.
    /// </summary>
    /// <remarks>
    /// Prevents leading/trailing dots in the local part and leading/trailing hyphens in domain labels.
    /// Each match call enforces a 5-second timeout. Input that causes catastrophic backtracking
    /// will throw <see cref="RegexMatchTimeoutException"/> rather than hanging.
    /// </remarks>
    public static readonly Regex EmailPattern = new(
        @"^[a-zA-Z0-9]([a-zA-Z0-9._%+\-]*[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9\-]*[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]*[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled, RegexTimeout);

    /// <summary>
    /// Compiled regex for validating Base64-encoded strings.
    /// </summary>
    /// <remarks>
    /// Ensures that the string contains only valid Base64 characters (A-Z, a-z, 0-9, +, /)
    /// and optionally up to two padding '=' characters.
    /// Each match call enforces a 5-second timeout. Input that causes catastrophic backtracking
    /// will throw <see cref="RegexMatchTimeoutException"/> rather than hanging.
    /// </remarks>
    public static readonly Regex Base64Pattern = new(
        @"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{4})$",
        RegexOptions.Compiled, RegexTimeout);

    /// <summary>Compiled regex for validating http/https URLs.</summary>
    /// <remarks>Each match call enforces a 5-second timeout and throws <see cref="RegexMatchTimeoutException"/> on timeout.</remarks>
    public static readonly Regex UrlPattern = new(
        @"^https?://[^\s/$.?#][^\s]*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout);

    /// <summary>Compiled regex for validating GUID strings (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).</summary>
    /// <remarks>Each match call enforces a 5-second timeout and throws <see cref="RegexMatchTimeoutException"/> on timeout.</remarks>
    public static readonly Regex GuidPattern = new(
        @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
        RegexOptions.Compiled, RegexTimeout);

    /// <summary>Compiled regex for validating E.164 phone numbers (7 to 15 digits total, optional leading +).</summary>
    /// <remarks>Each match call enforces a 5-second timeout and throws <see cref="RegexMatchTimeoutException"/> on timeout.</remarks>
    public static readonly Regex PhonePattern = new(
        @"^\+[1-9]\d{6,14}$",
        RegexOptions.Compiled, RegexTimeout);

    /// <summary>
    /// Compiled regex for validating credit card numbers (Visa, Mastercard, Amex, Discover, Diners, JCB).
    /// Accepts 13–19 digits, optionally separated by spaces or hyphens.
    /// Does NOT perform Luhn checksum validation — use this for format checks only.
    /// </summary>
    public static readonly Regex CreditCardPattern = new(
        @"^(?:4[0-9]{12}(?:[0-9]{3})?|(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|6(?:011|5[0-9]{2})[0-9]{12}|(?:2131|1800|35\d{3})\d{11})$",
        RegexOptions.Compiled, RegexTimeout);

    /// <summary>
    /// Compiled regex for validating IPv4 addresses (e.g. 192.168.1.1).
    /// Each octet must be 0–255.
    /// </summary>
    public static readonly Regex IPv4Pattern = new(
        @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$",
        RegexOptions.Compiled, RegexTimeout);

    /// <summary>
    /// Compiled regex for validating IPv6 addresses (full and compressed forms).
    /// </summary>
    public static readonly Regex IPv6Pattern = new(
        @"^(([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]+|::(ffff(:0{1,4})?:)?((25[0-5]|(2[0-4]|1?[0-9])?[0-9])\.){3}(25[0-5]|(2[0-4]|1?[0-9])?[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1?[0-9])?[0-9])\.){3}(25[0-5]|(2[0-4]|1?[0-9])?[0-9]))$",
        RegexOptions.Compiled, RegexTimeout);

    /// <summary>
    /// Compiled regex for validating CSS hex color codes.
    /// Accepts 3-digit (#RGB) and 6-digit (#RRGGBB) formats, case-insensitive.
    /// </summary>
    public static readonly Regex HexColorPattern = new(
        @"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$",
        RegexOptions.Compiled, RegexTimeout);

    /// <summary>
    /// Compiled regex for validating URL slugs.
    /// Allows lowercase letters, digits, and hyphens. Must not start or end with a hyphen.
    /// </summary>
    public static readonly Regex SlugPattern = new(
        @"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.Compiled, RegexTimeout);
}