using System.Linq.Expressions;
using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

public class NumericExpressionTests
{
    private static Product MakeProduct(decimal price = 10m, int quantity = 5) =>
        new("Product", price, quantity, true, DateTime.UtcNow, new List<string>());

    // 1. Zero(Quantity) — matches 0, rejects non-zero
    [Fact]
    public void Zero_Quantity_Matches0_RejectsNonZero()
    {
        var filter = new ValiFlow<Product>()
            .Zero(p => p.Quantity)
            .Build();

        filter.Compile()(MakeProduct(quantity: 0)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 5)).Should().BeFalse();
        filter.Compile()(MakeProduct(quantity: -1)).Should().BeFalse();
    }

    // 2. NotZero(Quantity) — matches non-zero, rejects 0
    [Fact]
    public void NotZero_Quantity_MatchesNonZero_Rejects0()
    {
        var filter = new ValiFlow<Product>()
            .NotZero(p => p.Quantity)
            .Build();

        filter.Compile()(MakeProduct(quantity: 5)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: -3)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 0)).Should().BeFalse();
    }

    // 3. Positive(Quantity) — matches > 0, rejects <= 0
    [Fact]
    public void Positive_Quantity_MatchesGt0_RejectsLte0()
    {
        var filter = new ValiFlow<Product>()
            .Positive(p => p.Quantity)
            .Build();

        filter.Compile()(MakeProduct(quantity: 1)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 10)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 0)).Should().BeFalse();
        filter.Compile()(MakeProduct(quantity: -1)).Should().BeFalse();
    }

    // 4. Negative(Quantity) — matches < 0, rejects >= 0
    [Fact]
    public void Negative_Quantity_MatchesLt0_RejectsGte0()
    {
        var filter = new ValiFlow<Product>()
            .Negative(p => p.Quantity)
            .Build();

        filter.Compile()(MakeProduct(quantity: -1)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: -10)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 0)).Should().BeFalse();
        filter.Compile()(MakeProduct(quantity: 1)).Should().BeFalse();
    }

    // 5. InRange(Quantity, 1, 10) — matches within range, rejects outside
    [Fact]
    public void InRange_Quantity_1_10_MatchesWithinRange_RejectsOutside()
    {
        var filter = new ValiFlow<Product>()
            .InRange(p => p.Quantity, 1, 10)
            .Build();

        filter.Compile()(MakeProduct(quantity: 1)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 5)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 10)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 0)).Should().BeFalse();
        filter.Compile()(MakeProduct(quantity: 11)).Should().BeFalse();
    }

    // 6. GreaterThan(Quantity, 5) — matches > 5
    [Fact]
    public void GreaterThan_Quantity_5_MatchesGt5()
    {
        var filter = new ValiFlow<Product>()
            .GreaterThan(p => p.Quantity, 5)
            .Build();

        filter.Compile()(MakeProduct(quantity: 6)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 5)).Should().BeFalse();
        filter.Compile()(MakeProduct(quantity: 4)).Should().BeFalse();
    }

    // 7. LessThan(Quantity, 5) — matches < 5
    [Fact]
    public void LessThan_Quantity_5_MatchesLt5()
    {
        var filter = new ValiFlow<Product>()
            .LessThan(p => p.Quantity, 5)
            .Build();

        filter.Compile()(MakeProduct(quantity: 4)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 5)).Should().BeFalse();
        filter.Compile()(MakeProduct(quantity: 6)).Should().BeFalse();
    }

    // 8. GreaterThanOrEqualTo(Quantity, 5) — matches >= 5
    [Fact]
    public void GreaterThanOrEqualTo_Quantity_5_MatchesGte5()
    {
        var filter = new ValiFlow<Product>()
            .GreaterThanOrEqualTo(p => p.Quantity, 5)
            .Build();

        filter.Compile()(MakeProduct(quantity: 5)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 6)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 4)).Should().BeFalse();
    }

    // 9. LessThanOrEqualTo(Quantity, 5) — matches <= 5
    [Fact]
    public void LessThanOrEqualTo_Quantity_5_MatchesLte5()
    {
        var filter = new ValiFlow<Product>()
            .LessThanOrEqualTo(p => p.Quantity, 5)
            .Build();

        filter.Compile()(MakeProduct(quantity: 5)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 4)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 6)).Should().BeFalse();
    }

    // 10. MinValue(Quantity, 3) — matches >= 3
    [Fact]
    public void MinValue_Quantity_3_MatchesGte3()
    {
        var filter = new ValiFlow<Product>()
            .MinValue(p => p.Quantity, 3)
            .Build();

        filter.Compile()(MakeProduct(quantity: 3)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 10)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 2)).Should().BeFalse();
    }

    // 11. MaxValue(Quantity, 10) — matches <= 10, must NOT act as MinValue
    [Fact]
    public void MaxValue_Quantity_10_MatchesLte10_NotActingAsMinValue()
    {
        var filter = new ValiFlow<Product>()
            .MaxValue(p => p.Quantity, 10)
            .Build();

        // Should match values less than 10 (proving it's max not min)
        filter.Compile()(MakeProduct(quantity: 1)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 5)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 10)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 11)).Should().BeFalse();
        // Small values should match (would fail if bugged as MinValue)
        filter.Compile()(MakeProduct(quantity: 0)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: -5)).Should().BeTrue();
    }

    // 12. IsEven(Quantity) — matches even numbers
    [Fact]
    public void IsEven_Quantity_MatchesEvenNumbers()
    {
        var filter = new ValiFlow<Product>()
            .IsEven(p => p.Quantity)
            .Build();

        filter.Compile()(MakeProduct(quantity: 2)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 4)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 0)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 3)).Should().BeFalse();
        filter.Compile()(MakeProduct(quantity: 1)).Should().BeFalse();
    }

    // 13. IsOdd(Quantity) — matches odd numbers
    [Fact]
    public void IsOdd_Quantity_MatchesOddNumbers()
    {
        var filter = new ValiFlow<Product>()
            .IsOdd(p => p.Quantity)
            .Build();

        filter.Compile()(MakeProduct(quantity: 1)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 3)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 2)).Should().BeFalse();
        filter.Compile()(MakeProduct(quantity: 4)).Should().BeFalse();
    }

    // 14. IsMultipleOf(Quantity, 3) — matches multiples of 3
    [Fact]
    public void IsMultipleOf_Quantity_3_MatchesMultiplesOf3()
    {
        var filter = new ValiFlow<Product>()
            .IsMultipleOf(p => p.Quantity, 3)
            .Build();

        filter.Compile()(MakeProduct(quantity: 3)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 6)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 9)).Should().BeTrue();
        filter.Compile()(MakeProduct(quantity: 4)).Should().BeFalse();
        filter.Compile()(MakeProduct(quantity: 5)).Should().BeFalse();
    }

    // 15. IsMultipleOf(Quantity, 0) — throws ArgumentException
    [Fact]
    public void IsMultipleOf_Quantity_0_ThrowsArgumentException()
    {
        var builder = new ValiFlow<Product>();

        Action act = () => builder.IsMultipleOf(p => p.Quantity, 0);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*zero*");
    }

    // ── Non-int numeric type overloads ──

    private record NumericEntity(long LongVal, decimal DecimalVal, double DoubleVal, float FloatVal, short ShortVal);

    [Fact]
    public void Zero_Long_Matches0_RejectsNonZero()
    {
        var filter = new ValiFlow<NumericEntity>().Zero(e => e.LongVal).Build().Compile();
        filter(new NumericEntity(0, 0, 0, 0, 0)).Should().BeTrue();
        filter(new NumericEntity(1, 0, 0, 0, 0)).Should().BeFalse();
    }

    [Fact]
    public void Positive_Decimal_MatchesGt0_RejectsZero()
    {
        var filter = new ValiFlow<NumericEntity>().Positive(e => e.DecimalVal).Build().Compile();
        filter(new NumericEntity(0, 10m, 0, 0, 0)).Should().BeTrue();
        filter(new NumericEntity(0, 0m, 0, 0, 0)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_Double_5_MatchesGt5()
    {
        var filter = new ValiFlow<NumericEntity>().GreaterThan(e => e.DoubleVal, 5.0).Build().Compile();
        filter(new NumericEntity(0, 0, 6.0, 0, 0)).Should().BeTrue();
        filter(new NumericEntity(0, 0, 4.0, 0, 0)).Should().BeFalse();
    }

    [Fact]
    public void InRange_Short_1_10_MatchesWithinRange()
    {
        var filter = new ValiFlow<NumericEntity>().InRange(e => e.ShortVal, (short)1, (short)10).Build().Compile();
        filter(new NumericEntity(0, 0, 0, 0, 5)).Should().BeTrue();
        filter(new NumericEntity(0, 0, 0, 0, 11)).Should().BeFalse();
    }

    // ── Regresión: IComparable<TValue> scalar — null no debe lanzar NRE ──

    [Fact]
    public void GreaterThan_IComparable_NullValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().GreaterThan(p => p.Name, "A").Build().Compile();

        // Name es null → no debe lanzar NullReferenceException, debe retornar false
        filter(new Product(null, 10m, 5, true, DateTime.UtcNow, new List<string>())).Should().BeFalse();
    }

    [Fact]
    public void LessThan_IComparable_NullValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().LessThan(p => p.Name, "Z").Build().Compile();

        filter(new Product(null, 10m, 5, true, DateTime.UtcNow, new List<string>())).Should().BeFalse();
    }

    [Fact]
    public void IsEven_Long_MatchesEvenNumbers()
    {
        var filter = new ValiFlow<NumericEntity>().IsEven(e => e.LongVal).Build().Compile();
        filter(new NumericEntity(2, 0, 0, 0, 0)).Should().BeTrue();
        filter(new NumericEntity(3, 0, 0, 0, 0)).Should().BeFalse();
    }

    [Fact]
    public void IsOdd_Long_MatchesOddNumbers()
    {
        var filter = new ValiFlow<NumericEntity>().IsOdd(e => e.LongVal).Build().Compile();
        filter(new NumericEntity(3, 0, 0, 0, 0)).Should().BeTrue();
        filter(new NumericEntity(2, 0, 0, 0, 0)).Should().BeFalse();
    }

    [Fact]
    public void IsMultipleOf_Long_3_MatchesMultiplesOf3()
    {
        var filter = new ValiFlow<NumericEntity>().IsMultipleOf(e => e.LongVal, 3L).Build().Compile();
        filter(new NumericEntity(9, 0, 0, 0, 0)).Should().BeTrue();
        filter(new NumericEntity(4, 0, 0, 0, 0)).Should().BeFalse();
    }

    [Fact]
    public void IsMultipleOf_Long_0_ThrowsArgumentException()
    {
        var builder = new ValiFlow<NumericEntity>();
        Action act = () => builder.IsMultipleOf(e => e.LongVal, 0L);
        act.Should().Throw<ArgumentException>().WithMessage("*zero*");
    }

    // ── Cross-property InRange (ValiFlow / NumericExpression) ─────────────────

    private record CrossRangeEntity(int Value, int Min, int Max);

    [Fact]
    public void InRange_CrossProperty_Int_ValiFlow_MatchesWhenValueBetweenMinMax()
    {
        var filter = new ValiFlow<CrossRangeEntity>()
            .InRange(e => e.Value, e => e.Min, e => e.Max)
            .Build().Compile();

        filter(new CrossRangeEntity(5, 1, 10)).Should().BeTrue();
        filter(new CrossRangeEntity(1, 1, 10)).Should().BeTrue();
        filter(new CrossRangeEntity(10, 1, 10)).Should().BeTrue();
        filter(new CrossRangeEntity(0, 1, 10)).Should().BeFalse();
        filter(new CrossRangeEntity(11, 1, 10)).Should().BeFalse();
    }

    [Fact]
    public void InRange_CrossProperty_Int_SameSelector_TwoCalls_NoAliasing()
    {
        // Regression: using the same selector twice should not cause node sharing
        Expression<Func<CrossRangeEntity, int>> valSel = e => e.Value;
        Expression<Func<CrossRangeEntity, int>> minSel = e => e.Min;
        Expression<Func<CrossRangeEntity, int>> maxSel = e => e.Max;

        var filter = new ValiFlow<CrossRangeEntity>()
            .InRange(valSel, minSel, maxSel)
            .InRange(valSel, minSel, maxSel)  // same selectors used twice
            .Build().Compile();

        filter(new CrossRangeEntity(5, 1, 10)).Should().BeTrue();
        filter(new CrossRangeEntity(0, 1, 10)).Should().BeFalse();
    }

    // ── Cross-property InRange for additional numeric types ───────────────────

    private record LongRangeEntity(long Value, long Min, long Max);
    private record DoubleRangeEntity(double Value, double Min, double Max);
    private record FloatRangeEntity(float Value, float Min, float Max);
    private record ShortRangeEntity(short Value, short Min, short Max);

    [Fact]
    public void InRange_CrossProperty_Long_ValiFlow_MatchesWithinRange()
    {
        var filter = new ValiFlow<LongRangeEntity>()
            .InRange(e => e.Value, e => e.Min, e => e.Max)
            .Build().Compile();

        filter(new LongRangeEntity(5L, 1L, 10L)).Should().BeTrue();
        filter(new LongRangeEntity(0L, 1L, 10L)).Should().BeFalse();
    }

    [Fact]
    public void InRange_CrossProperty_Double_ValiFlow_MatchesWithinRange()
    {
        var filter = new ValiFlow<DoubleRangeEntity>()
            .InRange(e => e.Value, e => e.Min, e => e.Max)
            .Build().Compile();

        filter(new DoubleRangeEntity(5.0, 1.0, 10.0)).Should().BeTrue();
        filter(new DoubleRangeEntity(11.0, 1.0, 10.0)).Should().BeFalse();
    }

    [Fact]
    public void InRange_CrossProperty_Float_ValiFlow_MatchesWithinRange()
    {
        var filter = new ValiFlow<FloatRangeEntity>()
            .InRange(e => e.Value, e => e.Min, e => e.Max)
            .Build().Compile();

        filter(new FloatRangeEntity(5f, 1f, 10f)).Should().BeTrue();
        filter(new FloatRangeEntity(0f, 1f, 10f)).Should().BeFalse();
    }

    [Fact]
    public void InRange_CrossProperty_Short_ValiFlow_MatchesWithinRange()
    {
        var filter = new ValiFlow<ShortRangeEntity>()
            .InRange(e => e.Value, e => e.Min, e => e.Max)
            .Build().Compile();

        filter(new ShortRangeEntity(5, 1, 10)).Should().BeTrue();
        filter(new ShortRangeEntity(0, 1, 10)).Should().BeFalse();
    }

    // ── Nullable float? HasValue / IsNullOrZero ───────────────────────────────

    private record FloatNullableEntity(float? Value);

    [Fact]
    public void HasValue_Float_NullReturnsFalse()
    {
        var filter = new ValiFlow<FloatNullableEntity>().HasValue(e => e.Value).Build().Compile();
        filter(new FloatNullableEntity(null)).Should().BeFalse();
        filter(new FloatNullableEntity(1.5f)).Should().BeTrue();
    }

    [Fact]
    public void IsNullOrZero_Float_NullReturnsTrue()
    {
        var filter = new ValiFlow<FloatNullableEntity>().IsNullOrZero(e => e.Value).Build().Compile();
        filter(new FloatNullableEntity(null)).Should().BeTrue();
        filter(new FloatNullableEntity(0f)).Should().BeTrue();
        filter(new FloatNullableEntity(1f)).Should().BeFalse();
    }

    // ── Cross-property InRange/GreaterThan — CloneSelectorBody regression ─────

    private record DecimalRangeEntity(decimal Total, decimal MinTotal, decimal MaxTotal);

    [Fact]
    public void NumericExpression_CrossProperty_InRange_DoesNotShareExpressionNodes()
    {
        // Arrange — two separate selectors for min and max
        var validator = new ValiFlow<DecimalRangeEntity>()
            .InRange(o => o.Total, o => o.MinTotal, o => o.MaxTotal);

        var entity = new DecimalRangeEntity(Total: 50m, MinTotal: 10m, MaxTotal: 100m);

        // Act
        var result = validator.IsValid(entity);

        // Assert — previously caused DAG aliasing exception or wrong result
        result.Should().BeTrue();
    }

    [Fact]
    public void NumericExpression_CrossProperty_GreaterThan_DoesNotShareExpressionNodes()
    {
        // Arrange
        var validator = new ValiFlow<DecimalRangeEntity>()
            .GreaterThan(o => o.Total, o => o.MinTotal);

        var entity = new DecimalRangeEntity(Total: 50m, MinTotal: 10m, MaxTotal: 0m);

        // Act
        var result = validator.IsValid(entity);

        // Assert
        result.Should().BeTrue();
    }
}

