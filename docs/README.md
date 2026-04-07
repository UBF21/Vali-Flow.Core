# Vali-Flow.Core — Documentation

**Vali-Flow.Core** is a dependency-free .NET library for building LINQ predicates (`Expression<Func<T, bool>>`) fluently. It supports in-memory validation with structured error reporting and EF Core-safe query filtering.

---

## Choose your language / Elige tu idioma

| | Language | Index |
|---|---|---|
| English | Full technical documentation in English | [en/README.md](en/README.md) |
| Español | Documentación técnica completa en español | [es/README.md](es/README.md) |

---

## What is in each section

Both language versions contain identical content organized in the same structure:

```
architecture/   ← How the library is designed internally
guides/         ← Step-by-step instructions for concrete tasks
internals/      ← Deep implementation details for contributors
```

### Architecture (7 documents)

| # | Topic |
|---|---|
| 01 | Overview: component map, execution flow, problem statement |
| 02 | `BaseExpression`: CRTP, AND/OR algorithm, Build(), Freeze/Fork |
| 03 | Facade + Composition: how `ValiFlow<T>` unifies 9 domains |
| 04 | LINQ expression trees: anatomy, construction, parameter unification |
| 05 | Roslyn source generator: `[ForwardInterface]`, pipeline, generated code |
| 06 | EF Core safety: `ValiFlowQuery<T>`, VF001 Analyzer, translatable methods |
| 07 | Design patterns catalog: CRTP, Builder, Visitor, Registry, Strategy, and more |

### Guides (3 documents)

| # | Topic |
|---|---|
| 01 | Getting started: installation, first example, method catalog by type |
| 02 | Adding new methods: checklist, expression building, EF Core decision |
| 03 | Validation with errors: `WithMessage`, `WithError`, `Validate()`, lazy factories |

### Internals (4 documents)

| # | Topic |
|---|---|
| 01 | `ConditionEntry<T>`: record, `with` syntax, `Lazy<>` compilation |
| 02 | Thread safety: Freeze/Fork model, `ImmutableList`, `Volatile`, `Interlocked` |
| 03 | Expression visitors: `ParameterReplacer`, `ForceCloneVisitor`, `ExpressionExplainer` |
| 04 | `ValiFlowGlobal` and `ValiSort<T>`: global filters and fluent sorting |

---

## Quick orientation

**To use the library** — start with: [en/guides/01-getting-started.md](en/guides/01-getting-started.md)

**To contribute** — start with: [en/architecture/01-overview.md](en/architecture/01-overview.md) then [en/guides/02-adding-new-methods.md](en/guides/02-adding-new-methods.md)

**For EF Core integration** — go to: [en/architecture/06-ef-core-safety.md](en/architecture/06-ef-core-safety.md)

---

**Para usar la librería** — empezar por: [es/guides/01-getting-started.md](es/guides/01-getting-started.md)

**Para contribuir** — empezar por: [es/architecture/01-overview.md](es/architecture/01-overview.md) luego [es/guides/02-adding-new-methods.md](es/guides/02-adding-new-methods.md)

**Para integración con EF Core** — ir a: [es/architecture/06-ef-core-safety.md](es/architecture/06-ef-core-safety.md)
