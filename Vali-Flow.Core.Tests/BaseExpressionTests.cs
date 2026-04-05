using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

public record Product(string? Name, decimal Price, int Quantity, bool IsActive, DateTime CreatedAt, List<string> Tags);

public class BaseExpressionTests
{
    private static Product MakeProduct(
        string? name = "Test",
        decimal price = 10m,
        int quantity = 5,
        bool isActive = true,
        DateTime createdAt = default,
        List<string>? tags = null)
        => new(name, price, quantity, isActive,
            createdAt == default ? DateTime.UtcNow : createdAt,
            tags ?? new List<string>());

    // 1. Build() with no conditions returns expression that evaluates to true for all
    [Fact]
    public void Build_NoConditions_ReturnsTrueForAll()
    {
        var filter = new ValiFlow<Product>()
            .Build();

        filter.Compile()(MakeProduct()).Should().BeTrue();
        filter.Compile()(MakeProduct(name: null, quantity: -999)).Should().BeTrue();
    }

    // 2. Build() with single Add condition
    [Fact]
    public void Build_SingleAddCondition_FiltersCorrectly()
    {
        var filter = new ValiFlow<Product>()
            .Add(p => p.Quantity > 0)
            .Build();

        filter.Compile()(MakeProduct(quantity: 5)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 0)).Should().BeFalse();
    }

    // 3. And() combines two conditions: both must be true
    [Fact]
    public void And_TwoConditions_BothMustBeTrue()
    {
        var filter = new ValiFlow<Product>()
            .Add(p => p.Quantity > 0)
            .And()
            .Add(p => p.IsActive)
            .Build();

        filter.Compile()(MakeProduct(quantity: 5, isActive: true)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 5, isActive: false)).Should().BeFalse();
        filter.Compile()(MakeProduct(quantity: 0, isActive: true)).Should().BeFalse();
        filter.Compile()(MakeProduct(quantity: 0, isActive: false)).Should().BeFalse();
    }

    // 4. Or() combines two conditions: either can be true
    [Fact]
    public void Or_TwoConditions_EitherCanBeTrue()
    {
        var filter = new ValiFlow<Product>()
            .Add(p => p.Quantity > 10)
            .Or()
            .Add(p => p.IsActive)
            .Build();

        filter.Compile()(MakeProduct(quantity: 15, isActive: false)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 1, isActive: true)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 15, isActive: true)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 1, isActive: false)).Should().BeFalse();
    }

    // 5. AddSubGroup() groups nested conditions correctly
    [Fact]
    public void AddSubGroup_NestedConditions_GroupedCorrectly()
    {
        var filter = new ValiFlow<Product>()
            .AddSubGroup(g =>
            {
                g.Add(p => p.Quantity > 0)
                 .And()
                 .Add(p => p.IsActive);
            })
            .Build();

        filter.Compile()(MakeProduct(quantity: 5, isActive: true)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 5, isActive: false)).Should().BeFalse();
        filter.Compile()(MakeProduct(quantity: 0, isActive: true)).Should().BeFalse();
    }

    // 6. BuildNegated() returns inverted expression
    [Fact]
    public void BuildNegated_InvertsExpression()
    {
        var filter = new ValiFlow<Product>()
            .Add(p => p.IsActive)
            .BuildNegated();

        filter.Compile()(MakeProduct(isActive: false)).Should().BeTrue();
        filter.Compile()(MakeProduct(isActive: true)).Should().BeFalse();
    }

    // 7. Add() with null expression throws ArgumentNullException
    [Fact]
    public void Add_NullExpression_ThrowsArgumentNullException()
    {
        var builder = new ValiFlow<Product>();

        Action act = () => builder.Add(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // 8. Add() with always-true constant expression throws ArgumentException
    [Fact]
    public void Add_AlwaysTrueConstantExpression_ThrowsArgumentException()
    {
        var builder = new ValiFlow<Product>();

        Action act = () => builder.Add(_ => true);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*always 'true'*");
    }

    // 9. Or() precedence: AddSubGroup(A AND B).Or().C produces (A AND B) OR C
    [Fact]
    public void Or_Precedence_SubGroupAAndB_OrC_CorrectGrouping()
    {
        // (A AND B) OR C
        // A: Quantity > 0
        // B: IsActive == true
        // C: Price > 100
        var filter = new ValiFlow<Product>()
            .AddSubGroup(g =>
            {
                g.Add(p => p.Quantity > 0)
                 .And()
                 .Add(p => p.IsActive);
            })
            .Or()
            .Add(p => p.Price > 100m)
            .Build();

        var compiled = filter.Compile();

        // (A AND B) is true, C is false -> should pass
        compiled(MakeProduct(quantity: 5, isActive: true, price: 10m)).Should().BeTrue();
        // (A AND B) is false (isActive=false), C is true -> should pass
        compiled(MakeProduct(quantity: 5, isActive: false, price: 200m)).Should().BeTrue();
        // (A AND B) is false, C is false -> should fail
        compiled(MakeProduct(quantity: 5, isActive: false, price: 10m)).Should().BeFalse();
        // A is false, B is true, C is false -> (A AND B) = false, C = false -> should fail
        compiled(MakeProduct(quantity: 0, isActive: true, price: 10m)).Should().BeFalse();
    }
}

public class EvaluationMethodTests
{
    private static readonly Product Active = new("Widget", 10m, 5, true, DateTime.Today, new List<string> { "a" });
    private static readonly Product Inactive = new("Widget", 0m, 0, false, DateTime.Today, new List<string>());

    private static ValiFlow<Product> ActiveFilter() =>
        new ValiFlow<Product>().IsTrue(p => p.IsActive);

    // --- IsValid / IsNotValid ---

    [Fact]
    public void IsValid_ReturnsTrue_WhenInstanceSatisfiesConditions()
    {
        ActiveFilter().IsValid(Active).Should().BeTrue();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenInstanceDoesNotSatisfyConditions()
    {
        ActiveFilter().IsValid(Inactive).Should().BeFalse();
    }

    [Fact]
    public void IsNotValid_ReturnsTrue_WhenInstanceDoesNotSatisfyConditions()
    {
        ActiveFilter().IsNotValid(Inactive).Should().BeTrue();
    }

    [Fact]
    public void IsNotValid_ReturnsFalse_WhenInstanceSatisfiesConditions()
    {
        ActiveFilter().IsNotValid(Active).Should().BeFalse();
    }

    // --- Build() applied by caller (EF Core / in-memory) ---

    [Fact]
    public void Build_ExpressionCanBeAppliedToInMemoryCollection()
    {
        var source = new List<Product> { Active, Inactive };
        var expr = ActiveFilter().Build().Compile();
        source.Where(expr).Should().ContainSingle().Which.Should().Be(Active);
    }

    [Fact]
    public void Build_ExpressionCanBeAppliedToIQueryable()
    {
        var source = new List<Product> { Active, Inactive }.AsQueryable();
        var expr = ActiveFilter().Build();
        source.Where(expr).Should().ContainSingle().Which.Should().Be(Active);
    }

    [Fact]
    public void Build_ExpressionCanBeUsedWithFirstOrDefault()
    {
        var source = new List<Product> { Inactive, Active };
        var expr = ActiveFilter().Build().Compile();
        source.FirstOrDefault(expr).Should().Be(Active);
    }

    [Fact]
    public void Build_ExpressionCanBeUsedWithCount()
    {
        var source = new List<Product> { Active, Active, Inactive };
        var expr = ActiveFilter().Build().Compile();
        source.Count(expr).Should().Be(2);
    }

    [Fact]
    public void Build_ExpressionCanBeUsedWithAny()
    {
        var source = new List<Product> { Active, Inactive };
        var expr = ActiveFilter().Build().Compile();
        source.Any(expr).Should().BeTrue();
    }
}

public class BuildCachedTests
{
    private class CachedEntity { public int Value { get; set; } public string? Name { get; set; } }

    private static CachedEntity MakeEntity(int value = 10, string? name = "Test")
        => new CachedEntity { Value = value, Name = name };

    // Test 1: BuildCached — first call returns non-null func
    [Fact]
    public void BuildCached_FirstCall_ReturnsNonNullFunc()
    {
        var builder = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0);

        var func = builder.BuildCached();

        func.Should().NotBeNull();
    }

    // Test 2: BuildCached — second call returns same instance (ReferenceEquals)
    [Fact]
    public void BuildCached_SecondCall_ReturnsSameInstance()
    {
        var builder = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0);

        var first = builder.BuildCached();
        var second = builder.BuildCached();

        ReferenceEquals(first, second).Should().BeTrue();
    }

    // Test 3: BuildCached — result is functionally equivalent to Build().Compile()
    [Fact]
    public void BuildCached_ResultFunctionallyEquivalentToBuildCompile()
    {
        var entity = MakeEntity(value: 5);
        var entityFail = MakeEntity(value: -1);

        var builder = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0);

        var cached = builder.BuildCached();
        var compiled = builder.Build().Compile();

        cached(entity).Should().Be(compiled(entity));
        cached(entityFail).Should().Be(compiled(entityFail));
    }

    // Test 4: BuildCached — builder is frozen after first call; mutation returns a fork, not throws
    [Fact]
    public void BuildCached_FreezesBuilder_MutationReturnsDerivedBuilder()
    {
        var builder = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0);

        builder.BuildCached();

        // Mutation on a frozen builder returns a NEW independent fork — never throws
        var fork = builder.And().IsNotNullOrEmpty(e => e.Name);

        // fork is a new builder with both conditions
        fork.IsValid(MakeEntity(value: 5, name: "ok")).Should().BeTrue();
        fork.IsValid(MakeEntity(value: 5, name: "")).Should().BeFalse();

        // Original is unchanged (only the Value > 0 condition)
        builder.IsValid(MakeEntity(value: 5, name: "")).Should().BeTrue();
    }

    // Test 4b: IsValid — builder is frozen after first call; mutation returns a fork
    [Fact]
    public void IsValid_FreezesBuilder_MutationReturnsDerivedBuilder()
    {
        var builder = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0);

        builder.IsValid(MakeEntity(value: 5));

        var fork = builder.GreaterThan(e => e.Value, 100);

        // fork has the stricter condition
        fork.IsValid(MakeEntity(value: 50)).Should().BeFalse();
        fork.IsValid(MakeEntity(value: 200)).Should().BeTrue();

        // Original still uses > 0
        builder.IsValid(MakeEntity(value: 50)).Should().BeTrue();
    }

    // Test 4c: Validate — builder is frozen after first call; mutation returns a fork
    [Fact]
    public void Validate_FreezesBuilder_MutationReturnsDerivedBuilder()
    {
        var builder = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0)
            .WithMessage("must be positive");

        builder.Validate(MakeEntity(value: 5));

        var fork = builder.GreaterThan(e => e.Value, 100).WithMessage("must be > 100");

        // Fork has two annotated conditions
        var result = fork.Validate(MakeEntity(value: 50));
        result.Errors.Should().HaveCount(1); // only the > 100 condition fails

        // Original still has only one condition
        builder.Validate(MakeEntity(value: 50)).Errors.Should().BeEmpty();
    }

    // Test 4d: IsNotValid — builder is frozen after first call; mutation returns a fork
    [Fact]
    public void IsNotValid_FreezesBuilder_MutationReturnsDerivedBuilder()
    {
        var builder = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0);

        builder.IsNotValid(MakeEntity(value: -1));

        var fork = builder.GreaterThan(e => e.Value, 100);

        fork.IsValid(MakeEntity(value: 50)).Should().BeFalse();
        builder.IsValid(MakeEntity(value: 50)).Should().BeTrue(); // original unchanged
    }

    // Test 4e: Freeze() — explicit freeze; mutation returns a fork, original is sealed
    [Fact]
    public void Freeze_ExplicitCall_MutationReturnsDerivedBuilder()
    {
        var builder = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0);

        builder.Freeze();

        var fork = builder.GreaterThan(e => e.Value, 100);

        // fork is independent
        fork.IsValid(MakeEntity(value: 50)).Should().BeFalse();
        fork.IsValid(MakeEntity(value: 200)).Should().BeTrue();

        // original sealed — still only Value > 0
        builder.IsValid(MakeEntity(value: 50)).Should().BeTrue();
    }

    // Test 4f: Frozen builder — IsValid and BuildCached still work after freeze
    [Fact]
    public void FrozenBuilder_IsValidAndBuildCached_StillWorkConcurrently()
    {
        var builder = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0);

        builder.Freeze();

        // Read operations must succeed on frozen builder
        builder.IsValid(MakeEntity(value: 5)).Should().BeTrue();
        builder.IsValid(MakeEntity(value: -1)).Should().BeFalse();
        builder.BuildCached()(MakeEntity(value: 5)).Should().BeTrue();
    }

    // Test 5: BuildCached — evaluates correctly: true case
    [Fact]
    public void BuildCached_EvaluatesCorrectly_TrueCase()
    {
        var func = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0)
            .BuildCached();

        func(MakeEntity(value: 5)).Should().BeTrue();
    }

    // Test 6: BuildCached — evaluates correctly: false case
    [Fact]
    public void BuildCached_EvaluatesCorrectly_FalseCase()
    {
        var func = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0)
            .BuildCached();

        func(MakeEntity(value: -1)).Should().BeFalse();
    }

    // Test 7: BuildCached — no conditions → returns func that always returns true
    [Fact]
    public void BuildCached_NoConditions_AlwaysReturnsTrue()
    {
        var func = new ValiFlow<CachedEntity>()
            .BuildCached();

        func(MakeEntity(value: -999, name: null)).Should().BeTrue();
        func(MakeEntity(value: 1000, name: "anything")).Should().BeTrue();
    }

    // Test 8: BuildCached — multiple calls, all return same instance
    [Fact]
    public void BuildCached_MultipleCalls_AllReturnSameInstance()
    {
        var builder = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0);

        var results = Enumerable.Range(0, 5)
            .Select(_ => builder.BuildCached())
            .ToList();

        results.Should().AllSatisfy(f => ReferenceEquals(f, results[0]).Should().BeTrue());
    }

    // Test 9: BuildCached — And() on a frozen builder returns a fork (IQueryable pattern)
    [Fact]
    public void BuildCached_AfterAndOperator_FrozenBuilderReturnsFork()
    {
        var builder = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0);

        builder.BuildCached();

        // And() on a frozen builder returns a new fork — does not throw
        var fork = builder.And().IsNotNullOrEmpty(e => e.Name);

        fork.IsValid(MakeEntity(value: 5, name: "x")).Should().BeTrue();
        fork.IsValid(MakeEntity(value: 5, name: "")).Should().BeFalse();

        // original frozen builder is unchanged
        builder.IsValid(MakeEntity(value: 5, name: "")).Should().BeTrue();
    }

    // Test 10: BuildCached — performance: 1000 evaluations use same cached func
    [Fact]
    public void BuildCached_Performance_1000Evaluations_UsesSameFunc()
    {
        var builder = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0)
            .And()
            .IsNotNullOrEmpty(e => e.Name);

        Func<CachedEntity, bool>? capturedRef = null;
        for (int i = 0; i < 1000; i++)
        {
            var func = builder.BuildCached();
            if (capturedRef == null)
                capturedRef = func;
            else
                ReferenceEquals(func, capturedRef).Should().BeTrue();

            func(MakeEntity(value: i + 1, name: "entity")).Should().BeTrue();
        }
    }

    // Test 11: BuildCached — result evaluates multiple conditions correctly
    [Fact]
    public void BuildCached_MultipleConditions_EvaluatesCorrectly()
    {
        var func = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0)
            .And()
            .IsNotNullOrEmpty(e => e.Name)
            .BuildCached();

        func(MakeEntity(value: 5, name: "Widget")).Should().BeTrue();
        func(MakeEntity(value: 5, name: "")).Should().BeFalse();
        func(MakeEntity(value: -1, name: "Widget")).Should().BeFalse();
    }

    // Test 12: BuildCached — result of Func<T,bool> can be used with List.Where
    [Fact]
    public void BuildCached_ResultCanBeUsedWithListWhere()
    {
        var func = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 5)
            .BuildCached();

        var items = new List<CachedEntity>
        {
            MakeEntity(value: 1),
            MakeEntity(value: 10),
            MakeEntity(value: 6)
        };

        items.Where(func).Should().HaveCount(2);
    }

    // Test 13: BuildCached — returns Func<T, bool> not Expression
    [Fact]
    public void BuildCached_ReturnsFuncNotExpression()
    {
        var result = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 0)
            .BuildCached();

        result.Should().BeAssignableTo<Func<CachedEntity, bool>>();
    }

    // Test 14: BuildCached — two different builders produce independent caches
    [Fact]
    public void BuildCached_TwoDifferentBuilders_ProduceIndependentCaches()
    {
        var builderA = new ValiFlow<CachedEntity>().GreaterThan(e => e.Value, 0);
        var builderB = new ValiFlow<CachedEntity>().LessThan(e => e.Value, 100);

        var funcA = builderA.BuildCached();
        var funcB = builderB.BuildCached();

        ReferenceEquals(funcA, funcB).Should().BeFalse();

        funcA(MakeEntity(value: 5)).Should().BeTrue();
        funcB(MakeEntity(value: 5)).Should().BeTrue();
        funcA(MakeEntity(value: -1)).Should().BeFalse();
        funcB(MakeEntity(value: 200)).Should().BeFalse();
    }

    // Test 15: BuildCached — Or condition evaluated correctly via cached func
    [Fact]
    public void BuildCached_OrCondition_EvaluatedCorrectly()
    {
        var func = new ValiFlow<CachedEntity>()
            .GreaterThan(e => e.Value, 100)
            .Or()
            .IsNotNullOrEmpty(e => e.Name)
            .BuildCached();

        func(MakeEntity(value: 200, name: "")).Should().BeTrue();   // value passes
        func(MakeEntity(value: 1, name: "Widget")).Should().BeTrue(); // name passes
        func(MakeEntity(value: 1, name: "")).Should().BeFalse();     // neither passes
    }

}

