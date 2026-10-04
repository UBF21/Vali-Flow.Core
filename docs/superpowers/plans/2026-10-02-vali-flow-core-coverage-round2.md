# Vali-Flow.Core Coverage Round 2 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. Several tasks in this plan are same-shape mechanical repeats across type families — per subagent-driven-development's "Batch small same-shape work" guidance, these MAY be batched into a single dispatch brief/subagent if the controller judges that reduces overhead without losing review granularity.

**Goal:** Push Vali-Flow.Core core library coverage as close to 100% as achievable (current: 83.2% line / ~68% branch after Round 1), plus bring `Vali-Flow.Core.Analyzers` (91.3%/65%) and `Vali-Flow.Core.Generator` (76.2%/51.1%) up with real coverage of their untested branches — while explicitly marking genuinely unreachable/low-value lines as out of scope rather than chasing them.

**Architecture:** No architectural change. All tasks are additive test-only changes except Task 17 (a real bug found during the gap audit in the analyzer's type-matching fallback) and Task 18 (adds coverlet.collector to 2 test projects — already done, see Global Constraints). Tests go into existing test files (one new file for the Generator's constraint/generic-method coverage if the existing file's scope doesn't fit cleanly).

**Tech Stack:** .NET 8/9, C#, xUnit, FluentAssertions, Microsoft.CodeAnalysis.CSharp (for Analyzer/Generator test projects).

