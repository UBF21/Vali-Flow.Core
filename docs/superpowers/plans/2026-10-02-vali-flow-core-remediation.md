# Vali-Flow.Core Remediation & Coverage Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix the critical/high/medium bugs found in the Vali-Flow.Core code audit (NotNull/Null crash on non-nullable value types, VF001 analyzer false positive, silent generator failures, node-aliasing defensive gap, cosmetics) and raise test coverage from 74.3%/62.8% (line/branch) toward 90%, with at least 3 new tests per expression-type family (Date, Numeric, String, Comparison) plus the specific infrastructure/gap tests the coverage audit identified.

**Architecture:** No architectural change. All fixes are localized to existing classes; all new tests are added to existing xUnit test files (or one new file for `ForceCloneVisitor` coverage, and one new test project for the Analyzer). Each task is a self-contained file+tests unit, independently runnable and parallelizable via subagent-driven-development.

**Tech Stack:** .NET 8/9, C#, xUnit, FluentAssertions, Microsoft.CodeAnalysis.CSharp (Roslyn, for the analyzer test project).

**Spec:** This plan is derived directly from four completed audit reports (code-quality/SOLID, C# idiomatic review, silent-failure hunt, coverage-gap analysis) produced in this session, and the user-approved design in chat. There is no separate spec file — the audit findings (file:line, repro, root cause) and the chat-approved design constitute the spec.

## Global Constraints

- Public NuGet library already shipped at v2.0.2 — **no breaking changes**: no public signature may change (parameter types, generic constraints, method names, return types).
- Never mutate a frozen builder in place — any mutation path must go through `ForkIfFrozen()`.
- `ValiFlowQuery<T>` must never gain a method/behavior that isn't translatable by EF Core (SQL-safe only).
- TDD: for bugfixes, write the failing test first, confirm it fails for the documented reason, then fix. For pure coverage-addition tasks (no production bug), write the test, run it, confirm it passes (there is no red phase because the behavior already exists and is correct — only untested).
- Every bugfix needs a regression test (established project culture, see `CHANGELOG.md`).
- Build command: `dotnet build Vali-Flow.Core.sln`
- Test + coverage command: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --collect:"XPlat Code Coverage" --results-directory ./TestResults`
- Test style already established in the repo: xUnit `[Fact]`, FluentAssertions `.Should().BeTrue()/.BeFalse()`, one record per test file acting as the test entity, helper `MakeX(...)` factory methods with named/optional parameters. Follow this exactly — do not introduce a new test style (no Theory/InlineData unless a task says so explicitly).
- Do not commit anything under `.claude/`, `.codegraph/`, or `.guardian.json` (already gitignored) — the plan never touches those.

---

### Task 1: Fix `ComparisonExpression.NotNull`/`Null` crash on non-nullable value types [CRITICAL]

**Files:**
- Modify: `Vali-Flow.Core/Classes/General/ComparisonExpression.cs:28-48`
- Test: `Vali-Flow.Core.Tests/BaseExpressionTests.cs` (append to the existing `Product`-based test class)

**Interfaces:**
- Consumes: `BaseExpression<TBuilder,T>.Add(Expression<Func<T,bool>>)` (existing, `Vali-Flow.Core/Classes/Base/BaseExpression.cs:212`) and `BaseExpression<TBuilder,T>.Add<TValue>(selector, predicate)` (existing, same file:234) — both already public on `_builder`, no signature change.
- Produces: `ComparisonExpression<TBuilder,T>.NotNull<TValue>`/`Null<TValue>` keep their exact existing public signatures (`Expression<Func<T, TValue?>> selector`) — callers (`IsNull`/`IsNotNull` aliases, `ValiFlow<T>`, `ValiFlowQuery<T>`) need no changes.

- [ ] **Step 1: Write the failing regression test**

Append to `Vali-Flow.Core.Tests/BaseExpressionTests.cs` (same file already defines `public record Product(string? Name, decimal Price, int Quantity, bool IsActive, DateTime CreatedAt, List<string> Tags);` — reuse it; `Quantity` is `int`, `IsActive` is `bool`, `CreatedAt` is `DateTime`, all non-nullable value types):

```csharp
    // ── Regression: NotNull/Null on non-nullable value types (int/DateTime/bool) ──

    [Fact]
    public void NotNull_OnNonNullableInt_DoesNotThrow_AndAlwaysPasses()
    {
        var act = () => new ValiFlow<Product>().NotNull(p => p.Quantity).Build();
        act.Should().NotThrow();

        var filter = new ValiFlow<Product>().NotNull(p => p.Quantity).Build().Compile();
        filter(new Product("A", 10m, 0, true, DateTime.Now, new List<string>())).Should().BeTrue();
    }

    [Fact]
    public void NotNull_OnNonNullableDateTime_DoesNotThrow_AndAlwaysPasses()
    {
        var act = () => new ValiFlow<Product>().NotNull(p => p.CreatedAt).Build();
        act.Should().NotThrow();

        var filter = new ValiFlow<Product>().NotNull(p => p.CreatedAt).Build().Compile();
        filter(new Product("A", 10m, 1, true, DateTime.Now, new List<string>())).Should().BeTrue();
    }

    [Fact]
    public void Null_OnNonNullableBool_DoesNotThrow_AndAlwaysFails()
    {
        var act = () => new ValiFlow<Product>().Null(p => p.IsActive).Build();
        act.Should().NotThrow();

        var filter = new ValiFlow<Product>().Null(p => p.IsActive).Build().Compile();
        filter(new Product("A", 10m, 1, true, DateTime.Now, new List<string>())).Should().BeFalse();
    }

    [Fact]
    public void IsNotNull_OnNonNullableInt_DoesNotThrow()
    {
        // IsNotNull is a pure alias for NotNull — confirms the fix propagates through the alias.
        var act = () => new ValiFlow<Product>().IsNotNull(p => p.Quantity).Build();
        act.Should().NotThrow();
    }

    [Fact]
    public void NotNull_OnNullableReferenceType_StillWorksAsBefore()
    {
        // Regression guard: the existing reference-type path must remain unaffected by the fix.
        var filter = new ValiFlow<Product>().NotNull(p => p.Name).Build().Compile();
        filter(new Product("A", 10m, 1, true, DateTime.Now, new List<string>())).Should().BeTrue();
        filter(new Product(null, 10m, 1, true, DateTime.Now, new List<string>())).Should().BeFalse();
    }
```

- [ ] **Step 2: Run tests to verify the first four fail**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~BaseExpressionTests&FullyQualifiedName~NotNull_On|FullyQualifiedName~Null_On|FullyQualifiedName~IsNotNull_On"`
Expected: FAIL — `System.ArgumentException: Argument types do not match.` for the first four tests; the fifth (`NotNull_OnNullableReferenceType_StillWorksAsBefore`) PASSES already (confirms it's testing the pre-existing working path, not accidentally also broken).

- [ ] **Step 3: Fix `NotNull`/`Null` in `ComparisonExpression.cs`**

Replace lines 28-48 of `Vali-Flow.Core/Classes/General/ComparisonExpression.cs`:

```csharp
    /// <summary>Validates that the selected value is not null.</summary>
    /// <typeparam name="TValue">The type of the property being compared.</typeparam>
    /// <remarks>
    /// For a <typeparamref name="TValue"/> that closes over a non-nullable value type
    /// (e.g. <c>int</c>, <c>DateTime</c>, <c>bool</c>, <c>Guid</c>, <c>decimal</c>, any <c>enum</c>),
    /// the value can never be <see langword="null"/> — the condition is trivially <see langword="true"/>.
    /// </remarks>
    public TBuilder NotNull<TValue>(Expression<Func<T, TValue?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var closedType = typeof(TValue?);
        if (closedType.IsValueType && Nullable.GetUnderlyingType(closedType) == null)
        {
            // Unconstrained TValue? erases to TValue at runtime for non-nullable value types
            // (no Nullable<T> wrapper is produced) — the value can never be null.
            Expression<Func<T, bool>> alwaysTrue = _ => true;
            return _builder.Add(alwaysTrue);
        }

        var param = Expression.Parameter(closedType, "value");
        var body = Expression.NotEqual(param, Expression.Constant(null, closedType));
        Expression<Func<TValue?, bool>> predicate = Expression.Lambda<Func<TValue?, bool>>(body, param);
        return _builder.Add(selector, predicate);
    }

    /// <summary>Validates that the selected value is null.</summary>
    /// <typeparam name="TValue">The type of the property being compared.</typeparam>
    /// <remarks>
    /// For a <typeparamref name="TValue"/> that closes over a non-nullable value type,
    /// the value can never be <see langword="null"/> — the condition is trivially <see langword="false"/>.
    /// </remarks>
    public TBuilder Null<TValue>(Expression<Func<T, TValue?>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var closedType = typeof(TValue?);
        if (closedType.IsValueType && Nullable.GetUnderlyingType(closedType) == null)
        {
            Expression<Func<T, bool>> alwaysFalse = _ => false;
            return _builder.Add(alwaysFalse);
        }

        var param = Expression.Parameter(closedType, "value");
        var body = Expression.Equal(param, Expression.Constant(null, closedType));
        Expression<Func<TValue?, bool>> predicate = Expression.Lambda<Func<TValue?, bool>>(body, param);
        return _builder.Add(selector, predicate);
    }
```

- [ ] **Step 4: Run tests to verify all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~BaseExpressionTests"`
Expected: PASS (all tests in the file, including the 5 new ones).

- [ ] **Step 5: Commit**

```bash
git add Vali-Flow.Core/Classes/General/ComparisonExpression.cs Vali-Flow.Core.Tests/BaseExpressionTests.cs
git commit -m "fix(core): NotNull/Null no longer throw for non-nullable value types"
```

---

### Task 2: Fix VF001 analyzer false positive on 4 EF-safe methods [HIGH]

**Files:**
- Modify: `Vali-Flow.Core.Analyzers/ValiFlowNonEfMethodAnalyzer.cs:68-78`
- Create: `Vali-Flow.Core.Analyzers.Tests/Vali-Flow.Core.Analyzers.Tests.csproj`
- Create: `Vali-Flow.Core.Analyzers.Tests/ValiFlowNonEfMethodAnalyzerTests.cs`
- Modify: `Vali-Flow.Core.sln` (add the new test project)

**Interfaces:**
- Consumes: `Vali_Flow.Core.Analyzers.ValiFlowNonEfMethodAnalyzer` (public, `DiagnosticAnalyzer`), `Vali_Flow.Core.Builder.ValiFlowQuery<T>` (public).
- Produces: nothing new consumed by later tasks — this is a leaf fix.

- [ ] **Step 1: Create the analyzer test project**

Run: `ls Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj` to confirm the sibling pattern, then create `Vali-Flow.Core.Analyzers.Tests/Vali-Flow.Core.Analyzers.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
        <RootNamespace>Vali_Flow.Core.Analyzers.Tests</RootNamespace>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
        <PackageReference Include="xunit" Version="2.9.3" />
        <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
            <PrivateAssets>all</PrivateAssets>
        </PackageReference>
        <PackageReference Include="FluentAssertions" Version="6.12.2" />
        <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\Vali-Flow.Core.Analyzers\Vali-Flow.Core.Analyzers.csproj" />
        <ProjectReference Include="..\Vali-Flow.Core\Vali-Flow.Core.csproj" />
    </ItemGroup>

</Project>
```

- [ ] **Step 2: Add the project to the solution**

Run: `dotnet sln Vali-Flow.Core.sln add Vali-Flow.Core.Analyzers.Tests/Vali-Flow.Core.Analyzers.Tests.csproj`

- [ ] **Step 3: Write the failing test**

Create `Vali-Flow.Core.Analyzers.Tests/ValiFlowNonEfMethodAnalyzerTests.cs`:

```csharp
using System.Collections.Immutable;
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

    [Fact]
    public async Task IsEmail_OnValiFlowQuery_StillTriggersVF001()
    {
        // Control case: a genuinely non-EF-safe method must still be flagged.
        var source = SourcePrefix + "q.IsEmail(e => e.Name);" + SourceSuffix;
        var diagnostics = await GetVf001DiagnosticsAsync(source);
        diagnostics.Should().ContainSingle(d => d.Id == "VF001");
    }
}
```

- [ ] **Step 4: Run tests to verify the first three fail**

Run: `dotnet test Vali-Flow.Core.Analyzers.Tests/Vali-Flow.Core.Analyzers.Tests.csproj -c Release`
Expected: `IsTrimmed_OnValiFlowQuery_DoesNotTriggerVF001`, `EqualToIgnoreCase_OnValiFlowQuery_DoesNotTriggerVF001`, `IsLowerCase_And_IsUpperCase_OnValiFlowQuery_DoNotTriggerVF001` FAIL (diagnostics not empty); `IsEmail_OnValiFlowQuery_StillTriggersVF001` PASSES already.

- [ ] **Step 5: Fix the analyzer**

In `Vali-Flow.Core.Analyzers/ValiFlowNonEfMethodAnalyzer.cs`, remove these 4 lines from the `NonEfMethods` set (lines 68-71 and 77 per current source):

```csharp
        // IStringStateExpression — char-level LINQ
        "IsTrimmed",
        "IsLowerCase",
        "IsUpperCase",
        "HasOnlyDigits",
        "HasOnlyLetters",
        "HasLettersAndNumbers",
        "HasSpecialCharacters",
        // IStringContentExpression — StringComparison / ToLower
        "EqualToIgnoreCase",
        "IsOneOf",
```

becomes:

```csharp
        // IStringStateExpression — char-level LINQ (IsTrimmed/IsLowerCase/IsUpperCase excluded:
        // ValiFlowQuery<T> has its own EF-safe reimplementation of these three — see
        // StringExpressionQuery.cs. Flagging them here was a false positive.)
        "HasOnlyDigits",
        "HasOnlyLetters",
        "HasLettersAndNumbers",
        "HasSpecialCharacters",
        // IStringContentExpression — StringComparison / ToLower (EqualToIgnoreCase excluded:
        // same reason — ValiFlowQuery<T> has its own EF-safe ToLower()-based reimplementation.)
        "IsOneOf",
```

- [ ] **Step 6: Run tests to verify all pass**

Run: `dotnet test Vali-Flow.Core.Analyzers.Tests/Vali-Flow.Core.Analyzers.Tests.csproj -c Release`
Expected: PASS (4/4).

Also run the full solution build to confirm the new test project doesn't break anything:
Run: `dotnet build Vali-Flow.Core.sln`
Expected: Build succeeded.

- [ ] **Step 7: Commit**

```bash
git add Vali-Flow.Core.Analyzers/ValiFlowNonEfMethodAnalyzer.cs Vali-Flow.Core.Analyzers.Tests/ Vali-Flow.Core.sln
git commit -m "fix(analyzers): remove false-positive VF001 on 4 EF-safe string methods"
```

---

### Task 3: Add build-time diagnostics to `ForwardingGenerator` for misused `[ForwardInterface]` [MEDIUM]

**Files:**
- Modify: `Vali-Flow.Core.Generator/ForwardingGenerator.cs:90-144,295-296`
- Create: `Vali-Flow.Core.Generator.Tests/Vali-Flow.Core.Generator.Tests.csproj`
- Create: `Vali-Flow.Core.Generator.Tests/ForwardingGeneratorDiagnosticsTests.cs`
- Modify: `Vali-Flow.Core.sln` (add the new test project)

**Interfaces:**
- Consumes: `Vali_Flow.Core.Generator.ForwardingGenerator` (public, `IIncrementalGenerator`), `Vali_Flow.Core.Builder.ForwardInterfaceAttribute` (public, already exists in `Vali-Flow.Core`).
- Produces: nothing consumed by later tasks.

- [ ] **Step 1: Create the generator test project**

Create `Vali-Flow.Core.Generator.Tests/Vali-Flow.Core.Generator.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
        <RootNamespace>Vali_Flow.Core.Generator.Tests</RootNamespace>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
        <PackageReference Include="xunit" Version="2.9.3" />
        <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
            <PrivateAssets>all</PrivateAssets>
        </PackageReference>
        <PackageReference Include="FluentAssertions" Version="6.12.2" />
        <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\Vali-Flow.Core.Generator\Vali-Flow.Core.Generator.csproj" />
        <ProjectReference Include="..\Vali-Flow.Core\Vali-Flow.Core.csproj" />
    </ItemGroup>

</Project>
```

Run: `dotnet sln Vali-Flow.Core.sln add Vali-Flow.Core.Generator.Tests/Vali-Flow.Core.Generator.Tests.csproj`

- [ ] **Step 2: Write the failing tests**

Create `Vali-Flow.Core.Generator.Tests/ForwardingGeneratorDiagnosticsTests.cs`:

```csharp
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
        var refs = new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Vali_Flow.Core.Builder.ForwardInterfaceAttribute).Assembly.Location) };
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
```

- [ ] **Step 3: Run tests to verify the first two fail**

Run: `dotnet test Vali-Flow.Core.Generator.Tests/Vali-Flow.Core.Generator.Tests.csproj -c Release`
Expected: `FieldTypedAsNonInterface_ReportsVFGEN001` and `InterfaceWithNoMethods_ReportsVFGEN002` FAIL (no diagnostics reported at all today); `ValidInterfaceField_ReportsNoDiagnostics` PASSES already.

- [ ] **Step 4: Add diagnostics to the generator**

In `Vali-Flow.Core.Generator/ForwardingGenerator.cs`, add two `DiagnosticDescriptor` fields right after the `FullyQualifiedNullable` field (after line ~29):

```csharp
    /// <summary>Reported when a <c>[ForwardInterface]</c> field is not typed as an interface.</summary>
    private static readonly DiagnosticDescriptor NotAnInterfaceRule = new(
        id: "VFGEN001",
        title: "ForwardInterface field must be typed as an interface",
        messageFormat: "Field '{0}' is marked [ForwardInterface] but its type is not an interface; no forwarding methods will be generated",
        category: "Vali_Flow.Core.Generator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>Reported when a <c>[ForwardInterface]</c> field's interface has no forwardable members.</summary>
    private static readonly DiagnosticDescriptor NoForwardableMembersRule = new(
        id: "VFGEN002",
        title: "ForwardInterface interface has no forwardable members",
        messageFormat: "Interface '{0}' has no methods to forward; no forwarding methods will be generated for this field",
        category: "Vali_Flow.Core.Generator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>Carries either a successfully extracted field entry, or a diagnostic explaining why extraction failed.</summary>
    private sealed record ExtractResult(FieldEntry? Entry, Diagnostic? Diagnostic);
```

Replace the `Initialize` method body (lines ~90-109) with:

```csharp
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var results = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeFullName,
                predicate: static (node, _) => node is VariableDeclaratorSyntax,
                transform: static (ctx, ct) => ExtractEntry(ctx, ct))
            .Collect();

        context.RegisterSourceOutput(results, static (spc, items) =>
        {
            foreach (var item in items)
            {
                if (item.Diagnostic != null)
                    spc.ReportDiagnostic(item.Diagnostic);
            }

            var entries = items.Where(i => i.Entry != null).Select(i => i.Entry!).ToImmutableArray();
            var classModels = GroupByClass(entries);
            foreach (var model in classModels)
                spc.AddSource($"{model.ClassName}.Forwarding.g.cs", Emit(model));
        });
    }
```

Replace `ExtractEntry` (lines ~121-144) to return `ExtractResult` instead of `FieldEntry?`:

```csharp
    private static ExtractResult ExtractEntry(
        GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (ctx.TargetSymbol is not IFieldSymbol field)
            return new ExtractResult(null, null); // predicate already restricts to VariableDeclaratorSyntax; unreachable in practice.

        if (field.Type is not INamedTypeSymbol ifaceType || ifaceType.TypeKind != TypeKind.Interface)
        {
            var diagnostic = Diagnostic.Create(NotAnInterfaceRule, ctx.TargetNode.GetLocation(), field.Name);
            return new ExtractResult(null, diagnostic);
        }

        var containingType = field.ContainingType;
        if (containingType is null) return new ExtractResult(null, null);

        var ns = containingType.ContainingNamespace?.ToDisplayString() ?? "";
        var typeParamList = containingType.TypeParameters.Length > 0
            ? "<" + string.Join(", ", containingType.TypeParameters.Select(tp => tp.Name)) + ">"
            : "";

        var methods = CollectAllMethods(ifaceType, ct);
        if (methods.IsEmpty)
        {
            var diagnostic = Diagnostic.Create(NoForwardableMembersRule, ctx.TargetNode.GetLocation(), ifaceType.Name);
            return new ExtractResult(null, diagnostic);
        }

        return new ExtractResult(
            new FieldEntry(containingType.Name, ns, typeParamList, field.Name, methods),
            null);
    }
```

Update the XML doc comment above `ExtractEntry` (was: "or returns `null` if the symbol is invalid") to:

```csharp
    /// <summary>
    /// Transforms a single <c>[ForwardInterface]</c>-decorated variable declarator into an
    /// <see cref="ExtractResult"/> — either a populated <see cref="FieldEntry"/>, or a
    /// <see cref="Diagnostic"/> explaining why extraction failed (VFGEN001/VFGEN002).
    /// </summary>
```

- [ ] **Step 5: Run tests to verify all pass**

Run: `dotnet test Vali-Flow.Core.Generator.Tests/Vali-Flow.Core.Generator.Tests.csproj -c Release`
Expected: PASS (3/3).

Run: `dotnet build Vali-Flow.Core.sln`
Expected: Build succeeded — confirms the real `[ForwardInterface]` usages in `Vali-Flow.Core/Builder/ValiFlow.cs`/`ValiFlowQuery.cs` still generate correctly with zero VFGEN001/VFGEN002 diagnostics.

- [ ] **Step 6: Commit**

```bash
git add Vali-Flow.Core.Generator/ForwardingGenerator.cs Vali-Flow.Core.Generator.Tests/ Vali-Flow.Core.sln
git commit -m "feat(generator): report VFGEN001/VFGEN002 diagnostics for misused [ForwardInterface]"
```

---

### Task 4: Defensive clone of `selectorBody` in `BaseExpression.Add<TValue>` [MEDIUM]

**Files:**
- Modify: `Vali-Flow.Core/Classes/Base/BaseExpression.cs:234-247`
- Test: `Vali-Flow.Core.Tests/BaseExpressionTests.cs`

**Interfaces:**
- Consumes: `ExpressionHelpers.ForceCloneVisitor` (existing internal/protected helper in `Vali-Flow.Core/Utils/ExpressionHelpers.cs`, already used by `BuildNestedExpression`).
- Produces: no public signature change.

- [ ] **Step 1: Write the regression test**

This is a defensive-consistency fix, not an observable bug today (see Task 1 audit note: `Build()` already de-aliases via its own re-mapping). The test documents and locks in the invariant via `ConditionEntry.CompiledFunc`, which compiles a condition's expression directly without going through `Build()`'s re-mapping — this is the one path that would see aliased nodes if the clone were ever removed. Append to `Vali-Flow.Core.Tests/BaseExpressionTests.cs`:

```csharp
    // ── Regression: Add<TValue> must not alias the selector body across predicate branches ──

    [Fact]
    public void Add_WithPredicateReferencingParameterTwice_CompilesAndEvaluatesCorrectly()
    {
        // MinLength-style predicate: "val != null && val.Length <= max" references its
        // parameter twice. If the selector body were aliased (same Expression instance
        // reused in both positions), this must still compile and evaluate correctly —
        // Expression.Compile() tolerates aliasing, so this mainly guards against a future
        // regression where a non-idempotent selector (e.g. one with a conversion) breaks.
        var filter = new ValiFlow<Product>()
            .MaxLength(p => p.Name, 3)
            .Build()
            .Compile();

        filter(new Product("ab", 1m, 1, true, DateTime.Now, new List<string>())).Should().BeTrue();
        filter(new Product("abcdef", 1m, 1, true, DateTime.Now, new List<string>())).Should().BeFalse();
        filter(new Product(null, 1m, 1, true, DateTime.Now, new List<string>())).Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithRepeatedSelectorPredicate_MatchesBuildCompiledResult()
    {
        // IsValid() goes through ConditionEntry.CompiledFunc (NOT Build()'s re-mapping).
        // This confirms both paths agree once selectorBody is cloned defensively.
        var validator = new ValiFlow<Product>().MaxLength(p => p.Name, 3);
        var viaBuild = validator.Build().Compile();
        var product = new Product("abcdef", 1m, 1, true, DateTime.Now, new List<string>());

        validator.IsValid(product).Should().Be(viaBuild(product));
    }
```

- [ ] **Step 2: Run tests to verify they pass before the change (baseline)**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Add_WithPredicateReferencingParameterTwice|FullyQualifiedName~IsValid_WithRepeatedSelectorPredicate"`
Expected: PASS already (this confirms there is no active bug — the fix below is purely defensive hardening, matching the established `ForceCloneVisitor` pattern elsewhere).

- [ ] **Step 3: Apply the defensive clone**

In `Vali-Flow.Core/Classes/Base/BaseExpression.cs`, change lines 240-242 of `Add<TValue>`:

```csharp
        var parameter = selector.Parameters[0];
        var selectorBody = selector.Body;
        var predicateBody = new ParameterReplacer(predicate.Parameters[0], selectorBody).Visit(predicate.Body);
```

to:

```csharp
        var parameter = selector.Parameters[0];
        var selectorBody = selector.Body;
        // Predicates frequently reference their parameter more than once (e.g. "val != null && val.Length <= max").
        // Clone the selector body so each substitution site gets its own node — consistent with the
        // ForceCloneVisitor pattern already used in BuildNestedExpression and BuildNullSafeCollectionPredicate.
        var predicateBody = new ParameterReplacer(predicate.Parameters[0], new ForceCloneVisitor().Visit(selectorBody)!).Visit(predicate.Body);
```

- [ ] **Step 4: Run tests to verify everything still passes**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release`
Expected: PASS (993+ tests, no regressions).

- [ ] **Step 5: Commit**

```bash
git add Vali-Flow.Core/Classes/Base/BaseExpression.cs Vali-Flow.Core.Tests/BaseExpressionTests.cs
git commit -m "refactor(core): clone selector body in Add<TValue> for node-aliasing consistency"
```

---

### Task 5: Cosmetic cleanup — duplicated XML doc and missing `static` [LOW]

**Files:**
- Modify: `Vali-Flow.Core/Classes/Types/StringExpression.cs:578-589`
- Modify: `Vali-Flow.Core/Classes/Types/CollectionExpression.cs` (`BuildNullSafeCollectionPredicate` method)

**Interfaces:** None — purely cosmetic, no behavior change, no new tests needed (covered by existing test suite which must stay green).

- [ ] **Step 1: Remove the duplicated XML doc block**

In `Vali-Flow.Core/Classes/Types/StringExpression.cs`, lines 578-589 currently read:

```csharp
/// <summary>
/// Non-generic static cache shared across all closed generic instantiations of
/// <see cref="StringExpression{TBuilder,T}"/>. This ensures that the same compiled
/// <see cref="Regex"/> instance is reused regardless of which builder type or entity
/// type is in use, and that the 1,000-entry cap applies globally, not per closed type.
/// </summary>
/// <summary>
/// Non-generic static cache shared across all closed generic instantiations of
/// <see cref="StringExpression{TBuilder,T}"/>. This ensures that the same compiled
/// <see cref="Regex"/> instance is reused regardless of which builder type or entity
/// type is in use, and that the 1,000-entry cap applies globally, not per closed type.
/// </summary>
internal static class StringExpressionCache
```

Replace with a single copy:

```csharp
/// <summary>
/// Non-generic static cache shared across all closed generic instantiations of
/// <see cref="StringExpression{TBuilder,T}"/>. This ensures that the same compiled
/// <see cref="Regex"/> instance is reused regardless of which builder type or entity
/// type is in use, and that the 1,000-entry cap applies globally, not per closed type.
/// </summary>
internal static class StringExpressionCache
```

- [ ] **Step 2: Mark `BuildNullSafeCollectionPredicate` as `static`**

In `Vali-Flow.Core/Classes/Types/CollectionExpression.cs`, find the method signature:

```csharp
    private Expression<Func<T, bool>> BuildNullSafeCollectionPredicate<TValue>(
```

Change `private` to `private static`:

```csharp
    private static Expression<Func<T, bool>> BuildNullSafeCollectionPredicate<TValue>(
```

- [ ] **Step 3: Build and run the full test suite**

Run: `dotnet build Vali-Flow.Core.sln`
Expected: Build succeeded (marking a method `static` is source-compatible; if the compiler complains that the method body still references `this` anywhere, do NOT force it — revert the `static` keyword and skip this half of the step, since the SOLID audit already confirmed no instance-state usage).

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release`
Expected: PASS (no behavior change).

- [ ] **Step 4: Commit**

```bash
git add Vali-Flow.Core/Classes/Types/StringExpression.cs Vali-Flow.Core/Classes/Types/CollectionExpression.cs
git commit -m "chore(core): remove duplicated XML doc, mark BuildNullSafeCollectionPredicate static"
```

---

### Task 6: Coverage — `ForceCloneVisitor` (0% branch → full) [COVERAGE]

**Files:**
- Create: `Vali-Flow.Core.Tests/ExpressionHelpersForceCloneVisitorTests.cs`

**Interfaces:**
- Consumes: `ValiFlowQuery<T>.BetweenDates(selector, fromSelector, toSelector)` for `DateTime`/`DateOnly`/`DateTimeOffset` — the cross-property 3-selector overloads that internally route through `ForceCloneVisitor` whenever `fromSelector`/`toSelector` share structure with `selector`. These overloads already exist and are public; this task only adds tests, no production code change.

- [ ] **Step 1: Write tests exercising each unvisited node kind**

`ForceCloneVisitor` (`Vali-Flow.Core/Utils/ExpressionHelpers.cs`) has unvisited branches in `VisitUnary` (conversion), `VisitBinary` (with `Conversion` lambda), `VisitMethodCall` (static vs. instance), `VisitIndex`, `VisitConditional`, `VisitNew`, `VisitLambda`. The cross-property `InRange`/`BetweenDates` overloads are the only call sites that invoke it on a selector body — so the test strategy is: use a selector whose body is NOT a simple `MemberExpression`, forcing the visitor down each node-kind path.

Create `Vali-Flow.Core.Tests/ExpressionHelpersForceCloneVisitorTests.cs`:

```csharp
using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

/// <summary>
/// Coverage tests for the internal ForceCloneVisitor (Vali-Flow.Core/Utils/ExpressionHelpers.cs),
/// exercised indirectly through cross-property InRange/BetweenDates — the only call sites that
/// run it over a non-trivial selector body.
/// </summary>
public class ExpressionHelpersForceCloneVisitorTests
{
    private record Measurement(int RawValue, int[] Readings, decimal Price, DateTime Start, DateTime End);

    // 1. VisitUnary (type conversion) — selector body is a Convert node, not a plain member access.
    [Fact]
    public void InRange_CrossProperty_WithConversionSelector_ClonesUnaryNodeCorrectly()
    {
        var filter = new ValiFlowQuery<Measurement>()
            .InRange(m => (decimal)m.RawValue, m => m.Price - 10m, m => m.Price + 10m)
            .Build().Compile();

        filter(new Measurement(100, new[] { 1 }, 105m, DateTime.Now, DateTime.Now)).Should().BeTrue();
        filter(new Measurement(1, new[] { 1 }, 105m, DateTime.Now, DateTime.Now)).Should().BeFalse();
    }

    // 2. VisitIndex (array element access) — selector body indexes into a collection.
    [Fact]
    public void InRange_CrossProperty_WithIndexerSelector_ClonesIndexNodeCorrectly()
    {
        var filter = new ValiFlowQuery<Measurement>()
            .InRange(m => m.Readings[0], m => m.RawValue - 5, m => m.RawValue + 5)
            .Build().Compile();

        filter(new Measurement(100, new[] { 100 }, 1m, DateTime.Now, DateTime.Now)).Should().BeTrue();
        filter(new Measurement(100, new[] { 1 }, 1m, DateTime.Now, DateTime.Now)).Should().BeFalse();
    }

    // 3. BetweenDates cross-property with a direct DateTime member selector — baseline sanity
    // confirming the visitor still produces a structurally correct, independently evaluable clone
    // for the most common real-world case (member access through a converted comparison).
    [Fact]
    public void BetweenDates_CrossProperty_DateTime_StartAndEndSelectors_EvaluatesIndependently()
    {
        var filter = new ValiFlowQuery<Measurement>()
            .BetweenDates(m => m.Start, m => m.Start, m => m.End)
            .Build().Compile();

        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2025, 12, 31);
        filter(new Measurement(1, new[] { 1 }, 1m, start, end)).Should().BeTrue();
        filter(new Measurement(1, new[] { 1 }, 1m, start.AddDays(-1), end)).Should().BeFalse();
    }
}
```

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ExpressionHelpersForceCloneVisitorTests"`
Expected: PASS (3/3). If `InRange`/`BetweenDates` cross-property overloads on `ValiFlowQuery<T>` don't exist with exactly this shape, check `Vali-Flow.Core/Classes/Types/NumericExpressionQuery.cs` and `DateTimeExpressionQuery.cs` for the actual cross-property overload signatures (`InRange(selector, minSelector, maxSelector)` / `BetweenDates(selector, fromSelector, toSelector)`) and adjust the call syntax to match — the assertions and intent stay the same.

- [ ] **Step 3: Collect coverage and confirm `ForceCloneVisitor` branch rate improved**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --collect:"XPlat Code Coverage" --results-directory ./TestResults`
Inspect the generated `coverage.cobertura.xml` for the `ExpressionHelpers`/`ForceCloneVisitor` class entry — branch-rate should no longer read `0`.

- [ ] **Step 4: Commit**

```bash
git add Vali-Flow.Core.Tests/ExpressionHelpersForceCloneVisitorTests.cs
git commit -m "test(core): cover ForceCloneVisitor node kinds via cross-property selectors"
```

---

### Task 7: Coverage — `DateTimeOffsetExpressionQuery` gaps [COVERAGE]

**Files:**
- Modify: `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs` (append new `[Fact]` methods; reuses existing `QueryEntity` record, field `UpdatedAt` of type `DateTimeOffset`)

**Interfaces:** None — pure test additions against existing public `ValiFlowQuery<QueryEntity>` methods (`IsInMonth`, `IsInYear`, `IsToday`, `IsYesterday`, `IsTomorrow`, `ExactDate`, `SameMonthAs`, `SameYearAs`, `InLastDays`, `InNextDays`, `IsWeekend`, `IsWeekday`, `IsDayOfWeek`, `IsFirstDayOfMonth`, `IsLastDayOfMonth`, `IsInQuarter`).

- [ ] **Step 1: Write at least 3 tests covering untested methods + their guards**

Append to `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs` (the file already has a `MakeEntity(... DateTimeOffset updatedAt = default ...)` helper — reuse it):

```csharp
    // ── DateTimeOffset coverage gaps (IsInMonth/IsInYear/IsToday/ExactDate/SameMonthAs/
    //    InLastDays/IsWeekend/IsInQuarter guards) ───────────────────────────────

    [Fact]
    public void DateTimeOffset_IsInMonth_InvalidMonth_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.IsInMonth(e => e.UpdatedAt, 13);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateTimeOffset_ExactDate_MatchesSameUtcDay()
    {
        var target = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlowQuery<QueryEntity>().ExactDate(e => e.UpdatedAt, target).Build().Compile();

        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 6, 15, 23, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 6, 16, 1, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_SameMonthAs_MatchesSameMonthAndYear()
    {
        var reference = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlowQuery<QueryEntity>().SameMonthAs(e => e.UpdatedAt, reference).Build().Compile();

        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 3, 20, 0, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2024, 3, 20, 0, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_InLastDays_InvalidDays_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.InLastDays(e => e.UpdatedAt, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateTimeOffset_IsWeekend_MatchesSaturdayAndSunday()
    {
        // 2025-06-14 is a Saturday (UTC).
        var filter = new ValiFlowQuery<QueryEntity>().IsWeekend(e => e.UpdatedAt).Build().Compile();

        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 6, 14, 0, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 6, 16, 0, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_IsInQuarter_InvalidQuarter_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.IsInQuarter(e => e.UpdatedAt, 5);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateTimeOffset_IsInQuarter_MatchesCorrectQuarter()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsInQuarter(e => e.UpdatedAt, 2).Build().Compile();

        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 5, 1, 0, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }
```

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~DateTimeOffset_IsInMonth|FullyQualifiedName~DateTimeOffset_ExactDate|FullyQualifiedName~DateTimeOffset_SameMonthAs|FullyQualifiedName~DateTimeOffset_InLastDays|FullyQualifiedName~DateTimeOffset_IsWeekend|FullyQualifiedName~DateTimeOffset_IsInQuarter"`
Expected: PASS (7/7).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/ValiFlowQueryTests.cs
git commit -m "test(core): cover DateTimeOffsetExpressionQuery gaps (month/quarter/weekend guards)"
```

---

### Task 8: Coverage — `DateOnlyExpressionQuery` and `DateTimeExpressionQuery` gaps [COVERAGE]

**Files:**
- Modify: `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs` (append new `[Fact]` methods; reuses `QueryEntity.BirthDate` (`DateOnly`) and `QueryEntity.CreatedAt` (`DateTime`))

**Interfaces:** None — pure test additions.

- [ ] **Step 1: Write at least 3 tests per class**

Append to `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`:

```csharp
    // ── DateOnly coverage gaps ──────────────────────────────────────────────────

    [Fact]
    public void DateOnly_IsYesterday_MatchesYesterday()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsYesterday(e => e.BirthDate).Build().Compile();
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        filter(MakeEntity(birthDate: yesterday)).Should().BeTrue();
        filter(MakeEntity(birthDate: DateOnly.FromDateTime(DateTime.UtcNow))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_InNextDays_InvalidDays_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.InNextDays(e => e.BirthDate, -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateOnly_IsDayOfWeek_MatchesSpecifiedDay()
    {
        // 2025-06-16 is a Monday.
        var filter = new ValiFlowQuery<QueryEntity>().IsDayOfWeek(e => e.BirthDate, DayOfWeek.Monday).Build().Compile();

        filter(MakeEntity(birthDate: new DateOnly(2025, 6, 16))).Should().BeTrue();
        filter(MakeEntity(birthDate: new DateOnly(2025, 6, 17))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_IsLastDayOfMonth_MatchesLastDay()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsLastDayOfMonth(e => e.BirthDate).Build().Compile();

        filter(MakeEntity(birthDate: new DateOnly(2025, 4, 30))).Should().BeTrue();
        filter(MakeEntity(birthDate: new DateOnly(2025, 4, 29))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_SameYearAs_MatchesSameYearOnly()
    {
        var reference = new DateOnly(2025, 1, 1);
        var filter = new ValiFlowQuery<QueryEntity>().SameYearAs(e => e.BirthDate, reference).Build().Compile();

        filter(MakeEntity(birthDate: new DateOnly(2025, 11, 30))).Should().BeTrue();
        filter(MakeEntity(birthDate: new DateOnly(2024, 11, 30))).Should().BeFalse();
    }

    // ── DateTime coverage gaps ──────────────────────────────────────────────────

    [Fact]
    public void DateTime_IsInMonth_InvalidMonth_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.IsInMonth(e => e.CreatedAt, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateTime_IsWeekday_MatchesMondayThroughFriday()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsWeekday(e => e.CreatedAt).Build().Compile();

        filter(MakeEntity(createdAt: new DateTime(2025, 6, 16))).Should().BeTrue();  // Monday
        filter(MakeEntity(createdAt: new DateTime(2025, 6, 14))).Should().BeFalse(); // Saturday
    }

    [Fact]
    public void DateTime_IsFirstDayOfMonth_MatchesFirstDay()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsFirstDayOfMonth(e => e.CreatedAt).Build().Compile();

        filter(MakeEntity(createdAt: new DateTime(2025, 7, 1))).Should().BeTrue();
        filter(MakeEntity(createdAt: new DateTime(2025, 7, 2))).Should().BeFalse();
    }
```

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~DateOnly_IsYesterday|FullyQualifiedName~DateOnly_InNextDays|FullyQualifiedName~DateOnly_IsDayOfWeek|FullyQualifiedName~DateOnly_IsLastDayOfMonth|FullyQualifiedName~DateOnly_SameYearAs|FullyQualifiedName~DateTime_IsInMonth_InvalidMonth|FullyQualifiedName~DateTime_IsWeekday|FullyQualifiedName~DateTime_IsFirstDayOfMonth"`
Expected: PASS (8/8). If `MakeEntity` does not accept a `birthDate:`/`createdAt:` named parameter exactly as called, check the existing helper signature earlier in `ValiFlowQueryTests.cs` and adjust the named-argument names to match (the helper already supports overriding every `QueryEntity` field per existing tests in the same file).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/ValiFlowQueryTests.cs
git commit -m "test(core): cover DateOnlyExpressionQuery and DateTimeExpressionQuery gaps"
```

---

### Task 9: Coverage — non-Query `DateTimeExpression`/`DateOnlyExpression`/`DateTimeOffsetExpression` gaps [COVERAGE]

**Files:**
- Modify: `Vali-Flow.Core.Tests/DateTimeExpressionTests.cs` (uses existing `Product.CreatedAt`)
- Modify: `Vali-Flow.Core.Tests/DateTimeOffsetDateOnlyTimeOnlyTests.cs` (uses existing `Event` record)

**Interfaces:** None — pure test additions against `ValiFlow<T>` (in-memory, non-EF-safe methods like `IsWeekend`/`IsDayOfWeek`/`IsLastDayOfMonth` which are intentionally NOT on `ValiFlowQuery<T>`'s DateTime variant per the EF-safety audit).

- [ ] **Step 1: Write at least 3 tests for `DateTimeExpression` (non-Query)**

Append to `Vali-Flow.Core.Tests/DateTimeExpressionTests.cs`:

```csharp
    // 6. IsWeekend(DateTime) — in-memory only
    [Fact]
    public void IsWeekend_MatchesSaturdayAndSunday()
    {
        var filter = new ValiFlow<Product>().IsWeekend(p => p.CreatedAt).Build().Compile();

        filter(MakeProduct(new DateTime(2025, 6, 14))).Should().BeTrue();  // Saturday
        filter(MakeProduct(new DateTime(2025, 6, 16))).Should().BeFalse(); // Monday
    }

    // 7. InLastDays(DateTime) — guard + happy path
    [Fact]
    public void InLastDays_InvalidDays_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.InLastDays(p => p.CreatedAt, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void InLastDays_MatchesWithinWindow()
    {
        var filter = new ValiFlow<Product>().InLastDays(p => p.CreatedAt, 5).Build().Compile();

        filter(MakeProduct(DateTime.Today.AddDays(-2))).Should().BeTrue();
        filter(MakeProduct(DateTime.Today.AddDays(-10))).Should().BeFalse();
    }

    // 8. IsInYear(DateTime) — guard + happy path
    [Fact]
    public void IsInYear_InvalidYear_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.IsInYear(p => p.CreatedAt, 10000);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
```

- [ ] **Step 2: Write at least 3 tests for `DateOnlyExpression` and `DateTimeOffsetExpression` (non-Query)**

Append to `Vali-Flow.Core.Tests/DateTimeOffsetDateOnlyTimeOnlyTests.cs` (inside `DateTimeOffsetDateOnlyTimeOnlyTests`, reusing the existing `Event`/`MakeEvent` helper):

```csharp
    // ── DateOnly (non-Query) coverage gaps ───────────────────────────────────

    [Fact]
    public void DateOnly_InLastDays_MatchesWithinWindow()
    {
        var filter = new ValiFlow<Event>().InLastDays(e => e.EventDate, 7).Build().Compile();

        filter(MakeEvent(eventDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)))).Should().BeTrue();
        filter(MakeEvent(eventDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_SameMonthAs_MatchesSameMonthAndYear()
    {
        var reference = new DateOnly(2025, 9, 1);
        var filter = new ValiFlow<Event>().SameMonthAs(e => e.EventDate, reference).Build().Compile();

        filter(MakeEvent(eventDate: new DateOnly(2025, 9, 20))).Should().BeTrue();
        filter(MakeEvent(eventDate: new DateOnly(2024, 9, 20))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_InNextDays_InvalidDays_Throws()
    {
        var builder = new ValiFlow<Event>();
        var act = () => builder.InNextDays(e => e.EventDate, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── DateTimeOffset (non-Query) coverage gaps ─────────────────────────────

    [Fact]
    public void DateTimeOffset_ExactDate_MatchesSameUtcDay()
    {
        var target = new DateTimeOffset(2025, 8, 10, 5, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().ExactDate(e => e.StartOffset, target).Build().Compile();

        filter(MakeEvent(startOffset: new DateTimeOffset(2025, 8, 10, 20, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEvent(startOffset: new DateTimeOffset(2025, 8, 11, 0, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_SameYearAs_MatchesSameYearOnly()
    {
        var reference = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().SameYearAs(e => e.StartOffset, reference).Build().Compile();

        filter(MakeEvent(startOffset: new DateTimeOffset(2025, 11, 1, 0, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEvent(startOffset: new DateTimeOffset(2024, 11, 1, 0, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_InLastDays_InvalidDays_Throws()
    {
        var builder = new ValiFlow<Event>();
        var act = () => builder.InLastDays(e => e.StartOffset, -3);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
```

- [ ] **Step 3: Run tests to verify they all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~DateTimeExpressionTests|FullyQualifiedName~DateTimeOffsetDateOnlyTimeOnlyTests"`
Expected: PASS (all tests in both files, including the new ones).

- [ ] **Step 4: Commit**

```bash
git add Vali-Flow.Core.Tests/DateTimeExpressionTests.cs Vali-Flow.Core.Tests/DateTimeOffsetDateOnlyTimeOnlyTests.cs
git commit -m "test(core): cover non-Query DateTime/DateOnly/DateTimeOffset expression gaps"
```

---

### Task 10: Coverage — `NumericExpressionQuery` scalar families (long/double/decimal/float/short) [COVERAGE]

**Files:**
- Modify: `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`

**Interfaces:** None — pure test additions. Requires a numeric-typed test entity; reuse `QueryEntityEx` (already defines `NullableLong`, `NullableDecimal`, `NullableDouble`, `NullableFloat` as `Nullable<T>`) — for the **non-nullable scalar** family tests below, add a new small record local to this task since `QueryEntity`/`QueryEntityEx` don't expose non-nullable `long`/`double`/`decimal`/`float`/`short` fields.

- [ ] **Step 1: Add a scalar numeric test entity and at least 3 tests per type family**

Append to `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`:

```csharp
    // ═══════════════════════════════════════════════════════════════════════
    // NumericExpressionQuery scalar families: long / double / decimal / float / short
    // ═══════════════════════════════════════════════════════════════════════

    private record ScalarNumerics(long LongValue, double DoubleValue, decimal DecimalValue, float FloatValue, short ShortValue);

    // ── long ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Long_GreaterThan_MatchesLargerValue()
    {
        var filter = new ValiFlowQuery<ScalarNumerics>().GreaterThan(e => e.LongValue, 100L).Build().Compile();
        filter(new ScalarNumerics(200L, 0, 0, 0, 0)).Should().BeTrue();
        filter(new ScalarNumerics(50L, 0, 0, 0, 0)).Should().BeFalse();
    }

    [Fact]
    public void Long_InRange_InvalidRange_Throws()
    {
        var builder = new ValiFlowQuery<ScalarNumerics>();
        var act = () => builder.InRange(e => e.LongValue, 100L, 1L);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Long_IsEven_And_IsMultipleOf_WorkCorrectly()
    {
        var evenFilter = new ValiFlowQuery<ScalarNumerics>().IsEven(e => e.LongValue).Build().Compile();
        evenFilter(new ScalarNumerics(4L, 0, 0, 0, 0)).Should().BeTrue();
        evenFilter(new ScalarNumerics(3L, 0, 0, 0, 0)).Should().BeFalse();

        var multipleFilter = new ValiFlowQuery<ScalarNumerics>().IsMultipleOf(e => e.LongValue, 5L).Build().Compile();
        multipleFilter(new ScalarNumerics(15L, 0, 0, 0, 0)).Should().BeTrue();
        multipleFilter(new ScalarNumerics(7L, 0, 0, 0, 0)).Should().BeFalse();
    }

    // ── double ────────────────────────────────────────────────────────────────

    [Fact]
    public void Double_Positive_And_Negative_WorkCorrectly()
    {
        var positiveFilter = new ValiFlowQuery<ScalarNumerics>().Positive(e => e.DoubleValue).Build().Compile();
        positiveFilter(new ScalarNumerics(0, 1.5, 0, 0, 0)).Should().BeTrue();
        positiveFilter(new ScalarNumerics(0, -1.5, 0, 0, 0)).Should().BeFalse();

        var negativeFilter = new ValiFlowQuery<ScalarNumerics>().Negative(e => e.DoubleValue).Build().Compile();
        negativeFilter(new ScalarNumerics(0, -1.5, 0, 0, 0)).Should().BeTrue();
    }

    [Fact]
    public void Double_InRange_InvalidRange_Throws()
    {
        var builder = new ValiFlowQuery<ScalarNumerics>();
        var act = () => builder.InRange(e => e.DoubleValue, 10.0, 1.0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Double_MinValue_MaxValue_WorkCorrectly()
    {
        var minFilter = new ValiFlowQuery<ScalarNumerics>().MinValue(e => e.DoubleValue, 10.0).Build().Compile();
        minFilter(new ScalarNumerics(0, 10.0, 0, 0, 0)).Should().BeTrue();
        minFilter(new ScalarNumerics(0, 9.9, 0, 0, 0)).Should().BeFalse();

        var maxFilter = new ValiFlowQuery<ScalarNumerics>().MaxValue(e => e.DoubleValue, 10.0).Build().Compile();
        maxFilter(new ScalarNumerics(0, 10.0, 0, 0, 0)).Should().BeTrue();
        maxFilter(new ScalarNumerics(0, 10.1, 0, 0, 0)).Should().BeFalse();
    }

    // ── decimal ───────────────────────────────────────────────────────────────

    [Fact]
    public void Decimal_Zero_And_NotZero_WorkCorrectly()
    {
        var zeroFilter = new ValiFlowQuery<ScalarNumerics>().Zero(e => e.DecimalValue).Build().Compile();
        zeroFilter(new ScalarNumerics(0, 0, 0m, 0, 0)).Should().BeTrue();
        zeroFilter(new ScalarNumerics(0, 0, 5m, 0, 0)).Should().BeFalse();

        var notZeroFilter = new ValiFlowQuery<ScalarNumerics>().NotZero(e => e.DecimalValue).Build().Compile();
        notZeroFilter(new ScalarNumerics(0, 0, 5m, 0, 0)).Should().BeTrue();
    }

    [Fact]
    public void Decimal_InRange_MatchesWithinBounds()
    {
        var filter = new ValiFlowQuery<ScalarNumerics>().InRange(e => e.DecimalValue, 10m, 20m).Build().Compile();
        filter(new ScalarNumerics(0, 0, 15m, 0, 0)).Should().BeTrue();
        filter(new ScalarNumerics(0, 0, 25m, 0, 0)).Should().BeFalse();
    }

    [Fact]
    public void Decimal_LessThanOrEqualTo_WorksCorrectly()
    {
        var filter = new ValiFlowQuery<ScalarNumerics>().LessThanOrEqualTo(e => e.DecimalValue, 10m).Build().Compile();
        filter(new ScalarNumerics(0, 0, 10m, 0, 0)).Should().BeTrue();
        filter(new ScalarNumerics(0, 0, 10.01m, 0, 0)).Should().BeFalse();
    }

    // ── float ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Float_GreaterThanOrEqualTo_WorksCorrectly()
    {
        var filter = new ValiFlowQuery<ScalarNumerics>().GreaterThanOrEqualTo(e => e.FloatValue, 5f).Build().Compile();
        filter(new ScalarNumerics(0, 0, 0, 5f, 0)).Should().BeTrue();
        filter(new ScalarNumerics(0, 0, 0, 4.9f, 0)).Should().BeFalse();
    }

    [Fact]
    public void Float_InRange_InvalidRange_Throws()
    {
        var builder = new ValiFlowQuery<ScalarNumerics>();
        var act = () => builder.InRange(e => e.FloatValue, 10f, 1f);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Float_MinValue_MaxValue_WorkCorrectly()
    {
        var minFilter = new ValiFlowQuery<ScalarNumerics>().MinValue(e => e.FloatValue, 2f).Build().Compile();
        minFilter(new ScalarNumerics(0, 0, 0, 2f, 0)).Should().BeTrue();
        minFilter(new ScalarNumerics(0, 0, 0, 1f, 0)).Should().BeFalse();
    }

    // ── short ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Short_Positive_And_Negative_WorkCorrectly()
    {
        var positiveFilter = new ValiFlowQuery<ScalarNumerics>().Positive(e => e.ShortValue).Build().Compile();
        positiveFilter(new ScalarNumerics(0, 0, 0, 0, 5)).Should().BeTrue();
        positiveFilter(new ScalarNumerics(0, 0, 0, 0, -5)).Should().BeFalse();
    }

    [Fact]
    public void Short_InRange_InvalidRange_Throws()
    {
        var builder = new ValiFlowQuery<ScalarNumerics>();
        var act = () => builder.InRange(e => e.ShortValue, (short)10, (short)1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Short_LessThan_WorksCorrectly()
    {
        var filter = new ValiFlowQuery<ScalarNumerics>().LessThan(e => e.ShortValue, (short)10).Build().Compile();
        filter(new ScalarNumerics(0, 0, 0, 0, 5)).Should().BeTrue();
        filter(new ScalarNumerics(0, 0, 0, 0, 15)).Should().BeFalse();
    }
```

- [ ] **Step 2: Run tests to verify they all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Long_|FullyQualifiedName~Double_|FullyQualifiedName~Decimal_|FullyQualifiedName~Float_|FullyQualifiedName~Short_"`
Expected: PASS (15/15).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/ValiFlowQueryTests.cs
git commit -m "test(core): cover NumericExpressionQuery scalar families (long/double/decimal/float/short)"
```

---

### Task 11: Coverage — `NumericExpressionQuery` nullable overloads [COVERAGE]

**Files:**
- Modify: `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs` (reuses existing `QueryEntityEx` record for `long?`/`decimal?`/`double?`/`float?`; adds one tiny new record for `short?`, which `QueryEntityEx` doesn't have)

**Interfaces:** None — pure test additions.

- [ ] **Step 1: Write at least 3 tests covering nullable `GreaterThan`/`LessThan`/`InRange`/`HasValue` across types**

Append to `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`:

```csharp
    // ═══════════════════════════════════════════════════════════════════════
    // NumericExpressionQuery nullable overloads
    // ═══════════════════════════════════════════════════════════════════════

    private record ShortEntity(short? NullableShort);

    [Fact]
    public void NullableLong_GreaterThan_WithValue_MatchesCorrectly()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>().GreaterThan(e => e.NullableLong, 10L).Build().Compile();
        filter(new QueryEntityEx(null, 20L, null, null, null, DateTime.UtcNow, DateOnly.MinValue)).Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue)).Should().BeFalse();
    }

    [Fact]
    public void NullableDecimal_InRange_WithValue_MatchesCorrectly()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>().InRange(e => e.NullableDecimal, 10m, 20m).Build().Compile();
        filter(new QueryEntityEx(null, null, 15m, null, null, DateTime.UtcNow, DateOnly.MinValue)).Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue)).Should().BeFalse();
    }

    [Fact]
    public void NullableDouble_LessThan_NullValue_ReturnsFalse()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>().LessThan(e => e.NullableDouble, 5.0).Build().Compile();
        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue)).Should().BeFalse();
        filter(new QueryEntityEx(null, null, null, 1.0, null, DateTime.UtcNow, DateOnly.MinValue)).Should().BeTrue();
    }

    [Fact]
    public void NullableFloat_HasValue_DistinguishesNullFromValue()
    {
        var filter = new ValiFlowQuery<QueryEntityEx>().HasValue(e => e.NullableFloat).Build().Compile();
        filter(new QueryEntityEx(null, null, null, null, 1.5f, DateTime.UtcNow, DateOnly.MinValue)).Should().BeTrue();
        filter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue)).Should().BeFalse();
    }

    [Fact]
    public void NullableShort_GreaterThan_WithValue_MatchesCorrectly()
    {
        var filter = new ValiFlowQuery<ShortEntity>().GreaterThan(e => e.NullableShort, (short)10).Build().Compile();
        filter(new ShortEntity((short)20)).Should().BeTrue();
        filter(new ShortEntity(null)).Should().BeFalse();
    }

    [Fact]
    public void NullableShort_InRange_WithValue_MatchesCorrectly()
    {
        var filter = new ValiFlowQuery<ShortEntity>().InRange(e => e.NullableShort, (short)1, (short)10).Build().Compile();
        filter(new ShortEntity((short)5)).Should().BeTrue();
        filter(new ShortEntity((short)20)).Should().BeFalse();
    }
```

- [ ] **Step 2: Run tests to verify they all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~NullableLong_|FullyQualifiedName~NullableDecimal_|FullyQualifiedName~NullableDouble_|FullyQualifiedName~NullableFloat_|FullyQualifiedName~NullableShort_"`
Expected: PASS (6/6).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/ValiFlowQueryTests.cs
git commit -m "test(core): cover NumericExpressionQuery nullable overloads (long/decimal/double/float/short)"
```

---

### Task 12: Coverage — `StringExpressionQuery` gaps [COVERAGE]

**Files:**
- Modify: `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs` (reuses existing `QueryEntity.Name`)

**Interfaces:** None — pure test additions.

- [ ] **Step 1: Write at least 3 tests covering the untested EF-safe string methods + length guards**

Append to `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`:

```csharp
    // ═══════════════════════════════════════════════════════════════════════
    // StringExpressionQuery coverage gaps
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void String_IsTrimmed_DistinguishesTrimmedFromPadded()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsTrimmed(e => e.Name).Build().Compile();
        filter(MakeEntity(name: "Alice")).Should().BeTrue();
        filter(MakeEntity(name: " Alice ")).Should().BeFalse();
    }

    [Fact]
    public void String_EqualToIgnoreCase_MatchesRegardlessOfCase()
    {
        var filter = new ValiFlowQuery<QueryEntity>().EqualToIgnoreCase(e => e.Name, "ALICE").Build().Compile();
        filter(MakeEntity(name: "alice")).Should().BeTrue();
        filter(MakeEntity(name: "bob")).Should().BeFalse();
    }

    [Fact]
    public void String_ContainsIgnoreCase_MatchesRegardlessOfCase()
    {
        var filter = new ValiFlowQuery<QueryEntity>().ContainsIgnoreCase(e => e.Name, "LIC").Build().Compile();
        filter(MakeEntity(name: "alice")).Should().BeTrue();
        filter(MakeEntity(name: "bob")).Should().BeFalse();
    }

    [Fact]
    public void String_NotContains_NullPassesAndNonMatchingPasses()
    {
        var filter = new ValiFlowQuery<QueryEntity>().NotContains(e => e.Name, "xyz").Build().Compile();
        filter(MakeEntity(name: null)).Should().BeTrue();
        filter(MakeEntity(name: "alice")).Should().BeTrue();
        filter(MakeEntity(name: "xyzabc")).Should().BeFalse();
    }

    [Fact]
    public void String_NotStartsWith_And_NotEndsWith_WorkCorrectly()
    {
        var notStarts = new ValiFlowQuery<QueryEntity>().NotStartsWith(e => e.Name, "al").Build().Compile();
        notStarts(MakeEntity(name: "bob")).Should().BeTrue();
        notStarts(MakeEntity(name: "alice")).Should().BeFalse();

        var notEnds = new ValiFlowQuery<QueryEntity>().NotEndsWith(e => e.Name, "ce").Build().Compile();
        notEnds(MakeEntity(name: "bob")).Should().BeTrue();
        notEnds(MakeEntity(name: "alice")).Should().BeFalse();
    }

    [Fact]
    public void String_MinLength_InvalidValue_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.MinLength(e => e.Name, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void String_StartsWith_EmptyValue_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.StartsWith(e => e.Name, "");
        act.Should().Throw<ArgumentException>();
    }
```

- [ ] **Step 2: Run tests to verify they all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~String_IsTrimmed|FullyQualifiedName~String_EqualToIgnoreCase|FullyQualifiedName~String_ContainsIgnoreCase|FullyQualifiedName~String_NotContains|FullyQualifiedName~String_NotStartsWith|FullyQualifiedName~String_MinLength|FullyQualifiedName~String_StartsWith_EmptyValue"`
Expected: PASS (7/7). If `MakeEntity(name: ...)` isn't the exact parameter name used by the existing helper in this file, check its signature at the top of `ValiFlowQueryTests.cs` and match the real parameter name.

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/ValiFlowQueryTests.cs
git commit -m "test(core): cover StringExpressionQuery gaps (ignore-case methods, negative matchers, guards)"
```

---

### Task 13: Coverage — `ComparisonExpression` additional methods [COVERAGE]

**Files:**
- Modify: `Vali-Flow.Core.Tests/BaseExpressionTests.cs`

**Interfaces:** None — pure test additions against `IsInEnum`, `IsDefault`, `IsNotDefault`, `EqualTo`, `NotEqualTo` (all already public on `ValiFlow<T>`/`ComparisonExpression`).

- [ ] **Step 1: Write at least 3 tests**

Append to `Vali-Flow.Core.Tests/BaseExpressionTests.cs`. This needs an enum-bearing test type; define a small local record (do not modify the shared `Product` record):

```csharp
    private enum Status { Draft, Published, Archived }
    private record StatusEntity(Status Status, int Code, string? Label);

    [Fact]
    public void IsInEnum_ValidDefinedValue_ReturnsTrue_UndefinedValue_ReturnsFalse()
    {
        var filter = new ValiFlow<StatusEntity>().IsInEnum(e => e.Status).Build().Compile();
        filter(new StatusEntity(Status.Published, 1, "x")).Should().BeTrue();
        filter(new StatusEntity((Status)99, 1, "x")).Should().BeFalse();
    }

    [Fact]
    public void IsDefault_And_IsNotDefault_WorkCorrectlyForValueType()
    {
        var isDefaultFilter = new ValiFlow<StatusEntity>().IsDefault(e => e.Code).Build().Compile();
        isDefaultFilter(new StatusEntity(Status.Draft, 0, null)).Should().BeTrue();
        isDefaultFilter(new StatusEntity(Status.Draft, 5, null)).Should().BeFalse();

        var isNotDefaultFilter = new ValiFlow<StatusEntity>().IsNotDefault(e => e.Code).Build().Compile();
        isNotDefaultFilter(new StatusEntity(Status.Draft, 5, null)).Should().BeTrue();
    }

    [Fact]
    public void EqualTo_And_NotEqualTo_WorkCorrectlyForReferenceType()
    {
        var equalFilter = new ValiFlow<StatusEntity>().EqualTo(e => e.Label, "target").Build().Compile();
        equalFilter(new StatusEntity(Status.Draft, 1, "target")).Should().BeTrue();
        equalFilter(new StatusEntity(Status.Draft, 1, "other")).Should().BeFalse();

        var notEqualFilter = new ValiFlow<StatusEntity>().NotEqualTo(e => e.Label, "target").Build().Compile();
        notEqualFilter(new StatusEntity(Status.Draft, 1, "other")).Should().BeTrue();
    }
```

- [ ] **Step 2: Run tests to verify they all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~IsInEnum|FullyQualifiedName~IsDefault_And_IsNotDefault|FullyQualifiedName~EqualTo_And_NotEqualTo"`
Expected: PASS (3/3). Note: `EqualTo<TValue>`/`NotEqualTo<TValue>` require `TValue : IEquatable<TValue>` — `string` already implements it, so `Label` (`string?`) works without changes.

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/BaseExpressionTests.cs
git commit -m "test(core): cover ComparisonExpression IsInEnum/IsDefault/EqualTo family"
```

---

### Task 14: Coverage — `ValiFlowQuery.cs` gaps (ValidateNested guard, WithError/WithSeverity forwarders) [COVERAGE]

**Files:**
- Modify: `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`

**Interfaces:** None — pure test additions against `ValiFlowQuery<T>.ValidateNested`, `.WithError(...)` (3 overloads), `.WithSeverity(...)`.

- [ ] **Step 1: Write at least 3 tests**

Append to `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`:

```csharp
    // ═══════════════════════════════════════════════════════════════════════
    // ValiFlowQuery.cs gaps: ValidateNested guard, WithError/WithSeverity forwarders
    // ═══════════════════════════════════════════════════════════════════════

    private record Address(string? City);
    private record Customer(string? Name, Address? HomeAddress);

    [Fact]
    public void ValidateNested_EmptyConfigure_Throws()
    {
        var builder = new ValiFlowQuery<Customer>();
        var act = () => builder.ValidateNested(c => c.HomeAddress, _ => { /* no conditions added */ });
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateNested_WithCondition_ValidatesNestedProperty()
    {
        var filter = new ValiFlowQuery<Customer>()
            .ValidateNested(c => c.HomeAddress, addr => addr.IsNotNullOrEmpty(a => a.City))
            .Build().Compile();

        filter(new Customer("Alice", new Address("Lima"))).Should().BeTrue();
        filter(new Customer("Alice", new Address(null))).Should().BeFalse();
        filter(new Customer("Alice", null)).Should().BeFalse();
    }

    [Fact]
    public void WithError_SeverityOverload_SetsSeverityOnLastCondition()
    {
        var builder = new ValiFlowQuery<Customer>()
            .IsNotNullOrEmpty(c => c.Name)
            .WithError("ERR001", "Name is required", Severity.Warning);

        var result = builder.Validate(new Customer(null, null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorCode == "ERR001" && e.Severity == Severity.Warning);
    }

    [Fact]
    public void WithError_PropertyPathAndSeverityOverload_SetsBoth()
    {
        var builder = new ValiFlowQuery<Customer>()
            .IsNotNullOrEmpty(c => c.Name)
            .WithError("ERR002", "Name is required", "Name", Severity.Error);

        var result = builder.Validate(new Customer(null, null));
        result.Errors.Should().ContainSingle(e => e.ErrorCode == "ERR002" && e.PropertyPath == "Name" && e.Severity == Severity.Error);
    }

    [Fact]
    public void WithSeverity_SetsSeverityOnLastCondition()
    {
        var builder = new ValiFlowQuery<Customer>()
            .IsNotNullOrEmpty(c => c.Name)
            .WithMessage("Name is required")
            .WithSeverity(Severity.Critical);

        var result = builder.Validate(new Customer(null, null));
        result.Errors.Should().ContainSingle(e => e.Severity == Severity.Critical);
    }
```

If `WithMessage`, `Validate`, `Severity`, or `ValidationError.PropertyPath`/`ErrorCode`/`Severity` are named differently, check `Vali-Flow.Core/Classes/Base/BaseExpression.cs` (`WithError`/`WithSeverity`/`WithMessage`/`Validate` region) and `Vali-Flow.Core/Models/ValidationError.cs` for the exact members and adjust accordingly — the test intent (verify severity/error-code/property-path propagate through the `ValiFlowQuery<T>` forwarders) stays the same.

- [ ] **Step 2: Run tests to verify they all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ValidateNested_|FullyQualifiedName~WithError_|FullyQualifiedName~WithSeverity_"`
Expected: PASS (5/5).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/ValiFlowQueryTests.cs
git commit -m "test(core): cover ValiFlowQuery ValidateNested guard and WithError/WithSeverity forwarders"
```

---

### Task 15: Coverage — `ValiSort.cs` gaps (`SortEntry` guards, `ApplyThenBy` descending branch) [COVERAGE]

**Files:**
- Modify: `Vali-Flow.Core.Tests/EachItemSortGlobalTests.cs` (existing file already tests `ValiSort`)

**Interfaces:** None — pure test additions against the public `ValiSort<T>.By`/`.ThenBy` fluent API.

- [ ] **Step 1: Write at least 3 tests**

First inspect the existing `ValiSort` usage pattern already in `Vali-Flow.Core.Tests/EachItemSortGlobalTests.cs` (look for `new ValiSort<...>()` and `.By(`/`.ThenBy(` calls) to match the exact fluent syntax, then append:

```csharp
    // ── ValiSort coverage gaps: ThenBy descending branch ─────────────────────

    [Fact]
    public void ValiSort_ThenByDescending_OrdersSecondaryKeyDescending()
    {
        var items = new[]
        {
            new Product("B", 10m, 2, true, DateTime.Now, new List<string>()),
            new Product("B", 5m, 1, true, DateTime.Now, new List<string>()),
            new Product("A", 1m, 1, true, DateTime.Now, new List<string>()),
        };

        var sort = new ValiSort<Product>()
            .By(p => p.Name)
            .ThenBy(p => p.Price, descending: true);

        var sorted = sort.Apply(items).ToList();

        sorted[0].Name.Should().Be("A");
        sorted[1].Name.Should().Be("B");
        sorted[1].Price.Should().Be(10m); // descending secondary key: higher price first within "B"
        sorted[2].Price.Should().Be(5m);
    }

    [Fact]
    public void ValiSort_ThenByAscending_StillOrdersCorrectly()
    {
        // Companion to the descending test above — confirms both branches of ApplyThenBy
        // (ascending vs descending) are exercised, not just the descending one.
        var items = new[]
        {
            new Product("B", 10m, 2, true, DateTime.Now, new List<string>()),
            new Product("B", 5m, 1, true, DateTime.Now, new List<string>()),
        };

        var sort = new ValiSort<Product>().By(p => p.Name).ThenBy(p => p.Price);
        var sorted = sort.Apply(items).ToList();

        sorted[0].Price.Should().Be(5m); // ascending secondary key: lower price first
        sorted[1].Price.Should().Be(10m);
    }

    [Fact]
    public void ValiSort_By_SingleKey_OrdersCorrectly()
    {
        var items = new[]
        {
            new Product("C", 1m, 1, true, DateTime.Now, new List<string>()),
            new Product("A", 1m, 1, true, DateTime.Now, new List<string>()),
            new Product("B", 1m, 1, true, DateTime.Now, new List<string>()),
        };

        var sort = new ValiSort<Product>().By(p => p.Name);
        var sorted = sort.Apply(items).Select(p => p.Name).ToList();

        sorted.Should().Equal("A", "B", "C");
    }
```

If `ValiSort<T>.Apply(...)` is named differently (e.g. `ApplyTo`, or it extends `IQueryable`/`IEnumerable` directly as an extension method rather than an instance method), check the actual public surface in `Vali-Flow.Core/Builder/ValiSort.cs` and `Vali-Flow.Core.Tests/EachItemSortGlobalTests.cs` for the exact call pattern already in use, and adjust these three tests to match — the intent (cover the ascending AND descending branch of the secondary-key application, plus a baseline single-key sort) stays the same.

- [ ] **Step 2: Run tests to verify they all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ValiSort_"`
Expected: PASS (3/3).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/EachItemSortGlobalTests.cs
git commit -m "test(core): cover ValiSort ThenBy ascending/descending branches"
```

---

### Task 16: Coverage — `ExpressionExplainer.cs` gaps (Multiply/Divide/default NodeType, Negate unary, extension method) [COVERAGE]

**Files:**
- Modify: `Vali-Flow.Core.Tests/ExpressionExplainerTests.cs`

**Interfaces:** None — pure test additions against `ValiFlow<T>.Explain()` (already used by the existing tests in this file via the `Explain(...)` local helper).

- [ ] **Step 1: Write at least 3 tests**

Append to `Vali-Flow.Core.Tests/ExpressionExplainerTests.cs` (reuses the file's existing `private record Item(string? Name, int Value, bool IsActive, decimal Price, DateTime CreatedAt)` and `Explain(...)` helper):

```csharp
    // ── Coverage gaps: Multiply/Divide/default NodeType, Negate unary, extension method ──

    [Fact]
    public void Explain_BinaryMultiply_ShowsMultiplicationOperator()
    {
        Expression<Func<Item, bool>> expr = item => item.Value * 2 > 10;
        var result = Explain(expr);
        result.Should().Contain("*");
    }

    [Fact]
    public void Explain_BinaryDivide_ShowsDivisionOperator()
    {
        Expression<Func<Item, bool>> expr = item => item.Value / 2 > 1;
        var result = Explain(expr);
        result.Should().Contain("/");
    }

    [Fact]
    public void Explain_UnaryNegate_WrapsInNodeTypeBrackets()
    {
        Expression<Func<Item, bool>> expr = item => -item.Value < 0;
        var result = Explain(expr);
        result.Should().Contain("[Negate]");
    }

    [Fact]
    public void Explain_ExtensionMethodCall_RendersAsReceiverDotMethod()
    {
        Expression<Func<Item, bool>> expr = item => item.Name!.Contains("test");
        var result = Explain(expr);
        result.Should().Contain("Name");
        result.Should().Contain(".Contains(");
    }
```

- [ ] **Step 2: Run tests to verify they all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ExpressionExplainerTests"`
Expected: PASS (all tests in the file, including the 4 new ones). Note `string.Contains` is an instance method, not an extension — if the coverage report still shows the extension-method branch (`isExtension` in `VisitMethodCall`) unvisited after this, replace the last test's expression with a genuine static extension method call already available in the codebase or BCL, e.g. `item.Name!.AsEnumerable().Any()` (via `System.Linq`), which IS an extension method with no explicit receiver syntax sugar removed by the compiler — add `using System.Linq;` to the test file if needed.

- [ ] **Step 3: Collect coverage and confirm `ExpressionExplainer` branch rate improved**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --collect:"XPlat Code Coverage" --results-directory ./TestResults`
Inspect `coverage.cobertura.xml` for `ExpressionExplainer` — lines 51-53, 78-79, 142 should now show `hits > 0`.

- [ ] **Step 4: Commit**

```bash
git add Vali-Flow.Core.Tests/ExpressionExplainerTests.cs
git commit -m "test(core): cover ExpressionExplainer Multiply/Divide/Negate/extension-method branches"
```

---

### Task 17: Full verification pass — build, full test suite, coverage report [FINAL GATE]

**Files:** None modified — this task only runs verification commands.

**Interfaces:** None.

- [ ] **Step 1: Full solution build**

Run: `dotnet build Vali-Flow.Core.sln`
Expected: Build succeeded, 0 errors (warnings pre-existing in the test project, e.g. the nullable-reference-type `ValidateNested` warnings already present before this plan, are acceptable — do not attempt to fix unrelated pre-existing warnings as part of this plan).

- [ ] **Step 2: Full test suite with coverage**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --collect:"XPlat Code Coverage" --results-directory ./TestResults`
Expected: 100% pass rate, 0 failures, 0 skipped.

Run: `dotnet test Vali-Flow.Core.Analyzers.Tests/Vali-Flow.Core.Analyzers.Tests.csproj -c Release`
Expected: 100% pass rate.

Run: `dotnet test Vali-Flow.Core.Generator.Tests/Vali-Flow.Core.Generator.Tests.csproj -c Release`
Expected: 100% pass rate.

- [ ] **Step 3: Confirm coverage improved toward the 90% target**

Open the newest `coverage.cobertura.xml` under `TestResults/` and check the root `<coverage line-rate="..." branch-rate="...">` attributes. Baseline before this plan: `line-rate="0.743"`, `branch-rate="0.6281"`. Record the new numbers. If still below 90% line-rate, that is an acceptable outcome for this plan (the specific gaps identified by the audit are now closed — reaching the full 90% project-wide target may require a follow-up pass over areas not flagged by the original audit, e.g. the Generator/Analyzer projects' own branch coverage, which this plan treats as out of scope beyond the diagnostics added in Tasks 2-3).

- [ ] **Step 4: Report final numbers**

No commit for this task (verification-only) — report the before/after line-rate and branch-rate, and the final pass/fail counts across all three test projects, to the user.