public class RegressionTests
{
    private static Product MakeProduct(
        string? name = "Test",
        decimal price = 10m,
        int quantity = 5,
        bool isActive = true)
        => new(name, price, quantity, isActive, DateTime.UtcNow, new List<string>());

    // Combine with empty left builder returns right expression
    [Fact]
    public void Combine_LeftEmpty_ReturnsRightExpression()
    {
        var left = new ValiFlow<Product>();
        var right = new ValiFlow<Product>().Add(p => p.IsActive);

        var combined = ValiFlow<Product>.Combine(left, right, and: true);
        var compiled = combined.Compile();

        compiled(MakeProduct(isActive: true)).Should().BeTrue();
        compiled(MakeProduct(isActive: false)).Should().BeFalse();
    }

    // Combine with empty right builder returns left expression
    [Fact]
    public void Combine_RightEmpty_ReturnsLeftExpression()
    {
        var left = new ValiFlow<Product>().Add(p => p.Quantity > 0);
        var right = new ValiFlow<Product>();

        var combined = ValiFlow<Product>.Combine(left, right, and: true);
        var compiled = combined.Compile();

        compiled(MakeProduct(quantity: 5)).Should().BeTrue();
        compiled(MakeProduct(quantity: 0)).Should().BeFalse();
    }

