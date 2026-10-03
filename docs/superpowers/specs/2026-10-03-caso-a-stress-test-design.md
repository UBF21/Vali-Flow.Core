# Caso A Stress Test — Vali-Flow.Core (ValiFlowQuery<T> bajo EF Core real)

## Objetivo

Validar cómo se comporta `ValiFlowQuery<T>` (el subconjunto "EF Core-safe" de Vali-Flow.Core) bajo
carga real de una API con EF Core sobre Postgres — no la construcción del árbol de expresión en
memoria (ya cubierto por Caso C esta sesión), sino su **traducción LINQ→SQL real** bajo tráfico
alto y diverso. Sigue el patrón ya usado en Vali-Mediator (k6 + Toxiproxy + sampler +
reconciliación de corrección contra la fuente de verdad).

Cuatro pilares, en orden de prioridad:

1. **Crecimiento del compiled-query-cache de EF Core**: `ValiFlowQuery<T>` genera un
   `Expression<Func<T,bool>>` distinto por cada combinación de métodos encadenados. Si la
   diversidad de *formas* de árbol (no solo de valores) hace crecer sin límite el cache interno de
   planes compilados de EF Core, es un memory leak real y específico de esta librería — el hallazgo
   más valioso de este experimento.
2. **Corrección bajo concurrencia**: lecturas filtradas concurrentes con escrituras/mutaciones
   concurrentes deben devolver resultados consistentes, reconciliados contra Postgres directamente.
3. **Latencia/throughput bajo carga realista**: p50/p95/p99 del endpoint filtrado a distintos
   niveles de carga, y dónde empieza a degradar.
4. **Cobertura sistemática de métodos**: el pool de plantillas de filtro (ver Sección 2) debe
   ejercitar la superficie completa de `ValiFlowQuery<T>` (todas las familias de tipo: String,
   Numeric, DateTime/DateOnly/TimeOnly, Collection, Boolean/Comparison), no solo un filtro trivial.

## Alcance y restricciones

