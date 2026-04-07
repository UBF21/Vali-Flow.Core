using System.Reflection;
using FluentAssertions;
using Vali_Flow.Core.Builder;
using Xunit;

namespace Vali_Flow.Core.Tests;

/// <summary>
/// Guards the ValiFlow / ValiFlowQuery mirror contract.
/// When a new method is added to ValiFlow it must either appear in ValiFlowQuery
/// or be listed in <see cref="KnownNonEfMethods"/> with a documented reason.
/// </summary>
public class SyncTests
{
    /// <summary>
    /// Methods that exist in ValiFlow but are intentionally absent from ValiFlowQuery
    /// because they are not translatable to SQL by EF Core.
    /// </summary>
    private static readonly HashSet<string> KnownNonEfMethods = new(StringComparer.Ordinal)
    {
        // ── Regex / case-insensitive string ──────────────────────────────────
        "Matches", "NotMatches",
        "EqualToIgnoreCase", "NotEqualToIgnoreCase",
        "StartsWithIgnoreCase", "EndsWithIgnoreCase", "ContainsIgnoreCase",
        "IsOneOf", "IsTrimmed",

        // ── Char-class string checks ──────────────────────────────────────────
        "IsLetter", "IsDigit", "IsAlphanumeric", "IsUpperCase", "IsLowerCase",

        // ── JSON ──────────────────────────────────────────────────────────────
        "IsValidJson",

        // ── Collection predicate methods ──────────────────────────────────────
        "All", "Any", "None", "AllMatch", "EachItem", "AnyItem",
        "HasDuplicates", "DistinctCount",

        // ── String pattern checks (regex/wildcard/format — not EF translatable) ─
        "RegexMatch", "MatchesWildcard",
        "IsEmail", "IsPhoneNumber", "IsUrl", "IsGuid",
        "IsCreditCard", "IsIPv4", "IsIPv6", "IsHexColor", "IsSlug",
        "IsBase64", "NotBase64", "IsJson", "NotJson",
        "HasLettersAndNumbers", "HasOnlyDigits", "HasOnlyLetters", "HasSpecialCharacters",

        // ── Numeric / date non-EF ─────────────────────────────────────────────
        "IsCloseTo", "IsBetweenExclusive", "IsLeapYear",

        // ── Nested / global / combine — ValiFlow-only concepts ────────────────
        "ValidateNested", "BuildWithGlobal", "Combine",

        // ── Operators (static, not instance methods in the usual sense) ───────
        "op_BitwiseAnd", "op_BitwiseOr", "op_LogicalNot",

        // ── Object overrides from object (not part of the API contract) ───────
        "ToString", "GetHashCode", "GetType", "Equals",
        "MemberwiseClone", "Finalize",
    };

    [Fact]
    public void ValiFlow_PublicMethods_ArePresentIn_ValiFlowQuery_OrInExclusionList()
    {
        // Use object as the entity type — avoids unresolvable open-generic constraints.
        var valiFlowMethods = typeof(ValiFlow<object>)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        var queryMethods = typeof(ValiFlowQuery<object>)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        var unexpected = valiFlowMethods
            .Except(queryMethods)
            .Except(KnownNonEfMethods)
            .OrderBy(m => m)
            .ToList();

        unexpected.Should().BeEmpty(
            $"the following methods exist in ValiFlow<T> but not in ValiFlowQuery<T> " +
            $"and are not in the known-exclusions list.\n" +
            $"Add them to ValiFlowQuery if EF Core-translatable, " +
            $"or to KnownNonEfMethods if not:\n{string.Join("\n  ", unexpected)}");
    }
}
