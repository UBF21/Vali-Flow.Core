using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;
using Vali_Flow.Core.Interfaces.Types;

namespace Vali_Flow.Core.Tests;

public class ComparisonExpressionTests
{
    private static Product MakeProduct(
        string? name = "Test",
        int quantity = 5,
        bool isActive = true) =>
        new(name, 10m, quantity, isActive, DateTime.UtcNow, new List<string>());

    // 1. NotNull(Name) — matches non-null, rejects null
    [Fact]
    public void NotNull_Name_MatchesNonNull_RejectsNull()
    {
        var filter = new ValiFlow<Product>()
            .NotNull(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct(name: "Hello")).Should().BeTrue();
        filter.Compile()(MakeProduct(name: null)).Should().BeFalse();
    }

    // 2. Null(Name) — matches null, rejects non-null
    [Fact]
    public void Null_Name_MatchesNull_RejectsNonNull()
    {
        var filter = new ValiFlow<Product>()
            .Null(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct(name: null)).Should().BeTrue();
        filter.Compile()(MakeProduct(name: "Hello")).Should().BeFalse();
    }

    // 3. EqualTo(Quantity, 5) — matches exact value
    [Fact]
    public void EqualTo_Quantity_5_MatchesExactValue()
    {
        var filter = new ValiFlow<Product>()
            .EqualTo(p => p.Quantity, 5)
            .Build();

        filter.Compile()(MakeProduct(quantity: 5)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 4)).Should().BeFalse();
        filter.Compile()(MakeProduct(quantity: 6)).Should().BeFalse();
    }

    // 4. NotEqualTo(Quantity, 5) — rejects exact value
    [Fact]
    public void NotEqualTo_Quantity_5_RejectsExactValue()
    {
        var filter = new ValiFlow<Product>()
            .NotEqualTo(p => p.Quantity, 5)
            .Build();

        filter.Compile()(MakeProduct(quantity: 4)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 6)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 5)).Should().BeFalse();
    }

    // 5. IsTrue(IsActive) — matches true
    [Fact]
    public void IsTrue_IsActive_MatchesTrue()
    {
        var filter = new ValiFlow<Product>()
            .IsTrue(p => p.IsActive)
            .Build();

        filter.Compile()(MakeProduct(isActive: true)).Should().BeTrue();
        filter.Compile()(MakeProduct(isActive: false)).Should().BeFalse();
    }

    // 6. IsFalse(IsActive) — matches false
    [Fact]
    public void IsFalse_IsActive_MatchesFalse()
    {
        var filter = new ValiFlow<Product>()
            .IsFalse(p => p.IsActive)
            .Build();

        filter.Compile()(MakeProduct(isActive: false)).Should().BeTrue();
        filter.Compile()(MakeProduct(isActive: true)).Should().BeFalse();
    }
}