    // AddSubGroup with empty action throws ArgumentException with clear message
    [Fact]
    public void AddSubGroup_EmptyAction_ThrowsArgumentExceptionWithClearMessage()
    {
        var builder = new ValiFlow<Product>();

        Action act = () => builder.AddSubGroup(g => { });

        act.Should().Throw<ArgumentException>()
            .WithMessage("*sub-group*");
    }

    // ValidateNested with empty configure throws ArgumentException
    [Fact]
    public void ValidateNested_EmptyConfigure_ThrowsArgumentException()
    {
        var builder = new ValiFlow<Product>();

        Action act = () => builder.ValidateNested(p => p.Name, (ValiFlow<string> configure) => { });

        act.Should().Throw<ArgumentException>();
    }

    // Validate — no annotated conditions returns empty errors
    [Fact]
    public void Validate_NoAnnotatedConditions_ReturnsEmptyErrors()
    {
        var builder = new ValiFlow<Product>()
            .Add(p => p.IsActive)
            .Add(p => p.Quantity > 0);

        var result = builder.Validate(MakeProduct(isActive: false, quantity: -1));

        result.Errors.Should().BeEmpty();
        result.IsValid.Should().BeTrue();
    }

    // Validate — mixed annotated and unannotated: only annotated conditions reported
    [Fact]
    public void Validate_MixedAnnotatedAndUnannotated_OnlyAnnotatedReported()
    {
        var builder = new ValiFlow<Product>()
            .Add(p => p.IsActive)
            .Add(p => p.Quantity > 0).WithMessage("Quantity must be positive")
            .And()
            .Add(p => p.Price > 0m);

        var result = builder.Validate(MakeProduct(isActive: false, quantity: -1, price: 10m));

        result.Errors.Should().HaveCount(1);
        result.Errors[0].Message.Should().Be("Quantity must be positive");
    }

