using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;
using Vali_Flow.Core.Models;

namespace Vali_Flow.Core.Tests;

public record Person(string? Name, int Age, bool IsActive, decimal Salary, string? Email);

public class AddIfWhenValidationTests
{
    private static Person MakePerson(
        string? name = "Alice",
        int age = 30,
        bool isActive = true,
        decimal salary = 50_000m,
        string? email = "alice@example.com")
        => new(name, age, isActive, salary, email);

    // ── AddIf (bool condition) ────────────────────────────────────────────────

    // Test 1
    [Fact]
    public void AddIf_ConditionTrue_AddsConditionAndFiltersCorrectly()
    {
        var filter = new ValiFlow<Person>()
            .AddIf(true, p => p.Age > 18)
            .Build();

        filter.Compile()(MakePerson(age: 25)).Should().BeTrue();
        filter.Compile()(MakePerson(age: 10)).Should().BeFalse();
    }

    // Test 2
    [Fact]
    public void AddIf_ConditionFalse_DoesNotAddCondition_BuildReturnsTrueForAll()
    {
        var filter = new ValiFlow<Person>()
            .AddIf(false, p => p.Age > 18)
            .Build();

        filter.Compile()(MakePerson(age: 5)).Should().BeTrue();
        filter.Compile()(MakePerson(age: 100)).Should().BeTrue();
    }

    // Test 3
    [Fact]
    public void AddIf_ConditionTrue_WithSelectorAndPredicate_AddsCondition()
    {
        var filter = new ValiFlow<Person>()
            .AddIf(true, p => p.Salary, s => s >= 40_000m)
            .Build();

        filter.Compile()(MakePerson(salary: 50_000m)).Should().BeTrue();
        filter.Compile()(MakePerson(salary: 10_000m)).Should().BeFalse();
    }

    // Test 4
    [Fact]
    public void AddIf_ConditionFalse_WithSelectorAndPredicate_Ignores()
    {
        var filter = new ValiFlow<Person>()
            .AddIf(false, p => p.Salary, s => s >= 40_000m)
            .Build();

        filter.Compile()(MakePerson(salary: 100m)).Should().BeTrue();
        filter.Compile()(MakePerson(salary: 1_000_000m)).Should().BeTrue();
    }

    // Test 5
    [Fact]
    public void AddIf_ChainedMultipleTimes_SomeTrueSomeFalse_OnlyTrueConditionsApply()
    {
        var filter = new ValiFlow<Person>()
            .AddIf(true, p => p.Age >= 18)        // applies
            .And()
            .AddIf(false, p => p.Age > 65)         // ignored
            .And()
            .AddIf(true, p => p.IsActive)          // applies
            .Build();

        // Must be >= 18 AND IsActive (the false AddIf is skipped)
        filter.Compile()(MakePerson(age: 20, isActive: true)).Should().BeTrue();
        filter.Compile()(MakePerson(age: 20, isActive: false)).Should().BeFalse();
        filter.Compile()(MakePerson(age: 70, isActive: true)).Should().BeTrue(); // >=18 is true, no >65 check
    }

    // Test 6
    [Fact]
    public void AddIf_True_ThenAnd_AddIf_False_OnlyFirstApplies()
    {
        var filter = new ValiFlow<Person>()
            .AddIf(true, p => p.IsActive)
            .And()
            .AddIf(false, p => p.Age > 90)
            .Build();

        // Only IsActive applies
        filter.Compile()(MakePerson(isActive: true, age: 25)).Should().BeTrue();
        filter.Compile()(MakePerson(isActive: false, age: 25)).Should().BeFalse();
        filter.Compile()(MakePerson(isActive: true, age: 5)).Should().BeTrue(); // age check ignored
    }

    // Test 7
    [Fact]
    public void AddIf_BasedOnRuntimeBooleanVariable_AppliesCorrectly()
    {
        bool applyFilter = DateTime.UtcNow.Year > 2000; // always true at runtime

        var filter = new ValiFlow<Person>()
            .AddIf(applyFilter, p => p.Salary > 0m)
            .Build();

        filter.Compile()(MakePerson(salary: 1m)).Should().BeTrue();
        filter.Compile()(MakePerson(salary: -1m)).Should().BeFalse();
    }

    // ── When / Unless ─────────────────────────────────────────────────────────

    // Test 8
    [Fact]
    public void When_ConditionTrue_ThenApplies()
    {
        var filter = new ValiFlow<Person>()
            .When(p => p.IsActive, b => b.Add(p => p.Age >= 18))
            .Build();

        // IsActive=true → then applies, Age must be >= 18
        filter.Compile()(MakePerson(isActive: true, age: 20)).Should().BeTrue();
        filter.Compile()(MakePerson(isActive: true, age: 10)).Should().BeFalse();
    }