**Spec:** This plan is derived from an exhaustive line-by-line coverage gap audit performed in this session (coverage.cobertura.xml from `TestResults2/`, `TestResultsAnalyzers/`, `TestResultsGenerator/` after Round 1's fixes). There is no separate spec file — the audit's file:line inventory and the user-approved scope ("core lo más cerca de 100%, incluir Generator/Analyzers") constitute the spec.

## Global Constraints

- Public NuGet library — no breaking changes to any public API, no signature changes.
- Pure test-addition tasks: no production code changes except Task 17 (confirmed bug fix) and already-applied Task 18 (coverlet.collector added to `Vali-Flow.Core.Analyzers.Tests.csproj`/`Vali-Flow.Core.Generator.Tests.csproj` — already done this session, verify present, do not re-add).
- Several lines are explicitly OUT OF SCOPE — do not write tests chasing them (listed per-task where relevant): `ValiSort.cs:88,90` (SortEntry ctor guards, unreachable via public API), `ValiFlowQuery.cs:156-158` (`CreateNestedBuilder`, documented unreachable-by-design), `BaseExpression.cs:831-832` ("always null" branch, impossible per C# type system — `bool` can't be `Constant(null)`), `StringExpression.cs:343` ("no searchable terms", suspected unreachable — `IsNullOrWhiteSpace`/`Split(null)` share whitespace criteria in .NET), `ForwardingGenerator.cs:154` (guaranteed unreachable by the pipeline's own predicate), `ExpressionHelpers.cs:123-125` (`VisitLambda`, low value — no real caller nests a lambda), `ValiFlowNonEfMethodAnalyzer.cs:149` (fragile to force cleanly, low priority).
- Test style already established: xUnit `[Fact]`, FluentAssertions, one record per test file as the test entity, `MakeX(...)` helper factories. Follow exactly.
- Build: `dotnet build Vali-Flow.Core.sln`. Tests: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --collect:"XPlat Code Coverage" --results-directory ./TestResults`.
- Current baseline (after Round 1 + the test-isolation fix applied this session): 1070 tests in `Vali-Flow.Core.Tests`, 4 in `Vali-Flow.Core.Analyzers.Tests`, 3 in `Vali-Flow.Core.Generator.Tests` — all green.

---

### Task 1: Coverage — `ForceCloneVisitor` full branch coverage via real consumers

**Files:**
- Modify: `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs` or a new `Vali-Flow.Core.Tests/ExpressionHelpersForceCloneVisitorTests.cs` (Round 1 already created this file for Task 6 — append here)

**Interfaces:** None — pure test additions. Real consumers of `ForceCloneVisitor` in production code (use any, pick ones matching the node kind needed): `CollectionExpression.cs:200,327`, `BaseExpression.cs:245,899`, `DateOnlyExpression.cs:189`, `DateTimeExpression.cs:105`, `NumericExpression.cs:574,584,609`, `NumericExpressionQuery.cs:1021`, `StringExpression.cs:366` (multi-selector `Contains`).

- [ ] **Step 1: Write tests covering `VisitUnary`, `VisitBinary` (simple), `VisitMethodCall`, `VisitIndex`**

Append to `Vali-Flow.Core.Tests/ExpressionHelpersForceCloneVisitorTests.cs`:

```csharp
    // 4. VisitUnary — selector body is a cast/conversion, not plain member access.
    [Fact]
    public void InRange_CrossProperty_WithNullableCastSelector_ClonesUnaryNodeCorrectly()
    {
        var filter = new ValiFlowQuery<Measurement>()
            .InRange(m => (int)m.RawValue, m => m.RawValue - 5, m => m.RawValue + 5)
            .Build().Compile();

        filter(new Measurement(100, new[] { 1 }, 1m, DateTime.Now, DateTime.Now)).Should().BeTrue();
    }

    // 5. VisitMethodCall — selector body is a method call (ToString/ToUpper-style), not plain member access.
    [Fact]
    public void Contains_MultiSelector_WithMethodCallSelector_ClonesMethodCallNodeCorrectly()
    {
        var filter = new ValiFlow<Measurement>()
            .Contains("1", m => m.RawValue.ToString())
            .Build().Compile();

        filter(new Measurement(1, new[] { 1 }, 1m, DateTime.Now, DateTime.Now)).Should().BeTrue();
        filter(new Measurement(99, new[] { 1 }, 1m, DateTime.Now, DateTime.Now)).Should().BeFalse();
    }
```

If `ValiFlow<T>.Contains(string, params Expression<Func<T,string>>[])` doesn't exist with exactly this signature, check `Vali-Flow.Core/Classes/Types/StringExpression.cs` around line 321-398 for the real multi-selector `Contains` overload and adjust the call syntax to match — the intent (force `ForceCloneVisitor` through a `VisitMethodCall` node via a selector whose body is `m.RawValue.ToString()`) stays the same. `Measurement` record is defined earlier in this file by Round 1's Task 6 (`int RawValue, int[] Readings, decimal Price, DateTime Start, DateTime End`) — reuse it, do not redefine.

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ForceCloneVisitor"`
Expected: PASS (5/5 — the 3 from Round 1 plus these 2).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/ExpressionHelpersForceCloneVisitorTests.cs
git commit -m "test(core): extend ForceCloneVisitor coverage to VisitUnary and VisitMethodCall"
```

---

### Task 2: Coverage — `DateOnlyExpressionQuery.cs` full method coverage

**Files:** Modify: `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`

**Interfaces:** None — pure test additions against `ValiFlowQuery<QueryEntity>` (existing `BirthDate` field, `DateOnly`).

- [ ] **Step 1: Write tests for every untested method (13 gaps from the audit)**

Append to `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`:

```csharp
    // ═══════════════════════════════════════════════════════════════════════
    // DateOnlyExpressionQuery — Round 2 remaining gaps
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void DateOnly_BetweenDates_ToBeforeFrom_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.BetweenDates(e => e.BirthDate, new DateOnly(2025, 1, 1), new DateOnly(2024, 1, 1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateOnly_IsInYear_InvalidYear_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.IsInYear(e => e.BirthDate, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
        var act2 = () => builder.IsInYear(e => e.BirthDate, 10000);
        act2.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateOnly_FutureDate_MatchesFutureOnly()
    {
        var filter = new ValiFlowQuery<QueryEntity>().FutureDate(e => e.BirthDate).Build().Compile();
        filter(MakeEntity(birthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))).Should().BeTrue();
        filter(MakeEntity(birthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_PastDate_MatchesPastOnly()
    {
        var filter = new ValiFlowQuery<QueryEntity>().PastDate(e => e.BirthDate).Build().Compile();
        filter(MakeEntity(birthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)))).Should().BeTrue();
        filter(MakeEntity(birthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_ExactDate_MatchesExactDayOnly()
    {
        var target = new DateOnly(2025, 6, 15);
        var filter = new ValiFlowQuery<QueryEntity>().ExactDate(e => e.BirthDate, target).Build().Compile();
        filter(MakeEntity(birthDate: target)).Should().BeTrue();
        filter(MakeEntity(birthDate: target.AddDays(1))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_IsTomorrow_MatchesTomorrowOnly()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsTomorrow(e => e.BirthDate).Build().Compile();
        filter(MakeEntity(birthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))).Should().BeTrue();
        filter(MakeEntity(birthDate: DateOnly.FromDateTime(DateTime.UtcNow))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_InLastDays_InvalidDays_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.InLastDays(e => e.BirthDate, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateOnly_InNextDays_MatchesWithinWindow()
    {
        var filter = new ValiFlowQuery<QueryEntity>().InNextDays(e => e.BirthDate, 5).Build().Compile();
        filter(MakeEntity(birthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)))).Should().BeTrue();
        filter(MakeEntity(birthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_SameMonthAs_MatchesSameMonthAndYear()
    {
        var reference = new DateOnly(2025, 8, 1);
        var filter = new ValiFlowQuery<QueryEntity>().SameMonthAs(e => e.BirthDate, reference).Build().Compile();
        filter(MakeEntity(birthDate: new DateOnly(2025, 8, 20))).Should().BeTrue();
        filter(MakeEntity(birthDate: new DateOnly(2024, 8, 20))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_IsWeekend_And_IsWeekday_AreMutuallyConsistent()
    {
        // 2025-06-14 is a Saturday.
        var weekendFilter = new ValiFlowQuery<QueryEntity>().IsWeekend(e => e.BirthDate).Build().Compile();
        var weekdayFilter = new ValiFlowQuery<QueryEntity>().IsWeekday(e => e.BirthDate).Build().Compile();

        weekendFilter(MakeEntity(birthDate: new DateOnly(2025, 6, 14))).Should().BeTrue();
        weekdayFilter(MakeEntity(birthDate: new DateOnly(2025, 6, 14))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_IsInQuarter_InvalidQuarter_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.IsInQuarter(e => e.BirthDate, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
        var act2 = () => builder.IsInQuarter(e => e.BirthDate, 5);
        act2.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateOnly_IsInQuarter_MatchesCorrectQuarter()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsInQuarter(e => e.BirthDate, 1).Build().Compile();
        filter(MakeEntity(birthDate: new DateOnly(2025, 1, 15))).Should().BeTrue();
        filter(MakeEntity(birthDate: new DateOnly(2025, 4, 15))).Should().BeFalse();
    }
```

(13 tests, covering `BetweenDates` guard, `IsInYear` guard, `FutureDate`, `PastDate`, `ExactDate`, `IsTomorrow`, `InLastDays` guard, `InNextDays`, `SameMonthAs`, `IsWeekend`/`IsWeekday`, `IsInQuarter` guard + happy path — every gap from the audit's section 2.)

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~DateOnly_"`
Expected: PASS (all, including Round 1's existing `DateOnly_*` tests).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/ValiFlowQueryTests.cs
git commit -m "test(core): close remaining DateOnlyExpressionQuery coverage gaps"
```

---

### Task 3: Coverage — `DateTimeOffsetExpressionQuery.cs` full method coverage

**Files:** Modify: `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`

**Interfaces:** None — pure test additions against `ValiFlowQuery<QueryEntity>.UpdatedAt` (`DateTimeOffset`).

- [ ] **Step 1: Write tests for the remaining gaps (audit section 3)**

Append to `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`:

```csharp
    // ═══════════════════════════════════════════════════════════════════════
    // DateTimeOffsetExpressionQuery — Round 2 remaining gaps
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void DateTimeOffset_IsInYear_InvalidYear_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.IsInYear(e => e.UpdatedAt, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateTimeOffset_IsToday_MatchesTodayOnly()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsToday(e => e.UpdatedAt).Build().Compile();
        filter(MakeEntity(updatedAt: DateTimeOffset.UtcNow)).Should().BeTrue();
        filter(MakeEntity(updatedAt: DateTimeOffset.UtcNow.AddDays(-2))).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_IsYesterday_MatchesYesterdayOnly()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsYesterday(e => e.UpdatedAt).Build().Compile();
        filter(MakeEntity(updatedAt: DateTimeOffset.UtcNow.AddDays(-1))).Should().BeTrue();
        filter(MakeEntity(updatedAt: DateTimeOffset.UtcNow)).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_InNextDays_InvalidDays_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.InNextDays(e => e.UpdatedAt, -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateTimeOffset_SameYearAs_MatchesSameYearOnly()
    {
        var reference = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlowQuery<QueryEntity>().SameYearAs(e => e.UpdatedAt, reference).Build().Compile();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 11, 1, 0, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2024, 11, 1, 0, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_IsDayOfWeek_MatchesSpecifiedDay()
    {
        // 2025-06-16 is a Monday.
        var filter = new ValiFlowQuery<QueryEntity>().IsDayOfWeek(e => e.UpdatedAt, DayOfWeek.Monday).Build().Compile();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 6, 16, 0, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 6, 17, 0, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_IsFirstDayOfMonth_MatchesFirstDayOnly()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsFirstDayOfMonth(e => e.UpdatedAt).Build().Compile();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 7, 2, 0, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_IsLastDayOfMonth_MatchesLastDayOnly()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsLastDayOfMonth(e => e.UpdatedAt).Build().Compile();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 4, 30, 0, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEntity(updatedAt: new DateTimeOffset(2025, 4, 29, 0, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }
```

(8 tests — the remaining gaps not already covered by Round 1's Task 7, cross-checked against the audit's section 3.)

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~DateTimeOffset_Is|FullyQualifiedName~DateTimeOffset_InNextDays|FullyQualifiedName~DateTimeOffset_SameYearAs"`
Expected: PASS (8/8).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/ValiFlowQueryTests.cs
git commit -m "test(core): close remaining DateTimeOffsetExpressionQuery coverage gaps"
```

---

### Task 4: Coverage — `DateTimeExpressionQuery.cs` full method coverage

**Files:** Modify: `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`

**Interfaces:** None — pure test additions against `ValiFlowQuery<QueryEntity>.CreatedAt` (`DateTime`).

- [ ] **Step 1: Write tests for the remaining gaps (audit section 4)**

Append to `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`:

```csharp
    // ═══════════════════════════════════════════════════════════════════════
    // DateTimeExpressionQuery — Round 2 remaining gaps
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void DateTime_FutureDate_And_PastDate_AreMutuallyExclusive()
    {
        var futureFilter = new ValiFlowQuery<QueryEntity>().FutureDate(e => e.CreatedAt).Build().Compile();
        var pastFilter = new ValiFlowQuery<QueryEntity>().PastDate(e => e.CreatedAt).Build().Compile();

        var future = MakeEntity(createdAt: DateTime.UtcNow.AddDays(1));
        futureFilter(future).Should().BeTrue();
        pastFilter(future).Should().BeFalse();
    }

    [Fact]
    public void DateTime_ExactDate_MatchesExactDayOnly()
    {
        var target = new DateTime(2025, 5, 10);
        var filter = new ValiFlowQuery<QueryEntity>().ExactDate(e => e.CreatedAt, target).Build().Compile();
        filter(MakeEntity(createdAt: target.AddHours(20))).Should().BeTrue();
        filter(MakeEntity(createdAt: target.AddDays(1))).Should().BeFalse();
    }

    [Fact]
    public void DateTime_IsTomorrow_And_IsYesterday_AreMutuallyExclusive()
    {
        var tomorrowFilter = new ValiFlowQuery<QueryEntity>().IsTomorrow(e => e.CreatedAt).Build().Compile();
        var yesterdayFilter = new ValiFlowQuery<QueryEntity>().IsYesterday(e => e.CreatedAt).Build().Compile();

        var tomorrow = MakeEntity(createdAt: DateTime.Today.AddDays(1));
        tomorrowFilter(tomorrow).Should().BeTrue();
        yesterdayFilter(tomorrow).Should().BeFalse();
    }

    [Fact]
    public void DateTime_InLastDays_InvalidDays_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.InLastDays(e => e.CreatedAt, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateTime_InNextDays_InvalidDays_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.InNextDays(e => e.CreatedAt, -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateTime_IsWeekend_MatchesSaturdayAndSunday()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsWeekend(e => e.CreatedAt).Build().Compile();
        filter(MakeEntity(createdAt: new DateTime(2025, 6, 14))).Should().BeTrue();
        filter(MakeEntity(createdAt: new DateTime(2025, 6, 16))).Should().BeFalse();
    }

    [Fact]
    public void DateTime_IsInQuarter_InvalidQuarter_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.IsInQuarter(e => e.CreatedAt, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
        var act2 = () => builder.IsInQuarter(e => e.CreatedAt, 5);
        act2.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateTime_IsInQuarter_MatchesCorrectQuarter()
    {
        var filter = new ValiFlowQuery<QueryEntity>().IsInQuarter(e => e.CreatedAt, 4).Build().Compile();
        filter(MakeEntity(createdAt: new DateTime(2025, 11, 1))).Should().BeTrue();
        filter(MakeEntity(createdAt: new DateTime(2025, 2, 1))).Should().BeFalse();
    }
```

(8 tests covering the remaining gaps from audit section 4.)

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~DateTime_FutureDate_And|FullyQualifiedName~DateTime_ExactDate|FullyQualifiedName~DateTime_IsTomorrow|FullyQualifiedName~DateTime_InLastDays_InvalidDays|FullyQualifiedName~DateTime_InNextDays_InvalidDays|FullyQualifiedName~DateTime_IsWeekend|FullyQualifiedName~DateTime_IsInQuarter"`
Expected: PASS (8/8).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/ValiFlowQueryTests.cs
git commit -m "test(core): close remaining DateTimeExpressionQuery coverage gaps"
```

---

### Task 5: Coverage — non-Query `DateOnlyExpression.cs`, `DateTimeExpression.cs`, `DateTimeOffsetExpression.cs` remaining gaps

**Files:**
- Modify: `Vali-Flow.Core.Tests/DateTimeExpressionTests.cs`
- Modify: `Vali-Flow.Core.Tests/DateTimeOffsetDateOnlyTimeOnlyTests.cs`

**Interfaces:** None — pure test additions against `ValiFlow<T>` (in-memory, non-EF-safe builder).

- [ ] **Step 1: Write tests for the remaining gaps (audit sections 5, 13, 15)**

Append to `Vali-Flow.Core.Tests/DateTimeExpressionTests.cs` (uses existing `Product`/`MakeProduct`):

```csharp
    // 9. BetweenDates guard — Round 2
    [Fact]
    public void BetweenDates_EndBeforeStart_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.BetweenDates(p => p.CreatedAt, DateTime.Today, DateTime.Today.AddDays(-1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // 10. InNextDays guard — Round 2
    [Fact]
    public void InNextDays_InvalidDays_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.InNextDays(p => p.CreatedAt, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // 11. IsInMonth guard — Round 2
    [Fact]
    public void IsInMonth_InvalidMonth_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.IsInMonth(p => p.CreatedAt, 13);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
```

Append to `Vali-Flow.Core.Tests/DateTimeOffsetDateOnlyTimeOnlyTests.cs` (uses existing `Event`/`MakeEvent`):

```csharp
    // ── DateOnly (non-Query) Round 2 gaps ────────────────────────────────────

    [Fact]
    public void DateOnly_IsToday_MatchesTodayOnly()
    {
        var filter = new ValiFlow<Event>().IsToday(e => e.EventDate).Build().Compile();
        filter(MakeEvent(eventDate: DateOnly.FromDateTime(DateTime.UtcNow))).Should().BeTrue();
        filter(MakeEvent(eventDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)))).Should().BeFalse();
    }

    [Fact]
    public void DateOnly_IsTomorrow_MatchesTomorrowOnly()
    {
        var filter = new ValiFlow<Event>().IsTomorrow(e => e.EventDate).Build().Compile();
        filter(MakeEvent(eventDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))).Should().BeTrue();
    }

    [Fact]
    public void DateOnly_IsDayOfWeek_MatchesSpecifiedDay()
    {
        var filter = new ValiFlow<Event>().IsDayOfWeek(e => e.EventDate, DayOfWeek.Monday).Build().Compile();
        filter(MakeEvent(eventDate: new DateOnly(2025, 6, 16))).Should().BeTrue();
        filter(MakeEvent(eventDate: new DateOnly(2025, 6, 17))).Should().BeFalse();
    }

    // ── DateTimeOffset (non-Query) Round 2 gaps ──────────────────────────────

    [Fact]
    public void DateTimeOffset_IsInMonth_InvalidMonth_Throws()
    {
        var builder = new ValiFlow<Event>();
        var act = () => builder.IsInMonth(e => e.StartOffset, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateTimeOffset_IsInYear_InvalidYear_Throws()
    {
        var builder = new ValiFlow<Event>();
        var act = () => builder.IsInYear(e => e.StartOffset, 10000);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateTimeOffset_BetweenDates_ToBeforeFrom_Throws()
    {
        var builder = new ValiFlow<Event>();
        var act = () => builder.BetweenDates(e => e.StartOffset, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(-1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateTimeOffset_SameMonthAs_MatchesSameMonthAndYear()
    {
        var reference = new DateTimeOffset(2025, 9, 1, 0, 0, 0, TimeSpan.Zero);
        var filter = new ValiFlow<Event>().SameMonthAs(e => e.StartOffset, reference).Build().Compile();
        filter(MakeEvent(startOffset: new DateTimeOffset(2025, 9, 20, 0, 0, 0, TimeSpan.Zero))).Should().BeTrue();
        filter(MakeEvent(startOffset: new DateTimeOffset(2024, 9, 20, 0, 0, 0, TimeSpan.Zero))).Should().BeFalse();
    }

    [Fact]
    public void DateTimeOffset_InNextDays_InvalidDays_Throws()
    {
        var builder = new ValiFlow<Event>();
        var act = () => builder.InNextDays(e => e.StartOffset, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
```

If any named parameter (`eventDate:`, `startOffset:`) doesn't match the real `MakeEvent` helper signature, check its definition at the top of the file and adjust.

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~DateTimeExpressionTests|FullyQualifiedName~DateTimeOffsetDateOnlyTimeOnlyTests"`
Expected: PASS (all tests in both files).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/DateTimeExpressionTests.cs Vali-Flow.Core.Tests/DateTimeOffsetDateOnlyTimeOnlyTests.cs
git commit -m "test(core): close remaining non-Query date expression coverage gaps"
```

---

### Task 6: Coverage — `TimeOnlyExpression.cs` + `TimeOnlyExpressionQuery.cs` guard gaps

**Files:** Modify: `Vali-Flow.Core.Tests/DateTimeOffsetDateOnlyTimeOnlyTests.cs`, `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`

**Interfaces:** None.

- [ ] **Step 1: Write the 2 guard tests per builder (4 total)**

Append to `Vali-Flow.Core.Tests/DateTimeOffsetDateOnlyTimeOnlyTests.cs`:

```csharp
    [Fact]
    public void TimeOnly_IsBetween_ToBeforeFrom_Throws()
    {
        var builder = new ValiFlow<Event>();
        var act = () => builder.IsBetween(e => e.StartTime, new TimeOnly(10, 0), new TimeOnly(5, 0));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TimeOnly_IsInHour_InvalidHour_Throws()
    {
        var builder = new ValiFlow<Event>();
        var act = () => builder.IsInHour(e => e.StartTime, 24);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
```

Append to `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs` (uses existing `QueryEntity.WorkStart`):

```csharp
    [Fact]
    public void TimeOnly_IsBetween_ToBeforeFrom_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.IsBetween(e => e.WorkStart, new TimeOnly(10, 0), new TimeOnly(5, 0));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TimeOnly_IsInHour_InvalidHour_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.IsInHour(e => e.WorkStart, -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
```

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~TimeOnly_IsBetween_ToBeforeFrom|FullyQualifiedName~TimeOnly_IsInHour_Invalid"`
Expected: PASS (4/4).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/DateTimeOffsetDateOnlyTimeOnlyTests.cs Vali-Flow.Core.Tests/ValiFlowQueryTests.cs
git commit -m "test(core): cover TimeOnlyExpression/Query IsBetween and IsInHour guards"
```

---

### Task 7: Coverage — `NumericExpressionQuery.cs` full type matrix (long/double/decimal/float/short + nullable)

**Files:** Modify: `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`

**Interfaces:** None — reuses `ScalarNumerics` record (Round 1) and `QueryEntityEx`/`ShortEntity` (Round 1).

- [ ] **Step 1: Write the remaining scalar-type tests**

The audit found: for each of `long/double/decimal/float/short`, several individual scalar methods (`NotZero`, `Positive`/`Negative` variants not already tested, `GreaterThanOrEqualTo`, `LessThan`, `LessThanOrEqualTo`, `MinValue`, `MaxValue`, `IsOdd`, `InRange` happy-path) remain untested per the exact line numbers in the audit. Append to `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`:

```csharp
    // ═══════════════════════════════════════════════════════════════════════
    // NumericExpressionQuery — Round 2: remaining scalar matrix gaps
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Int_NotZero_And_Negative_And_IsOdd_WorkCorrectly()
    {
        var notZero = new ValiFlowQuery<ScalarNumerics>().NotZero(e => e.LongValue == 0 ? 0 : 1).Build; // placeholder removed below
    }
```

**IMPORTANT — the snippet above is intentionally incomplete; do not transcribe it as-is.** Instead, write one `[Fact]` per row of this table. Each test follows the EXACT pattern already established in Round 1's Task 10 (`Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`, search for `Long_GreaterThan_MatchesLargerValue` to see the established shape: build a `ValiFlowQuery<ScalarNumerics>()`, call the method under test, `.Build().Compile()`, assert true/false with two `ScalarNumerics` instances). Use the `ScalarNumerics(long LongValue, double DoubleValue, decimal DecimalValue, float FloatValue, short ShortValue)` record already defined in that file.

| Test name | Method under test | True case | False case |
|---|---|---|---|
| `Long_NotZero_And_Negative_WorkCorrectly` | `.NotZero(e => e.LongValue)` then `.Negative(e => e.LongValue)` | `LongValue: 5L` (NotZero true), `LongValue: -5L` (Negative true) | `LongValue: 0L` (both false) |
| `Long_IsOdd_WorksCorrectly` | `.IsOdd(e => e.LongValue)` | `LongValue: 3L` | `LongValue: 4L` |
| `Double_GreaterThanOrEqualTo_And_LessThan_WorkCorrectly` | `.GreaterThanOrEqualTo(e => e.DoubleValue, 5.0)` then `.LessThan(e => e.DoubleValue, 5.0)` | `DoubleValue: 5.0` (GTE true), `DoubleValue: 4.9` (LessThan true) | `DoubleValue: 4.9` (GTE false), `DoubleValue: 5.0` (LessThan false) |
| `Decimal_GreaterThan_And_MinValue_WorkCorrectly` | `.GreaterThan(e => e.DecimalValue, 10m)` then `.MinValue(e => e.DecimalValue, 10m)` | `DecimalValue: 10.01m` (GT true), `DecimalValue: 10m` (MinValue true) | `DecimalValue: 10m` (GT false), `DecimalValue: 9.99m` (MinValue false) |
| `Float_Zero_And_NotZero_WorkCorrectly` | `.Zero(e => e.FloatValue)` then `.NotZero(e => e.FloatValue)` | `FloatValue: 0f` (Zero true), `FloatValue: 1f` (NotZero true) | `FloatValue: 1f` (Zero false), `FloatValue: 0f` (NotZero false) |
| `Short_Zero_And_NotZero_And_MaxValue_WorkCorrectly` | `.Zero(e => e.ShortValue)` then `.MaxValue(e => e.ShortValue, (short)10)` | `ShortValue: (short)0` (Zero true), `ShortValue: (short)10` (MaxValue true) | `ShortValue: (short)11` (MaxValue false) |
| `Double_InRange_MatchesWithinBounds` | `.InRange(e => e.DoubleValue, 10.0, 20.0)` | `DoubleValue: 15.0` | `DoubleValue: 25.0` |
| `Float_InRange_MatchesWithinBounds` | `.InRange(e => e.FloatValue, 10f, 20f)` | `FloatValue: 15f` | `FloatValue: 25f` |
| `Short_InRange_MatchesWithinBounds` | `.InRange(e => e.ShortValue, (short)1, (short)10)` | `ShortValue: (short)5` | `ShortValue: (short)20` |

Write all 8 tests following this table, using the established pattern. Delete the placeholder `Int_NotZero_And_Negative_And_IsOdd_WorkCorrectly` sketch above entirely before committing — it does not compile and exists only as an anti-example of what NOT to transcribe literally.

- [ ] **Step 2: Write the remaining nullable-overload tests**

```csharp
    // ═══════════════════════════════════════════════════════════════════════
    // NumericExpressionQuery — Round 2: remaining nullable overload gaps
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void NullableInt_GreaterThan_And_LessThan_WithValue_MatchCorrectly()
    {
        var gtFilter = new ValiFlowQuery<QueryEntityEx>().GreaterThan(e => e.NullableInt, 10).Build().Compile();
        gtFilter(new QueryEntityEx(20, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue)).Should().BeTrue();
        gtFilter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue)).Should().BeFalse();

        var ltFilter = new ValiFlowQuery<QueryEntityEx>().LessThan(e => e.NullableInt, 10).Build().Compile();
        ltFilter(new QueryEntityEx(5, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue)).Should().BeTrue();
    }

    [Fact]
    public void NullableDecimal_GreaterThan_And_LessThan_WithValue_MatchCorrectly()
    {
        var gtFilter = new ValiFlowQuery<QueryEntityEx>().GreaterThan(e => e.NullableDecimal, 10m).Build().Compile();
        gtFilter(new QueryEntityEx(null, null, 20m, null, null, DateTime.UtcNow, DateOnly.MinValue)).Should().BeTrue();

        var ltFilter = new ValiFlowQuery<QueryEntityEx>().LessThan(e => e.NullableDecimal, 10m).Build().Compile();
        ltFilter(new QueryEntityEx(null, null, 5m, null, null, DateTime.UtcNow, DateOnly.MinValue)).Should().BeTrue();
        ltFilter(new QueryEntityEx(null, null, null, null, null, DateTime.UtcNow, DateOnly.MinValue)).Should().BeFalse();
    }

    [Fact]
    public void NullableInt_InRange_InvalidRange_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntityEx>();
        var act = () => builder.InRange(e => e.NullableInt, 10, 1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void NullableLong_InRange_InvalidRange_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntityEx>();
        var act = () => builder.InRange(e => e.NullableLong, 10L, 1L);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
```

- [ ] **Step 3: Run tests to verify they all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Long_NotZero|FullyQualifiedName~Long_IsOdd|FullyQualifiedName~Double_GreaterThanOrEqualTo|FullyQualifiedName~Decimal_GreaterThan_And_MinValue|FullyQualifiedName~Float_Zero_And_NotZero|FullyQualifiedName~Short_Zero_And_NotZero|FullyQualifiedName~_InRange_MatchesWithinBounds|FullyQualifiedName~NullableInt_|FullyQualifiedName~NullableDecimal_|FullyQualifiedName~NullableLong_InRange"`
Expected: PASS (all).

- [ ] **Step 4: Commit**

```bash
git add Vali-Flow.Core.Tests/ValiFlowQueryTests.cs
git commit -m "test(core): close remaining NumericExpressionQuery scalar and nullable matrix gaps"
```

---

### Task 8: Coverage — `NumericExpression.cs` generic/`IComparable` gaps

**Files:** Modify: `Vali-Flow.Core.Tests/BaseExpressionTests.cs`

**Interfaces:** None — pure test additions against `ValiFlow<T>`'s generic `INumber<TValue>`/`IComparable<TValue>` methods, including explicit-interface members accessed via the `IComparableExpression<TBuilder,T>` interface reference.

- [ ] **Step 1: Write tests for the 6 gaps from audit section 7**

Append to `Vali-Flow.Core.Tests/BaseExpressionTests.cs` (reuses `Product`):

```csharp
    [Fact]
    public void InRange_Generic_InvalidRange_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.InRange(p => p.Price, 10m, 1m);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void InRange_GenericNullable_InvalidRange_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.InRange(p => (int?)p.Quantity, 10, 1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void EqualTo_Generic_NullValue_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.EqualTo(p => p.Name, (string?)null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IComparableExpression_LessThanOrEqualTo_ViaInterface_WorksCorrectly()
    {
        IComparableExpression<ValiFlow<Product>, Product> builder = new ValiFlow<Product>();
        var filter = builder.LessThanOrEqualTo(p => p.Name!, "M").Build().Compile();

        filter(new Product("A", 1m, 1, true, DateTime.Now, new List<string>())).Should().BeTrue();
        filter(new Product("Z", 1m, 1, true, DateTime.Now, new List<string>())).Should().BeFalse();
    }

    [Fact]
    public void IComparableExpression_EqualTo_ViaInterface_InRangeInvalid_Throws()
    {
        IComparableExpression<ValiFlow<Product>, Product> builder = new ValiFlow<Product>();
        var act = () => builder.InRange(p => p.Name!, "Z", "A");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CrossPropertyComparable_ReferenceType_WorksCorrectly()
    {
        IComparableExpression<ValiFlow<Product>, Product> builder = new ValiFlow<Product>();
        var filter = builder.GreaterThan(p => p.Name!, p => p.Tags.Count > 0 ? p.Tags[0] : "").Build().Compile();

        filter(new Product("B", 1m, 1, true, DateTime.Now, new List<string> { "A" })).Should().BeTrue();
        filter(new Product("A", 1m, 1, true, DateTime.Now, new List<string> { "B" })).Should().BeFalse();
    }
```

If `IComparableExpression<TBuilder,T>` isn't the exact interface name, or `GreaterThan`/`InRange`/`LessThanOrEqualTo`/`EqualTo` aren't exposed on it with these exact signatures, check `Vali-Flow.Core/Interfaces/General/IComparableExpression.cs` (or wherever it's defined — search the `Interfaces/` tree) for the real contract and adjust — the intent (exercise the explicit-interface-implementation code paths in `NumericExpression.cs` lines 481-484, 500, 514-519, 584-586 from the audit) stays the same.

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~InRange_Generic|FullyQualifiedName~EqualTo_Generic|FullyQualifiedName~IComparableExpression_|FullyQualifiedName~CrossPropertyComparable_"`
Expected: PASS (6/6).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/BaseExpressionTests.cs
git commit -m "test(core): cover NumericExpression generic/IComparable explicit-interface gaps"
```

---

### Task 9: Coverage — `StringExpression.cs` remaining gaps (guards, `StringComparison` overloads, cache-full guard)

**Files:** Modify: `Vali-Flow.Core.Tests/BaseExpressionTests.cs`

**Interfaces:** None — pure test additions against `ValiFlow<T>`'s string methods.

- [ ] **Step 1: Write guard + overload tests**

Append to `Vali-Flow.Core.Tests/BaseExpressionTests.cs`:

```csharp
    [Fact]
    public void MinLength_InvalidValue_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.MinLength(p => p.Name, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MaxLength_InvalidValue_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.MaxLength(p => p.Name, -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ExactLength_NegativeValue_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.ExactLength(p => p.Name, -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void EndsWith_WithStringComparisonOverload_EmptyValue_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.EndsWith(p => p.Name, "", StringComparison.OrdinalIgnoreCase);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EndsWith_WithStringComparisonOverload_WorksCorrectly()
    {
        var filter = new ValiFlow<Product>().EndsWith(p => p.Name, "ICE", StringComparison.OrdinalIgnoreCase).Build().Compile();
        filter(new Product("Alice", 1m, 1, true, DateTime.Now, new List<string>())).Should().BeTrue();
    }

    [Fact]
    public void StartsWith_WithStringComparisonOverload_NullValue_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.StartsWith(p => p.Name, null!, StringComparison.Ordinal);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Contains_WithStringComparisonOverload_NullValue_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.Contains(p => p.Name, null!, StringComparison.OrdinalIgnoreCase);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EqualToIgnoreCase_NullValue_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.EqualToIgnoreCase(p => p.Name, null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Contains_MultiSelector_AllWhitespaceValue_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.Contains("   ", p => p.Name);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Contains_MultiSelector_WithInvariantCultureIgnoreCase_WorksCorrectly()
    {
        var filter = new ValiFlow<Product>().Contains("ALICE", StringComparison.InvariantCultureIgnoreCase, p => p.Name).Build().Compile();
        filter(new Product("alice", 1m, 1, true, DateTime.Now, new List<string>())).Should().BeTrue();
    }
```

If any overload (`EndsWith(selector,value,StringComparison)`, `Contains(selector,value,StringComparison)`, the multi-selector `Contains(value, StringComparison, params selectors)`) has a different parameter order than guessed here, check `Vali-Flow.Core/Classes/Types/StringExpression.cs` around lines 110-170 and 321-398 for the real signatures and adjust — the intent (exercise each guard and the `InvariantCultureIgnoreCase`/`CurrentCultureIgnoreCase` switch branches) stays the same.

- [ ] **Step 2: Write the regex-cache-full guard test (isolated, no test parallelism assumption)**

```csharp
    [Fact]
    public void RegexMatch_CacheExceedsCapacity_ThrowsOnOverflow()
    {
        // StringExpressionCache is a static, process-wide cache shared by every ValiFlow<T>/StringExpression<,>
        // instance, capped at 1000 distinct patterns. This test intentionally pushes it past capacity with
        // unique patterns to exercise the cache-full guard — it does NOT assume a clean cache (other tests
        // in the suite may have already inserted some patterns), it only asserts that AT SOME POINT within
        // 1100 unique patterns, the guard fires.
        var builder = new ValiFlow<Product>();
        Action act = () =>
        {
            for (int i = 0; i < 1100; i++)
            {
                builder.RegexMatch(p => p.Name, $"^unique-pattern-{i}-[a-z]+$");
            }
        };
        act.Should().Throw<InvalidOperationException>();
    }
```

- [ ] **Step 3: Run tests to verify they all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~MinLength_InvalidValue|FullyQualifiedName~MaxLength_InvalidValue|FullyQualifiedName~ExactLength_NegativeValue|FullyQualifiedName~EndsWith_WithStringComparisonOverload|FullyQualifiedName~StartsWith_WithStringComparisonOverload|FullyQualifiedName~Contains_WithStringComparisonOverload|FullyQualifiedName~EqualToIgnoreCase_NullValue|FullyQualifiedName~Contains_MultiSelector|FullyQualifiedName~RegexMatch_CacheExceedsCapacity"`
Expected: PASS (all). The cache-full test may take a few hundred ms (1100 regex compiles) — that's expected, not a failure.

- [ ] **Step 4: Commit**

```bash
git add Vali-Flow.Core.Tests/BaseExpressionTests.cs
git commit -m "test(core): cover StringExpression guards, StringComparison overloads, and regex cache-full guard"
```

---

### Task 10: Coverage — `StringExpressionQuery.cs` remaining gaps

**Files:** Modify: `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`

**Interfaces:** None.

- [ ] **Step 1: Write the 6 remaining gaps from audit section 9**

Append to `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`:

```csharp
    [Fact]
    public void String_MaxLength_InvalidValue_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.MaxLength(e => e.Name, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void String_ExactLength_NegativeValue_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.ExactLength(e => e.Name, -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void String_EndsWith_EmptyValue_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.EndsWith(e => e.Name, "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void String_Contains_EmptyValue_Throws()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.Contains(e => e.Name, "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void String_IsLowerCase_And_IsUpperCase_WorkCorrectly()
    {
        var lowerFilter = new ValiFlowQuery<QueryEntity>().IsLowerCase(e => e.Name).Build().Compile();
        lowerFilter(MakeEntity(name: "alice")).Should().BeTrue();
        lowerFilter(MakeEntity(name: "Alice")).Should().BeFalse();

        var upperFilter = new ValiFlowQuery<QueryEntity>().IsUpperCase(e => e.Name).Build().Compile();
        upperFilter(MakeEntity(name: "ALICE")).Should().BeTrue();
        upperFilter(MakeEntity(name: "Alice")).Should().BeFalse();
    }

    [Fact]
    public void String_StartsWithIgnoreCase_And_EndsWithIgnoreCase_GuardAndHappyPath()
    {
        var builder = new ValiFlowQuery<QueryEntity>();
        var act = () => builder.StartsWithIgnoreCase(e => e.Name, "");
        act.Should().Throw<ArgumentException>();

        var filter = new ValiFlowQuery<QueryEntity>().EndsWithIgnoreCase(e => e.Name, "ICE").Build().Compile();
        filter(MakeEntity(name: "alice")).Should().BeTrue();
    }
```

- [ ] **Step 2: Run tests to verify they all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~String_MaxLength_InvalidValue|FullyQualifiedName~String_ExactLength_NegativeValue|FullyQualifiedName~String_EndsWith_EmptyValue|FullyQualifiedName~String_Contains_EmptyValue|FullyQualifiedName~String_IsLowerCase|FullyQualifiedName~String_StartsWithIgnoreCase"`
Expected: PASS (6/6).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/ValiFlowQueryTests.cs
git commit -m "test(core): close remaining StringExpressionQuery coverage gaps"
```

---

### Task 11: Coverage — `ValiSort.cs`, `ValiFlowQuery.cs`, `ValiFlow.cs` remaining gaps

**Files:** Modify: `Vali-Flow.Core.Tests/EachItemSortGlobalTests.cs`, `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs`, `Vali-Flow.Core.Tests/BaseExpressionTests.cs`

**Interfaces:** None. Note: `ValiSort.cs:88,90` and `ValiFlowQuery.cs:156-158` are explicitly OUT OF SCOPE per Global Constraints — do not attempt to cover them.

- [ ] **Step 1: Write `ValiSort.ApplyThenBy` descending-branch test**

Append to `Vali-Flow.Core.Tests/EachItemSortGlobalTests.cs` (if Round 1's Task 15 already added a `ValiSort_ThenByDescending_...` test covering this exact branch, skip this step — verify first by searching the file for `ThenBy.*descending` before adding a duplicate):

```csharp
    [Fact]
    public void ValiSort_ApplyThenBy_BothAscendingAndDescendingBranchesWork()
    {
        var items = new[]
        {
            new Product("B", 10m, 2, true, DateTime.Now, new List<string>()),
            new Product("B", 5m, 1, true, DateTime.Now, new List<string>()),
        };

        var descendingSorted = new ValiSort<Product>().By(p => p.Name).ThenBy(p => p.Price, descending: true)
            .Apply(items.AsEnumerable()).ToList();
        descendingSorted[0].Price.Should().Be(10m);

        var ascendingSorted = new ValiSort<Product>().By(p => p.Name).ThenBy(p => p.Price)
            .Apply(items.AsEnumerable()).ToList();
        ascendingSorted[0].Price.Should().Be(5m);
    }
```

If `.Apply(...)` isn't the real method name, check the existing `ValiSort` usage elsewhere in this file for the correct call pattern (Round 1's Task 15 already established it).

- [ ] **Step 2: Write `ValiFlowQuery.WithError(string,string,Severity)` overload test**

Append to `Vali-Flow.Core.Tests/ValiFlowQueryTests.cs` (if Round 1's Task 14 already covers this exact overload, skip — verify first):

```csharp
    [Fact]
    public void WithError_TwoArgSeverityOverload_SetsSeverity()
    {
        var builder = new ValiFlowQuery<Customer>()
            .IsNotNullOrEmpty(c => c.Name)
            .WithError("ERR_NAME", "Name required", Severity.Critical);

        var result = builder.Validate(new Customer(null, null));
        result.Errors.Should().ContainSingle(e => e.ErrorCode == "ERR_NAME" && e.Severity == Severity.Critical);
    }
```

- [ ] **Step 3: Write `ValiFlow<T>` explicit-interface `LessThanOrEqualTo`/`EqualTo` test**

Append to `Vali-Flow.Core.Tests/BaseExpressionTests.cs`:

```csharp
    [Fact]
    public void ValiFlow_ExplicitInterface_LessThanOrEqualTo_And_EqualTo_WorkCorrectly()
    {
        IComparableExpression<ValiFlow<Product>, Product> builder = new ValiFlow<Product>();

        var lteFilter = builder.LessThanOrEqualTo(p => p.Name!, "M").Build().Compile();
        lteFilter(new Product("A", 1m, 1, true, DateTime.Now, new List<string>())).Should().BeTrue();

        IComparableExpression<ValiFlow<Product>, Product> builder2 = new ValiFlow<Product>();
        var eqFilter = builder2.EqualTo(p => p.Name!, "Alice").Build().Compile();
        eqFilter(new Product("Alice", 1m, 1, true, DateTime.Now, new List<string>())).Should().BeTrue();
        eqFilter(new Product("Bob", 1m, 1, true, DateTime.Now, new List<string>())).Should().BeFalse();
    }
```

(Note: this is the same interface pattern as Task 8's tests — if Task 8 already instantiated this exact interface call for `GreaterThan`, this task covers the DIFFERENT methods `LessThanOrEqualTo`/`EqualTo` on `ValiFlow<T>` specifically, per the audit's section 12 which calls out `ValiFlow.cs:120,130` as distinct from `NumericExpression.cs`'s own explicit-interface gaps in section 7 — both are real, separate lines.)

- [ ] **Step 4: Run tests to verify they all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ValiSort_ApplyThenBy|FullyQualifiedName~WithError_TwoArgSeverityOverload|FullyQualifiedName~ValiFlow_ExplicitInterface"`
Expected: PASS (if Steps 1/2 weren't skipped as duplicates; if skipped, their suite file's existing test should already cover it — confirm with the filter above).

- [ ] **Step 5: Commit**

```bash
git add Vali-Flow.Core.Tests/EachItemSortGlobalTests.cs Vali-Flow.Core.Tests/ValiFlowQueryTests.cs Vali-Flow.Core.Tests/BaseExpressionTests.cs
git commit -m "test(core): cover ValiSort/ValiFlowQuery/ValiFlow remaining explicit-interface and overload gaps"
```

---

### Task 12: Coverage — `CollectionExpression.cs` empty-configure guards

**Files:** Modify: `Vali-Flow.Core.Tests/BaseExpressionTests.cs`

**Interfaces:** None.

- [ ] **Step 1: Write the 2 guard tests**

Append to `Vali-Flow.Core.Tests/BaseExpressionTests.cs`:

```csharp
    [Fact]
    public void EachItem_EmptyConfigure_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.EachItem<string>(p => p.Tags, _ => { });
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AnyItem_EmptyConfigure_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.AnyItem<string>(p => p.Tags, _ => { });
        act.Should().Throw<ArgumentException>();
    }
```

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~EachItem_EmptyConfigure|FullyQualifiedName~AnyItem_EmptyConfigure"`
Expected: PASS (2/2).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/BaseExpressionTests.cs
git commit -m "test(core): cover CollectionExpression EachItem/AnyItem empty-configure guards"
```

---

### Task 13: Coverage — `BaseExpression.cs` fork-on-write-after-freeze (7 methods) + remaining guards

**Files:** Modify: `Vali-Flow.Core.Tests/BaseExpressionTests.cs`

**Interfaces:** None.

- [ ] **Step 1: Write ONE parametrized-by-hand test per fork-on-write method**

Append to `Vali-Flow.Core.Tests/BaseExpressionTests.cs`:

```csharp
    [Fact]
    public void AddSubGroup_AfterFreeze_ReturnsForkNotOriginal()
    {
        var original = new ValiFlow<Product>().IsTrue(p => p.IsActive);
        original.IsValid(new Product("A", 1m, 1, true, DateTime.Now, new List<string>())); // freezes

        var forked = original.AddSubGroup(g => g.IsTrue(p => p.IsActive));

        forked.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Or_AfterFreeze_ReturnsForkNotOriginal()
    {
        var original = new ValiFlow<Product>().IsTrue(p => p.IsActive);
        original.IsValid(new Product("A", 1m, 1, true, DateTime.Now, new List<string>()));

        var forked = original.Or();

        forked.Should().NotBeSameAs(original);
    }

    [Fact]
    public void AddIf_BooleanOverload_AfterFreeze_ReturnsForkNotOriginal()
    {
        var original = new ValiFlow<Product>().IsTrue(p => p.IsActive);
        original.IsValid(new Product("A", 1m, 1, true, DateTime.Now, new List<string>()));

        var forked = original.AddIf(true, p => p.IsActive);

        forked.Should().NotBeSameAs(original);
    }

    [Fact]
    public void AddIf_SelectorPredicateOverload_AfterFreeze_ReturnsForkNotOriginal()
    {
        var original = new ValiFlow<Product>().IsTrue(p => p.IsActive);
        original.IsValid(new Product("A", 1m, 1, true, DateTime.Now, new List<string>()));

        var forked = original.AddIf(true, p => p.Quantity, q => q > 0);

        forked.Should().NotBeSameAs(original);
    }

    [Fact]
    public void When_AfterFreeze_ReturnsForkNotOriginal()
    {
        var original = new ValiFlow<Product>().IsTrue(p => p.IsActive);
        original.IsValid(new Product("A", 1m, 1, true, DateTime.Now, new List<string>()));

        var forked = original.When(true, b => b.IsTrue(p => p.IsActive));

        forked.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Unless_AfterFreeze_ReturnsForkNotOriginal()
    {
        var original = new ValiFlow<Product>().IsTrue(p => p.IsActive);
        original.IsValid(new Product("A", 1m, 1, true, DateTime.Now, new List<string>()));

        var forked = original.Unless(false, b => b.IsTrue(p => p.IsActive));

        forked.Should().NotBeSameAs(original);
    }

    [Fact]
    public void ValidateNested_AfterFreeze_ReturnsForkNotOriginal()
    {
        var original = new ValiFlow<Customer>().IsNotNullOrEmpty(c => c.Name);
        original.IsValid(new Customer("A", new Address("X")));

        var forked = original.ValidateNested(c => c.HomeAddress, b => b.IsNotNullOrEmpty(a => a.City));

        forked.Should().NotBeSameAs(original);
    }

    [Fact]
    public void When_EmptyThenAction_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.When(true, _ => { });
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Unless_EmptyUnlessAction_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.Unless(false, _ => { });
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithMessage_EmptyValue_Throws()
    {
        var builder = new ValiFlow<Product>().IsTrue(p => p.IsActive);
        var act = () => builder.WithMessage("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Add_AlwaysFalseConstant_Throws()
    {
        var builder = new ValiFlow<Product>();
        var act = () => builder.Add(_ => false);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsFalse_WithConstantTrueBody_Throws()
    {
        // IsFalse(x => true) builds Expression.Not(Constant(true)) without C# constant-folding it away
        // (unlike a literal `_ => false`, which Roslyn folds to a bare ConstantExpression) — this is the
        // one reachable way to hit BaseExpression.cs's "Not(Constant(bool))" validation branch.
        var builder = new ValiFlow<Product>();
        var act = () => builder.IsFalse(p => true);
        act.Should().Throw<ArgumentException>();
    }
```

`Customer`/`Address` records already exist in this file from Round 1's Task 14 — reuse them, do not redefine. If `IsFalse` isn't a real method on `ValiFlow<T>` (check `Vali-Flow.Core/Classes/General/BooleanExpression.cs` or similar for the actual boolean-literal methods), find whichever method builds a bare `Expression.Not(Constant(bool))` without C# folding it and use that instead — the intent (hit `BaseExpression.cs` line ~836 per the audit) stays the same.

- [ ] **Step 2: Run tests to verify they all pass**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~AfterFreeze_ReturnsFork|FullyQualifiedName~When_EmptyThenAction|FullyQualifiedName~Unless_EmptyUnlessAction|FullyQualifiedName~WithMessage_EmptyValue|FullyQualifiedName~Add_AlwaysFalseConstant|FullyQualifiedName~IsFalse_WithConstantTrueBody"`
Expected: PASS (all).

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/BaseExpressionTests.cs
git commit -m "test(core): cover BaseExpression fork-on-write-after-freeze paths and remaining guards"
```

---

### Task 14: Coverage — `ExpressionExplainer.cs` `Modulo`/default-NodeType branch

**Files:** Modify: `Vali-Flow.Core.Tests/ExpressionExplainerTests.cs`

**Interfaces:** None.

- [ ] **Step 1: Write the test**

Append to `Vali-Flow.Core.Tests/ExpressionExplainerTests.cs`:

```csharp
    [Fact]
    public void Explain_BinaryModulo_ShowsRawNodeTypeFallback()
    {
        Expression<Func<Item, bool>> expr = item => item.Value % 2 == 0;
        var result = Explain(expr);
        result.Should().Contain("Modulo");
    }
```

- [ ] **Step 2: Run tests to verify it passes**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Explain_BinaryModulo"`
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add Vali-Flow.Core.Tests/ExpressionExplainerTests.cs
git commit -m "test(core): cover ExpressionExplainer default NodeType fallback branch (Modulo)"
```

---

### Task 15: Fix — Analyzer `FullName.Contains` false-positive risk + coverage for its matching gaps

**Files:**
- Modify: `Vali-Flow.Core.Analyzers/ValiFlowNonEfMethodAnalyzer.cs`
- Modify: `Vali-Flow.Core.Analyzers.Tests/ValiFlowNonEfMethodAnalyzerTests.cs`

**Interfaces:** None — this is both a confirmed-bug fix (not pure coverage) and its regression test.

- [ ] **Step 1: Write the failing regression test for the false-positive**

Append to `Vali-Flow.Core.Analyzers.Tests/ValiFlowNonEfMethodAnalyzerTests.cs`:

```csharp
    [Fact]
    public async Task SimilarlyNamedType_InSameNamespace_DoesNotTriggerFalsePositive()
    {
        var source = @"
using Vali_Flow.Core.Builder;
namespace Vali_Flow.Core.Builder { public class ValiFlowQueryExtra { public void All() {} } }
public class Entity { public string? Name { get; set; } }
public class Usage
{
    public void Run()
    {
        var q = new Vali_Flow.Core.Builder.ValiFlowQueryExtra();
        q.All();
    }
}";
        var diagnostics = await GetVf001DiagnosticsAsync(source);
        diagnostics.Should().BeEmpty("ValiFlowQueryExtra is not ValiFlowQuery<T> and must not be flagged by a substring match on its name");
    }

    [Fact]
    public async Task DerivedValiFlowQueryType_StillTriggersVF001ViaInheritance()
    {
        var source = @"
using Vali_Flow.Core.Builder;
public class Entity { public string? Name { get; set; } }
public class MyQuery<T> : ValiFlowQuery<T> { }
public class Usage
{
    public void Run()
    {
        var q = new MyQuery<Entity>();
        q.IsEmail(e => e.Name);
    }
}";
        var diagnostics = await GetVf001DiagnosticsAsync(source);
        diagnostics.Should().ContainSingle(d => d.Id == "VF001", "a type derived from ValiFlowQuery<T> must still be flagged via the base-type walk");
    }
```

- [ ] **Step 2: Run tests to verify the first one fails (confirming the bug) and the second passes**

Run: `dotnet test Vali-Flow.Core.Analyzers.Tests/Vali-Flow.Core.Analyzers.Tests.csproj -c Release`
Expected: `SimilarlyNamedType_InSameNamespace_DoesNotTriggerFalsePositive` FAILS (diagnostics not empty — confirms the `FullName.Contains` substring-match bug); `DerivedValiFlowQueryType_StillTriggersVF001ViaInheritance` PASSES already (the base-type walk already works correctly per the audit).

If the first test does NOT fail (i.e. it already passes), the bug may not be reproducible with this exact repro — do not force a fix for a bug you can't reproduce. In that case, skip Step 3, keep both tests (they're valid coverage either way), and note in your report that the suspected false-positive could not be confirmed with this repro.

- [ ] **Step 3: Fix `MatchesValiFlowQuery` to use exact name comparison instead of substring `Contains`**

In `Vali-Flow.Core.Analyzers/ValiFlowNonEfMethodAnalyzer.cs`, find `MatchesValiFlowQuery` (around line 193-203):

```csharp
    private static bool MatchesValiFlowQuery(ITypeSymbol type)
    {
        // Match by short name OR full metadata name (covers generic and non-generic)
        if (type.Name == ValiFlowQueryTypeName)
        {
            return true;
        }

        var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return fullName.Contains(ValiFlowQueryFullName);
    }
```

Replace the `Contains` fallback with an exact-match check against the metadata name (which correctly distinguishes `ValiFlowQuery` from `ValiFlowQueryExtra`):

```csharp
    private static bool MatchesValiFlowQuery(ITypeSymbol type)
    {
        // Exact short-name match handles the common case (including generic instantiations,
        // since ITypeSymbol.Name excludes type arguments).
        return type.Name == ValiFlowQueryTypeName;
    }
```

This also lets you delete the now-unused `ValiFlowQueryFullName` constant if nothing else references it — check with a search before removing.

- [ ] **Step 4: Run tests to verify both pass**

Run: `dotnet test Vali-Flow.Core.Analyzers.Tests/Vali-Flow.Core.Analyzers.Tests.csproj -c Release`
Expected: PASS (6/6 — the 4 from Round 1 plus these 2).

Run: `dotnet build Vali-Flow.Core.sln` to confirm nothing else in the solution relied on the removed `Contains` fallback behavior.

- [ ] **Step 5: Commit**

```bash
git add Vali-Flow.Core.Analyzers/ValiFlowNonEfMethodAnalyzer.cs Vali-Flow.Core.Analyzers.Tests/ValiFlowNonEfMethodAnalyzerTests.cs
git commit -m "fix(analyzers): replace substring FullName.Contains match with exact type-name comparison

Vali_Flow.Core.Builder.ValiFlowQueryExtra (or any type whose fully-qualified
name happens to contain \"Vali_Flow.Core.Builder.ValiFlowQuery\" as a substring)
was incorrectly matched by MatchesValiFlowQuery's Contains() fallback, which
could cause VF001 to fire on unrelated types. The short-name exact-match
check already handles ValiFlowQuery<T> and its generic instantiations
correctly (ITypeSymbol.Name excludes type arguments) — the substring
fallback added a false-positive risk without covering any case the exact
match didn't already handle."
```

- [ ] **Step 6: Write coverage for the remaining early-return branches**

Append to `Vali-Flow.Core.Analyzers.Tests/ValiFlowNonEfMethodAnalyzerTests.cs`:

```csharp
    [Fact]
    public async Task NonMemberAccessInvocation_DoesNotTriggerVF001()
    {
        var source = @"
public class Usage
{
    public void Run() { Foo(); }
    private void Foo() { }
}";
        var diagnostics = await GetVf001DiagnosticsAsync(source);
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task DelegateFieldNamedLikeNonEfMethod_DoesNotTriggerVF001()
    {
        var source = @"
public class Usage
{
    public System.Func<bool> All = () => true;
    public void Run() { All(); }
}";
        var diagnostics = await GetVf001DiagnosticsAsync(source);
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task UnrelatedTypeWithSameMethodName_DoesNotTriggerVF001()
    {
        var source = @"
using System.Collections.Generic;
public class Usage
{
    public void Run()
    {
        var list = new List<int> { 1, 2, 3 };
        list.All(x => x > 0);
    }
}
public static class ListExt
{
    public static bool All(this List<int> list, System.Func<int,bool> predicate) => true;
}";
        var diagnostics = await GetVf001DiagnosticsAsync(source);
        diagnostics.Should().BeEmpty("List<int> is not ValiFlowQuery<T> and the base-type walk must terminate at object without matching");
    }
```

- [ ] **Step 7: Run tests to verify they pass**

Run: `dotnet test Vali-Flow.Core.Analyzers.Tests/Vali-Flow.Core.Analyzers.Tests.csproj -c Release`
Expected: PASS (9/9).

- [ ] **Step 8: Commit**

```bash
git add Vali-Flow.Core.Analyzers.Tests/ValiFlowNonEfMethodAnalyzerTests.cs
git commit -m "test(analyzers): cover early-return branches (non-member-access, non-method-symbol, unrelated types)"
```

---

### Task 16: Coverage — `ForwardingGenerator.cs` generics, defaults, and explicit-implementation branches

**Files:**
- Create: `Vali-Flow.Core.Generator.Tests/ForwardingGeneratorCoverageTests.cs`

**Interfaces:** None — pure test additions using the same `CSharpGeneratorDriver` pattern established in Round 1's Task 3.

- [ ] **Step 1: Write tests exercising the generic-container, generic-method, default-value, and constraint rendering paths**

Create `Vali-Flow.Core.Generator.Tests/ForwardingGeneratorCoverageTests.cs`:

```csharp
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Vali_Flow.Core.Generator;
using Xunit;

namespace Vali_Flow.Core.Generator.Tests;

/// <summary>
/// Round 2: exercises ForwardingGenerator's generic container/method, default-parameter-value,
/// and explicit-interface-implementation-on-conflict code paths — Round 1's Task 3 only covered
/// the diagnostics (VFGEN001/VFGEN002) and a trivial non-generic single-method case.
/// </summary>
public class ForwardingGeneratorCoverageTests
{
    private static (System.Collections.Immutable.ImmutableArray<Diagnostic> Diagnostics, string GeneratedSource) RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var refs = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Vali_Flow.Core.Builder.ValiFlow<object>).Assembly.Location),
        };
        var compilation = CSharpCompilation.Create("TestAssembly", new[] { syntaxTree }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ForwardingGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var generatedTree = outputCompilation.SyntaxTrees.FirstOrDefault(t => t.FilePath.Contains(".Forwarding.g.cs"));
        return (diagnostics, generatedTree?.ToString() ?? "");
    }

    [Fact]
    public void GenericContainerClass_ForwardsMethodsWithContainerTypeParameter()
    {
        var source = @"
using Vali_Flow.Core.Builder;
public interface IHasMethod { void Foo(); }
public partial class Container<T>
{
    [ForwardInterface] private IHasMethod _field = null!;
}";
        var (diagnostics, generated) = RunGenerator(source);
        diagnostics.Where(d => d.Id is "VFGEN001" or "VFGEN002").Should().BeEmpty();
        generated.Should().Contain("Container<T>").And.Contain("Foo");
    }

    [Fact]
    public void GenericMethodOnInterface_ForwardsWithMethodTypeParameter()
    {
        var source = @"
using Vali_Flow.Core.Builder;
public interface IHasGenericMethod { T2 Map<T2>(System.Func<T2> f); }
public partial class Container
{
    [ForwardInterface] private IHasGenericMethod _field = null!;
}";
        var (diagnostics, generated) = RunGenerator(source);
        diagnostics.Where(d => d.Id is "VFGEN001" or "VFGEN002").Should().BeEmpty();
        generated.Should().Contain("Map<T2>");
    }

    [Fact]
    public void MethodWithDefaultParameterValues_RendersDefaultsCorrectly()
    {
        var source = @"
using Vali_Flow.Core.Builder;
public interface IHasDefaults
{
    void Foo(int x = 5, string? s = null, bool b = true);
}
public partial class Container
{
    [ForwardInterface] private IHasDefaults _field = null!;
}";
        var (diagnostics, generated) = RunGenerator(source);
        diagnostics.Where(d => d.Id is "VFGEN001" or "VFGEN002").Should().BeEmpty();
        generated.Should().Contain("= 5").And.Contain("= null").And.Contain("= true");
    }

    [Fact]
    public void GenericMethodWithClassAndStructConstraints_RendersConstraintsCorrectly()
    {
        var source = @"
using Vali_Flow.Core.Builder;
public interface IHasConstraints
{
    void RefOnly<T2>(T2 x) where T2 : class;
    void ValueOnly<T2>(T2 x) where T2 : struct;
    void NewableOnly<T2>() where T2 : new();
}
public partial class Container
{
    [ForwardInterface] private IHasConstraints _field = null!;
}";
        var (diagnostics, generated) = RunGenerator(source);
        diagnostics.Where(d => d.Id is "VFGEN001" or "VFGEN002").Should().BeEmpty();
        generated.Should().Contain("where T2 : class").And.Contain("where T2 : struct").And.Contain("where T2 : new()");
    }

    [Fact]
    public void TwoFieldsWithConflictingSignature_EmitsExplicitImplementationForSecond()
    {
        var source = @"
using Vali_Flow.Core.Builder;
public interface IA { void Foo(); }
public interface IB { void Foo(); }
public partial class Container
{
    [ForwardInterface] private IA _a = null!;
    [ForwardInterface] private IB _b = null!;
}";
        var (diagnostics, generated) = RunGenerator(source);
        diagnostics.Where(d => d.Id is "VFGEN001" or "VFGEN002").Should().BeEmpty();
        // The second conflicting member must be emitted as an explicit interface implementation
        // (e.g. "void IB.Foo()") rather than a second public "void Foo()" (which would not compile).
        generated.Should().Contain("IB.Foo");
    }
}
```

If `ValiFlow<object>` isn't accessible this way, or the generated source's file path/content check doesn't match (e.g. the generated file is named differently), check Round 1's `ForwardingGeneratorDiagnosticsTests.cs` for the exact `CSharpGeneratorDriver` usage pattern already established and align this file to it.

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test Vali-Flow.Core.Generator.Tests/Vali-Flow.Core.Generator.Tests.csproj -c Release`
Expected: PASS (8/8 — the 3 from Round 1 plus these 5).

- [ ] **Step 3: Run the full solution build to confirm the real `[ForwardInterface]` usages in `ValiFlow.cs`/`ValiFlowQuery.cs` are unaffected**

Run: `dotnet build Vali-Flow.Core.sln`
Expected: Build succeeded, 0 errors, no new VFGEN001/VFGEN002 diagnostics.

- [ ] **Step 4: Commit**

```bash
git add Vali-Flow.Core.Generator.Tests/ForwardingGeneratorCoverageTests.cs
git commit -m "test(generator): cover generic container/method, default values, and explicit-implementation conflict paths"
```

---

### Task 17: Final verification gate — Round 2

**Files:** None modified — verification only.

- [ ] **Step 1: Full solution build**

Run: `dotnet build Vali-Flow.Core.sln`
Expected: 0 errors.

- [ ] **Step 2: Full test suite with coverage, all 3 projects**

Run: `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --collect:"XPlat Code Coverage" --results-directory ./TestResultsR2`
Run: `dotnet test Vali-Flow.Core.Analyzers.Tests/Vali-Flow.Core.Analyzers.Tests.csproj -c Release --collect:"XPlat Code Coverage" --results-directory ./TestResultsAnalyzersR2`
Run: `dotnet test Vali-Flow.Core.Generator.Tests/Vali-Flow.Core.Generator.Tests.csproj -c Release --collect:"XPlat Code Coverage" --results-directory ./TestResultsGeneratorR2`
Expected: 100% pass across all three.

- [ ] **Step 3: Compare coverage numbers before/after Round 2**

Baseline (post-Round-1, pre-Round-2): core line-rate 0.832, branch-rate ~0.68 (from `TestResults2/`); Analyzer class 91.3%/65%; Generator class 76.2%/51.1%.
Open the new `coverage.cobertura.xml` files and report the new line-rate/branch-rate for each of the 3 projects. Report whether core crossed 90%/95%+ line-rate, and whether the explicitly-out-of-scope lines (listed in Global Constraints) account for the remainder of any gap below 100%.

- [ ] **Step 4: Run the full suite 3x to confirm no flakiness was reintroduced**

Run (3 times): `dotnet test Vali-Flow.Core.Tests/Vali-Flow.Core.Tests.csproj -c Release --no-build`
Expected: identical pass count all 3 times (the `[Collection("ValiFlowGlobal")]` fix from this session should hold).

No commit for this task — report the final numbers to the user.
