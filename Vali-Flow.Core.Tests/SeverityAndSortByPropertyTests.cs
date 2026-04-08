using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;
using Vali_Flow.Core.Models;

namespace Vali_Flow.Core.Tests;

// ── SeverityTests ──────────────────────────────────────────────────────────────

public class SeverityTests
{
    private static Person P(
        string? name = "Alice",
        int age = 30,
        bool isActive = true,
        decimal salary = 50_000m,
        string? email = "alice@example.com")
        => new(name, age, isActive, salary, email);

    // ── ValidationError construction ──────────────────────────────────────────

    // Test 1
    [Fact]
    public void ValidationError_DefaultSeverity_IsError()
    {
        var error = new ValidationError("Something failed");

        error.Severity.Should().Be(Severity.Error);
    }

    // Test 2
    [Fact]
    public void ValidationError_ExplicitWarning_StoredCorrectly()
    {
        var error = new ValidationError("Minor issue", severity: Severity.Warning);

        error.Severity.Should().Be(Severity.Warning);
    }

    // Test 3
    [Fact]
    public void ValidationError_ExplicitCritical_StoredCorrectly()
    {
        var error = new ValidationError("Blocking failure", severity: Severity.Critical);

        error.Severity.Should().Be(Severity.Critical);
    }

    // Test 4
    [Fact]
    public void ValidationError_ExplicitInfo_StoredCorrectly()
    {
        var error = new ValidationError("FYI", severity: Severity.Info);

        error.Severity.Should().Be(Severity.Info);
    }

    // ── ToString format ───────────────────────────────────────────────────────

    // Test 5
    [Fact]
    public void ToString_DefaultError_NoSeverityPrefix()
    {
        var error = new ValidationError("Bad value", errorCode: "E01");

        var result = error.ToString();

        result.Should().NotContain("[Error]");
        result.Should().Be("[E01] Bad value");
    }

    // Test 6
    [Fact]
    public void ToString_Warning_HasPrefix()
    {
        var error = new ValidationError("Watch out", severity: Severity.Warning);

        error.ToString().Should().Be("[Warning] Watch out");
    }

    // Test 7
    [Fact]
    public void ToString_Critical_HasPrefix()
    {
        var error = new ValidationError("Blocking", errorCode: "CRIT01", severity: Severity.Critical);

        error.ToString().Should().Be("[Critical] [CRIT01] Blocking");
    }

    // Test 8
    [Fact]
    public void ToString_Info_WithPath_HasPrefix()
    {
        var error = new ValidationError("Hint text", propertyPath: "Name", severity: Severity.Info);

        error.ToString().Should().Be("[Info] [Name] Hint text");
    }

    // ── Builder .WithSeverity() ───────────────────────────────────────────────