    // ValidateAll — freeze fix: builder is frozen before iteration; IsValid still works after
    [Fact]
    public void ValidateAll_FreezesBuildersBeforeIteration_BuilderIsFrozenAfterEnumeration()
    {
        // Arrange
        var validator = new ValiFlow<Product>()
            .NotNull(p => p.Name)
            .WithMessage("Name is required");

        var model = MakeProduct(name: null);

        // Act — enumerate results
        var results = validator.ValidateAll(new[] { model }).ToList();

        // Assert — after iterating, IsValid should still work (builder frozen, no mutation)
        validator.IsValid(MakeProduct(name: "valid")).Should().BeTrue();
        results.Should().HaveCount(1);
        results[0].Result.Errors.Should().HaveCount(1);
    }

    // Validate — TOCTOU fix: concurrent calls return correct results
    [Fact]
    public void Validate_ConcurrentCalls_AllReturnCorrectResults()
    {
        // Arrange
        var validator = new ValiFlow<Product>()
            .NotNull(p => p.Name)
            .WithMessage("Name is required");

        validator.Freeze();

        // Act — call Validate from multiple threads
        var errors = new System.Collections.Concurrent.ConcurrentBag<Vali_Flow.Core.Models.ValidationResult>();
        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
        {
            var result = validator.Validate(MakeProduct(name: null));
            errors.Add(result);
        }));
        Task.WaitAll(tasks.ToArray());

        // Assert — all calls should return one error
        errors.Should().AllSatisfy(r => r.Errors.Should().HaveCount(1));
    }

    // ValidateAll — empty sequence returns empty enumerable
    [Fact]
    public void ValidateAll_EmptySequence_ReturnsEmptyEnumerable()
    {
        var builder = new ValiFlow<Product>()
            .Add(p => p.IsActive).WithMessage("Must be active");

        var results = builder.ValidateAll(Enumerable.Empty<Product>()).ToList();

        results.Should().BeEmpty();
    }
}

