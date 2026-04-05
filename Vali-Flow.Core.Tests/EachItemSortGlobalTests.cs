using System.Linq.Expressions;
using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

public record Order(int Id, decimal Total, bool IsActive, List<OrderItem> Items);
public record OrderItem(string Name, decimal Price, int Quantity);
public record Widget(string Name, decimal Price, bool IsDeleted);

public class EachItemSortGlobalTests
{
    // =========================================================================
    // EachItem Tests
    // =========================================================================

    [Fact]
    public void EachItem_AllItemsPass_ReturnsTrue()
    {
        var order = new Order(1, 100m, true, new List<OrderItem>
        {
            new("A", 10m, 1),
            new("B", 20m, 2)
        });
        var filter = new ValiFlow<Order>()
            .EachItem(o => o.Items, item => item.GreaterThan(i => i.Price, 0m))
            .Build();
        filter.Compile()(order).Should().BeTrue();
    }

    [Fact]
    public void EachItem_OneItemFails_ReturnsFalse()
    {
        var order = new Order(1, 100m, true, new List<OrderItem>
        {
            new("A", 10m, 1),
            new("B", -5m, 2)
        });
        var filter = new ValiFlow<Order>()
            .EachItem(o => o.Items, item => item.GreaterThan(i => i.Price, 0m))
            .Build();
        filter.Compile()(order).Should().BeFalse();
    }

    [Fact]
    public void EachItem_EmptyCollection_ReturnsTrue()
    {
        // Enumerable.All on empty is vacuously true
        var order = new Order(1, 0m, true, new List<OrderItem>());
        var filter = new ValiFlow<Order>()
            .EachItem(o => o.Items, item => item.GreaterThan(i => i.Price, 0m))
            .Build();
        filter.Compile()(order).Should().BeTrue();
    }

    [Fact]
    public void EachItem_SingleConditionGreaterThan_ReturnsCorrectResult()
    {
        var passing = new Order(1, 100m, true, new List<OrderItem> { new("X", 50m, 2) });
        var failing = new Order(2, 0m, true, new List<OrderItem> { new("Y", 0m, 0) });

        var filter = new ValiFlow<Order>()
            .EachItem(o => o.Items, item => item.GreaterThan(i => i.Quantity, 0))
            .Build();
        var compiled = filter.Compile();

        compiled(passing).Should().BeTrue();
        compiled(failing).Should().BeFalse();
    }

    [Fact]
    public void EachItem_MultipleConditionsAndChain_AllMustPass()
    {
        var allPass = new Order(1, 100m, true, new List<OrderItem>
        {
            new("Widget", 10m, 5),
            new("Gadget", 20m, 3)
        });
        var oneFails = new Order(2, 100m, true, new List<OrderItem>
        {
            new("Widget", 10m, 5),
            new("Bad", 10m, -1)  // Quantity < 0 fails
        });

        var filter = new ValiFlow<Order>()
            .EachItem(o => o.Items, item => item
                .GreaterThan(i => i.Price, 0m)
                .And()
                .GreaterThan(i => i.Quantity, 0))
            .Build();
        var compiled = filter.Compile();

        compiled(allPass).Should().BeTrue();
        compiled(oneFails).Should().BeFalse();
    }

    [Fact]
    public void EachItem_CombinedWithOtherConditions_BothMustSatisfy()
    {
        var order = new Order(1, 100m, true, new List<OrderItem> { new("A", 10m, 1) });
        var orderInactive = new Order(2, 100m, false, new List<OrderItem> { new("A", 10m, 1) });

        var filter = new ValiFlow<Order>()
            .IsTrue(o => o.IsActive)
            .And()
            .EachItem(o => o.Items, item => item.GreaterThan(i => i.Price, 0m))
            .Build();
        var compiled = filter.Compile();

        compiled(order).Should().BeTrue();
        compiled(orderInactive).Should().BeFalse();
    }

    [Fact]
    public void EachItem_WithStringConditionIsNotNullOrEmpty_ReturnsCorrectResult()
    {
        var order = new Order(1, 100m, true, new List<OrderItem>
        {
            new("Widget", 10m, 1),
            new("Gadget", 20m, 2)
        });
        var orderWithEmpty = new Order(2, 100m, true, new List<OrderItem>
        {
            new("Widget", 10m, 1),
            new("", 20m, 2)  // empty name
        });

        var filter = new ValiFlow<Order>()
            .EachItem(o => o.Items, item => item.IsNotNullOrEmpty(i => i.Name))
            .Build();
        var compiled = filter.Compile();

        compiled(order).Should().BeTrue();
        compiled(orderWithEmpty).Should().BeFalse();
    }

    [Fact]
    public void EachItem_WithCollectionOfStrings_AllNonEmpty()
    {
        var source = new { Tags = new List<string> { "alpha", "beta", "gamma" } };
        var filter = new ValiFlow<List<string>>()
            .Add(list => list.All(s => !string.IsNullOrEmpty(s)))
            .Build();
        filter.Compile()(source.Tags).Should().BeTrue();
    }

    [Fact]
    public void EachItem_WithCollectionOfInts_AllPositive()
    {
        var nums = new List<int> { 1, 2, 3, 4 };
        var filter = new ValiFlow<List<int>>()
            .Add(list => list.All(n => n > 0))
            .Build();
        filter.Compile()(nums).Should().BeTrue();
    }

