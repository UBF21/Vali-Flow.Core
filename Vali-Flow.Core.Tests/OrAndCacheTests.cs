using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// Or() state-machine tests
// ─────────────────────────────────────────────────────────────────────────────

public class OrStateMachineTests
{
    // Person record is already declared in AddIfWhenValidationTests.cs (same namespace).
    // We use a local helper to keep this file self-contained.
    private static Person P(string? name = "Alice", int age = 30, bool isActive = true,
        decimal salary = 50_000m, string? email = "alice@example.com")
        => new(name, age, isActive, salary, email);

    // ── Test 1 ───────────────────────────────────────────────────────────────
    [Fact]
    public void Single_Or_TwoConditions_EitherTrue_ReturnsTrue()
    {
        // A || B  →  Age > 30 OR Age < 10
        var filter = new ValiFlow<Person>()
            .Add(p => p.Age > 30)
            .Or()
            .Add(p => p.Age < 10)
            .Build()
            .Compile();

        filter(P(age: 35)).Should().BeTrue();   // A true
        filter(P(age: 5)).Should().BeTrue();    // B true
        filter(P(age: 20)).Should().BeFalse();  // neither
    }

    // ── Test 2 ───────────────────────────────────────────────────────────────
    [Fact]
    public void Or_AfterAnd_CorrectPrecedence()
    {
        // (Age>0 && Name!=null) || IsActive
        var filter = new ValiFlow<Person>()
            .Add(p => p.Age > 0)
            .Add(p => p.Name != null)
            .Or()
            .Add(p => p.IsActive)
            .Build()
            .Compile();

        filter(P(age: 5, name: "X", isActive: false)).Should().BeTrue();   // group 1 true
        filter(P(age: 0, name: null, isActive: true)).Should().BeTrue();   // group 2 true
        filter(P(age: 0, name: null, isActive: false)).Should().BeFalse(); // neither
    }

    // ── Test 3 ───────────────────────────────────────────────────────────────
    [Fact]
    public void Triple_Or_ThreeGroups()
    {
        // A || B || C  →  Age<10 | Age>50 | Salary<0
        var filter = new ValiFlow<Person>()
            .Add(p => p.Age < 10)
            .Or()
            .Add(p => p.Age > 50)
            .Or()
            .Add(p => p.Salary < 0m)
            .Build()
            .Compile();

        filter(P(age: 5)).Should().BeTrue();             // only A
        filter(P(age: 55)).Should().BeTrue();            // only B
        filter(P(age: 30, salary: -1m)).Should().BeTrue(); // only C
        filter(P(age: 30, salary: 100m)).Should().BeFalse(); // none
        filter(P(age: 5, salary: -1m)).Should().BeTrue();    // A and C
    }

    // ── Test 4 ───────────────────────────────────────────────────────────────
    [Fact]
    public void Or_ThenAnd_GroupsCorrectly()
    {
        // A || (B && C)  →  Age<10 | (IsActive && Age>20)
        var filter = new ValiFlow<Person>()
            .Add(p => p.Age < 10)
            .Or()
            .Add(p => p.IsActive)
            .Add(p => p.Age > 20)
            .Build()
            .Compile();

        filter(P(age: 5, isActive: false)).Should().BeTrue();  // A true
        filter(P(age: 25, isActive: true)).Should().BeTrue();  // B && C true
        filter(P(age: 25, isActive: false)).Should().BeFalse(); // A false, B true but C... A=false, B&&C: isActive=false so false
    }

    // ── Test 5 ───────────────────────────────────────────────────────────────
    [Fact]
    public void Or_WithNoConditions_OnlyAddAfter_ProducesValidExpression()
    {
        // Or() with nothing prior → just sets flag, then Add makes first condition
        var filter = new ValiFlow<Person>()
            .Or()
            .Add(p => p.Age > 0)
            .Build()
            .Compile();

        filter(P(age: 1)).Should().BeTrue();
        filter(P(age: -1)).Should().BeFalse();
    }

