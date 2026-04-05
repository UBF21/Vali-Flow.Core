using System.Linq.Expressions;
using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

public record QueryEntity(
    string? Name,
    int Age,
    decimal Salary,
    bool IsActive,
    DateTime CreatedAt,
    DateTimeOffset UpdatedAt,
    DateOnly BirthDate,
    TimeOnly WorkStart,
    List<string> Tags,
    int? OptionalScore);

public class ValiFlowQueryTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static QueryEntity MakeEntity(
        string? name = "Alice",
        int age = 30,
        decimal salary = 50_000m,
        bool isActive = true,
        DateTime createdAt = default,
        DateTimeOffset updatedAt = default,
        DateOnly birthDate = default,
        TimeOnly workStart = default,
        List<string>? tags = null,
        int? optionalScore = 10)
        => new(
            name,
            age,
            salary,
            isActive,
            createdAt == default ? DateTime.UtcNow : createdAt,
            updatedAt == default ? DateTimeOffset.UtcNow : updatedAt,
            birthDate == default ? new DateOnly(1995, 6, 15) : birthDate,
            workStart == default ? new TimeOnly(9, 0) : workStart,
            tags ?? new List<string> { "a", "b" },
            optionalScore);

    // ═══════════════════════════════════════════════════════════════════════
    // Boolean
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void IsTrue_ActiveEntity_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsTrue(e => e.IsActive).Build().Compile();
        filter(MakeEntity(isActive: true)).Should().BeTrue();
    }

    [Fact]
    public void IsTrue_InactiveEntity_ReturnsFalse()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsTrue(e => e.IsActive).Build().Compile();
        filter(MakeEntity(isActive: false)).Should().BeFalse();
    }

    [Fact]
    public void IsFalse_InactiveEntity_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsFalse(e => e.IsActive).Build().Compile();
        filter(MakeEntity(isActive: false)).Should().BeTrue();
    }

    [Fact]
    public void IsFalse_ActiveEntity_ReturnsFalse()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsFalse(e => e.IsActive).Build().Compile();
        filter(MakeEntity(isActive: true)).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Comparison
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void NotNull_NonNullName_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().NotNull(e => e.Name).Build().Compile();
        filter(MakeEntity(name: "Alice")).Should().BeTrue();
    }

    [Fact]
    public void NotNull_NullName_ReturnsFalse()
    {
        var filter = new ValiFlowQuery<QueryEntity>().NotNull(e => e.Name).Build().Compile();
        filter(MakeEntity(name: null)).Should().BeFalse();
    }

    [Fact]
    public void Null_NullName_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().Null(e => e.Name).Build().Compile();
        filter(MakeEntity(name: null)).Should().BeTrue();
    }

    [Fact]
    public void EqualTo_MatchingAge_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().EqualTo(e => e.Age, 30).Build().Compile();
        filter(MakeEntity(age: 30)).Should().BeTrue();
        filter(MakeEntity(age: 25)).Should().BeFalse();
    }

    [Fact]
    public void NotEqualTo_DifferentAge_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().NotEqualTo(e => e.Age, 30).Build().Compile();
        filter(MakeEntity(age: 25)).Should().BeTrue();
        filter(MakeEntity(age: 30)).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // String
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void MinLength_NameLongEnough_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().MinLength(e => e.Name, 3).Build().Compile();
        filter(MakeEntity(name: "Alice")).Should().BeTrue();
        filter(MakeEntity(name: "Al")).Should().BeFalse();
    }

    [Fact]
    public void MaxLength_NameShortEnough_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().MaxLength(e => e.Name, 5).Build().Compile();
        filter(MakeEntity(name: "Alice")).Should().BeTrue();
        filter(MakeEntity(name: "Alexander")).Should().BeFalse();
    }

    [Fact]
    public void ExactLength_ExactMatch_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().ExactLength(e => e.Name, 5).Build().Compile();
        filter(MakeEntity(name: "Alice")).Should().BeTrue();
        filter(MakeEntity(name: "Al")).Should().BeFalse();
    }

    [Fact]
    public void LengthBetween_WithinRange_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().LengthBetween(e => e.Name, 3, 7).Build().Compile();
        filter(MakeEntity(name: "Alice")).Should().BeTrue();
        filter(MakeEntity(name: "Al")).Should().BeFalse();
        filter(MakeEntity(name: "Alexander")).Should().BeFalse();
    }

    [Fact]
    public void IsNullOrEmpty_NullName_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsNullOrEmpty(e => e.Name).Build().Compile();
        filter(MakeEntity(name: null)).Should().BeTrue();
        filter(MakeEntity(name: "")).Should().BeTrue();
        filter(MakeEntity(name: "Alice")).Should().BeFalse();
    }

    [Fact]
    public void IsNotNullOrEmpty_NonEmptyName_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsNotNullOrEmpty(e => e.Name).Build().Compile();
        filter(MakeEntity(name: "Alice")).Should().BeTrue();
        filter(MakeEntity(name: null)).Should().BeFalse();
        filter(MakeEntity(name: "")).Should().BeFalse();
    }

    [Fact]
    public void IsNullOrWhiteSpace_WhitespaceOnly_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsNullOrWhiteSpace(e => e.Name).Build().Compile();
        filter(MakeEntity(name: "   ")).Should().BeTrue();
        filter(MakeEntity(name: null)).Should().BeTrue();
        filter(MakeEntity(name: "Alice")).Should().BeFalse();
    }

    [Fact]
    public void IsNotNullOrWhiteSpace_NonBlankName_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsNotNullOrWhiteSpace(e => e.Name).Build().Compile();
        filter(MakeEntity(name: "Alice")).Should().BeTrue();
        filter(MakeEntity(name: "   ")).Should().BeFalse();
        filter(MakeEntity(name: null)).Should().BeFalse();
    }

    [Fact]
    public void IsNullOrWhiteSpace_EmptyString_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsNullOrWhiteSpace(e => e.Name).Build().Compile();
        filter(MakeEntity(name: "")).Should().BeTrue();
    }

    [Fact]
    public void IsNotNullOrWhiteSpace_EmptyString_ReturnsFalse()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsNotNullOrWhiteSpace(e => e.Name).Build().Compile();
        filter(MakeEntity(name: "")).Should().BeFalse();
    }

    [Fact]
    public void StartsWith_MatchingPrefix_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().StartsWith(e => e.Name, "Ali").Build().Compile();
        filter(MakeEntity(name: "Alice")).Should().BeTrue();
        filter(MakeEntity(name: "Bob")).Should().BeFalse();
    }

    [Fact]
    public void EndsWith_MatchingSuffix_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().EndsWith(e => e.Name, "ice").Build().Compile();
        filter(MakeEntity(name: "Alice")).Should().BeTrue();
        filter(MakeEntity(name: "Bob")).Should().BeFalse();
    }

    [Fact]
    public void Contains_MatchingSubstring_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().Contains(e => e.Name, "lic").Build().Compile();
        filter(MakeEntity(name: "Alice")).Should().BeTrue();
        filter(MakeEntity(name: "Bob")).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Collection
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void NotEmpty_WithItems_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().NotEmpty<string>(e => e.Tags).Build().Compile();
        filter(MakeEntity(tags: new List<string> { "x" })).Should().BeTrue();
        filter(MakeEntity(tags: new List<string>())).Should().BeFalse();
    }

    [Fact]
    public void Empty_NoItems_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().Empty<string>(e => e.Tags).Build().Compile();
        filter(MakeEntity(tags: new List<string>())).Should().BeTrue();
        filter(MakeEntity(tags: new List<string> { "x" })).Should().BeFalse();
    }

    [Fact]
    public void In_ValueInList_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().In(e => e.Age, new List<int> { 25, 30, 35 }).Build().Compile();
        filter(MakeEntity(age: 30)).Should().BeTrue();
        filter(MakeEntity(age: 40)).Should().BeFalse();
    }

    [Fact]
    public void NotIn_ValueNotInList_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().NotIn(e => e.Age, new List<int> { 25, 30, 35 }).Build().Compile();
        filter(MakeEntity(age: 40)).Should().BeTrue();
        filter(MakeEntity(age: 30)).Should().BeFalse();
    }

    [Fact]
    public void Count_ExactCount_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().Count<string>(e => e.Tags, 2).Build().Compile();
        filter(MakeEntity(tags: new List<string> { "a", "b" })).Should().BeTrue();
        filter(MakeEntity(tags: new List<string> { "a" })).Should().BeFalse();
    }

    [Fact]
    public void MinCount_AtLeastN_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().MinCount<string>(e => e.Tags, 2).Build().Compile();
        filter(MakeEntity(tags: new List<string> { "a", "b", "c" })).Should().BeTrue();
        filter(MakeEntity(tags: new List<string> { "a" })).Should().BeFalse();
    }

    [Fact]
    public void MaxCount_AtMostN_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().MaxCount<string>(e => e.Tags, 2).Build().Compile();
        filter(MakeEntity(tags: new List<string> { "a", "b" })).Should().BeTrue();
        filter(MakeEntity(tags: new List<string> { "a", "b", "c" })).Should().BeFalse();
    }

    [Fact]
    public void CountBetween_WithinRange_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>()
            .CountBetween<string>(e => e.Tags, 2, 4)
            .Build().Compile();

        filter(MakeEntity(tags: new List<string> { "a", "b" })).Should().BeTrue();
        filter(MakeEntity(tags: new List<string> { "a", "b", "c", "d" })).Should().BeTrue();
        filter(MakeEntity(tags: new List<string> { "a" })).Should().BeFalse();
        filter(MakeEntity(tags: new List<string> { "a", "b", "c", "d", "e" })).Should().BeFalse();
    }

    private record TagsEntity(List<string>? Tags);

    [Fact]
    public void CountBetween_NullCollection_ReturnsFalse()
    {
        var filter = new ValiFlowQuery<TagsEntity>()
            .CountBetween<string>(e => e.Tags, 1, 5)
            .Build().Compile();

        filter(new TagsEntity(null)).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Numeric — int
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Zero_ZeroAge_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().Zero(e => e.Age).Build().Compile();
        filter(MakeEntity(age: 0)).Should().BeTrue();
        filter(MakeEntity(age: 1)).Should().BeFalse();
    }

    [Fact]
    public void Positive_PositiveAge_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().Positive(e => e.Age).Build().Compile();
        filter(MakeEntity(age: 30)).Should().BeTrue();
        filter(MakeEntity(age: 0)).Should().BeFalse();
        filter(MakeEntity(age: -1)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_Int_AboveThreshold_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().GreaterThan(e => e.Age, 18).Build().Compile();
        filter(MakeEntity(age: 30)).Should().BeTrue();
        filter(MakeEntity(age: 18)).Should().BeFalse();
        filter(MakeEntity(age: 10)).Should().BeFalse();
    }

    [Fact]
    public void InRange_Int_WithinBounds_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().InRange(e => e.Age, 18, 65).Build().Compile();
        filter(MakeEntity(age: 30)).Should().BeTrue();
        filter(MakeEntity(age: 18)).Should().BeTrue();
        filter(MakeEntity(age: 65)).Should().BeTrue();
        filter(MakeEntity(age: 17)).Should().BeFalse();
        filter(MakeEntity(age: 66)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThanOrEqualTo_Int_AtBoundary_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().GreaterThanOrEqualTo(e => e.Age, 30).Build().Compile();
        filter(MakeEntity(age: 30)).Should().BeTrue();
        filter(MakeEntity(age: 31)).Should().BeTrue();
        filter(MakeEntity(age: 29)).Should().BeFalse();
    }

    [Fact]
    public void LessThan_Int_BelowThreshold_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().LessThan(e => e.Age, 65).Build().Compile();
        filter(MakeEntity(age: 30)).Should().BeTrue();
        filter(MakeEntity(age: 65)).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Numeric — decimal
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void GreaterThan_Decimal_AboveThreshold_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().GreaterThan(e => e.Salary, 40_000m).Build().Compile();
        filter(MakeEntity(salary: 50_000m)).Should().BeTrue();
        filter(MakeEntity(salary: 30_000m)).Should().BeFalse();
    }

    [Fact]
    public void InRange_Decimal_WithinBounds_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().InRange(e => e.Salary, 30_000m, 100_000m).Build().Compile();
        filter(MakeEntity(salary: 50_000m)).Should().BeTrue();
        filter(MakeEntity(salary: 20_000m)).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Nullable Numeric
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void IsNullOrZero_NullScore_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsNullOrZero(e => e.OptionalScore).Build().Compile();
        filter(MakeEntity(optionalScore: null)).Should().BeTrue();
        filter(MakeEntity(optionalScore: 0)).Should().BeTrue();
        filter(MakeEntity(optionalScore: 5)).Should().BeFalse();
    }

    [Fact]
    public void HasValue_NonNullScore_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().HasValue(e => e.OptionalScore).Build().Compile();
        filter(MakeEntity(optionalScore: 5)).Should().BeTrue();
        filter(MakeEntity(optionalScore: null)).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DateTime
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void DateTime_IsBefore_EarlierDate_ReturnsTrue()
    {
        var date = new DateTime(2030, 1, 1);
        var filter = new ValiFlowQuery<QueryEntity>().IsBefore(e => e.CreatedAt, date).Build().Compile();
        filter(MakeEntity(createdAt: new DateTime(2025, 1, 1))).Should().BeTrue();
        filter(MakeEntity(createdAt: new DateTime(2031, 1, 1))).Should().BeFalse();
    }

    [Fact]
    public void DateTime_IsAfter_LaterDate_ReturnsTrue()
    {
        var date = new DateTime(2020, 1, 1);
        var filter = new ValiFlowQuery<QueryEntity>().IsAfter(e => e.CreatedAt, date).Build().Compile();
        filter(MakeEntity(createdAt: new DateTime(2025, 1, 1))).Should().BeTrue();
        filter(MakeEntity(createdAt: new DateTime(2019, 1, 1))).Should().BeFalse();
    }

    [Fact]
    public void DateTime_BetweenDates_WithinRange_ReturnsTrue()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2026, 12, 31);
        var filter = new ValiFlowQuery<QueryEntity>().BetweenDates(e => e.CreatedAt, start, end).Build().Compile();
        filter(MakeEntity(createdAt: new DateTime(2025, 6, 15))).Should().BeTrue();
        filter(MakeEntity(createdAt: new DateTime(2023, 1, 1))).Should().BeFalse();
    }

    [Fact]
    public void DateTime_IsInYear_MatchingYear_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsInYear(e => e.CreatedAt, 2025).Build().Compile();
        filter(MakeEntity(createdAt: new DateTime(2025, 6, 1))).Should().BeTrue();
        filter(MakeEntity(createdAt: new DateTime(2024, 6, 1))).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DateTimeOffset
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void DateTimeOffset_IsBefore_EarlierOffset_ReturnsTrue()
    {
        var date = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlowQuery<QueryEntity>().IsBefore(e => e.UpdatedAt, date).Build().Compile();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2031, 1, 1, 0, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_IsAfter_LaterOffset_ReturnsTrue()
    {
        var date = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlowQuery<QueryEntity>().IsAfter(e => e.UpdatedAt, date).Build().Compile();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_BetweenDates_WithinRange_ReturnsTrue()
    {
        var from = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlowQuery<QueryEntity>().BetweenDates(e => e.UpdatedAt, from, to).Build().Compile();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 6, 15, 0, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DateOnly
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void DateOnly_IsBefore_EarlierDate_ReturnsTrue()
    {
        var date = new DateOnly(2000, 1, 1);
        var filter = new ValiFlowQuery<QueryEntity>().IsBefore(e => e.BirthDate, date).Build().Compile();
        filter(MakeEntity(birthDate: new DateOnly(1995, 6, 15))).Should().BeTrue();
        filter(MakeEntity(birthDate: new DateOnly(2001, 1, 1))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_IsAfter_LaterDate_ReturnsTrue()
    {
        var date = new DateOnly(1990, 1, 1);
        var filter = new ValiFlowQuery<QueryEntity>().IsAfter(e => e.BirthDate, date).Build().Compile();
        filter(MakeEntity(birthDate: new DateOnly(1995, 6, 15))).Should().BeTrue();
        filter(MakeEntity(birthDate: new DateOnly(1985, 1, 1))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_IsFirstDayOfMonth_FirstDay_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsFirstDayOfMonth(e => e.BirthDate).Build().Compile();
        filter(MakeEntity(birthDate: new DateOnly(1995, 6, 1))).Should().BeTrue();
        filter(MakeEntity(birthDate: new DateOnly(1995, 6, 15))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_IsInMonth_MatchingMonth_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsInMonth(e => e.BirthDate, 6).Build().Compile();
        filter(MakeEntity(birthDate: new DateOnly(1995, 6, 15))).Should().BeTrue();
        filter(MakeEntity(birthDate: new DateOnly(1995, 7, 15))).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // TimeOnly
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void TimeOnly_IsBefore_EarlierTime_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsBefore(e => e.WorkStart, new TimeOnly(12, 0)).Build().Compile();
        filter(MakeEntity(workStart: new TimeOnly(9, 0))).Should().BeTrue();
        filter(MakeEntity(workStart: new TimeOnly(14, 0))).Should().BeFalse();
    }

    [Fact]
    public void TimeOnly_IsBetween_WithinRange_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsBetween(e => e.WorkStart, new TimeOnly(8, 0), new TimeOnly(12, 0)).Build().Compile();
        filter(MakeEntity(workStart: new TimeOnly(9, 0))).Should().BeTrue();
        filter(MakeEntity(workStart: new TimeOnly(13, 0))).Should().BeFalse();
    }

    [Fact]
    public void TimeOnly_IsAM_MorningTime_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsAM(e => e.WorkStart).Build().Compile();
        filter(MakeEntity(workStart: new TimeOnly(9, 0))).Should().BeTrue();
        filter(MakeEntity(workStart: new TimeOnly(14, 0))).Should().BeFalse();
    }

    [Fact]
    public void TimeOnly_IsInHour_MatchingHour_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsInHour(e => e.WorkStart, 9).Build().Compile();
        filter(MakeEntity(workStart: new TimeOnly(9, 30))).Should().BeTrue();
        filter(MakeEntity(workStart: new TimeOnly(10, 0))).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Inherited BaseExpression behavior
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Build_NoConditions_ReturnsTrueForAll()
    {
        var filter = new ValiFlowQuery<QueryEntity>().Build().Compile();
        filter(MakeEntity()).Should().BeTrue();
        filter(MakeEntity(name: null, age: -999)).Should().BeTrue();
    }

    [Fact]
    public void Or_TwoConditions_EitherSatisfies()
    {
        var filter = new ValiFlowQuery<QueryEntity>()
            .EqualTo(e => e.Age, 25)
            .Or()
            .EqualTo(e => e.Age, 30)
            .Build().Compile();

        filter(MakeEntity(age: 25)).Should().BeTrue();
        filter(MakeEntity(age: 30)).Should().BeTrue();
        filter(MakeEntity(age: 35)).Should().BeFalse();
    }

    [Fact]
    public void And_TwoConditions_BothRequired()
    {
        var filter = new ValiFlowQuery<QueryEntity>()
            .IsTrue(e => e.IsActive)
            .And()
            .GreaterThan(e => e.Age, 18)
            .Build().Compile();

        filter(MakeEntity(isActive: true, age: 30)).Should().BeTrue();
        filter(MakeEntity(isActive: false, age: 30)).Should().BeFalse();
        filter(MakeEntity(isActive: true, age: 15)).Should().BeFalse();
    }

    [Fact]
    public void AddSubGroup_GroupCondition_Applied()
    {
        var filter = new ValiFlowQuery<QueryEntity>()
            .IsTrue(e => e.IsActive)
            .AddSubGroup(g => g
                .Add(e => e.Age > 18)
                .Or()
                .Add(e => e.Salary > 100_000m))
            .Build().Compile();

        filter(MakeEntity(isActive: true, age: 25, salary: 50_000m)).Should().BeTrue();
        filter(MakeEntity(isActive: true, age: 15, salary: 200_000m)).Should().BeTrue();
        filter(MakeEntity(isActive: false, age: 25, salary: 50_000m)).Should().BeFalse();
        filter(MakeEntity(isActive: true, age: 15, salary: 50_000m)).Should().BeFalse();
    }

    [Fact]
    public void BuildCached_ReturnsConsistentResult()
    {
        var builder = new ValiFlowQuery<QueryEntity>().GreaterThan(e => e.Age, 18);
        var fn1 = builder.BuildCached();
        var fn2 = builder.BuildCached();
        fn1.Should().BeSameAs(fn2);
        fn1(MakeEntity(age: 30)).Should().BeTrue();
        fn1(MakeEntity(age: 10)).Should().BeFalse();
    }

    [Fact]
    public void Validate_AllConditionsMet_ReturnsIsValidTrue()
    {
        var result = new ValiFlowQuery<QueryEntity>()
            .IsTrue(e => e.IsActive)
            .GreaterThan(e => e.Age, 18)
            .Validate(MakeEntity(isActive: true, age: 30));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ConditionFailed_ReturnsIsValidFalse()
    {
        var result = new ValiFlowQuery<QueryEntity>()
            .IsTrue(e => e.IsActive)
            .WithMessage("Must be active")
            .Validate(MakeEntity(isActive: false));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateNested_EmptyConfigure_ThrowsArgumentException()
    {
        var act = () => new ValiFlowQuery<QueryEntity>()
            .ValidateNested(e => e.Name, _ => { });

        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least one condition*");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Surface-area guards — métodos unsafe NO deben existir en ValiFlowQuery
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void ValiFlowQuery_DoesNotExposeRegexMatch()
    {
        typeof(ValiFlowQuery<QueryEntity>).GetMethod("RegexMatch").Should().BeNull();
    }

    [Fact]
    public void ValiFlowQuery_IsToday_DateOnly_CapturesAtBuildTime_ReturnsTrue()
    {
        // IsToday is now EF Core-safe: today is captured at build time as a constant parameter
        var validator = new ValiFlowQuery<QueryEntity>()
            .IsToday(e => e.BirthDate);
        var entity = MakeEntity(birthDate: DateOnly.FromDateTime(DateTime.UtcNow));
        validator.IsValid(entity).Should().BeTrue();
    }

    [Fact]
    public void ValiFlowQuery_IsEven_ExposedAndTranslatable()
    {
        // IsEven uses val % 2 == 0 — EF Core translates modulo on all major providers
        typeof(ValiFlowQuery<QueryEntity>).GetMethod("IsEven", [typeof(Expression<Func<QueryEntity, int>>)])
            .Should().NotBeNull();
        var even = MakeEntity(age: 4);
        var odd  = MakeEntity(age: 3);
        var validator = new ValiFlowQuery<QueryEntity>().IsEven(x => x.Age);
        validator.IsValid(even).Should().BeTrue();
        validator.IsValid(odd).Should().BeFalse();
    }

    [Fact]
    public void ValiFlowQuery_DoesNotExposeIsJson()
    {
        typeof(ValiFlowQuery<QueryEntity>).GetMethod("IsJson").Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Combine / operators
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Combine_BothNonEmpty_CombinesWithAnd()
    {
        var left = new ValiFlowQuery<QueryEntity>().IsTrue(e => e.IsActive);
        var right = new ValiFlowQuery<QueryEntity>().GreaterThan(e => e.Age, 18);
        var expr = ValiFlowQuery<QueryEntity>.Combine(left, right, and: true).Compile();

        expr(MakeEntity(isActive: true, age: 30)).Should().BeTrue();
        expr(MakeEntity(isActive: false, age: 30)).Should().BeFalse();
    }

    [Fact]
    public void Combine_LeftEmpty_ReturnsRightExpression()
    {
        var left = new ValiFlowQuery<QueryEntity>(); // sin condiciones → _ => true
        var right = new ValiFlowQuery<QueryEntity>().GreaterThan(e => e.Age, 18);
        var expr = ValiFlowQuery<QueryEntity>.Combine(left, right, and: true).Compile();

        // El árbol devuelto debe ser exactamente el de `right`, no `Constant(true) AND right`
        expr(MakeEntity(age: 30)).Should().BeTrue();
        expr(MakeEntity(age: 10)).Should().BeFalse();
    }

    [Fact]
    public void Combine_RightEmpty_ReturnsLeftExpression()
    {
        var left = new ValiFlowQuery<QueryEntity>().GreaterThan(e => e.Age, 18);
        var right = new ValiFlowQuery<QueryEntity>(); // sin condiciones → _ => true
        var expr = ValiFlowQuery<QueryEntity>.Combine(left, right, and: true).Compile();

        expr(MakeEntity(age: 30)).Should().BeTrue();
        expr(MakeEntity(age: 10)).Should().BeFalse();
    }

    [Fact]
    public void AndOperator_CombinesTwoBuilders()
    {
        var left = new ValiFlowQuery<QueryEntity>().IsTrue(e => e.IsActive);
        var right = new ValiFlowQuery<QueryEntity>().GreaterThan(e => e.Age, 18);
        var expr = (left & right).Compile();

        expr(MakeEntity(isActive: true, age: 30)).Should().BeTrue();
        expr(MakeEntity(isActive: false, age: 30)).Should().BeFalse();
    }

    [Fact]
    public void OrOperator_CombinesTwoBuilders()
    {
        var left = new ValiFlowQuery<QueryEntity>().EqualTo(e => e.Age, 25);
        var right = new ValiFlowQuery<QueryEntity>().EqualTo(e => e.Age, 30);
        var expr = (left | right).Compile();

        expr(MakeEntity(age: 25)).Should().BeTrue();
        expr(MakeEntity(age: 30)).Should().BeTrue();
        expr(MakeEntity(age: 35)).Should().BeFalse();
    }

    [Fact]
    public void NotOperator_NegatesExpression()
    {
        var builder = new ValiFlowQuery<QueryEntity>().IsTrue(e => e.IsActive);
        var expr = (!builder).Compile();

        expr(MakeEntity(isActive: false)).Should().BeTrue();
        expr(MakeEntity(isActive: true)).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // QueryEntityEx — nullable numeric / DateTime / DateOnly tests
    // ═══════════════════════════════════════════════════════════════════════

    private record QueryEntityEx(
        int? NullableInt, long? NullableLong, decimal? NullableDecimal,
        double? NullableDouble, float? NullableFloat,
        DateTime CreatedAt, DateOnly BirthDate);

    // ── IsNullOrZero(float?) ──────────────────────────────────────────────────

    [Fact]
    public void IsNullOrZero_Float_NullValue_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .IsNullOrZero(e => e.NullableFloat)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
    }

    [Fact]
    public void IsNullOrZero_Float_ZeroValue_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .IsNullOrZero(e => e.NullableFloat)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, 0f, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
    }

    [Fact]
    public void IsNullOrZero_Float_NonZeroValue_ReturnsFalse()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .IsNullOrZero(e => e.NullableFloat)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, 1.5f, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
    }

    // ── double? overloads ─────────────────────────────────────────────────────

    [Fact]
    public void GreaterThan_NullableDouble_MatchesAboveThreshold()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .GreaterThan(e => e.NullableDouble, 5.0)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, 6.0, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, 4.0, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
    }

    [Fact]
    public void LessThan_NullableDouble_MatchesBelowThreshold()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .LessThan(e => e.NullableDouble, 5.0)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, 4.0, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, 6.0, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
    }

    [Fact]
    public void InRange_NullableDouble_MatchesWithinRange()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .InRange(e => e.NullableDouble, 1.0, 10.0)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, 5.0, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, 11.0, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
    }

    // ── FutureDate / PastDate (DateTimeOffset) ───────────────────────────────

    [Fact]
    public void FutureDate_DateTimeOffset_FutureValue_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>()
            .FutureDate(e => e.UpdatedAt)
            .Build().Compile();

        filter(MakeEntity(updatedAt: DateTimeOffset.UtcNow.AddDays(1))).Should().BeTrue();
        filter(MakeEntity(updatedAt: DateTimeOffset.UtcNow.AddDays(-1))).Should().BeFalse();
    }

    [Fact]
    public void PastDate_DateTimeOffset_PastValue_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntity>()
            .PastDate(e => e.UpdatedAt)
            .Build().Compile();

        filter(MakeEntity(updatedAt: DateTimeOffset.UtcNow.AddDays(-1))).Should().BeTrue();
        filter(MakeEntity(updatedAt: DateTimeOffset.UtcNow.AddDays(1))).Should().BeFalse();
    }

    // ── FutureDate / PastDate (DateTime) ─────────────────────────────────────

    [Fact]
    public void ValiFlowQuery_FutureDate_DateTime_FutureDate_ReturnsTrue()
    {
        // Arrange
        var filter = new ValiFlowQuery<QueryEntity>()
            .FutureDate(e => e.CreatedAt)
            .Build().Compile();

        // Act & Assert
        filter(MakeEntity(createdAt: DateTime.UtcNow.AddDays(1))).Should().BeTrue();
        filter(MakeEntity(createdAt: DateTime.UtcNow.AddDays(-1))).Should().BeFalse();
    }

    [Fact]
    public void ValiFlowQuery_PastDate_DateTime_PastDate_ReturnsTrue()
    {
        // Arrange
        var filter = new ValiFlowQuery<QueryEntity>()
            .PastDate(e => e.CreatedAt)
            .Build().Compile();

        // Act & Assert
        filter(MakeEntity(createdAt: DateTime.UtcNow.AddDays(-1))).Should().BeTrue();
        filter(MakeEntity(createdAt: DateTime.UtcNow.AddDays(1))).Should().BeFalse();
    }

    // ── float? overloads ──────────────────────────────────────────────────────

    [Fact]
    public void GreaterThan_NullableFloat_MatchesAboveThreshold()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .GreaterThan(e => e.NullableFloat, 5f)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, 6f, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, null, 4f, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
    }

    [Fact]
    public void LessThan_NullableFloat_MatchesBelowThreshold()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .LessThan(e => e.NullableFloat, 5f)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, 4f, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, null, 6f, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
    }

    [Fact]
    public void InRange_NullableFloat_MatchesWithinRange()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .InRange(e => e.NullableFloat, 1f, 10f)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, 5f, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, null, 11f, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
    }

    // ── short? overloads ──────────────────────────────────────────────────────

    private record ShortNullableEntity(short? Score);

    [Fact]
    public void IsNullOrZero_ShortNullable_NullValue_ReturnsTrue()
    {
        var filter = new ValiFlowQuery<ShortNullableEntity>()
            .IsNullOrZero(e => e.Score)
            .Build().Compile();

        filter(new ShortNullableEntity(null)).Should().BeTrue();
        filter(new ShortNullableEntity(0)).Should().BeTrue();
        filter(new ShortNullableEntity(1)).Should().BeFalse();
    }

    [Fact]
    public void HasValue_ShortNullable_ReturnsTrueWhenHasValue()
    {
        var filter = new ValiFlowQuery<ShortNullableEntity>()
            .HasValue(e => e.Score)
            .Build().Compile();

        filter(new ShortNullableEntity(5)).Should().BeTrue();
        filter(new ShortNullableEntity(null)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_ShortNullable_MatchesAboveThreshold()
    {
        var filter = new ValiFlowQuery<ShortNullableEntity>()
            .GreaterThan(e => e.Score, (short)10)
            .Build().Compile();

        filter(new ShortNullableEntity(15)).Should().BeTrue();
        filter(new ShortNullableEntity(5)).Should().BeFalse();
        filter(new ShortNullableEntity(null)).Should().BeFalse();
    }

    [Fact]
    public void LessThan_ShortNullable_MatchesBelowThreshold()
    {
        var filter = new ValiFlowQuery<ShortNullableEntity>()
            .LessThan(e => e.Score, (short)10)
            .Build().Compile();

        filter(new ShortNullableEntity(5)).Should().BeTrue();
        filter(new ShortNullableEntity(15)).Should().BeFalse();
        filter(new ShortNullableEntity(null)).Should().BeFalse();
    }

    [Fact]
    public void InRange_ShortNullable_MatchesWithinRange()
    {
        var filter = new ValiFlowQuery<ShortNullableEntity>()
            .InRange(e => e.Score, (short)1, (short)100)
            .Build().Compile();

        filter(new ShortNullableEntity(50)).Should().BeTrue();
        filter(new ShortNullableEntity(0)).Should().BeFalse();
        filter(new ShortNullableEntity(null)).Should().BeFalse();
    }

    // ── IsBefore / IsAfter / ExactDate (DateTime) ────────────────────────

    [Fact]
    public void IsBefore_DateTime_MatchesEarlierDate()
    {
        var boundary = new DateTime(2026, 3, 15);
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .IsBefore(e => e.CreatedAt, boundary)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 3, 14, 23, 59, 59), DateOnly.MinValue)).Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 3, 15, 0, 0, 0), DateOnly.MinValue)).Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 3, 16, 0, 0, 0), DateOnly.MinValue)).Should().BeFalse();
    }

    [Fact]
    public void IsAfter_DateTime_MatchesLaterDate()
    {
        var boundary = new DateTime(2026, 3, 15);
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .IsAfter(e => e.CreatedAt, boundary)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 3, 16, 0, 0, 0), DateOnly.MinValue)).Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 3, 15, 0, 0, 0), DateOnly.MinValue)).Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 3, 14, 0, 0, 0), DateOnly.MinValue)).Should().BeFalse();
    }

    [Fact]
    public void ExactDate_DateTime_MatchesSameDay()
    {
        var target = new DateTime(2026, 3, 15);
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .ExactDate(e => e.CreatedAt, target)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 3, 15, 12, 30, 0), DateOnly.MinValue)).Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 3, 15, 0, 0, 0), DateOnly.MinValue)).Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 3, 14, 23, 59, 59), DateOnly.MinValue)).Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 3, 16, 0, 0, 0), DateOnly.MinValue)).Should().BeFalse();
    }

    // ── SameMonthAs / SameYearAs ──────────────────────────────────────────────

    [Fact]
    public void SameMonthAs_DateTime_MatchesSameMonth()
    {
        var reference = new DateTime(2026, 3, 10);
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .SameMonthAs(e => e.CreatedAt, reference)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 3, 28), DateOnly.MinValue)).Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 4, 1), DateOnly.MinValue)).Should().BeFalse();
    }

    [Fact]
    public void SameYearAs_DateTime_MatchesSameYear()
    {
        var reference = new DateTime(2026, 1, 1);
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .SameYearAs(e => e.CreatedAt, reference)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 12, 31), DateOnly.MinValue)).Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2025, 6, 15), DateOnly.MinValue)).Should().BeFalse();
    }

    // ── IsInMonth(DateTime) ───────────────────────────────────────────────────

    [Fact]
    public void IsInMonth_DateTime_MatchesCorrectMonth()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .IsInMonth(e => e.CreatedAt, 3)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 3, 15), DateOnly.MinValue)).Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, null, null,
            new DateTime(2026, 4, 15), DateOnly.MinValue)).Should().BeFalse();
    }

    // ── Guard: IsTrue null selector ───────────────────────────────────────────

    [Fact]
    public void IsTrue_NullSelector_ThrowsArgumentNullException()
    {
        var builder = new ValiFlowQuery<QueryEntityEx>();
        Action act = () => builder.IsTrue(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Cross-property InRange (ValiFlowQuery) ────────────────────────────────

    private record RangeEntity(int Value, int Min, int Max, decimal DValue, decimal DMin, decimal DMax);

    [Fact]
    public void InRange_CrossProperty_Int_MatchesWhenValueBetweenMinMax()
    {
        var filter = new ValiFlowQuery<RangeEntity>()
            .InRange(e => e.Value, e => e.Min, e => e.Max)
            .Build().Compile();

        filter(new RangeEntity(5, 1, 10, 0, 0, 0)).Should().BeTrue();
        filter(new RangeEntity(1, 1, 10, 0, 0, 0)).Should().BeTrue();
        filter(new RangeEntity(10, 1, 10, 0, 0, 0)).Should().BeTrue();
        filter(new RangeEntity(0, 1, 10, 0, 0, 0)).Should().BeFalse();
        filter(new RangeEntity(11, 1, 10, 0, 0, 0)).Should().BeFalse();
    }

    [Fact]
    public void InRange_CrossProperty_Decimal_MatchesWhenValueBetweenMinMax()
    {
        var filter = new ValiFlowQuery<RangeEntity>()
            .InRange(e => e.DValue, e => e.DMin, e => e.DMax)
            .Build().Compile();

        filter(new RangeEntity(0, 0, 0, 5m, 1m, 10m)).Should().BeTrue();
        filter(new RangeEntity(0, 0, 0, 0m, 1m, 10m)).Should().BeFalse();
    }

    // ── BetweenDates cross-property (ValiFlowQuery) ───────────────────────────

    private record DateRangeEntity(DateTime Value, DateTime Start, DateTime End, DateOnly DValue, DateOnly DStart, DateOnly DEnd);

    [Fact]
    public void BetweenDates_CrossProperty_DateTime_MatchesWhenValueInRange()
    {
        var filter = new ValiFlowQuery<DateRangeEntity>()
            .BetweenDates(e => e.Value, e => e.Start, e => e.End)
            .Build().Compile();

        filter(new DateRangeEntity(
            new DateTime(2026, 3, 15),
            new DateTime(2026, 3, 10),
            new DateTime(2026, 3, 20),
            DateOnly.MinValue, DateOnly.MinValue, DateOnly.MinValue)).Should().BeTrue();

        filter(new DateRangeEntity(
            new DateTime(2026, 3, 25),
            new DateTime(2026, 3, 10),
            new DateTime(2026, 3, 20),
            DateOnly.MinValue, DateOnly.MinValue, DateOnly.MinValue)).Should().BeFalse();
    }

    [Fact]
    public void BetweenDates_CrossProperty_DateOnly_MatchesWhenValueInRange()
    {
        var filter = new ValiFlowQuery<DateRangeEntity>()
            .BetweenDates(e => e.DValue, e => e.DStart, e => e.DEnd)
            .Build().Compile();

        filter(new DateRangeEntity(
            DateTime.MinValue,
            DateTime.MinValue,
            DateTime.MinValue,
            new DateOnly(2026, 3, 15),
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 20))).Should().BeTrue();

        filter(new DateRangeEntity(
            DateTime.MinValue,
            DateTime.MinValue,
            DateTime.MinValue,
            new DateOnly(2026, 3, 25),
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 20))).Should().BeFalse();
    }

    // ── HasValue nullable ─────────────────────────────────────────────────────

    [Fact]
    public void HasValue_NullableDouble_NullReturnsFalse()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .HasValue(e => e.NullableDouble)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, 5.0, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
    }

    [Fact]
    public void HasValue_NullableInt_NullReturnsFalse()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .HasValue(e => e.NullableInt)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
        filter(new QueryEntityEx(42, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
    }

    [Fact]
    public void HasValue_NullableFloat_NullReturnsFalse()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .HasValue(e => e.NullableFloat)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, null, 1.5f, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
    }


    // ── Nullable long?/decimal? overloads on ValiFlowQuery ───────────────────

    [Fact]
    public void IsNullOrZero_NullableLong_NullReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .IsNullOrZero(e => e.NullableLong)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
        filter(new QueryEntityEx(null, 0L, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
        filter(new QueryEntityEx(null, 5L, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
    }

    [Fact]
    public void IsNullOrZero_NullableDecimal_NullReturnsTrue()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .IsNullOrZero(e => e.NullableDecimal)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
        filter(new QueryEntityEx(null, null, 0m, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
        filter(new QueryEntityEx(null, null, 5m, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
    }

    [Fact]
    public void HasValue_NullableLong_NullReturnsFalse()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .HasValue(e => e.NullableLong)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
        filter(new QueryEntityEx(null, 5L, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
    }

    [Fact]
    public void HasValue_NullableDecimal_NullReturnsFalse()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .HasValue(e => e.NullableDecimal)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
        filter(new QueryEntityEx(null, null, 5m, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
    }

    [Fact]
    public void GreaterThan_NullableLong_MatchesAboveThreshold()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .GreaterThan(e => e.NullableLong, 10L)
            .Build().Compile();

        filter(new QueryEntityEx(null, 11L, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
        filter(new QueryEntityEx(null, 5L, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
    }

    [Fact]
    public void LessThan_NullableDecimal_MatchesBelowThreshold()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>()
            .LessThan(e => e.NullableDecimal, 10m)
            .Build().Compile();

        filter(new QueryEntityEx(null, null, 5m, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeTrue();
        filter(new QueryEntityEx(null, null, 15m, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue))
            .Should().BeFalse();
    }

    // ── TimeOnly methods on ValiFlowQuery ─────────────────────────────────────

    [Fact]
    public void IsAfter_TimeOnly_MatchesLaterTime()
    {
        var filter = new ValiFlowQuery<QueryEntity>()
            .IsAfter(e => e.WorkStart, new TimeOnly(9, 0))
            .Build().Compile();

        filter(new QueryEntity("A", 25, 1000m, true, DateTime.UtcNow, DateTimeOffset.UtcNow,
            DateOnly.MinValue, new TimeOnly(10, 0), new List<string>(), null)).Should().BeTrue();
        filter(new QueryEntity("A", 25, 1000m, true, DateTime.UtcNow, DateTimeOffset.UtcNow,
            DateOnly.MinValue, new TimeOnly(8, 0), new List<string>(), null)).Should().BeFalse();
    }

    [Fact]
    public void IsPM_TimeOnly_MatchesAfterNoon()
    {
        var filter = new ValiFlowQuery<QueryEntity>()
            .IsPM(e => e.WorkStart)
            .Build().Compile();

        filter(new QueryEntity("A", 25, 1000m, true, DateTime.UtcNow, DateTimeOffset.UtcNow,
            DateOnly.MinValue, new TimeOnly(13, 0), new List<string>(), null)).Should().BeTrue();
        filter(new QueryEntity("A", 25, 1000m, true, DateTime.UtcNow, DateTimeOffset.UtcNow,
            DateOnly.MinValue, new TimeOnly(9, 0), new List<string>(), null)).Should().BeFalse();
    }

    [Fact]
    public void IsExactTime_TimeOnly_MatchesExactTime()
    {
        var filter = new ValiFlowQuery<QueryEntity>()
            .IsExactTime(e => e.WorkStart, new TimeOnly(9, 30))
            .Build().Compile();

        filter(new QueryEntity("A", 25, 1000m, true, DateTime.UtcNow, DateTimeOffset.UtcNow,
            DateOnly.MinValue, new TimeOnly(9, 30), new List<string>(), null)).Should().BeTrue();
        filter(new QueryEntity("A", 25, 1000m, true, DateTime.UtcNow, DateTimeOffset.UtcNow,
            DateOnly.MinValue, new TimeOnly(9, 31), new List<string>(), null)).Should().BeFalse();
    }

    // ── MaxCount null semantics (regression) ─────────────────────────────────

    [Fact]
    public void MaxCount_NullCollection_ReturnsFalse()
    {
        var filter = new ValiFlowQuery<QueryEntity>()
            .MaxCount<string>(e => e.Tags, 5)
            .Build().Compile();

        // null collection fails validation (consistent with MinCount null behaviour)
        filter(new QueryEntity("A", 25, 1000m, true, DateTime.UtcNow, DateTimeOffset.UtcNow,
            DateOnly.MinValue, TimeOnly.MinValue, null, null)).Should().BeFalse();
    }

    // ── ValiFlowQuery cross-property InRange: long / double / float / short ───

    private record QueryRangeEntity(long LVal, long LMin, long LMax,
        double DVal, double DMin, double DMax,
        float FVal, float FMin, float FMax,
        short SVal, short SMin, short SMax);

    [Fact]
    public void InRange_CrossProperty_Long_ValiFlowQuery_MatchesWithinRange()
    {
        var filter = new ValiFlowQuery<QueryRangeEntity>()
            .InRange(e => e.LVal, e => e.LMin, e => e.LMax)
            .Build().Compile();

        filter(new QueryRangeEntity(5L, 1L, 10L, 0, 0, 0, 0, 0, 0, 0, 0, 0)).Should().BeTrue();
        filter(new QueryRangeEntity(0L, 1L, 10L, 0, 0, 0, 0, 0, 0, 0, 0, 0)).Should().BeFalse();
    }

    [Fact]
    public void InRange_CrossProperty_Double_ValiFlowQuery_MatchesWithinRange()
    {
        var filter = new ValiFlowQuery<QueryRangeEntity>()
            .InRange(e => e.DVal, e => e.DMin, e => e.DMax)
            .Build().Compile();

        filter(new QueryRangeEntity(0, 0, 0, 5.0, 1.0, 10.0, 0, 0, 0, 0, 0, 0)).Should().BeTrue();
        filter(new QueryRangeEntity(0, 0, 0, 11.0, 1.0, 10.0, 0, 0, 0, 0, 0, 0)).Should().BeFalse();
    }

    [Fact]
    public void InRange_CrossProperty_Float_ValiFlowQuery_MatchesWithinRange()
    {
        var filter = new ValiFlowQuery<QueryRangeEntity>()
            .InRange(e => e.FVal, e => e.FMin, e => e.FMax)
            .Build().Compile();

        filter(new QueryRangeEntity(0, 0, 0, 0, 0, 0, 5f, 1f, 10f, 0, 0, 0)).Should().BeTrue();
        filter(new QueryRangeEntity(0, 0, 0, 0, 0, 0, 0f, 1f, 10f, 0, 0, 0)).Should().BeFalse();
    }

    [Fact]
    public void InRange_CrossProperty_Short_ValiFlowQuery_MatchesWithinRange()
    {
        var filter = new ValiFlowQuery<QueryRangeEntity>()
            .InRange(e => e.SVal, e => e.SMin, e => e.SMax)
            .Build().Compile();

        filter(new QueryRangeEntity(0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 1, 10)).Should().BeTrue();
        filter(new QueryRangeEntity(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 10)).Should().BeFalse();
    }

    // ── Null-selector guards ──────────────────────────────────────────────────

    [Fact]
    public void InRange_NullSelector_ThrowsArgumentNullException()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        Action act = () => builder.InRange((Expression<Func<QueryEntity, int>>)null!, 1, 10);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BetweenDates_NullSelector_ThrowsArgumentNullException()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        Action act = () => builder.BetweenDates(null!, DateTime.MinValue, DateTime.MaxValue);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Contains_NullSelector_ThrowsArgumentNullException()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        Action act = () => builder.Contains(null!, "test");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void StartsWith_NullSelector_ThrowsArgumentNullException()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        Action act = () => builder.StartsWith(null!, "A");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EndsWith_NullSelector_ThrowsArgumentNullException()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        Action act = () => builder.EndsWith(null!, "Z");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void In_NullSelector_ThrowsArgumentNullException()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        Action act = () => builder.In(null!, new List<string> { "A" });
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NotIn_NullSelector_ThrowsArgumentNullException()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        Action act = () => builder.NotIn(null!, new List<string> { "A" });
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GreaterThan_NullSelector_ThrowsArgumentNullException()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        Action act = () => builder.GreaterThan((Expression<Func<QueryEntity, int>>)null!, 0);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void LessThan_NullSelector_ThrowsArgumentNullException()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        Action act = () => builder.LessThan((Expression<Func<QueryEntity, int>>)null!, 100);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MinLength_NullSelector_ThrowsArgumentNullException()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        Action act = () => builder.MinLength(null!, 1);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NotEmpty_NullSelector_ThrowsArgumentNullException()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        Action act = () => builder.NotEmpty<string>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void In_EmptyList_ThrowsArgumentException()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        Action act = () => builder.In(e => e.Name, new List<string>());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NotIn_EmptyList_ThrowsArgumentException()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        Action act = () => builder.NotIn(e => e.Name, new List<string>());
        act.Should().Throw<ArgumentException>();
    }
}