public interface INamedEntity { string? Name { get; } }
public record ConcreteNamedEntity(string? Name) : INamedEntity;

public class ValiFlowGlobalTests
{
    private static Product MakeProduct(
        string? name = "Test",
        decimal price = 10m,
        int quantity = 5,
        bool isActive = true)
        => new(name, price, quantity, isActive, DateTime.UtcNow, new List<string>());

    [Fact]
    public void Register_GlobalFilter_IsAppliedViaBuildWithGlobal()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Product>(p => p.IsActive);

        var expr = new ValiFlow<Product>()
            .GreaterThan(p => p.Quantity, 0)
            .BuildWithGlobal();

        var compiled = expr.Compile();

        compiled(MakeProduct(quantity: 5, isActive: true)).Should().BeTrue();
        compiled(MakeProduct(quantity: 5, isActive: false)).Should().BeFalse();
        compiled(MakeProduct(quantity: 0, isActive: true)).Should().BeFalse();
    }

    [Fact]
    public void ClearAll_AfterRegister_GetFiltersReturnsEmpty()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Product>(p => p.IsActive);

        ValiFlowGlobal.ClearAll();

        ValiFlowGlobal.GetFilters<Product>().Should().BeEmpty();
    }

    [Fact]
    public void BuildWithGlobal_NoGlobals_BehavesIdenticalToBuild()
    {
        ValiFlowGlobal.ClearAll();

        var builder = new ValiFlow<Product>()
            .GreaterThan(p => p.Quantity, 0);

        var fromBuild           = builder.Build().Compile();
        var fromBuildWithGlobal = builder.BuildWithGlobal().Compile();

        fromBuild(MakeProduct(quantity: 5)).Should().Be(fromBuildWithGlobal(MakeProduct(quantity: 5)));
        fromBuild(MakeProduct(quantity: 0)).Should().Be(fromBuildWithGlobal(MakeProduct(quantity: 0)));
    }

    [Fact]
    public void BuildWithGlobal_MultipleGlobals_AllApplied()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Product>(p => p.IsActive);
        ValiFlowGlobal.Register<Product>(p => p.Price > 0m);

        var compiled = new ValiFlow<Product>()
            .GreaterThan(p => p.Quantity, 0)
            .BuildWithGlobal()
            .Compile();

        compiled(MakeProduct(quantity: 1, isActive: true, price: 5m)).Should().BeTrue();
        compiled(MakeProduct(quantity: 1, isActive: false, price: 5m)).Should().BeFalse();
        compiled(MakeProduct(quantity: 1, isActive: true, price: 0m)).Should().BeFalse();
        compiled(MakeProduct(quantity: 0, isActive: true, price: 5m)).Should().BeFalse();
    }

    [Fact]
    public void HasFilters_InterfaceRegistered_ReturnsTrue()
    {
        // Arrange
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<INamedEntity>(e => e.Name != null);

        // Act — ask for a concrete type that implements the interface
        var result = ValiFlowGlobal.HasFilters<ConcreteNamedEntity>();

        // Cleanup
        ValiFlowGlobal.ClearAll();

        // Assert
        result.Should().BeTrue();
    }
}