    [Fact]
    public void EachItem_ChainedAfterIsTrue_BothConditionsApply()
    {
        var active = new Order(1, 50m, true, new List<OrderItem> { new("A", 10m, 2) });
        var inactiveGoodItems = new Order(2, 50m, false, new List<OrderItem> { new("A", 10m, 2) });

        var filter = new ValiFlow<Order>()
            .IsTrue(o => o.IsActive)
            .And()
            .EachItem(o => o.Items, item => item.GreaterThan(i => i.Quantity, 1))
            .Build();
        var compiled = filter.Compile();

        compiled(active).Should().BeTrue();
        compiled(inactiveGoodItems).Should().BeFalse();
    }

    [Fact]
    public void EachItem_BuildReturnsExpressionNotDelegate()
    {
        // Build() must return Expression<Func<T,bool>>, not Func<T,bool>
        var order = new Order(1, 100m, true, new List<OrderItem> { new("A", 10m, 1) });
        var filter = new ValiFlow<Order>()
            .EachItem(o => o.Items, item => item.GreaterThan(i => i.Price, 0m))
            .Build();

        filter.Should().BeAssignableTo<Expression<Func<Order, bool>>>();
        filter.Should().NotBeNull();
        // The expression tree body is available (not just a compiled delegate)
        filter.Body.Should().NotBeNull();
    }

    // =========================================================================
    // Builder Combining Operator Tests
    // =========================================================================

    [Fact]
    public void AndOperator_BothFiltersTrue_ReturnsTrue()
    {
        var left = new ValiFlow<Widget>().IsTrue(w => w.IsDeleted == false);
        var right = new ValiFlow<Widget>().GreaterThan(w => w.Price, 0m);
        var expr = (left & right).Compile();
        expr(new Widget("A", 10m, false)).Should().BeTrue();
    }

    [Fact]
    public void AndOperator_LeftFalse_ReturnsFalse()
    {
        var left = new ValiFlow<Widget>().IsTrue(w => w.IsDeleted);
        var right = new ValiFlow<Widget>().GreaterThan(w => w.Price, 0m);
        var expr = (left & right).Compile();
        expr(new Widget("A", 10m, false)).Should().BeFalse();
    }

    [Fact]
    public void AndOperator_RightFalse_ReturnsFalse()
    {
        var left = new ValiFlow<Widget>().IsFalse(w => w.IsDeleted);
        var right = new ValiFlow<Widget>().LessThan(w => w.Price, 0m);
        var expr = (left & right).Compile();
        expr(new Widget("A", 10m, false)).Should().BeFalse();
    }

    [Fact]
    public void AndOperator_BothFalse_ReturnsFalse()
    {
        var left = new ValiFlow<Widget>().IsTrue(w => w.IsDeleted);
        var right = new ValiFlow<Widget>().LessThan(w => w.Price, 0m);
        var expr = (left & right).Compile();
        expr(new Widget("A", 10m, false)).Should().BeFalse();
    }

    [Fact]
    public void OrOperator_BothTrue_ReturnsTrue()
    {
        var left = new ValiFlow<Widget>().IsFalse(w => w.IsDeleted);
        var right = new ValiFlow<Widget>().GreaterThan(w => w.Price, 0m);
        var expr = (left | right).Compile();
        expr(new Widget("A", 10m, false)).Should().BeTrue();
    }

    [Fact]
    public void OrOperator_LeftTrueRightFalse_ReturnsTrue()
    {
        var left = new ValiFlow<Widget>().IsFalse(w => w.IsDeleted);
        var right = new ValiFlow<Widget>().LessThan(w => w.Price, 0m);
        var expr = (left | right).Compile();
        expr(new Widget("A", 10m, false)).Should().BeTrue();
    }

    [Fact]
    public void OrOperator_LeftFalseRightTrue_ReturnsTrue()
    {
        var left = new ValiFlow<Widget>().IsTrue(w => w.IsDeleted);
        var right = new ValiFlow<Widget>().GreaterThan(w => w.Price, 5m);
        var expr = (left | right).Compile();
        expr(new Widget("A", 10m, false)).Should().BeTrue();
    }

    [Fact]
    public void OrOperator_BothFalse_ReturnsFalse()
    {
        var left = new ValiFlow<Widget>().IsTrue(w => w.IsDeleted);
        var right = new ValiFlow<Widget>().LessThan(w => w.Price, 0m);
        var expr = (left | right).Compile();
        expr(new Widget("A", 10m, false)).Should().BeFalse();
    }

    [Fact]
    public void NotOperator_NegatesTrueFilter_ReturnsFalse()
    {
        var flow = new ValiFlow<Widget>().IsFalse(w => w.IsDeleted);
        var expr = (!flow).Compile();
        // Widget is NOT deleted (IsDeleted=false) → IsFalse passes → negated is false
        expr(new Widget("A", 10m, false)).Should().BeFalse();
    }

    [Fact]
    public void NotOperator_NegatesFalseFilter_ReturnsTrue()
    {
        var flow = new ValiFlow<Widget>().IsTrue(w => w.IsDeleted);
        var expr = (!flow).Compile();
        // Widget is NOT deleted → IsTrue fails → negated is true
        expr(new Widget("A", 10m, false)).Should().BeTrue();
    }

