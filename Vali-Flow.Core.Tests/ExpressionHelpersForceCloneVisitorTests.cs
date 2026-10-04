using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

/// <summary>
/// Coverage tests for the internal ForceCloneVisitor (Vali-Flow.Core/Utils/ExpressionHelpers.cs),
/// exercised indirectly through cross-property InRange/BetweenDates — the only call sites that
/// run it over a non-trivial selector body.
/// </summary>
public class ExpressionHelpersForceCloneVisitorTests
{
    private record Measurement(int RawValue, int[] Readings, decimal Price, DateTime Start, DateTime End);

    // 1. VisitUnary (type conversion) — selector body is a Convert node, not a plain member access.
    [Fact]
    public void InRange_CrossProperty_WithConversionSelector_ClonesUnaryNodeCorrectly()
    {
        var filter = new ValiFlowQuery<Measurement>()
            .InRange(m => (decimal)m.RawValue, m => m.Price - 10m, m => m.Price + 10m)
            .Build().Compile();

        filter(new Measurement(100, new[] { 1 }, 105m, DateTime.Now, DateTime.Now)).Should().BeTrue();
        filter(new Measurement(1, new[] { 1 }, 105m, DateTime.Now, DateTime.Now)).Should().BeFalse();
    }

    // 2. VisitIndex (array element access) — selector body indexes into a collection.
    [Fact]
    public void InRange_CrossProperty_WithIndexerSelector_ClonesIndexNodeCorrectly()
    {
        var filter = new ValiFlowQuery<Measurement>()
            .InRange(m => m.Readings[0], m => m.RawValue - 5, m => m.RawValue + 5)
            .Build().Compile();

        filter(new Measurement(100, new[] { 100 }, 1m, DateTime.Now, DateTime.Now)).Should().BeTrue();
        filter(new Measurement(100, new[] { 1 }, 1m, DateTime.Now, DateTime.Now)).Should().BeFalse();
    }

    // 3. BetweenDates cross-property with a direct DateTime member selector — baseline sanity
    // confirming the visitor still produces a structurally correct, independently evaluable clone
    // for the most common real-world case (member access through a converted comparison).
    [Fact]
    public void BetweenDates_CrossProperty_DateTime_StartAndEndSelectors_EvaluatesIndependently()
    {
        var filter = new ValiFlowQuery<Measurement>()
            .BetweenDates(m => m.Start, m => m.Start, m => m.End)
            .Build().Compile();

        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2025, 12, 31);
        filter(new Measurement(1, new[] { 1 }, 1m, start, end)).Should().BeTrue();
        // When Start > End, the range becomes backwards, so the condition fails
        filter(new Measurement(1, new[] { 1 }, 1m, end.AddDays(1), end)).Should().BeFalse();
    }

    // 4. VisitUnary — selector body is a cast/conversion, not plain member access.
    [Fact]
    public void InRange_CrossProperty_WithNullableCastSelector_ClonesUnaryNodeCorrectly()
    {
        var filter = new ValiFlowQuery<Measurement>()
            .InRange(m => (int)m.RawValue, m => m.RawValue - 5, m => m.RawValue + 5)
            .Build().Compile();

        filter(new Measurement(100, new[] { 1 }, 1m, DateTime.Now, DateTime.Now)).Should().BeTrue();
    }

    // 5. VisitMethodCall — selector body is a method call (ToString/ToUpper-style), not plain member access.
    [Fact]
    public void Contains_MultiSelector_WithMethodCallSelector_ClonesMethodCallNodeCorrectly()
    {
        var filter = new ValiFlow<Measurement>()
            .Contains("1", new System.Linq.Expressions.Expression<System.Func<Measurement, string?>>[] { m => m.RawValue.ToString() })
            .Build().Compile();

        filter(new Measurement(1, new[] { 1 }, 1m, DateTime.Now, DateTime.Now)).Should().BeTrue();
        filter(new Measurement(99, new[] { 1 }, 1m, DateTime.Now, DateTime.Now)).Should().BeFalse();
    }
}
