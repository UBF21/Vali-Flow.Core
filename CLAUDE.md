# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Vali-Flow.Core is a dependency-free .NET library providing a fluent API for building LINQ expression trees (`Expression<Func<T, bool>>`) for validation and filtering. It targets net8.0 and net9.0 and is distributed as a NuGet package.

## Build, Test & Pack Commands

```bash
# Build
dotnet build

# Build release
dotnet build --configuration Release

# Run all tests
dotnet test

# Run tests (release)
dotnet test --configuration Release

# Run a single test
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Pack NuGet package
dotnet pack --configuration Release
```

## Architecture

**Entry point:** `ValiFlow<T>` in `Builder/ValiFlow.cs` — the public fluent builder that users instantiate. It inherits from `BaseExpression<ValiFlow<T>, T>` and composes all type-specific expression classes as private fields (composition, not inheritance).

**Composition structure:**
```
ValiFlow<T> : BaseExpression<ValiFlow<T>, T>
  (composed via private fields):
  ├─ BooleanExpression<ValiFlow<T>, T>
  ├─ ComparisonExpression<ValiFlow<T>, T>
  ├─ NumericExpression<ValiFlow<T>, T>
  ├─ StringExpression<ValiFlow<T>, T>
  ├─ CollectionExpression<ValiFlow<T>, T>
  ├─ DateTimeExpression<ValiFlow<T>, T>
  ├─ DateTimeOffsetExpression<ValiFlow<T>, T>
  ├─ DateOnlyExpression<ValiFlow<T>, T>
  └─ TimeOnlyExpression<ValiFlow<T>, T>
```

**`BaseExpression<TBuilder, T>`** (`Classes/Base/BaseExpression.cs`) is the core — it owns the condition list, manages `And()`/`Or()` operator precedence, and contains `Build()` / `BuildNegated()` which materializes the final expression tree. Operator precedence is handled by grouping OR-separated conditions into sub-expressions. Parameter replacement across sub-expressions uses the internal `ParameterReplacer : ExpressionVisitor`.

**Type-specific classes** in `Classes/Types/` each add domain-specific `Add()` overloads — string length/regex checks, numeric range checks, collection emptiness/count/Any/All, DateTime/DateTimeOffset/DateOnly/TimeOnly comparisons, boolean checks. All methods return `TBuilder` (the concrete type) for fluent chaining.

**Interfaces** in `Interfaces/` mirror the class hierarchy and define the public contract.

**Utilities:**
- `RegularExpressions/RegularExpression.cs` — predefined regex patterns used by `StringExpression`
- `Utils/Validation.cs` — guards that throw if a condition would always be true/false/null
- `Utils/Constant.cs` / `Utils/Util.cs` — shared constants and helpers

## Key Design Decisions

- `AddSubGroup(group => ...)` and `Add().Or().Add()` must produce equivalent expression trees — keep them consistent when modifying `BaseExpression`.
- The `^1` index operator is intentionally avoided (uses `_conditions.Count - 1`) for C# pre-8.0 compatibility.
- `Contains(selector, value, comparison)` uses `string.Contains(string, StringComparison)` — NOT EF Core translatable. The multi-selector `Contains(value, selectors)` uses `ToLower()` + single-arg `Contains` internally (also not EF Core translatable due to `ToLower`).
- The library is explicitly dependency-free — do not add NuGet dependencies.
- `ExpressionHelpers.ParameterReplacer` is the single canonical parameter replacer — do not create local variants. It accepts `Expression` (not just `ParameterExpression`) as the replacement value, which covers the interface-filter cast case in `ValiFlowGlobal`.
- `IExpressionAnnotator<TBuilder>` owns all `With*` methods (WithMessage, WithError, WithSeverity). `IExpressionBuilder<TBuilder,T>` inherits it. Consumers that only need annotation can depend on the narrower interface.

## ValiFlow / ValiFlowQuery Synchrony Rule

`ValiFlow<T>` (full feature set) and `ValiFlowQuery<T>` (EF Core-safe subset) are **manually mirrored**. This is the largest source of maintenance cost in the library. Until a source generator is added, follow this checklist when adding a new validation method:

**Checklist — adding a new method:**
- [ ] 1. Add implementation to the type-specific class in `Classes/Types/` (e.g., `StringExpression<TBuilder,T>`)
- [ ] 2. Add declaration to the corresponding interface in `Interfaces/Types/` (e.g., `IStringExpression`)
- [ ] 3. Add one-line delegation wrapper to `Builder/ValiFlow.cs`
- [ ] 4. Add one-line delegation wrapper to `Builder/ValiFlowQuery.cs` **only if EF Core-translatable**. If the method uses Regex, StringComparison, char-level LINQ, or Enumerable.All/Any with predicates, skip ValiFlowQuery and add `<remarks>Not EF Core translatable</remarks>` to the interface declaration.
- [ ] 5. Run `dotnet test` — 0 failures required before committing.

**Known divergence:** ValiFlowQuery intentionally omits all regex-based string methods, `EqualToIgnoreCase`, `IsOneOf`, `IsTrimmed`, character-class checks, and collection predicate methods (All, Any, None, EachItem, AnyItem, AllMatch, DistinctCount, HasDuplicates).