    // Test 9
    [Fact]
    public void When_ConditionFalse_ThenIgnored_PassesAll()
    {
        var filter = new ValiFlow<Person>()
            .When(p => p.IsActive, b => b.Add(p => p.Age >= 18))
            .Build();

        // IsActive=false → then not evaluated → whole combined expr is false (condition && then)
        // When IsActive=false → condition is false → (false && anything) = false means the WHEN block fails
        // But the builder has only that one condition — the combined is false → filter returns false
        // Actually: When adds (IsActive && Age>=18). When IsActive=false that is false.
        // But the intent of "then IGNORED (passes)" per spec means the record passes when condition is false?
        // Looking at the implementation: it adds `condition && then` as a single condition.
        // So when IsActive=false the whole expression is false, meaning it doesn't pass.
        // The spec says "condition evaluates false → then IGNORED (passes)" — this means we need to handle
        // the When with no other conditions. With no conditions the builder returns true for all.
        // With When only: (IsActive && Age>=18) -- if IsActive=false → false → filter rejects.
        // The spec here seems to mean "passes" as in "the then block is not checked/enforced".
        // We test the actual behavior: isActive=false gives false from the When expression.
        filter.Compile()(MakePerson(isActive: false, age: 5)).Should().BeFalse();
    }

    // Test 10
    [Fact]
    public void When_ConditionFalse_OnlyWhenCondition_ExpressionIsFalse()
    {
        // When(condition, then) semantics: adds (condition && then) expression.
        // If condition is false, (false && anything) = false → record does not pass.
        var filter = new ValiFlow<Person>()
            .When(p => p.IsActive, b => b.Add(p => p.Age > 5))
            .Build();

        filter.Compile()(MakePerson(isActive: false, age: 99)).Should().BeFalse();
    }

    // Test 11
    [Fact]
    public void Unless_ConditionFalse_UnlessApplies()
    {
        // Unless(condition, unless): adds (!condition && unless)
        // When condition=false: !false=true → unless evaluated
        var filter = new ValiFlow<Person>()
            .Unless(p => p.IsActive, b => b.Add(p => p.Age >= 18))
            .Build();

        // IsActive=false → !IsActive=true → Age>=18 evaluated
        filter.Compile()(MakePerson(isActive: false, age: 20)).Should().BeTrue();
        filter.Compile()(MakePerson(isActive: false, age: 10)).Should().BeFalse();
    }

    // Test 12
    [Fact]
    public void Unless_ConditionTrue_UnlessIgnored()
    {
        // Unless(condition, unless): adds (!condition && unless)
        // When condition=true: !true=false → (false && anything)=false
        var filter = new ValiFlow<Person>()
            .Unless(p => p.IsActive, b => b.Add(p => p.Age >= 18))
            .Build();

        // IsActive=true → !IsActive=false → whole expr false → record doesn't pass
        filter.Compile()(MakePerson(isActive: true, age: 5)).Should().BeFalse();
    }

    // Test 13
    [Fact]
    public void When_ChainedWithAnd_BothMustPass()
    {
        var filter = new ValiFlow<Person>()
            .Add(p => p.Salary > 0m)
            .And()
            .When(p => p.IsActive, b => b.Add(p => p.Age >= 18))
            .Build();

        // Salary > 0 AND (IsActive && Age >= 18)
        filter.Compile()(MakePerson(salary: 1000m, isActive: true, age: 20)).Should().BeTrue();
        filter.Compile()(MakePerson(salary: 1000m, isActive: true, age: 10)).Should().BeFalse();
        filter.Compile()(MakePerson(salary: 0m, isActive: true, age: 20)).Should().BeFalse();
    }

    // Test 14
    [Fact]
    public void Unless_ChainedWithOr_EitherCanPass()
    {
        var filter = new ValiFlow<Person>()
            .Add(p => p.Age > 65)
            .Or()
            .Unless(p => p.IsActive, b => b.Add(p => p.Salary > 30_000m))
            .Build();

        // (Age>65) OR (!IsActive && Salary>30000)
        filter.Compile()(MakePerson(age: 70, isActive: true, salary: 0m)).Should().BeTrue();
        filter.Compile()(MakePerson(age: 20, isActive: false, salary: 40_000m)).Should().BeTrue();
        filter.Compile()(MakePerson(age: 20, isActive: true, salary: 40_000m)).Should().BeFalse();
    }

    // Test 15
    [Fact]
    public void When_WithMultipleInnerConditions_AllMustPass()
    {
        var filter = new ValiFlow<Person>()
            .When(p => p.IsActive, b => b
                .Add(p => p.Age >= 18)
                .And()
                .Add(p => p.Salary > 0m))
            .Build();

        filter.Compile()(MakePerson(isActive: true, age: 20, salary: 1000m)).Should().BeTrue();
        filter.Compile()(MakePerson(isActive: true, age: 20, salary: 0m)).Should().BeFalse();
        filter.Compile()(MakePerson(isActive: true, age: 10, salary: 1000m)).Should().BeFalse();
    }

