using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Benchmarks;

/// <summary>
/// Lightweight product model used exclusively as benchmark input data.
/// All properties reflect a realistic domain entity without pulling in any real model dependency.
/// </summary>
/// <param name="Name">Product display name; may be <see langword="null"/> for invalid-input scenarios.</param>
/// <param name="Price">Unit price in decimal currency; negative values represent invalid inputs.</param>
/// <param name="Stock">Available stock quantity; zero or negative values represent invalid inputs.</param>
/// <param name="IsActive">Whether the product is currently active in the catalogue.</param>
/// <param name="CreatedAt">UTC creation timestamp; future values represent invalid inputs.</param>
public record BenchProduct(string? Name, decimal Price, int Stock, bool IsActive, DateTime CreatedAt);

/// <summary>
/// BenchmarkDotNet suite that measures the performance of <see cref="ValiFlow{T}"/> across
/// the most common usage patterns: cold vs. warm expression building, cached-func invocation,
/// validation with valid and invalid inputs, cloning, and complex sub-group expressions.
/// </summary>
/// <remarks>
/// Run with <c>dotnet run -c Release --project Vali-Flow.Core.Benchmarks</c>.
/// Results are ordered fastest-to-slowest; the warm <c>IsValid()</c> call is the baseline.
/// </remarks>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ValiFlowBenchmarks
{
    /// <summary>A fully valid <see cref="BenchProduct"/> used as the positive test input.</summary>
    private static BenchProduct ValidProduct => new("Laptop", 999.99m, 10, true, DateTime.UtcNow.AddDays(-30));

    /// <summary>A deliberately invalid <see cref="BenchProduct"/> used to measure short-circuit paths.</summary>
    private static BenchProduct InvalidProduct => new(null, -1m, 0, false, DateTime.UtcNow.AddDays(1));

    /// <summary>Pre-configured builder reused across benchmark iterations to isolate build cost.</summary>
    private ValiFlow<BenchProduct> _cachedBuilder = null!;

    /// <summary>Expression tree materialized once during setup; reused by expression-level benchmarks.</summary>
    private Expression<Func<BenchProduct, bool>> _builtExpression = null!;

    /// <summary>Compiled delegate produced by <c>BuildCached()</c>; represents the fastest invocation path.</summary>
    private Func<BenchProduct, bool> _cachedFunc = null!;

    /// <summary>
    /// Initializes all shared state (builder, expression, compiled func) before any benchmark iteration runs.
    /// Called once per process by BenchmarkDotNet.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _cachedBuilder = new ValiFlow<BenchProduct>()
            .Add(p => p.Name != null)
            .And()
            .Add(p => p.Price, price => price > 0)
            .And()
            .Add(p => p.Stock, stock => stock >= 0)
            .And()
            .Add(p => p.IsActive, active => active == true);

        _builtExpression = _cachedBuilder.Build();
        _cachedFunc = _cachedBuilder.BuildCached();
    }

    // ── Build benchmarks ─────────────────────────────────────────────────────

    /// <summary>Full builder construction + Build() on every call — worst case.</summary>
    [Benchmark(Description = "Build() — cold (new builder each call)")]
    public Expression<Func<BenchProduct, bool>> BuildCold()
    {
        return new ValiFlow<BenchProduct>()
            .Add(p => p.Name != null)
            .And()
            .Add(p => p.Price, price => price > 0)
            .And()
            .Add(p => p.Stock, stock => stock >= 0)
            .And()
            .Add(p => p.IsActive, active => active == true)
            .Build();
    }

    /// <summary>Build() on a pre-configured builder — builder allocation excluded.</summary>
    [Benchmark(Description = "Build() — warm (pre-built builder)")]
    public Expression<Func<BenchProduct, bool>> BuildWarm()
        => _cachedBuilder.Build();

    /// <summary>BuildCached() returns the same compiled Func on repeat calls — no recompile.</summary>
    [Benchmark(Description = "BuildCached() — repeat call (cached Func)")]
    public Func<BenchProduct, bool> BuildCachedFunc()
        => _cachedBuilder.BuildCached();

    // ── Validate benchmarks ──────────────────────────────────────────────────

    /// <summary>IsValid() forces Lazy compilation on first call.</summary>
    [Benchmark(Description = "IsValid() — first call (Lazy compile)")]
    public bool IsValidFirstCall()
    {
        var builder = new ValiFlow<BenchProduct>()
            .Add(p => p.Name != null)
            .And()
            .Add(p => p.Price, price => price > 0);
        return builder.IsValid(ValidProduct);
    }

    /// <summary>IsValid() on a pre-warmed builder — Lazy already compiled.</summary>
    [Benchmark(Description = "IsValid() — warm (pre-compiled predicates)", Baseline = true)]
    public bool IsValidWarm()
        => _cachedBuilder.IsValid(ValidProduct);

    /// <summary>IsValid() on a pre-warmed builder with data that fails validation — exercises early-exit paths.</summary>
    [Benchmark(Description = "IsValid() — warm, invalid input")]
    public bool IsValidWarmInvalid()
        => _cachedBuilder.IsValid(InvalidProduct);

    // ── Clone benchmark ───────────────────────────────────────────────────────

    /// <summary>Measures the cost of <c>Clone()</c>, which performs a shallow structural share of the condition list.</summary>
    [Benchmark(Description = "Clone() — shallow structural share")]
    public ValiFlow<BenchProduct> CloneBuilder()
        => _cachedBuilder.Clone();

    // ── Complex builder ───────────────────────────────────────────────────────

    /// <summary>
    /// Full cold build of a complex expression that includes an <c>AddSubGroup</c> with an <c>Or</c> branch —
    /// exercises the full operator-precedence grouping path in <c>BaseExpression.Build()</c>.
    /// </summary>
    [Benchmark(Description = "Build() — complex (sub-group, Or)")]
    public Expression<Func<BenchProduct, bool>> BuildComplex()
    {
        return new ValiFlow<BenchProduct>()
            .Add(p => p.Name != null)
            .And()
            .Add(p => p.Price, price => price > 0)
            .And()
            .Add(p => p.Stock, stock => stock >= 0)
            .And()
            .Add(p => p.IsActive, active => active)
            .And()
            .AddSubGroup(g => g
                .Add(p => p.CreatedAt, d => d.Year >= 2020)
                .Or()
                .Add(p => p.Price, price => price < 10_000m))
            .Build();
    }

    // ── Compiled func invocation ──────────────────────────────────────────────

    /// <summary>
    /// Invokes the pre-compiled <see cref="Func{T, TResult}"/> directly, with zero expression-tree
    /// overhead. Serves as a lower-bound reference for the compiled-delegate execution cost.
    /// </summary>
    [Benchmark(Description = "Compiled Func<> invocation (no expression overhead)")]
    public bool CompiledFuncInvoke()
        => _cachedFunc(ValidProduct);
}