public class BetweenExclusiveAndCloseToTests
{
    private record IntEntity(int Value);
    private record DecimalEntity(decimal Value);
    private record DoubleEntity(double Value);
    private record FloatEntity(float Value);

    // IsBetweenExclusive — int

    [Fact]
    public void IsBetweenExclusive_Int_Inside_ReturnsTrue()
    {
        new ValiFlow<IntEntity>().IsBetweenExclusive(e => e.Value, 1, 10)
            .IsValid(new IntEntity(5)).Should().BeTrue();
    }

    [Fact]
    public void IsBetweenExclusive_Int_AtMin_ReturnsFalse()
    {
        new ValiFlow<IntEntity>().IsBetweenExclusive(e => e.Value, 1, 10)
            .IsValid(new IntEntity(1)).Should().BeFalse();
    }

    [Fact]
    public void IsBetweenExclusive_Int_AtMax_ReturnsFalse()
    {
        new ValiFlow<IntEntity>().IsBetweenExclusive(e => e.Value, 1, 10)
            .IsValid(new IntEntity(10)).Should().BeFalse();
    }

    [Fact]
    public void IsBetweenExclusive_Int_Below_ReturnsFalse()
    {
        new ValiFlow<IntEntity>().IsBetweenExclusive(e => e.Value, 1, 10)
            .IsValid(new IntEntity(0)).Should().BeFalse();
    }