- **Greenfield, descartable**: no existe ninguna sample API en el repo. Todo el **código del
  harness** (sample API, scripts de k6/sampler/reconcile, compose de Docker) vive en el scratchpad
  de la sesión (`C:\Users\fmontenegro\AppData\Local\Temp\claude\...\scratchpad\stress-case-a\`) y
  **nunca se commitea al repositorio de Vali-Flow.Core**. Este spec y el plan de implementación que
  se deriva de él sí se commitean localmente a la rama de trabajo actual (son documentación de
  planificación, igual que los planes de Round 1/Round 2) — la distinción es código del harness
  vs. documentos de planificación, no "nada se commitea". Nada de esto se pushea a ningún remoto.
- **No se modifica Vali-Flow.Core**: el harness consume el paquete/proyecto tal cual vía
  `ProjectReference` (igual que `Vali-Flow.Core.Tests`), sin tocar su código fuente.
- **Motor de BD**: PostgreSQL en Docker, contenedor descartable y dedicado a este run
  (`docker compose`), nunca una BD compartida de dev/qa. Se usa el proveedor EF Core real
  (`Npgsql.EntityFrameworkCore.PostgreSQL`), no `UseInMemoryDatabase` — InMemory no ejercita
  traducción LINQ→SQL real y por lo tanto no puede revelar el riesgo del pilar 1.
- **Docker confirmado disponible** en esta máquina (`docker --version` → 29.4.3, Docker Desktop
  corriendo con otros contenedores activos).
- **.NET 8** (el mínimo multi-target de Vali-Flow.Core) para la sample API — evita divergencia
  innecesaria entre el harness y el runtime mínimo soportado.

## Arquitectura

```
k6 (generador de carga, ramping-arrival-rate)
  → Sample API (ASP.NET Core Minimal API, .NET 8)
      → MediatR (CQRS: Commands para seed/mutate, Queries para filtrar)
      → FluentValidation (valida el DTO de filtro antes del handler)
      → ValiFlowQuery<StressItem> (construido dinámicamente desde el pool de plantillas)
      → EF Core + Npgsql
      → Toxiproxy → PostgreSQL (contenedor Docker descartable)
  → sampler.py pollea /diag/runtime cada N seg, escribe CSV
  → al terminar: reconcile.py consulta Postgres directamente y valida corrección
```

### Componentes

**Entidad `StressItem`** (una tabla, sin relaciones — el foco es el filtrado, no navegación):

```csharp
public class StressItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int IntValue { get; set; }
    public long LongValue { get; set; }
    public decimal DecimalValue { get; set; }
    public double DoubleValue { get; set; }
    public float FloatValue { get; set; }
    public short ShortValue { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateOnly BirthDate { get; set; }
    public TimeOnly WorkStart { get; set; }
    public List<string> Tags { get; set; } = new();
    public int? OptionalScore { get; set; }
    public int Version { get; set; } // para detectar mutaciones concurrentes en la reconciliación
}
```

Cubre todas las familias de tipo que `ValiFlowQuery<T>` soporta: String, Numeric (int/long/decimal
/double/float/short + nullable), DateTime/DateTimeOffset/DateOnly/TimeOnly, Boolean/Comparison,
Collection (`Tags`, mapeado como columna JSON o tabla relacionada simple vía Npgsql array nativo
`string[]`).

**CQRS (patrón MediatR)**:
- `SeedDataCommand(int rowCount, int seed)` → puebla la tabla con datos deterministas (mismo seed →
  mismos datos, necesario para que `reconcile.py` pueda recomputar el resultado esperado).
- `MutateDataCommand(int seed, int operationCount)` → aplica una mezcla de updates/deletes/inserts
  concurrentes controlados, incrementando `Version` en cada update.
- `FilterItemsQuery(int templateId, int seed)` → resuelve la plantilla `templateId` del pool (ver
  Sección 2), construye el `ValiFlowQuery<StressItem>` correspondiente con valores derivados de
  `seed`, lo compila a `Expression<Func<StressItem,bool>>`, lo traduce a IQueryable de EF Core y
  ejecuta. Devuelve `{ count, idsHash }` (hash de los IDs ordenados, no la lista completa, para
  mantener el payload de respuesta liviano bajo carga alta).

**FluentValidation**: valida el DTO de entrada de `FilterItemsQuery` (`templateId` dentro del rango
del pool, `seed` no negativo) antes de llegar al handler — mantiene el patrón CQRS+FluentValidation
que el usuario pidió explícitamente, aunque el valor principal del experimento está en el handler,
no en la validación de entrada.

**Endpoints**:

| Método | Ruta | CQRS | Propósito |
|---|---|---|---|
| POST | `/items/seed` | `SeedDataCommand` | Poblar dataset determinista al inicio de cada perfil |
| POST | `/items/mutate` | `MutateDataCommand` | Generar contención de escritura concurrente (pilar 2) |
| POST | `/items/filter` | `FilterItemsQuery` | El endpoint bajo prueba — recibe `{templateId, seed}` |
| GET | `/diag/runtime` | — | Métricas de proceso + contador del compiled-query-cache |

**`/diag/runtime`** expone, vía `System.Diagnostics.Metrics` (mismo patrón genérico que
`/testing:stress-test` documenta): `workingSetMb`, `gen0/1/2`, `allocatedMb`, `cpuMs`, `threads`,
`threadPoolQueued`, y el contador específico de este experimento:
**`efCompiledQueryCacheSize`** — el número de entradas distintas en el cache interno de EF Core
(`IMemoryCache` que respalda `CompiledQueryCacheKeyGenerator`). Se obtiene leyendo el
`MemoryCache` interno vía reflection sobre `DbContext.GetService<IMemoryCache>()` (el
`CompiledQueryCache` de EF Core usa un `MemoryCache` inyectado en el `IServiceProvider` interno del
contexto) — si esa ruta de reflection resulta frágil entre versiones de EF Core, el plan de
implementación debe documentar el fallback: contar entradas vía un `DiagnosticListener` suscrito a
los eventos de compilación de EF Core (`Microsoft.EntityFrameworkCore.Query.QueryCompilationStarting`),
incrementando un contador propio por cada shape nuevo observado.

## Pool de plantillas de filtro

~80-100 plantillas fijas, cada una una función `Func<int /*seed*/, ValiFlowQuery<StressItem>>` que
aplica una secuencia **fija** de llamadas a métodos de `ValiFlowQuery<StressItem>`, parametrizada
con valores derivados del `seed` (mismo seed → mismo valor, para que `reconcile.py` pueda
recomputar el resultado esperado sin re-ejecutar la plantilla).

Distribución por familia:

| Familia | # plantillas | Ejemplos de método ejercitado |
|---|---|---|
| String | ~20 | `Contains`, `StartsWith`, `EndsWith`, `*IgnoreCase`, `IsLowerCase/IsUpperCase`, `MinLength/MaxLength/ExactLength`, `RegexMatch` |
| Numeric (todos los tipos) | ~20 | `GreaterThan/LessThan/InRange` para cada uno de int/long/decimal/double/float/short (+ nullable), `IsOdd/IsEven`, `IsMultipleOf` |
| DateTime/DateTimeOffset/DateOnly/TimeOnly | ~20 | `IsInMonth/IsInYear`, `IsToday/IsYesterday/IsTomorrow`, `BetweenDates`, `IsWeekend/IsWeekday`, `IsInQuarter` |
| Collection | ~10 | `EachItem`, `AnyItem`, filtros sobre `Tags` |
| Boolean/Comparison | ~10 | `IsTrue/IsFalse`, `IsNull/IsNotNull`, `EqualTo/NotEqualTo`, `IsInEnum` |
| Combinadas (AND/OR de 2-3 condiciones de familias distintas) | ~15-20 | p.ej. `(String.Contains AND Numeric.InRange) OR DateOnly.IsWeekend` |

Cada plantilla tiene un `templateId` estable (índice en un array ordenado, documentado en el plan
de implementación con el código completo — no hay generación dinámica de plantillas en runtime,
el pool es finito y conocido de antemano). Esto es lo que hace la señal del pilar 1 interpretable:
**se espera que el compiled-query-cache de EF Core se estabilice en ~N entradas** (una por forma de
árbol, una vez que k6 haya muestreado todas las plantillas al menos una vez) — si sigue creciendo
de forma sostenida después de ese punto (visible en el perfil Soak), es la señal de que algo
(probablemente un valor literal no parametrizado colándose en la clave de cache) está generando una
entrada nueva en cada request en lugar de reusar una existente.

## Reconciliación de corrección

Después de cada run, `reconcile.py`:

1. Conecta directamente a Postgres (nunca usa el conteo que reportó k6/el endpoint).
2. Para cada request de `/items/filter` registrada en el log de k6 (templateId + seed +
   resultado recibido), recomputa el resultado esperado aplicando la misma plantilla **en
   LINQ-to-Objects sobre un snapshot de la tabla tomado con un timestamp cercano al de la
   request** — tolera mutaciones concurrentes por rango de timestamp usando la columna `Version`
   (si la fila fue mutada entre el snapshot y la request, se excluye de la comparación estricta y
   se cuenta aparte como "ambiguo", igual que la metodología genérica documenta para 5xx/timeouts).
3. Verifica: cero excepciones no esperadas en cualquier plantilla; el conteo final de filas
   coincide con `seed - deletes + inserts` de `MutateDataCommand`; ninguna plantilla devolvió un
   resultado que no reconcilia fuera de la tolerancia de mutación concurrente.

## Perfiles de carga (catálogo completo — 6 perfiles)

| Perfil | Dataset | Latencia Toxiproxy | Patrón de carga | Qué valida |
|---|---|---|---|---|
| **Baseline** | 10K filas | ~0ms | 10 req/s constante, mezcla de plantillas | Paridad simple, humo |
| **Realista** | 500K-1M filas | 5-15ms (hop de red real) | Rampa a carga moderada-alta, mezcla aleatoria de las ~80-100 plantillas | Comportamiento bajo condiciones parecidas a producción |
| **Escala máxima** | El más grande que entre cómodo en Postgres local | ~0ms | Sube tasa hasta saturar (CPU/conexiones/lo que sature primero) | Dónde está el cuello de botella real |
| **Soak** (30-60min) | Igual que Realista | 5-15ms | Tasa constante moderada, larga duración | **El perfil clave para el pilar 1** — si `efCompiledQueryCacheSize` no se estabiliza, es el hallazgo principal |
| **Saturación escalonada** | Igual que Realista | 5-15ms | Pasos ×1/×2/×3... contra el mismo proceso vivo, parar cuando p99 cruce 2× el primer paso | Punto real de degradación bajo el mismo proceso |
| **Multi-instancia** | Igual que Realista | 5-15ms | 2 instancias de la API compartiendo la misma Postgres | El cache de EF Core es per-proceso — confirma que no hay una condición de carrera a nivel de conexión/transacción compartida entre instancias |

Infra Docker: mismo patrón que `/testing:stress-test` documenta (`compose.perf.yml` con un solo
servicio Postgres + Toxiproxy delante, `tmpfs` para evitar desgaste de disco en corridas repetidas).

## Entregable final

Un reporte (no un artifact HTML, a menos que el usuario lo pida después) con: tabla antes/después
de latencia p50/p95/p99 por perfil, gráfico de `efCompiledQueryCacheSize` en el tiempo durante Soak
(plateau esperado vs. observado), resultado de la reconciliación de corrección (0 discrepancias
esperado), y cualquier hallazgo de bug real descubierto. Si el pilar 1 revela un leak real, se
documenta como hallazgo separado para decidir si amerita un issue/fix en Vali-Flow.Core (fuera del
alcance de este harness descartable).
