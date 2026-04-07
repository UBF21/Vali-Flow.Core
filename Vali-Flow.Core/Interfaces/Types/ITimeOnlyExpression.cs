using System.Linq.Expressions;
#pragma warning disable CS1591 // Missing XML comment — documented on implementation or in API reference

namespace Vali_Flow.Core.Interfaces.Types;

public interface ITimeOnlyExpression<out TBuilder, T>
{
    TBuilder IsBefore(Expression<Func<T, TimeOnly>> selector, TimeOnly time);
    TBuilder IsAfter(Expression<Func<T, TimeOnly>> selector, TimeOnly time);
    TBuilder IsBetween(Expression<Func<T, TimeOnly>> selector, TimeOnly from, TimeOnly to);
    TBuilder IsAM(Expression<Func<T, TimeOnly>> selector);
    TBuilder IsPM(Expression<Func<T, TimeOnly>> selector);
    TBuilder IsExactTime(Expression<Func<T, TimeOnly>> selector, TimeOnly time);
    TBuilder IsInHour(Expression<Func<T, TimeOnly>> selector, int hour);
}
