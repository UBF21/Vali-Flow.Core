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
    /// Builds a <see cref="Expression{TDelegate}"/> from all accumulated conditions.
    /// </summary>
    Expression<Func<T, bool>> Build();

    /// <summary>
    /// Builds the logical negation of <see cref="Build"/>.
    /// </summary>
    Expression<Func<T, bool>> BuildNegated();

    /// <summary>
    /// Compiles and caches the expression as a <see cref="Func{T, TResult}"/> delegate,
    /// and permanently freezes the builder.
    /// </summary>
    Func<T, bool> BuildCached();

    /// <summary>
    /// Builds combining all local conditions AND any globally registered filters
    /// for <typeparamref name="T"/> via <see cref="ValiFlowGlobal"/>.
    /// </summary>
    Expression<Func<T, bool>> BuildWithGlobal();
}