public class CloneTests
{
    private record CloneEntity(string? Name, int Age, bool IsActive);
    private static CloneEntity Make(string? name = "Alice", int age = 25, bool active = true)
        => new(name, age, active);

    // Clone — base conditions copied; original unchanged after clone mutates
    [Fact]
    public void Clone_BaseConditionsCopied_OriginalUnchanged()
    {
        var baseBuilder = new ValiFlow<CloneEntity>()
            .IsNotNullOrEmpty(x => x.Name)
            .GreaterThan(x => x.Age, 0);

        var extended = baseBuilder.Clone()
            .IsTrue(x => x.IsActive);

        // Extended has all three conditions
        extended.IsValid(Make(active: true)).Should().BeTrue();
        extended.IsValid(Make(active: false)).Should().BeFalse();

        // Base still has only two conditions — IsActive irrelevant
        baseBuilder.IsValid(Make(active: false)).Should().BeTrue();
    }

    // Clone — multiple independent derivations from same base (IQueryable pattern)
    [Fact]
    public void Clone_MultipleDerivations_AreIndependent()
    {
        var base_ = new ValiFlow<CloneEntity>()
            .IsNotNullOrEmpty(x => x.Name);

        var strictAge  = base_.Clone().GreaterThan(x => x.Age, 18);
        var activeOnly = base_.Clone().IsTrue(x => x.IsActive);

        var adult    = Make(age: 25, active: false);
        var inactive = Make(age: 10, active: true);

        strictAge.IsValid(adult).Should().BeTrue();    // name ok, age ok
        strictAge.IsValid(inactive).Should().BeFalse(); // age fails

        activeOnly.IsValid(adult).Should().BeFalse();   // active fails
        activeOnly.IsValid(inactive).Should().BeTrue(); // name ok, active ok
    }

    // Clone of frozen builder — clone is unfrozen and can be extended
    [Fact]
    public void Clone_OfFrozenBuilder_StartsUnfrozen()
    {
        var frozen = new ValiFlow<CloneEntity>()
            .IsNotNullOrEmpty(x => x.Name);
        frozen.Freeze();

        // Explicit Clone() returns an unfrozen builder — can add new conditions freely
        var extended = frozen.Clone().GreaterThan(x => x.Age, 0);

        extended.IsValid(Make(age: 5)).Should().BeTrue();
        extended.IsValid(Make(age: -1)).Should().BeFalse();

        // Original frozen builder still has only the Name condition
        frozen.IsValid(Make(age: -1)).Should().BeTrue();
    }

    // Clone preserves pending OR state (_nextIsAnd == 0)
    [Fact]
    public void Clone_AfterOr_PendingOrStateIsTransferred()
    {
        var builder = new ValiFlow<CloneEntity>()
            .GreaterThan(x => x.Age, 0)
            .Or(); // _nextIsAnd = 0

        var clone = builder.Clone();
        clone.GreaterThan(x => x.Age, 18); // should be OR-joined

        // Second condition is OR-joined: Age > 0 OR Age > 18
        // entity with Age=5 satisfies first part → overall true
        clone.IsValid(Make(age: 5)).Should().BeTrue();
        // entity with Age=-1 fails both → false
        clone.IsValid(Make(age: -1)).Should().BeFalse();
    }

