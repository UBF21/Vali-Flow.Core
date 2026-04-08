# <img src="https://github.com/UBF21/Vali-Flow.Core/blob/main/Vali-Flow.Core/logo_vali_flow_core_.png?raw=true" alt="Logo de Vali Mediator" style="width: 46px; height: 46px; max-width: 300px;">  Vali-Flow.Core - Fluent Expression Builder for .NET Validation


## Introduction 🚀
Welcome to Vali-Flow.Core, the foundational library for the Vali-Flow ecosystem, providing a fluent API to build logical expressions for validation in .NET applications. Designed for seamless integration with LINQ and Entity Framework (EF), Vali-Flow.Core allows developers to construct complex validation conditions in a readable and type-safe manner. It supports a variety of data types and provides methods to build expressions for filtering entities, making it ideal for use in domain logic, repositories, or query pipelines.

## Installation 📦
To add Vali-Flow.Core to your .NET project, install it via NuGet with the following command:

```sh
dotnet add package Vali-Flow.Core
```
Ensure your project targets a compatible .NET version (e.g., .NET 8.0 or 9.0) for optimal performance. Vali-Flow.Core is lightweight and dependency-free, making it easy to integrate into any .NET application.

## Usage 🛠️

Vali-Flow.Core focuses on building expressions that can be used for validation or filtering. The library provides a fluent API through the **ValiFlow<T>** builder, which implements the **IExpression<TBuilder, T>** interface. You can construct conditions, combine them with logical operators (**And**, **Or**), and finalize the builder by generating an expression using **Build()** or **BuildNegated()**.

### Basic Example

Here’s how you can build a simple expression to filter Product entities:

```csharp
using System.Linq.Expressions;
using Vali_Flow.Core.Builder;

var validator = new ValiFlow<Product>()
    .Add(p => p.Name != null)
    .And()
    .Add(p => p.Price, price => price > 0);

Expression<Func<Product, bool>> filter = validator.Build();
```
This expression can be used in a LINQ query or with Entity Framework to filter products where the name is not null and the price is greater than 0.

## Key Methods 📝

Vali-Flow.Core provides methods to construct and finalize logical expressions. Below are the key methods for terminating the builder and generating expressions:

### Build 🏗️

Generates a boolean expression (**Expression<Func<T, bool>>**) from the conditions defined in the builder. This expression can be used in LINQ queries or Entity Framework to filter entities.

```csharp
var validator = new ValiFlow<Product>()
    .Add(p => p.Name != null)
    .And()
    .Add(p => p.Price, price => price > 0);

Expression<Func<Product, bool>> filter = validator.Build();

// Use the expression in a query
var validProducts = dbContext.Products.Where(filter).ToList();
```

### BuildNegated 🔄

Generates a negated version of the expression produced by **Build()**. This is useful when you need to find entities that do not satisfy the defined conditions.

```csharp
var validator = new ValiFlow<Product>()
    .Add(p => p.Name != null)
    .And()
    .Add(p => p.Price, price => price > 0);

Expression<Func<Product, bool>> negatedFilter = validator.BuildNegated();

// Use the negated expression in a query
var invalidProducts = dbContext.Products.Where(negatedFilter).ToList();
```

## Building Complex Conditions 🧩

Vali-Flow.Core allows you to create complex expressions using logical operators (**And**, **Or**) and sub-groups (**AddSubGroup**). Here are some examples:

### Using **And** and **Or**

Combine conditions with logical operators to create sophisticated filters:

```csharp
var validator = new ValiFlow<Product>()
    .Add(p => p.Price, price => price > 0)
    .And()
    .Add(p => p.Name, name => name.StartsWith("A"))
    .Or()
    .Add(p => p.CreatedAt, date => date.Year == 2023);

Expression<Func<Product, bool>> filter = validator.Build();
```
This expression filters products where the price is greater than 0 AND the name starts with "A", OR the creation year is 2023.

### Using **AddSubGroup**

Group conditions to create nested expressions:

```csharp
var validator = new ValiFlow<Product>()
    .AddSubGroup(group => group
        .Add(p => p.Price, price => price > 0)
        .And()
        .Add(p => p.Name, name => name.Length > 3))
    .Or()
    .Add(p => p.IsActive, isActive => isActive == true);

Expression<Func<Product, bool>> filter = validator.Build();
```

This expression filters products where (price > 0 AND name length > 3) OR the product is active.

## Comparison: Without vs. With Vali-Flow.Core ⚖️

### Without Vali-Flow.Core (Manual Expression Building)

Manually building expressions can be cumbersome and error-prone:

```csharp
Expression<Func<Product, bool>> filter = p =>
    p.Name != null &&
    p.Price > 0 &&
    p.CreatedAt.Date == DateTime.Today;
```

### With Vali-Flow.Core (Fluent Expression Building)

Vali-Flow.Core simplifies the process with a fluent and readable API:

```csharp
var validator = new ValiFlow<Product>()
    .Add(p => p.Name != null)
    .And()
    .Add(p => p.Price, price => price > 0)
    .And()
    .Add(p => p.CreatedAt, date => date.Date == DateTime.Today);

Expression<Func<Product, bool>> filter = validator.Build();
```
## Features and Enhancements 🌟