    [Fact]
    public void IsBetweenExclusive_Decimal_Inside_ReturnsTrue()
    {
        new ValiFlow<DecimalEntity>().IsBetweenExclusive(e => e.Value, 1m, 10m)
            .IsValid(new DecimalEntity(5.5m)).Should().BeTrue();
    }

    [Fact]
    public void IsBetweenExclusive_MaxLteMin_ThrowsArgumentOutOfRangeException()
        => Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ValiFlow<DecimalEntity>().IsBetweenExclusive(e => e.Value, 10m, 10m));

    [Fact]
    public void IsBetweenExclusive_NullSelector_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() =>
            new ValiFlow<DecimalEntity>().IsBetweenExclusive(null!, 1m, 10m));

    // IsCloseTo — double

    [Fact]
    public void IsCloseTo_Double_WithinTolerance_ReturnsTrue()
    {
        new ValiFlow<DoubleEntity>().IsCloseTo(e => e.Value, 10.0, 0.5)
            .IsValid(new DoubleEntity(10.3)).Should().BeTrue();
    }

    [Fact]
    public void IsCloseTo_Double_ExactMatch_ReturnsTrue()
    {
        new ValiFlow<DoubleEntity>().IsCloseTo(e => e.Value, 10.0, 0.5)
            .IsValid(new DoubleEntity(10.0)).Should().BeTrue();
    }

    [Fact]
    public void IsCloseTo_Double_OutsideTolerance_ReturnsFalse()
    {
        new ValiFlow<DoubleEntity>().IsCloseTo(e => e.Value, 10.0, 0.5)
            .IsValid(new DoubleEntity(11.0)).Should().BeFalse();
    }

    // IsCloseTo — float

    [Fact]
    public void IsCloseTo_Float_WithinTolerance_ReturnsTrue()
    {
        new ValiFlow<FloatEntity>().IsCloseTo(e => e.Value, 10.0f, 0.5f)
            .IsValid(new FloatEntity(10.3f)).Should().BeTrue();
    }

    [Fact]
    public void IsCloseTo_NegativeTolerance_ThrowsArgumentOutOfRangeException()
        => Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ValiFlow<DoubleEntity>().IsCloseTo(e => e.Value, 10.0, -0.1));

    [Fact]
    public void IsCloseTo_NullSelector_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() =>
            new ValiFlow<DoubleEntity>().IsCloseTo(null!, 10.0, 0.5));
}
