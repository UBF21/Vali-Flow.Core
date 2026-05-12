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
    ITimeOnlyExpression<ValiFlow<T>, T>,
    INestedValidation<ValiFlow<T>, T>
{
    /// <summary>Boolean condition component (<c>IsTrue</c>, <c>IsFalse</c>, and in-memory predicates).</summary>
    [ForwardInterface]
    private readonly IBooleanExpression<ValiFlow<T>, T> _booleanExpression;

    /// <summary>Collection condition component (IsEmpty, Any, All, DistinctCount, etc.).</summary>
    [ForwardInterface]
    private readonly ICollectionExpression<ValiFlow<T>, T> _collectionExpression;

    /// <summary>Comparison condition component (null checks, equality, enum membership).</summary>
    [ForwardInterface]
    private readonly IComparisonExpression<ValiFlow<T>, T> _comparisonExpression;

    /// <summary>String condition component (length, regex, casing, Contains with <see cref="StringComparison"/>, etc.).</summary>
    [ForwardInterface]
    private readonly IStringExpression<ValiFlow<T>, T> _stringExpression;

    /// <summary>Numeric condition component (GreaterThan, InRange, IsEven, IsPositive, etc.).</summary>
    [ForwardInterface]
    private readonly INumericExpression<ValiFlow<T>, T> _numericExpression;

    /// <summary><see cref="DateTime"/> condition component.</summary>
    [ForwardInterface]
    private readonly IDateTimeExpression<ValiFlow<T>, T> _dateTimeExpression;

    /// <summary><see cref="DateTimeOffset"/> condition component.</summary>
    [ForwardInterface]
    private readonly IDateTimeOffsetExpression<ValiFlow<T>, T> _dateTimeOffsetExpression;

    /// <summary><see cref="DateOnly"/> condition component.</summary>
    [ForwardInterface]
    private readonly IDateOnlyExpression<ValiFlow<T>, T> _dateOnlyExpression;

    /// <summary><see cref="TimeOnly"/> condition component.</summary>
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

    /// <summary>
    /// Casts <see cref="_numericExpression"/> to <see cref="IComparableExpression{TBuilder,T}"/> to satisfy explicit
    /// interface implementations without duplicating logic.
    /// </summary>
    private IComparableExpression<ValiFlow<T>, T> Comparable
        => (IComparableExpression<ValiFlow<T>, T>)_numericExpression;

    /// <inheritdoc cref="IComparableExpression{TBuilder,T}.GreaterThan{TValue}(Expression{Func{T,TValue}},TValue)"/>
    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.GreaterThan<TValue>(
        Expression<Func<T, TValue>> selector, TValue value)
        => Comparable.GreaterThan(selector, value);

    /// <inheritdoc cref="IComparableExpression{TBuilder,T}.GreaterThanOrEqualTo{TValue}(Expression{Func{T,TValue}},TValue)"/>
    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.GreaterThanOrEqualTo<TValue>(
        Expression<Func<T, TValue>> selector, TValue value)
        => Comparable.GreaterThanOrEqualTo(selector, value);

    /// <inheritdoc cref="IComparableExpression{TBuilder,T}.LessThan{TValue}(Expression{Func{T,TValue}},TValue)"/>
    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.LessThan<TValue>(
        Expression<Func<T, TValue>> selector, TValue value)
        => Comparable.LessThan(selector, value);

    /// <inheritdoc cref="IComparableExpression{TBuilder,T}.LessThanOrEqualTo{TValue}(Expression{Func{T,TValue}},TValue)"/>
    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.LessThanOrEqualTo<TValue>(
        Expression<Func<T, TValue>> selector, TValue value)
        => Comparable.LessThanOrEqualTo(selector, value);

    /// <inheritdoc cref="IComparableExpression{TBuilder,T}.InRange{TValue}"/>
    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.InRange<TValue>(
        Expression<Func<T, TValue>> selector, TValue min, TValue max)
        => Comparable.InRange(selector, min, max);

    /// <inheritdoc cref="IComparableExpression{TBuilder,T}.EqualTo{TValue}"/>
    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.EqualTo<TValue>(
        Expression<Func<T, TValue>> selector, TValue value)
        => Comparable.EqualTo(selector, value);

    /// <inheritdoc cref="IComparableExpression{TBuilder,T}.GreaterThan{TValue}(Expression{Func{T,TValue}},Expression{Func{T,TValue}})"/>
    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.GreaterThan<TValue>(
        Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => Comparable.GreaterThan(selector, otherSelector);

    /// <inheritdoc cref="IComparableExpression{TBuilder,T}.GreaterThanOrEqualTo{TValue}(Expression{Func{T,TValue}},Expression{Func{T,TValue}})"/>
    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.GreaterThanOrEqualTo<TValue>(
        Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => Comparable.GreaterThanOrEqualTo(selector, otherSelector);

    /// <inheritdoc cref="IComparableExpression{TBuilder,T}.LessThan{TValue}(Expression{Func{T,TValue}},Expression{Func{T,TValue}})"/>
    ValiFlow<T> IComparableExpression<ValiFlow<T>, T>.LessThan<TValue>(
        Expression<Func<T, TValue>> selector, Expression<Func<T, TValue>> otherSelector)
        => Comparable.LessThan(selector, otherSelector);

    /// <inheritdoc cref="IComparableExpression{TBuilder,T}.LessThanOrEqualTo{TValue}(Expression{Func{T,TValue}},Expression{Func{T,TValue}})"/>
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