    // Test 9
    [Fact]
    public void WithSeverity_SetsWarning_OnLastCondition()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age > 100).WithError("E01", "Too old").WithSeverity(Severity.Warning)
            .Validate(P(age: 30));

        result.Errors.Should().HaveCount(1);
        result.Errors[0].Severity.Should().Be(Severity.Warning);
    }

    // Test 10
    [Fact]
    public void WithSeverity_SetsCritical_OnLastCondition()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.IsActive).WithError("E02", "Not active").WithSeverity(Severity.Critical)
            .Validate(P(isActive: false));

        result.Errors.Should().HaveCount(1);
        result.Errors[0].Severity.Should().Be(Severity.Critical);
    }

    // Test 11
    [Fact]
    public void WithSeverity_OnEmptyBuilder_ThrowsInvalidOperationException()
    {
        var act = () =>
        {
            _ = new ValiFlow<Person>()
                .WithSeverity(Severity.Warning);
        };

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*WithMessage, WithError, and WithSeverity require at least one condition*");
    }

    // Test 12
    [Fact]
    public void WithSeverity_AfterWithError_PreservesCodeAndMessage()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age > 100)
            .WithError("AGE01", "Age out of range")
            .WithSeverity(Severity.Info)
            .Validate(P(age: 30));

        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorCode.Should().Be("AGE01");
        result.Errors[0].Message.Should().Be("Age out of range");
        result.Errors[0].Severity.Should().Be(Severity.Info);
    }

    // ── Builder .WithError(code, msg, Severity) ───────────────────────────────

    // Test 13
    [Fact]
    public void WithError_WithSeverityOverload_SetsCorrectly()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Salary > 200_000m)
            .WithError("SAL01", "Salary too low", Severity.Warning)
            .Validate(P(salary: 50_000m));

        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorCode.Should().Be("SAL01");
        result.Errors[0].Message.Should().Be("Salary too low");
        result.Errors[0].Severity.Should().Be(Severity.Warning);
    }

    // Test 14
    [Fact]
    public void WithError_WithSeverityAndPath_SetsAllFields()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Name != null && p.Name.Length >= 10)
            .WithError("NAME01", "Name too short", "Name", Severity.Critical)
            .Validate(P(name: "Bob"));

        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorCode.Should().Be("NAME01");
        result.Errors[0].Message.Should().Be("Name too short");
        result.Errors[0].PropertyPath.Should().Be("Name");
        result.Errors[0].Severity.Should().Be(Severity.Critical);
    }

    // ── ValidationResult filtering ────────────────────────────────────────────

    private static ValidationResult BuildResultWithMixedSeverities()
    {
        // All four conditions fail for: name="Alice", age=30, isActive=true, salary=50_000, email="alice@example.com"
        // age > 100 fails, salary > 200_000 fails, age < 0 fails, name.Length > 100 fails
        return new ValiFlow<Person>()
            .Add(p => p.Age > 100).WithError("I01", "Info msg", Severity.Info)
            .And()
            .Add(p => p.Salary > 200_000m).WithError("W01", "Warning msg", Severity.Warning)
            .And()
            .Add(p => p.Age < 0).WithError("E01", "Error msg") // default Error
            .And()
            .Add(p => p.Name != null && p.Name.Length > 100).WithError("C01", "Critical msg", Severity.Critical)
            .Validate(P());
    }

    // Test 15
    [Fact]
    public void ValidationResult_Warnings_ReturnsOnlyWarnings()
    {
        var result = BuildResultWithMixedSeverities();

        result.Warnings.Should().HaveCount(1);
        result.Warnings[0].Severity.Should().Be(Severity.Warning);
    }

    // Test 16
    [Fact]
    public void ValidationResult_CriticalErrors_ReturnsOnlyCritical()
    {
        var result = BuildResultWithMixedSeverities();

        result.CriticalErrors.Should().HaveCount(1);
        result.CriticalErrors[0].Severity.Should().Be(Severity.Critical);
    }

    // Test 17
    [Fact]
    public void ValidationResult_ErrorsAbove_Warning_ReturnsErrorAndCritical()
    {
        var result = BuildResultWithMixedSeverities();

        var above = result.ErrorsAtOrAbove(Severity.Warning);

        // Warning(1), Error(2), Critical(3) are all >= Warning
        above.Should().HaveCount(3);
        above.Select(e => e.Severity).Should().Contain(Severity.Warning);
        above.Select(e => e.Severity).Should().Contain(Severity.Error);
        above.Select(e => e.Severity).Should().Contain(Severity.Critical);
    }

    // Test 18
    [Fact]
    public void ValidationResult_ErrorsAbove_Critical_ReturnsOnlyCritical()
    {
        var result = BuildResultWithMixedSeverities();

        var above = result.ErrorsAtOrAbove(Severity.Critical);

        above.Should().HaveCount(1);
        above[0].Severity.Should().Be(Severity.Critical);
    }

    // Test 19
    [Fact]
    public void ValidationResult_HasAnySeverity_Critical_TrueWhenCriticalPresent()
    {
        var result = BuildResultWithMixedSeverities();

        result.HasAnySeverity(Severity.Critical).Should().BeTrue();
    }

    // Test 20
    [Fact]
    public void ValidationResult_IsValid_StillFalse_WhenAnyErrorRegardlessOfSeverity()
    {
        // Even if the only error is a Warning, IsValid should be false.
        var result = new ValiFlow<Person>()
            .Add(p => p.Age > 100).WithError("W01", "Minor warning", Severity.Warning)
            .Validate(P(age: 30));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }
}