    // ── Test 6 ───────────────────────────────────────────────────────────────
    [Fact]
    public void Consecutive_Or_Calls_NoAdd_DoesNotCrash()
    {
        // .Or().Or() between two conditions
        var filter = new ValiFlow<Person>()
            .Add(p => p.Age > 30)
            .Or()
            .Or()
            .Add(p => p.Age < 10)
            .Build()
            .Compile();

        filter(P(age: 35)).Should().BeTrue();
        filter(P(age: 5)).Should().BeTrue();
        filter(P(age: 20)).Should().BeFalse();
    }

    // ── Test 7 ───────────────────────────────────────────────────────────────
    [Fact]
    public void Or_WithAddSubGroup_Combines()
    {
        // A || (B && C)  using AddSubGroup for the right side
        var filter = new ValiFlow<Person>()
            .Add(p => p.Age < 10)
            .Or()
            .AddSubGroup(g => g
                .Add(p => p.IsActive)
                .Add(p => p.Salary > 0m))
            .Build()
            .Compile();

        filter(P(age: 5)).Should().BeTrue();                            // A true
        filter(P(age: 30, isActive: true, salary: 1m)).Should().BeTrue(); // subgroup true
        filter(P(age: 30, isActive: false, salary: 1m)).Should().BeFalse(); // A false, B false
    }

