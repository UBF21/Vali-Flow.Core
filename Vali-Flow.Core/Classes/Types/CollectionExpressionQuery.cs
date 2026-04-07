using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — implementation class, docs on interface
using Vali_Flow.Core.Classes.Base;
using Vali_Flow.Core.Interfaces.Types;

namespace Vali_Flow.Core.Classes.Types;

public class CollectionExpressionQuery<TBuilder, T> : ICollectionExpressionQuery<TBuilder, T>
    where TBuilder : BaseExpression<TBuilder, T>, new()
{
    private readonly BaseExpression<TBuilder, T> _builder;

    public CollectionExpressionQuery(BaseExpression<TBuilder, T> builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public TBuilder NotEmpty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Any();
        return _builder.Add(selector, predicate);
    }

    public TBuilder Empty<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && !val.Any();
        return _builder.Add(selector, predicate);
    }

    public TBuilder In<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(values);
        List<TValue> valueList = values.ToList();
        if (valueList.Count == 0)
            throw new ArgumentException("values must not be empty for In(). An empty IN list would silently filter out every row.", nameof(values));
        Expression<Func<TValue, bool>> predicate = val => valueList.Contains(val);
        return _builder.Add(selector, predicate);
    }

    public TBuilder NotIn<TValue>(Expression<Func<T, TValue>> selector, IEnumerable<TValue> values)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(values);
        List<TValue> valueList = values.ToList();
        if (valueList.Count == 0)
            throw new ArgumentException("values must not be empty for NotIn(). An empty NOT IN list would silently pass every row.", nameof(values));
        Expression<Func<TValue, bool>> predicate = val => !valueList.Contains(val);
        return _builder.Add(selector, predicate);
    }

    public TBuilder Count<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int count)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "count must be >= 0.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() == count;
        return _builder.Add(selector, predicate);
    }

    public TBuilder MinCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "min must be >= 0.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() >= min;
        return _builder.Add(selector, predicate);
    }

    public TBuilder MaxCount<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (max < 0) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= 0.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() <= max;
        return _builder.Add(selector, predicate);
    }

    public TBuilder CountBetween<TValue>(Expression<Func<T, IEnumerable<TValue?>>> selector, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(selector);
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "min must be >= 0.");
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= min.");
        Expression<Func<IEnumerable<TValue?>, bool>> predicate = val => val != null && val.Count() >= min && val.Count() <= max;
        return _builder.Add(selector, predicate);
    }
}