public class ComparisonExpressionNegativeTests
{
    [Fact]
    public void NotNull_NullSelector_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<Product>().NotNull<string>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Null_NullSelector_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<Product>().Null<string>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EqualTo_NullSelector_ThrowsArgumentNullException_BeforeValueCheck()
    {
        // Act & Assert — selector null should throw even when value is also null
        Action act = () => new ValiFlow<Product>().EqualTo<string>(null!, null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

public class IsInEnumIsDefaultTests
{
    private enum OrderStatus { Pending = 0, Active = 1, Closed = 2 }
    private enum BadStatus { } // empty enum for negative test

    private record EnumEntity(OrderStatus Status, int Score, DateTime CreatedAt, string? Name);

    private static EnumEntity Make(OrderStatus status = OrderStatus.Active, int score = 5,
        DateTime createdAt = default, string? name = "test")
        => new(status, score, createdAt == default ? DateTime.UtcNow : createdAt, name);

    // IsInEnum — defined value passes
    [Fact]
    public void IsInEnum_DefinedValue_ReturnsTrue()
    {
        var v = new ValiFlow<EnumEntity>().IsInEnum(e => e.Status);
        v.IsValid(Make(OrderStatus.Active)).Should().BeTrue();
        v.IsValid(Make(OrderStatus.Pending)).Should().BeTrue();
        v.IsValid(Make(OrderStatus.Closed)).Should().BeTrue();
    }

    // IsInEnum — undefined cast value fails
    [Fact]
    public void IsInEnum_UndefinedCastValue_ReturnsFalse()
    {
        var v = new ValiFlow<EnumEntity>().IsInEnum(e => e.Status);
        v.IsValid(Make((OrderStatus)99)).Should().BeFalse();
    }

    // IsInEnum — null selector throws
    [Fact]
    public void IsInEnum_NullSelector_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ValiFlow<EnumEntity>().IsInEnum<OrderStatus>(null!));
    }

    // IsDefault — int default (0) passes
    [Fact]
    public void IsDefault_IntZero_ReturnsTrue()
    {
        var v = new ValiFlow<EnumEntity>().IsDefault(e => e.Score);
        v.IsValid(Make(score: 0)).Should().BeTrue();
        v.IsValid(Make(score: 1)).Should().BeFalse();
    }

    // IsDefault — string null passes
    [Fact]
    public void IsDefault_NullString_ReturnsTrue()
    {
        var v = new ValiFlow<EnumEntity>().IsDefault(e => e.Name);
        v.IsValid(Make(name: null)).Should().BeTrue();
        v.IsValid(Make(name: "hello")).Should().BeFalse();
    }

    // IsDefault — DateTime default passes
    [Fact]
    public void IsDefault_DefaultDateTime_ReturnsTrue()
    {
        var v = new ValiFlow<EnumEntity>().IsDefault(e => e.CreatedAt);
        // Construct directly to avoid Make() substituting default with UtcNow
        var withDefault = new EnumEntity(OrderStatus.Active, 5, default(DateTime), "test");
        var withNow = new EnumEntity(OrderStatus.Active, 5, DateTime.UtcNow, "test");
        v.IsValid(withDefault).Should().BeTrue();
        v.IsValid(withNow).Should().BeFalse();
    }

    // IsDefault — null selector throws
    [Fact]
    public void IsDefault_NullSelector_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ValiFlow<EnumEntity>().IsDefault<int>(null!));
    }

    // IsNotDefault — non-zero passes
    [Fact]
    public void IsNotDefault_NonZeroInt_ReturnsTrue()
    {
        var v = new ValiFlow<EnumEntity>().IsNotDefault(e => e.Score);
        v.IsValid(Make(score: 5)).Should().BeTrue();
        v.IsValid(Make(score: 0)).Should().BeFalse();
    }

    // IsNotDefault — non-null string passes
    [Fact]
    public void IsNotDefault_NonNullString_ReturnsTrue()
    {
        var v = new ValiFlow<EnumEntity>().IsNotDefault(e => e.Name);
        v.IsValid(Make(name: "hello")).Should().BeTrue();
        v.IsValid(Make(name: null)).Should().BeFalse();
    }

    // IsNotDefault — null selector throws
    [Fact]
    public void IsNotDefault_NullSelector_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ValiFlow<EnumEntity>().IsNotDefault<int>(null!));
    }
}

public class CrossFieldAndNullAliasTests
{
    private record CrossEntity(int Value, int Min, int Max, DateTime Start, DateTime End, decimal Price, decimal Cap, string? Name);

    private static IComparableExpression<ValiFlow<CrossEntity>, CrossEntity> AsComparable()
        => new ValiFlow<CrossEntity>();

    // Cross-field: GreaterThan
    [Fact]
    public void GreaterThan_CrossField_Int_ValueGtMin_ReturnsTrue()
    {
        var f = AsComparable().GreaterThan(x => x.Value, x => x.Min);
        f.IsValid(new CrossEntity(10, 5, 20, default, default, 0, 0, null)).Should().BeTrue();
    }

