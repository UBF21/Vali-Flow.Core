using System.Linq.Expressions;
using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

/// <summary>
/// Tests for ExpressionExplainer — the internal utility that converts expression trees
/// into human-readable strings, primarily surfaced via BaseExpression.Explain().
/// </summary>
public class ExpressionExplainerTests
{
    private record Item(string? Name, int Value, bool IsActive, decimal Price, DateTime CreatedAt);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string Explain(Expression<Func<Item, bool>> expr)
    {
        // ExpressionExplainer is internal; we access it through ValiFlow.Explain()
        var builder = new ValiFlow<Item>();
        builder.Add(expr);
        return builder.Explain();
    }

    // ── Binary expressions ────────────────────────────────────────────────────

    [Fact]
    public void Explain_BinaryGreaterThan_ShowsCorrectOperator()
    {
        Expression<Func<Item, bool>> expr = item => item.Value > 0;
        var result = Explain(expr);
        result.Should().Contain(">");
        result.Should().Contain("Value");
        result.Should().Contain("0");
    }

    [Fact]
    public void Explain_BinaryLessThan_ShowsCorrectOperator()
    {
        Expression<Func<Item, bool>> expr = item => item.Value < 100;
        var result = Explain(expr);
        result.Should().Contain("<");
        result.Should().Contain("100");
    }

    [Fact]
    public void Explain_BinaryEqual_ShowsDoubleEquals()
    {
        Expression<Func<Item, bool>> expr = item => item.Value == 42;
        var result = Explain(expr);
        result.Should().Contain("==");
        result.Should().Contain("42");
    }

    [Fact]
    public void Explain_BinaryNotEqual_ShowsNotEqualOperator()
    {
        Expression<Func<Item, bool>> expr = item => item.Name != null;
        var result = Explain(expr);
        result.Should().Contain("!=");
        result.Should().Contain("null");
    }

    [Fact]
    public void Explain_BinaryGreaterThanOrEqual_ShowsOperator()
    {
        Expression<Func<Item, bool>> expr = item => item.Value >= 5;
        var result = Explain(expr);
        result.Should().Contain(">=");
    }

    [Fact]
    public void Explain_BinaryLessThanOrEqual_ShowsOperator()
    {
        Expression<Func<Item, bool>> expr = item => item.Value <= 10;
        var result = Explain(expr);
        result.Should().Contain("<=");
    }

    // ── Logical operators ─────────────────────────────────────────────────────

    [Fact]
    public void Explain_AndAlso_ShowsAND()
    {
        var builder = new ValiFlow<Item>()
            .Add(x => x.Value > 0)
            .And()
            .Add(x => x.IsActive == true);
        var result = builder.Explain();
        result.Should().Contain("AND");
    }

    [Fact]
    public void Explain_OrElse_ShowsOR()
    {
        var builder = new ValiFlow<Item>()
            .Add(x => x.Value > 100)
            .Or()
            .Add(x => x.IsActive == true);
        var result = builder.Explain();
        result.Should().Contain("OR");
    }

    // ── Unary expressions ─────────────────────────────────────────────────────

    [Fact]
    public void Explain_UnaryNot_ShowsNOT()
    {
        Expression<Func<Item, bool>> expr = item => !item.IsActive;
        var result = Explain(expr);
        result.Should().Contain("NOT");
        result.Should().Contain("IsActive");
    }

    // ── Member access ─────────────────────────────────────────────────────────

    [Fact]
    public void Explain_MemberAccess_ShowsPropertyName()
    {
        Expression<Func<Item, bool>> expr = item => item.IsActive;
        var result = Explain(expr);
        result.Should().Contain("IsActive");
    }

    [Fact]
    public void Explain_NestedMember_ShowsDotNotation()
    {
        // Use CreatedAt.Year as a nested member access
        Expression<Func<Item, bool>> expr = item => item.CreatedAt.Year == 2024;
        var result = Explain(expr);
        result.Should().Contain("CreatedAt");
        result.Should().Contain("Year");
        result.Should().Contain("2024");
    }

