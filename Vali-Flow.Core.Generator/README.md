# Vali-Flow.Core.Generator

[![NuGet](https://img.shields.io/nuget/v/Vali-Flow.Core.Generator.svg)](https://www.nuget.org/packages/Vali-Flow.Core.Generator)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Roslyn source generator for **Vali-Flow.Core**: eliminates manually-maintained delegation methods in `ValiFlow<T>` and `ValiFlowQuery<T>` by generating forwarding methods from `[ForwardInterface]`-annotated fields.

## Installation

```bash
dotnet add package Vali-Flow.Core.Generator
```

## How It Works

This source generator automatically runs at compile time and:

1. Scans for fields annotated with `[ForwardInterface]`
2. Generates delegation methods for all members of the specified interface
3. Writes generated code to the compilation unit

This eliminates ~468 manually-maintained delegation methods that would otherwise need to be kept in sync across `ValiFlow<T>` and `ValiFlowQuery<T>`.

### Example

```csharp
using Vali_Flow.Core.Attributes;

public class ValiFlow<T> : BaseExpression<ValiFlow<T>, T>
{
    [ForwardInterface(typeof(IStringExpression<,>))]
    private StringExpression<ValiFlow<T>, T> _stringExpression;

    // Generator automatically creates:
    // public ValiFlow<T> MinLength(Expression<Func<T, string?>> selector, int length)
    //     => _stringExpression.MinLength(selector, length);
    // public ValiFlow<T> MaxLength(Expression<Func<T, string?>> selector, int length)
    //     => _stringExpression.MaxLength(selector, length);
    // ... (and ~460+ more methods)
}
```

## Benefits

- **Zero boilerplate**: No manual delegation methods to write or maintain
- **Type-safe**: All forwarded methods are verified at compile time
- **Transparent**: Generated code is invisible to callers; the API remains clean
- **Sync automatically**: Whenever an interface changes, all delegations update automatically

## License

MIT © 2026 Felipe Rafael Montenegro Morriberon

## Contributions

Contributions are welcome! Feel free to open issues and submit pull requests to improve this generator.

See the [GitHub repository](https://github.com/UBF21/Vali-Flow.Core) to get started.