    [Fact]
    public void GreaterThan_CrossField_Int_ValueEqMin_ReturnsFalse()
    {
        var f = AsComparable().GreaterThan(x => x.Value, x => x.Min);
        f.IsValid(new CrossEntity(5, 5, 20, default, default, 0, 0, null)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_CrossField_DateTime_EndAfterStart_ReturnsTrue()
    {
        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2025, 6, 1);
        var f = AsComparable().GreaterThan(x => x.End, x => x.Start);
        f.IsValid(new CrossEntity(0, 0, 0, start, end, 0, 0, null)).Should().BeTrue();
    }

    [Fact]
    public void GreaterThan_CrossField_DateTime_EndEqualsStart_ReturnsFalse()
    {
        var dt = new DateTime(2025, 1, 1);
        var f = AsComparable().GreaterThan(x => x.End, x => x.Start);
        f.IsValid(new CrossEntity(0, 0, 0, dt, dt, 0, 0, null)).Should().BeFalse();
    }

    // Cross-field: GreaterThanOrEqualTo
    [Fact]
    public void GreaterThanOrEqualTo_CrossField_Int_Equal_ReturnsTrue()
    {
        var f = AsComparable().GreaterThanOrEqualTo(x => x.Value, x => x.Min);
        f.IsValid(new CrossEntity(5, 5, 20, default, default, 0, 0, null)).Should().BeTrue();
    }

    [Fact]
    public void GreaterThanOrEqualTo_CrossField_Int_Less_ReturnsFalse()
    {
        var f = AsComparable().GreaterThanOrEqualTo(x => x.Value, x => x.Min);
        f.IsValid(new CrossEntity(4, 5, 20, default, default, 0, 0, null)).Should().BeFalse();
    }

    // Cross-field: LessThan
    [Fact]
    public void LessThan_CrossField_Decimal_PriceBelowCap_ReturnsTrue()
    {
        var f = AsComparable().LessThan(x => x.Price, x => x.Cap);
        f.IsValid(new CrossEntity(0, 0, 0, default, default, 9m, 10m, null)).Should().BeTrue();
    }

    [Fact]
    public void LessThan_CrossField_Decimal_PriceEqCap_ReturnsFalse()
    {
        var f = AsComparable().LessThan(x => x.Price, x => x.Cap);
        f.IsValid(new CrossEntity(0, 0, 0, default, default, 10m, 10m, null)).Should().BeFalse();
    }

    // Cross-field: LessThanOrEqualTo
    [Fact]
    public void LessThanOrEqualTo_CrossField_Int_Equal_ReturnsTrue()
    {
        var f = AsComparable().LessThanOrEqualTo(x => x.Value, x => x.Max);
        f.IsValid(new CrossEntity(5, 0, 5, default, default, 0, 0, null)).Should().BeTrue();
    }

    [Fact]
    public void LessThanOrEqualTo_CrossField_Int_Greater_ReturnsFalse()
    {
        var f = AsComparable().LessThanOrEqualTo(x => x.Value, x => x.Max);
        f.IsValid(new CrossEntity(6, 0, 5, default, default, 0, 0, null)).Should().BeFalse();
    }

    // IsNull / IsNotNull aliases
    [Fact]
    public void IsNull_NullableName_Null_ReturnsTrue()
    {
        var f = new ValiFlow<CrossEntity>().IsNull(x => x.Name);
        f.IsValid(new CrossEntity(0, 0, 0, default, default, 0, 0, null)).Should().BeTrue();
        f.IsValid(new CrossEntity(0, 0, 0, default, default, 0, 0, "hello")).Should().BeFalse();
    }

    [Fact]
    public void IsNotNull_NullableName_NonNull_ReturnsTrue()
    {
        var f = new ValiFlow<CrossEntity>().IsNotNull(x => x.Name);
        f.IsValid(new CrossEntity(0, 0, 0, default, default, 0, 0, "hello")).Should().BeTrue();
        f.IsValid(new CrossEntity(0, 0, 0, default, default, 0, 0, null)).Should().BeFalse();
    }

    // Null guards
    [Fact]
    public void GreaterThan_CrossField_NullLeftSelector_ThrowsArgumentNullException()
    {
        Action act = () => AsComparable().GreaterThan<int>(null!, x => x.Min);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GreaterThan_CrossField_NullRightSelector_ThrowsArgumentNullException()
    {
        Action act = () => AsComparable().GreaterThan<int>(x => x.Value, null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