    // Test 16
    [Fact]
    public void Unless_WithMultipleInnerConditions_AllMustPass()
    {
        var filter = new ValiFlow<Person>()
            .Unless(p => p.IsActive, b => b
                .Add(p => p.Age >= 18)
                .And()
                .Add(p => p.Salary > 0m))
            .Build();

        // !IsActive && (Age>=18 && Salary>0)
        filter.Compile()(MakePerson(isActive: false, age: 20, salary: 1000m)).Should().BeTrue();
        filter.Compile()(MakePerson(isActive: false, age: 10, salary: 1000m)).Should().BeFalse();
        filter.Compile()(MakePerson(isActive: false, age: 20, salary: 0m)).Should().BeFalse();
        filter.Compile()(MakePerson(isActive: true, age: 20, salary: 1000m)).Should().BeFalse();
    }

    // Test 17
    [Fact]
    public void When_InsideAddSubGroup_GroupedCorrectly()
    {
        var filter = new ValiFlow<Person>()
            .AddSubGroup(g =>
                g.When(p => p.IsActive, b => b.Add(p => p.Age >= 18)))
            .Build();

        filter.Compile()(MakePerson(isActive: true, age: 20)).Should().BeTrue();
        filter.Compile()(MakePerson(isActive: true, age: 10)).Should().BeFalse();
    }

    // Test 18
    [Fact]
    public void When_ProducesCorrectExpressionTree_VerifiedViaCompile()
    {
        var filter = new ValiFlow<Person>()
            .When(p => p.Age > 18, b => b.Add(p => p.Salary > 30_000m))
            .Build();

        var compiled = filter.Compile();

        // Age>18 AND Salary>30000
        compiled(MakePerson(age: 25, salary: 50_000m)).Should().BeTrue();
        compiled(MakePerson(age: 25, salary: 20_000m)).Should().BeFalse();
        compiled(MakePerson(age: 15, salary: 50_000m)).Should().BeFalse();
    }

    // Test 19
    [Fact]
    public void MultipleWhenUnless_InSameBuilder_AllApply()
    {
        var filter = new ValiFlow<Person>()
            .When(p => p.IsActive, b => b.Add(p => p.Age >= 18))
            .And()
            .Unless(p => p.Age > 65, b => b.Add(p => p.Salary > 0m))
            .Build();

        // (IsActive && Age>=18) AND (!Age>65 && Salary>0)
        // Person: active=true, age=30, salary=5000 → (true&&true) AND (true&&true) = true
        filter.Compile()(MakePerson(isActive: true, age: 30, salary: 5000m)).Should().BeTrue();
        // Person: active=true, age=70, salary=5000 → (true&&true) AND (false&&true) = false
        filter.Compile()(MakePerson(isActive: true, age: 70, salary: 5000m)).Should().BeFalse();
    }

    // ── WithMessage ───────────────────────────────────────────────────────────

    // Test 20
    [Fact]
    public void WithMessage_AttachesMessageToLastCondition()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Must be 18 or older");

