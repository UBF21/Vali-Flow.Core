# Internals — ConditionEntry: el modelo de una condición

## Qué es ConditionEntry

`ConditionEntry<T>` es el objeto que representa una sola condición dentro de un builder. Cada vez que el usuario llama un método de validación (`.NotNull(...)`, `.GreaterThan(...)`, `.IsEmail(...)`), se crea un `ConditionEntry<T>` y se agrega a la lista interna del builder.

Es el "átomo" de la librería: la unidad mínima de información que el motor necesita para construir, evaluar y reportar.

---

## La estructura completa

```csharp
internal sealed record ConditionEntry<T>
{
    // El árbol de expresión de esta condición específica
    public Expression<Func<T, bool>> Condition { get; init; }

    // ¿Se combina con la condición anterior con AND (true) u OR (false)?
    public bool IsAnd { get; init; }

    // Código de error machine-readable (ej: "USER_EMAIL_REQUIRED")
    public string? ErrorCode { get; init; }

    // Mensaje de error estático (ej: "El email es obligatorio")
    public string? Message { get; init; }

    // Factory de mensaje lazy (para localización dinámica)
    public Func<string>? MessageFactory { get; init; }

    // Ruta de la propiedad afectada (ej: "Address.City")
    public string? PropertyPath { get; init; }

    // Severidad del error si esta condición falla
    public Severity Severity { get; init; }

    // Delegate compilado, lazy y thread-safe
    public Lazy<Func<T, bool>> CompiledFunc { get; init; }
}
```

---

## Por qué es un record

`ConditionEntry<T>` es un `record` por dos razones técnicas concretas:

### 1. La sintaxis `with`

Los métodos `WithMessage`, `WithError`, `WithSeverity` y `WithPropertyPath` necesitan modificar la última condición de la lista. Si `ConditionEntry` fuera una `class` mutable, se modificaría directamente el objeto. Si fuera una `class` inmutable, habría que construir un nuevo objeto manualmente con 7 parámetros.

Con `record`, la sintaxis `with` permite crear una copia con solo el campo modificado:

```csharp
// Implementación interna de WithMessage:
private TBuilder MutateLastCondition(Func<ConditionEntry<T>, ConditionEntry<T>> mutate)
{
    // Tomar la última condición, aplicar la mutación, reemplazarla en la lista
    var index = _conditions.Count - 1;
    var updated = mutate(_conditions[index]);
    _conditions = _conditions.SetItem(index, updated);
    return (TBuilder)this;
}

// WithMessage usa MutateLastCondition:
public TBuilder WithMessage(string message)
    => MutateLastCondition(entry => entry with { Message = message });
//                                          ^^^^^^^^^^^^^^^^^^^
//                                          La sintaxis `with` crea una copia nueva
//                                          con solo Message modificado
```

Sin `record`, esta operación requeriría un constructor con 7 parámetros o un builder propio para `ConditionEntry`.

### 2. La inmutabilidad con `init`

Los setters `init` garantizan que los campos no pueden cambiar después de la construcción. Esto es crucial para la thread safety: si múltiples threads leen el mismo `ConditionEntry` concurrentemente, no hay riesgo de data race porque el objeto nunca cambia.

---

## El campo CompiledFunc y la compilación diferida

```csharp
public Lazy<Func<T, bool>> CompiledFunc { get; init; }

// Inicializado en el constructor:
CompiledFunc = new Lazy<Func<T, bool>>(() => condition.Compile());
```

`Lazy<T>` retrasa la compilación del delegate hasta que se necesite por primera vez. Esto es importante porque:

1. **`Build()` no necesita delegates**: `Build()` trabaja con los árboles de expresión directamente. Los delegates solo se necesitan en `Validate()`.

2. **La compilación es costosa**: `Expression.Compile()` puede tomar ~1ms. Si un objeto tiene 10 condiciones pero solo falla la primera, las 9 restantes nunca se compilarán.

