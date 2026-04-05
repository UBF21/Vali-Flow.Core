using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

public class CollectionExpressionTests
{
    private static Product MakeProduct(List<string>? tags = null) =>
        new("Product", 10m, 1, true, DateTime.UtcNow, tags ?? new List<string>());

    // 1. NotEmpty(Tags) — matches non-empty, rejects empty
    [Fact]
    public void NotEmpty_Tags_MatchesNonEmpty_RejectsEmpty()
    {
        var filter = new ValiFlow<Product>()
            .NotEmpty<string>(p => p.Tags)
            .Build();

        filter.Compile()(MakeProduct(new List<string> { "tag1" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string>())).Should().BeFalse();
    }

    // 2. Empty(Tags) — matches empty, rejects non-empty
    [Fact]
    public void Empty_Tags_MatchesEmpty_RejectsNonEmpty()
    {
        var filter = new ValiFlow<Product>()
            .Empty<string>(p => p.Tags)
            .Build();

        filter.Compile()(MakeProduct(new List<string>())).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "tag1" })).Should().BeFalse();
    }

    // 3. Count(Tags, 2) — matches list of exactly 2 items
    [Fact]
    public void Count_Tags_2_MatchesExactly2Items()
    {
        var filter = new ValiFlow<Product>()
            .Count<string>(p => p.Tags, 2)
            .Build();

        filter.Compile()(MakeProduct(new List<string> { "a", "b" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "a" })).Should().BeFalse();
        filter.Compile()(MakeProduct(new List<string> { "a", "b", "c" })).Should().BeFalse();
    }

    // 4. CountBetween(Tags, 1, 3) — matches 1-3 items
    [Fact]
    public void CountBetween_Tags_1_3_Matches1To3Items()
    {
        var filter = new ValiFlow<Product>()
            .CountBetween<string>(p => p.Tags, 1, 3)
            .Build();

        filter.Compile()(MakeProduct(new List<string> { "a" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "a", "b" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "a", "b", "c" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string>())).Should().BeFalse();
        filter.Compile()(MakeProduct(new List<string> { "a", "b", "c", "d" })).Should().BeFalse();
    }

    // 5. MinCount(Tags, 2) — matches >= 2 items
    [Fact]
    public void MinCount_Tags_2_MatchesGte2Items()
    {
        var filter = new ValiFlow<Product>()
            .MinCount<string>(p => p.Tags, 2)
            .Build();

        filter.Compile()(MakeProduct(new List<string> { "a", "b" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "a", "b", "c" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "a" })).Should().BeFalse();
        filter.Compile()(MakeProduct(new List<string>())).Should().BeFalse();
    }

    // 6. MaxCount(Tags, 3) — matches <= 3 items
    [Fact]
    public void MaxCount_Tags_3_MatchesLte3Items()
    {
        var filter = new ValiFlow<Product>()
            .MaxCount<string>(p => p.Tags, 3)
            .Build();

        filter.Compile()(MakeProduct(new List<string> { "a" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "a", "b", "c" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string>())).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "a", "b", "c", "d" })).Should().BeFalse();
    }

    // 7. Contains(Tags, "csharp") — matches if contains value
    [Fact]
    public void Contains_Tags_CSharp_MatchesIfContainsValue()
    {
        var filter = new ValiFlow<Product>()
            .Contains<string>(p => p.Tags, "csharp")
            .Build();

        filter.Compile()(MakeProduct(new List<string> { "csharp", "dotnet" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "dotnet" })).Should().BeFalse();
        filter.Compile()(MakeProduct(new List<string>())).Should().BeFalse();
    }

    // 8. Any(Tags, t => t.StartsWith("c")) — matches if any starts with "c"
    [Fact]
    public void Any_Tags_StartsWithC_MatchesIfAnyStartsWithC()
    {
        var filter = new ValiFlow<Product>()
            .Any<string>(p => p.Tags, t => t.StartsWith("c"))
            .Build();

        filter.Compile()(MakeProduct(new List<string> { "csharp", "dotnet" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "dotnet", "java" })).Should().BeFalse();
        filter.Compile()(MakeProduct(new List<string>())).Should().BeFalse();
    }

    // 9. All(Tags, t => t.Length > 0) — matches if all non-empty
    [Fact]
    public void All_Tags_LengthGt0_MatchesIfAllNonEmpty()
    {
        var filter = new ValiFlow<Product>()
            .All<string>(p => p.Tags, t => t.Length > 0)
            .Build();

        filter.Compile()(MakeProduct(new List<string> { "a", "bb" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "a", "" })).Should().BeFalse();
    }

    // 10. None(Tags, t => t == "banned") — matches if none equals "banned"
    [Fact]
    public void None_Tags_EqualsBanned_MatchesIfNoneEqualsBanned()
    {
        var filter = new ValiFlow<Product>()
            .None<string>(p => p.Tags, t => t == "banned")
            .Build();

        filter.Compile()(MakeProduct(new List<string> { "csharp", "dotnet" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "csharp", "banned" })).Should().BeFalse();
    }

    // 11. HasDuplicates(Tags) — matches if duplicates exist
    [Fact]
    public void HasDuplicates_Tags_MatchesIfDuplicatesExist()
    {
        var filter = new ValiFlow<Product>()
            .HasDuplicates<string>(p => p.Tags)
            .Build();

        filter.Compile()(MakeProduct(new List<string> { "a", "a", "b" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "a", "b", "c" })).Should().BeFalse();
    }

    // 12. DistinctCount(Tags, 2) — matches if 2 distinct elements
    [Fact]
    public void DistinctCount_Tags_2_MatchesIf2DistinctElements()
    {
        var filter = new ValiFlow<Product>()
            .DistinctCount<string>(p => p.Tags, 2)
            .Build();

        filter.Compile()(MakeProduct(new List<string> { "a", "b" })).Should().BeTrue();
        // duplicates with 2 distinct
        filter.Compile()(MakeProduct(new List<string> { "a", "b", "a" })).Should().BeTrue();
        filter.Compile()(MakeProduct(new List<string> { "a" })).Should().BeFalse();
        filter.Compile()(MakeProduct(new List<string> { "a", "b", "c" })).Should().BeFalse();
    }

    // 13. In — empty list throws ArgumentException
    [Fact]
    public void In_EmptyList_ThrowsArgumentException()
    {
        var act = () => new ValiFlow<Product>().In(p => p.Quantity, Array.Empty<int>());
        act.Should().Throw<ArgumentException>()
           .WithMessage("*empty*")
           .WithParameterName("values");
    }

    // 14. NotIn — empty list throws ArgumentException
    [Fact]
    public void NotIn_EmptyList_ThrowsArgumentException()
    {
        var act = () => new ValiFlow<Product>().NotIn(p => p.Quantity, Array.Empty<int>());
        act.Should().Throw<ArgumentException>()
           .WithMessage("*empty*")
           .WithParameterName("values");
    }
}