    // Clone of frozen source — _frozen must be 0 on the clone
    [Fact]
    public void Clone_IsFrozenSource_CloneStartsUnfrozen()
    {
        var frozen = new ValiFlow<CloneEntity>()
            .IsNotNullOrEmpty(x => x.Name);
        frozen.Freeze();

        var clone = frozen.Clone();
        // Clone is unfrozen — mutations return same builder, not a fork
        var extended = clone.GreaterThan(x => x.Age, 18);
        extended.Should().BeSameAs(clone);
    }

    // Implicit fork — mutation on frozen builder without explicit Clone()
    [Fact]
    public void Clone_ImplicitFork_FrozenBuilderMutationReturnsDerivedBuilder()
    {
        var base_ = new ValiFlow<CloneEntity>()
            .IsNotNullOrEmpty(x => x.Name);
        base_.Freeze();

        // Calling mutation methods on a frozen builder silently returns a fork (IQueryable pattern)
        var fork1 = base_.GreaterThan(x => x.Age, 18);
        var fork2 = base_.IsTrue(x => x.IsActive);

        // base_ is unchanged
        base_.IsValid(Make(age: 5, active: false)).Should().BeTrue();

        // each fork has the extra condition
        fork1.IsValid(Make(age: 25)).Should().BeTrue();
        fork1.IsValid(Make(age: 10)).Should().BeFalse();

        fork2.IsValid(Make(active: true)).Should().BeTrue();
        fork2.IsValid(Make(active: false)).Should().BeFalse();
    }
}

public class ConcurrencyTests
{
    private record ConcEntity(string? Name, int Age, bool IsActive);
    private static ConcEntity Make(int age = 25, bool active = true, string name = "Alice")
        => new(name, age, active);

    // Verifies that calling IsValid concurrently on a frozen builder never corrupts the result.
    [Fact]
    public void FrozenBuilder_ConcurrentIsValid_AllReturnCorrectResult()
    {
        var builder = new ValiFlow<ConcEntity>()
            .GreaterThan(x => x.Age, 18)
            .IsTrue(x => x.IsActive);
        builder.BuildCached(); // freeze

        const int threadCount = 20;
        var results = new bool[threadCount];
        var threads = Enumerable.Range(0, threadCount).Select(i => new System.Threading.Thread(() =>
        {
            results[i] = builder.IsValid(Make(age: 25, active: true));
        })).ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        results.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    // Verifies that concurrent forks from a frozen builder are independent and do not share mutation state.
    [Fact]
    public void FrozenBuilder_ConcurrentFork_AllForksAreIndependent()
    {
        var frozen = new ValiFlow<ConcEntity>()
            .GreaterThan(x => x.Age, 18);
        frozen.BuildCached(); // freeze

        const int forkCount = 20;
        var forks = new ValiFlow<ConcEntity>[forkCount];
        var threads = Enumerable.Range(0, forkCount).Select(i => new System.Threading.Thread(() =>
        {
            // Each fork adds a different condition — they must not interfere.
            forks[i] = frozen.IsTrue(x => x.IsActive);
        })).ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        // The original frozen builder has only the age condition.
        frozen.IsValid(Make(age: 25, active: false)).Should().BeTrue();

        // Every fork has both conditions.
        foreach (var fork in forks)
        {
            fork.IsValid(Make(age: 25, active: true)).Should().BeTrue();
            fork.IsValid(Make(age: 25, active: false)).Should().BeFalse();
        }
    }

    // Verifies that calling Validate() concurrently on a frozen builder does not throw.
    [Fact]
    public void FrozenBuilder_ConcurrentValidate_NoExceptions()
    {
        var builder = new ValiFlow<ConcEntity>()
            .GreaterThan(x => x.Age, 18).WithMessage("Age must be > 18")
            .IsTrue(x => x.IsActive).WithMessage("Must be active");
        builder.BuildCached(); // freeze

        const int threadCount = 20;
        var exceptions = new System.Exception?[threadCount];
        var threads = Enumerable.Range(0, threadCount).Select(i => new System.Threading.Thread(() =>
        {
            try
            {
                var result = builder.Validate(Make(age: 10, active: false));
                result.Errors.Should().HaveCount(2);
            }
            catch (System.Exception ex)
            {
                exceptions[i] = ex;
            }
        })).ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        exceptions.Should().AllSatisfy(e => e.Should().BeNull());
    }

    // Verifies that two threads simultaneously forking a frozen builder each get independent, correct results.
    [Fact]
    public void FrozenBuilder_SimultaneousFork_BothForksAreIndependentAndCorrect()
    {
        var frozen = new ValiFlow<ConcEntity>()
            .GreaterThan(x => x.Age, 0);
        frozen.BuildCached(); // freeze

        ValiFlow<ConcEntity>? fork1 = null;
        ValiFlow<ConcEntity>? fork2 = null;

        var t1 = new System.Threading.Thread(() =>
            fork1 = frozen.GreaterThan(x => x.Age, 18));
        var t2 = new System.Threading.Thread(() =>
            fork2 = frozen.IsTrue(x => x.IsActive));

        t1.Start(); t2.Start();
        t1.Join(); t2.Join();

        // Original frozen builder unchanged
        frozen.IsValid(Make(age: 1, active: false)).Should().BeTrue();

        // fork1: age > 0 AND age > 18
        fork1!.IsValid(Make(age: 25, active: false)).Should().BeTrue();
        fork1!.IsValid(Make(age: 10, active: true)).Should().BeFalse();

        // fork2: age > 0 AND isActive == true
        fork2!.IsValid(Make(age: 1, active: true)).Should().BeTrue();
        fork2!.IsValid(Make(age: 1, active: false)).Should().BeFalse();
    }
}

public class ExplainTests
{
    private record ExplainEntity(string? Name, int Age, bool IsActive);

