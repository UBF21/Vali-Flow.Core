using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;
using Vali_Flow.Core.Models;

namespace Vali_Flow.Core.Tests;

// ─── Models ──────────────────────────────────────────────────────────────────

public record ComplexOrderItem(string? ProductId, int Quantity, decimal UnitPrice);

public record ComplexAddress(string? Street, string? City, string? Country, string? ZipCode);

public record ComplexOrder(
    string? OrderId,
    string? CustomerId,
    decimal Amount,
    string? Status,       // "Pending", "Approved", "Rejected", "Cancelled"
    bool IsPriority,
    List<string> Tags,
    List<ComplexOrderItem> Items,
    ComplexAddress? ShippingAddress);

// ─── Test class ──────────────────────────────────────────────────────────────

public class ComplexFilterGroupTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static ComplexOrder MakeOrder(
        string? orderId = "ORD-001",
        string? customerId = "CUST-001",
        decimal amount = 500m,
        string? status = "Pending",
        bool isPriority = false,
        List<string>? tags = null,
        List<ComplexOrderItem>? items = null,
        ComplexAddress? shippingAddress = null)
        => new(
            orderId,
            customerId,
            amount,
            status,
            isPriority,
            tags ?? new List<string>(),
            items ?? new List<ComplexOrderItem> { new("P1", 1, 10m) },
            shippingAddress ?? new ComplexAddress("123 Main St", "New York", "US", "10001"));

    private static ComplexAddress MakeAddress(
        string? street = "123 Main St",
        string? city = "New York",
        string? country = "US",
        string? zipCode = "10001")
        => new(street, city, country, zipCode);

    // Creates a ComplexOrder with a null ShippingAddress (bypasses the ?? default in MakeOrder)
    private static ComplexOrder MakeOrderNullAddress(
        string? orderId = "ORD-001",
        string? customerId = "CUST-001",
        decimal amount = 500m,
        string? status = "Pending",
        bool isPriority = false,
        List<string>? tags = null,
        List<ComplexOrderItem>? items = null)
        => new(
            orderId,
            customerId,
            amount,
            status,
            isPriority,
            tags ?? new List<string>(),
            items ?? new List<ComplexOrderItem> { new("P1", 1, 10m) },
            null);

    // ═══════════════════════════════════════════════════════════════════════
    // GROUP 1 — Multi-OR with 3+ AND-groups
    // ═══════════════════════════════════════════════════════════════════════

    // Test 1
    // (Amount > 1000 && IsPriority) || (Amount > 5000 && Status=="Approved") || (CustomerId != null && Status=="Pending")
    [Fact]
    public void ThreeOrGroups_EachWithMultipleAnds_CorrectlyEvaluated()
    {
        var filter = new ValiFlow<ComplexOrder>()
            .Add(o => o.Amount > 1000m)
            .Add(o => o.IsPriority)
            .Or()
            .Add(o => o.Amount > 5000m)
            .Add(o => o.Status == "Approved")
            .Or()
            .Add(o => o.CustomerId != null)
            .Add(o => o.Status == "Pending")
            .Build()
            .Compile();

        // Group 1 true alone: Amount > 1000 && IsPriority
        filter(MakeOrder(amount: 2000m, isPriority: true, status: "Rejected")).Should().BeTrue();

        // Group 2 true alone: Amount > 5000 && Approved
        filter(MakeOrder(amount: 6000m, status: "Approved", isPriority: false)).Should().BeTrue();

        // Group 3 true alone: CustomerId != null && Pending
        filter(MakeOrder(customerId: "C1", status: "Pending", amount: 50m, isPriority: false)).Should().BeTrue();

        // Group 3 false: CustomerId null
        filter(MakeOrder(customerId: null, status: "Pending", amount: 50m, isPriority: false)).Should().BeFalse();

        // All false
        filter(MakeOrder(amount: 100m, isPriority: false, status: "Rejected", customerId: null)).Should().BeFalse();

        // Group 1 and 3 both true
        filter(MakeOrder(amount: 2000m, isPriority: true, customerId: "C1", status: "Pending")).Should().BeTrue();
    }

    // Test 2
    // Four OR-groups each with 2-3 AND conditions across different fields; boundary tests
    [Fact]
    public void FourOrGroups_ComplexMixedFields()
    {
        // G1: Amount == 0 && Status=="Cancelled"
        // G2: IsPriority && Amount >= 10000
        // G3: CustomerId != null && Status=="Rejected" && Amount < 100
        // G4: Tags not empty && Amount > 500
        var filter = new ValiFlow<ComplexOrder>()
            .Add(o => o.Amount == 0m)
            .Add(o => o.Status == "Cancelled")
            .Or()
            .Add(o => o.IsPriority)
            .Add(o => o.Amount >= 10000m)
            .Or()
            .Add(o => o.CustomerId != null)
            .Add(o => o.Status == "Rejected")
            .Add(o => o.Amount < 100m)
            .Or()
            .Add(o => o.Tags.Any())
            .Add(o => o.Amount > 500m)
            .Build()
            .Compile();

        // G1 true: Amount == 0, Cancelled
        filter(MakeOrder(amount: 0m, status: "Cancelled", isPriority: false, tags: new List<string>()))
            .Should().BeTrue();

        // G2 true: Priority, Amount >= 10000
        filter(MakeOrder(isPriority: true, amount: 10000m)).Should().BeTrue();

        // G2 boundary: Amount exactly 9999 → fails G2
        filter(MakeOrder(isPriority: true, amount: 9999m, status: "Cancelled",
                customerId: null, tags: new List<string>()))
            .Should().BeFalse();

        // G3 true
        filter(MakeOrder(customerId: "C", status: "Rejected", amount: 50m, isPriority: false, tags: new List<string>()))
            .Should().BeTrue();

        // G4 true
        filter(MakeOrder(tags: new List<string> { "rush" }, amount: 600m, isPriority: false, status: "Pending"))
            .Should().BeTrue();

        // All false
        filter(MakeOrder(amount: 300m, isPriority: false, status: "Pending", customerId: "C",
                tags: new List<string>()))
            .Should().BeFalse();
    }

    // Test 3
    // Five conditions: A && B && C || D && E — split must happen at the Or, not at arbitrary position
    [Fact]
    public void Or_InMiddleOfLongAndChain_SplitsCorrectly()
    {
        // (Amount > 0 && Status != null && IsPriority) || (CustomerId != null && Amount < 1000)
        var filter = new ValiFlow<ComplexOrder>()
            .Add(o => o.Amount > 0m)
            .Add(o => o.Status != null)
            .Add(o => o.IsPriority)
            .Or()
            .Add(o => o.CustomerId != null)
            .Add(o => o.Amount < 1000m)
            .Build()
            .Compile();

        // Left group true: all three AND pass
        filter(MakeOrder(amount: 1m, status: "Pending", isPriority: true, customerId: null)).Should().BeTrue();

        // Left group partially false (IsPriority = false), right group true
        filter(MakeOrder(amount: 500m, status: "Pending", isPriority: false, customerId: "C")).Should().BeTrue();

        // Left group false (Amount == 0), right group false (CustomerId null)
        filter(MakeOrder(amount: 0m, isPriority: true, status: "Pending", customerId: null)).Should().BeFalse();

        // Both groups true
        filter(MakeOrder(amount: 500m, status: "Pending", isPriority: true, customerId: "C")).Should().BeTrue();
    }

    // Test 4
    // (A && B) || SubGroup(C && D && E) || (F && G)
    [Fact]
    public void Or_FollowedByAddSubGroup_MixedGrouping()
    {
        // G1: Amount > 1000 && IsPriority
        // G2: SubGroup(Status=="Approved" && Amount > 0 && CustomerId != null)
        // G3: Amount > 50000 && Tags.Any()
        var filter = new ValiFlow<ComplexOrder>()
            .Add(o => o.Amount > 1000m)
            .Add(o => o.IsPriority)
            .Or()
            .AddSubGroup(g => g
                .Add(o => o.Status == "Approved")
                .Add(o => o.Amount > 0m)
                .Add(o => o.CustomerId != null))
            .Or()
            .Add(o => o.Amount > 50000m)
            .Add(o => o.Tags.Any())
            .Build()
            .Compile();

        // G1 true
        filter(MakeOrder(amount: 2000m, isPriority: true, status: "Rejected")).Should().BeTrue();

        // G2 true (subgroup)
        filter(MakeOrder(status: "Approved", amount: 1m, customerId: "C", isPriority: false)).Should().BeTrue();

        // G2 partially false: CustomerId null
        filter(MakeOrder(status: "Approved", amount: 1m, customerId: null, isPriority: false,
                tags: new List<string>()))
            .Should().BeFalse();

        // G3 true
        filter(MakeOrder(amount: 60000m, tags: new List<string> { "big" }, isPriority: false, status: "Pending"))
            .Should().BeTrue();

        // All false
        filter(MakeOrder(amount: 100m, isPriority: false, status: "Pending", customerId: null,
                tags: new List<string>()))
            .Should().BeFalse();
    }

    // Test 5
    // A.Add(B).Or().Add(C).Add(D).Or().Add(E).Add(F) → (A&&B) || (C&&D) || (E&&F)
    [Fact]
    public void Consecutive_Or_Between_And_Groups_NoConditionLost()
    {
        // G1: Amount > 10 && IsPriority
        // G2: Status=="Approved" && Amount > 100
        // G3: CustomerId != null && Amount < 5000
        var filter = new ValiFlow<ComplexOrder>()
            .Add(o => o.Amount > 10m)
            .Add(o => o.IsPriority)
            .Or()
            .Add(o => o.Status == "Approved")
            .Add(o => o.Amount > 100m)
            .Or()
            .Add(o => o.CustomerId != null)
            .Add(o => o.Amount < 5000m)
            .Build()
            .Compile();

        // Only G1 passes
        filter(MakeOrder(amount: 50m, isPriority: true, status: "Rejected", customerId: null)).Should().BeTrue();

        // Only G2 passes
        filter(MakeOrder(status: "Approved", amount: 200m, isPriority: false, customerId: null)).Should().BeTrue();

        // Only G3 passes
        filter(MakeOrder(customerId: "C", amount: 100m, isPriority: false, status: "Pending")).Should().BeTrue();

        // None passes
        filter(MakeOrder(amount: 5m, isPriority: false, status: "Pending", customerId: null)).Should().BeFalse();

        // G1 and G3 both pass
        filter(MakeOrder(amount: 50m, isPriority: true, customerId: "C")).Should().BeTrue();

        // G2 boundary: Amount == 100 is NOT > 100
        filter(MakeOrder(status: "Approved", amount: 100m, isPriority: false, customerId: null)).Should().BeFalse();
    }

    // Test 6
    // OR groups with AddIf — some conditions activate only when flag is true
    [Fact]
    public void Or_WithAddIf_ConditionallyActiveGroups()
    {
        bool enforcePriorityCheck = true;
        bool enforceCustomerCheck = false;

        // (IsPriority [if flag]) || (Amount > 5000) || (CustomerId != null [if flag2])
        var filter = new ValiFlow<ComplexOrder>()
            .AddIf(enforcePriorityCheck, o => o.IsPriority)
            .Or()
            .Add(o => o.Amount > 5000m)
            .Or()
            .AddIf(enforceCustomerCheck, o => o.CustomerId != null)
            .Build()
            .Compile();

        // G1 active (flag true), IsPriority=true → true
        filter(MakeOrder(isPriority: true, amount: 100m)).Should().BeTrue();

        // G1 active, IsPriority=false, G2 Amount > 5000 → true
        filter(MakeOrder(isPriority: false, amount: 6000m)).Should().BeTrue();

        // G3 not active (flag false), G1 false, G2 false → false
        filter(MakeOrder(isPriority: false, amount: 100m, customerId: "C")).Should().BeFalse();

        // Now disable flag 1, enable flag 2
        var filter2 = new ValiFlow<ComplexOrder>()
            .AddIf(false, o => o.IsPriority)
            .Or()
            .Add(o => o.Amount > 5000m)
            .Or()
            .AddIf(true, o => o.CustomerId != null)
            .Build()
            .Compile();

        // G1 not active, G3 active with customerId
        filter2(MakeOrder(isPriority: false, amount: 100m, customerId: "C")).Should().BeTrue();

        // G1 not active, G3 active but null customer, G2 false
        filter2(MakeOrder(isPriority: false, amount: 100m, customerId: null)).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GROUP 2 — AddSubGroup deep nesting
    // ═══════════════════════════════════════════════════════════════════════

    // Test 7
    // Outer: A || SubGroup(B && SubGroup(C || D)) — two levels of nesting
    [Fact]
    public void NestedSubGroups_TwoLevels()
    {
        // IsPriority || SubGroup(Amount > 0 && SubGroup(Status=="Approved" || Status=="Pending"))
        var filter = new ValiFlow<ComplexOrder>()
            .Add(o => o.IsPriority)
            .Or()
            .AddSubGroup(outer => outer
                .Add(o => o.Amount > 0m)
                .AddSubGroup(inner => inner
                    .Add(o => o.Status == "Approved")
                    .Or()
                    .Add(o => o.Status == "Pending")))
            .Build()
            .Compile();

        // Outer A true
        filter(MakeOrder(isPriority: true, amount: 0m, status: "Rejected")).Should().BeTrue();

        // Inner subgroup: Amount > 0 && (Approved || Pending)
        filter(MakeOrder(isPriority: false, amount: 100m, status: "Approved")).Should().BeTrue();
        filter(MakeOrder(isPriority: false, amount: 100m, status: "Pending")).Should().BeTrue();

        // Inner subgroup: Amount > 0 but status is Rejected → false
        filter(MakeOrder(isPriority: false, amount: 100m, status: "Rejected")).Should().BeFalse();

        // Inner subgroup: Amount == 0 → false
        filter(MakeOrder(isPriority: false, amount: 0m, status: "Approved")).Should().BeFalse();
    }

    // Test 8
    // A && SubGroup(B || C) && D — subgroup in the middle of an AND chain
    [Fact]
    public void SubGroup_Combined_WithOr_And_MainConditions()
    {
        // Amount > 0 && SubGroup(Status=="Approved" || IsPriority) && CustomerId != null
        var filter = new ValiFlow<ComplexOrder>()
            .Add(o => o.Amount > 0m)
            .AddSubGroup(g => g
                .Add(o => o.Status == "Approved")
                .Or()
                .Add(o => o.IsPriority))
            .Add(o => o.CustomerId != null)
            .Build()
            .Compile();

        // All three pass (Status == Approved)
        filter(MakeOrder(amount: 100m, status: "Approved", customerId: "C", isPriority: false)).Should().BeTrue();

        // All three pass (IsPriority instead)
        filter(MakeOrder(amount: 100m, status: "Rejected", customerId: "C", isPriority: true)).Should().BeTrue();

        // SubGroup fails (neither Approved nor IsPriority)
        filter(MakeOrder(amount: 100m, status: "Rejected", customerId: "C", isPriority: false)).Should().BeFalse();

        // Amount fails
        filter(MakeOrder(amount: 0m, status: "Approved", customerId: "C", isPriority: false)).Should().BeFalse();

        // CustomerId fails
        filter(MakeOrder(amount: 100m, status: "Approved", customerId: null, isPriority: false)).Should().BeFalse();
    }

    // Test 9
    // SubGroup1(A && B) || SubGroup2(C && D) || SubGroup3(E && F && G)
    [Fact]
    public void MultipleSubGroups_AllConnectedWithOr()
    {
        // SG1: Amount > 100 && IsPriority
        // SG2: Status=="Approved" && CustomerId != null
        // SG3: Amount > 1000 && Status != null && Tags.Any()
        var filter = new ValiFlow<ComplexOrder>()
            .AddSubGroup(g => g
                .Add(o => o.Amount > 100m)
                .Add(o => o.IsPriority))
            .Or()
            .AddSubGroup(g => g
                .Add(o => o.Status == "Approved")
                .Add(o => o.CustomerId != null))
            .Or()
            .AddSubGroup(g => g
                .Add(o => o.Amount > 1000m)
                .Add(o => o.Status != null)
                .Add(o => o.Tags.Any()))
            .Build()
            .Compile();

        // SG1 true
        filter(MakeOrder(amount: 200m, isPriority: true, status: "Rejected", customerId: null,
                tags: new List<string>()))
            .Should().BeTrue();

        // SG2 true
        filter(MakeOrder(amount: 50m, status: "Approved", customerId: "C", isPriority: false,
                tags: new List<string>()))
            .Should().BeTrue();

        // SG3 true
        filter(MakeOrder(amount: 2000m, status: "Pending", tags: new List<string> { "hot" },
                isPriority: false, customerId: null))
            .Should().BeTrue();

        // All false
        filter(MakeOrder(amount: 50m, isPriority: false, status: "Rejected", customerId: null,
                tags: new List<string>()))
            .Should().BeFalse();
    }

    // Test 10
    // SubGroup with ValidateNested inside subgroup (address fields validated)
    [Fact]
    public void SubGroup_WithValidateNested_InsideSubGroup()
    {
        // SubGroup(IsPriority && ValidateNested(addr => addr.City is not empty)) || Amount > 50000
        var filter = new ValiFlow<ComplexOrder>()
            .AddSubGroup(g => g
                .Add(o => o.IsPriority)
                .ValidateNested(o => o.ShippingAddress, addr => addr.IsNotNullOrEmpty(a => a.City)))
            .Or()
            .Add(o => o.Amount > 50000m)
            .Build()
            .Compile();

        // SubGroup true: priority + valid city
        filter(MakeOrder(isPriority: true, shippingAddress: MakeAddress(city: "NYC"), amount: 100m))
            .Should().BeTrue();

        // SubGroup false (city empty), but Amount > 50000 true
        filter(MakeOrder(isPriority: true, shippingAddress: MakeAddress(city: ""), amount: 60000m))
            .Should().BeTrue();

        // SubGroup false (not priority), Amount false
        filter(MakeOrder(isPriority: false, shippingAddress: MakeAddress(city: "NYC"), amount: 100m))
            .Should().BeFalse();

        // SubGroup false (null address), Amount false
        filter(MakeOrderNullAddress(isPriority: true, amount: 100m))
            .Should().BeFalse();
    }

    // Test 11
    // SubGroup(Status IN ["Approved","Pending"] && Tags not empty) || SubGroup(Amount > 10000)
    [Fact]
    public void SubGroup_Combined_WithIn_NotIn_Collections()
    {
        var validStatuses = new[] { "Approved", "Pending" };

        var filter = new ValiFlow<ComplexOrder>()
            .AddSubGroup(g => g
                .Add(o => validStatuses.Contains(o.Status))
                .Add(o => o.Tags.Any()))
            .Or()
            .AddSubGroup(g => g
                .Add(o => o.Amount > 10000m))
            .Build()
            .Compile();

        // SG1 true: valid status + tags
        filter(MakeOrder(status: "Approved", tags: new List<string> { "a" }, amount: 100m))
            .Should().BeTrue();

        // SG1 partially false: valid status but no tags
        filter(MakeOrder(status: "Pending", tags: new List<string>(), amount: 100m))
            .Should().BeFalse();

        // SG2 true
        filter(MakeOrder(status: "Rejected", tags: new List<string>(), amount: 15000m))
            .Should().BeTrue();

        // Both false
        filter(MakeOrder(status: "Rejected", tags: new List<string>(), amount: 5000m))
            .Should().BeFalse();

        // NotIn variant: status not in invalid list
        var filter2 = new ValiFlow<ComplexOrder>()
            .NotIn(o => o.Status, new[] { "Cancelled", "Rejected" })
            .Build()
            .Compile();

        filter2(MakeOrder(status: "Approved")).Should().BeTrue();
        filter2(MakeOrder(status: "Cancelled")).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GROUP 3 — When / Unless complex
    // ═══════════════════════════════════════════════════════════════════════

    // Test 12
    // When(IsPriority, then => Amount > 100 && Status=="Approved")
    [Fact]
    public void When_WithMultipleConditions_InThenBlock()
    {
        // IsPriority => (Amount > 100 && Status == "Approved")
        var filter = new ValiFlow<ComplexOrder>()
            .When(o => o.IsPriority, then => then
                .Add(o => o.Amount > 100m)
                .Add(o => o.Status == "Approved"))
            .Build()
            .Compile();

        // IsPriority=true, all conditions pass
        filter(MakeOrder(isPriority: true, amount: 200m, status: "Approved")).Should().BeTrue();

        // IsPriority=true, Amount fails
        filter(MakeOrder(isPriority: true, amount: 50m, status: "Approved")).Should().BeFalse();

        // IsPriority=true, Status fails
        filter(MakeOrder(isPriority: true, amount: 200m, status: "Pending")).Should().BeFalse();

        // IsPriority=false → When condition false, entire When clause is false
        filter(MakeOrder(isPriority: false, amount: 50m, status: "Rejected")).Should().BeFalse();
    }

    // Test 13
    // Unless(Status=="Cancelled", then => Amount > 0 && CustomerId != null)
    [Fact]
    public void Unless_WithComplexCondition()
    {
        // !Cancelled => (Amount > 0 && CustomerId != null)
        var filter = new ValiFlow<ComplexOrder>()
            .Unless(o => o.Status == "Cancelled", unless => unless
                .Add(o => o.Amount > 0m)
                .Add(o => o.CustomerId != null))
            .Build()
            .Compile();

        // Not cancelled, amount > 0, customer present → true
        filter(MakeOrder(status: "Pending", amount: 100m, customerId: "C")).Should().BeTrue();

        // Not cancelled but amount == 0 → false
        filter(MakeOrder(status: "Approved", amount: 0m, customerId: "C")).Should().BeFalse();

        // Not cancelled but customer null → false
        filter(MakeOrder(status: "Approved", amount: 100m, customerId: null)).Should().BeFalse();

        // Cancelled → Unless fires as false (condition is true, so !condition is false)
        filter(MakeOrder(status: "Cancelled", amount: 100m, customerId: "C")).Should().BeFalse();
    }

    // Test 14
    // A.Or().When(condition, then => B.C).Or().D
    [Fact]
    public void When_CombinedWith_Or_Groups()
    {
        // Amount > 10000 || (IsPriority => Status=="Approved" && Amount > 0) || CustomerId != null
        var filter = new ValiFlow<ComplexOrder>()
            .Add(o => o.Amount > 10000m)
            .Or()
            .When(o => o.IsPriority, then => then
                .Add(o => o.Status == "Approved")
                .Add(o => o.Amount > 0m))
            .Or()
            .Add(o => o.CustomerId != null)
            .Build()
            .Compile();

        // First OR passes
        filter(MakeOrder(amount: 15000m, isPriority: false, status: "Rejected", customerId: null))
            .Should().BeTrue();

        // Second OR passes: IsPriority + Approved + Amount > 0
        filter(MakeOrder(amount: 500m, isPriority: true, status: "Approved", customerId: null))
            .Should().BeTrue();

        // Third OR passes
        filter(MakeOrder(amount: 100m, isPriority: false, status: "Pending", customerId: "C"))
            .Should().BeTrue();

        // All false: amount small, priority but wrong status, no customer
        filter(MakeOrder(amount: 100m, isPriority: true, status: "Pending", customerId: null))
            .Should().BeFalse();
    }

    // Test 15
    // When and Unless both present targeting same Order
    [Fact]
    public void When_And_Unless_BothPresent()
    {
        // When(IsPriority, Amount > 1000) AND Unless(Status=="Cancelled", CustomerId != null)
        var filter = new ValiFlow<ComplexOrder>()
            .When(o => o.IsPriority, then => then.Add(o => o.Amount > 1000m))
            .And()
            .Unless(o => o.Status == "Cancelled", unless => unless.Add(o => o.CustomerId != null))
            .Build()
            .Compile();

        // Both When and Unless pass
        filter(MakeOrder(isPriority: true, amount: 2000m, status: "Pending", customerId: "C"))
            .Should().BeTrue();

        // When fails (amount too small)
        filter(MakeOrder(isPriority: true, amount: 500m, status: "Pending", customerId: "C"))
            .Should().BeFalse();

        // Unless fails (status Cancelled, so !Cancelled is false)
        filter(MakeOrder(isPriority: true, amount: 2000m, status: "Cancelled", customerId: "C"))
            .Should().BeFalse();

        // IsPriority false → When evaluates as false
        filter(MakeOrder(isPriority: false, amount: 2000m, status: "Pending", customerId: "C"))
            .Should().BeFalse();
    }

    // Test 16
    // When(IsPriority, then => AddSubGroup(X || Y))
    [Fact]
    public void When_WithNestedSubGroup_In_Then()
    {
        // IsPriority => SubGroup(Status=="Approved" || Amount > 5000)
        var filter = new ValiFlow<ComplexOrder>()
            .When(o => o.IsPriority, then => then
                .AddSubGroup(g => g
                    .Add(o => o.Status == "Approved")
                    .Or()
                    .Add(o => o.Amount > 5000m)))
            .Build()
            .Compile();

        // IsPriority + Status Approved
        filter(MakeOrder(isPriority: true, status: "Approved", amount: 100m)).Should().BeTrue();

        // IsPriority + Amount > 5000 (not approved)
        filter(MakeOrder(isPriority: true, status: "Rejected", amount: 6000m)).Should().BeTrue();

        // IsPriority but subgroup fails
        filter(MakeOrder(isPriority: true, status: "Rejected", amount: 100m)).Should().BeFalse();

        // Not priority → When is false
        filter(MakeOrder(isPriority: false, status: "Approved", amount: 6000m)).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GROUP 4 — ValidateNested complex
    // ═══════════════════════════════════════════════════════════════════════

    // Test 17
    // When(Items.Any(), ValidateNested(address, city + street not empty))
    [Fact]
    public void ValidateNested_AddressRequired_WhenOrderHasItems()
    {
        // Items.Any() => (ShippingAddress.City != empty && ShippingAddress.Street != empty)
        var filter = new ValiFlow<ComplexOrder>()
            .When(
                o => o.Items.Any(),
                then => then.ValidateNested(
                    o => o.ShippingAddress,
                    addr => addr
                        .IsNotNullOrEmpty(a => a.City)
                        .And()
                        .IsNotNullOrEmpty(a => a.Street)))
            .Build()
            .Compile();

        // Has items + valid address
        filter(MakeOrder(
            items: new List<ComplexOrderItem> { new("P1", 1, 10m) },
            shippingAddress: MakeAddress(city: "NYC", street: "5th Ave"))).Should().BeTrue();

        // Has items + empty city → false
        filter(MakeOrder(
            items: new List<ComplexOrderItem> { new("P1", 1, 10m) },
            shippingAddress: MakeAddress(city: "", street: "5th Ave"))).Should().BeFalse();

        // No items → When condition false → false
        filter(MakeOrder(
            items: new List<ComplexOrderItem>(),
            shippingAddress: MakeAddress(city: "", street: ""))).Should().BeFalse();
    }

    // Test 18
    // ValidateNested with ShippingAddress=null → false automatically (null check)
    [Fact]
    public void ValidateNested_NullAddress_FailsAutomatically()
    {
        var filter = new ValiFlow<ComplexOrder>()
            .ValidateNested(o => o.ShippingAddress, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build()
            .Compile();

        // Null address → fails the implicit null check
        filter(MakeOrderNullAddress()).Should().BeFalse();

        // Valid address → passes
        filter(MakeOrder(shippingAddress: MakeAddress(city: "Paris"))).Should().BeTrue();

        // Address not null but empty city → fails inner condition
        filter(MakeOrder(shippingAddress: MakeAddress(city: ""))).Should().BeFalse();
    }

    // Test 19
    // ValidateNested(addr) || Amount > 50000
    [Fact]
    public void ValidateNested_Combined_WithOr_Groups()
    {
        var filter = new ValiFlow<ComplexOrder>()
            .ValidateNested(o => o.ShippingAddress, addr => addr
                .IsNotNullOrEmpty(a => a.City)
                .And()
                .IsNotNullOrEmpty(a => a.ZipCode))
            .Or()
            .Add(o => o.Amount > 50000m)
            .Build()
            .Compile();

        // Address valid (city + zip) → true
        filter(MakeOrder(shippingAddress: MakeAddress(city: "NYC", zipCode: "10001"), amount: 100m))
            .Should().BeTrue();

        // Address invalid (no zip) but Amount > 50000 → true
        filter(MakeOrder(shippingAddress: MakeAddress(city: "NYC", zipCode: ""), amount: 60000m))
            .Should().BeTrue();

        // Address null → null check fails, Amount not big enough → false
        filter(MakeOrderNullAddress(amount: 100m)).Should().BeFalse();

        // Both fail
        filter(MakeOrder(shippingAddress: MakeAddress(city: ""), amount: 100m)).Should().BeFalse();
    }

    // Test 20
    // ValidateNested with WithError with PropertyPath, check PropertyPath in result
    [Fact]
    public void ValidateNested_WithPropertyPath_InValidate()
    {
        var builder = new ValiFlow<ComplexOrder>()
            .ValidateNested(o => o.ShippingAddress, addr => addr.IsNotNullOrEmpty(a => a.City))
            .WithError("ADDR_001", "City is required", "ShippingAddress.City");

        // Failing instance
        var result = builder.Validate(MakeOrder(shippingAddress: MakeAddress(city: "")));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].ErrorCode.Should().Be("ADDR_001");
        result.Errors[0].Message.Should().Be("City is required");
        result.Errors[0].PropertyPath.Should().Be("ShippingAddress.City");

        // Passing instance
        var okResult = builder.Validate(MakeOrder(shippingAddress: MakeAddress(city: "London")));
        okResult.IsValid.Should().BeTrue();
        okResult.Errors.Should().BeEmpty();
    }

    // Test 21
    // ValidateNested two levels: outer order fields AND nested address fields
    [Fact]
    public void ValidateNested_TwoLevels_OrderAndAddress()
    {
        var filter = new ValiFlow<ComplexOrder>()
            .Add(o => o.Amount > 0m)
            .Add(o => o.CustomerId != null)
            .ValidateNested(o => o.ShippingAddress, addr => addr
                .IsNotNullOrEmpty(a => a.City)
                .And()
                .IsNotNullOrEmpty(a => a.Country))
            .Build()
            .Compile();

        // All pass
        filter(MakeOrder(amount: 100m, customerId: "C",
            shippingAddress: MakeAddress(city: "Berlin", country: "DE"))).Should().BeTrue();

        // Amount fails
        filter(MakeOrder(amount: 0m, customerId: "C",
            shippingAddress: MakeAddress(city: "Berlin", country: "DE"))).Should().BeFalse();

        // CustomerId fails
        filter(MakeOrder(amount: 100m, customerId: null,
            shippingAddress: MakeAddress(city: "Berlin", country: "DE"))).Should().BeFalse();

        // Nested city empty
        filter(MakeOrder(amount: 100m, customerId: "C",
            shippingAddress: MakeAddress(city: "", country: "DE"))).Should().BeFalse();

        // Nested country empty
        filter(MakeOrder(amount: 100m, customerId: "C",
            shippingAddress: MakeAddress(city: "Berlin", country: ""))).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GROUP 5 — Collection methods complex
    // ═══════════════════════════════════════════════════════════════════════

    // Test 22
    // EachItem(all items have Quantity > 0) || IsPriority
    [Fact]
    public void EachItem_CombinedWith_Or_OuterCondition()
    {
        // EachItem(Quantity > 0) || IsPriority
        var filter = new ValiFlow<ComplexOrder>()
            .EachItem(o => o.Items, item => item.Add(i => i.Quantity > 0))
            .Or()
            .Add(o => o.IsPriority)
            .Build()
            .Compile();

        // All items have Quantity > 0
        filter(MakeOrder(
            items: new List<ComplexOrderItem> { new("P1", 1, 5m), new("P2", 3, 10m) },
            isPriority: false)).Should().BeTrue();

        // One item has Quantity == 0, but IsPriority → true via second OR
        filter(MakeOrder(
            items: new List<ComplexOrderItem> { new("P1", 0, 5m) },
            isPriority: true)).Should().BeTrue();

        // Item with Quantity == 0, not priority → false
        filter(MakeOrder(
            items: new List<ComplexOrderItem> { new("P1", 0, 5m), new("P2", 1, 10m) },
            isPriority: false)).Should().BeFalse();
    }

    // Test 23
    // EachItem(all UnitPrice > 0) AND AnyItem(at least one Quantity > 10)
    [Fact]
    public void AnyItem_And_EachItem_SameBuilder()
    {
        // EachItem(UnitPrice > 0) && AnyItem(Quantity > 10)
        var filter = new ValiFlow<ComplexOrder>()
            .EachItem(o => o.Items, item => item.Add(i => i.UnitPrice > 0m))
            .And()
            .AnyItem(o => o.Items, item => item.Add(i => i.Quantity > 10))
            .Build()
            .Compile();

        // All prices valid, at least one with Quantity > 10
        filter(MakeOrder(items: new List<ComplexOrderItem>
        {
            new("P1", 15, 5m),
            new("P2", 2, 20m)
        })).Should().BeTrue();

        // All prices valid, but no item has Quantity > 10
        filter(MakeOrder(items: new List<ComplexOrderItem>
        {
            new("P1", 5, 5m),
            new("P2", 3, 20m)
        })).Should().BeFalse();

        // One price is 0 → EachItem fails even though AnyItem would pass
        filter(MakeOrder(items: new List<ComplexOrderItem>
        {
            new("P1", 15, 0m),
            new("P2", 2, 20m)
        })).Should().BeFalse();
    }

    // Test 24
    // SubGroup(Tags.Count >= 2 && Tags contains "urgent") || SubGroup(all Items Quantity > 5)
    [Fact]
    public void Complex_Collection_In_SubGroup()
    {
        var filter = new ValiFlow<ComplexOrder>()
            .AddSubGroup(g => g
                .Add(o => o.Tags.Count >= 2)
                .Add(o => o.Tags.Contains("urgent")))
            .Or()
            .AddSubGroup(g => g
                .Add(o => o.Items.All(i => i.Quantity > 5)))
            .Build()
            .Compile();

        // SG1 true: 2+ tags including "urgent"
        filter(MakeOrder(
            tags: new List<string> { "urgent", "express" },
            items: new List<ComplexOrderItem> { new("P1", 1, 5m) })).Should().BeTrue();

        // SG1 false (only 1 tag), SG2 true (all quantities > 5)
        filter(MakeOrder(
            tags: new List<string> { "urgent" },
            items: new List<ComplexOrderItem> { new("P1", 10, 5m), new("P2", 6, 10m) })).Should().BeTrue();

        // SG1 false (has 2 tags but not "urgent"), SG2 false (quantity <= 5)
        filter(MakeOrder(
            tags: new List<string> { "normal", "slow" },
            items: new List<ComplexOrderItem> { new("P1", 1, 5m) })).Should().BeFalse();
    }

    // Test 25
    // (!HasDuplicates(Tags) && Items.Any()) || IsPriority
    [Fact]
    public void HasDuplicates_Tags_Combined_With_Or()
    {
        // No-duplicate-tags + Items.Any() || IsPriority
        var filter = new ValiFlow<ComplexOrder>()
            .AddSubGroup(g => g
                .Add(o => !o.Tags.GroupBy(t => t).Any(grp => grp.Count() > 1))  // no duplicates
                .NotEmpty<ComplexOrderItem>(o => o.Items))
            .Or()
            .Add(o => o.IsPriority)
            .Build()
            .Compile();

        // No duplicates + has items → true
        filter(MakeOrder(
            tags: new List<string> { "a", "b", "c" },
            items: new List<ComplexOrderItem> { new("P1", 1, 5m) },
            isPriority: false)).Should().BeTrue();

        // Has duplicates + has items, but IsPriority → true via second group
        filter(MakeOrder(
            tags: new List<string> { "a", "a" },
            items: new List<ComplexOrderItem> { new("P1", 1, 5m) },
            isPriority: true)).Should().BeTrue();

        // Has duplicates, not priority → false
        filter(MakeOrder(
            tags: new List<string> { "a", "a" },
            items: new List<ComplexOrderItem> { new("P1", 1, 5m) },
            isPriority: false)).Should().BeFalse();

        // No duplicates but empty items, not priority → false
        filter(MakeOrder(
            tags: new List<string> { "a", "b" },
            items: new List<ComplexOrderItem>(),
            isPriority: false)).Should().BeFalse();
    }

    // Test 26
    // SubGroup(Items.MinCount(1) && Items.MaxCount(5)) || Amount > 100000
    [Fact]
    public void MinCount_MaxCount_Combined_With_Or()
    {
        var filter = new ValiFlow<ComplexOrder>()
            .AddSubGroup(g => g
                .Add(o => o.Items.Count >= 1)
                .Add(o => o.Items.Count <= 5))
            .Or()
            .Add(o => o.Amount > 100000m)
            .Build()
            .Compile();

        // SG: 1-5 items
        filter(MakeOrder(items: new List<ComplexOrderItem> { new("P1", 1, 5m) }, amount: 100m))
            .Should().BeTrue();

        filter(MakeOrder(items: Enumerable.Range(1, 5)
            .Select(i => new ComplexOrderItem($"P{i}", i, 10m)).ToList(), amount: 100m))
            .Should().BeTrue();

        // 6 items → SG fails, but Amount > 100000 → true
        filter(MakeOrder(items: Enumerable.Range(1, 6)
            .Select(i => new ComplexOrderItem($"P{i}", i, 10m)).ToList(), amount: 150000m))
            .Should().BeTrue();

        // 6 items, Amount small → false
        filter(MakeOrder(items: Enumerable.Range(1, 6)
            .Select(i => new ComplexOrderItem($"P{i}", i, 10m)).ToList(), amount: 100m))
            .Should().BeFalse();

        // 0 items → MinCount fails, Amount small → false
        filter(MakeOrder(items: new List<ComplexOrderItem>(), amount: 100m))
            .Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GROUP 6 — Validate() with errors and complex rules
    // ═══════════════════════════════════════════════════════════════════════

    // Test 27
    // Builder with 5+ WithError rules; instance fails exactly 3. Verify exact errors.
    [Fact]
    public void Validate_ComplexRule_MultipleErrors_SingleInstance()
    {
        var builder = new ValiFlow<ComplexOrder>()
            .Add(o => o.OrderId != null)
            .WithError("ORD_001", "OrderId is required")
            .Add(o => o.CustomerId != null)
            .WithError("ORD_002", "CustomerId is required")
            .Add(o => o.Amount > 0m)
            .WithError("ORD_003", "Amount must be positive")
            .Add(o => o.Status != null)
            .WithError("ORD_004", "Status is required")
            .Add(o => o.Items.Any())
            .WithError("ORD_005", "At least one item is required");

        // Instance fails: CustomerId null, Amount == 0, Items empty
        var order = new ComplexOrder("ORD-1", null, 0m, "Pending", false,
            new List<string>(), new List<ComplexOrderItem>(), null);

        var result = builder.Validate(order);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Select(e => e.ErrorCode).Should().Contain("ORD_002");
        result.Errors.Select(e => e.ErrorCode).Should().Contain("ORD_003");
        result.Errors.Select(e => e.ErrorCode).Should().Contain("ORD_005");
        result.Errors.Select(e => e.ErrorCode).Should().NotContain("ORD_001");
        result.Errors.Select(e => e.ErrorCode).Should().NotContain("ORD_004");
    }

    // Test 28
    // ValidateAll with 10 instances, complex rules, verify each result individually
    [Fact]
    public void ValidateAll_MixedBatch_ComplexRules()
    {
        var builder = new ValiFlow<ComplexOrder>()
            .Add(o => o.Amount > 0m)
            .WithError("E001", "Amount must be > 0")
            .Add(o => o.CustomerId != null)
            .WithError("E002", "CustomerId required")
            .Add(o => o.Status != null)
            .WithError("E003", "Status required");

        var orders = new List<ComplexOrder>
        {
            MakeOrder(amount: 100m, customerId: "C1", status: "Pending"),    // 0: all pass
            MakeOrder(amount: 0m, customerId: "C2", status: "Pending"),      // 1: amount fails
            MakeOrder(amount: 100m, customerId: null, status: "Pending"),    // 2: customer fails
            MakeOrder(amount: 100m, customerId: "C4", status: null),         // 3: status fails
            MakeOrder(amount: 0m, customerId: null, status: "Approved"),     // 4: amount+customer fail
            MakeOrder(amount: 0m, customerId: "C6", status: null),           // 5: amount+status fail
            MakeOrder(amount: 100m, customerId: null, status: null),         // 6: customer+status fail
            MakeOrder(amount: 0m, customerId: null, status: null),           // 7: all fail
            MakeOrder(amount: 500m, customerId: "C9", status: "Approved"),   // 8: all pass
            MakeOrder(amount: 1m, customerId: "C10", status: "Rejected"),    // 9: all pass
        };

        var results = builder.ValidateAll(orders).Select(r => r.Result).ToList();

        results[0].IsValid.Should().BeTrue();
        results[1].IsValid.Should().BeFalse();
        results[1].Errors.Select(e => e.ErrorCode).Should().Contain("E001");
        results[2].IsValid.Should().BeFalse();
        results[2].Errors.Select(e => e.ErrorCode).Should().Contain("E002");
        results[3].IsValid.Should().BeFalse();
        results[3].Errors.Select(e => e.ErrorCode).Should().Contain("E003");
        results[4].Errors.Should().HaveCount(2);
        results[5].Errors.Should().HaveCount(2);
        results[6].Errors.Should().HaveCount(2);
        results[7].Errors.Should().HaveCount(3);
        results[8].IsValid.Should().BeTrue();
        results[9].IsValid.Should().BeTrue();
    }

    // Test 29
    // Each condition has its own WithError; verify only failing conditions produce errors
    [Fact]
    public void Validate_Or_GroupsWithErrors_PerCondition()
    {
        // Three independent conditions each with WithError (evaluated individually by Validate)
        var builder = new ValiFlow<ComplexOrder>()
            .Add(o => o.IsPriority)
            .WithError("G1", "Must be priority")
            .Add(o => o.Amount > 500m)
            .WithError("G2", "Amount must exceed 500")
            .Add(o => o.CustomerId != null)
            .WithError("G3", "CustomerId required");

        // Instance where G1 and G2 fail, G3 passes
        var order = MakeOrder(isPriority: false, amount: 100m, customerId: "C");

        var result = builder.Validate(order);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Select(e => e.ErrorCode).Should().Contain("G1");
        result.Errors.Select(e => e.ErrorCode).Should().Contain("G2");
        result.Errors.Select(e => e.ErrorCode).Should().NotContain("G3");

        // All pass
        var goodOrder = MakeOrder(isPriority: true, amount: 1000m, customerId: "C");
        builder.Validate(goodOrder).IsValid.Should().BeTrue();
    }

    // Test 30
    // Three conditions with different PropertyPaths; instance fails all
    [Fact]
    public void Validate_WithPropertyPath_MultipleFields()
    {
        var builder = new ValiFlow<ComplexOrder>()
            .Add(o => o.OrderId != null)
            .WithError("F001", "OrderId required", "OrderId")
            .Add(o => o.Amount > 0m)
            .WithError("F002", "Amount must be positive", "Amount")
            .ValidateNested(o => o.ShippingAddress, addr => addr.IsNotNullOrEmpty(a => a.City))
            .WithError("F003", "City required", "ShippingAddress.City");

        // Instance fails all three
        var order = new ComplexOrder(null, "C", 0m, "Pending", false,
            new List<string>(), new List<ComplexOrderItem> { new("P1", 1, 5m) },
            new ComplexAddress("Street", "", "US", "12345"));

        var result = builder.Validate(order);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);

        var f001 = result.Errors.First(e => e.ErrorCode == "F001");
        f001.PropertyPath.Should().Be("OrderId");

        var f002 = result.Errors.First(e => e.ErrorCode == "F002");
        f002.PropertyPath.Should().Be("Amount");

        var f003 = result.Errors.First(e => e.ErrorCode == "F003");
        f003.PropertyPath.Should().Be("ShippingAddress.City");

        // Passing instance
        var goodOrder = new ComplexOrder("ORD-1", "C", 100m, "Pending", false,
            new List<string>(), new List<ComplexOrderItem> { new("P1", 1, 5m) },
            new ComplexAddress("Street", "NYC", "US", "12345"));
        builder.Validate(goodOrder).IsValid.Should().BeTrue();
    }
}
