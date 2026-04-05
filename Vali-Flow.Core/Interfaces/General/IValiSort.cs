using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.General;

public interface IValiSort<T>
{
    IValiSort<T> By<TKey>(Expression<Func<T, TKey>> selector, bool descending = false);
    IValiSort<T> ThenBy<TKey>(Expression<Func<T, TKey>> selector, bool descending = false);
    IOrderedQueryable<T> Apply(IQueryable<T> query);
    /// <summary>Applies the configured sort criteria to an in-memory sequence.</summary>
    /// <remarks>This overload evaluates the sort in memory. For database queries, use <see cref="Apply(IQueryable{T})"/> instead.</remarks>
    IOrderedEnumerable<T> Apply(IEnumerable<T> source);
    string Explain();
}
