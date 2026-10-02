# CLAUDE.md — Vali-Flow.Core

## Stack
- **Runtime/Lenguaje**: .NET — `net8.0;net9.0` multi-target
- **Tipo de proyecto**: Librería NuGet pura (fluent API de expression trees), sin framework de app, sin DB propia
- **Testing**: xUnit + FluentAssertions + coverlet.collector; EF Core InMemory solo para validar traducción LINQ→SQL de `ValiFlowQuery`
- **Extras del repo**: Source Generator (Roslyn) propio + Analyzer (Roslyn) propio + proyecto de Benchmarks (BenchmarkDotNet)

## Comandos clave
```bash
# Build
dotnet build Vali-Flow.Core.sln

# Tests
dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj

# Benchmarks
dotnet run -c Release --project Vali-Flow.Core.Benchmarks

# Pack (NuGet)
dotnet pack Vali-Flow.Core/Vali-Flow.Core.csproj -c Release
```

## Convenciones del proyecto
- **Nombres**: PascalCase para clases/interfaces, camelCase para variables/parámetros, prefijo `I` en interfaces
- **Estructura**: feature-based por tipo de expresión (`Classes/Types`, `Interfaces/Types` — Boolean, String, Numeric, Collection, DateTime, DateTimeOffset, DateOnly, TimeOnly)
- **Commits**: Conventional Commits (`feat/fix/refactor/chore/docs/test`) — ver `CHANGELOG.md` para el historial detallado por versión
- **Branches**: no hay convención documentada aún en el repo

## Arquitectura de alto nivel
Fluent builder (`ValiFlow<T>` / `ValiFlowQuery<T>`) que compone `Expression<Func<T, bool>>` en memoria mediante clases de composición por tipo de dato (`*ExpressionQuery` en `Classes/Types`), cada una implementando su interfaz correspondiente en `Interfaces/Types`. Un source generator (`Vali-Flow.Core.Generator`) elimina ~468 métodos de delegación manuales generando partials. `ValiFlowQuery<T>` es el subconjunto "EF Core-safe" — solo expone métodos traducibles a SQL; un analyzer Roslyn (`VFCORE001`) advierte si se usan métodos no-EF-safe dentro de contextos `IQueryable`.

## Módulos/áreas principales
- `Vali-Flow.Core/Builder` — entry points fluidos (`ValiFlow<T>`, `ValiFlowQuery<T>`)
- `Vali-Flow.Core/Classes/Base` — `BaseExpression` y lógica común (clonado de expression trees, freeze/fork, nested validation)
- `Vali-Flow.Core/Classes/General` — `ValiFlowGlobal` (filtros ambientales estáticos), `ValiSort` (sort builder fluido)
- `Vali-Flow.Core/Classes/Types` — una clase de composición por tipo (String/Numeric/Collection/DateTime/DateTimeOffset/DateOnly/TimeOnly/Boolean)
- `Vali-Flow.Core/Interfaces` — contratos espejo de `Classes` (General + Types)
- `Vali-Flow.Core/Models` — `ValidationError`, `ValidationResult`, `Severity`, etc.
- `Vali-Flow.Core/RegularExpressions` — patrones regex compartidos (email, URL, E.164, Base64, etc.)
- `Vali-Flow.Core/Utils` — helpers (clonado de expression trees, explainer)
- `Vali-Flow.Core.Generator` — source generator que genera los métodos de delegación partial
- `Vali-Flow.Core.Analyzers` — analyzer Roslyn `VFCORE001` (uso de métodos no-EF-safe en `IQueryable`)
- `Vali-Flow.Core.Benchmarks` — BenchmarkDotNet
- `Vali-Flow.Core.Tests` — xUnit, un archivo de test por área (~1 clase de producción ↔ 1 clase de test)

## NO hacer
- [ ] No agregar métodos no-EF-safe (reflection, regex, DateTime.Now, etc.) a `ValiFlowQuery<T>` — rompe el contrato "EF Core-safe"; van en `ValiFlow<T>` (in-memory)
- [ ] No mutar un builder después de `Freeze()`/primer `Build*()` — cualquier mutación debe retornar un fork (clone), nunca lanzar excepción ni mutar in-place
- [ ] No introducir aliasing de nodos de expression tree (reutilizar el mismo `Expression` en dos posiciones del árbol) — ver historial extenso de bugfixes de "node aliasing" en `CHANGELOG.md`
- [ ] No commitear `.env` ni credenciales (no debería haber ninguna en este repo — es librería pura)
- [ ] No remover guards de `ArgumentNullException` en selectors — es el contrato público de toda la API

## Contexto de negocio
Librería .NET sin dependencias (dependency-free) que provee una fluent API para construir árboles de expresión LINQ de validación y filtrado, compatible con EF Core, LINQ-to-Objects y cualquier proveedor `IQueryable`.