public class CollectionExpressionNegativeTests
{
    private record NullableTagsEntity(List<string?>? Items);

    private static Product MakeProduct(List<string>? tags = null) =>
        new("Product", 10m, 1, true, DateTime.UtcNow, tags ?? new List<string>());

    [Fact]
    public void All_NullPredicate_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<Product>().All<string>(p => p.Tags, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Any_NullPredicate_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<Product>().Any<string>(p => p.Tags, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void None_NullPredicate_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<Product>().None<string>(p => p.Tags, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EachItem_NullConfigure_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<Product>().EachItem<string>(p => p.Tags, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AnyItem_NullConfigure_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<Product>().AnyItem<string>(p => p.Tags, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HasDuplicates_EmptyCollection_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().HasDuplicates<string>(p => p.Tags).Build();
        filter.Compile()(MakeProduct(new List<string>())).Should().BeFalse();
    }

    [Fact]
    public void HasDuplicates_AllSameValues_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().HasDuplicates<string>(p => p.Tags).Build();
        filter.Compile()(MakeProduct(new List<string> { "x", "x", "x" })).Should().BeTrue();
    }

    [Fact]
    public void DistinctCount_NullCollection_ReturnsFalse()
    {
        var filter = new ValiFlow<NullableTagsEntity>().DistinctCount<string>(e => e.Items, 2).Build();
        filter.Compile()(new NullableTagsEntity(null)).Should().BeFalse();
    }

    [Fact]
    public void DistinctCount_ExactCount_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().DistinctCount<string>(p => p.Tags, 3).Build();
        filter.Compile()(MakeProduct(new List<string> { "a", "b", "c" })).Should().BeTrue();
    }

    [Fact]
    public void DistinctCount_WrongCount_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().DistinctCount<string>(p => p.Tags, 3).Build();
        filter.Compile()(MakeProduct(new List<string> { "a", "b" })).Should().BeFalse();
    }

    [Fact]
    public void CountBetween_BelowMin_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().CountBetween<string>(p => p.Tags, 2, 5).Build();
        filter.Compile()(MakeProduct(new List<string> { "a" })).Should().BeFalse();
    }