    // ── Constants ─────────────────────────────────────────────────────────────

    [Fact]
    public void Explain_StringConstant_ShowsQuotedValue()
    {
        Expression<Func<Item, bool>> expr = item => item.Name == "hello";
        var result = Explain(expr);
        result.Should().Contain("\"hello\"");
    }

    [Fact]
    public void Explain_NullConstant_ShowsNull()
    {
        Expression<Func<Item, bool>> expr = item => item.Name == null;
        var result = Explain(expr);
        result.Should().Contain("null");
    }

    [Fact]
    public void Explain_IntegerConstant_ShowsNumber()
    {
        Expression<Func<Item, bool>> expr = item => item.Value == 99;
        var result = Explain(expr);
        result.Should().Contain("99");
    }

    [Fact]
    public void Explain_BoolConstant_ShowsValue()
    {
        Expression<Func<Item, bool>> expr = item => item.IsActive == true;
        var result = Explain(expr);
        result.Should().Contain("True");
    }

    // ── Method calls ──────────────────────────────────────────────────────────

    [Fact]
    public void Explain_InstanceMethodCall_ShowsDotMethodName()
    {
        Expression<Func<Item, bool>> expr = item => item.Name!.StartsWith("A");
        var result = Explain(expr);
        result.Should().Contain("StartsWith");
        result.Should().Contain("Name");
    }

    [Fact]
    public void Explain_StaticMethodCall_ShowsDeclaringTypeDotMethodName()
    {
        Expression<Func<Item, bool>> expr = item => string.IsNullOrEmpty(item.Name);
        var result = Explain(expr);
        result.Should().Contain("IsNullOrEmpty");
    }

    [Fact]
    public void Explain_MethodCallWithArgument_ShowsArgument()
    {
        Expression<Func<Item, bool>> expr = item => item.Name!.Contains("test");
        var result = Explain(expr);
        result.Should().Contain("Contains");
        result.Should().Contain("\"test\"");
    }

    // ── Complex / compound expressions ────────────────────────────────────────

    [Fact]
    public void Explain_WrapsSubExpressionsInParentheses()
    {
        Expression<Func<Item, bool>> expr = item => item.Value > 0 && item.Value < 100;
        var result = Explain(expr);
        result.Should().Contain("(");
        result.Should().Contain(")");
    }

    [Fact]
    public void Explain_MultipleConditions_ProducesNonEmptyString()
    {
        var builder = new ValiFlow<Item>()
            .Add(x => x.Name != null)
            .And()
            .Add(x => x.Value > 0)
            .And()
            .Add(x => x.IsActive == true);
        var result = builder.Explain();
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("AND");
    }

    [Fact]
    public void Explain_EmptyBuilder_ReturnsNonNullString()
    {
        var builder = new ValiFlow<Item>();
        var result = builder.Explain();
        result.Should().NotBeNull();
    }

    [Fact]
    public void Explain_SubGroup_ContainsGroupConditions()
    {
        var builder = new ValiFlow<Item>()
            .Add(x => x.IsActive == true)
            .And()
            .AddSubGroup(g => g
                .Add(x => x.Value > 0)
                .Or()
                .Add(x => x.Price > 0m));
        var result = builder.Explain();
        result.Should().Contain("Value");
        result.Should().Contain("Price");
        result.Should().Contain("OR");
    }

    // ── Arithmetic operators ──────────────────────────────────────────────────

    [Fact]
    public void Explain_AddOperator_ShowsPlus()
    {
        Expression<Func<Item, bool>> expr = item => item.Value + 1 > 5;
        var result = Explain(expr);
        result.Should().Contain("+");
    }

    [Fact]
    public void Explain_SubtractOperator_ShowsMinus()
    {
        Expression<Func<Item, bool>> expr = item => item.Value - 1 > 0;
        var result = Explain(expr);
        result.Should().Contain("-");
    }
}
