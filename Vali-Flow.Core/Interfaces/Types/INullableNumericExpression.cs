using System.Linq.Expressions;

namespace Vali_Flow.Core.Interfaces.Types;

/// <summary>
/// Defines fluent builder methods for nullable numeric property evaluations: IsNullOrZero, HasValue, GreaterThan, LessThan, and InRange for nullable types.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder to enable method chaining.</typeparam>
/// <typeparam name="T">The type of the entity being evaluated.</typeparam>
public interface INullableNumericExpression<out TBuilder, T>
{
    /// <summary>Adds a condition to check if the selected nullable int property is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, int?>> selector);

    /// <summary>Adds a condition to check if the selected nullable decimal property is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, decimal?>> selector);

    /// <summary>Adds a condition to check if the selected nullable long property is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, long?>> selector);

    /// <summary>Adds a condition to check if the selected nullable double property is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, double?>> selector);

    /// <summary>Adds a condition to check if the selected nullable float property is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, float?>> selector);

    /// <summary>Adds a condition to check if the selected nullable short property is null or zero.</summary>
    TBuilder IsNullOrZero(Expression<Func<T, short?>> selector);

    /// <summary>Adds a condition to check if the selected nullable int property has a value.</summary>
    TBuilder HasValue(Expression<Func<T, int?>> selector);

    /// <summary>Adds a condition to check if the selected nullable decimal property has a value.</summary>
    TBuilder HasValue(Expression<Func<T, decimal?>> selector);

    /// <summary>Adds a condition to check if the selected nullable long property has a value.</summary>
    TBuilder HasValue(Expression<Func<T, long?>> selector);

    /// <summary>Adds a condition to check if the selected nullable double property has a value.</summary>
    TBuilder HasValue(Expression<Func<T, double?>> selector);

    /// <summary>Adds a condition to check if the selected nullable float property has a value.</summary>
    TBuilder HasValue(Expression<Func<T, float?>> selector);

    /// <summary>Adds a condition to check if the selected nullable short property has a value.</summary>
    TBuilder HasValue(Expression<Func<T, short?>> selector);

    /// <summary>Adds a condition to check if the selected nullable int property has a value and is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, int?>> selector, int value);

    /// <summary>Adds a condition to check if the selected nullable decimal property has a value and is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, decimal?>> selector, decimal value);

    /// <summary>Adds a condition to check if the selected nullable long property has a value and is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, long?>> selector, long value);

    /// <summary>Adds a condition to check if the selected nullable double property has a value and is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, double?>> selector, double value);

    /// <summary>Adds a condition to check if the selected nullable float property has a value and is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, float?>> selector, float value);

    /// <summary>Adds a condition to check if the selected nullable short property has a value and is greater than <paramref name="value"/>.</summary>
    TBuilder GreaterThan(Expression<Func<T, short?>> selector, short value);

    /// <summary>Adds a condition to check if the selected nullable int property has a value and is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, int?>> selector, int value);

    /// <summary>Adds a condition to check if the selected nullable decimal property has a value and is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, decimal?>> selector, decimal value);

    /// <summary>Adds a condition to check if the selected nullable long property has a value and is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, long?>> selector, long value);

    /// <summary>Adds a condition to check if the selected nullable double property has a value and is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, double?>> selector, double value);

    /// <summary>Adds a condition to check if the selected nullable float property has a value and is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, float?>> selector, float value);

    /// <summary>Adds a condition to check if the selected nullable short property has a value and is less than <paramref name="value"/>.</summary>
    TBuilder LessThan(Expression<Func<T, short?>> selector, short value);

    /// <summary>Adds a condition to check if the selected nullable int property has a value and is within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, int?>> selector, int min, int max);

    /// <summary>Adds a condition to check if the selected nullable decimal property has a value and is within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, decimal?>> selector, decimal min, decimal max);

    /// <summary>Adds a condition to check if the selected nullable long property has a value and is within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, long?>> selector, long min, long max);

    /// <summary>Adds a condition to check if the selected nullable double property has a value and is within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, double?>> selector, double min, double max);

    /// <summary>Adds a condition to check if the selected nullable float property has a value and is within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, float?>> selector, float min, float max);

    /// <summary>Adds a condition to check if the selected nullable short property has a value and is within [<paramref name="min"/>, <paramref name="max"/>].</summary>
    TBuilder InRange(Expression<Func<T, short?>> selector, short min, short max);
}