    // ── Test 8 ───────────────────────────────────────────────────────────────
    [Fact]
    public void Or_WithError_ValidationResult_Correct()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age > 18).WithError("E01", "Must be adult")
            .Or()
            .Add(p => p.Name != null).WithError("E02", "Name required");

        // Age=10, Name=null → both OR-groups fail → both errors reported
        var result1 = builder.Validate(P(age: 10, name: null));
        result1.Errors.Should().Contain(e => e.ErrorCode == "E01");
        result1.Errors.Should().Contain(e => e.ErrorCode == "E02");

        // Age=20, Name=null → Group 1 (Age > 18) passes → overall valid → no errors
        var result2 = builder.Validate(P(age: 20, name: null));
        result2.IsValid.Should().BeTrue();
        result2.Errors.Should().BeEmpty();

        // Age=10, Name="X" → Group 2 (Name != null) passes → overall valid → no errors
        var result3 = builder.Validate(P(age: 10, name: "X"));
        result3.IsValid.Should().BeTrue();
        result3.Errors.Should().BeEmpty();
    }

    // ── Test 9 ───────────────────────────────────────────────────────────────
    [Fact]
    public void Or_IsValid_ConsistentWithBuild()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age > 30)
            .Or()
            .Add(p => p.IsActive);

        var instances = new[]
        {
            P(age: 35, isActive: false),
            P(age: 20, isActive: true),
            P(age: 20, isActive: false),
            P(age: 35, isActive: true),
        };

        var compiled = builder.Build().Compile();
        foreach (var instance in instances)
        {
            builder.IsValid(instance).Should().Be(compiled(instance));
        }
    }

    // ── Test 10 ──────────────────────────────────────────────────────────────
    [Fact]
    public void And_AfterOr_StartsNewGroup()
    {
        // A || (B && C)
        var filter = new ValiFlow<Person>()
            .Add(p => p.Age < 10)
            .Or()
            .Add(p => p.IsActive)
            .And()
            .Add(p => p.Salary > 0m)
            .Build()
            .Compile();

        filter(P(age: 5, isActive: false, salary: 0m)).Should().BeTrue();      // A true
        filter(P(age: 30, isActive: true, salary: 100m)).Should().BeTrue();    // B && C true
        filter(P(age: 30, isActive: true, salary: 0m)).Should().BeFalse();     // B true, C false → false
        filter(P(age: 30, isActive: false, salary: 100m)).Should().BeFalse(); // A false, B false
    }

    // ── Test 11 ──────────────────────────────────────────────────────────────
    [Fact]
    public void Or_TwoAndGroups_FourConditions()
    {
        // (A && B) || (C && D)
        // A: Age > 18, B: IsActive
        // C: Salary > 100_000, D: Name != null
        var filter = new ValiFlow<Person>()
            .Add(p => p.Age > 18)
            .Add(p => p.IsActive)
            .Or()
            .Add(p => p.Salary > 100_000m)
            .Add(p => p.Name != null)
            .Build()
            .Compile();

        // Group 1 true
        filter(P(age: 25, isActive: true, salary: 0m, name: null)).Should().BeTrue();
        // Group 2 true
        filter(P(age: 10, isActive: false, salary: 200_000m, name: "X")).Should().BeTrue();
        // Both groups true
        filter(P(age: 25, isActive: true, salary: 200_000m, name: "X")).Should().BeTrue();
        // Group 1 false (age ok, not active)
        filter(P(age: 25, isActive: false, salary: 0m, name: null)).Should().BeFalse();
        // Group 2 false (salary ok, name null)
        filter(P(age: 10, isActive: false, salary: 200_000m, name: null)).Should().BeFalse();
        // Both false
        filter(P(age: 10, isActive: false, salary: 0m, name: null)).Should().BeFalse();
    }

    // ── Test 12 ──────────────────────────────────────────────────────────────
    [Fact]
    public void Or_BuildNegated_Correct()
    {
        // A || B negated = !(A || B) = !A && !B
        // A: Age > 30, B: IsActive
        var filter = new ValiFlow<Person>()
            .Add(p => p.Age > 30)
            .Or()
            .Add(p => p.IsActive)
            .BuildNegated()
            .Compile();

        // Age=20, isActive=false → !(false || false) = true
        filter(P(age: 20, isActive: false)).Should().BeTrue();
        // Age=35, isActive=false → !(true || false) = false
        filter(P(age: 35, isActive: false)).Should().BeFalse();
        // Age=20, isActive=true → !(false || true) = false
        filter(P(age: 20, isActive: true)).Should().BeFalse();
        // Age=35, isActive=true → !(true || true) = false
        filter(P(age: 35, isActive: true)).Should().BeFalse();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Validate() compiled-cache tests
// ─────────────────────────────────────────────────────────────────────────────

public class ValidateCacheTests
{
    private static Person P(string? name = "Alice", int age = 30, bool isActive = true,
        decimal salary = 50_000m, string? email = "alice@example.com")
        => new(name, age, isActive, salary, email);

    // ── Test 13 ──────────────────────────────────────────────────────────────
    [Fact]
    public void Validate_CalledTwice_SameInstance_ReturnsIdenticalResult()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age > 18).WithError("E01", "Must be adult");

        var instance = P(age: 10);
        var result1 = builder.Validate(instance);
        var result2 = builder.Validate(instance);

        result1.IsValid.Should().Be(result2.IsValid);
        result1.Errors.Count.Should().Be(result2.Errors.Count);
        result1.Errors[0].ErrorCode.Should().Be(result2.Errors[0].ErrorCode);
    }

    // ── Test 14 ──────────────────────────────────────────────────────────────
    [Fact]
    public void Validate_AfterWithError_ReturnsErrorForFailingCondition()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18).WithError("AGE", "Age must be at least 18")
            .Add(p => p.Name != null).WithError("NAME", "Name is required");

        var passing = builder.Validate(P(age: 25, name: "Bob"));
        passing.IsValid.Should().BeTrue();
        passing.Errors.Should().BeEmpty();

        var failing = builder.Validate(P(age: 10, name: null));
        failing.IsValid.Should().BeFalse();
        failing.Errors.Should().Contain(e => e.ErrorCode == "AGE");
        failing.Errors.Should().Contain(e => e.ErrorCode == "NAME");
    }

    // ── Test 15 ──────────────────────────────────────────────────────────────
    [Fact]
    public void ValidateAll_MultipleInstances_ReturnsCorrectResults()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18).WithError("AGE", "Must be adult")
            .Add(p => p.IsActive).WithError("ACTIVE", "Must be active");

        var instances = new[]
        {
            P(age: 25, isActive: true),  // valid
            P(age: 10, isActive: true),  // age fails
            P(age: 25, isActive: false), // active fails
        };

        var results = builder.ValidateAll(instances).ToList();

        results.Should().HaveCount(3);

        results[0].Result.IsValid.Should().BeTrue();

        results[1].Result.IsValid.Should().BeFalse();
        results[1].Result.Errors.Should().Contain(e => e.ErrorCode == "AGE");
        results[1].Result.Errors.Should().NotContain(e => e.ErrorCode == "ACTIVE");

        results[2].Result.IsValid.Should().BeFalse();
        results[2].Result.Errors.Should().NotContain(e => e.ErrorCode == "AGE");
        results[2].Result.Errors.Should().Contain(e => e.ErrorCode == "ACTIVE");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// In / NotIn tests
