using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Builder;
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;
using static Vali_Flow.Core.Utils.ExpressionHelpers;

namespace Vali_Flow.Core.Classes.Types;

public class CollectionExpression<TBuilder, T> : ICollectionExpression<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, ICollectionExpression<TBuilder, T>, new()
{
    private static readonly System.Reflection.MethodInfo _enumerableAllMethod =
        typeof(Enumerable).GetMethods()
            .Single(m => m.Name == "All" && m.GetParameters().Length == 2);

    private static readonly System.Reflection.MethodInfo _enumerableAnyMethod =
        typeof(Enumerable).GetMethods()
            .Single(m => m.Name == "Any" && m.GetParameters().Length == 2);

    private readonly BaseExpression<TBuilder, T> _builder;

    public CollectionExpression(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>Validates that the selected collection is not null and contains at least one element.</summary>
    public TBuilder NotEmpty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Any();
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is contained in <paramref name="values"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> EF Core translates <c>list.Contains(val)</c> to SQL <c>IN (...)</c> for scalar
    /// types (string, int, Guid, enum, etc.). For complex reference types, EF Core cannot translate
    /// this and will throw at query-translation time. Use <see cref="Any{TValue}"/> with a predicate
    /// for complex-type membership checks.
    /// </remarks>
    public TBuilder In<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(values);

        List<TValue> valueList = values.ToList();
        if (valueList.Count == 0)
            throw new ArgumentException(
                "values must not be empty for In(). An empty IN list would silently filter out every row.",
                nameof(values));
        Expression<Func<TValue, bool>> predicate = val => valueList.Contains(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is NOT contained in <paramref name="values"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> EF Core translates <c>!list.Contains(val)</c> to SQL <c>NOT IN (...)</c> for scalar
    /// types (string, int, Guid, enum, etc.). For complex reference types, EF Core cannot translate
    /// this and will throw at query-translation time.
    /// </remarks>
    public TBuilder NotIn<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(values);

        List<TValue> valueList = values.ToList();
        if (valueList.Count == 0)
            throw new ArgumentException(
                "values must not be empty for NotIn(). An empty NOT IN list would silently pass every row.",
                nameof(values));
        Expression<Func<TValue, bool>> predicate = val => !valueList.Contains(val);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected collection has exactly <paramref name="count"/> elements.</summary>
    public TBuilder Count<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int count)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "count must be >= 0.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() == count;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the number of elements in the collection is between <paramref name="min"/> and <paramref name="max"/> (inclusive).</summary>
    /// <remarks>
    /// The generated expression uses a single <c>Count()</c> call assigned to a local variable,
    /// so the sequence is enumerated exactly once regardless of its type.
    /// </remarks>
    public TBuilder CountBetween<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "min must be >= 0.");
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");

        // Build expression manually to enumerate the sequence only once.
        // val => val != null && (c = val.Count()) >= min && c <= max
        var valParam = Expression.Parameter(typeof(IEnumerable<TValue>), "val");
        var countVar = Expression.Variable(typeof(int), "c");
        var countMethod = typeof(Enumerable)
            .GetMethods()
            .First(m => m.Name == "Count" && m.GetParameters().Length == 1)
            .MakeGenericMethod(typeof(TValue));

        var nullCheck = Expression.NotEqual(valParam, Expression.Constant(null, typeof(IEnumerable<TValue>)));
        var assignCount = Expression.Assign(countVar, Expression.Call(countMethod, valParam));
        var rangeCheck = Expression.AndAlso(
            Expression.GreaterThanOrEqual(countVar, Expression.Constant(min)),
            Expression.LessThanOrEqual(countVar, Expression.Constant(max)));
        var countBlock = Expression.Block(new[] { countVar }, assignCount, rangeCheck);
        var body = Expression.AndAlso(nullCheck, countBlock);
        var predicate = Expression.Lambda<Func<IEnumerable<TValue>, bool>>(body, valParam);

        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that all elements in the selected collection satisfy the given condition.</summary>
    /// <remarks>
    /// <b>EF Core:</b> Uses <c>Enumerable.All</c> with a lambda predicate, which is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder All<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector,
        Expression<Func<TValue, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(predicate);

