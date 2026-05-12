using System.Linq.Expressions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Interfaces.General;

/// <summary>
/// Defines expression-tree materialization operations targeting <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The entity type being compiled.</typeparam>
public interface IExpressionCompiler<T>
{
    /// <summary>
    /// Builds an <see cref="Expression{TDelegate}"/> from all accumulated conditions.
    /// </summary>
    /// <returns>A compiled LINQ expression tree representing the combined validation predicate.</returns>
    Expression<Func<T, bool>> Build();

    /// <summary>
    /// Builds the logical negation of <see cref="Build"/>.
    /// </summary>
    /// <returns>A LINQ expression tree that is the logical NOT of all accumulated conditions.</returns>
    Expression<Func<T, bool>> BuildNegated();

    /// <summary>
    /// Compiles and caches the expression as a <see cref="Func{T, TResult}"/> delegate,
    /// and permanently freezes the builder.
    /// </summary>
    /// <returns>A compiled delegate for efficient repeated in-memory evaluation.</returns>
    /// <remarks>Calling this method freezes the builder — subsequent mutation calls will return a clone rather than modifying the current instance.</remarks>
    Func<T, bool> BuildCached();

    /// <summary>
    /// Builds an expression combining all local conditions AND any globally registered filters
    /// for <typeparamref name="T"/> via <see cref="ValiFlowGlobal"/>.
    /// </summary>
    /// <returns>A LINQ expression tree that ANDs local conditions with any matching global filters.</returns>
    Expression<Func<T, bool>> BuildWithGlobal();
}