// ─────────────────────────────────────────────────────────────────────────────

public class InNotInTests
{
    private static Person P(string? name = "Alice", int age = 30, bool isActive = true,
        decimal salary = 50_000m, string? email = "alice@example.com")
        => new(name, age, isActive, salary, email);

    // ── Test 16 ──────────────────────────────────────────────────────────────
    [Fact]
    public void In_ValueInList_ReturnsTrue()
    {
        var filter = new ValiFlow<Person>()
            .In(p => p.Age, new[] { 18, 25, 30, 40 })
            .Build()
            .Compile();

        filter(P(age: 25)).Should().BeTrue();
        filter(P(age: 30)).Should().BeTrue();
    }

    // ── Test 17 ──────────────────────────────────────────────────────────────
    [Fact]
    public void In_ValueNotInList_ReturnsFalse()
    {
        var filter = new ValiFlow<Person>()
            .In(p => p.Age, new[] { 18, 25, 40 })
            .Build()
            .Compile();

        filter(P(age: 30)).Should().BeFalse();
        filter(P(age: 99)).Should().BeFalse();
    }

    // ── Test 18 ──────────────────────────────────────────────────────────────
    [Fact]
    public void In_EmptyList_ThrowsArgumentException()
    {
        var act = () => new ValiFlow<Person>().In(p => p.Age, Array.Empty<int>());
        act.Should().Throw<ArgumentException>().WithParameterName("values");
    }

    // ── Test 19 ──────────────────────────────────────────────────────────────
    [Fact]
    public void NotIn_ValueInList_ReturnsFalse()
    {
        var filter = new ValiFlow<Person>()
            .NotIn(p => p.Age, new[] { 18, 25, 30 })
            .Build()
            .Compile();

        filter(P(age: 25)).Should().BeFalse();
        filter(P(age: 30)).Should().BeFalse();
    }

    // ── Test 20 ──────────────────────────────────────────────────────────────
    [Fact]
    public void NotIn_ValueNotInList_ReturnsTrue()
    {
        var filter = new ValiFlow<Person>()
            .NotIn(p => p.Age, new[] { 18, 25, 40 })
            .Build()
            .Compile();

        filter(P(age: 30)).Should().BeTrue();
        filter(P(age: 99)).Should().BeTrue();
    }

    // ── Test 21 ──────────────────────────────────────────────────────────────
    [Fact]
    public void NotIn_EmptyList_ThrowsArgumentException()
    {
        var act = () => new ValiFlow<Person>().NotIn(p => p.Age, Array.Empty<int>());
        act.Should().Throw<ArgumentException>().WithParameterName("values");
    }

    // ── Bonus: In works with string values ───────────────────────────────────
    [Fact]
    public void In_StringValues_WorksCorrectly()
    {
        var filter = new ValiFlow<Person>()
            .In(p => p.Name, new[] { "Alice", "Bob", "Carol" })
            .Build()
            .Compile();

        filter(P(name: "Alice")).Should().BeTrue();
        filter(P(name: "Dave")).Should().BeFalse();
    }

    // ── Bonus: NotIn works with string values ────────────────────────────────
    [Fact]
    public void NotIn_StringValues_WorksCorrectly()
    {
        var filter = new ValiFlow<Person>()
            .NotIn(p => p.Name, new[] { "Blocked1", "Blocked2" })
            .Build()
            .Compile();

        filter(P(name: "Alice")).Should().BeTrue();
        filter(P(name: "Blocked1")).Should().BeFalse();
    }
}
