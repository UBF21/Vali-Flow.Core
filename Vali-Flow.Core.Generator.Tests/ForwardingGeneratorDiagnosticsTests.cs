using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Vali_Flow.Core.Generator;
using Xunit;

namespace Vali_Flow.Core.Generator.Tests;

public class ForwardingGeneratorDiagnosticsTests
{
    private static ImmutableArray<Diagnostic> RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        // ForwardInterfaceAttribute is internal to Vali-Flow.Core, so we can't reference it by type here;
        // use a public type from the same assembly (ValiFlow<>) to get the assembly reference instead.
        var refs = new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Vali_Flow.Core.Builder.ValiFlow<object>).Assembly.Location) };
        var compilation = CSharpCompilation.Create("TestAssembly", new[] { syntaxTree }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ForwardingGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);
        return diagnostics;
    }

    private const string Prelude = @"
using Vali_Flow.Core.Builder;
public interface IEmpty { }
public class NotAnInterface { }
public interface IHasMethod { void Foo(); }
public partial class Container
{
";

    [Fact]
    public void FieldTypedAsNonInterface_ReportsVFGEN001()
    {
        var source = Prelude + @"
    [ForwardInterface] private NotAnInterface _field = new();
}";
        var diagnostics = RunGenerator(source);
        diagnostics.Should().Contain(d => d.Id == "VFGEN001");
    }

    [Fact]
    public void InterfaceWithNoMethods_ReportsVFGEN002()
    {
        var source = Prelude + @"
    [ForwardInterface] private IEmpty _field = null!;
}";
        var diagnostics = RunGenerator(source);
        diagnostics.Should().Contain(d => d.Id == "VFGEN002");
    }

    [Fact]
    public void ValidInterfaceField_ReportsNoDiagnostics()
    {
        var source = Prelude + @"
    [ForwardInterface] private IHasMethod _field = null!;
}";
        var diagnostics = RunGenerator(source);
        diagnostics.Where(d => d.Id is "VFGEN001" or "VFGEN002").Should().BeEmpty();
    }
}