## What's New in v2.0.0 🚀

### Breaking Changes
- `ValidationResult.ErrorsAbove()` → **`ErrorsAtOrAbove()`** — semantics use `>=` (at or above the given severity), not `>`. Update all call sites.
- `ValidationResult.ErrorsAtOrAbove()` returns an `IReadOnlyList<ValidationError>` backed by `AsReadOnly()` — casting to `List<T>` will fail at runtime.

### Performance
- `Validate()` short-circuits OR groups — returns `Ok()` as soon as any group passes, without evaluating remaining groups.
- `ValidationResult.Warnings` and `CriticalErrors` are now lazy — computed only when accessed, not on construction.
- Regex cache in `RegexMatch` is now global (shared across all types) instead of per-closed-generic-type.
- `BuildWithGlobal()` caches `Build()` — multiple calls on a frozen builder reuse the same expression tree.

### API Improvements
- `CreateNestedBuilder<TProperty>()` is now `virtual` — external subclasses of `BaseExpression` no longer required to override it.
- `Severity.Info` behavior clarified: only appears in `ValidationResult` when the condition fails AND has an attached message.
- `WithMessage(Func<string>)` factory must not return `null` — documented in IntelliSense.
- All regex methods (`IsEmail`, `IsUrl`, `RegexMatch`, etc.) now document `RegexMatchTimeoutException` in their XML signatures.

### Migration Guide
| v1.x | v2.0.0 |
|------|--------|
| `result.ErrorsAbove(Severity.Warning)` | `result.ErrorsAtOrAbove(Severity.Warning)` |

## What's New in v2.0.0

### Breaking Changes
- **Removed `BeforeDate` / `AfterDate`** — use `IsBefore` / `IsAfter` (full DateTime/DateTimeOffset comparison including time-of-day).
- **Removed `CountEquals`** — use `Count` (identical semantics).
- **`IStringExpression<TBuilder,T>` segregated into 4 focused sub-interfaces**:
  - `IStringLengthExpression` — MinLength, MaxLength, ExactLength, LengthBetween
  - `IStringContentExpression` — StartsWith, EndsWith, Contains, EqualToIgnoreCase, IsOneOf
  - `IStringStateExpression` — IsNullOrEmpty, IsNullOrWhiteSpace, IsTrimmed, IsLowerCase, IsUpperCase, HasOnlyDigits/Letters/SpecialCharacters
  - `IStringFormatExpression` — IsEmail, IsUrl, IsGuid, IsJson, IsBase64, RegexMatch, MatchesWildcard, IsCreditCard, IsIPv4/IPv6, IsHexColor, IsSlug
  - If you implemented `IStringExpression` directly, implement the 4 sub-interfaces instead.

### Performance
- `ConditionEntry<T>` now compiles predicates lazily via `Lazy<Func<T,bool>>` — thread-safe without explicit locking. `Validate()` no longer acquires a per-condition lock.

### Migration Guide

| v1.x | v2.0 |
|------|------|
| `BeforeDate(selector, date)` | `IsBefore(selector, date)` |
| `AfterDate(selector, date)` | `IsAfter(selector, date)` |
| `CountEquals(selector, n)` | `Count(selector, n)` |

See [CHANGELOG.md](CHANGELOG.md) for the full version history.

## Performance Tips ⚡

Choose the right method for your use case:

| Method | Best for | Compilation |
|--------|----------|-------------|
| `IsValid(item)` | Ad-hoc single checks | Compiles full predicate on first call, caches it |
| `BuildCached()` | Batch filtering of large collections | Compiles once — reuse the returned `Func<T, bool>` |
| `Validate(item)` | Error collection (field-level details) | Each condition compiled independently (lazy per entry) |
| `ValidateAll(items)` | Per-item error details on a list | Calls `Validate()` per item — prefer when you need error context |

**For filtering large collections, always prefer `BuildCached()` over `Validate()`:**

```csharp
// ✅ Fast — expression compiled once, cached delegate reused
var isValid = validator.BuildCached();
var validItems = records.Where(isValid).ToList();

// ⚠️ Slower — Validate() compiles lazily per condition on each call
var validItems = records.Where(r => validator.Validate(r).IsValid).ToList();
```

**When you need structured error output for a batch:**

```csharp
// Use ValidateAll() — freezes the builder once, iterates Validate() per item
var results = validator.ValidateAll(records)
    .Where(r => !r.Result.IsValid)
    .ToList();
```

## Donations 💖
If you find **Vali-Flow.Core** useful and would like to support its development, consider making a donation:

- **For Latin America**: [Donate via MercadoPago](https://link.mercadopago.com.pe/felipermm)
- **For International Donations**: [Donate via PayPal](https://paypal.me/felipeRMM?country.x=PE&locale.x=es_XC)


Your contributions help keep this project alive and improve its development! 🚀

## Changelog 📋
See [CHANGELOG.md](CHANGELOG.md) for a detailed history of all changes.

## License 📜
This project is licensed under the [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0).

## Contributions 🤝
Feel free to open issues and submit pull requests to improve this library!
