# Vali-Flow.Core - Fluent Expression Builder for .NET Validation

[![NuGet](https://img.shields.io/nuget/v/Vali-Flow.Core.svg)](https://www.nuget.org/packages/Vali-Flow.Core)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-purple.svg)](https://dotnet.microsoft.com)

## Introduction 🚀
Welcome to Vali-Flow.Core, the foundational library for the Vali-Flow ecosystem, providing a fluent API to build logical expressions for validation in .NET applications. Designed for seamless integration with LINQ and Entity Framework (EF), Vali-Flow.Core allows developers to construct complex validation conditions in a readable and type-safe manner. It supports a variety of data types and provides methods to build expressions for filtering entities, making it ideal for use in domain logic, repositories, or query pipelines.

## Installation 📦
To add Vali-Flow.Core to your .NET project, install it via NuGet with the following command:

```sh
dotnet add package Vali-Flow.Core
```
Ensure your project targets a compatible .NET version (e.g., .NET 8.0 or 9.0) for optimal performance. Vali-Flow.Core is lightweight and dependency-free, making it easy to integrate into any .NET application. The package builds for `net8.0`/`net9.0` but also runs on **.NET 10** projects thanks to .NET's forward compatibility — a library built for an earlier TFM works unmodified under a newer runtime.

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

## What's New in v2.0.3 🚀

### Fixed
- **`IsLastDayOfMonth()`** on `DateTimeExpressionQuery`, `DateTimeOffsetExpressionQuery`, and `DateOnlyExpressionQuery` built `DateTime.DaysInMonth(...)`, which no EF Core relational provider (SQL Server, PostgreSQL, SQLite, MySQL) can translate to SQL — any call inside `IQueryable` threw `InvalidOperationException`. Replaced with a provider-agnostic `Year`/`Month`/`Day` integer formula, translatable on every relational provider. Verified against real SQLite and PostgreSQL.

### Added
- `EfCoreRelationalTranslationTests`: regression tests that verify `ValiFlowQuery<T>`'s EF Core-safe methods actually translate against a real relational provider (SQLite), not just `UseInMemoryDatabase`.

### v2.0.2 — Fixed
- `ComparisonExpression.Null()`/`NotNull()`: fixed a `WHERE 0=1` bug when used together with EF Core `GlobalQueryFilter`.

### v2.0.1 — Infrastructure
- New companion packages: `Vali-Flow.Core.Analyzers` and `Vali-Flow.Core.Generator`, both requiring `Vali-Flow.Core >= 2.0.0`.

### Breaking Changes since v1.x
| v1.x | v2.0 |
|------|------|
| `result.ErrorsAbove(Severity.Warning)` | `result.ErrorsAtOrAbove(Severity.Warning)` |
| `BeforeDate(selector, date)` | `IsBefore(selector, date)` |
| `AfterDate(selector, date)` | `IsAfter(selector, date)` |
| `CountEquals(selector, n)` | `Count(selector, n)` |
| Implemented `IStringExpression` directly | Implement its 4 sub-interfaces instead: `IStringLengthExpression`, `IStringContentExpression`, `IStringStateExpression`, `IStringFormatExpression` |

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

MIT © 2026 Felipe Rafael Montenegro Morriberon

## Contributions 🤝

Contributions are welcome! Feel free to open issues and submit pull requests to improve this library.

- **Bug reports** — open an issue with a minimal reproducible example
- **Feature requests** — open an issue describing the use case
- **Pull requests** — fork the repo, create a branch, and submit a PR against `main`

See the [GitHub repository](https://github.com/UBF21/Vali-Flow.Core) to get started.
