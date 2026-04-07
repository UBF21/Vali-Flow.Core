# Vali-Flow.Core — Documentación Técnica (Español)

Documentación completa de **Vali-Flow.Core**: una librería .NET sin dependencias externas para construir predicados LINQ (`Expression<Func<T, bool>>`) de forma fluida, para validación en memoria y filtrado con EF Core.

> Versión en inglés: [../en/README.md](../en/README.md)

---

## Arquitectura

Explica cómo está diseñada la librería desde sus cimientos. Leer en orden para entender el diseño completo.

| Archivo | Contenido |
|---|---|
| [architecture/01-overview.md](architecture/01-overview.md) | Visión general: qué es, qué problema resuelve, mapa de componentes y flujo de ejecución |
| [architecture/02-base-expression.md](architecture/02-base-expression.md) | `BaseExpression<TBuilder,T>` — el motor central: CRTP, estado interno, algoritmo Build() |
| [architecture/03-facade-composition.md](architecture/03-facade-composition.md) | Patrón Facade + Composition: cómo `ValiFlow<T>` unifica 9 dominios |
| [architecture/04-expression-trees.md](architecture/04-expression-trees.md) | Cómo se construyen los árboles de expresión LINQ internamente |
| [architecture/05-source-generator.md](architecture/05-source-generator.md) | El generador de código Roslyn: qué hace, pipeline y atributo `[ForwardInterface]` |
| [architecture/06-ef-core-safety.md](architecture/06-ef-core-safety.md) | `ValiFlowQuery<T>`, el Analyzer VF001 y seguridad EF Core |
| [architecture/07-design-patterns.md](architecture/07-design-patterns.md) | Catálogo completo de todos los patrones de diseño usados |

---

## Guías

Instrucciones paso a paso para tareas concretas.

| Archivo | Contenido |
|---|---|
| [guides/01-getting-started.md](guides/01-getting-started.md) | Instalación, primer uso, catálogo de métodos por tipo, reutilización de reglas |
| [guides/02-adding-new-methods.md](guides/02-adding-new-methods.md) | Checklist completo para agregar un nuevo método de validación a la librería |
| [guides/03-validation-with-errors.md](guides/03-validation-with-errors.md) | `WithMessage`, `WithError`, `WithSeverity`, `Validate()` y mensajes lazy |

---

## Internals

Documenta los detalles de implementación más complejos. Para contribuidores y mantenedores.

| Archivo | Contenido |
|---|---|
| [internals/01-condition-entry.md](internals/01-condition-entry.md) | `ConditionEntry<T>`: por qué es record, compilación diferida, tres campos de mensaje |
| [internals/02-thread-safety.md](internals/02-thread-safety.md) | Thread safety, el ciclo Freeze/Fork, ImmutableList, publicación segura del cache |
| [internals/03-expression-visitors.md](internals/03-expression-visitors.md) | `ParameterReplacer`, `ForceCloneVisitor`, `ExpressionExplainer` — los tres visitors |
| [internals/04-valiflowglobal-valisort.md](internals/04-valiflowglobal-valisort.md) | Filtros globales (`ValiFlowGlobal`) y ordenamiento fluido (`ValiSort<T>`) |

---

## Referencia de métodos

Documentación de todos los métodos disponibles agrupados por tipo de dato.

| Archivo | Contenido |
|---|---|
| [01-primeros-pasos.md](01-primeros-pasos.md) | Inicio rápido, `ValiFlow<T>` vs `ValiFlowQuery<T>`, `When`/`Unless`, `AddSubGroup` |
| [02-comparacion-boolean.md](02-comparacion-boolean.md) | Checks booleanos, nulidad, igualdad, tipos y comparaciones entre campos |
| [03-cadenas.md](03-cadenas.md) | Métodos de validación de strings |
| [04-numerico.md](04-numerico.md) | Métodos numéricos |
| [05-coleccion.md](05-coleccion.md) | Métodos de colecciones |
| [06-fechas.md](06-fechas.md) | Métodos de fecha y hora |
| [07-valiflow-query.md](07-valiflow-query.md) | Referencia de `ValiFlowQuery<T>` |
| [08-avanzado.md](08-avanzado.md) | Uso avanzado |

---

## Rutas de lectura recomendadas

### Soy nuevo en la librería y quiero usarla

1. [Visión general](architecture/01-overview.md)
2. [Getting started](guides/01-getting-started.md)
3. [Validación con errores](guides/03-validation-with-errors.md)

### Voy a contribuir código a la librería

1. [Visión general](architecture/01-overview.md)
2. [BaseExpression](architecture/02-base-expression.md)
3. [Facade + Composition](architecture/03-facade-composition.md)
4. [Cómo agregar métodos](guides/02-adding-new-methods.md)
5. [ConditionEntry](internals/01-condition-entry.md)
6. [Thread safety](internals/02-thread-safety.md)

### Necesito integrar con EF Core

1. [EF Core safety](architecture/06-ef-core-safety.md)
2. [Getting started — sección EF Core](guides/01-getting-started.md)
3. [ValiFlowQuery referencia](07-valiflow-query.md)

### Quiero entender los expression trees

1. [Expression trees](architecture/04-expression-trees.md)
2. [Expression visitors](internals/03-expression-visitors.md)
3. [BaseExpression](architecture/02-base-expression.md)

### Quiero entender los patrones de diseño usados

1. [Catálogo de patrones](architecture/07-design-patterns.md)
2. [Facade + Composition](architecture/03-facade-composition.md)
3. [Thread safety](internals/02-thread-safety.md)

---

## Versiones

Esta documentación cubre las versiones que soportan `net8.0` y `net9.0`. La librería no tiene dependencias externas de NuGet.
