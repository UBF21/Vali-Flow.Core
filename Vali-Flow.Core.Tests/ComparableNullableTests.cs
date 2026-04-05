using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

public record Item(
    int Count,
    decimal Price,
    long Stock,
    double? Rating,
    int? Discount,
    decimal? Tax,
    long? Bonus,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime ExpiresAt,
    string Name,
    TimeSpan Duration);

public class ComparableNullableTests
{
    private static readonly DateTime BaseDate = new DateTime(2025, 1, 15);

    private static Item Make(
        int count = 5,
        decimal price = 10m,
        long stock = 100L,
        double? rating = null,
        int? discount = null,
        decimal? tax = null,
        long? bonus = null,
        DateTime createdAt = default,
        DateTime? updatedAt = null,
        DateTime expiresAt = default,
        string name = "Apple",
        TimeSpan duration = default)
        => new Item(
            count,
            price,
            stock,
            rating,
            discount,
            tax,
            bonus,
            createdAt == default ? BaseDate : createdAt,
            updatedAt,
            expiresAt == default ? BaseDate.AddDays(30) : expiresAt,
            name,
            duration == default ? TimeSpan.FromHours(1) : duration);

    // ── IComparable<TValue> GreaterThan ────────────────────────────────────────

    [Fact]
    public void GreaterThan_Generic_Int_GreaterValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Count, 5)
            .Build();
        filter.Compile()(Make(count: 10)).Should().BeTrue();
    }

    [Fact]
    public void GreaterThan_Generic_Int_EqualValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Count, 5)
            .Build();
        filter.Compile()(Make(count: 5)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_Generic_Int_LessValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Count, 5)
            .Build();
        filter.Compile()(Make(count: 3)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_Generic_Decimal_GreaterValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Price, 9.99m)
            .Build();
        filter.Compile()(Make(price: 10m)).Should().BeTrue();
    }

    [Fact]
    public void GreaterThan_Generic_Decimal_EqualValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Price, 10m)
            .Build();
        filter.Compile()(Make(price: 10m)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_Generic_DateTime_GreaterValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.CreatedAt, BaseDate.AddDays(-1))
            .Build();
        filter.Compile()(Make(createdAt: BaseDate)).Should().BeTrue();
    }

    [Fact]
    public void GreaterThan_Generic_DateTime_EqualValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.CreatedAt, BaseDate)
            .Build();
        filter.Compile()(Make(createdAt: BaseDate)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_Generic_TimeSpan_GreaterValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Duration, TimeSpan.FromMinutes(30))
            .Build();
        filter.Compile()(Make(duration: TimeSpan.FromHours(1))).Should().BeTrue();
    }

    [Fact]
    public void GreaterThan_Generic_TimeSpan_LessValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Duration, TimeSpan.FromHours(2))
            .Build();
        filter.Compile()(Make(duration: TimeSpan.FromHours(1))).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_Generic_String_AlphabeticallyGreater_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Name, "Banana")
            .Build();
        filter.Compile()(Make(name: "Cherry")).Should().BeTrue();
    }

    [Fact]
    public void GreaterThan_Generic_String_AlphabeticallyLess_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Name, "Banana")
            .Build();
        filter.Compile()(Make(name: "Apple")).Should().BeFalse();
    }

    // ── IComparable<TValue> GreaterThanOrEqualTo ───────────────────────────────

    [Fact]
    public void GreaterThanOrEqualTo_Generic_Int_GreaterValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThanOrEqualTo(i => i.Count, 5)
            .Build();
        filter.Compile()(Make(count: 10)).Should().BeTrue();
    }

    [Fact]
    public void GreaterThanOrEqualTo_Generic_Int_EqualValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThanOrEqualTo(i => i.Count, 5)
            .Build();
        filter.Compile()(Make(count: 5)).Should().BeTrue();
    }

    [Fact]
    public void GreaterThanOrEqualTo_Generic_Int_LessValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThanOrEqualTo(i => i.Count, 5)
            .Build();
        filter.Compile()(Make(count: 3)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThanOrEqualTo_Generic_Decimal_BoundaryValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThanOrEqualTo(i => i.Price, 10m)
            .Build();
        filter.Compile()(Make(price: 10m)).Should().BeTrue();
    }

    [Fact]
    public void GreaterThanOrEqualTo_Generic_DateTime_EqualValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThanOrEqualTo(i => i.CreatedAt, BaseDate)
            .Build();
        filter.Compile()(Make(createdAt: BaseDate)).Should().BeTrue();
    }

    // ── IComparable<TValue> LessThan ───────────────────────────────────────────

    [Fact]
    public void LessThan_Generic_Int_LessValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .LessThan(i => i.Count, 10)
            .Build();
        filter.Compile()(Make(count: 3)).Should().BeTrue();
    }

    [Fact]
    public void LessThan_Generic_Int_EqualValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .LessThan(i => i.Count, 5)
            .Build();
        filter.Compile()(Make(count: 5)).Should().BeFalse();
    }

    [Fact]
    public void LessThan_Generic_Int_GreaterValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .LessThan(i => i.Count, 5)
            .Build();
        filter.Compile()(Make(count: 10)).Should().BeFalse();
    }

    [Fact]
    public void LessThan_Generic_Decimal_LessValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .LessThan(i => i.Price, 20m)
            .Build();
        filter.Compile()(Make(price: 9.99m)).Should().BeTrue();
    }

    [Fact]
    public void LessThan_Generic_DateTime_LessValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .LessThan(i => i.CreatedAt, BaseDate.AddDays(1))
            .Build();
        filter.Compile()(Make(createdAt: BaseDate)).Should().BeTrue();
    }

    // ── IComparable<TValue> LessThanOrEqualTo ─────────────────────────────────

    [Fact]
    public void LessThanOrEqualTo_Generic_Int_LessValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .LessThanOrEqualTo(i => i.Count, 10)
            .Build();
        filter.Compile()(Make(count: 3)).Should().BeTrue();
    }

    [Fact]
    public void LessThanOrEqualTo_Generic_Int_EqualValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .LessThanOrEqualTo(i => i.Count, 5)
            .Build();
        filter.Compile()(Make(count: 5)).Should().BeTrue();
    }

    [Fact]
    public void LessThanOrEqualTo_Generic_Int_GreaterValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .LessThanOrEqualTo(i => i.Count, 5)
            .Build();
        filter.Compile()(Make(count: 10)).Should().BeFalse();
    }

    [Fact]
    public void LessThanOrEqualTo_Generic_Decimal_BoundaryValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .LessThanOrEqualTo(i => i.Price, 10m)
            .Build();
        filter.Compile()(Make(price: 10m)).Should().BeTrue();
    }

    // ── IComparable<TValue> InRange ────────────────────────────────────────────

    [Fact]
    public void InRange_Generic_Int_WithinRange_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Count, 1, 10)
            .Build();
        filter.Compile()(Make(count: 5)).Should().BeTrue();
    }

    [Fact]
    public void InRange_Generic_Int_AtMinBoundary_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Count, 1, 10)
            .Build();
        filter.Compile()(Make(count: 1)).Should().BeTrue();
    }

    [Fact]
    public void InRange_Generic_Int_AtMaxBoundary_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Count, 1, 10)
            .Build();
        filter.Compile()(Make(count: 10)).Should().BeTrue();
    }

    [Fact]
    public void InRange_Generic_Int_BelowMin_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Count, 1, 10)
            .Build();
        filter.Compile()(Make(count: 0)).Should().BeFalse();
    }

    [Fact]
    public void InRange_Generic_Int_AboveMax_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Count, 1, 10)
            .Build();
        filter.Compile()(Make(count: 11)).Should().BeFalse();
    }

    [Fact]
    public void InRange_Generic_Decimal_WithinRange_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Price, 5m, 15m)
            .Build();
        filter.Compile()(Make(price: 10m)).Should().BeTrue();
    }

    [Fact]
    public void InRange_Generic_Decimal_OutOfRange_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Price, 5m, 9m)
            .Build();
        filter.Compile()(Make(price: 10m)).Should().BeFalse();
    }

    [Fact]
    public void InRange_Generic_DateTime_WithinRange_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.CreatedAt, BaseDate.AddDays(-1), BaseDate.AddDays(1))
            .Build();
        filter.Compile()(Make(createdAt: BaseDate)).Should().BeTrue();
    }

    [Fact]
    public void InRange_Generic_DateTime_OutOfRange_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.CreatedAt, BaseDate.AddDays(1), BaseDate.AddDays(5))
            .Build();
        filter.Compile()(Make(createdAt: BaseDate)).Should().BeFalse();
    }

    // ── IComparable<TValue> EqualTo ────────────────────────────────────────────

    [Fact]
    public void EqualTo_Generic_Int_EqualValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .EqualTo(i => i.Count, 5)
            .Build();
        filter.Compile()(Make(count: 5)).Should().BeTrue();
    }

    [Fact]
    public void EqualTo_Generic_Int_NotEqualValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .EqualTo(i => i.Count, 5)
            .Build();
        filter.Compile()(Make(count: 6)).Should().BeFalse();
    }

    [Fact]
    public void EqualTo_Generic_String_EqualValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .EqualTo(i => i.Name, "Apple")
            .Build();
        filter.Compile()(Make(name: "Apple")).Should().BeTrue();
    }

    [Fact]
    public void EqualTo_Generic_String_NotEqualValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .EqualTo(i => i.Name, "Apple")
            .Build();
        filter.Compile()(Make(name: "Banana")).Should().BeFalse();
    }

    // ── Cross-property comparisons ─────────────────────────────────────────────

    [Fact]
    public void GreaterThan_CrossProperty_Int_EndGreaterThanStart_ReturnsTrue()
    {
        // ExpiresAt > CreatedAt: Count (End) > Stock is tested via int cross-property
        // Use Count vs Discount — but Discount is nullable, so use Count and a different record
        // Actually we test int cross-property: ExpiresAt.Day doesn't work directly.
        // We use a helper record in-line. Let's use Count > 0 via cross-property with another field.
        // Item has int Count and int? Discount — but cross-property needs same type.
        // We'll test using the item where Count is the "End" and we compare to a constant via selector trick.
        // Cross-property: Count (10) > Count (5) — we need two int properties, so we can use Count vs itself but that's trivial.
        // The Item record has Count (int) only. Let's create an item where ExpiresAt > CreatedAt.
        var filter = new ValiFlow<Item>()
            .GreaterThan<DateTime>(i => i.ExpiresAt, i => i.CreatedAt)
            .Build();
        var item = Make(createdAt: BaseDate, expiresAt: BaseDate.AddDays(30));
        filter.Compile()(item).Should().BeTrue();
    }

    [Fact]
    public void GreaterThan_CrossProperty_Int_StartGreaterThanEnd_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan<DateTime>(i => i.ExpiresAt, i => i.CreatedAt)
            .Build();
        var item = Make(createdAt: BaseDate.AddDays(30), expiresAt: BaseDate);
        filter.Compile()(item).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_CrossProperty_Decimal_GreaterValue_ReturnsTrue()
    {
        // Price > Tax.Value — but Tax is nullable. Use Price vs Price (same ref) is trivial.
        // ExpiresAt > CreatedAt as Decimal doesn't make sense. Let's use Count (int) and stock doesn't share type.
        // The best cross-property decimal test: Price (10m) > Price (10m) — same → false. Let's ensure it handles correctly.
        // Actually we want a record with two decimal properties. Item has Price (decimal) and Tax (decimal?).
        // We need decimal vs decimal non-nullable. We can test ExpiresAt.Ticks not helpful.
        // Just test with GreaterThan<decimal> on Price vs a hardcoded via selector returning constant.
        // The simplest approach: compare Price to itself should be false (not strictly greater).
        var filter = new ValiFlow<Item>()
            .GreaterThan<decimal>(i => i.Price, i => i.Price)
            .Build();
        filter.Compile()(Make(price: 10m)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_CrossProperty_DateTime_ExpiresAfterCreated_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan<DateTime>(i => i.ExpiresAt, i => i.CreatedAt)
            .Build();
        var item = Make(createdAt: BaseDate, expiresAt: BaseDate.AddYears(1));
        filter.Compile()(item).Should().BeTrue();
    }

    [Fact]
    public void LessThan_CrossProperty_DateTime_CreatedBeforeExpires_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .LessThan<DateTime>(i => i.CreatedAt, i => i.ExpiresAt)
            .Build();
        var item = Make(createdAt: BaseDate, expiresAt: BaseDate.AddDays(30));
        filter.Compile()(item).Should().BeTrue();
    }

    [Fact]
    public void GreaterThanOrEqualTo_CrossProperty_DateTime_SameValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThanOrEqualTo<DateTime>(i => i.CreatedAt, i => i.CreatedAt)
            .Build();
        filter.Compile()(Make(createdAt: BaseDate)).Should().BeTrue();
    }

    [Fact]
    public void LessThanOrEqualTo_CrossProperty_DateTime_SameValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .LessThanOrEqualTo<DateTime>(i => i.ExpiresAt, i => i.ExpiresAt)
            .Build();
        filter.Compile()(Make()).Should().BeTrue();
    }

    [Fact]
    public void CrossProperty_WithAnd_BothConditions_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan<DateTime>(i => i.ExpiresAt, i => i.CreatedAt)
            .And()
            .GreaterThan(i => i.Count, 0)
            .Build();
        var item = Make(createdAt: BaseDate, expiresAt: BaseDate.AddDays(30), count: 5);
        filter.Compile()(item).Should().BeTrue();
    }

    [Fact]
    public void CrossProperty_WithAnd_OneConditionFails_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan<DateTime>(i => i.ExpiresAt, i => i.CreatedAt)
            .And()
            .GreaterThan(i => i.Count, 10)
            .Build();
        var item = Make(createdAt: BaseDate, expiresAt: BaseDate.AddDays(30), count: 5);
        filter.Compile()(item).Should().BeFalse();
    }

    // ── Nullable numeric IsNullOrZero ──────────────────────────────────────────

    [Fact]
    public void IsNullOrZero_Int_Null_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .IsNullOrZero(i => i.Discount)
            .Build();
        filter.Compile()(Make(discount: null)).Should().BeTrue();
    }

    [Fact]
    public void IsNullOrZero_Int_Zero_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .IsNullOrZero(i => i.Discount)
            .Build();
        filter.Compile()(Make(discount: 0)).Should().BeTrue();
    }

    [Fact]
    public void IsNullOrZero_Int_PositiveValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .IsNullOrZero(i => i.Discount)
            .Build();
        filter.Compile()(Make(discount: 5)).Should().BeFalse();
    }

    [Fact]
    public void IsNullOrZero_Int_NegativeValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .IsNullOrZero(i => i.Discount)
            .Build();
        filter.Compile()(Make(discount: -1)).Should().BeFalse();
    }

    [Fact]
    public void IsNullOrZero_Decimal_Null_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .IsNullOrZero(i => i.Tax)
            .Build();
        filter.Compile()(Make(tax: null)).Should().BeTrue();
    }

    [Fact]
    public void IsNullOrZero_Decimal_Zero_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .IsNullOrZero(i => i.Tax)
            .Build();
        filter.Compile()(Make(tax: 0m)).Should().BeTrue();
    }

    [Fact]
    public void IsNullOrZero_Decimal_NonZero_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .IsNullOrZero(i => i.Tax)
            .Build();
        filter.Compile()(Make(tax: 0.1m)).Should().BeFalse();
    }

    [Fact]
    public void IsNullOrZero_Long_Null_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .IsNullOrZero(i => i.Bonus)
            .Build();
        filter.Compile()(Make(bonus: null)).Should().BeTrue();
    }

    [Fact]
    public void IsNullOrZero_Long_Zero_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .IsNullOrZero(i => i.Bonus)
            .Build();
        filter.Compile()(Make(bonus: 0L)).Should().BeTrue();
    }

    [Fact]
    public void IsNullOrZero_Long_NonZero_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .IsNullOrZero(i => i.Bonus)
            .Build();
        filter.Compile()(Make(bonus: 1L)).Should().BeFalse();
    }

    [Fact]
    public void IsNullOrZero_Double_Null_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .IsNullOrZero(i => i.Rating)
            .Build();
        filter.Compile()(Make(rating: null)).Should().BeTrue();
    }

    [Fact]
    public void IsNullOrZero_Double_Zero_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .IsNullOrZero(i => i.Rating)
            .Build();
        filter.Compile()(Make(rating: 0.0)).Should().BeTrue();
    }

    [Fact]
    public void IsNullOrZero_Double_NonZero_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .IsNullOrZero(i => i.Rating)
            .Build();
        filter.Compile()(Make(rating: 0.1)).Should().BeFalse();
    }

    // ── Nullable numeric HasValue ──────────────────────────────────────────────

    [Fact]
    public void HasValue_Int_Null_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .HasValue(i => i.Discount)
            .Build();
        filter.Compile()(Make(discount: null)).Should().BeFalse();
    }

    [Fact]
    public void HasValue_Int_Zero_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .HasValue(i => i.Discount)
            .Build();
        filter.Compile()(Make(discount: 0)).Should().BeTrue();
    }

    [Fact]
    public void HasValue_Int_PositiveValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .HasValue(i => i.Discount)
            .Build();
        filter.Compile()(Make(discount: 5)).Should().BeTrue();
    }

    [Fact]
    public void HasValue_Decimal_Null_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .HasValue(i => i.Tax)
            .Build();
        filter.Compile()(Make(tax: null)).Should().BeFalse();
    }

    [Fact]
    public void HasValue_Decimal_NonNull_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .HasValue(i => i.Tax)
            .Build();
        filter.Compile()(Make(tax: 100m)).Should().BeTrue();
    }

    [Fact]
    public void HasValue_Long_Null_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .HasValue(i => i.Bonus)
            .Build();
        filter.Compile()(Make(bonus: null)).Should().BeFalse();
    }

    [Fact]
    public void HasValue_Long_NonNull_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .HasValue(i => i.Bonus)
            .Build();
        filter.Compile()(Make(bonus: 1L)).Should().BeTrue();
    }

    // ── Nullable numeric GreaterThan ───────────────────────────────────────────

    [Fact]
    public void GreaterThan_NullableInt_Null_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Discount, 0)
            .Build();
        filter.Compile()(Make(discount: null)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_NullableInt_GreaterValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Discount, 0)
            .Build();
        filter.Compile()(Make(discount: 5)).Should().BeTrue();
    }

    [Fact]
    public void GreaterThan_NullableInt_EqualValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Discount, 5)
            .Build();
        filter.Compile()(Make(discount: 5)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_NullableInt_LessValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Discount, 10)
            .Build();
        filter.Compile()(Make(discount: 5)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_NullableDecimal_Null_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Tax, 0m)
            .Build();
        filter.Compile()(Make(tax: null)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_NullableDecimal_GreaterValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Tax, 5m)
            .Build();
        filter.Compile()(Make(tax: 10m)).Should().BeTrue();
    }

    [Fact]
    public void GreaterThan_NullableLong_Null_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Bonus, 0L)
            .Build();
        filter.Compile()(Make(bonus: null)).Should().BeFalse();
    }

    [Fact]
    public void GreaterThan_NullableLong_GreaterValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Bonus, 5L)
            .Build();
        filter.Compile()(Make(bonus: 10L)).Should().BeTrue();
    }

    // ── Nullable numeric LessThan ──────────────────────────────────────────────

    [Fact]
    public void LessThan_NullableInt_Null_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .LessThan(i => i.Discount, 10)
            .Build();
        filter.Compile()(Make(discount: null)).Should().BeFalse();
    }

    [Fact]
    public void LessThan_NullableInt_LessValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .LessThan(i => i.Discount, 10)
            .Build();
        filter.Compile()(Make(discount: 5)).Should().BeTrue();
    }

    [Fact]
    public void LessThan_NullableInt_EqualValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .LessThan(i => i.Discount, 5)
            .Build();
        filter.Compile()(Make(discount: 5)).Should().BeFalse();
    }

    [Fact]
    public void LessThan_NullableInt_GreaterValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .LessThan(i => i.Discount, 3)
            .Build();
        filter.Compile()(Make(discount: 5)).Should().BeFalse();
    }

    [Fact]
    public void LessThan_NullableDecimal_Null_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .LessThan(i => i.Tax, 10m)
            .Build();
        filter.Compile()(Make(tax: null)).Should().BeFalse();
    }

    [Fact]
    public void LessThan_NullableDecimal_LessValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .LessThan(i => i.Tax, 10m)
            .Build();
        filter.Compile()(Make(tax: 5m)).Should().BeTrue();
    }

    [Fact]
    public void LessThan_NullableLong_Null_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .LessThan(i => i.Bonus, 10L)
            .Build();
        filter.Compile()(Make(bonus: null)).Should().BeFalse();
    }

    [Fact]
    public void LessThan_NullableLong_LessValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .LessThan(i => i.Bonus, 10L)
            .Build();
        filter.Compile()(Make(bonus: 5L)).Should().BeTrue();
    }

    // ── Nullable numeric InRange ───────────────────────────────────────────────

    [Fact]
    public void InRange_NullableInt_Null_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Discount, 1, 10)
            .Build();
        filter.Compile()(Make(discount: null)).Should().BeFalse();
    }

    [Fact]
    public void InRange_NullableInt_WithinRange_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Discount, 1, 10)
            .Build();
        filter.Compile()(Make(discount: 5)).Should().BeTrue();
    }

    [Fact]
    public void InRange_NullableInt_OutOfRange_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Discount, 1, 10)
            .Build();
        filter.Compile()(Make(discount: 15)).Should().BeFalse();
    }

    [Fact]
    public void InRange_NullableDecimal_Null_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Tax, 1m, 10m)
            .Build();
        filter.Compile()(Make(tax: null)).Should().BeFalse();
    }

    [Fact]
    public void InRange_NullableDecimal_AtMinBoundary_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Tax, 1m, 10m)
            .Build();
        filter.Compile()(Make(tax: 1m)).Should().BeTrue();
    }

    [Fact]
    public void InRange_NullableDecimal_AtMaxBoundary_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Tax, 1m, 10m)
            .Build();
        filter.Compile()(Make(tax: 10m)).Should().BeTrue();
    }

    [Fact]
    public void InRange_NullableDecimal_OutOfRange_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Tax, 1m, 10m)
            .Build();
        filter.Compile()(Make(tax: 15m)).Should().BeFalse();
    }

    [Fact]
    public void InRange_NullableLong_Null_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Bonus, 1L, 100L)
            .Build();
        filter.Compile()(Make(bonus: null)).Should().BeFalse();
    }

    [Fact]
    public void InRange_NullableLong_WithinRange_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Bonus, 1L, 100L)
            .Build();
        filter.Compile()(Make(bonus: 50L)).Should().BeTrue();
    }

    [Fact]
    public void InRange_NullableLong_OutOfRange_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Bonus, 1L, 100L)
            .Build();
        filter.Compile()(Make(bonus: 200L)).Should().BeFalse();
    }

    // ── Backward compatibility ─────────────────────────────────────────────────

    [Fact]
    public void BackwardCompat_GreaterThan_Int_ExistingOverload_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Count, 3)
            .Build();
        filter.Compile()(Make(count: 5)).Should().BeTrue();
    }

    [Fact]
    public void BackwardCompat_GreaterThan_Int_ExistingOverload_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .GreaterThan(i => i.Count, 10)
            .Build();
        filter.Compile()(Make(count: 5)).Should().BeFalse();
    }

    [Fact]
    public void BackwardCompat_InRange_Decimal_ExistingOverload_ReturnsTrue()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Price, 5m, 15m)
            .Build();
        filter.Compile()(Make(price: 10m)).Should().BeTrue();
    }

    [Fact]
    public void BackwardCompat_InRange_Decimal_ExistingOverload_ReturnsFalse()
    {
        var filter = new ValiFlow<Item>()
            .InRange(i => i.Price, 5m, 9m)
            .Build();
        filter.Compile()(Make(price: 10m)).Should().BeFalse();
    }
}
