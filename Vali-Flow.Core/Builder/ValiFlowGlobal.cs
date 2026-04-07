using System.Linq.Expressions;

namespace Vali_Flow.Core.Builder;

/// <summary>
/// Process-wide static registry of global filter expressions.
/// All methods delegate to <see cref="Default"/>, a single <see cref="ValiFlowGlobalRegistry"/> instance
/// that persists for the lifetime of the process.
/// </summary>
/// <remarks>
/// <para>
/// For dependency-injection scenarios (per-tenant isolation, per-test isolation, or multi-context apps),
/// create and inject a <see cref="ValiFlowGlobalRegistry"/> instance directly and call
/// <see cref="Classes.Base.BaseExpression{TBuilder,T}.BuildWithGlobal(ValiFlowGlobalRegistry)"/>.
/// </para>
/// <para>
/// <b>Unit tests:</b> The <see cref="Default"/> registry is static and persists for the lifetime of the process.
/// Call <see cref="ClearAll"/> (or <see cref="Clear{T}"/>) in test teardown to prevent cross-test
/// state pollution. Alternatively, use a fresh <see cref="ValiFlowGlobalRegistry"/> instance per test.
/// </para>
/// </remarks>
public static class ValiFlowGlobal
{
    /// <summary>
    /// The process-wide <see cref="ValiFlowGlobalRegistry"/> instance used by
    /// <see cref="Classes.Base.BaseExpression{TBuilder,T}.BuildWithGlobal()"/> (no-parameter overload).
    /// </summary>
    public static readonly ValiFlowGlobalRegistry Default = new();

    /// <inheritdoc cref="ValiFlowGlobalRegistry.Register{T}"/>
    public static void Register<T>(Expression<Func<T, bool>> filter) => Default.Register(filter);

    /// <inheritdoc cref="ValiFlowGlobalRegistry.Clear{T}"/>
    public static void Clear<T>() => Default.Clear<T>();

    /// <inheritdoc cref="ValiFlowGlobalRegistry.ClearAll"/>
    public static void ClearAll() => Default.ClearAll();

    /// <inheritdoc cref="ValiFlowGlobalRegistry.HasFilters{T}"/>
    public static bool HasFilters<T>() => Default.HasFilters<T>();

    /// <inheritdoc cref="ValiFlowGlobalRegistry.GetFilters{T}"/>
    public static IReadOnlyList<Expression<Func<T, bool>>> GetFilters<T>() => Default.GetFilters<T>();
}
