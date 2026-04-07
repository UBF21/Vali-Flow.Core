using System.Linq.Expressions;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Classes.General;
using Vali_Flow.Core.Classes.Types;
using Vali_Flow.Core.Interfaces.General;
using Vali_Flow.Core.Interfaces.Types;
using Vali_Flow.Core.Models;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Builder;

/// <summary>
/// The main public fluent builder for constructing <see cref="Expression{TDelegate}"/> trees
/// for in-memory validation and general LINQ expression use.
/// All condition types (string, numeric, collection, DateTime, etc.) are available.
/// For EF Core <c>IQueryable</c> use, prefer <see cref="ValiFlowQuery{T}"/> which only
/// exposes provider-translatable predicates.
/// </summary>
/// <example>
/// <code>
/// var rule = new ValiFlow&lt;Order&gt;()
///     .NotNull(o => o.CustomerId)
///     .GreaterThan(o => o.Total, 0m);
///
/// bool ok  = rule.IsValid(order);
/// var expr = rule.Build(); // Expression&lt;Func&lt;Order, bool&gt;&gt;
/// </code>
/// </example>
/// <typeparam name="T">The entity type being validated or filtered.</typeparam>
public partial class ValiFlow<T> : BaseExpression<ValiFlow<T>, T>,
    IBooleanExpression<ValiFlow<T>, T>, IComparisonExpression<ValiFlow<T>, T>,
    ICollectionExpression<ValiFlow<T>, T>, IStringExpression<ValiFlow<T>, T>,
    INumericExpression<ValiFlow<T>, T>, IComparableExpression<ValiFlow<T>, T>,
    IDateTimeExpression<ValiFlow<T>, T>,
    IDateTimeOffsetExpression<ValiFlow<T>, T>, IDateOnlyExpression<ValiFlow<T>, T>,
    ITimeOnlyExpression<ValiFlow<T>, T>
{
    [ForwardInterface]
    private readonly IBooleanExpression<ValiFlow<T>, T> _booleanExpression;
    [ForwardInterface]
    private readonly ICollectionExpression<ValiFlow<T>, T> _collectionExpression;
    [ForwardInterface]
    private readonly IComparisonExpression<ValiFlow<T>, T> _comparisonExpression;
    [ForwardInterface]
    private readonly IStringExpression<ValiFlow<T>, T> _stringExpression;
    [ForwardInterface]
    private readonly INumericExpression<ValiFlow<T>, T> _numericExpression;
    [ForwardInterface]
    private readonly IDateTimeExpression<ValiFlow<T>, T> _dateTimeExpression;
    [ForwardInterface]
    private readonly IDateTimeOffsetExpression<ValiFlow<T>, T> _dateTimeOffsetExpression;
    [ForwardInterface]
    private readonly IDateOnlyExpression<ValiFlow<T>, T> _dateOnlyExpression;
    [ForwardInterface]
    private readonly ITimeOnlyExpression<ValiFlow<T>, T> _timeOnlyExpression;

    /// <summary>Initializes a new <see cref="ValiFlow{T}"/> builder with no conditions.</summary>
    public ValiFlow()
    {
        _booleanExpression = new BooleanExpression<ValiFlow<T>, T>(this);
        _collectionExpression = new CollectionExpression<ValiFlow<T>, T>(this);
        _comparisonExpression = new ComparisonExpression<ValiFlow<T>, T>(this);
        _stringExpression = new StringExpression<ValiFlow<T>, T>(this);
        _numericExpression = new NumericExpression<ValiFlow<T>, T>(this);
        _dateTimeExpression = new DateTimeExpression<ValiFlow<T>, T>(this);
        _dateTimeOffsetExpression = new DateTimeOffsetExpression<ValiFlow<T>, T>(this);
        _dateOnlyExpression = new DateOnlyExpression<ValiFlow<T>, T>(this);
        _timeOnlyExpression = new TimeOnlyExpression<ValiFlow<T>, T>(this);
    }

    // ── IComparableExpression explicit implementations ─────────────────────────
    // INumericComparisonExpression (INumber<TValue>) and IComparableExpression (IComparable<TValue>)
    // both define GreaterThan/LessThan/etc with generic TValue — same name, different constraints.
    // C# does not allow two public methods with the same signature and different constraints,
    // so IComparableExpression methods are implemented explicitly and delegate to _numericExpression.

    private IComparableExpression<ValiFlow<T>, T> Comparable
        => (IComparableExpression<ValiFlow<T>, T>)_numericExpression;

    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.GreaterThan<TValue>(
        Expression<Func<T, TValue>> selector, TValue value)
        => Comparable.GreaterThan(selector, value);

    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.GreaterThanOrEqualTo<TValue>(
        Expression<Func<T, TValue>> selector, TValue value)
        => Comparable.GreaterThanOrEqualTo(selector, value);

    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.LessThan<TValue>(
        Expression<Func<T, TValue>> selector, TValue value)
        => Comparable.LessThan(selector, value);

    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.LessThanOrEqualTo<TValue>(
        Expression<Func<T, TValue>> selector, TValue value)
        => Comparable.LessThanOrEqualTo(selector, value);

    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.InRange<TValue>(
        Expression<Func<T, TValue>> selector, TValue min, TValue max)
        => Comparable.InRange(selector, min, max);

    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.EqualTo<TValue>(
        Expression<Func<T, TValue>> selector, TValue value)
        => Comparable.EqualTo(selector, value);

    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.GreaterThan<TValue>(
        Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => Comparable.GreaterThan(selector, otherSelector);

    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.GreaterThanOrEqualTo<TValue>(
        Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => Comparable.GreaterThanOrEqualTo(selector, otherSelector);

    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.LessThan<TValue>(
        Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => Comparable.LessThan(selector, otherSelector);

    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.LessThanOrEqualTo<TValue>(
        Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => Comparable.LessThanOrEqualTo(selector, otherSelector);

    // ── Nested validation ─────────────────────────────────────────────────────

    /// <inheritdoc cref="BaseExpression{TBuilder,T}.ValidateNested{TProperty}"/>
    public new ValiFlow<T> ValidateNested<TProperty>(
        Expression<Func<T, TProperty>> selector,
        Action<ValiFlow<TProperty>> configure)
        where TProperty : class
        => base.ValidateNested(selector, configure);

    // ── Cached build ──────────────────────────────────────────────────────────

    /// <inheritdoc cref="BaseExpression{TBuilder,T}.BuildCached"/>
    public new Func<T, bool> BuildCached() => base.BuildCached();

    // ── WithError / WithSeverity overloads ────────────────────────────────────

    /// <inheritdoc cref="BaseExpression{TBuilder,T}.WithError(string,string,string)"/>
    public new ValiFlow<T> WithError(string errorCode, string message, string propertyPath)
        => base.WithError(errorCode, message, propertyPath);

    /// <inheritdoc cref="BaseExpression{TBuilder,T}.WithError(string,string,Severity)"/>
    public new ValiFlow<T> WithError(string errorCode, string message, Severity severity)
        => base.WithError(errorCode, message, severity);

    /// <inheritdoc cref="BaseExpression{TBuilder,T}.WithError(string,string,string,Severity)"/>
    public new ValiFlow<T> WithError(string errorCode, string message, string propertyPath, Severity severity)
        => base.WithError(errorCode, message, propertyPath, severity);

    /// <inheritdoc cref="BaseExpression{TBuilder,T}.WithSeverity(Severity)"/>
    public new ValiFlow<T> WithSeverity(Severity severity)
        => base.WithSeverity(severity);

    // ── Builder combining ─────────────────────────────────────────────────────

    /// <summary>Combines two builders into a single expression using AND (<paramref name="and"/>=<c>true</c>) or OR.</summary>
    public static Expression<Func<T, bool>> Combine(ValiFlow<T> left, ValiFlow<T> right, bool and = true)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return CombineExpressions(left.Build(), right.Build(), and);
    }

    /// <summary>Combines two builders with AND semantics (<c>left &amp;&amp; right</c>).</summary>
    public static Expression<Func<T, bool>> operator &(ValiFlow<T> left, ValiFlow<T> right) => Combine(left, right, and: true);
    /// <summary>Combines two builders with OR semantics (<c>left || right</c>).</summary>
    public static Expression<Func<T, bool>> operator |(ValiFlow<T> left, ValiFlow<T> right) => Combine(left, right, and: false);
    /// <summary>Negates the expression produced by this builder.</summary>
    public static Expression<Func<T, bool>> operator !(ValiFlow<T> flow)
    {
        ArgumentNullException.ThrowIfNull(flow);
        return flow.BuildNegated();
    }
}