    [Fact]
    public void Explain_EmptyBuilder_ReturnsNoConditionsMessage()
    {
        var builder = new ValiFlow<ExplainEntity>();
        builder.Explain().Should().Be("(no conditions)");
    }

    [Fact]
    public void Explain_SingleCondition_ReturnsNonEmptyString()
    {
        var builder = new ValiFlow<ExplainEntity>().GreaterThan(x => x.Age, 18);
        var result = builder.Explain();
        result.Should().NotBeNullOrEmpty();
        result.Should().NotBe("(no conditions)");
    }

    [Fact]
    public void Explain_MultipleConditions_ContainsAndOrKeywords()
    {
        var builder = new ValiFlow<ExplainEntity>()
            .GreaterThan(x => x.Age, 18)
            .Or()
            .IsTrue(x => x.IsActive);
        var result = builder.Explain();
        result.Should().NotBeNullOrEmpty();
    }
}

public class ValidateVsIsValidTests
{
    private record VVEntity(int Age, bool IsActive);

    // Validate() evaluates each annotated condition independently — Or-grouping semantics are NOT respected.
    // A condition in an Or-group may be reported as failed even when IsValid() returns true.
    [Fact]
    public void Validate_OrGroupedAnnotatedCondition_ReportsErrorEvenWhenIsValidIsTrue()
    {
        var builder = new ValiFlow<VVEntity>()
            .GreaterThan(x => x.Age, 100).WithMessage("Age > 100")
            .Or()
            .IsTrue(x => x.IsActive); // This passes → full OR expression is true

        var entity = new VVEntity(Age: 5, IsActive: true);

        // IsValid evaluates the full boolean expression (OR logic) → true
        builder.IsValid(entity).Should().BeTrue();

        // Validate evaluates each annotated condition independently → the first condition fails
        var result = builder.Validate(entity);
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e => e.Message == "Age > 100");
    }
}

public class WithMessageFactoryTests
{
    private static Product MakeProduct(string? name = "Test", decimal price = 10m, int quantity = 5)
        => new(name, price, quantity, true, DateTime.UtcNow, new List<string>());

    // WithMessage(Func<string>) — factory evaluated at validate time
    [Fact]
    public void WithMessage_Factory_IsEvaluatedAtValidateTime()
    {
        var culture = "en";
        var messages = new Dictionary<string, string>
        {
            ["en"] = "Name is required",
            ["es"] = "El nombre es requerido"
        };

        var validator = new ValiFlow<Product>()
            .NotNull(m => m.Name)
            .WithError("ERR001", "fallback", "Name")
            .WithMessage(() => messages[culture]);

        // English
        culture = "en";
        var result = validator.Validate(MakeProduct(name: null));
        result.Errors.Should().ContainSingle(e => e.Message == "Name is required");

        // Spanish — same validator, different culture
        culture = "es";
        result = validator.Validate(MakeProduct(name: null));
        result.Errors.Should().ContainSingle(e => e.Message == "El nombre es requerido");
    }

    [Fact]
    public void WithMessage_Factory_NullFactory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ValiFlow<Product>()
                .NotNull(m => m.Name)
                .WithMessage((Func<string>)null!));
    }

    [Fact]
    public void WithMessage_Factory_ValidCondition_ReturnsTrue_NoFactoryInvoked()
    {
        var invoked = false;
        var validator = new ValiFlow<Product>()
            .NotNull(m => m.Name)
            .WithError("ERR001", "fallback", "Name")
            .WithMessage(() => { invoked = true; return "Name is required"; });

        // Valid entity — condition passes, Validate has no errors so factory should NOT be invoked
        // (the condition passes, so no ValidationError is created)
        var result = validator.Validate(MakeProduct(name: "valid"));
        result.Errors.Should().BeEmpty();
        invoked.Should().BeFalse();
    }

    [Fact]
    public void WithMessage_Factory_OverridesStaticMessage()
    {
        var validator = new ValiFlow<Product>()
            .NotNull(m => m.Name)
            .WithMessage("static message")
            .WithMessage(() => "factory message");

        var result = validator.Validate(MakeProduct(name: null));
        // WithMessage(Func) was called last — factory message wins
        result.Errors.Should().ContainSingle(e => e.Message == "factory message");
    }
}
