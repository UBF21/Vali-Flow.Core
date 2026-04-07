# Changelog

All notable changes to Vali-Flow.Core are documented here.
Format: [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

## [2.0.0] - 2026-04-05

### Architecture

- **Source generator** (`Vali-Flow.Core.Generator`): Roslyn `IIncrementalGenerator` that reads `[ForwardInterface]`-marked fields in `partial` classes and emits a sibling `.g.cs` file with one forwarding method per interface member. Traverses the full interface inheritance hierarchy and generates explicit interface implementations when two interfaces declare conflicting signatures (e.g. `EqualTo<TValue>` on both `IComparisonExpression` and `IComparableExpression` with different type constraints). Eliminates ~468 manually-maintained delegation methods from `ValiFlow<T>` and `ValiFlowQuery<T>`.
- **`ValiFlow<T>` and `ValiFlowQuery<T>`** reduced from ~893 / ~808 lines to ~110 lines each — now thin `partial` facades; all method bodies live in their respective `*Expression` / `*ExpressionQuery` classes.
- **9 query-specific interfaces** added (`IBooleanExpressionQuery`, `IStringExpressionQuery`, `ICollectionExpressionQuery`, `INumericExpressionQuery`, `IDateTimeExpressionQuery`, `IDateTimeOffsetExpressionQuery`, `IDateOnlyExpressionQuery`, `ITimeOnlyExpressionQuery`) — complete the interface hierarchy for the EF Core-safe variant and are required for the generator to process `ValiFlowQuery<T>`.
- **`BaseExpression.ValidateNested` DIP fix**: extracted `protected virtual CreateNestedBuilder<TProperty>()` factory method — decouples `BaseExpression` from the concrete `ValiFlow<T>` type so `ValiFlowQuery<T>` (and any future subclass) can override which builder is instantiated for nested validation.

### Infrastructure

- **`Vali-Flow.Core.Analyzers`**: Roslyn diagnostic analyzer (`ValiFlowNonEfMethodAnalyzer`) that reports `VFCORE001` when a method not supported by EF Core providers is called on a `ValiFlowQuery<T>` instance inside an `IQueryable<T>` expression context.
- **Bilingual technical documentation** added under `docs/` (Spanish + English): architecture overview, builder lifecycle, expression tree internals, EF Core compatibility guide, source generator guide, and full API reference for all expression types.

### Breaking Changes

- **Removed deprecated methods**: `BeforeDate`, `AfterDate` (use `IsBefore`/`IsAfter`), `CountEquals` (use `Count`)
- **`IStringExpression<TBuilder,T>`** now inherits from 4 focused sub-interfaces:
  - `IStringLengthExpression` — MinLength, MaxLength, ExactLength, LengthBetween
  - `IStringContentExpression` — StartsWith, EndsWith, Contains, EqualToIgnoreCase, IsOneOf
  - `IStringStateExpression` — IsNullOrEmpty, IsNullOrWhiteSpace, IsTrimmed, IsLowerCase, IsUpperCase, HasOnlyDigits, HasOnlyLetters, HasLettersAndNumbers, HasSpecialCharacters
  - `IStringFormatExpression` — IsEmail, IsUrl, IsGuid, IsJson, IsBase64, RegexMatch, MatchesWildcard, IsCreditCard, IsIPv4, IsIPv6, IsHexColor, IsSlug
  - If you implemented `IStringExpression` directly, implement the 4 sub-interfaces instead.

### Performance

- `ConditionEntry<T>` now compiles predicates lazily via `Lazy<Func<T,bool>>` — thread-safe without explicit locking. `Validate()` no longer acquires a lock per condition.

### Migration Guide

| v1.x | v2.0 |
|------|------|
| `BeforeDate(selector, date)` | `IsBefore(selector, date)` |
| `AfterDate(selector, date)` | `IsAfter(selector, date)` |
| `CountEquals(selector, n)` | `Count(selector, n)` |

## [1.7.0] - 2026-04-04

### Architecture

- **ValiFlowQuery<T> God Class eliminated**: reduced from 2 329 lines to ~530 lines by extracting all method bodies into 9 domain-specific composition classes (`BooleanExpressionQuery`, `ComparisonExpressionQuery`, `StringExpressionQuery`, `CollectionExpressionQuery`, `NumericExpressionQuery`, `DateTimeExpressionQuery`, `DateTimeOffsetExpressionQuery`, `DateOnlyExpressionQuery`, `TimeOnlyExpressionQuery`). `ValiFlowQuery<T>` is now a thin wrapper identical in structure to `ValiFlow<T>`. Public API unchanged.
- Deleted `Constant.cs` and `Util.cs` (dead code — all literals inlined at usage site, `GetCurrentMethodName` had zero consumers).
- Consolidated `ExpressionDeepCloner` into `ForceCloneVisitor` (was a structural duplicate); all cross-property expression methods now use the single canonical visitor.

### New Features — ValiFlowQuery<T> (EF Core-safe)

**String**
- `IsTrimmed` — `val == val.Trim()` → SQL `LTRIM(RTRIM(x))` / `trim(x)`
- `IsLowerCase` — `val == val.ToLower()` → SQL `LOWER(x)`
- `IsUpperCase` — `val == val.ToUpper()` → SQL `UPPER(x)`
- `EqualToIgnoreCase` — case-insensitive equality via `ToLower()` on both sides
- `StartsWithIgnoreCase` — `LOWER(x) LIKE 'value%'`
- `EndsWithIgnoreCase` — `LOWER(x) LIKE '%value'`
- `ContainsIgnoreCase` — `LOWER(x) LIKE '%value%'`
- `NotContains` — `!val.Contains(value)` → translatable negation
- `NotStartsWith` — `!val.StartsWith(value)`
- `NotEndsWith` — `!val.EndsWith(value)`

**Numeric (int and long)**
- `IsEven` — `val % 2 == 0` → EF Core translatable via modulo
- `IsOdd` — `val % 2 != 0`
- `IsMultipleOf(n)` — `val % n == 0`

**DateTime**
- `IsWeekend` / `IsWeekday` / `IsDayOfWeek` — via `.DayOfWeek` (SQL Server, PostgreSQL, MySQL Pomelo 5.0+; **not SQLite**)
- `IsToday` / `IsYesterday` / `IsTomorrow` — UTC midnight boundaries captured at build time (EF-safe constants)
- `InLastDays(n)` / `InNextDays(n)` — sliding UTC windows captured at build time
- `IsFirstDayOfMonth` / `IsLastDayOfMonth` — via `.Day` property
- `IsInQuarter(q)` — month-range comparison

**DateTimeOffset**
- `IsWeekend` / `IsWeekday` / `IsDayOfWeek` — same as DateTime variants
- `IsFirstDayOfMonth` / `IsLastDayOfMonth` / `IsInQuarter(q)`

**DateOnly**
- `IsWeekend` / `IsWeekday` / `IsDayOfWeek`
- `IsFirstDayOfMonth` / `IsLastDayOfMonth` / `IsInQuarter(q)`

### Bug Fixes
- `ComparisonExpression.EqualTo` / `NotEqualTo`: removed incorrect null guard — `Expression.Constant(value, typeof(TValue))` handles `null` correctly; fixes nullable value type comparisons.
- `StringExpression`: replaced private `ReplaceParamVisitor` with the canonical `ParameterReplacer` from `ExpressionHelpers`; deleted the duplicate private class.

### Test Coverage
- xUnit test project added with **961 tests** (net9.0)
- Full coverage: `ValiFlow<T>`, `ValiFlowQuery<T>`, `BaseExpression`, all type-specific expression classes, `ValiFlowGlobal`, `ValiSort`, `ValidateNested`, `AddIf`/`When`/`Unless`, freeze/clone, Or-group precedence, cross-property ranges
- All 961 tests pass on net9.0

## [1.6.1] - 2026-03-28

### New Features
- `BaseExpression<TBuilder,T>`: **`Clone()`** — creates a new, independent builder pre-populated with all conditions from the source. The clone starts unfrozen regardless of whether the source is frozen, enabling the same composition pattern as `IQueryable`: build a base set of rules once, then derive specializations without modifying the original. Compiled delegates are not copied — the clone recompiles on first use. Added to `IExpression<TBuilder,T>` interface.

### Test Coverage
- Added 3 new Clone tests: base unchanged after clone mutates, multiple independent derivations, clone of frozen builder starts unfrozen
- Total: 790 tests

## [1.6.0] - 2026-03-28

### New Features
- `BaseExpression<TBuilder,T>`: **Freeze pattern** — builder is permanently sealed on first call to `BuildCached()`, `IsValid()`, `IsNotValid()`, or `Validate()`. Any mutation attempt after that point (`Add`, `Or`, `And`, `WithMessage`, `WithError`, `WithSeverity`, `AddIf`, `When`, `Unless`, `AddSubGroup`, `ValidateNested`) throws `InvalidOperationException` with a clear message.
- `BaseExpression<TBuilder,T>`: new public `Freeze()` method — explicitly seal a builder before handing it to multiple threads (e.g., at DI/startup phase).
- `IExpression<TBuilder,T>`: `Freeze()` added to the public interface.
- `IExpression<TBuilder,T>`: `BuildCached()` XML doc updated to document freeze semantics.

### Breaking Change
- **`BuildCached()` no longer allows post-call mutation.** Previously, calling `Add()`/`And()` after `BuildCached()` would silently invalidate the cache and produce a new delegate. This was a footgun: any shared/singleton builder mutated after compilation would corrupt expression trees under concurrent use. Now it throws `InvalidOperationException`. **Migration:** if you need a builder with different conditions, create a new instance.

### Bug Fix (thread safety)
- Eliminated silent data-corruption when a shared builder was mutated after use. Previously, concurrent mutation during validation could corrupt the `_conditions` `List<T>` internal array. The freeze contract converts this into a fast, deterministic failure with a descriptive error message.

### Test Coverage
- Replaced `BuildCached_AfterAddingCondition_CacheInvalidated_NewInstance` with freeze-contract tests
- Added 7 new freeze tests: BuildCached/IsValid/IsNotValid/Validate each freeze, explicit `Freeze()`, frozen builder still serves read operations
- Total: 787 tests

## [1.5.0] - 2026-03-28

### Documentation
- ValiFlowQuery: added XML `<summary>` and EF Core `<remarks>` to all previously undocumented numeric methods — long/double/decimal/float/short scalar and cross-property InRange (Zero/NotZero/Positive/Negative/GreaterThan/GreaterThanOrEqualTo/LessThan/LessThanOrEqualTo/MinValue/MaxValue/InRange)
- ValiFlowQuery: added XML docs to int InRange scalar and cross-property overloads
- ValiFlowQuery: added XML docs to all DateTime methods — IsBefore/IsAfter/SameMonthAs/SameYearAs/IsInMonth/IsInYear
- ValiFlowQuery: added XML docs to all DateTimeOffset methods — IsBefore/IsAfter/BetweenDates (scalar)
- ValiFlowQuery: added XML docs to all DateOnly methods — IsBefore/IsAfter/BetweenDates/IsInMonth/IsInYear
- ValiFlowQuery: added XML docs to all TimeOnly methods — IsBefore/IsAfter/IsAM/IsPM/IsExactTime/IsInHour

### Test Coverage
- Total: 782 tests (unchanged)

## [1.4.9] - 2026-03-28

### Bug Fixes
- NumericExpression nullable overloads (IsNullOrZero/HasValue/GreaterThan/LessThan for int?/decimal?/long?/double?): added missing selector null guard
- NumericExpression nullable InRange (int?/decimal?/long?): selector null guard now runs before max < min validation

### New Features
- NumericExpression/ValiFlow/INumericExpression: full nullable comparison parity for double?, float?, short? — GreaterThan/LessThan/InRange added to all three layers; IsNullOrZero(short?), HasValue(short?) added
- ValiFlowQuery: added FutureDate(DateTimeOffset) and PastDate(DateTimeOffset) — EF Core translatable

### Test Coverage
- Added 2 new tests: FutureDate and PastDate for DateTimeOffset on ValiFlowQuery
- Total: 782 tests

## [1.4.8] - 2026-03-28

### Bug Fixes
- NumericExpression scalar InRange (all 6 typed overloads): selector null guard now runs before max < min validation — eliminates wrong exception type when selector is null and range is also invalid
- NumericExpression all 50+ scalar overloads: added explicit selector null guard at method boundary for consistent API contract (Zero, NotZero, GreaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo, Positive, Negative, MinValue, MaxValue, IsEven, IsOdd, IsMultipleOf)
- ValiFlowQuery StartsWith, EndsWith, Contains: added selector null guard before value validation — eliminates ambiguous exception when both selector and value are invalid

### New Features
- ValiFlowQuery: added `CountBetween<TValue>` — validates collection element count within [min, max]; completes API parity with CollectionExpression.CountBetween

### Test Coverage
- Added 4 new tests: CountBetween within range, CountBetween null collection
- Total: 780 tests

## [1.4.7] - 2026-03-28

### Bug Fixes
- NumericExpression InRange<TValue>(IComparable): added missing ArgumentNullException selector null guard — completes null-guard coverage for all IComparable overloads
- DateTimeExpression BetweenDates cross-property: replaced fragile Convert fallback with CloneExpression helper that handles MemberExpression, UnaryExpression, and MethodCallExpression

### New Features
- ValiFlowQuery: added nullable float? overloads — GreaterThan, LessThan, InRange
- ValiFlowQuery: added nullable short? overloads — IsNullOrZero, HasValue, GreaterThan, LessThan, InRange — completes full nullable coverage for all numeric types

### Test Coverage
- Added 9 new tests: float? GreaterThan/LessThan/InRange on ValiFlowQuery, short? IsNullOrZero/HasValue/GreaterThan/LessThan/InRange on ValiFlowQuery
- Total: 778 tests

## [1.4.5] - 2026-03-28

### Bug Fixes
- ValiFlowQuery ValidateNested: added ForceCloneVisitor for null-check node — eliminates expression tree node aliasing (matching BaseExpression fix)
- NumericExpression cross-property IComparable<TValue>: added CloneSelectorBody helper — both selectorBodyForCall and freshBody now use reference-distinct nodes in all 4 overloads
- StringExpression ExpressionDeepCloner.VisitMethodCall: added explicit null guard for static method calls (node.Object == null), aligning with CollectionExpression pattern
- ValiFlowQuery MaxCount: null collection now returns true (was false) — aligns with CollectionExpression.MaxCount semantics (null = 0 items <= max)

### Test Coverage
- Added 20 new tests: IsDayOfWeek(DateTimeOffset) on ValiFlowQuery, nullable long?/decimal? overloads (IsNullOrZero/HasValue/GreaterThan/LessThan), TimeOnly IsAfter/IsPM/IsExactTime on ValiFlowQuery, MaxCount null regression, cross-property InRange for long/double/float/short on ValiFlow, BetweenDates cross-property DateTime on ValiFlow, HasValue(float?) and IsNullOrZero(float?) on ValiFlow
- Total: 759 tests

## [1.4.4] - 2026-03-28

### New Features
- NumericExpression/ValiFlow/ValiFlowQuery: added HasValue(float?) overload — completes the nullable numeric HasValue set

### Bug Fixes
- BaseExpression IsNotValid: replaced plain field read post-CompareExchange with atomic return — eliminates stale-delegate race on ARM64
- BaseExpression ValidateNested: replaced no-op ParameterReplacer with ForceCloneVisitor to produce reference-distinct null-check node (real fix for expression tree node aliasing)
- CollectionExpression All/Any/None/EachItem/AnyItem: null-check now uses its own clone of selectorBody — eliminates node aliasing when the same selector is used in multiple calls
- NumericExpression cross-property IComparable<TValue>: else-branch builds selectorBodyForCall as a fresh node so selectorBody is no longer aliased between callExpr and nullCheck
- StringExpression RegexMatch: cache-full guard now uses Volatile.Read — prevents stale read bypassing the cap on ARM64
- DateTimeOffsetExpression InLastDays/InNextDays: aligned guard to days <= 0 (was days < 0), matching DateTimeExpression contract
- ValiFlowQuery: added ArgumentNullException selector null guard to all string, collection, numeric scalar, DateOnly, and TimeOnly methods

### Test Coverage
- Added 11 new tests: cross-property InRange (ValiFlowQuery int/decimal, ValiFlow int + alias regression), IsDayOfWeek (DateTime/DateOnly), BetweenDates cross-property (DateTime/DateOnly), HasValue(double?/float?/int?)
- Total: 741 tests

## [1.4.3] - 2026-03-28

### Bug Fixes
- BaseExpression BuildCached: replaced plain field read after CompareExchange with atomic return — eliminates stale-read risk on ARM64
- BaseExpression ValidateNested: cloned selector.Body for null-check node to eliminate expression tree node aliasing
- NumericExpression cross-property IComparable<TValue>: fixed node aliasing in all 4 overloads (GreaterThan/GreaterThanOrEqualTo/LessThan/LessThanOrEqualTo) for reference-type TValue
- DateTimeOffsetExpression InLastDays/InNextDays: date now evaluated inside lambda (was captured at builder-construction time, causing stale results when predicate was reused across a day boundary)
- CollectionExpression ExpressionDeepCloner.VisitMethodCall: guard against null Object (static method calls) to prevent NullReferenceException
- StringExpression RegexMatch: Interlocked.Increment now fires only on successful TryAdd — eliminates overcount that caused premature cache-full exception under concurrency
- RegularExpression PhonePattern: + prefix now mandatory (was optional), enforcing strict E.164 format
- ValiFlowQuery IsTrue: added ArgumentNullException null guard on selector (was the only method in the class missing it)
- ValiFlowQuery nullable numeric overloads (IsNullOrZero/HasValue/GreaterThan/LessThan for int?/long?/decimal?/double?): added ArgumentNullException null guard on selector
- ValiFlowQuery IsLastDayOfMonth(DateOnly): removed from EF-safe builder — uses DateTime.DaysInMonth which is not EF Core translatable (violates class contract); remains available on ValiFlow<T>

### Test Coverage
- Added 25 new tests: IsNullOrZero(float?), double? GreaterThan/LessThan/InRange, BeforeDate/AfterDate/ExactDate (DateTime), SameMonthAs/SameYearAs, IsInMonth(DateTime), IsWeekday(DateTime/DateOnly), IsTrue null guard, IsEven/IsOdd/IsMultipleOf for long
- Total: 730 tests

## [1.4.2] - 2026-03-28

### New Features
- NumericExpression/ValiFlowQuery: added IsNullOrZero(float?) overload for nullable float columns

### Bug Fixes
- NumericExpression EqualTo<TValue>: added ArgumentNullException null guard with "Use Null() to check for null values." message
- ValiFlowQuery IsInMonth/IsInYear (DateTimeOffset): added EF Core UTC-vs-offset divergence warning in XML docs

## [1.4.1] - 2026-03-28

### Bug Fixes
- StringExpression RegexMatch: cache cap now enforced with atomic Interlocked counter — thread-safe under high concurrency
- ValiFlowQuery DateTime/DateTimeOffset: added ArgumentNullException null guard on selector in all 22 delegating methods
- ValiFlowQuery IsNullOrWhiteSpace/IsNotNullOrWhiteSpace: corrected EF Core remarks — explicitly documents Pomelo < 5.0 as unsupported and recommends IsNullOrEmpty as fallback

## [1.4.0] - 2026-03-28

### New Features
- ValiFlowQuery: added GreaterThan(double?), LessThan(double?), InRange(double?) overloads for nullable double columns
- ValiFlowQuery: added IsLastDayOfMonth(DateOnly) with EF Core note
- ValiFlowQuery: added BeforeDate/AfterDate for DateTime now normalize to date.Date (consistent semantics with ValiFlow)

### Bug Fixes
- StringExpression: RegexMatch cache now capped at 1,000 entries to prevent unbounded memory growth
- CollectionExpression All/Any/None/EachItem/AnyItem: fixed shared selector.Body node across two tree positions (ExpressionDeepCloner)
- CollectionExpression DistinctCount: added count < 0 guard
- CollectionExpression MaxCount: null collection now correctly returns true (has <= max items)
- CollectionExpression Contains<TValue>: added null guard on selector
- NumericExpression: generic scalar comparison overloads now guard against null value argument
- ValiFlowQuery Contains: rejects empty string (would match every non-null row)
- ValiFlowQuery IsWeekend/IsWeekday/IsDayOfWeek: added EF Core untranslatability warning (DayOfWeek not supported on SQL Server/MySQL/Oracle)
- RegularExpression: E.164 phone pattern minimum raised to 7 digits (was 2)
- RegularExpression: email pattern now rejects leading/trailing dots and hyphen-leading domain labels
- StringExpression Contains multi-selector: splits on all whitespace (was space-only)
- DateTimeOffsetExpression InLastDays/InNextDays: UtcNow captured once per call (was read twice causing boundary race)
- DateTimeExpression/DateTimeOffsetExpression/DateOnly/TimeOnly/Boolean: null guards added to all single-selector methods
- ValiFlowQuery nullable numeric section: added EF Core remarks confirming Nullable.HasValue/Value translation

### Documentation
- ValiFlowQuery class-level doc: scoped EF Core safety guarantee to major providers (SQL Server, PostgreSQL, MySQL Pomelo 5.0+, SQLite, Oracle 7.0+)
- SameMonthAs/SameYearAs: added XML docs confirming EF Core translatability
- IsJson: documented that bare primitives (42, true, null) are accepted
- BuildWithGlobal: added EF Core safety warning about globally registered filters
- ValidateAll: added Or-grouping blind spot warning

### Test Coverage
- Added 41 new tests covering: RegexMatch, HasLettersAndNumbers, HasSpecialCharacters, IsPhoneNumber, NotJson, NotBase64, IsToday/IsYesterday/IsTomorrow/IsLeapYear/InLastDays/InNextDays/BeforeDate/AfterDate/ExactDate (DateTime), numeric type overloads (long/decimal/double/short), null edge cases for string methods
- Total: 711 tests

## [1.3.0] - 2025-09-01

### New Features
- DateTimeOffsetExpression: added IsYesterday and IsTomorrow (in-memory only)

### Bug Fixes
- Combine() / & / | operators: short-circuit when either builder is empty (fixes EF Core Constant(true) translation error)
- IComparable<TValue> cross-property overloads (GreaterThan/GreaterThanOrEqualTo/LessThan/LessThanOrEqualTo): null guard now skipped for value types, preventing ArgumentException on DateTime/int/decimal
- IComparable<TValue> scalar overloads: null guard prevents NullReferenceException when value is null reference type
- AddSubGroup empty action: throws ArgumentException with clear message mentioning sub-group instead of "always true"
- ValidateNested empty configure: throws ArgumentException instead of silently producing a null-only check
- ComparisonExpression EqualTo/NotEqualTo: throws ArgumentNullException with "Use Null() to check for null values." message
- Base64Pattern regex: enforces groups-of-4 (length must be multiple of 4 with correct padding)
- UrlPattern regex: removed unescaped dot that matched whitespace characters
- EqualsIgnoreCase: corrected XML doc (not EF Core translatable)

### Test Coverage
- Added ValiFlowQueryTests: 65 tests covering all ValiFlowQuery<T> method groups
- Added regression tests for all bug fixes in this release
- Added IsYesterday/IsTomorrow tests for DateTimeOffset
- Total: 665 tests

## [1.2.0] - 2025-06-01

### New Features
- ValiFlowQuery<T>: new EF Core-safe builder that only exposes methods translatable to SQL — Boolean, Comparison, String (11 methods), Collection (7), Numeric (typed overloads for int/long/double/decimal/float/short + nullable), DateTime (14), DateTimeOffset (8), DateOnly (9), TimeOnly (7)
- Excludes all in-memory-only methods: RegexMatch, IsEmail, IsUrl, IsPhoneNumber, IsGuid, IsJson, IsBase64, IsEven, IsOdd, IsMultipleOf, IsToday, IsYesterday, IsTomorrow, InLastDays, InNextDays, IsLeapYear, IsLastDayOfMonth, All, Any, None, AnyItem, EachItem, HasDuplicates, DistinctCount, IComparable<T> generic overloads
- Static operators &, |, ! and Combine() for composing ValiFlowQuery<T> expressions

## [1.1.0] - 2025-01-01

### New Features
- ValidateNested<TProperty>: validate nested objects with automatic null check
- AnyItem<TValue>: pass if at least one collection element satisfies conditions
- EachItem<TValue>: pass if all collection elements satisfy conditions
- BuildCached(): compile expression once and reuse (thread-safe)
- BuildWithGlobal(): combine local conditions with ambient ValiFlowGlobal filters
- ValiSort<T>: fluent sort builder with By/ThenBy, applies to IQueryable and IEnumerable
- ValiFlowGlobal: static ambient filter registry (thread-safe)
- When/Unless: conditional condition blocks evaluated at runtime
- AddIf: conditionally add conditions based on a bool flag
- WithSeverity / Severity enum: mark errors as Info, Warning, Error, or Critical
- ValidationResult: Warnings, CriticalErrors, ErrorsAbove(), HasAnySeverity()
- PropertyPath on ValidationError: identify which property failed
- ValidateAll: batch validation returning per-item results
- Explain(): human-readable description of the built expression tree
- DateTimeOffset, DateOnly, TimeOnly expression support
- IComparable<T> generic comparison overloads (in-memory)
- Cross-property InRange comparisons (EF Core compatible)
- ExpressionExplainer utility

### Bug Fixes
- Fixed Or() state machine: _groupConditions removed, flat list with isAnd flag
- Fixed BuildCached() thread safety: Volatile.Read + Interlocked.CompareExchange
- Fixed IsValid/IsNotValid recompilation on every call
- Fixed NotEmpty null guard
- Fixed Contains(string, List<selectors>) OR-logic composition
- Fixed ValidateExpressionBody false-positive for binary-zero expressions
- Fixed ArgumentOutOfRangeException constructors in InLastDays/InNextDays
- Fixed RegexMatch: Regex compiled once at build time
- Fixed string.Contains(StringComparison): EF Core compatible via ToLower()
- Fixed cross-property InRange: Expression.Invoke replaced with ParameterReplacer
- Fixed All/Any/None/EachItem/AnyItem: Expression.Quote for EF Core compatibility
- Fixed Validate() thread safety: lock on lazy compile write-back
- Fixed DateTimeOffset/DateOnly stale date capture in IsToday/IsFuture/IsPast
- Fixed CountBetween: null guard and min/max validation
- Fixed InRange: min <= max validation
- Fixed ValiSort enumerable reflection: MethodInfo cached per key type (plain Dictionary, non-thread-safe by design)