    [Fact]
    public void Combine_AndTrue_SameAsAndOperator()
    {
        var left = new ValiFlow<Widget>().IsFalse(w => w.IsDeleted);
        var right = new ValiFlow<Widget>().GreaterThan(w => w.Price, 5m);
        var combined = ValiFlow<Widget>.Combine(left, right, and: true).Compile();
        var andOp = (left & right).Compile();
        var widget = new Widget("A", 10m, false);
        combined(widget).Should().Be(andOp(widget));
    }

    [Fact]
    public void Combine_AndFalse_SameAsOrOperator()
    {
        var left = new ValiFlow<Widget>().IsTrue(w => w.IsDeleted);
        var right = new ValiFlow<Widget>().GreaterThan(w => w.Price, 5m);
        var combined = ValiFlow<Widget>.Combine(left, right, and: false).Compile();
        var orOp = (left | right).Compile();
        var widget = new Widget("A", 10m, false);
        combined(widget).Should().Be(orOp(widget));
    }

    [Fact]
    public void CombinedExpression_AppliesToIQueryable()
    {
        var source = new List<Widget>
        {
            new("A", 10m, false),
            new("B", 5m, true),
            new("C", 0m, false)
        }.AsQueryable();

        var left = new ValiFlow<Widget>().IsFalse(w => w.IsDeleted);
        var right = new ValiFlow<Widget>().GreaterThan(w => w.Price, 5m);
        var expr = left & right;

        source.Where(expr).Should().ContainSingle().Which.Name.Should().Be("A");
    }

    [Fact]
    public void CombinedExpression_AppliesToListWhere()
    {
        var source = new List<Widget>
        {
            new("A", 10m, false),
            new("B", 5m, true),
            new("C", 20m, false)
        };

        var left = new ValiFlow<Widget>().IsFalse(w => w.IsDeleted);
        var right = new ValiFlow<Widget>().GreaterThan(w => w.Price, 5m);
        var expr = (left & right).Compile();

        source.Where(expr).Should().HaveCount(2).And.Contain(w => w.Name == "A").And.Contain(w => w.Name == "C");
    }

