# Vali-Flow.Core.Analyzers

[![NuGet](https://img.shields.io/nuget/v/Vali-Flow.Core.Analyzers.svg)](https://www.nuget.org/packages/Vali-Flow.Core.Analyzers)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Roslyn analyzer for **Vali-Flow.Core**: warns when EF Core-incompatible `ValiFlowQuery<T>` methods are used in an `IQueryable` context.

## Installation

```bash
dotnet add package Vali-Flow.Core.Analyzers
```

## Usage

This analyzer automatically checks your code when you install it. It emits diagnostic **VF001** when you call non-translatable methods on `ValiFlowQuery<T>` inside LINQ queries.

### Example

```csharp
using Vali_Flow.Core.Builder;

// ❌ Warning VF001: RegexMatch is not EF Core translatable
var query = dbContext.Products
    .Where(p => new ValiFlowQuery<Product>()
        .Add(p => p.Name, name => name.RegexMatch("^[A-Z]"))
        .Build());

// ✅ OK: IsNotNullOrEmpty is EF Core translatable
var query = dbContext.Products
    .Where(p => new ValiFlowQuery<Product>()
        .Add(p => p.Name, name => name.IsNotNullOrEmpty())
        .Build());
```

## Excluded Methods

The following methods are **not EF Core translatable** and will trigger warning VF001 when used on `ValiFlowQuery<T>` in an IQueryable context:

- Regex-based: `RegexMatch`, `IsEmail`, `IsUrl`, `IsPhoneNumber`, `IsGuid`, `IsJson`, `IsBase64`
- In-memory only: `IsEven`, `IsOdd`, `IsMultipleOf`, `IsToday`, `IsYesterday`, `IsTomorrow`, `InLastDays`, `InNextDays`, `IsLeapYear`, `IsLastDayOfMonth`
- Collection predicates: `All`, `Any`, `None`, `AnyItem`, `EachItem`, `HasDuplicates`, `DistinctCount`
- Other: `EqualToIgnoreCase`, `IsOneOf`, `IsTrimmed`, character-class checks

Use `ValiFlow<T>` instead for these methods.

## License

MIT © 2026 Felipe Rafael Montenegro Morriberon

## Contributions

Contributions are welcome! Feel free to open issues and submit pull requests to improve this analyzer.

See the [GitHub repository](https://github.com/UBF21/Vali-Flow.Core) to get started.