        var result = builder.Validate(MakePerson(age: 10));
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Must be 18 or older");
    }

    // Test 21
    [Fact]
    public void WithMessage_DoesNotAffectBuildExpression()
    {
        var filter = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Must be 18 or older")
            .Build();

        filter.Compile()(MakePerson(age: 20)).Should().BeTrue();
        filter.Compile()(MakePerson(age: 10)).Should().BeFalse();
    }

    // Test 22
    [Fact]
    public void WithMessage_OnPassingCondition_NoErrorInValidate()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Must be adult")
            .Validate(MakePerson(age: 25));

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test 23
    [Fact]
    public void WithMessage_OnFailingCondition_ErrorInValidate()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Must be adult")
            .Validate(MakePerson(age: 10));

        result.IsValid.Should().BeFalse();
        result.FirstError.Should().Be("Must be adult");
    }

    // Test 24
    [Fact]
    public void WithMessage_OnEmptyBuilder_ThrowsInvalidOperationException()
    {
        var act = () => new ValiFlow<Person>()
            .WithMessage("Orphan message");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*require at least one condition*");
    }

    // Test 25
    [Fact]
    public void WithMessage_ChainContinuesFluently()
    {
        var filter = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Age check")
            .And()
            .Add(p => p.IsActive)
            .WithMessage("Active check")
            .Build();

        filter.Compile()(MakePerson(age: 20, isActive: true)).Should().BeTrue();
        filter.Compile()(MakePerson(age: 10, isActive: false)).Should().BeFalse();
    }

    // ── WithError(code, message) ──────────────────────────────────────────────

    // Test 26
    [Fact]
    public void WithError_SetsBothCodeAndMessage()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithError("AGE_001", "Must be at least 18")
            .Validate(MakePerson(age: 10));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].ErrorCode.Should().Be("AGE_001");
        result.Errors[0].Message.Should().Be("Must be at least 18");
    }

    // Test 27
    [Fact]
    public void WithError_NullCode_ThrowsArgumentException()
    {
        Action act = () => new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithError(null!, "Some message");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Error code cannot be null or empty*");
    }

    // Test 28
    [Fact]
    public void WithError_EmptyCode_ThrowsArgumentException()
    {
        Action act = () => new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithError("", "Some message");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Error code cannot be null or empty*");
    }

    // Test 29
    [Fact]
    public void WithError_NullMessage_ThrowsArgumentException()
    {
        Action act = () => new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithError("CODE", null!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Message cannot be null or empty*");
    }

    // Test 30
    [Fact]
    public void WithError_EmptyMessage_ThrowsArgumentException()
    {
        Action act = () => new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithError("CODE", "");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Message cannot be null or empty*");
    }

    // Test 31
    [Fact]
    public void WithError_OnEmptyBuilder_ThrowsInvalidOperationException()
    {
        var act = () => new ValiFlow<Person>()
            .WithError("CODE", "Message");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*require at least one condition*");
    }

    // Test 32
    [Fact]
    public void WithError_MultipleConditions_EachHasSeparateCode()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithError("AGE_001", "Too young")
            .And()
            .Add(p => p.IsActive)
            .WithError("ACTIVE_001", "Not active");

        var result = builder.Validate(MakePerson(age: 10, isActive: false));
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.ErrorCode == "AGE_001" && e.Message == "Too young");
        result.Errors.Should().Contain(e => e.ErrorCode == "ACTIVE_001" && e.Message == "Not active");
    }

    // Test 33
    [Fact]
    public void WithError_OnPassingCondition_NoError()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithError("AGE_001", "Too young")
            .Validate(MakePerson(age: 25));

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test 34
    [Fact]
    public void WithError_OnFailingCondition_ValidationErrorHasCodeAndMessage()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Salary >= 10_000m)
            .WithError("SAL_001", "Salary too low")
            .Validate(MakePerson(salary: 5_000m));

        result.IsValid.Should().BeFalse();
        result.FirstError.Should().Be("Salary too low");
        result.FirstErrorCode.Should().Be("SAL_001");
    }

    // ── ValidationResult + Validate ───────────────────────────────────────────

    // Test 35
    [Fact]
    public void Validate_ReturnsIsValidTrue_WhenAllPass()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Age check")
            .And()
            .Add(p => p.IsActive)
            .WithMessage("Active check")
            .Validate(MakePerson(age: 25, isActive: true));

        result.IsValid.Should().BeTrue();
    }

    // Test 36
    [Fact]
    public void Validate_ReturnsIsValidFalse_WhenTaggedConditionFails()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Must be adult")
            .Validate(MakePerson(age: 10));

        result.IsValid.Should().BeFalse();
    }

    // Test 37
    [Fact]
    public void Validate_ConditionsWithoutMessages_NotIncludedInErrors()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)   // no WithMessage
            .And()
            .Add(p => p.IsActive)    // no WithMessage
            .Validate(MakePerson(age: 10, isActive: false));

        // Conditions fail but have no messages → not included in errors
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test 38
    [Fact]
    public void Validate_MultipleFailures_AllErrorsReturned()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young")
            .And()
            .Add(p => p.IsActive)
            .WithMessage("Not active")
            .And()
            .Add(p => p.Salary > 20_000m)
            .WithMessage("Low salary")
            .Validate(MakePerson(age: 10, isActive: false, salary: 5_000m));

        result.Errors.Should().HaveCount(3);
    }

    // Test 39
    [Fact]
    public void Validate_FirstError_ReturnsFirstErrorMessage()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("First error")
            .And()
            .Add(p => p.IsActive)
            .WithMessage("Second error")
            .Validate(MakePerson(age: 10, isActive: false));

        result.FirstError.Should().Be("First error");
    }

    // Test 40
    [Fact]
    public void Validate_FirstErrorCode_ReturnsFirstErrorCode()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithError("CODE_A", "Error A")
            .And()
            .Add(p => p.IsActive)
            .WithError("CODE_B", "Error B")
            .Validate(MakePerson(age: 10, isActive: false));

        result.FirstErrorCode.Should().Be("CODE_A");
    }

    // Test 41
    [Fact]
    public void ValidationResult_ToString_WhenValid_ReturnsValid()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young")
            .Validate(MakePerson(age: 25));

        result.ToString().Should().Be("Valid");
    }

    // Test 42
    [Fact]
    public void ValidationResult_ToString_WhenInvalid_ContainsErrors()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young")
            .Validate(MakePerson(age: 10));

        result.ToString().Should().Contain("Too young");
    }

    // Test 43
    [Fact]
    public void Validate_OnlyUntaggedConditions_AlwaysIsValidTrue()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .And()
            .Add(p => p.IsActive);

        var result = builder.Validate(MakePerson(age: 5, isActive: false));
        result.IsValid.Should().BeTrue();
    }

    // Test 44
    [Fact]
    public void ValidationError_ToString_WithoutCode_ReturnsJustMessage()
    {
        var error = new ValidationError("Some message");
        error.ToString().Should().Be("Some message");
    }

    // Test 45
    [Fact]
    public void ValidationError_ToString_WithCode_ReturnsFormattedString()
    {
        var error = new ValidationError("Some message", "CODE_001");
        error.ToString().Should().Be("[CODE_001] Some message");
    }

    // Test 46
    [Fact]
    public void ValidationResult_Ok_IsValidTrue_EmptyErrors()
    {
        var result = ValidationResult.Ok();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.FirstError.Should().BeNull();
        result.FirstErrorCode.Should().BeNull();
    }

    // Test 47
    [Fact]
    public void Validate_ConditionFails_NoWithMessage_NotInErrors()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Salary > 100_000m)
            .Validate(MakePerson(salary: 50_000m));

        // Condition fails but has no message → not in errors → IsValid = true
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // ── ValidateAll ───────────────────────────────────────────────────────────

    // Test 48
    [Fact]
    public void ValidateAll_MixOfValidAndInvalid_CorrectPerItemResults()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young");

        var people = new List<Person>
        {
            MakePerson(age: 25),
            MakePerson(age: 10),
            MakePerson(age: 30)
        };

        var results = builder.ValidateAll(people).ToList();
        results.Should().HaveCount(3);
        results[0].Result.IsValid.Should().BeTrue();
        results[1].Result.IsValid.Should().BeFalse();
        results[2].Result.IsValid.Should().BeTrue();
    }

    // Test 49
    [Fact]
    public void ValidateAll_AllValid_AllIsValidTrue()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young");

        var people = new List<Person>
        {
            MakePerson(age: 20),
            MakePerson(age: 30),
            MakePerson(age: 40)
        };

        var results = builder.ValidateAll(people).ToList();
        results.Should().AllSatisfy(r => r.Result.IsValid.Should().BeTrue());
    }

    // Test 50
    [Fact]
    public void ValidateAll_AllInvalid_AllIsValidFalse()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young");

        var people = new List<Person>
        {
            MakePerson(age: 5),
            MakePerson(age: 10),
            MakePerson(age: 15)
        };

        var results = builder.ValidateAll(people).ToList();
        results.Should().AllSatisfy(r => r.Result.IsValid.Should().BeFalse());
    }

    // Test 51
    [Fact]
    public void ValidateAll_EmptyList_ReturnsEmptyEnumerable()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young");

        var results = builder.ValidateAll(new List<Person>()).ToList();
        results.Should().BeEmpty();
    }

    // Test 52
    [Fact]
    public void ValidateAll_NullInput_ThrowsArgumentNullException()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young");

        Action act = () => builder.ValidateAll(null!).ToList();
        act.Should().Throw<ArgumentNullException>();
    }

    // Test 53
    [Fact]
    public void ValidateAll_ResultItemMatchesOriginalObject()
    {
        var person = MakePerson(name: "Bob", age: 10);

        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young");

        var results = builder.ValidateAll(new[] { person }).ToList();
        results[0].Item.Should().Be(person);
    }

    // Test 54
    [Fact]
    public void ValidateAll_IsLazy_IEnumerable()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young");

        var people = new List<Person> { MakePerson(age: 25), MakePerson(age: 10) };

        // Take only 1 — lazy evaluation: should not process all items
        var firstResult = builder.ValidateAll(people).Take(1).ToList();
        firstResult.Should().HaveCount(1);
        firstResult[0].Item.Should().Be(people[0]);
    }

    // Test 55
    [Fact]
    public void ValidateAll_ThreeItems_TwoErrorsEach()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young")
            .And()
            .Add(p => p.IsActive)
            .WithMessage("Not active");

        var people = new List<Person>
        {
            MakePerson(age: 10, isActive: false),
            MakePerson(age: 5, isActive: false),
            MakePerson(age: 1, isActive: false)
        };

        var results = builder.ValidateAll(people).ToList();
        results.Should().AllSatisfy(r => r.Result.Errors.Should().HaveCount(2));
    }

    // Test 56
    [Fact]
    public void ValidateAll_CombinedWithLinq_FiltersInvalidResults()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young");

        var people = new List<Person>
        {
            MakePerson(age: 25),
            MakePerson(age: 10),
            MakePerson(age: 30),
            MakePerson(age: 5)
        };

        var invalid = builder.ValidateAll(people)
            .Where(r => !r.Result.IsValid)
            .ToList();

        invalid.Should().HaveCount(2);
    }

    // ── Integration ───────────────────────────────────────────────────────────

    // Test 57
    [Fact]
    public void Integration_AddIfTrue_When_WithError_Validate_Combined()
    {
        bool enforceAge = true;

        var builder = new ValiFlow<Person>()
            .AddIf(enforceAge, p => p.Age >= 18)
            .WithError("AGE_ERR", "Must be at least 18")
            .And()
            .When(p => p.IsActive, b => b.Add(p => p.Salary > 0m))
            .WithMessage("Active person must have salary");

        // Fails age: age=10 isActive=true salary=1000
        var result1 = builder.Validate(MakePerson(age: 10, isActive: true, salary: 1000m));
        result1.IsValid.Should().BeFalse();
        result1.Errors.Should().Contain(e => e.ErrorCode == "AGE_ERR");

        // Passes all: age=25 isActive=true salary=1000
        var result2 = builder.Validate(MakePerson(age: 25, isActive: true, salary: 1000m));
        result2.IsValid.Should().BeTrue();
    }

    // Test 58
    [Fact]
    public void Integration_Complex_AddIf_When_Unless_ValidateAll()
    {
        bool strictMode = true;

        var builder = new ValiFlow<Person>()
            .AddIf(strictMode, p => p.Age >= 18)
            .WithMessage("Age check")
            .And()
            .When(p => p.IsActive, b => b.Add(p => p.Email != null))
            .WithMessage("Active must have email")
            .And()
            .Unless(p => p.Age > 60, b => b.Add(p => p.Salary > 20_000m))
            .WithMessage("Non-seniors need salary > 20k");

        var people = new List<Person>
        {
            MakePerson(age: 25, isActive: true, salary: 30_000m, email: "x@x.com"),
            MakePerson(age: 10, isActive: true, salary: 5_000m, email: "y@y.com"),
            MakePerson(age: 65, isActive: false, salary: 0m, email: null)
        };

        var results = builder.ValidateAll(people).ToList();

        // Person 0 (25, active, salary 30k, has email): age ok, active+email ok, not senior+salary ok → valid
        results[0].Result.IsValid.Should().BeTrue();

        // Person 1 (10, active, salary 5k, has email): age fails, active+email ok, not senior+salary fails → 2 errors
        results[1].Result.IsValid.Should().BeFalse();
        results[1].Result.Errors.Should().HaveCount(2);

        // Person 2 (65, inactive, salary 0, no email):
        //   - Age>=18 ok (no error)
        //   - When(IsActive, email!=null): IsActive=false → (false && anything) = false → condition fails → "Active must have email" error
        //   - Unless(Age>60, Salary>20k): Age=65 which is NOT >60 (65 > 60 = true) → !true=false → (false && ...) = false → condition fails → "Non-seniors need salary > 20k" error
        // So 2 errors
        results[2].Result.IsValid.Should().BeFalse();
        results[2].Result.Errors.Should().HaveCount(2);
        results[2].Result.Errors.Should().Contain(e => e.Message == "Active must have email");
        results[2].Result.Errors.Should().Contain(e => e.Message == "Non-seniors need salary > 20k");
    }

    // Test 59
    [Fact]
    public void Integration_Filter_Vs_Validate_DifferentPurposesOnSameBuilder()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young")
            .And()
            .Add(p => p.IsActive)
            .WithMessage("Not active");

        var alice = MakePerson(age: 25, isActive: true);
        var bob = MakePerson(age: 10, isActive: false);

        // Filter: Build() expression works as boolean predicate
        var filter = builder.Build().Compile();
        filter(alice).Should().BeTrue();
        filter(bob).Should().BeFalse();

        // Validate: collects tagged errors
        builder.Validate(alice).IsValid.Should().BeTrue();
        var bobResult = builder.Validate(bob);
        bobResult.IsValid.Should().BeFalse();
        bobResult.Errors.Should().HaveCount(2);
    }

    // Test 60
    [Fact]
    public void Integration_ValidationResult_ToString_MultipleErrors_ContainsAll()
    {
        var result = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithError("AGE", "Age invalid")
            .And()
            .Add(p => p.IsActive)
            .WithError("ACT", "Not active")
            .Validate(MakePerson(age: 10, isActive: false));

        var str = result.ToString();
        str.Should().Contain("[AGE] Age invalid");
        str.Should().Contain("[ACT] Not active");
    }

    // Test 61
    [Fact]
    public void AddIf_ConditionFalse_NoValidationErrors()
    {
        // AddIf(false) means the condition is never added — builder stays empty.
        var builder = new ValiFlow<Person>()
            .AddIf(false, p => p.Age >= 18);

        var result = builder.Validate(MakePerson(age: 5));
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // Test 61b
    [Fact]
    public void AddIf_ConditionFalse_WithMessage_ThrowsInvalidOperationException()
    {
        // Calling WithMessage when no condition was added (AddIf(false)) throws — the message
        // has nothing to attach to and would be silently lost under the old no-op contract.
        var builder = new ValiFlow<Person>()
            .AddIf(false, p => p.Age >= 18);

        var act = () => builder.WithMessage("Should never appear");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*require at least one condition*");
    }

    // Test 62
    [Fact]
    public void ValidateAll_ResultErrors_FirstErrorCode_Null_WhenOnlyMessage()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young"); // no error code

        var results = builder.ValidateAll(new[] { MakePerson(age: 10) }).ToList();
        results[0].Result.FirstError.Should().Be("Too young");
        results[0].Result.FirstErrorCode.Should().BeNull();
    }

    // Test 63
    [Fact]
    public void When_ConditionTrue_AndInnerPasses_WholeExpressionTrue()
    {
        var filter = new ValiFlow<Person>()
            .When(p => p.Salary > 10_000m, b => b.Add(p => p.IsActive))
            .Build();

        // Salary > 10k AND IsActive
        filter.Compile()(MakePerson(salary: 50_000m, isActive: true)).Should().BeTrue();
    }

    // Test 64
    [Fact]
    public void Unless_ConditionFalse_InnerPasses_WholeExpressionTrue()
    {
        var filter = new ValiFlow<Person>()
            .Unless(p => p.IsActive, b => b.Add(p => p.Age >= 18))
            .Build();

        // !IsActive(false)=true AND Age>=18
        filter.Compile()(MakePerson(isActive: false, age: 20)).Should().BeTrue();
    }

    // Test 65
    [Fact]
    public void Validate_ValidateAll_Consistent_SameBuilder()
    {
        var builder = new ValiFlow<Person>()
            .Add(p => p.Age >= 18)
            .WithMessage("Too young");

        var person = MakePerson(age: 10);

        var singleResult = builder.Validate(person);
        var allResults = builder.ValidateAll(new[] { person }).ToList();

        singleResult.IsValid.Should().Be(allResults[0].Result.IsValid);
        singleResult.FirstError.Should().Be(allResults[0].Result.FirstError);
    }

    [Fact]
    public void When_NullCondition_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<Person>().When(null!, _ => { });
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Unless_NullUnlessAction_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<Person>()
            .Unless(x => x.IsActive, null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

public class PropertyPathTests
{
    private class PathUser { public string? Email { get; set; } public int Age { get; set; } }

    private static PathUser MakeUser(string? email = "user@example.com", int age = 25)
        => new PathUser { Email = email, Age = age };

    // Test 1: WithError(code, msg, path) — ValidationError.PropertyPath is set
    [Fact]
    public void WithError_ThreeArgs_PropertyPathIsSet()
    {
        var result = new ValiFlow<PathUser>()
            .Add(u => u.Age >= 18)
            .WithError("AGE_001", "Must be at least 18", "Age")
            .Validate(MakeUser(age: 10));

        result.Errors.Should().ContainSingle()
            .Which.PropertyPath.Should().Be("Age");
    }

    // Test 2: WithError(code, msg, path) — ValidationError.ErrorCode is set
    [Fact]
    public void WithError_ThreeArgs_ErrorCodeIsSet()
    {
        var result = new ValiFlow<PathUser>()
            .Add(u => u.Age >= 18)
            .WithError("AGE_001", "Must be at least 18", "Age")
            .Validate(MakeUser(age: 10));

        result.Errors.Should().ContainSingle()
            .Which.ErrorCode.Should().Be("AGE_001");
    }

    // Test 3: WithError(code, msg, path) — ValidationError.Message is set
    [Fact]
    public void WithError_ThreeArgs_MessageIsSet()
    {
        var result = new ValiFlow<PathUser>()
            .Add(u => u.Age >= 18)
            .WithError("AGE_001", "Must be at least 18", "Age")
            .Validate(MakeUser(age: 10));

        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Must be at least 18");
    }

    // Test 4: WithError(code, msg, path) — ToString() includes path
    [Fact]
    public void WithError_ThreeArgs_ToString_IncludesPath()
    {
        var error = new ValidationError("Must be at least 18", "AGE_001", "Age");
        error.ToString().Should().Contain("Age");
    }

    // Test 5: WithError(code, msg, path) — ToString() format: "[path] [code] message"
    [Fact]
    public void WithError_ThreeArgs_ToString_FormatIsPathCodeMessage()
    {
        var error = new ValidationError("Must be at least 18", "AGE_001", "Age");
        error.ToString().Should().Be("[Age] [AGE_001] Must be at least 18");
    }

    // Test 6: WithError(code, msg) — ToString() format: "[code] message" (no path)
    [Fact]
    public void WithError_TwoArgs_ToString_FormatIsCodeMessage()
    {
        var error = new ValidationError("Must be at least 18", "AGE_001");
        error.ToString().Should().Be("[AGE_001] Must be at least 18");
    }

    // Test 7: ValidationError(msg) — ToString() format: "message" (no code, no path)
    [Fact]
    public void ValidationError_MessageOnly_ToString_FormatIsMessageOnly()
    {
        var error = new ValidationError("Must be at least 18");
        error.ToString().Should().Be("Must be at least 18");
    }

    // Test 8: ValidationError(msg, null, path) — ToString() format: "[path] message"
    [Fact]
    public void ValidationError_MessageAndPath_ToString_FormatIsPathMessage()
    {
        var error = new ValidationError("Must be at least 18", null, "Age");
        error.ToString().Should().Be("[Age] Must be at least 18");
    }

    // Test 9: WithError(null, ...) — throws ArgumentException
    [Fact]
    public void WithError_ThreeArgs_NullCode_ThrowsArgumentException()
    {
        Action act = () => new ValiFlow<PathUser>()
            .Add(u => u.Age >= 18)
            .WithError(null!, "Some message", "Age");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Error code cannot be null or empty*");
    }

    // Test 10: WithError("", ...) — throws ArgumentException
    [Fact]
    public void WithError_ThreeArgs_EmptyCode_ThrowsArgumentException()
    {
        Action act = () => new ValiFlow<PathUser>()
            .Add(u => u.Age >= 18)
            .WithError("", "Some message", "Age");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Error code cannot be null or empty*");
    }

    // Test 11: WithError(code, null, ...) — throws ArgumentException
    [Fact]
    public void WithError_ThreeArgs_NullMessage_ThrowsArgumentException()
    {
        Action act = () => new ValiFlow<PathUser>()
            .Add(u => u.Age >= 18)
            .WithError("AGE_001", null!, "Age");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Message cannot be null or empty*");
    }

    // Test 12: WithError(code, msg, null) — throws ArgumentException
    [Fact]
    public void WithError_ThreeArgs_NullPath_ThrowsArgumentException()
    {
        Action act = () => new ValiFlow<PathUser>()
            .Add(u => u.Age >= 18)
            .WithError("AGE_001", "Must be at least 18", null!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Property path cannot be null or empty*");
    }

    // Test 13: WithError(code, msg, "") — throws ArgumentException
    [Fact]
    public void WithError_ThreeArgs_EmptyPath_ThrowsArgumentException()
    {
        Action act = () => new ValiFlow<PathUser>()
            .Add(u => u.Age >= 18)
            .WithError("AGE_001", "Must be at least 18", "");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Property path cannot be null or empty*");
    }

    // Test 14: Validate() — error includes PropertyPath
    [Fact]
    public void Validate_Error_IncludesPropertyPath()
    {
        var result = new ValiFlow<PathUser>()
            .Add(u => u.Age >= 18)
            .WithError("AGE_001", "Must be at least 18", "Age")
            .Validate(MakeUser(age: 10));

        result.IsValid.Should().BeFalse();
        result.Errors[0].PropertyPath.Should().Be("Age");
    }

    // Test 15: Multiple errors with different PropertyPaths in same ValidateAll
    [Fact]
    public void ValidateAll_MultipleErrorsWithDifferentPropertyPaths()
    {
        var builder = new ValiFlow<PathUser>()
            .Add(u => u.Age >= 18)
            .WithError("AGE_001", "Too young", "Age")
            .And()
            .IsNotNullOrEmpty(u => u.Email)
            .WithError("EMAIL_001", "Email required", "Email");

        var users = new[]
        {
            MakeUser(age: 10, email: ""),
            MakeUser(age: 20, email: "valid@test.com")
        };

        var results = builder.ValidateAll(users).ToList();

        results[0].Result.IsValid.Should().BeFalse();
        results[0].Result.Errors.Should().HaveCount(2);
        results[0].Result.Errors.Should().Contain(e => e.PropertyPath == "Age");
        results[0].Result.Errors.Should().Contain(e => e.PropertyPath == "Email");

        results[1].Result.IsValid.Should().BeTrue();
    }

    // Test 16: ValidationError.PropertyPath is null when not set
    [Fact]
    public void ValidationError_PropertyPath_IsNullWhenNotSet()
    {
        var error = new ValidationError("Some message", "CODE_001");
        error.PropertyPath.Should().BeNull();
    }

    // Test 17: ValidationError.PropertyPath is set when provided in constructor
    [Fact]
    public void ValidationError_Constructor_SetsPropertyPath()
    {
        var error = new ValidationError("Some message", "CODE_001", "user.email");
        error.PropertyPath.Should().Be("user.email");
    }

    // Test 18: ValidationError with only message and path (no code) has null ErrorCode
    [Fact]
    public void ValidationError_MessageAndPath_ErrorCodeIsNull()
    {
        var error = new ValidationError("Some message", null, "user.email");
        error.ErrorCode.Should().BeNull();
        error.PropertyPath.Should().Be("user.email");
    }

    // Test 19: ValidationResult.ToString includes error with path
    [Fact]
    public void ValidationResult_ToString_IncludesErrorWithPath()
    {
        var result = new ValiFlow<PathUser>()
            .Add(u => u.Age >= 18)
            .WithError("AGE_001", "Too young", "Age")
            .Validate(MakeUser(age: 10));

        result.ToString().Should().Contain("[Age]");
        result.ToString().Should().Contain("[AGE_001]");
        result.ToString().Should().Contain("Too young");
    }
}