    [Fact]
    public void CountBetween_AboveMax_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().CountBetween<string>(p => p.Tags, 2, 4).Build();
        filter.Compile()(MakeProduct(new List<string> { "a", "b", "c", "d", "e" })).Should().BeFalse();
    }

    [Fact]
    public void CountBetween_ExactMin_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().CountBetween<string>(p => p.Tags, 2, 5).Build();
        filter.Compile()(MakeProduct(new List<string> { "a", "b" })).Should().BeTrue();
    }

    [Fact]
    public void CountBetween_ExactMax_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().CountBetween<string>(p => p.Tags, 2, 4).Build();
        filter.Compile()(MakeProduct(new List<string> { "a", "b", "c", "d" })).Should().BeTrue();
    }

    private record NullableTagsProduct(List<string>? Tags);

    [Fact]
    public void None_NullCollection_ReturnsTrue_VacuouslyTrue()
    {
        // Arrange — null collection means no elements satisfy the predicate → vacuously true
        var filter = new ValiFlow<NullableTagsProduct>()
            .None<string>(p => p.Tags, t => t == "forbidden")
            .Build().Compile();

        var model = new NullableTagsProduct(Tags: null);

        // Act & Assert
        filter(model).Should().BeTrue();
    }
}

public class AllMatchAndCountEqualsTests
{
    private record TagEntity(List<string>? Tags);

    private static TagEntity MakeEntity(List<string>? tags) => new(tags);

    // AllMatch — all items pass filter (length >= 3) → true
    [Fact]
    public void AllMatch_AllItemsPassFilter_ReturnsTrue()
    {
        var itemFilter = new ValiFlow<string>().MinLength(x => x, 3);
        var filter = new ValiFlow<TagEntity>()
            .AllMatch<string>(e => e.Tags!, itemFilter)
            .Build().Compile();

        filter(MakeEntity(new List<string> { "abc", "defg" })).Should().BeTrue();
    }

    // AllMatch — one item fails filter (length < 3) → false
    [Fact]
    public void AllMatch_OneItemFailsFilter_ReturnsFalse()
    {
        var itemFilter = new ValiFlow<string>().MinLength(x => x, 3);
        var filter = new ValiFlow<TagEntity>()
            .AllMatch<string>(e => e.Tags!, itemFilter)
            .Build().Compile();

        filter(MakeEntity(new List<string> { "abc", "de" })).Should().BeFalse();
    }

    // AllMatch — empty collection → vacuously true
    [Fact]
    public void AllMatch_EmptyCollection_ReturnsTrue()
    {
        var itemFilter = new ValiFlow<string>().MinLength(x => x, 3);
        var filter = new ValiFlow<TagEntity>()
            .AllMatch<string>(e => e.Tags!, itemFilter)
            .Build().Compile();

        filter(MakeEntity(new List<string>())).Should().BeTrue();
    }

    // AllMatch — null collection → false
    [Fact]
    public void AllMatch_NullCollection_ReturnsFalse()
    {
        var itemFilter = new ValiFlow<string>().MinLength(x => x, 3);
        var filter = new ValiFlow<TagEntity>()
            .AllMatch<string>(e => e.Tags!, itemFilter)
            .Build().Compile();

        filter(MakeEntity(null)).Should().BeFalse();
    }

    // AllMatch — null filter → throws ArgumentNullException
    [Fact]
    public void AllMatch_NullFilter_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<TagEntity>().AllMatch<string>(e => e.Tags!, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("filter");
    }

    // AllMatch — null selector → throws ArgumentNullException
    [Fact]
    public void AllMatch_NullSelector_ThrowsArgumentNullException()
    {
        var itemFilter = new ValiFlow<string>().MinLength(x => x, 3);
        Action act = () => new ValiFlow<TagEntity>().AllMatch<string>(null!, itemFilter);
        act.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }

    // CountEquals — exact count matches → true
    [Fact]
    public void CountEquals_ExactCount_ReturnsTrue()
    {
        var filter = new ValiFlow<TagEntity>()
            .CountEquals<string>(e => e.Tags, 2)
            .Build().Compile();

        filter(MakeEntity(new List<string> { "a", "b" })).Should().BeTrue();
    }

    // CountEquals — wrong count → false
    [Fact]
    public void CountEquals_WrongCount_ReturnsFalse()
    {
        var filter = new ValiFlow<TagEntity>()
            .CountEquals<string>(e => e.Tags, 3)
            .Build().Compile();

        filter(MakeEntity(new List<string> { "a", "b" })).Should().BeFalse();
    }

    // CountEquals — null collection → false
    [Fact]
    public void CountEquals_NullCollection_ReturnsFalse()
    {
        var filter = new ValiFlow<TagEntity>()
            .CountEquals<string>(e => e.Tags, 0)
            .Build().Compile();

        filter(MakeEntity(null)).Should().BeFalse();
    }

    // CountEquals — null selector → throws ArgumentNullException
    [Fact]
    public void CountEquals_NullSelector_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<TagEntity>().CountEquals<string>(null!, 2);
        act.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }
}
