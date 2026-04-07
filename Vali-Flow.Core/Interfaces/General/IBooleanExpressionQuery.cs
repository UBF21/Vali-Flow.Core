using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.General;

/// <summary>
/// Defines fluent builder methods for EF Core-safe boolean expression conditions.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder implementing the interface.</typeparam>
/// <typeparam name="T">The type of the object being validated.</typeparam>
public interface IBooleanExpressionQuery<out TBuilder, T>
{
    /// <summary>Validates that the selected bool property is <see langword="true"/>.</summary>
    TBuilder IsTrue(Expression<Func<T, bool>> selector);

    /// <summary>Validates that the selected bool property is <see langword="false"/>.</summary>
    TBuilder IsFalse(Expression<Func<T, bool>> selector);
}