        var method = _enumerableAllMethod.MakeGenericMethod(typeof(TValue));
        return _builder.Add(BuildNullSafeCollectionPredicate(selector, method, predicate));
    }

    /// <summary>Validates that at least one element in the selected collection satisfies the given condition.</summary>
    /// <remarks>
    /// <b>EF Core:</b> Uses <c>Enumerable.Any</c> with a lambda predicate, which is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder Any<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector,
        Expression<Func<TValue, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(predicate);

        var method = _enumerableAnyMethod.MakeGenericMethod(typeof(TValue));
        return _builder.Add(BuildNullSafeCollectionPredicate(selector, method, predicate));
    }

    /// <summary>Validates that the selected collection contains <paramref name="value"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> <c>Enumerable.Contains</c> with a lambda closure is not translatable to SQL by EF Core.
    /// For EF Core queries, use the scalar <see cref="In{TValue}"/> overload which produces SQL <c>IN (...)</c>.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder Contains<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector, TValue value)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<IEnumerable<TValue>, bool>> predicate = collection => collection != null && collection.Contains(value);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the number of distinct elements in the collection equals <paramref name="count"/>.</summary>
    /// <remarks>
    /// <b>EF Core:</b> This method uses <c>Enumerable.Distinct()</c> which is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder DistinctCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int count)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "count must be >= 0.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Distinct().Count() == count;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that no elements in the selected collection satisfy the given condition.
    /// Returns <c>true</c> if no element in the collection satisfies the predicate. A <c>null</c> collection is treated as vacuously true (no elements can violate the predicate).</summary>
    /// <remarks>
    /// <b>EF Core:</b> Uses <c>Enumerable.Any</c> with a lambda predicate internally, which is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder None<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector,
        Expression<Func<TValue, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(predicate);

        var anyMethod = _enumerableAnyMethod.MakeGenericMethod(typeof(TValue));

        var selectorBody = selector.Body;
        var clonedForNull = new ForceCloneVisitor().Visit(selectorBody)!;
        var clonedForCall = new ForceCloneVisitor().Visit(selectorBody)!;
        var nullCheck = Expression.Equal(clonedForNull, Expression.Constant(null, typeof(IEnumerable<TValue>)));
        var noneCall = Expression.Not(Expression.Call(anyMethod, clonedForCall, predicate));
        var body = Expression.OrElse(nullCheck, noneCall);
        return _builder.Add(Expression.Lambda<Func<T, bool>>(body, selector.Parameters[0]));
    }

    /// <summary>Validates that the selected collection is not null and contains no elements.</summary>
    public TBuilder Empty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && !val.Any();
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected collection has at least <paramref name="min"/> elements.</summary>
    public TBuilder MinCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "min must be >= 0.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() >= min;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected collection has at most <paramref name="max"/> elements. A <c>null</c> collection fails validation.</summary>
    public TBuilder MaxCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < 0) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= 0.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() <= max;
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the collection contains at least one duplicate element.</summary>
    /// <remarks>
    /// <b>EF Core:</b> This method uses <c>GroupBy()</c> which is not translatable to SQL by EF Core.
    /// Use this method only with in-memory collections (LINQ-to-Objects).
    /// </remarks>
    public TBuilder HasDuplicates<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<IEnumerable<TValue>, bool>> predicate = val =>
            val != null && val.GroupBy(x => x).Any(g => g.Count() > 1);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that every element in the selected collection satisfies the given <paramref name="predicate"/> expression.</summary>
    /// <remarks>
    /// Accepts a pre-built expression directly, decoupling this method from the <see cref="ValiFlow{TValue}"/> concrete type.
    /// In-memory only — not EF Core translatable.
    /// </remarks>
    public TBuilder AllMatch<TValue>(
        Expression<Func<T, IEnumerable<TValue>>> selector,
        Expression<Func<TValue, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(predicate);
        var allMethod = _enumerableAllMethod.MakeGenericMethod(typeof(TValue));
        return _builder.Add(BuildNullSafeCollectionPredicate(selector, allMethod, predicate));
    }

    /// <summary>Validates that every element in the selected collection satisfies the given pre-built <paramref name="filter"/>.</summary>
    /// <remarks>
    /// Equivalent to <see cref="EachItem{TValue}(Expression{Func{T,IEnumerable{TValue}}},Action{ValiFlow{TValue}})"/>
    /// but accepts a pre-built <see cref="ValiFlow{TValue}"/> for reuse across multiple builders.
    /// A null collection returns <c>false</c>. An empty collection returns <c>true</c> (vacuously true).
    /// In-memory only — not EF Core translatable.
    /// </remarks>
    public TBuilder AllMatch<TValue>(
        Expression<Func<T, IEnumerable<TValue>>> selector,
        ValiFlow<TValue> filter)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(filter);
        var innerExpr = filter.Build();
        var allMethod = _enumerableAllMethod.MakeGenericMethod(typeof(TValue));
        return _builder.Add(BuildNullSafeCollectionPredicate(selector, allMethod, innerExpr));
    }

    /// <summary>Validates that every element in the selected collection satisfies conditions built by <paramref name="configure"/>.</summary>
    /// <remarks>
    /// Equivalent to <see cref="All{TValue}"/> but accepts a fluent builder action instead of a raw predicate expression.
    /// <para>
    /// The inner builder for element conditions is always a <see cref="ValiFlow{TValue}"/> instance,
    /// regardless of the outer <typeparamref name="TBuilder"/> type. This is by design: the outer builder
    /// operates on <typeparamref name="T"/> entities, while element conditions operate on <typeparamref name="TValue"/>
    /// items, requiring a separate builder instance of the correct element type.
    /// <b>Not EF Core translatable</b> — use with in-memory collections (LINQ-to-Objects) only.
    /// </para>
    /// </remarks>
    public TBuilder EachItem<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector, Action<ValiFlow<TValue>> configure)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(configure);

        var inner = new ValiFlow<TValue>();
        configure(inner);
        Expression<Func<TValue, bool>> innerExpr = inner.Build();
        if (innerExpr.Body is System.Linq.Expressions.ConstantExpression { Value: true })
            throw new ArgumentException("The configure action must add at least one condition.", nameof(configure));

        var allMethod = _enumerableAllMethod.MakeGenericMethod(typeof(TValue));
        return _builder.Add(BuildNullSafeCollectionPredicate(selector, allMethod, innerExpr));
    }

    /// <summary>Validates that at least one element in the selected collection satisfies conditions built by <paramref name="configure"/>.</summary>
    /// <remarks>
    /// Equivalent to <see cref="Any{TValue}"/> but accepts a fluent builder action instead of a raw predicate expression.
    /// <para>
    /// The inner builder for element conditions is always a <see cref="ValiFlow{TValue}"/> instance,
    /// regardless of the outer <typeparamref name="TBuilder"/> type. This is by design: the outer builder
    /// operates on <typeparamref name="T"/> entities, while element conditions operate on <typeparamref name="TValue"/>
    /// items, requiring a separate builder instance of the correct element type.
    /// <b>Not EF Core translatable</b> — use with in-memory collections (LINQ-to-Objects) only.
    /// </para>
    /// </remarks>
    public TBuilder AnyItem<TValue>(Expression<Func<T, IEnumerable<TValue>>> selector, Action<ValiFlow<TValue>> configure)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(configure);

        var inner = new ValiFlow<TValue>();
        configure(inner);
        Expression<Func<TValue, bool>> innerExpr = inner.Build();
        if (innerExpr.Body is System.Linq.Expressions.ConstantExpression { Value: true })
            throw new ArgumentException("The configure action must add at least one condition.", nameof(configure));

        var anyMethod = _enumerableAnyMethod.MakeGenericMethod(typeof(TValue));
        return _builder.Add(BuildNullSafeCollectionPredicate(selector, anyMethod, innerExpr));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Builds a null-safe collection predicate: <c>collection != null &amp;&amp; method(collection, predicate)</c>.
    /// Both the null-check and the method-call get independent clones of the selector body
    /// so the same expression node never appears twice in the tree.
    /// </summary>
    private Expression<Func<T, bool>> BuildNullSafeCollectionPredicate<TValue>(
        Expression<Func<T, IEnumerable<TValue>>> selector,
        System.Reflection.MethodInfo method,
        LambdaExpression predicate)
    {
        var cloner = new ForceCloneVisitor();
        var nullCheck = Expression.NotEqual(
            cloner.Visit(selector.Body)!,
            Expression.Constant(null, typeof(IEnumerable<TValue>)));
        var call = Expression.Call(method, cloner.Visit(selector.Body)!, predicate);
        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(nullCheck, call),
            selector.Parameters[0]);
    }

}