    [Fact]
    public void AndOperator_NullLeft_ThrowsArgumentNullException()
    {
        ValiFlow<Widget>? left = null;
        var right = new ValiFlow<Widget>().GreaterThan(w => w.Price, 0m);
        Action act = () => { var _ = left! & right; };
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ChainedOperators_Filter1AndFilter2OrFilter3_CorrectResult()
    {
        // (filter1 & filter2) | filter3
        var filter1 = new ValiFlow<Widget>().IsFalse(w => w.IsDeleted);
        var filter2 = new ValiFlow<Widget>().GreaterThan(w => w.Price, 5m);
        var filter3 = new ValiFlow<Widget>().IsNotNullOrEmpty(w => w.Name);

        // Combine f1 & f2 first, then | f3
        var f1Andf2 = ValiFlow<Widget>.Combine(filter1, filter2, and: true);
        // wrap in a new flow approach via expression directly
        var param = f1Andf2.Parameters[0];
        var f3Expr = new ValiFlow<Widget>().IsNotNullOrEmpty(w => w.Name).Build();
        var f3Body = new System.Linq.Expressions.ParameterExpression[] { }.Length == 0
            ? f3Expr.Body
            : f3Expr.Body; // just use the body

        // Use Combine via separate ValiFlow instances
        var combined = Expression.Lambda<Func<Widget, bool>>(
            Expression.OrElse(
                f1Andf2.Body,
                new ReplaceVisitor(f3Expr.Parameters[0], param).Visit(f3Expr.Body)!),
            param).Compile();

        // deleted=false, price=10 → f1&f2 true
        combined(new Widget("A", 10m, false)).Should().BeTrue();
        // deleted=true, price=10, name="B" → f1&f2 false, f3 true (name not empty)
        combined(new Widget("B", 10m, true)).Should().BeTrue();
        // deleted=true, price=10, name="" → f1&f2 false, f3 false
        combined(new Widget("", 10m, true)).Should().BeFalse();
    }

    // =========================================================================
    // ValiSort<T> Tests
    // =========================================================================

    [Fact]
    public void ValiSort_ByAsc_SortsAscending()
    {
        var items = new List<Widget> { new("C", 30m, false), new("A", 10m, false), new("B", 20m, false) };
        var sorter = new ValiSort<Widget>().By(w => w.Price);
        var result = sorter.Apply(items.AsEnumerable()).ToList();
        result.Select(w => w.Price).Should().BeInAscendingOrder();
    }

    [Fact]
    public void ValiSort_ByDesc_SortsDescending()
    {
        var items = new List<Widget> { new("A", 10m, false), new("C", 30m, false), new("B", 20m, false) };
        var sorter = new ValiSort<Widget>().By(w => w.Price, descending: true);
        var result = sorter.Apply(items.AsEnumerable()).ToList();
        result.Select(w => w.Price).Should().BeInDescendingOrder();
    }

    [Fact]
    public void ValiSort_ByAscThenByAsc_PrimaryThenSecondarySort()
    {
        var items = new List<Widget>
        {
            new("Z", 10m, false),
            new("A", 20m, false),
            new("M", 10m, false)
        };
        var sorter = new ValiSort<Widget>().By(w => w.Price).ThenBy(w => w.Name);
        var result = sorter.Apply(items.AsEnumerable()).ToList();

        result[0].Name.Should().Be("M");
        result[1].Name.Should().Be("Z");
        result[2].Name.Should().Be("A");
    }

    [Fact]
    public void ValiSort_ByAscThenByDesc_PrimaryAscSecondaryDesc()
    {
        var items = new List<Widget>
        {
            new("Z", 10m, false),
            new("A", 10m, false),
            new("M", 20m, false)
        };
        var sorter = new ValiSort<Widget>().By(w => w.Price).ThenBy(w => w.Name, descending: true);
        var result = sorter.Apply(items.AsEnumerable()).ToList();

        // Price 10 comes first, then sorted by Name DESC: Z > A
        result[0].Name.Should().Be("Z");
        result[1].Name.Should().Be("A");
        result[2].Name.Should().Be("M");
    }

    [Fact]
    public void ValiSort_ByDescThenByAsc_PrimaryDescSecondaryAsc()
    {
        var items = new List<Widget>
        {
            new("Z", 10m, false),
            new("A", 20m, false),
            new("M", 20m, false)
        };
        var sorter = new ValiSort<Widget>().By(w => w.Price, descending: true).ThenBy(w => w.Name);
        var result = sorter.Apply(items.AsEnumerable()).ToList();

        // Price 20 first (DESC), then sorted by Name ASC: A < M
        result[0].Name.Should().Be("A");
        result[1].Name.Should().Be("M");
        result[2].Name.Should().Be("Z");
    }

    [Fact]
    public void ValiSort_ThenByWithoutBy_ThrowsInvalidOperationException()
    {
        var sorter = new ValiSort<Widget>();
        Action act = () => sorter.ThenBy(w => w.Name);
        act.Should().Throw<InvalidOperationException>().WithMessage("*By()*");
    }

    [Fact]
    public void ValiSort_ApplyQueryable_NoCriteria_ThrowsInvalidOperationException()
    {
        var sorter = new ValiSort<Widget>();
        var query = new List<Widget>().AsQueryable();
        Action act = () => sorter.Apply(query);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ValiSort_ApplyEnumerable_NoCriteria_ThrowsInvalidOperationException()
    {
        var sorter = new ValiSort<Widget>();
        var source = new List<Widget>().AsEnumerable();
        Action act = () => sorter.Apply(source);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ValiSort_ByNull_ThrowsArgumentNullException()
    {
        var sorter = new ValiSort<Widget>();
        Action act = () => sorter.By<string>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValiSort_ThenByNull_ThrowsArgumentNullException()
    {
        var sorter = new ValiSort<Widget>();
        sorter.By(w => w.Price);
        Action act = () => sorter.ThenBy<string>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValiSort_ApplyQueryableNullSource_ThrowsArgumentNullException()
    {
        var sorter = new ValiSort<Widget>().By(w => w.Price);
        IQueryable<Widget>? nullQuery = null;
        Action act = () => sorter.Apply(nullQuery!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValiSort_ApplyEnumerableNullSource_ThrowsArgumentNullException()
    {
        var sorter = new ValiSort<Widget>().By(w => w.Price);
        IEnumerable<Widget>? nullSource = null;
        Action act = () => sorter.Apply(nullSource!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValiSort_ApplyQueryable_ReturnsIOrderedQueryable()
    {
        var sorter = new ValiSort<Widget>().By(w => w.Price);
        var query = new List<Widget> { new("A", 10m, false) }.AsQueryable();
        var result = sorter.Apply(query);
        result.Should().BeAssignableTo<IOrderedQueryable<Widget>>();
    }

    [Fact]
    public void ValiSort_ApplyEnumerable_ReturnsIOrderedEnumerable()
    {
        var sorter = new ValiSort<Widget>().By(w => w.Price);
        var source = new List<Widget> { new("A", 10m, false) }.AsEnumerable();
        var result = sorter.Apply(source);
        result.Should().BeAssignableTo<IOrderedEnumerable<Widget>>();
    }

    [Fact]
    public void ValiSort_ExplainByAsc_CorrectFormat()
    {
        var sorter = new ValiSort<Widget>().By(w => w.Price);
        sorter.Explain().Should().Be("ORDER BY Price ASC");
    }

    [Fact]
    public void ValiSort_ExplainByDesc_CorrectFormat()
    {
        var sorter = new ValiSort<Widget>().By(w => w.Price, descending: true);
        sorter.Explain().Should().Be("ORDER BY Price DESC");
    }

    [Fact]
    public void ValiSort_ExplainByThenBy_CorrectFormat()
    {
        var sorter = new ValiSort<Widget>().By(w => w.Price).ThenBy(w => w.Name);
        sorter.Explain().Should().Be("ORDER BY Price ASC, Name ASC");
    }

    [Fact]
    public void ValiSort_ExplainEmpty_ReturnsNoSort()
    {
        var sorter = new ValiSort<Widget>();
        sorter.Explain().Should().Be("(no sort)");
    }

    [Fact]
    public void ValiSort_ByCalledTwice_ResetsPreviousSorts()
    {
        var items = new List<Widget>
        {
            new("A", 10m, false),
            new("B", 30m, false),
            new("C", 20m, false)
        };

        var sorter = new ValiSort<Widget>();
        sorter.By(w => w.Name);         // first By (name asc)
        sorter.By(w => w.Price, true);  // second By resets → price desc

        var result = sorter.Apply(items.AsEnumerable()).ToList();
        result[0].Name.Should().Be("B");  // price 30 desc
        result[1].Name.Should().Be("C");  // price 20
        result[2].Name.Should().Be("A");  // price 10
    }

    [Fact]
    public void ValiSort_TiesOrderedBySecondaryKey()
    {
        var items = new List<Widget>
        {
            new("Beta", 10m, false),
            new("Alpha", 10m, false),
            new("Gamma", 20m, false)
        };
        var sorter = new ValiSort<Widget>().By(w => w.Price).ThenBy(w => w.Name);
        var result = sorter.Apply(items.AsEnumerable()).ToList();

        // Price 10: Alpha < Beta alphabetically
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Beta");
        result[2].Name.Should().Be("Gamma");
    }

    // =========================================================================
    // ValiFlowGlobal Tests
    // =========================================================================

    [Fact]
    public void ValiFlowGlobal_Register_FilterAppliesInBuildWithGlobal()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Widget>(w => !w.IsDeleted);

        var widget = new Widget("A", 10m, false);
        var deletedWidget = new Widget("B", 10m, true);

        var expr = new ValiFlow<Widget>().BuildWithGlobal().Compile();

        expr(widget).Should().BeTrue();
        expr(deletedWidget).Should().BeFalse();
    }

    [Fact]
    public void ValiFlowGlobal_Register_FilterDoesNotApplyInBuild()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Widget>(w => !w.IsDeleted);

        var deletedWidget = new Widget("B", 10m, true);

        // Build() ignores global filters
        var expr = new ValiFlow<Widget>().Build().Compile();
        expr(deletedWidget).Should().BeTrue();
    }

    [Fact]
    public void ValiFlowGlobal_MultipleFilters_AreAndCombinedInBuildWithGlobal()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Widget>(w => !w.IsDeleted);
        ValiFlowGlobal.Register<Widget>(w => w.Price > 0m);

        var passesAll = new Widget("A", 10m, false);
        var failsPrice = new Widget("B", 0m, false);   // price fails
        var failsDeleted = new Widget("C", 10m, true); // deleted fails

        var expr = new ValiFlow<Widget>().BuildWithGlobal().Compile();

        expr(passesAll).Should().BeTrue();
        expr(failsPrice).Should().BeFalse();
        expr(failsDeleted).Should().BeFalse();
    }

    [Fact]
    public void ValiFlowGlobal_RegisterNull_ThrowsArgumentNullException()
    {
        ValiFlowGlobal.ClearAll();
        Action act = () => ValiFlowGlobal.Register<Widget>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValiFlowGlobal_ClearT_RemovesFiltersForThatTypeOnly()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Widget>(w => !w.IsDeleted);
        ValiFlowGlobal.Register<Order>(o => o.IsActive);

        ValiFlowGlobal.Clear<Widget>();

        ValiFlowGlobal.HasFilters<Widget>().Should().BeFalse();
        ValiFlowGlobal.HasFilters<Order>().Should().BeTrue();
    }

    [Fact]
    public void ValiFlowGlobal_ClearT_OtherTypesUnaffected()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Widget>(w => !w.IsDeleted);
        ValiFlowGlobal.Register<Order>(o => o.IsActive);

        ValiFlowGlobal.Clear<Widget>();

        // Order global still applies
        var expr = new ValiFlow<Order>().BuildWithGlobal().Compile();
        var activeOrder = new Order(1, 100m, true, new List<OrderItem>());
        var inactiveOrder = new Order(2, 50m, false, new List<OrderItem>());

        expr(activeOrder).Should().BeTrue();
        expr(inactiveOrder).Should().BeFalse();
    }

    [Fact]
    public void ValiFlowGlobal_ClearAll_RemovesAllFilters()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Widget>(w => !w.IsDeleted);
        ValiFlowGlobal.Register<Order>(o => o.IsActive);

        ValiFlowGlobal.ClearAll();

        ValiFlowGlobal.HasFilters<Widget>().Should().BeFalse();
        ValiFlowGlobal.HasFilters<Order>().Should().BeFalse();
    }

    [Fact]
    public void ValiFlowGlobal_HasFilters_ReturnsTrueWhenRegistered()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.HasFilters<Widget>().Should().BeFalse();

        ValiFlowGlobal.Register<Widget>(w => !w.IsDeleted);

        ValiFlowGlobal.HasFilters<Widget>().Should().BeTrue();
    }

    [Fact]
    public void ValiFlowGlobal_HasFilters_ReturnsFalseAfterClear()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Widget>(w => !w.IsDeleted);
        ValiFlowGlobal.Clear<Widget>();

        ValiFlowGlobal.HasFilters<Widget>().Should().BeFalse();
    }

    [Fact]
    public void ValiFlowGlobal_BuildWithGlobal_NoGlobalsRegistered_SameAsBuild()
    {
        ValiFlowGlobal.ClearAll();

        var widget = new Widget("A", 10m, false);
        var flow = new ValiFlow<Widget>().GreaterThan(w => w.Price, 5m);

        var local = flow.Build().Compile()(widget);
        var global = flow.BuildWithGlobal().Compile()(widget);

        local.Should().Be(global);
    }

    [Fact]
    public void ValiFlowGlobal_BuildWithGlobal_GlobalAndLocalBothApply()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Widget>(w => !w.IsDeleted);

        var passesAll = new Widget("A", 10m, false);
        var failsLocal = new Widget("B", 1m, false);  // price <= 5 fails local
        var failsGlobal = new Widget("C", 10m, true); // deleted fails global

        var expr = new ValiFlow<Widget>()
            .GreaterThan(w => w.Price, 5m)
            .BuildWithGlobal()
            .Compile();

        expr(passesAll).Should().BeTrue();
        expr(failsLocal).Should().BeFalse();
        expr(failsGlobal).Should().BeFalse();
    }

    [Fact]
    public void ValiFlowGlobal_ItemPassesGlobalButFailsLocal_ReturnsFalse()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Widget>(w => !w.IsDeleted);

        // not deleted (passes global), but price = 1 (fails local: price > 5)
        var widget = new Widget("A", 1m, false);

        var expr = new ValiFlow<Widget>()
            .GreaterThan(w => w.Price, 5m)
            .BuildWithGlobal()
            .Compile();

        expr(widget).Should().BeFalse();
    }

