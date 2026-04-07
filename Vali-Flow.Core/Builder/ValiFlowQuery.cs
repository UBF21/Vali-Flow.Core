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
/// A fluent builder for constructing <see cref="Expression{TDelegate}"/> trees that are safe to pass
/// to EF Core <c>DbContext.Where()</c> and other IQueryable providers.
/// </summary>
/// <remarks>
/// <para>
/// <b>Use this builder for EF Core queries.</b> Only methods whose predicates are translatable by the major EF Core relational providers
/// (SQL Server, PostgreSQL, MySQL/Pomelo 5.0+, Oracle EF Core 7.0+) are exposed.
/// <b>SQLite note:</b> Methods that access <c>.DayOfWeek</c> are not translated by the SQLite EF Core provider —
/// use in-memory filtering via <see cref="ValiFlow{T}"/> or a raw SQL workaround for SQLite.
/// Some methods may not translate on less common providers or when used with non-navigation collection properties.
/// Methods that rely on in-memory delegates (e.g. <c>Regex.IsMatch</c>,
/// <c>char.IsDigit</c>, <c>DateTime.Today</c>, <c>%</c> modulo, <c>Enumerable.All/Any</c> with lambdas)
/// are intentionally absent.
/// </para>
/// <para>
/// For in-memory validation where all methods are available, use <see cref="ValiFlow{T}"/> instead.
/// </para>
/// </remarks>
/// <typeparam name="T">The entity type being validated or filtered.</typeparam>
public partial class ValiFlowQuery<T> : BaseExpression<ValiFlowQuery<T>, T>,
    IBooleanExpressionQuery<ValiFlowQuery<T>, T>,
    IComparisonExpression<ValiFlowQuery<T>, T>,
    IStringExpressionQuery<ValiFlowQuery<T>, T>,
    ICollectionExpressionQuery<ValiFlowQuery<T>, T>,
    INumericExpressionQuery<ValiFlowQuery<T>, T>,
    IDateTimeExpressionQuery<ValiFlowQuery<T>, T>,
    IDateTimeOffsetExpressionQuery<ValiFlowQuery<T>, T>,
    IDateOnlyExpressionQuery<ValiFlowQuery<T>, T>,
    ITimeOnlyExpressionQuery<ValiFlowQuery<T>, T>
{
    [ForwardInterface]
    private readonly IBooleanExpressionQuery<ValiFlowQuery<T>, T> _boolean;
    [ForwardInterface]
    private readonly IComparisonExpression<ValiFlowQuery<T>, T> _comparison;
    [ForwardInterface]
    private readonly IStringExpressionQuery<ValiFlowQuery<T>, T> _string;
    [ForwardInterface]
    private readonly ICollectionExpressionQuery<ValiFlowQuery<T>, T> _collection;
    [ForwardInterface]
    private readonly INumericExpressionQuery<ValiFlowQuery<T>, T> _numeric;
    [ForwardInterface]
    private readonly IDateTimeExpressionQuery<ValiFlowQuery<T>, T> _dateTime;
    [ForwardInterface]
    private readonly IDateTimeOffsetExpressionQuery<ValiFlowQuery<T>, T> _dateTimeOffset;
    [ForwardInterface]
    private readonly IDateOnlyExpressionQuery<ValiFlowQuery<T>, T> _dateOnly;
    [ForwardInterface]
    private readonly ITimeOnlyExpressionQuery<ValiFlowQuery<T>, T> _timeOnly;

    /// <summary>Initializes a new <see cref="ValiFlowQuery{T}"/> builder with all EF Core-safe expression components.</summary>
    public ValiFlowQuery()
    {
        _boolean = new BooleanExpressionQuery<ValiFlowQuery<T>, T>(this);
        _comparison = new ComparisonExpression<ValiFlowQuery<T>, T>(this);
        _string = new StringExpressionQuery<ValiFlowQuery<T>, T>(this);
        _collection = new CollectionExpressionQuery<ValiFlowQuery<T>, T>(this);
        _numeric = new NumericExpressionQuery<ValiFlowQuery<T>, T>(this);
        _dateTime = new DateTimeExpressionQuery<ValiFlowQuery<T>, T>(this);
        _dateTimeOffset = new DateTimeOffsetExpressionQuery<ValiFlowQuery<T>, T>(this);
        _dateOnly = new DateOnlyExpressionQuery<ValiFlowQuery<T>, T>(this);
        _timeOnly = new TimeOnlyExpressionQuery<ValiFlowQuery<T>, T>(this);
    }

    // ── Nested validation ─────────────────────────────────────────────────────

    /// <summary>
    /// Validates that the navigation property selected by <paramref name="selector"/> is not null
    /// and satisfies all conditions configured in <paramref name="configure"/> using an inner
    /// <see cref="ValiFlowQuery{TProperty}"/> builder that only exposes EF Core-safe methods.
    /// </summary>
    public ValiFlowQuery<T> ValidateNested<TProperty>(
        Expression<Func<T, TProperty>> selector,
        Action<ValiFlowQuery<TProperty>> configure)
        where TProperty : class
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(configure);
        var nestedBuilder = new ValiFlowQuery<TProperty>();
        configure(nestedBuilder);
        var nestedExpr = nestedBuilder.Build();
        if (nestedExpr.Body is ConstantExpression { Value: true })
        {
            throw new ArgumentException("The configure action must add at least one condition.", nameof(configure));
        }
        return Add(BuildNestedExpression(selector, nestedExpr));
    }

    // ── WithError / WithSeverity overloads ────────────────────────────────────

    /// <inheritdoc cref="BaseExpression{TBuilder,T}.WithError(string,string,string)"/>
    public new ValiFlowQuery<T> WithError(string errorCode, string message, string propertyPath)
        => base.WithError(errorCode, message, propertyPath);

    /// <inheritdoc cref="BaseExpression{TBuilder,T}.WithError(string,string,Severity)"/>
    public new ValiFlowQuery<T> WithError(string errorCode, string message, Severity severity)
        => base.WithError(errorCode, message, severity);

    /// <inheritdoc cref="BaseExpression{TBuilder,T}.WithError(string,string,string,Severity)"/>
    public new ValiFlowQuery<T> WithError(string errorCode, string message, string propertyPath, Severity severity)
        => base.WithError(errorCode, message, propertyPath, severity);

    /// <inheritdoc cref="BaseExpression{TBuilder,T}.WithSeverity"/>
    public new ValiFlowQuery<T> WithSeverity(Severity severity)
        => base.WithSeverity(severity);

    // ── Builder combining ─────────────────────────────────────────────────────

    /// <summary>
    /// Combines two <see cref="ValiFlowQuery{T}"/> builders into a single
    /// <see cref="Expression{TDelegate}"/> joined with AND (default) or OR.
    /// </summary>
    public static Expression<Func<T, bool>> Combine(ValiFlowQuery<T> left, ValiFlowQuery<T> right, bool and = true)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return CombineExpressions(left.Build(), right.Build(), and);
    }

    /// <inheritdoc/>
    protected override ValiFlow<TProperty> CreateNestedBuilder<TProperty>() => new ValiFlow<TProperty>();

    /// <summary>Combines two builders with a logical AND into a single expression.</summary>
    public static Expression<Func<T, bool>> operator &(ValiFlowQuery<T> left, ValiFlowQuery<T> right)
        => Combine(left, right, and: true);

    /// <summary>Combines two builders with a logical OR into a single expression.</summary>
    public static Expression<Func<T, bool>> operator |(ValiFlowQuery<T> left, ValiFlowQuery<T> right)
        => Combine(left, right, and: false);

    /// <summary>Returns the logical negation of the builder's expression via <see cref="BaseExpression{TBuilder,T}.BuildNegated"/>.</summary>
    public static Expression<Func<T, bool>> operator !(ValiFlowQuery<T> flow)
    {
        ArgumentNullException.ThrowIfNull(flow);
        return flow.BuildNegated();
    }
}
