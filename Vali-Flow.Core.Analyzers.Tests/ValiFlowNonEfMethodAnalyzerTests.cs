using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Vali_Flow.Core.Analyzers;
using Xunit;

namespace Vali_Flow.Core.Analyzers.Tests;

public class ValiFlowNonEfMethodAnalyzerTests
{
    private static async Task<ImmutableArray<Diagnostic>> GetVf001DiagnosticsAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var refs = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Vali_Flow.Core.Builder.ValiFlowQuery<object>).Assembly.Location),
        };
        var compilation = CSharpCompilation.Create("TestAssembly", new[] { syntaxTree }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new ValiFlowNonEfMethodAnalyzer();
        var withAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));
        var diagnostics = await withAnalyzers.GetAnalyzerDiagnosticsAsync();
        return diagnostics.Where(d => d.Id == "VF001").ToImmutableArray();
    }

    private const string SourcePrefix = @"
using Vali_Flow.Core.Builder;
public class Entity { public string? Name { get; set; } }
public class Usage
{
    public void Run()
    {
        var q = new ValiFlowQuery<Entity>();
";

    private const string SourceSuffix = @"
    }
}";

    [Fact]
    public async Task IsTrimmed_OnValiFlowQuery_DoesNotTriggerVF001()
    {
        var source = SourcePrefix + "q.IsTrimmed(e => e.Name);" + SourceSuffix;
        var diagnostics = await GetVf001DiagnosticsAsync(source);
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task EqualToIgnoreCase_OnValiFlowQuery_DoesNotTriggerVF001()
    {
        var source = SourcePrefix + "q.EqualToIgnoreCase(e => e.Name, \"x\");" + SourceSuffix;
        var diagnostics = await GetVf001DiagnosticsAsync(source);
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task IsLowerCase_And_IsUpperCase_OnValiFlowQuery_DoNotTriggerVF001()
    {
        var source = SourcePrefix + "q.IsLowerCase(e => e.Name); q.IsUpperCase(e => e.Name);" + SourceSuffix;
        var diagnostics = await GetVf001DiagnosticsAsync(source);
        diagnostics.Should().BeEmpty();
    }

    // Deviation from brief: the brief's control case called `q.IsEmail(...)`, but IsEmail
    // (IStringFormatExpression) is never exposed on ValiFlowQuery<T> at all — only ValiFlow<T>
    // implements it — so that call does not compile and the analyzer never sees a resolved
    // symbol. "IsOneOf" has the same problem (not exposed on ValiFlowQuery<T> either). To keep
    // a genuine, compiling control case that still exercises "a name present in NonEfMethods,
    // not one of the 4 being removed, still gets flagged when invoked on a ValiFlowQuery<T>
    // receiver", we declare a local extension method named "IsOneOf" on ValiFlowQuery<Entity>.
    // The analyzer matches purely on method name + receiver type (it never checks
    // methodSymbol.ContainingType), so this faithfully exercises the same code path.
    private const string ControlSource = @"
using System;
using System.Linq.Expressions;
using Vali_Flow.Core.Builder;
public class Entity { public string? Name { get; set; } }
public static class TestExtensions
{
    public static void IsOneOf(this ValiFlowQuery<Entity> q, Expression<Func<Entity, string?>> selector, params string[] values) { }
}
public class Usage
{
    public void Run()
    {
        var q = new ValiFlowQuery<Entity>();
        q.IsOneOf(e => e.Name, ""a"", ""b"");
    }
}";

    [Fact]
    public async Task IsOneOf_OnValiFlowQuery_StillTriggersVF001()
    {
        // Control case: a genuinely non-EF-safe method name must still be flagged.
        var diagnostics = await GetVf001DiagnosticsAsync(ControlSource);
        diagnostics.Should().ContainSingle(d => d.Id == "VF001");
    }

    [Fact]
    public async Task SimilarlyNamedType_InSameNamespace_DoesNotTriggerFalsePositive()
    {
        var source = @"
using Vali_Flow.Core.Builder;
namespace Vali_Flow.Core.Builder { public class ValiFlowQueryExtra { public void All() {} } }
public class Entity { public string? Name { get; set; } }
public class Usage
{
    public void Run()
    {
        var q = new Vali_Flow.Core.Builder.ValiFlowQueryExtra();
        q.All();
    }
}";
        var diagnostics = await GetVf001DiagnosticsAsync(source);
        diagnostics.Should().BeEmpty("ValiFlowQueryExtra is not ValiFlowQuery<T> and must not be flagged by a substring match on its name");
    }

    // Deviation from brief: the brief's literal repro calls `q.IsEmail(e => e.Name)` on a type
    // derived from ValiFlowQuery<T>. As established above for ControlSource/IsOneOf, IsEmail
    // (and every other NonEfMethods name) is not actually exposed on ValiFlowQuery<T> in this
    // codebase — only ValiFlow<T> implements IStringFormatExpression/ICollectionExpression — so
    // that call would not compile and the analyzer would never see a resolved symbol. The
    // analyzer matches purely on method name + receiver type (never methodSymbol.ContainingType),
    // so declaring the extension method on the ValiFlowQuery<Entity> base (same pattern as
    // ControlSource) and invoking it through a derived type faithfully exercises the same
    // base-type-walk code path the brief intends to cover.
    [Fact]
    public async Task DerivedValiFlowQueryType_StillTriggersVF001ViaInheritance()
    {
        var source = @"
using System;
using System.Linq.Expressions;
using Vali_Flow.Core.Builder;
public class Entity { public string? Name { get; set; } }
public static class DerivedTestExtensions
{
    public static void IsEmail(this ValiFlowQuery<Entity> q, Expression<Func<Entity, string?>> selector) { }
}
public class MyQuery<T> : ValiFlowQuery<T> { }
public class Usage
{
    public void Run()
    {
        var q = new MyQuery<Entity>();
        q.IsEmail(e => e.Name);
    }
}";
        var diagnostics = await GetVf001DiagnosticsAsync(source);
        diagnostics.Should().ContainSingle(d => d.Id == "VF001", "a type derived from ValiFlowQuery<T> must still be flagged via the base-type walk");
    }
}