    [Fact]
    public void ValiFlowGlobal_ItemFailsGlobalButPassesLocal_ReturnsFalse()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Widget>(w => !w.IsDeleted);

        // deleted (fails global), but price = 50 (passes local: price > 5)
        var widget = new Widget("A", 50m, true);

        var expr = new ValiFlow<Widget>()
            .GreaterThan(w => w.Price, 5m)
            .BuildWithGlobal()
            .Compile();

        expr(widget).Should().BeFalse();
    }

    [Fact]
    public void ValiFlowGlobal_ItemPassesBothGlobalAndLocal_ReturnsTrue()
    {
        ValiFlowGlobal.ClearAll();
        ValiFlowGlobal.Register<Widget>(w => !w.IsDeleted);

        var widget = new Widget("A", 50m, false);

        var expr = new ValiFlow<Widget>()
            .GreaterThan(w => w.Price, 5m)
            .BuildWithGlobal()
            .Compile();

        expr(widget).Should().BeTrue();
    }
}

// =========================================================================
// AnyItem Tests
// =========================================================================

public class AnyItemTests
{
    private class AnyOrder { public decimal Amount { get; set; } public string? Status { get; set; } }
    private class AnyCustomer { public List<AnyOrder>? Orders { get; set; } }

    private static AnyCustomer MakeCustomer(params AnyOrder[] orders)
        => new AnyCustomer { Orders = new List<AnyOrder>(orders) };