3. **Thread safety sin locks**: `Lazy<T>` con el modo predeterminado (`LazyThreadSafetyMode.ExecutionAndPublication`) garantiza que aunque múltiples threads llamen `Validate()` simultáneamente en el mismo builder, la compilación de cada condición ocurre exactamente una vez.

```csharp
// Thread A y Thread B llaman Validate() al mismo tiempo
// Ambos intentan acceder a CompiledFunc.Value

// Sin Lazy: posible double-compilation (benign pero ineficiente)
// Con Lazy(ExecutionAndPublication): un solo thread compila, el otro espera y luego usa el resultado
```

---

## Los tres campos de mensaje

`ConditionEntry` tiene tres campos relacionados con el mensaje de error. Solo uno se usa por condición:

### Message (string estático)

```csharp
.NotNull(u => u.Email).WithMessage("El email es obligatorio")
// Almacena: Message = "El email es obligatorio"
// Se usa directamente cuando se construye ValidationError
```

### ErrorCode + Message (de WithError)

```csharp
.NotNull(u => u.Email).WithError("EMAIL_REQUIRED", "El email es obligatorio")
// Almacena: ErrorCode = "EMAIL_REQUIRED", Message = "El email es obligatorio"
```

### MessageFactory (lazy, para localización)

```csharp
.NotNull(u => u.Email).WithMessageFactory(() => _localizationService.Get("email.required"))
// Almacena: MessageFactory = () => _localizationService.Get("email.required")
// La función se ejecuta solo cuando se construye el ValidationError
```

Cuando `Validate()` construye el `ValidationError`, la lógica de resolución del mensaje es:

```csharp
// Pseudocódigo de cómo se resuelve el mensaje:
string? resolvedMessage = entry.Message
    ?? entry.MessageFactory?.Invoke();
// Si hay Message estático, se usa. Si no, se invoca la factory.
// Si ninguno está definido, el mensaje del error es null.
```

---

## El factory method Create

Para el caso común donde solo se necesita la condición y el operador (sin metadatos de error), existe un factory method que evita pasar cuatro nulos:

```csharp
// Sin el factory method:
_conditions = _conditions.Add(new ConditionEntry<T>(
    condition: expr,
    isAnd: true,
    errorCode: null,
    message: null,
    messageFactory: null,
    propertyPath: null,
    severity: Severity.Error
));

// Con el factory method:
_conditions = _conditions.Add(ConditionEntry<T>.Create(expr, isAnd: true));
```

Esto es lo que llama `BaseExpression.Add(Expression<Func<T,bool>>)` internamente. Solo cuando se llama `.WithMessage()`, `.WithError()`, etc., la condición se muta con `with` para agregar los metadatos.

---

## Relación con ValidationError

Cuando `Validate()` detecta que una condición falla, construye un `ValidationError` a partir del `ConditionEntry`:

```csharp
public sealed record ValidationError
{
    public string? ErrorCode { get; init; }
    public string? Message { get; init; }
    public string? PropertyPath { get; init; }
    public Severity Severity { get; init; }
}
```

```csharp
// Pseudocódigo de la construcción del error:
var error = new ValidationError
{
    ErrorCode    = entry.ErrorCode,
    Message      = entry.Message ?? entry.MessageFactory?.Invoke(),
    PropertyPath = entry.PropertyPath,
    Severity     = entry.Severity
};
```

---

## Visibilidad interna

`ConditionEntry<T>` es `internal` — no forma parte de la API pública de la librería. Los consumidores interactúan con los metadatos de las condiciones a través de:
- Los métodos `WithMessage`, `WithError`, `WithSeverity`, `WithPropertyPath` del builder
- El `ValidationResult` y `ValidationError` que retorna `Validate()`

Esta decisión de diseño mantiene la libertad de cambiar la estructura interna de `ConditionEntry` sin romper la API pública.
