using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;
using Vali_Flow.Core.Models;

namespace Vali_Flow.Core.Tests;

public class ValidateNestedTests
{
    private class Address
    {
        public string? City { get; set; }
        public string? ZipCode { get; set; }
    }

    private class NestedPerson
    {
        public string? Name { get; set; }
        public Address? Address { get; set; }
        public int Age { get; set; }
    }

    private static NestedPerson MakePerson(
        string? name = "Alice",
        int age = 30,
        string? city = "New York",
        string? zipCode = "10001")
        => new NestedPerson
        {
            Name = name,
            Age = age,
            Address = new Address { City = city, ZipCode = zipCode }
        };

    private static NestedPerson MakePersonWithNullAddress(string? name = "Bob", int age = 25)
        => new NestedPerson { Name = name, Age = age, Address = null };

    // Test 1: ValidateNested — nested object valid → IsValid true
    [Fact]
    public void ValidateNested_NestedObjectValid_IsValidTrue()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City));

        builder.IsValid(MakePerson()).Should().BeTrue();
    }

    // Test 2: ValidateNested — nested object fails condition → IsValid false
    [Fact]
    public void ValidateNested_NestedObjectFailsCondition_IsValidFalse()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City));

        builder.IsValid(MakePerson(city: "")).Should().BeFalse();
    }

    // Test 3: ValidateNested — null nested property → IsValid false (automatic null check)
    [Fact]
    public void ValidateNested_NullNestedProperty_IsValidFalse()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City));

        builder.IsValid(MakePersonWithNullAddress()).Should().BeFalse();
    }

    // Test 4: ValidateNested — multiple conditions in inner builder, all pass → true
    [Fact]
    public void ValidateNested_MultipleConditionsAllPass_ReturnsTrue()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr
                .IsNotNullOrEmpty(a => a.City)
                .And()
                .IsNotNullOrEmpty(a => a.ZipCode));

        builder.IsValid(MakePerson(city: "Chicago", zipCode: "60601")).Should().BeTrue();
    }

    // Test 5: ValidateNested — multiple conditions in inner builder, one fails → false
    [Fact]
    public void ValidateNested_MultipleConditionsOneFails_ReturnsFalse()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr
                .IsNotNullOrEmpty(a => a.City)
                .And()
                .IsNotNullOrEmpty(a => a.ZipCode));

        builder.IsValid(MakePerson(city: "Chicago", zipCode: "")).Should().BeFalse();
    }

    // Test 6: ValidateNested — chained with And (outer condition AND nested) → both must pass
    [Fact]
    public void ValidateNested_ChainedWithAnd_BothMustPass()
    {
        var filter = new ValiFlow<NestedPerson>()
            .IsNotNullOrEmpty(p => p.Name)
            .And()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build();

        var compiled = filter.Compile();
        compiled(MakePerson(name: "Alice", city: "NYC")).Should().BeTrue();
        compiled(MakePerson(name: null, city: "NYC")).Should().BeFalse();
        compiled(MakePerson(name: "Alice", city: "")).Should().BeFalse();
    }

    // Test 7: ValidateNested — chained with Or (outer condition OR nested)
    [Fact]
    public void ValidateNested_ChainedWithOr_EitherCanPass()
    {
        var filter = new ValiFlow<NestedPerson>()
            .GreaterThan(p => p.Age, 60)
            .Or()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build();

        var compiled = filter.Compile();
        // Age > 60 → passes without nested check
        compiled(MakePerson(age: 70, city: "")).Should().BeTrue();
        // Age <= 60 but city is valid → passes via nested
        compiled(MakePerson(age: 25, city: "Boston")).Should().BeTrue();
        // Age <= 60 and city empty → fails both
        compiled(MakePerson(age: 25, city: "")).Should().BeFalse();
    }

    // Test 8: ValidateNested — null selector throws ArgumentNullException
    [Fact]
    public void ValidateNested_NullSelector_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<NestedPerson>()
            .ValidateNested<Address>(null!, addr => addr.IsNotNullOrEmpty(a => a.City));

        act.Should().Throw<ArgumentNullException>();
    }

    // Test 9: ValidateNested — null configure throws ArgumentNullException
    [Fact]
    public void ValidateNested_NullConfigure_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<NestedPerson>()
            .ValidateNested<Address>(p => p.Address, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // Test 10: ValidateNested — nested with WithMessage → ValidationResult has error message
    [Fact]
    public void ValidateNested_WithMessage_ValidationResultHasError()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .WithMessage("City is required");

        var result = builder.Validate(MakePerson(city: ""));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("City is required");
    }

    // Test 11: ValidateNested — nested with EqualTo string
    [Fact]
    public void ValidateNested_WithEqualToString_CorrectResult()
    {
        var filter = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.EqualTo(a => a.City, "Paris"))
            .Build();

        var compiled = filter.Compile();
        compiled(MakePerson(city: "Paris")).Should().BeTrue();
        compiled(MakePerson(city: "London")).Should().BeFalse();
    }

    // Test 12: ValidateNested — nested with MinLength string
    [Fact]
    public void ValidateNested_WithMinLength_CorrectResult()
    {
        var filter = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.MinLength(a => a.City, 3))
            .Build();

        var compiled = filter.Compile();
        compiled(MakePerson(city: "New York")).Should().BeTrue();
        compiled(MakePerson(city: "LA")).Should().BeFalse();
    }

    // Test 13: ValidateNested — nested 2 levels deep (Address.City)
    [Fact]
    public void ValidateNested_TwoLevelsDeep_CorrectResult()
    {
        // Simulate 2-level nesting: outer filters on Address, inner filters on City
        var filter = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr =>
                addr.ValidateNested(a => a, inner => inner.IsNotNullOrEmpty(a => a.ZipCode)))
            .Build();

        var compiled = filter.Compile();
        compiled(MakePerson(zipCode: "10001")).Should().BeTrue();
        compiled(MakePerson(zipCode: "")).Should().BeFalse();
    }

    // Test 14: ValidateNested — used in Build() then applied to IQueryable
    [Fact]
    public void ValidateNested_UsedInBuild_AppliesToIQueryable()
    {
        var people = new List<NestedPerson>
        {
            MakePerson(name: "Alice", city: "NYC"),
            MakePerson(name: "Bob", city: ""),
            MakePersonWithNullAddress("Carol")
        }.AsQueryable();

        var expr = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build();

        people.Where(expr).Should().ContainSingle().Which.Name.Should().Be("Alice");
    }

    // Test 15: ValidateNested — used in Build() then applied to list.Where()
    [Fact]
    public void ValidateNested_UsedInBuild_AppliesToListWhere()
    {
        var people = new List<NestedPerson>
        {
            MakePerson(name: "Alice", city: "NYC"),
            MakePerson(name: "Bob", city: "Los Angeles"),
            MakePerson(name: "Carol", city: "")
        };

        var compiled = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build()
            .Compile();

        people.Where(compiled).Should().HaveCount(2);
    }

    // Test 16: ValidateNested — works with multiple entities in ValidateAll
    [Fact]
    public void ValidateNested_WorksWithValidateAll()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .WithMessage("City required");

        var people = new[]
        {
            MakePerson(name: "Alice", city: "NYC"),
            MakePerson(name: "Bob", city: "")
        };

        var results = builder.ValidateAll(people).ToList();
        results[0].Result.IsValid.Should().BeTrue();
        results[1].Result.IsValid.Should().BeFalse();
    }

    // Test 17: ValidateNested — when condition precedes ValidateNested
    [Fact]
    public void ValidateNested_WhenConditionPrecedes_BothApply()
    {
        var filter = new ValiFlow<NestedPerson>()
            .Add(p => p.Age >= 18)
            .And()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build();

        var compiled = filter.Compile();
        compiled(MakePerson(age: 20, city: "NYC")).Should().BeTrue();
        compiled(MakePerson(age: 16, city: "NYC")).Should().BeFalse();
        compiled(MakePerson(age: 20, city: "")).Should().BeFalse();
    }

    // Test 18: AddIf(false) + ValidateNested (condition not added)
    [Fact]
    public void ValidateNested_WithAddIfFalse_ConditionNotAdded()
    {
        var filter = new ValiFlow<NestedPerson>()
            .AddIf(false, p => p.Age >= 18)
            .And()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build();

        var compiled = filter.Compile();
        // Age condition not added, only nested matters
        compiled(MakePerson(age: 5, city: "NYC")).Should().BeTrue();
        compiled(MakePerson(age: 5, city: "")).Should().BeFalse();
    }

    // Test 19: AddIf(true) + ValidateNested (condition added)
    [Fact]
    public void ValidateNested_WithAddIfTrue_ConditionAdded()
    {
        var filter = new ValiFlow<NestedPerson>()
            .AddIf(true, p => p.Age >= 18)
            .And()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build();

        var compiled = filter.Compile();
        compiled(MakePerson(age: 20, city: "NYC")).Should().BeTrue();
        compiled(MakePerson(age: 16, city: "NYC")).Should().BeFalse();
    }

    // Test 20: ValidateNested — builds correct expression tree (Build() not null)
    [Fact]
    public void ValidateNested_BuildReturnsNonNullExpression()
    {
        var expr = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build();

        expr.Should().NotBeNull();
        expr.Body.Should().NotBeNull();
    }

    // Test 21: ValidateNested — empty string in nested field
    [Fact]
    public void ValidateNested_EmptyStringInNestedField_FailsIsNotNullOrEmpty()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.ZipCode));

        builder.IsValid(MakePerson(zipCode: "")).Should().BeFalse();
    }

    // Test 22: ValidateNested — whitespace string in nested field
    [Fact]
    public void ValidateNested_WhitespaceInNestedField_FailsIsNotNullOrWhiteSpace()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrWhiteSpace(a => a.City));

        builder.IsValid(MakePerson(city: "   ")).Should().BeFalse();
    }

    // Test 23: ValidateNested — null nested field value
    [Fact]
    public void ValidateNested_NullNestedFieldValue_FailsIsNotNullOrEmpty()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City));

        builder.IsValid(MakePerson(city: null)).Should().BeFalse();
    }

    // Test 24: ValidateNested — NotNull check in nested builder passes when value present
    [Fact]
    public void ValidateNested_NotNullCheckInNested_PassesWhenValuePresent()
    {
        var filter = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.ZipCode))
            .Build();

        filter.Compile()(MakePerson(zipCode: "90210")).Should().BeTrue();
    }

    // Test 25: ValidateNested — condition on outer object combined with nested OR
    [Fact]
    public void ValidateNested_OuterIsActiveOrNestedCityValid_EitherPasses()
    {
        var filter = new ValiFlow<NestedPerson>()
            .Add(p => p.Age > 65)
            .Or()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.ZipCode))
            .Build();

        var compiled = filter.Compile();
        compiled(MakePerson(age: 70, zipCode: "")).Should().BeTrue();     // age passes
        compiled(MakePerson(age: 25, zipCode: "10001")).Should().BeTrue(); // zip passes
        compiled(MakePerson(age: 25, zipCode: "")).Should().BeFalse();     // neither
    }

    // Test 26: ValidateNested — Validate returns correct IsValid for passing entity
    [Fact]
    public void ValidateNested_Validate_PassingEntity_IsValidTrue()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr
                .IsNotNullOrEmpty(a => a.City)
                .WithMessage("City required"));

        builder.Validate(MakePerson(city: "Dallas")).IsValid.Should().BeTrue();
    }

    // Test 27: ValidateNested — Validate returns correct IsValid for failing entity
    [Fact]
    public void ValidateNested_Validate_FailingEntity_IsValidFalse()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .WithMessage("City required");

        var result = builder.Validate(MakePerson(city: null));
        result.IsValid.Should().BeFalse();
        result.FirstError.Should().Be("City required");
    }

    // Test 28: ValidateNested — multiple WithError in nested, includes PropertyPath
    [Fact]
    public void ValidateNested_WithError_InNested_ErrorCodeAndMessagePresent()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .WithError("CITY_001", "City is required");

        var result = builder.Validate(MakePerson(city: ""));
        result.IsValid.Should().BeFalse();
        result.Errors[0].ErrorCode.Should().Be("CITY_001");
        result.Errors[0].Message.Should().Be("City is required");
    }

    // Test 29: ValidateNested — nested with GreaterThan numeric on outer + nested
    [Fact]
    public void ValidateNested_CombinedWithNumericConditionOnOuter_WorksCorrectly()
    {
        var filter = new ValiFlow<NestedPerson>()
            .GreaterThan(p => p.Age, 0)
            .And()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build();

        var compiled = filter.Compile();
        compiled(MakePerson(age: 1, city: "NYC")).Should().BeTrue();
        compiled(MakePerson(age: 0, city: "NYC")).Should().BeFalse();
    }

    // Test 30: ValidateNested — IsValid used directly on builder (no Build needed)
    [Fact]
    public void ValidateNested_IsValidCalledDirectly_ReturnsCorrectResult()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.EqualTo(a => a.City, "Seattle"));

        builder.IsValid(MakePerson(city: "Seattle")).Should().BeTrue();
        builder.IsValid(MakePerson(city: "Portland")).Should().BeFalse();
    }

    // Test 31: ValidateNested — IsNotValid used on builder
    [Fact]
    public void ValidateNested_IsNotValid_ReturnsCorrectResult()
    {
        var builder = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City));

        builder.IsNotValid(MakePerson(city: "")).Should().BeTrue();
        builder.IsNotValid(MakePerson(city: "NYC")).Should().BeFalse();
    }

    // Test 32: ValidateNested — expression tree produced is not constant true
    [Fact]
    public void ValidateNested_ExpressionTree_IsNotAlwaysTrue()
    {
        var filter = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build();

        var compiled = filter.Compile();
        // at least one should be false to confirm it's not constant
        compiled(MakePerson(city: "")).Should().BeFalse();
    }

    // Test 33: ValidateNested — two sequential ValidateNested calls both apply
    [Fact]
    public void ValidateNested_TwoSequentialNestedCalls_BothApply()
    {
        var filter = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .And()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.ZipCode))
            .Build();

        var compiled = filter.Compile();
        compiled(MakePerson(city: "NYC", zipCode: "10001")).Should().BeTrue();
        compiled(MakePerson(city: "NYC", zipCode: "")).Should().BeFalse();
        compiled(MakePerson(city: "", zipCode: "10001")).Should().BeFalse();
    }

    // Test 34: ValidateNested — null address with multiple entities in list
    [Fact]
    public void ValidateNested_NullAddressInMixedList_OnlyValidEntitiesPass()
    {
        var people = new List<NestedPerson>
        {
            MakePerson(name: "Alice", city: "NYC"),
            MakePersonWithNullAddress("Bob"),
            MakePerson(name: "Carol", city: "LA")
        };

        var compiled = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build()
            .Compile();

        people.Where(compiled).Select(p => p.Name).Should().BeEquivalentTo(new[] { "Alice", "Carol" });
    }

    // Test 36: ValidateNested — null nested property via compiled expression returns false
    [Fact]
    public void ValidateNested_NullNestedProperty_ReturnsFalse()
    {
        var filter = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build();

        filter.Compile()(MakePersonWithNullAddress()).Should().BeFalse();
    }

    // Test 35: ValidateNested — inner builder uses Or condition
    [Fact]
    public void ValidateNested_InnerBuilderUsesOr_EitherNestedConditionPasses()
    {
        var filter = new ValiFlow<NestedPerson>()
            .ValidateNested(p => p.Address, addr => addr
                .EqualTo(a => a.City, "NYC")
                .Or()
                .EqualTo(a => a.City, "LA"))
            .Build();

        var compiled = filter.Compile();
        compiled(MakePerson(city: "NYC")).Should().BeTrue();
        compiled(MakePerson(city: "LA")).Should().BeTrue();
        compiled(MakePerson(city: "Boston")).Should().BeFalse();
    }
}