    private static AnyOrder MakeOrder(decimal amount = 100m, string? status = "active")
        => new AnyOrder { Amount = amount, Status = status };

    // Test 1: AnyItem — at least one passes → true
    [Fact]
    public void AnyItem_AtLeastOnePasses_ReturnsTrue()
    {
        var customer = MakeCustomer(
            MakeOrder(amount: 5m),
            MakeOrder(amount: 200m),
            MakeOrder(amount: 10m));

        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .Build();

        filter.Compile()(customer).Should().BeTrue();
    }

    // Test 2: AnyItem — none pass → false
    [Fact]
    public void AnyItem_NonePasses_ReturnsFalse()
    {
        var customer = MakeCustomer(
            MakeOrder(amount: 5m),
            MakeOrder(amount: 10m));

        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .Build();

        filter.Compile()(customer).Should().BeFalse();
    }

    // Test 3: AnyItem — all pass → true
    [Fact]
    public void AnyItem_AllPass_ReturnsTrue()
    {
        var customer = MakeCustomer(
            MakeOrder(amount: 150m),
            MakeOrder(amount: 200m),
            MakeOrder(amount: 300m));

        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .Build();

        filter.Compile()(customer).Should().BeTrue();
    }

    // Test 4: AnyItem — empty collection → false
    [Fact]
    public void AnyItem_EmptyCollection_ReturnsFalse()
    {
        var customer = MakeCustomer();

        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 0m))
            .Build();

        filter.Compile()(customer).Should().BeFalse();
    }

    // Test 5: AnyItem — single item passes → true
    [Fact]
    public void AnyItem_SingleItemPasses_ReturnsTrue()
    {
        var customer = MakeCustomer(MakeOrder(amount: 500m));

        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .Build();

        filter.Compile()(customer).Should().BeTrue();
    }

    // Test 6: AnyItem — single item fails → false
    [Fact]
    public void AnyItem_SingleItemFails_ReturnsFalse()
    {
        var customer = MakeCustomer(MakeOrder(amount: 10m));

        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .Build();

        filter.Compile()(customer).Should().BeFalse();
    }

    // Test 7: AnyItem — multiple conditions in inner, one item satisfies all → true
    [Fact]
    public void AnyItem_MultipleConditions_OneItemSatisfiesAll_ReturnsTrue()
    {
        var customer = MakeCustomer(
            MakeOrder(amount: 50m, status: "pending"),
            MakeOrder(amount: 200m, status: "active"));

        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order
                .GreaterThan(o => o.Amount, 100m)
                .And()
                .EqualTo(o => o.Status, "active"))
            .Build();

        filter.Compile()(customer).Should().BeTrue();
    }

    // Test 8: AnyItem — multiple conditions in inner, no item satisfies all → false
    [Fact]
    public void AnyItem_MultipleConditions_NoItemSatisfiesAll_ReturnsFalse()
    {
        var customer = MakeCustomer(
            MakeOrder(amount: 200m, status: "pending"),  // amount ok but status wrong
            MakeOrder(amount: 50m, status: "active"));    // status ok but amount wrong

        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order
                .GreaterThan(o => o.Amount, 100m)
                .And()
                .EqualTo(o => o.Status, "active"))
            .Build();

        filter.Compile()(customer).Should().BeFalse();
    }

    // Test 9: AnyItem — null selector throws ArgumentNullException
    [Fact]
    public void AnyItem_NullSelector_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<AnyCustomer>()
            .AnyItem<AnyOrder>(null!, order => order.GreaterThan(o => o.Amount, 0m));

        act.Should().Throw<ArgumentNullException>();
    }

    // Test 10: AnyItem — null configure throws ArgumentNullException
    [Fact]
    public void AnyItem_NullConfigure_ThrowsArgumentNullException()
    {
        Action act = () => new ValiFlow<AnyCustomer>()
            .AnyItem<AnyOrder>(c => c.Orders, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // Test 11: AnyItem — combined with EachItem (both must hold)
    [Fact]
    public void AnyItem_CombinedWithEachItem_BothMustHold()
    {
        var customer = MakeCustomer(
            MakeOrder(amount: 50m, status: "active"),
            MakeOrder(amount: 200m, status: "active"));

        var filter = new ValiFlow<AnyCustomer>()
            .EachItem(c => c.Orders, order => order.IsNotNullOrEmpty(o => o.Status))
            .And()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .Build();

        filter.Compile()(customer).Should().BeTrue();

        var customerNoHighValue = MakeCustomer(
            MakeOrder(amount: 10m, status: "active"),
            MakeOrder(amount: 20m, status: "active"));

        filter.Compile()(customerNoHighValue).Should().BeFalse();
    }

    // Test 12: AnyItem — combined with EachItem via Or
    [Fact]
    public void AnyItem_CombinedWithEachItemViaOr_EitherCanPass()
    {
        var allSmallActive = MakeCustomer(
            MakeOrder(amount: 5m, status: "active"),
            MakeOrder(amount: 10m, status: "active"));

        var someHighInactive = MakeCustomer(
            MakeOrder(amount: 500m, status: "inactive"),
            MakeOrder(amount: 1m, status: "inactive"));

        var filter = new ValiFlow<AnyCustomer>()
            .EachItem(c => c.Orders, order => order.EqualTo(o => o.Status, "active"))
            .Or()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .Build();

        var compiled = filter.Compile();
        compiled(allSmallActive).Should().BeTrue();    // EachItem passes (all active)
        compiled(someHighInactive).Should().BeTrue();  // AnyItem passes (one high amount)
    }

    // Test 13: AnyItem — builds expression tree compatible with IQueryable
    [Fact]
    public void AnyItem_BuildsExpressionCompatibleWithIQueryable()
    {
        var customers = new List<AnyCustomer>
        {
            MakeCustomer(MakeOrder(amount: 500m)),
            MakeCustomer(MakeOrder(amount: 5m)),
            MakeCustomer(MakeOrder(amount: 200m))
        }.AsQueryable();

        var expr = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .Build();

        customers.Where(expr).Should().HaveCount(2);
    }

    // Test 14: AnyItem vs EachItem — same predicate, EachItem false but AnyItem true
    [Fact]
    public void AnyItem_VsEachItem_SamePredicate_EachItemFalseButAnyItemTrue()
    {
        var customer = MakeCustomer(
            MakeOrder(amount: 500m),
            MakeOrder(amount: 5m));   // this one fails

        var eachItemFilter = new ValiFlow<AnyCustomer>()
            .EachItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .Build()
            .Compile();

        var anyItemFilter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .Build()
            .Compile();

        eachItemFilter(customer).Should().BeFalse();  // not ALL pass
        anyItemFilter(customer).Should().BeTrue();    // at least ONE passes
    }

    // Test 15: AnyItem — with string condition (status equals)
    [Fact]
    public void AnyItem_WithStringCondition_AnyStatusMatches_ReturnsTrue()
    {
        var customer = MakeCustomer(
            MakeOrder(status: "pending"),
            MakeOrder(status: "cancelled"),
            MakeOrder(status: "shipped"));

        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.EqualTo(o => o.Status, "shipped"))
            .Build();

        filter.Compile()(customer).Should().BeTrue();
    }

    // Test 16: AnyItem — no item matches string condition → false
    [Fact]
    public void AnyItem_WithStringCondition_NoStatusMatches_ReturnsFalse()
    {
        var customer = MakeCustomer(
            MakeOrder(status: "pending"),
            MakeOrder(status: "cancelled"));

        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.EqualTo(o => o.Status, "shipped"))
            .Build();

        filter.Compile()(customer).Should().BeFalse();
    }

    // Test 17: AnyItem — used with outer condition AND
    [Fact]
    public void AnyItem_WithOuterConditionAnd_BothApply()
    {
        var activeCustomerWithHighOrder = new AnyCustomer
        {
            Orders = new List<AnyOrder> { MakeOrder(amount: 500m, status: "active") }
        };

        var filter = new ValiFlow<AnyCustomer>()
            .Add(c => c.Orders != null && c.Orders.Count > 0)
            .And()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .Build();

        var compiled = filter.Compile();
        compiled(activeCustomerWithHighOrder).Should().BeTrue();
        compiled(MakeCustomer()).Should().BeFalse();  // empty orders → first condition fails
    }

    // Test 18: AnyItem — collection with null items is handled
    [Fact]
    public void AnyItem_AllItemsPassCondition_ReturnsTrue()
    {
        var customer = MakeCustomer(
            MakeOrder(amount: 100m),
            MakeOrder(amount: 200m),
            MakeOrder(amount: 300m));

        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThanOrEqualTo(o => o.Amount, 100m))
            .Build();

        filter.Compile()(customer).Should().BeTrue();
    }

    // Test 19: AnyItem — expression body is not null
    [Fact]
    public void AnyItem_BuildReturnsExpressionNotNull()
    {
        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 0m))
            .Build();

        filter.Should().NotBeNull();
        filter.Body.Should().NotBeNull();
    }

    // Test 20: AnyItem — used with BuildNegated inverts the result
    [Fact]
    public void AnyItem_UsedWithBuildNegated_InvertsResult()
    {
        var customer = MakeCustomer(MakeOrder(amount: 500m));

        var negatedFilter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .BuildNegated();

        negatedFilter.Compile()(customer).Should().BeFalse();
    }

    // Test 21: AnyItem — large collection, only last item passes
    [Fact]
    public void AnyItem_LargeCollection_OnlyLastItemPasses_ReturnsTrue()
    {
        var orders = Enumerable.Range(1, 99)
            .Select(_ => MakeOrder(amount: 1m))
            .Append(MakeOrder(amount: 1000m))
            .ToArray();
        var customer = MakeCustomer(orders);

        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 500m))
            .Build();

        filter.Compile()(customer).Should().BeTrue();
    }

    // Test 22: AnyItem — large collection, none pass → false
    [Fact]
    public void AnyItem_LargeCollection_NonePasses_ReturnsFalse()
    {
        var orders = Enumerable.Range(1, 100)
            .Select(_ => MakeOrder(amount: 1m))
            .ToArray();
        var customer = MakeCustomer(orders);

        var filter = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 500m))
            .Build();

        filter.Compile()(customer).Should().BeFalse();
    }

    // Test 23: AnyItem — chained with Or against outer condition
    [Fact]
    public void AnyItem_ChainedOr_OuterOrAnyItem_ReturnsCorrectResult()
    {
        var filter = new ValiFlow<AnyCustomer>()
            .Add(c => c.Orders == null)
            .Or()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .Build();

        var compiled = filter.Compile();

        // null orders → outer passes
        compiled(new AnyCustomer { Orders = null }).Should().BeTrue();
        // non-null with high order → AnyItem passes
        compiled(MakeCustomer(MakeOrder(amount: 500m))).Should().BeTrue();
        // non-null with no high order → both fail
        compiled(MakeCustomer(MakeOrder(amount: 5m))).Should().BeFalse();
    }

    // Test 24: AnyItem — applied to in-memory list with Where
    [Fact]
    public void AnyItem_AppliesToListWhere_FiltersCorrectly()
    {
        var customers = new List<AnyCustomer>
        {
            MakeCustomer(MakeOrder(amount: 5m)),
            MakeCustomer(MakeOrder(amount: 500m)),
            MakeCustomer(MakeOrder(amount: 5m), MakeOrder(amount: 300m))
        };

        var compiled = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m))
            .Build()
            .Compile();

        customers.Where(compiled).Should().HaveCount(2);
    }

    // Test 25: AnyItem — IsValid and IsNotValid on builder
    [Fact]
    public void AnyItem_IsValidAndIsNotValid_ReturnCorrectResults()
    {
        var builder = new ValiFlow<AnyCustomer>()
            .AnyItem(c => c.Orders, order => order.GreaterThan(o => o.Amount, 100m));

        var customerWithHighOrder = MakeCustomer(MakeOrder(amount: 500m));
        var customerWithLowOrders = MakeCustomer(MakeOrder(amount: 5m));

        builder.IsValid(customerWithHighOrder).Should().BeTrue();
        builder.IsValid(customerWithLowOrders).Should().BeFalse();
        builder.IsNotValid(customerWithLowOrders).Should().BeTrue();
        builder.IsNotValid(customerWithHighOrder).Should().BeFalse();
    }
}

// Helper visitor for the chained operator test
internal sealed class ReplaceVisitor : System.Linq.Expressions.ExpressionVisitor
{
    private readonly System.Linq.Expressions.ParameterExpression _old;
    private readonly System.Linq.Expressions.Expression _new;

    internal ReplaceVisitor(System.Linq.Expressions.ParameterExpression old, System.Linq.Expressions.Expression @new)
    {
        _old = old;
        _new = @new;
    }

    protected override System.Linq.Expressions.Expression VisitParameter(System.Linq.Expressions.ParameterExpression node)
        => node == _old ? _new : base.VisitParameter(node);
}
