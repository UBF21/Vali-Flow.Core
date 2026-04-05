using System.Collections.Immutable;
using System.Linq.Expressions;
using Vali_Flow.Core.Interfaces.General;
using Vali_Flow.Core.Models;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Base;

/// <summary>
/// Base class for all Vali-Flow expression builders.
/// Owns the condition list, manages And/Or operator precedence, and provides
/// the full IExpression contract.
/// </summary>
/// <remarks>
/// <b>Thread safety — builder phase:</b> A <c>BaseExpression</c> instance is <em>not</em> safe for
/// concurrent mutation. All condition setup (<see cref="Add(System.Linq.Expressions.Expression{System.Func{T,bool}})"/>,
/// <see cref="Or"/>, <see cref="And"/>, <see cref="WithMessage(string)"/>, <see cref="WithError(string,string)"/>, etc.)
/// must happen on a single thread before the builder is used for validation.
///
/// <b>Freeze contract:</b> Once the builder transitions to its <em>use</em> phase — triggered by the first
/// call to <see cref="BuildCached"/>, <see cref="IsValid"/>, <see cref="IsNotValid"/>, <see cref="Validate"/>,
/// or an explicit call to <see cref="Freeze"/> — the original instance is sealed: any mutation method
/// called on a frozen builder returns a <em>new, independent clone</em> (a fork) rather than modifying
/// the original. The original frozen builder is never mutated. This is the same composable pattern as
/// <c>IQueryable&lt;T&gt;</c>, where each <c>.Where()</c> returns a new query object.
///
/// <b>Explicit fork:</b> Call <see cref="Clone"/> directly to create a named fork and extend it freely.
/// Implicit forking (calling mutation methods on a frozen builder) is equally valid but silently discards
/// the result if not assigned — same caveat as an unassigned <c>IQueryable.Where()</c>.
///
/// <b>Thread safety — use phase:</b> After the builder is frozen, <see cref="IsValid"/>,
/// <see cref="IsNotValid"/>, <see cref="Validate"/>, and <see cref="ValidateAll"/> are safe to
/// call concurrently from multiple threads on the same instance.
/// </remarks>
public class BaseExpression<TBuilder, T> : IExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    private volatile ImmutableList<ConditionEntry<T>> _conditions = ImmutableList<ConditionEntry<T>>.Empty;

    private int _nextIsAnd = 1; // 1 = AND (default), 0 = OR — int for Volatile/Interlocked consistency with _frozen
    private Func<T, bool>? _cachedFunc;
    private Func<T, bool>? _cachedNegatedFunc;
    private Expression<Func<T, bool>>? _cachedExpression;
    private int _frozen; // 0 = mutable, 1 = frozen

    /// <inheritdoc/>
    public Expression<Func<T, bool>> Build()
    {
        if (_conditions.Count == 0)
        {
            return _ => true;
        }

        var parameter = Expression.Parameter(typeof(T), "x");

        // Split flat list into AND-groups. A new group starts whenever isAnd=false.
        // Example: [A(T), B(T), C(F), D(T), E(F)] → groups [[A,B],[C,D],[E]]
        var groups = new List<List<Expression>>();
        List<Expression>? currentGroup = null;

        foreach (var entry in _conditions)
        {
            var body = new ParameterReplacer(entry.Condition.Parameters[0], parameter).Visit(entry.Condition.Body);
            if (!entry.IsAnd || currentGroup == null)
            {
                currentGroup = new List<Expression>();
                groups.Add(currentGroup);
            }
            currentGroup.Add(body);
        }

        // AND within each group, then OR across groups
        Expression? result = null;
        foreach (var group in groups)
        {
            Expression? groupExpr = null;
            foreach (var expr in group)
            {
                groupExpr = groupExpr == null ? expr : Expression.AndAlso(groupExpr, expr);
            }

            result = result == null ? groupExpr : Expression.OrElse(result, groupExpr!);
        }

        return Expression.Lambda<Func<T, bool>>(result!, parameter);
    }

    /// <summary>
    /// Creates a new, independent builder pre-populated with all conditions from this instance.
    /// The clone starts <em>unfrozen</em> regardless of whether the source builder is frozen,
    /// so new conditions can be appended freely before the first validation call.
    /// </summary>
    /// <returns>A new <typeparamref name="TBuilder"/> with a shared snapshot of all conditions and their metadata.</returns>
    /// <remarks>
    /// This enables the same composition pattern as <c>IQueryable</c>:
    /// <code>
    /// var base = new ValiFlow&lt;User&gt;()
    ///     .IsNotNullOrEmpty(x => x.Email)
    ///     .GreaterThan(x => x.Age, 0);
    ///
    /// var adminValidator  = base.Clone().IsTrue(x => x.IsAdmin);
    /// var activeValidator = base.Clone().IsTrue(x => x.IsActive);
    /// // base is unchanged; adminValidator and activeValidator are independent.
    /// </code>
    /// The source builder is never frozen or modified by this call.
    /// Internally the condition list uses structural sharing (<c>ImmutableList&lt;T&gt;</c>), so
    /// <c>Clone()</c> is an O(1) operation regardless of how many conditions the builder holds.
    /// Compiled delegates (<see cref="BuildCached"/>) are not copied — the clone recompiles
    /// on first use, reflecting any conditions added after cloning.
    /// </remarks>
    public TBuilder Clone()
    {
        var clone = new TBuilder();
        clone._conditions = _conditions; // O(1) structural sharing via ImmutableList
        clone._nextIsAnd = Volatile.Read(ref _nextIsAnd);
        Interlocked.Exchange(ref clone._frozen, 0); // Explicitly enforce "starts unfrozen" contract
        return clone;
    }

    /// <inheritdoc/>
    public Expression<Func<T, bool>> BuildNegated()
    {
        Expression<Func<T, bool>> condition = Build();
        var parameter = condition.Parameters[0];
        var negatedBody = Expression.Not(condition.Body);
        return Expression.Lambda<Func<T, bool>>(negatedBody, parameter);
    }

    /// <summary>Compiles and caches the predicate built by <see cref="Build"/>. Subsequent calls return the cached delegate without recompiling.</summary>
    /// <remarks>Calling this method permanently freezes the builder (see <see cref="Freeze"/>).</remarks>
    public Func<T, bool> BuildCached()
    {
        var cached = Volatile.Read(ref _cachedFunc);
        if (cached != null)
        {
            return cached;
        }

        Interlocked.Exchange(ref _frozen, 1);
        var expr = Volatile.Read(ref _cachedExpression) ?? Build();
        Interlocked.CompareExchange(ref _cachedExpression, expr, null);
        var built = expr.Compile();
        return Interlocked.CompareExchange(ref _cachedFunc, built, null) ?? built;
    }

    /// <summary>
    /// Permanently freezes this builder so that any mutation method called on it returns a new
    /// independent clone (fork) rather than modifying this instance.
    /// </summary>
    /// <remarks>
    /// <see cref="Freeze"/> is called automatically by <see cref="BuildCached"/>,
    /// <see cref="IsValid"/>, <see cref="IsNotValid"/>, and <see cref="Validate"/>.
    /// Call it explicitly when you want to seal the builder during the DI / startup
    /// phase, before the instance is handed to multiple threads. Once sealed, this builder
    /// can be safely shared across threads for read operations while any thread that needs
    /// a specialized variant can chain mutation methods to obtain an independent fork.
    ///
    /// <b>IQueryable-like pattern:</b>
    /// <code>
    /// // Seal the shared base validator at startup
    /// var sharedValidator = new ValiFlow&lt;Order&gt;()
    ///     .NotNull(o => o.CustomerId)
    ///     .GreaterThan(o => o.Total, 0);
    /// sharedValidator.Freeze();
    ///
    /// // Later, create specialized variants without touching sharedValidator:
    /// var premiumValidator = sharedValidator.GreaterThan(o => o.Total, 1000); // fork
    /// var draftValidator   = sharedValidator.IsTrue(o => o.IsDraft);           // another fork
    /// </code>
    /// </remarks>
    public void Freeze() => Interlocked.Exchange(ref _frozen, 1);

    /// <inheritdoc/>
    public TBuilder Add(Expression<Func<T, bool>> expression)
    {
        var fork = ForkIfFrozen();
        if (fork != null)
        {
            return fork.Add(expression);
        }

        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }
        EnsureValidCondition(expression);
        _conditions = _conditions.Add(new ConditionEntry<T>(expression, Volatile.Read(ref _nextIsAnd) != 0, null, null, null, null, Severity.Error));
        Volatile.Write(ref _nextIsAnd, 1);
        Volatile.Write(ref _cachedFunc, null);
        Volatile.Write(ref _cachedNegatedFunc, null);
        Volatile.Write(ref _cachedExpression, null);
        return (TBuilder)this;
    }

    /// <summary>Adds a condition by composing a property <paramref name="selector"/> with a <paramref name="predicate"/> over its value.</summary>
    /// <typeparam name="TValue">The type of the selected property.</typeparam>
    /// <param name="selector">Selects the property to validate.</param>
    /// <param name="predicate">The predicate to apply to the selected value.</param>
    public TBuilder Add<TValue>(Expression<Func<T, TValue>> selector, Expression<Func<TValue, bool>> predicate)
    {
        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        EnsureValidCondition(predicate);

        var parameter = selector.Parameters[0];
        var selectorBody = selector.Body;
        var predicateBody = new ParameterReplacer(predicate.Parameters[0], selectorBody).Visit(predicate.Body);

        Expression<Func<T, bool>> combinedCondition = Expression.Lambda<Func<T, bool>>(predicateBody, parameter);

        return Add(combinedCondition);
    }

    /// <summary>
    /// Adds a sub-group of conditions built by the <paramref name="group"/> action as a single
    /// logical unit, combined with the outer builder's current And/Or state.
    /// </summary>
    /// <param name="group">
    /// An action that receives a fresh builder and populates it with conditions
    /// that form the sub-group.
    /// </param>
    /// <remarks>
    /// Error metadata (<see cref="WithMessage(string)"/>, <see cref="WithError(string,string)"/>,
    /// <see cref="WithSeverity"/>) applied to conditions inside the <paramref name="group"/>
    /// action is <b>not</b> visible to <see cref="Validate"/>. Only the boolean logic of the
    /// sub-group is preserved. To produce a <see cref="Models.ValidationError"/> for the
    /// sub-group as a whole, chain <see cref="WithMessage(string)"/> or <see cref="WithError(string,string)"/>
    /// on the outer builder after calling <c>AddSubGroup</c>.
    /// </remarks>
    public TBuilder AddSubGroup(Action<IExpression<TBuilder, T>> group)
    {
        var fork = ForkIfFrozen();
        if (fork != null)
        {
            return fork.AddSubGroup(group);
        }

        TBuilder groupBuilderInstance = new TBuilder();
        group(groupBuilderInstance);

        Expression<Func<T, bool>> groupCondition = groupBuilderInstance.Build();
        if (groupCondition.Body is ConstantExpression { Value: true })
        {
            throw new ArgumentException(
                "The group action must add at least one condition to the sub-group.", nameof(group));
        }
        EnsureValidCondition(groupCondition);

        return Add(groupCondition);
    }

    /// <inheritdoc/>
    public TBuilder And()
    {
        var fork = ForkIfFrozen();
        if (fork != null)
        {
            return fork.And();
        }
        Volatile.Write(ref _nextIsAnd, 1);
        Volatile.Write(ref _cachedFunc, null);
        Volatile.Write(ref _cachedNegatedFunc, null);
        Volatile.Write(ref _cachedExpression, null);
        return (TBuilder)this;
    }

    /// <inheritdoc/>
    public TBuilder Or()
    {
        var fork = ForkIfFrozen();
        if (fork != null)
        {
            return fork.Or();
        }
        Volatile.Write(ref _nextIsAnd, 0);
        Volatile.Write(ref _cachedFunc, null);
        Volatile.Write(ref _cachedNegatedFunc, null);
        Volatile.Write(ref _cachedExpression, null);
        return (TBuilder)this;
    }

    // -------------------------------------------------------------------------
    // Validation — single instance
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public bool IsValid(T instance) => BuildCached()(instance);

    /// <inheritdoc/>
    public bool IsNotValid(T instance)
    {
        Interlocked.Exchange(ref _frozen, 1);
        var cached = Volatile.Read(ref _cachedNegatedFunc);
        if (cached != null)
        {
            return cached(instance);
        }

        var expr = Volatile.Read(ref _cachedExpression) ?? Build();
        Interlocked.CompareExchange(ref _cachedExpression, expr, null);
        var parameter = expr.Parameters[0];
        var negatedBody = Expression.Not(expr.Body);
        var negated = Expression.Lambda<Func<T, bool>>(negatedBody, parameter);
        var built = negated.Compile();
        var stored = Interlocked.CompareExchange(ref _cachedNegatedFunc, built, null) ?? built;
        return stored(instance);
    }

    // -------------------------------------------------------------------------
    // Conditional add
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public TBuilder AddIf(bool condition, Expression<Func<T, bool>> expression)
    {
        var fork = ForkIfFrozen();
        if (fork != null)
        {
            return fork.AddIf(condition, expression);
        }
        return condition ? Add(expression) : (TBuilder)this;
    }

    /// <inheritdoc/>
    public TBuilder AddIf<TValue>(bool condition, Expression<Func<T, TValue>> selector, Expression<Func<TValue, bool>> predicate)
    {
        var fork = ForkIfFrozen();
        if (fork != null)
        {
            return fork.AddIf(condition, selector, predicate);
        }
        return condition ? Add(selector, predicate) : (TBuilder)this;
    }

    // -------------------------------------------------------------------------
    // When / Unless
    // -------------------------------------------------------------------------

    /// <summary>Adds a condition that is only evaluated when <paramref name="condition"/> is true. Equivalent to <c>condition &amp;&amp; then</c>.</summary>
    /// <param name="condition">The guard predicate. If false, the <paramref name="then"/> conditions are skipped.</param>
    /// <param name="then">Action that populates the conditional sub-expression.</param>
    public TBuilder When(Expression<Func<T, bool>> condition, Action<IExpression<TBuilder, T>> then)
    {
        var fork = ForkIfFrozen();
        if (fork != null)
        {
            return fork.When(condition, then);
        }

        if (condition == null)
        {
            throw new ArgumentNullException(nameof(condition));
        }

        if (then == null)
        {
            throw new ArgumentNullException(nameof(then));
        }

        TBuilder thenBuilder = new TBuilder();
        then(thenBuilder);
        if (thenBuilder._conditions.Count == 0)
        {
            throw new ArgumentException("The 'then' action must add at least one condition.", nameof(then));
        }
        Expression<Func<T, bool>> thenExpr = thenBuilder.Build();

        var param = condition.Parameters[0];
        var thenBody = new ParameterReplacer(thenExpr.Parameters[0], param).Visit(thenExpr.Body);
        var combined = Expression.AndAlso(condition.Body, thenBody);
        return Add(Expression.Lambda<Func<T, bool>>(combined, param));
    }

    /// <summary>Adds a condition that is only evaluated when <paramref name="condition"/> is false. Equivalent to <c>!condition &amp;&amp; unless</c>.</summary>
    /// <param name="condition">The guard predicate. If true, the <paramref name="unless"/> conditions are skipped.</param>
    /// <param name="unless">Action that populates the conditional sub-expression.</param>
    public TBuilder Unless(Expression<Func<T, bool>> condition, Action<IExpression<TBuilder, T>> unless)
    {
        var fork = ForkIfFrozen();
        if (fork != null)
        {
            return fork.Unless(condition, unless);
        }

        if (condition == null)
        {
            throw new ArgumentNullException(nameof(condition));
        }

        if (unless == null)
        {
            throw new ArgumentNullException(nameof(unless));
        }

        TBuilder unlessBuilder = new TBuilder();
        unless(unlessBuilder);
        if (unlessBuilder._conditions.Count == 0)
        {
            throw new ArgumentException("The 'unless' action must add at least one condition.", nameof(unless));
        }
        Expression<Func<T, bool>> unlessExpr = unlessBuilder.Build();

        var param = condition.Parameters[0];
        var negatedCondition = Expression.Not(condition.Body);
        var unlessBody = new ParameterReplacer(unlessExpr.Parameters[0], param).Visit(unlessExpr.Body);
        var combined = Expression.AndAlso(negatedCondition, unlessBody);
        return Add(Expression.Lambda<Func<T, bool>>(combined, param));
    }

    // -------------------------------------------------------------------------
    // Nested validation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates a nested object by applying conditions configured in an inner <see cref="Builder.ValiFlow{TProperty}"/>.
    /// Automatically returns false if the selected property is null.
    /// </summary>
    /// <typeparam name="TProperty">The type of the nested property. Must be a reference type (class).</typeparam>
    /// <remarks>
    /// The <c>where TProperty : class</c> constraint exists because this method inserts an automatic
    /// null-check for the selected property before evaluating inner conditions. A null check requires a
    /// reference type — value types (structs) can never be null and would make the null guard a compile-time
    /// no-op or runtime error.<br/>
    /// If you need to validate fields of a nested struct, use <see cref="Add{TValue}"/> or
    /// <see cref="AddSubGroup"/> to compose conditions directly on the struct's fields.
    /// </remarks>
    public TBuilder ValidateNested<TProperty>(
        Expression<Func<T, TProperty>> selector,
        Action<Builder.ValiFlow<TProperty>> configure)
        where TProperty : class
    {
        var fork = ForkIfFrozen();
        if (fork != null)
        {
            return fork.ValidateNested(selector, configure);
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var nestedBuilder = new Builder.ValiFlow<TProperty>();
        configure(nestedBuilder);
        Expression<Func<TProperty, bool>> nestedExpr = nestedBuilder.Build();
        if (nestedExpr.Body is ConstantExpression { Value: true })
        {
            throw new ArgumentException("The configure action must add at least one condition.", nameof(configure));
        }

        return Add(BuildNestedExpression(selector, nestedExpr));
    }

    // -------------------------------------------------------------------------
    // Error metadata
    // -------------------------------------------------------------------------

    /// <summary>Attaches a human-readable <paramref name="message"/> to the most recently added condition, surfaced by <see cref="Validate"/> when that condition fails.</summary>
    /// <param name="message">The validation message. Cannot be null or empty.</param>
    public TBuilder WithMessage(string message)
    {
        var fork = ForkIfFrozen();
        if (fork != null) { return fork.WithMessage(message); }
        if (string.IsNullOrEmpty(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));
        return MutateLastCondition(last => last with { Message = message, MessageFactory = null });
    }

    /// <summary>Attaches a lazily-evaluated message factory to the most recently added condition. The factory is invoked each time <see cref="Validate"/> runs, enabling resource-based localization.</summary>
    /// <param name="messageFactory">A factory that returns the validation message. Evaluated at validation time — use this to integrate with <c>IStringLocalizer</c>, resource files, or any localization provider.</param>
    /// <remarks>
    /// <code>
    /// // With .NET resource files:
    /// .WithMessage(() => Resources.NameRequired)
    ///
    /// // With Microsoft.Extensions.Localization:
    /// .WithMessage(() => localizer["Validation.NameRequired"])
    /// </code>
    /// </remarks>
    public TBuilder WithMessage(Func<string> messageFactory)
    {
        ArgumentNullException.ThrowIfNull(messageFactory);
        var fork = ForkIfFrozen();
        if (fork != null) { return fork.WithMessage(messageFactory); }
        return MutateLastCondition(last => last with { MessageFactory = messageFactory });
    }

    /// <summary>Attaches a structured error (<paramref name="errorCode"/> + <paramref name="message"/>) with <see cref="Severity.Error"/> to the most recently added condition.</summary>
    /// <param name="errorCode">A machine-readable error code. Cannot be null or empty.</param>
    /// <param name="message">A human-readable description. Cannot be null or empty.</param>
    public TBuilder WithError(string errorCode, string message)
        => WithError(errorCode, message, Severity.Error);

    /// <summary>Attaches a structured error (<paramref name="errorCode"/> + <paramref name="message"/> + <paramref name="severity"/>) to the most recently added condition.</summary>
    /// <param name="errorCode">A machine-readable error code. Cannot be null or empty.</param>
    /// <param name="message">A human-readable description. Cannot be null or empty.</param>
    /// <param name="severity">The severity level of the validation failure.</param>
    public TBuilder WithError(string errorCode, string message, Severity severity)
    {
        var fork = ForkIfFrozen();
        if (fork != null) { return fork.WithError(errorCode, message, severity); }
        if (string.IsNullOrEmpty(errorCode))
            throw new ArgumentException("Error code cannot be null or empty.", nameof(errorCode));
        if (string.IsNullOrEmpty(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));
        return MutateLastCondition(last => last with { ErrorCode = errorCode, Message = message, MessageFactory = null, Severity = severity });
    }

    /// <summary>Attaches a structured error with <paramref name="propertyPath"/> and <see cref="Severity.Error"/> to the most recently added condition.</summary>
    /// <param name="errorCode">A machine-readable error code. Cannot be null or empty.</param>
    /// <param name="message">A human-readable description. Cannot be null or empty.</param>
    /// <param name="propertyPath">The property path associated with the validation failure. Cannot be null or empty.</param>
    public TBuilder WithError(string errorCode, string message, string propertyPath)
        => WithError(errorCode, message, propertyPath, Severity.Error);

    /// <summary>Attaches a fully-specified structured error (<paramref name="errorCode"/> + <paramref name="message"/> + <paramref name="propertyPath"/> + <paramref name="severity"/>) to the most recently added condition.</summary>
    /// <param name="errorCode">A machine-readable error code. Cannot be null or empty.</param>
    /// <param name="message">A human-readable description. Cannot be null or empty.</param>
    /// <param name="propertyPath">The property path associated with the validation failure. Cannot be null or empty.</param>
    /// <param name="severity">The severity level of the validation failure.</param>
    public TBuilder WithError(string errorCode, string message, string propertyPath, Severity severity)
    {
        var fork = ForkIfFrozen();
        if (fork != null) { return fork.WithError(errorCode, message, propertyPath, severity); }
        if (string.IsNullOrEmpty(errorCode))
            throw new ArgumentException("Error code cannot be null or empty.", nameof(errorCode));
        if (string.IsNullOrEmpty(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));
        if (string.IsNullOrEmpty(propertyPath))
            throw new ArgumentException("Property path cannot be null or empty.", nameof(propertyPath));
        return MutateLastCondition(last => last with { ErrorCode = errorCode, Message = message, MessageFactory = null, PropertyPath = propertyPath, Severity = severity });
    }

    /// <summary>Sets the severity level on the most recently added condition.</summary>
    /// <remarks>
    /// This method only affects the severity stored on the condition; it has no effect unless the condition
    /// also has a message or error code (set via <see cref="WithMessage(string)"/> or <see cref="WithError(string,string)"/>).
    /// A condition with severity but no message or error code is silently skipped by <see cref="Validate"/>.
    /// </remarks>
    public TBuilder WithSeverity(Severity severity)
    {
        var fork = ForkIfFrozen();
        if (fork != null) { return fork.WithSeverity(severity); }
        return MutateLastCondition(last => last with { Severity = severity });
    }

    private TBuilder MutateLastCondition(Func<ConditionEntry<T>, ConditionEntry<T>> mutate)
    {
        if (_conditions.Count == 0) { return (TBuilder)this; }
        var last = _conditions[_conditions.Count - 1];
        _conditions = _conditions.SetItem(_conditions.Count - 1, mutate(last));
        return (TBuilder)this;
    }

    // -------------------------------------------------------------------------
    // Validation result
    // -------------------------------------------------------------------------

    /// <summary>
    /// Evaluates each annotated condition individually and collects <see cref="Models.ValidationError"/> entries
    /// for every condition that fails.
    /// </summary>
    /// <remarks>
    /// <b>Important:</b> <see cref="Validate"/> evaluates each condition with a
    /// <see cref="WithMessage(string)"/> or <see cref="WithError(string,string)"/> annotation independently.
    /// It does <b>not</b> respect <see cref="Or()"/> grouping semantics.
    /// A condition that belongs to an OR-group will still be reported as an error if it
    /// individually fails, even if another condition in the same OR-group passes and the
    /// overall <see cref="IsValid"/> result is <see langword="true"/>.
    /// Use <see cref="IsValid"/> when you need the full Or/And boolean logic evaluated;
    /// use <see cref="Validate"/> when you want per-field error messages collected independently.
    /// </remarks>
    public ValidationResult Validate(T instance)
    {
        Interlocked.Exchange(ref _frozen, 1);
        var errors = new List<ValidationError>();
        var snapshot = _conditions;
        for (int i = 0; i < snapshot.Count; i++)
        {
            var entry = snapshot[i];
            if (entry.Message == null && entry.ErrorCode == null && entry.MessageFactory == null)
                continue;

            if (!entry.CompiledFunc.Value(instance))  // Lazy<T> is thread-safe (ExecutionAndPublication)
            {
                var resolvedMessage = entry.MessageFactory?.Invoke() ?? entry.Message;
                errors.Add(new ValidationError(resolvedMessage ?? entry.ErrorCode!, entry.ErrorCode, entry.PropertyPath, entry.Severity));
            }
        }
        return new ValidationResult(errors);
    }

    /// <summary>
    /// Applies <see cref="Validate"/> to each element of <paramref name="instances"/> and
    /// yields <c>(item, result)</c> pairs for every element in the sequence.
    /// </summary>
    /// <remarks>
    /// <b>Or-grouping blind spot:</b> Because this method calls <see cref="Validate"/> for each
    /// item, it inherits the same limitation described on <see cref="Validate"/>: each annotated
    /// condition is evaluated individually and independently of <see cref="Or()"/> grouping.
    /// A condition that belongs to an Or-group may still appear in the returned
    /// <see cref="Models.ValidationResult"/> as an error even when the overall
    /// <see cref="IsValid"/> result for that item is <see langword="true"/>.
    /// Use <see cref="IsValid"/> alongside a separate per-item query when you need
    /// full Or/And boolean correctness together with per-field error messages.
    /// </remarks>
    /// <param name="instances">The sequence of instances to validate. Must not be <see langword="null"/>.</param>
    /// <returns>A lazily-evaluated sequence of <c>(T Item, ValidationResult Result)</c> tuples.</returns>
    public IEnumerable<(T Item, ValidationResult Result)> ValidateAll(IEnumerable<T> instances)
    {
        if (instances == null)
        {
            throw new ArgumentNullException(nameof(instances));
        }

        return ValidateAllCore(instances);
    }

    private IEnumerable<(T Item, ValidationResult Result)> ValidateAllCore(IEnumerable<T> instances)
    {
        Interlocked.Exchange(ref _frozen, 1);
        foreach (var item in instances)
        {
            yield return (item, Validate(item));
        }
    }

    // -------------------------------------------------------------------------
    // Explain
    // -------------------------------------------------------------------------

    /// <summary>Returns a human-readable string representation of the expression tree built by <see cref="Build"/>, useful for debugging and logging.</summary>
    public string Explain()
    {
        if (_conditions.Count == 0)
        {
            return "(no conditions)";
        }

        return Utils.ExpressionExplainer.Explain(Build().Body);
    }

    // -------------------------------------------------------------------------
    // BuildWithGlobal
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds an <see cref="Expression{TDelegate}"/> that combines the current builder's conditions
    /// with any global filters registered for <typeparamref name="T"/> via <see cref="Builder.ValiFlowGlobal"/>.
    /// </summary>
    /// <returns>A combined <see cref="Expression{TDelegate}"/> of type <c>Func&lt;T, bool&gt;</c>.</returns>
    /// <remarks>
    /// <b>EF Core safety:</b> Global filters registered via <c>ValiFlowGlobal.Register&lt;T&gt;</c> may contain
    /// predicates that are not EF Core-translatable (e.g., regex, string formatting, or in-memory-only checks).
    /// When using <c>BuildWithGlobal()</c> from a <c>ValiFlowQuery&lt;T&gt;</c> instance, verify that all
    /// registered global filters for type <c>T</c> use only EF Core-safe expressions.
    /// </remarks>
    public Expression<Func<T, bool>> BuildWithGlobal()
    {
        var globals = Builder.ValiFlowGlobal.GetFilters<T>();

        // If no local conditions, build from globals only to avoid a leading `true AND ...` constant.
        if (_conditions.Count == 0 && globals.Count > 0)
        {
            var gParam = Expression.Parameter(typeof(T), "x");
            Expression? gBody = null;
            foreach (var g in globals)
            {
                var replaced = new ParameterReplacer(g.Parameters[0], gParam).Visit(g.Body);
                gBody = gBody == null ? replaced : Expression.AndAlso(gBody, replaced);
            }
            return Expression.Lambda<Func<T, bool>>(gBody!, gParam);
        }

        var local = Build();
        if (globals.Count == 0)
        {
            return local;
        }

        var param = local.Parameters[0];
        Expression body = local.Body;
        foreach (var g in globals)
        {
            var gBody = new ParameterReplacer(g.Parameters[0], param).Visit(g.Body);
            body = Expression.AndAlso(body, gBody);
        }

        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a new unfrozen clone if this builder is currently frozen; otherwise returns <c>null</c>.
    /// Callers use this at the start of every mutation method to implement the implicit-fork pattern:
    /// a frozen builder never throws — it silently produces a derived builder instead.
    /// </summary>
    private TBuilder? ForkIfFrozen()
        => Volatile.Read(ref _frozen) != 0 ? Clone() : null;

    private static void ValidateExpressionBody(Expression body)
    {
        switch (body)
        {
            case ConstantExpression constant when constant.Value is bool value && value:
                throw new ArgumentException("The condition provided has no effect because it is always 'true'.");
            case ConstantExpression constantFalse when constantFalse.Value is bool valueFalse && !valueFalse:
                throw new ArgumentException("The condition provided has no effect because it is always 'false'.");
            case ConstantExpression constantNull when constantNull.Value == null:
                throw new ArgumentException("The condition provided has no effect because it is always 'null'.");
            // Not(Constant(bool)) — e.g. IsFalse(x => true) produces Not(true) which is always false
            case UnaryExpression { NodeType: ExpressionType.Not } notExpr
                when notExpr.Operand is ConstantExpression { Value: bool }:
                throw new ArgumentException("The condition provided has no effect because it is always a constant boolean value.");
        }
    }

    /// <summary>
    /// Core implementation of builder combining. Merges two built expressions into one using
    /// AND (<paramref name="and"/> = true) or OR (<paramref name="and"/> = false).
    /// Handles the identity shortcuts: a constant-true left or right expression is bypassed.
    /// </summary>
    protected static Expression<Func<T, bool>> CombineExpressions(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right,
        bool and)
    {
        if (left.Body is ConstantExpression { Value: true }) { return right; }
        if (right.Body is ConstantExpression { Value: true }) { return left; }
        var param = left.Parameters[0];
        var rBody = new ParameterReplacer(right.Parameters[0], param).Visit(right.Body);
        var body = and ? Expression.AndAlso(left.Body, rBody) : Expression.OrElse(left.Body, rBody);
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    private static void EnsureValidCondition(Expression<Func<T, bool>> condition)
    {
        ValidateExpressionBody(condition.Body);
    }

    private static void EnsureValidCondition<TValue>(Expression<Func<TValue, bool>> condition)
    {
        ValidateExpressionBody(condition.Body);
    }

    /// <summary>
    /// Shared helper for ValidateNested implementations. Given a selector and an already-built
    /// nested expression, produces a combined null-check + inner-condition expression on <typeparamref name="T"/>.
    /// </summary>
    protected static Expression<Func<T, bool>> BuildNestedExpression<TProperty>(
        Expression<Func<T, TProperty>> selector,
        Expression<Func<TProperty, bool>> nestedExpr)
        where TProperty : class
    {
        var param = selector.Parameters[0];
        var selectorBody = selector.Body;
        var nestedBody = new ParameterReplacer(nestedExpr.Parameters[0], selectorBody).Visit(nestedExpr.Body);
        var selectorBodyForNullCheck = new ForceCloneVisitor().Visit(selectorBody);
        var nullCheck = Expression.NotEqual(selectorBodyForNullCheck, Expression.Constant(null, typeof(TProperty)));
        var combined = Expression.AndAlso(nullCheck, nestedBody);
        return Expression.Lambda<Func<T, bool>>(combined, param);
    }

}
