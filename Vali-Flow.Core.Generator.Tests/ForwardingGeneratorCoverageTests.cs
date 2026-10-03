using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Vali_Flow.Core.Generator;
using Xunit;

namespace Vali_Flow.Core.Generator.Tests;

/// <summary>
/// Round 2: exercises ForwardingGenerator's generic container/method, default-parameter-value,
/// and explicit-interface-implementation-on-conflict code paths — Round 1's Task 3 only covered
/// the diagnostics (VFGEN001/VFGEN002) and a trivial non-generic single-method case.
/// </summary>
public class ForwardingGeneratorCoverageTests
{
    private static (System.Collections.Immutable.ImmutableArray<Diagnostic> Diagnostics, string GeneratedSource) RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var refs = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Vali_Flow.Core.Builder.ValiFlow<object>).Assembly.Location),
        };
        var compilation = CSharpCompilation.Create("TestAssembly", new[] { syntaxTree }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ForwardingGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var generatedTree = outputCompilation.SyntaxTrees.FirstOrDefault(t => t.FilePath.Contains(".Forwarding.g.cs"));
        return (diagnostics, generatedTree?.ToString() ?? "");
    }

    [Fact]
    public void GenericContainerClass_ForwardsMethodsWithContainerTypeParameter()
    {
        var source = @"
using Vali_Flow.Core.Builder;
public interface IHasMethod { void Foo(); }
public partial class Container<T>
{
    [ForwardInterface] private IHasMethod _field = null!;
}";
        var (diagnostics, generated) = RunGenerator(source);
        diagnostics.Where(d => d.Id is "VFGEN001" or "VFGEN002").Should().BeEmpty();
        generated.Should().Contain("Container<T>").And.Contain("Foo");
    }

    [Fact]
    public void GenericMethodOnInterface_ForwardsWithMethodTypeParameter()
    {
        var source = @"
using Vali_Flow.Core.Builder;
public interface IHasGenericMethod { T2 Map<T2>(System.Func<T2> f); }
public partial class Container
{
    [ForwardInterface] private IHasGenericMethod _field = null!;
}";
        var (diagnostics, generated) = RunGenerator(source);
        diagnostics.Where(d => d.Id is "VFGEN001" or "VFGEN002").Should().BeEmpty();
        generated.Should().Contain("Map<T2>");
    }

    [Fact]
    public void MethodWithDefaultParameterValues_RendersDefaultsCorrectly()
    {
        var source = @"
using Vali_Flow.Core.Builder;
public interface IHasDefaults
{
    void Foo(int x = 5, string? s = null, bool b = true);
}
public partial class Container
{
    [ForwardInterface] private IHasDefaults _field = null!;
}";
        var (diagnostics, generated) = RunGenerator(source);
        diagnostics.Where(d => d.Id is "VFGEN001" or "VFGEN002").Should().BeEmpty();
        generated.Should().Contain("= 5").And.Contain("= null").And.Contain("= true");
    }

    [Fact]
    public void GenericMethodWithClassAndStructConstraints_RendersConstraintsCorrectly()
    {
        var source = @"
using Vali_Flow.Core.Builder;
public interface IHasConstraints
{
    void RefOnly<T2>(T2 x) where T2 : class;
    void ValueOnly<T2>(T2 x) where T2 : struct;
    void NewableOnly<T2>() where T2 : new();
}
public partial class Container
{
    [ForwardInterface] private IHasConstraints _field = null!;
}";
        var (diagnostics, generated) = RunGenerator(source);
        diagnostics.Where(d => d.Id is "VFGEN001" or "VFGEN002").Should().BeEmpty();
        generated.Should().Contain("where T2 : class").And.Contain("where T2 : struct").And.Contain("where T2 : new()");
    }

    [Fact]
    public void TwoFieldsWithConflictingSignature_EmitsExplicitImplementationForSecond()
    {
        var source = @"
using Vali_Flow.Core.Builder;
public interface IA { void Foo(); }
public interface IB { void Foo(); }
public partial class Container
{
    [ForwardInterface] private IA _a = null!;
    [ForwardInterface] private IB _b = null!;
}";
        var (diagnostics, generated) = RunGenerator(source);
        diagnostics.Where(d => d.Id is "VFGEN001" or "VFGEN002").Should().BeEmpty();
        // The second conflicting member must be emitted as an explicit interface implementation
        // (e.g. "void IB.Foo()") rather than a second public "void Foo()" (which would not compile).
        generated.Should().Contain("IB.Foo");
    }
}
