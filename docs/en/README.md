# Vali-Flow.Core — Technical Documentation (English)

Complete documentation for **Vali-Flow.Core**: a dependency-free .NET library for building LINQ predicates (`Expression<Func<T, bool>>`) fluently, for in-memory validation and EF Core filtering.

> Spanish version: [../es/README.md](../es/README.md)

---

## Architecture

Explains how the library is designed from the ground up. Read in order to understand the complete design.

| File | Contents |
|---|---|
| [architecture/01-overview.md](architecture/01-overview.md) | Overview: what it is, what problem it solves, component map, and execution flow |
| [architecture/02-base-expression.md](architecture/02-base-expression.md) | `BaseExpression<TBuilder,T>` — the central engine: CRTP, internal state, Build() algorithm |
| [architecture/03-facade-composition.md](architecture/03-facade-composition.md) | Facade + Composition pattern: how `ValiFlow<T>` unifies 9 domains |
| [architecture/04-expression-trees.md](architecture/04-expression-trees.md) | How LINQ expression trees are built internally |
| [architecture/05-source-generator.md](architecture/05-source-generator.md) | The Roslyn source generator: what it does, pipeline, and `[ForwardInterface]` attribute |
| [architecture/06-ef-core-safety.md](architecture/06-ef-core-safety.md) | `ValiFlowQuery<T>`, the VF001 Analyzer, and EF Core safety |
| [architecture/07-design-patterns.md](architecture/07-design-patterns.md) | Complete catalog of all design patterns used |

---

## Guides

Step-by-step instructions for concrete tasks.

| File | Contents |
|---|---|
| [guides/01-getting-started.md](guides/01-getting-started.md) | Installation, first use, method catalog by type, rule reuse |
| [guides/02-adding-new-methods.md](guides/02-adding-new-methods.md) | Complete checklist for adding a new validation method to the library |
| [guides/03-validation-with-errors.md](guides/03-validation-with-errors.md) | `WithMessage`, `WithError`, `WithSeverity`, `Validate()`, and lazy messages |

---

## Internals

Documents the most complex implementation details. For contributors and maintainers.

| File | Contents |
|---|---|
| [internals/01-condition-entry.md](internals/01-condition-entry.md) | `ConditionEntry<T>`: why it is a record, deferred compilation, three message fields |
| [internals/02-thread-safety.md](internals/02-thread-safety.md) | Thread safety, the Freeze/Fork cycle, ImmutableList, safe cache publication |
| [internals/03-expression-visitors.md](internals/03-expression-visitors.md) | `ParameterReplacer`, `ForceCloneVisitor`, `ExpressionExplainer` — the three visitors |
| [internals/04-valiflowglobal-valisort.md](internals/04-valiflowglobal-valisort.md) | Global filters (`ValiFlowGlobal`) and fluent sorting (`ValiSort<T>`) |

---

## Method Reference

Documentation for all available methods grouped by data type.

| File | Contents |
|---|---|
| [01-getting-started.md](01-getting-started.md) | Quick start, `ValiFlow<T>` vs `ValiFlowQuery<T>`, `When`/`Unless`, `AddSubGroup` |
| [02-comparison-boolean.md](02-comparison-boolean.md) | Boolean checks, nullability, equality, type checks, and field comparisons |
| [03-string.md](03-string.md) | String validation methods |
| [04-numeric.md](04-numeric.md) | Numeric methods |
| [05-collection.md](05-collection.md) | Collection methods |
| [06-datetime.md](06-datetime.md) | Date and time methods |
| [07-valiflow-query.md](07-valiflow-query.md) | `ValiFlowQuery<T>` reference |
| [08-advanced.md](08-advanced.md) | Advanced usage |

---

## Recommended Reading Paths

### I am new to the library and want to use it

1. [Overview](architecture/01-overview.md)
2. [Getting started](guides/01-getting-started.md)
3. [Validation with errors](guides/03-validation-with-errors.md)

### I am going to contribute code to the library

1. [Overview](architecture/01-overview.md)
2. [BaseExpression](architecture/02-base-expression.md)
3. [Facade + Composition](architecture/03-facade-composition.md)
4. [How to add methods](guides/02-adding-new-methods.md)
5. [ConditionEntry](internals/01-condition-entry.md)
6. [Thread safety](internals/02-thread-safety.md)

### I need to integrate with EF Core

1. [EF Core safety](architecture/06-ef-core-safety.md)
2. [Getting started — EF Core section](guides/01-getting-started.md)
3. [ValiFlowQuery reference](07-valiflow-query.md)

### I want to understand expression trees

1. [Expression trees](architecture/04-expression-trees.md)
2. [Expression visitors](internals/03-expression-visitors.md)
3. [BaseExpression](architecture/02-base-expression.md)

### I want to understand the design patterns used

1. [Design patterns catalog](architecture/07-design-patterns.md)
2. [Facade + Composition](architecture/03-facade-composition.md)
3. [Thread safety](internals/02-thread-safety.md)

---

## Versions

This documentation covers versions targeting `net8.0` and `net9.0`. The library has no external NuGet dependencies.
