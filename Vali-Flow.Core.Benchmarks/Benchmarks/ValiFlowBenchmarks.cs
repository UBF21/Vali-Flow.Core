using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Benchmarks;

public record BenchProduct(string? Name, decimal Price, int Stock, bool IsActive, DateTime CreatedAt);

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ValiFlowBenchmarks
{
    private static BenchProduct ValidProduct => new("Laptop", 999.99m, 10, true, DateTime.UtcNow.AddDays(-30));
    private static BenchProduct InvalidProduct => new(null, -1m, 0, false, DateTime.UtcNow.AddDays(1));

    // Pre-built builders and expressions reused across iterations
    private ValiFlow<BenchProduct> _cachedBuilder = null!;
    private Expression<Func<BenchProduct, bool>> _builtExpression = null!;
    private Func<BenchProduct, bool> _cachedFunc = null!;

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

    [Benchmark(Description = "IsValid() — warm, invalid input")]
    public bool IsValidWarmInvalid()
        => _cachedBuilder.IsValid(InvalidProduct);

    // ── Clone benchmark ───────────────────────────────────────────────────────

    [Benchmark(Description = "Clone() — shallow structural share")]
    public ValiFlow<BenchProduct> CloneBuilder()
        => _cachedBuilder.Clone();

    // ── Complex builder ───────────────────────────────────────────────────────

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

    [Benchmark(Description = "Compiled Func<> invocation (no expression overhead)")]
    public bool CompiledFuncInvoke()
        => _cachedFunc(ValidProduct);
}
