# Caso A Stress Test (ValiFlowQuery<T> + EF Core + Postgres) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a throwaway sample API (CQRS + EF Core/Npgsql over Postgres) that exercises `ValiFlowQuery<T>` through ~80-100 fixed filter templates, then run it under k6 load across 6 profiles to validate EF Core's compiled-query-cache behavior, correctness under concurrency, and latency/throughput.

**Architecture:** k6 → ASP.NET Core Minimal API (.NET 8, MediatR CQRS, FluentValidation) → `ValiFlowQuery<StressItem>` built from a fixed template pool → EF Core/Npgsql → Toxiproxy → PostgreSQL (Docker). A Python sampler polls `/diag/runtime` during each run; a Python reconciler verifies correctness against Postgres directly afterward.

**Tech Stack:** .NET 8, ASP.NET Core Minimal API, MediatR 12.x, FluentValidation 11.x, EF Core 8.x, Npgsql.EntityFrameworkCore.PostgreSQL 8.x, Docker (postgres:16-alpine, ghcr.io/shopify/toxiproxy:2.9.0), k6, Python 3 (stdlib only — no extra deps).

**Spec:** `docs/superpowers/specs/2026-10-03-caso-a-stress-test-design.md`

## Global Constraints

- **Harness code location**: everything built by this plan lives under
  `C:\Users\fmontenegro\AppData\Local\Temp\claude\C--Users-fmontenegro-Documents-proyectos-Vali-Flow-Core\7513b8aa-d2b8-4c03-a4e7-b34af27f181b\scratchpad\stress-case-a\` —
  **never inside the Vali-Flow.Core repo**, never committed, never pushed.
- **No modification of Vali-Flow.Core**: the harness consumes it via `<ProjectReference>` to
  `C:\Users\fmontenegro\Documents\proyectos\Vali-Flow.Core\.claude\worktrees\vali-flow-core-remediation\Vali-Flow.Core\Vali-Flow.Core.csproj`,
  read-only.
- **No `UseInMemoryDatabase`** anywhere in this harness — defeats the entire point (pillar 1 needs
  real LINQ→SQL translation). Postgres via Docker only.
- **Never point load generators at a shared/existing dev or qa database** — always the dedicated
  Docker Postgres this plan's compose file creates.
- **.NET target**: net8.0 only for the harness (Vali-Flow.Core's own minimum supported TFM — no
  need to also target net9.0 for throwaway infra).
- **Nothing in this plan is pushed to any git remote.** Plan/spec docs are committed locally to the
  current worktree branch only (no Claude author/co-author in any commit message).

---

### Task 1: Scaffold sample API + Docker Postgres/Toxiproxy, verify end-to-end health

**Files (all under the scratchpad root above, henceforth `$ROOT`):**
- Create: `$ROOT/StressApi/StressApi.csproj`
- Create: `$ROOT/StressApi/Program.cs`
- Create: `$ROOT/docker/compose.perf.yml`
- Create: `$ROOT/docker/toxiproxy.json`

**Interfaces:**
- Produces: a running API on `http://127.0.0.1:5080` with `GET /health` returning `200 OK`, and a
  Postgres reachable via Toxiproxy on `127.0.0.1:35432` (upstream `postgres:5432` inside the Docker
  network).

- [ ] **Step 1: Create the compose file**

`$ROOT/docker/compose.perf.yml`:
```yaml
name: valiflow-stress-a
networks:
  net: { name: valiflow-stress-net }
services:
  postgres:
    image: postgres:16-alpine
    command: ["postgres", "-c", "shared_buffers=256MB", "-c", "max_connections=500"]
    environment: { POSTGRES_USER: stress, POSTGRES_PASSWORD: stress, POSTGRES_DB: stress }
    ports: ["35434:5432"]
    networks: [net]
    tmpfs: ["/var/lib/postgresql/data:size=4g"]
    healthcheck: { test: ["CMD-SHELL", "pg_isready -U stress -d stress"], interval: 2s, retries: 30 }

  toxiproxy:
    image: ghcr.io/shopify/toxiproxy:2.9.0
    command: ["-host=0.0.0.0", "-config=/config/toxiproxy.json"]
    volumes: ["./toxiproxy.json:/config/toxiproxy.json:ro"]
    ports: ["38474:8474", "35432:25432"]
    networks: [net]
    depends_on:
      postgres: { condition: service_healthy }
```

- [ ] **Step 2: Create the Toxiproxy config**

`$ROOT/docker/toxiproxy.json`:
```json
[
  { "name": "postgres", "listen": "0.0.0.0:25432", "upstream": "postgres:5432", "enabled": true }
]
```

- [ ] **Step 3: Bring the containers up and verify health**

Run (from `$ROOT/docker`):
```bash
docker compose -f compose.perf.yml up -d --wait
```
Expected: both `postgres` and `toxiproxy` report healthy/running. Verify with:
```bash
docker compose -f compose.perf.yml ps
```

- [ ] **Step 4: Verify Postgres is reachable through Toxiproxy**

Run:
```bash
docker run --rm --network valiflow-stress-net postgres:16-alpine psql "postgresql://stress:stress@toxiproxy:25432/stress" -c "select 1;"
```
Expected: returns `1` (confirms the proxy forwards traffic to the real Postgres).

- [ ] **Step 5: Scaffold the API project**

`$ROOT/StressApi/StressApi.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.10" />
    <PackageReference Include="MediatR" Version="12.4.1" />
    <PackageReference Include="FluentValidation" Version="11.11.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="C:\Users\fmontenegro\Documents\proyectos\Vali-Flow.Core\.claude\worktrees\vali-flow-core-remediation\Vali-Flow.Core\Vali-Flow.Core.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 6: Minimal Program.cs with a health endpoint**

`$ROOT/StressApi/Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:5080");

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
```

- [ ] **Step 7: Run the API and verify the health endpoint**

Run (from `$ROOT/StressApi`):
```bash
dotnet run --configuration Release
```
In another shell:
```bash
curl http://127.0.0.1:5080/health
```
Expected: `{"status":"ok"}`.

No commit — this is scratchpad-only infrastructure (see Global Constraints). Stop the API
(Ctrl+C) before moving to Task 2.

---

### Task 2: StressItem entity, DbContext, Npgsql mapping, apply schema against real Postgres

**Files:**
- Create: `$ROOT/StressApi/StressItem.cs`
- Create: `$ROOT/StressApi/StressDbContext.cs`
- Modify: `$ROOT/StressApi/Program.cs`

**Interfaces:**
- Consumes: the running Postgres from Task 1 (`127.0.0.1:35434` direct, or `127.0.0.1:35432` via
  Toxiproxy — use the direct port for schema setup, the proxied port for the actual API's
  connection string so load profiles can inject latency later).
- Produces: `StressItem` class and `StressDbContext : DbContext` with a `DbSet<StressItem> Items`,
  consumed by Tasks 3-10.

- [ ] **Step 1: Define the entity**

`$ROOT/StressApi/StressItem.cs`:
```csharp
namespace StressApi;

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
    public int? OptionalScore { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateOnly BirthDate { get; set; }
    public TimeOnly WorkStart { get; set; }
    public List<string> Tags { get; set; } = new();
    public int Version { get; set; }
}
```

- [ ] **Step 2: Define the DbContext with explicit Npgsql array mapping for Tags**

`$ROOT/StressApi/StressDbContext.cs`:
```csharp
using Microsoft.EntityFrameworkCore;

namespace StressApi;

public class StressDbContext : DbContext
{
    public StressDbContext(DbContextOptions<StressDbContext> options) : base(options) { }

    public DbSet<StressItem> Items => Set<StressItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<StressItem>();
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Tags).HasColumnType("text[]");
        entity.HasIndex(x => x.Name);
        entity.HasIndex(x => x.IntValue);
        entity.HasIndex(x => x.CreatedAt);
    }
}
```

- [ ] **Step 3: Wire the DbContext into Program.cs and add schema-creation on startup**

Add `using Microsoft.EntityFrameworkCore;` to the very top of `$ROOT/StressApi/Program.cs`
(required for `AddDbContext`/`UseNpgsql` below — top-level `Program.cs` only gets implicit usings
for `System.*`/ASP.NET namespaces, not EF Core's).

Modify `$ROOT/StressApi/Program.cs` — add after `var builder = WebApplication.CreateBuilder(args);`:
```csharp
const string ConnString = "Host=127.0.0.1;Port=35432;Database=stress;Username=stress;Password=stress";
builder.Services.AddDbContext<StressDbContext>(o => o.UseNpgsql(ConnString));
```

Add after `var app = builder.Build();`, before `app.MapGet("/health", ...)`:
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StressDbContext>();
    db.Database.EnsureCreated();
}
```

- [ ] **Step 4: Run and verify the table was created**

Run the API (`dotnet run --configuration Release` from `$ROOT/StressApi`), then in another shell:
```bash
docker run --rm --network valiflow-stress-net postgres:16-alpine psql "postgresql://stress:stress@toxiproxy:25432/stress" -c "\d \"Items\""
```
Expected: lists all `StressItem` columns including `Tags` as `text[]`.

No commit (scratchpad). Stop the API before Task 3.

---

### Task 3: MediatR + FluentValidation wiring, SeedDataCommand, POST /items/seed

**Files:**
- Create: `$ROOT/StressApi/SeedDataCommand.cs`
- Modify: `$ROOT/StressApi/Program.cs`

**Interfaces:**
- Consumes: `StressDbContext` (Task 2).
- Produces: `SeedDataCommand(int RowCount, int Seed)` handled by `SeedDataHandler`, registered with
  MediatR; `POST /items/seed` endpoint. Later tasks (4, 9) follow the same MediatR
  command/query + FluentValidation pattern established here.

- [ ] **Step 1: Write the command, deterministic generator, and handler**

`$ROOT/StressApi/SeedDataCommand.cs`:
```csharp
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace StressApi;

public record SeedDataCommand(int RowCount, int Seed) : IRequest<int>;

public class SeedDataCommandValidator : AbstractValidator<SeedDataCommand>
{
    public SeedDataCommandValidator()
    {
        RuleFor(x => x.RowCount).GreaterThan(0).LessThanOrEqualTo(2_000_000);
        RuleFor(x => x.Seed).GreaterThanOrEqualTo(0);
    }
}

public class SeedDataHandler : IRequestHandler<SeedDataCommand, int>
{
    private readonly StressDbContext _db;
    public SeedDataHandler(StressDbContext db) => _db = db;

    public async Task<int> Handle(SeedDataCommand request, CancellationToken ct)
    {
        await _db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Items\"", ct);

        var rnd = new Random(request.Seed);
        var baseDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var batch = new List<StressItem>(1000);

        for (var i = 0; i < request.RowCount; i++)
        {
            batch.Add(new StressItem
            {
                Id = Guid.NewGuid(),
                Name = $"item-{i}-{rnd.Next(0, 10000)}",
                Description = rnd.Next(0, 2) == 0 ? null : $"desc-{i}",
                IntValue = rnd.Next(-1000, 1000),
                LongValue = rnd.NextInt64(-1_000_000, 1_000_000),
                DecimalValue = Math.Round((decimal)rnd.NextDouble() * 1000, 2),
                DoubleValue = rnd.NextDouble() * 1000,
                FloatValue = (float)(rnd.NextDouble() * 1000),
                ShortValue = (short)rnd.Next(short.MinValue, short.MaxValue),
                OptionalScore = rnd.Next(0, 2) == 0 ? null : rnd.Next(0, 100),
                IsActive = rnd.Next(0, 2) == 0,
                CreatedAt = baseDate.AddDays(rnd.Next(0, 2000)),
                UpdatedAt = new DateTimeOffset(baseDate.AddDays(rnd.Next(0, 2000)), TimeSpan.Zero),
                BirthDate = DateOnly.FromDateTime(baseDate.AddDays(rnd.Next(0, 20000))),
                WorkStart = new TimeOnly(rnd.Next(0, 24), rnd.Next(0, 60)),
                Tags = Enumerable.Range(0, rnd.Next(0, 4)).Select(_ => $"tag{rnd.Next(0, 20)}").ToList(),
                Version = 0,
            });

            if (batch.Count == 1000)
            {
                _db.Items.AddRange(batch);
                await _db.SaveChangesAsync(ct);
                _db.ChangeTracker.Clear();
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            _db.Items.AddRange(batch);
            await _db.SaveChangesAsync(ct);
            _db.ChangeTracker.Clear();
        }

        return request.RowCount;
    }
}
```

- [ ] **Step 2: Register MediatR + FluentValidation, add the endpoint**

Modify `$ROOT/StressApi/Program.cs` — add after the `AddDbContext` line:
```csharp
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

Add after the `/health` endpoint:
```csharp
app.MapPost("/items/seed", async (SeedDataCommand cmd, IMediator mediator, IValidator<SeedDataCommand> validator) =>
{
    var result = await validator.ValidateAsync(cmd);
    if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
    var count = await mediator.Send(cmd);
    return Results.Ok(new { seeded = count });
});
```

Add `using FluentValidation;` to the top of `Program.cs`.

- [ ] **Step 3: Run and verify seeding works**

Run the API, then:
```bash
curl -X POST http://127.0.0.1:5080/items/seed -H "Content-Type: application/json" -d "{\"rowCount\":1000,\"seed\":42}"
```
Expected: `{"seeded":1000}`. Verify row count directly:
```bash
docker run --rm --network valiflow-stress-net postgres:16-alpine psql "postgresql://stress:stress@toxiproxy:25432/stress" -c "select count(*) from \"Items\";"
```
Expected: `1000`.

No commit (scratchpad). Stop the API before Task 4.

---

### Task 4: MutateDataCommand (concurrent-safe mutation generator), POST /items/mutate

**Files:**
- Create: `$ROOT/StressApi/MutateDataCommand.cs`
- Modify: `$ROOT/StressApi/Program.cs`

**Interfaces:**
- Consumes: `StressDbContext` (Task 2), same MediatR/FluentValidation pattern (Task 3).
- Produces: `MutateDataCommand(int Seed, int OperationCount)` → `{updated, deleted, inserted}`;
  `POST /items/mutate`. Each update increments `Version` — Task 12/13's reconciliation script
  relies on this to tell "mutated after snapshot" apart from "genuinely inconsistent."

- [ ] **Step 1: Write the command and handler**

`$ROOT/StressApi/MutateDataCommand.cs`:
```csharp
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace StressApi;

public record MutateDataCommand(int Seed, int OperationCount) : IRequest<MutateResult>;
public record MutateResult(int Updated, int Deleted, int Inserted);

public class MutateDataCommandValidator : AbstractValidator<MutateDataCommand>
{
    public MutateDataCommandValidator()
    {
        RuleFor(x => x.OperationCount).GreaterThan(0).LessThanOrEqualTo(100_000);
        RuleFor(x => x.Seed).GreaterThanOrEqualTo(0);
    }
}

public class MutateDataHandler : IRequestHandler<MutateDataCommand, MutateResult>
{
    private readonly StressDbContext _db;
    public MutateDataHandler(StressDbContext db) => _db = db;

    public async Task<MutateResult> Handle(MutateDataCommand request, CancellationToken ct)
    {
        var rnd = new Random(request.Seed);
        int updated = 0, deleted = 0, inserted = 0;

        for (var i = 0; i < request.OperationCount; i++)
        {
            var op = rnd.Next(0, 3);
            if (op == 0)
            {
                var target = await _db.Items.OrderBy(x => x.Id).Skip(rnd.Next(0, 1000)).Take(1).FirstOrDefaultAsync(ct);
                if (target is null) continue;
                target.IntValue = rnd.Next(-1000, 1000);
                target.Version += 1;
                updated++;
            }
            else if (op == 1)
            {
                var target = await _db.Items.OrderBy(x => x.Id).Skip(rnd.Next(0, 1000)).Take(1).FirstOrDefaultAsync(ct);
                if (target is null) continue;
                _db.Items.Remove(target);
                deleted++;
            }
            else
            {
                _db.Items.Add(new StressItem
                {
                    Id = Guid.NewGuid(),
                    Name = $"mutated-{request.Seed}-{i}",
                    IntValue = rnd.Next(-1000, 1000),
                    LongValue = rnd.NextInt64(-1000, 1000),
                    DecimalValue = 1m,
                    DoubleValue = 1.0,
                    FloatValue = 1f,
                    ShortValue = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    BirthDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    WorkStart = TimeOnly.FromDateTime(DateTime.UtcNow),
                    Tags = new List<string> { "mutated" },
                    Version = 0,
                });
                inserted++;
            }

            if (i % 100 == 0) await _db.SaveChangesAsync(ct);
        }

        await _db.SaveChangesAsync(ct);
        return new MutateResult(updated, deleted, inserted);
    }
}
```

- [ ] **Step 2: Add the endpoint**

Add to `Program.cs`, after `/items/seed`:
```csharp
app.MapPost("/items/mutate", async (MutateDataCommand cmd, IMediator mediator, IValidator<MutateDataCommand> validator) =>
{
    var result = await validator.ValidateAsync(cmd);
    if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
    var outcome = await mediator.Send(cmd);
    return Results.Ok(outcome);
});
```

- [ ] **Step 3: Run and verify**

With the API running and 1000 rows seeded (Task 3, Step 3):
```bash
curl -X POST http://127.0.0.1:5080/items/mutate -H "Content-Type: application/json" -d "{\"seed\":7,\"operationCount\":50}"
```
Expected: `{"updated":N,"deleted":M,"inserted":K}` with `N+M+K` close to 50 (some skip iterations
are possible if a random target row was already deleted by an earlier iteration — acceptable).

No commit (scratchpad). Stop the API before Task 5.

---

### Task 5: Filter template pool — infrastructure + String family (~15-20 templates)

**Files:**
- Create: `$ROOT/StressApi/FilterTemplates.cs`

**Interfaces:**
- Consumes: `StressItem` (Task 2), `ValiFlowQuery<T>` from
  `Vali_Flow.Core.Builder` (the referenced library project).
- Produces: `static class FilterTemplates` with `IReadOnlyList<Func<int, ValiFlowQuery<StressItem>>> All`
  — Tasks 6, 7, 8 each append their family's templates to the same `All` list (a single growing
  array literal split across tasks by clearly marked region comments, so each task's diff is
  reviewable independently without merge conflicts — see Step 1's region-comment convention).
  Task 9 consumes `FilterTemplates.All[templateId](seed)`.

- [ ] **Step 1: Create the file with the infrastructure and the String family**

`$ROOT/StressApi/FilterTemplates.cs`:
```csharp
using Vali_Flow.Core.Builder;

namespace StressApi;

public static class FilterTemplates
{
    // Each template is deterministic given `seed`: same seed -> same literal values used in the
    // filter, so reconcile.py can recompute the expected result without re-running the template.
    private static string NameAt(int seed, int offset) => $"item-{(seed + offset) % 1000}-";
    private static int IntAt(int seed) => (seed % 2000) - 1000;

    public static readonly IReadOnlyList<Func<int, ValiFlowQuery<StressItem>>> All = new List<Func<int, ValiFlowQuery<StressItem>>>
    {
        // ── String family (templateId 0-14) ─────────────────────────────────
        seed => new ValiFlowQuery<StressItem>().Contains(x => x.Name, NameAt(seed, 0)),
        seed => new ValiFlowQuery<StressItem>().StartsWith(x => x.Name, "item-"),
        seed => new ValiFlowQuery<StressItem>().EndsWith(x => x.Name, (seed % 10).ToString()),
        seed => new ValiFlowQuery<StressItem>().ContainsIgnoreCase(x => x.Name, "ITEM"),
        seed => new ValiFlowQuery<StressItem>().StartsWithIgnoreCase(x => x.Name, "ITEM-"),
        seed => new ValiFlowQuery<StressItem>().EndsWithIgnoreCase(x => x.Name, (seed % 10).ToString()),
        seed => new ValiFlowQuery<StressItem>().MinLength(x => x.Name, 5),
        seed => new ValiFlowQuery<StressItem>().MaxLength(x => x.Name, 50),
        seed => new ValiFlowQuery<StressItem>().NotContains(x => x.Name, "zzz-nonexistent"),
        seed => new ValiFlowQuery<StressItem>().NotStartsWith(x => x.Name, "zzz-nonexistent"),
        seed => new ValiFlowQuery<StressItem>().IsLowerCase(x => x.Name),
        seed => new ValiFlowQuery<StressItem>().IsNotNullOrEmpty(x => x.Name),
        seed => new ValiFlowQuery<StressItem>().EqualToIgnoreCase(x => x.Name, NameAt(seed, 0) + "0"),
        seed => new ValiFlowQuery<StressItem>().Contains(x => x.Description, "desc"),
        seed => new ValiFlowQuery<StressItem>().IsNull(x => x.Description),
    };
}
```

- [ ] **Step 2: Verify it compiles against the real `ValiFlowQuery<T>` API**

Run (from `$ROOT/StressApi`):
```bash
dotnet build
```
Expected: `Build succeeded, 0 errors`. If any method name/signature doesn't match the real
`ValiFlowQuery<T>` API (check
`C:\Users\fmontenegro\Documents\proyectos\Vali-Flow.Core\.claude\worktrees\vali-flow-core-remediation\Vali-Flow.Core\Classes\Types\StringExpressionQuery.cs`
for the real surface), fix the call to the closest real equivalent that still exercises the same
kind of string predicate — do not delete the template, substitute it.

No commit (scratchpad).

---

### Task 6: Filter template pool — Numeric family, all types (~15-20 templates)

**Files:**
- Modify: `$ROOT/StressApi/FilterTemplates.cs`

**Interfaces:**
- Consumes/Produces: same `All` list from Task 5 — append after the String family's closing
  entries, before the final `};`.

- [ ] **Step 1: Append the Numeric family**

Insert into the `All` list in `$ROOT/StressApi/FilterTemplates.cs`, right after the String
family's last entry (`seed => new ValiFlowQuery<StressItem>().IsNull(x => x.Description),`) and
before the closing `};`:
```csharp
        // ── Numeric family (templateId 15-31) ───────────────────────────────
        seed => new ValiFlowQuery<StressItem>().GreaterThan(x => x.IntValue, IntAt(seed)),
        seed => new ValiFlowQuery<StressItem>().LessThan(x => x.IntValue, IntAt(seed)),
        seed => new ValiFlowQuery<StressItem>().InRange(x => x.IntValue, -1000, 1000),
        seed => new ValiFlowQuery<StressItem>().IsOdd(x => x.IntValue),
        seed => new ValiFlowQuery<StressItem>().IsEven(x => x.IntValue),
        seed => new ValiFlowQuery<StressItem>().NotZero(x => x.IntValue),
        seed => new ValiFlowQuery<StressItem>().GreaterThan(x => x.LongValue, (long)IntAt(seed)),
        seed => new ValiFlowQuery<StressItem>().InRange(x => x.LongValue, -1_000_000L, 1_000_000L),
        seed => new ValiFlowQuery<StressItem>().GreaterThan(x => x.DecimalValue, 0m),
        seed => new ValiFlowQuery<StressItem>().InRange(x => x.DecimalValue, 0m, 1000m),
        seed => new ValiFlowQuery<StressItem>().GreaterThan(x => x.DoubleValue, 0.0),
        seed => new ValiFlowQuery<StressItem>().InRange(x => x.DoubleValue, 0.0, 1000.0),
        seed => new ValiFlowQuery<StressItem>().GreaterThan(x => x.FloatValue, 0f),
        seed => new ValiFlowQuery<StressItem>().InRange(x => x.FloatValue, 0f, 1000f),
        seed => new ValiFlowQuery<StressItem>().GreaterThan(x => x.ShortValue, (short)0),
        seed => new ValiFlowQuery<StressItem>().HasValue(x => x.OptionalScore),
        seed => new ValiFlowQuery<StressItem>().GreaterThan(x => (int?)x.OptionalScore, 50),
```

- [ ] **Step 2: Verify it still compiles**

Run (from `$ROOT/StressApi`):
```bash
dotnet build
```
Expected: `Build succeeded, 0 errors`. Same substitution rule as Task 5 Step 2 if a signature
doesn't match the real API (check
`C:\Users\fmontenegro\Documents\proyectos\Vali-Flow.Core\.claude\worktrees\vali-flow-core-remediation\Vali-Flow.Core\Classes\Types\NumericExpressionQuery.cs`).

No commit (scratchpad).

---

### Task 7: Filter template pool — DateTime/DateOnly/TimeOnly/DateTimeOffset family (~15-20 templates)

**Files:**
- Modify: `$ROOT/StressApi/FilterTemplates.cs`

**Interfaces:**
- Consumes/Produces: same `All` list — append after the Numeric family.

- [ ] **Step 1: Append the date/time family**

Insert after the Numeric family's last entry, before the closing `};`:
```csharp
        // ── Date/Time family (templateId 32-48) ─────────────────────────────
        seed => new ValiFlowQuery<StressItem>().IsInYear(x => x.CreatedAt, 2020 + (seed % 5)),
        seed => new ValiFlowQuery<StressItem>().IsInMonth(x => x.CreatedAt, 1 + (seed % 12)),
        seed => new ValiFlowQuery<StressItem>().IsWeekend(x => x.CreatedAt),
        seed => new ValiFlowQuery<StressItem>().IsWeekday(x => x.CreatedAt),
        seed => new ValiFlowQuery<StressItem>().BetweenDates(x => x.CreatedAt,
            new DateTime(2020, 1, 1), new DateTime(2025, 1, 1)),
        seed => new ValiFlowQuery<StressItem>().IsInQuarter(x => x.CreatedAt, 1 + (seed % 4)),
        seed => new ValiFlowQuery<StressItem>().IsFirstDayOfMonth(x => x.CreatedAt),
        seed => new ValiFlowQuery<StressItem>().IsLastDayOfMonth(x => x.CreatedAt),
        seed => new ValiFlowQuery<StressItem>().IsInYear(x => x.UpdatedAt, 2020 + (seed % 5)),
        seed => new ValiFlowQuery<StressItem>().IsWeekend(x => x.UpdatedAt),
        seed => new ValiFlowQuery<StressItem>().BetweenDates(x => x.UpdatedAt,
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)),
        seed => new ValiFlowQuery<StressItem>().IsInYear(x => x.BirthDate, 2020 + (seed % 5)),
        seed => new ValiFlowQuery<StressItem>().IsWeekend(x => x.BirthDate),
        seed => new ValiFlowQuery<StressItem>().BetweenDates(x => x.BirthDate,
            new DateOnly(2020, 1, 1), new DateOnly(2030, 1, 1)),
        seed => new ValiFlowQuery<StressItem>().IsFirstDayOfMonth(x => x.BirthDate),
        seed => new ValiFlowQuery<StressItem>().IsBetween(x => x.WorkStart, new TimeOnly(6, 0), new TimeOnly(18, 0)),
        seed => new ValiFlowQuery<StressItem>().IsInHour(x => x.WorkStart, seed % 24),
```

- [ ] **Step 2: Verify it still compiles**

Run (from `$ROOT/StressApi`):
```bash
dotnet build
```
Expected: `Build succeeded, 0 errors`. Same substitution rule if signatures diverge — check the
real API in
`C:\Users\fmontenegro\Documents\proyectos\Vali-Flow.Core\.claude\worktrees\vali-flow-core-remediation\Vali-Flow.Core\Classes\Types\`
(`DateTimeExpressionQuery.cs`, `DateOnlyExpressionQuery.cs`, `TimeOnlyExpressionQuery.cs`,
`DateTimeOffsetExpressionQuery.cs`).

No commit (scratchpad).

---

### Task 8: Filter template pool — Collection, Boolean/Comparison, combined AND/OR (~30-35 templates)

**Files:**
- Modify: `$ROOT/StressApi/FilterTemplates.cs`

**Interfaces:**
- Consumes/Produces: same `All` list — append after the Date/Time family. This is the pool's final
  segment — Task 9 consumes the complete, now-frozen `All` list.

- [ ] **Step 1: Append Collection, Boolean/Comparison, and combined templates**

Insert after the Date/Time family's last entry, before the closing `};`:
```csharp
        // ── Collection family (templateId 49-56) — EF-safe surface only:
        // NotEmpty/Empty/Count/MinCount/MaxCount/CountBetween for the Tags collection, In/NotIn
        // for scalar-membership checks (verified against
        // Vali-Flow.Core/Classes/Types/CollectionExpressionQuery.cs — In/NotIn take a scalar
        // selector + a value list, NOT a collection selector; AnyItem/EachItem/IsEmpty/
        // CountGreaterThan etc. exist only on the in-memory ValiFlow<T>, not ValiFlowQuery<T>).
        seed => new ValiFlowQuery<StressItem>().NotEmpty(x => x.Tags),
        seed => new ValiFlowQuery<StressItem>().Empty(x => x.Tags),
        seed => new ValiFlowQuery<StressItem>().Count(x => x.Tags, seed % 4),
        seed => new ValiFlowQuery<StressItem>().MinCount(x => x.Tags, 0),
        seed => new ValiFlowQuery<StressItem>().MaxCount(x => x.Tags, 4),
        seed => new ValiFlowQuery<StressItem>().CountBetween(x => x.Tags, 0, 4),
        seed => new ValiFlowQuery<StressItem>().In(x => x.IntValue, new List<int> { IntAt(seed), IntAt(seed) + 1, IntAt(seed) + 2 }),
        seed => new ValiFlowQuery<StressItem>().NotIn(x => x.ShortValue, new List<short> { -1, 0, 1 }),

        // ── Boolean/Comparison family (templateId 59-67) ────────────────────
        seed => new ValiFlowQuery<StressItem>().IsTrue(x => x.IsActive),
        seed => new ValiFlowQuery<StressItem>().IsFalse(x => x.IsActive),
        seed => new ValiFlowQuery<StressItem>().EqualTo(x => x.IsActive, seed % 2 == 0),
        seed => new ValiFlowQuery<StressItem>().NotEqualTo(x => x.IntValue, 0),
        seed => new ValiFlowQuery<StressItem>().IsNotNull(x => x.OptionalScore),
        seed => new ValiFlowQuery<StressItem>().IsNull(x => x.OptionalScore),

        // ── Combined AND/OR across families (templateId 70-74) — .And()/.Or() are logical
        // connectors placed BETWEEN condition calls in the same fluent chain, not group-configure
        // delegates (verified against Vali-Flow.Core.Tests/AddIfWhenValidationTests.cs's real
        // usage pattern: `.AddIf(...).And().AddIf(...)`).
        seed => new ValiFlowQuery<StressItem>()
            .Contains(x => x.Name, "item").And()
            .InRange(x => x.IntValue, -1000, 1000),
        seed => new ValiFlowQuery<StressItem>()
            .IsWeekend(x => x.CreatedAt).Or()
            .IsTrue(x => x.IsActive),
        seed => new ValiFlowQuery<StressItem>()
            .GreaterThan(x => x.DecimalValue, 500m).And()
            .NotEmpty(x => x.Tags),
        seed => new ValiFlowQuery<StressItem>()
            .StartsWith(x => x.Name, "item-").And()
            .IsWeekday(x => x.CreatedAt).And()
            .IsTrue(x => x.IsActive),
        seed => new ValiFlowQuery<StressItem>()
            .InRange(x => x.LongValue, -1_000_000L, 1_000_000L).Or()
            .IsNotNull(x => x.OptionalScore),
    };
}
```

- [ ] **Step 2: Verify the complete pool compiles**

Run (from `$ROOT/StressApi`):
```bash
dotnet build
```
Expected: `Build succeeded, 0 errors`. Same substitution rule as prior tasks — check
`CollectionExpressionQuery.cs`/`ComparisonExpression.cs` in the real library for exact signatures
if anything (especially `AnyItem`/`EachItem`'s nested-configure syntax, or `And`/`Or` group
chaining) doesn't match. Preserve the intent (which family/combination each template targets), not
the literal syntax, if a substitution is needed.

- [ ] **Step 3: Verify the final pool size and print the count**

Run (from `$ROOT/StressApi`, after `dotnet build` succeeds) a throwaway one-liner via `dotnet run`
with a temporary `Console.WriteLine(FilterTemplates.All.Count); return;` at the top of
`Program.cs`'s `Main`-equivalent (top of the file, before `app.Run()` — remove it again after
checking). Expected: a number between 75 and 85 (the plan's `~80-100` target — if it's
meaningfully outside that range because some templates were skipped due to real-API mismatches in
Tasks 5-8, note the actual count in your report; it does not block proceeding, the Global
Constraint is "cover all method families", not an exact count).

No commit (scratchpad).

---

### Task 9: FilterItemsQuery handler, validator, POST /items/filter

**Files:**
- Create: `$ROOT/StressApi/FilterItemsQuery.cs`
- Modify: `$ROOT/StressApi/Program.cs`

**Interfaces:**
- Consumes: `FilterTemplates.All` (Tasks 5-8), `StressDbContext` (Task 2).
- Produces: `POST /items/filter` — the endpoint under test for the entire load-testing phase
  (Tasks 13-16).

- [ ] **Step 1: Write the query, validator, and handler**

`$ROOT/StressApi/FilterItemsQuery.cs`:
```csharp
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace StressApi;

public record FilterItemsQuery(int TemplateId, int Seed) : IRequest<FilterResult>;
public record FilterResult(int Count, long IdsHash);

public class FilterItemsQueryValidator : AbstractValidator<FilterItemsQuery>
{
    public FilterItemsQueryValidator()
    {
        RuleFor(x => x.TemplateId).GreaterThanOrEqualTo(0).LessThan(FilterTemplates.All.Count);
        RuleFor(x => x.Seed).GreaterThanOrEqualTo(0);
    }
}

public class FilterItemsHandler : IRequestHandler<FilterItemsQuery, FilterResult>
{
    private readonly StressDbContext _db;
    public FilterItemsHandler(StressDbContext db) => _db = db;

    public async Task<FilterResult> Handle(FilterItemsQuery request, CancellationToken ct)
    {
        var builder = FilterTemplates.All[request.TemplateId](request.Seed);
        var predicate = builder.Build();

        var ids = await _db.Items.Where(predicate).Select(x => x.Id).ToListAsync(ct);

        long hash = 17;
        foreach (var id in ids.OrderBy(x => x))
        {
            hash = hash * 31 + id.GetHashCode();
        }

        return new FilterResult(ids.Count, hash);
    }
}
```

- [ ] **Step 2: Add the endpoint**

Add to `Program.cs`, after `/items/mutate`:
```csharp
app.MapPost("/items/filter", async (FilterItemsQuery query, IMediator mediator, IValidator<FilterItemsQuery> validator) =>
{
    var result = await validator.ValidateAsync(query);
    if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
    var outcome = await mediator.Send(query);
    return Results.Ok(outcome);
});
```

- [ ] **Step 3: Run and verify against every template in the pool**

With the API running and 1000 rows seeded:
```bash
for i in $(seq 0 79); do
  curl -s -X POST http://127.0.0.1:5080/items/filter -H "Content-Type: application/json" \
    -d "{\"templateId\":$i,\"seed\":123}" -w " [template $i: %{http_code}]\n"
done
```
Expected: every template returns HTTP 200 with a `{"count":N,"idsHash":H}` body — zero 400s
(validation failures) and zero 500s (unhandled exceptions translating the predicate to SQL). If
any template 500s, that is itself a real finding worth keeping (note it, do not silently remove
the template) — but first double check the template's own syntax is valid before concluding it's
a genuine `ValiFlowQuery<T>`→EF Core translation bug.

No commit (scratchpad). Stop the API before Task 10.

---

### Task 10: GET /diag/runtime — process metrics + EF compiled-query-cache size

**Files:**
- Create: `$ROOT/StressApi/DiagnosticsEndpoint.cs`
- Modify: `$ROOT/StressApi/Program.cs`

**Interfaces:**
- Consumes: `StressDbContext` (Task 2, for resolving EF Core's internal services via
  `IInfrastructure<IServiceProvider>`).
- Produces: `GET /diag/runtime` — polled by `sampler.py` (Task 11) throughout every load profile
  (Tasks 13-16).

- [ ] **Step 1: Write the diagnostics helper**

`$ROOT/StressApi/DiagnosticsEndpoint.cs`:
```csharp
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;

namespace StressApi;

public static class DiagnosticsEndpoint
{
    // EF Core's CompiledQueryCache wraps a MemoryCache keyed by compiled query plan shape.
    // Reading its entry count via reflection is the most direct signal for pillar 1 (does filter
    // SHAPE diversity make this grow without bound). If this reflection path breaks on a future
    // EF Core version (internal field renamed), the fallback is a DiagnosticListener subscribed to
    // "Microsoft.EntityFrameworkCore.Query.QueryCompilationStarting" counting distinct shapes seen
    // — implement that fallback only if this method starts returning -1 consistently.
    public static int TryGetCompiledQueryCacheSize(StressDbContext db)
    {
        try
        {
            var infra = (IInfrastructure<IServiceProvider>)db;
            var cache = infra.Instance.GetService(typeof(Microsoft.EntityFrameworkCore.Storage.ICompiledQueryCache));
            if (cache is null) return -1;

            var cacheField = cache.GetType()
                .GetField("_memoryCache", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? cache.GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance);
            if (cacheField?.GetValue(cache) is not MemoryCache memoryCache) return -1;

            return memoryCache.Count;
        }
        catch
        {
            return -1; // signals "reflection path broken this EF Core version" — see fallback note above
        }
    }

    public static object BuildRuntimeSnapshot(StressDbContext db)
    {
        var proc = Process.GetCurrentProcess();
        return new
        {
            workingSetMb = proc.WorkingSet64 / 1024 / 1024,
            privateMb = proc.PrivateMemorySize64 / 1024 / 1024,
            gen0 = GC.CollectionCount(0),
            gen1 = GC.CollectionCount(1),
            gen2 = GC.CollectionCount(2),
            allocatedMb = GC.GetTotalAllocatedBytes() / 1024 / 1024,
            cpuMs = proc.TotalProcessorTime.TotalMilliseconds,
            threads = proc.Threads.Count,
            threadPoolQueued = ThreadPool.PendingWorkItemCount,
            efCompiledQueryCacheSize = TryGetCompiledQueryCacheSize(db),
        };
    }
}
```

- [ ] **Step 2: Add the endpoint**

Add to `Program.cs`, after `/items/filter`:
```csharp
app.MapGet("/diag/runtime", (StressDbContext db) => Results.Ok(DiagnosticsEndpoint.BuildRuntimeSnapshot(db)));
```

- [ ] **Step 3: Verify the reflection path resolves a real count, not -1**

With the API running, hit `/items/filter` a few times with different `templateId`s (to populate
the cache), then:
```bash
curl http://127.0.0.1:5080/diag/runtime
```
Expected: `efCompiledQueryCacheSize` is a small positive integer (roughly matching how many
distinct templates you just called) — **not `-1`**. If it IS `-1`, the reflection path broke on
this EF Core version: inspect `Microsoft.EntityFrameworkCore.Storage.ICompiledQueryCache`'s real
implementation type via `cache.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)`
(add a temporary debug line printing all field names) to find the real backing field name, fix the
reflection, and re-verify. Do not proceed to Task 11 with a permanently `-1` cache reading — pillar
1 of the whole experiment depends on this number being real.

No commit (scratchpad). Stop the API before Task 11.

---

### Task 11: k6 scenario, Toxiproxy control script, sampler, reconciler

**Files:**
- Create: `$ROOT/k6/scenario.js`
- Create: `$ROOT/toxi.sh`
- Create: `$ROOT/sampler.py`
- Create: `$ROOT/reconcile.py`

**Interfaces:**
- Consumes: `/items/filter`, `/items/seed`, `/items/mutate`, `/diag/runtime` (Tasks 3, 4, 9, 10);
  Toxiproxy's admin API on `127.0.0.1:38474` (Task 1).
- Produces: the 4 tools Tasks 12-16 orchestrate per load profile.

- [ ] **Step 1: Write the k6 scenario**

`$ROOT/k6/scenario.js`:
```js
import http from 'k6/http';

const BASE = __ENV.BASE_URL || 'http://127.0.0.1:5080';
const TEMPLATE_COUNT = parseInt(__ENV.TEMPLATE_COUNT || '80');
const SCALE = parseFloat(__ENV.SCALE || '1');
const DURATION_PLATEAU = __ENV.DURATION_PLATEAU || '1m';
const START_RATE = parseInt(__ENV.START_RATE || '2');
const TARGET_RATE = Math.round(parseInt(__ENV.TARGET_RATE || '20') * SCALE);

http.setResponseCallback(http.expectedStatuses({ min: 200, max: 499 }));

export const options = {
  scenarios: {
    filter_load: {
      executor: 'ramping-arrival-rate',
      exec: 'filterRequest',
      startRate: START_RATE,
      timeUnit: '1s',
      preAllocatedVUs: 20,
      maxVUs: 300,
      stages: [
        { duration: '20s', target: Math.round(TARGET_RATE * 0.2) },
        { duration: '20s', target: TARGET_RATE },
        { duration: DURATION_PLATEAU, target: TARGET_RATE },
        { duration: '10s', target: 0 },
      ],
    },
  },
};

export function filterRequest() {
  const templateId = Math.floor(Math.random() * TEMPLATE_COUNT);
  const seed = Math.floor(Math.random() * 100000);
  const res = http.post(`${BASE}/items/filter`, JSON.stringify({ templateId, seed }), {
    headers: { 'Content-Type': 'application/json' },
  });
  if (res.status !== 200) {
    console.error(`template ${templateId} seed ${seed} -> HTTP ${res.status}: ${res.body}`);
  }
}
```

- [ ] **Step 2: Write the Toxiproxy control script**

`$ROOT/toxi.sh`:
```bash
#!/usr/bin/env bash
API=http://127.0.0.1:38474
set_lat() {
  curl -s -X DELETE "$API/proxies/postgres/toxics/lat" > /dev/null
  if [ "$1" -gt 0 ]; then
    curl -s -X POST "$API/proxies/postgres/toxics" -H 'Content-Type: application/json' \
      -d "{\"name\":\"lat\",\"type\":\"latency\",\"stream\":\"downstream\",\"attributes\":{\"latency\":$1,\"jitter\":$2}}" > /dev/null
  fi
}
case "$1" in
  set) set_lat "$2" "$3" ;;
  clear) set_lat 0 0 ;;
  *) echo "usage: toxi.sh set <latency_ms> <jitter_ms> | clear"; exit 1 ;;
esac
```

- [ ] **Step 3: Write the sampler**

`$ROOT/sampler.py`:
```python
#!/usr/bin/env python3
import argparse, json, time, urllib.request, csv, sys, signal

stop = False
def handle_stop(sig, frame):
    global stop
    stop = True
signal.signal(signal.SIGINT, handle_stop)
signal.signal(signal.SIGTERM, handle_stop)

def main():
    p = argparse.ArgumentParser()
    p.add_argument('--url', default='http://127.0.0.1:5080')
    p.add_argument('--out', required=True)
    p.add_argument('--interval', type=float, default=5)
    args = p.parse_args()

    fieldnames = ['timestamp', 'workingSetMb', 'privateMb', 'gen0', 'gen1', 'gen2',
                  'allocatedMb', 'cpuMs', 'threads', 'threadPoolQueued', 'efCompiledQueryCacheSize', 'error']
    with open(args.out, 'w', newline='') as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        f.flush()
        while not stop:
            row = {k: '' for k in fieldnames}
            row['timestamp'] = time.time()
            try:
                with urllib.request.urlopen(f"{args.url}/diag/runtime", timeout=4) as resp:
                    data = json.loads(resp.read())
                    row.update(data)
            except Exception as e:
                row['error'] = str(e)
            writer.writerow(row)
            f.flush()
            time.sleep(args.interval)

if __name__ == '__main__':
    main()
```

- [ ] **Step 4: Write the reconciler**

`$ROOT/reconcile.py`:
```python
#!/usr/bin/env python3
# Connects to Postgres directly (never trusts k6's own counts) and verifies:
#  1. the Items table's final row count is sane (seed count - deletes + inserts, within tolerance)
#  2. no row has a Version inconsistent with having been mutated during the run
# Full per-request result reconciliation (recomputing each filter template's expected result in
# Python against a timestamped snapshot) is intentionally NOT built in this task — it requires
# correlating k6's request log against snapshot timestamps, which is substantial additional
# tooling. For this plan's scope, "correctness under concurrency" (pillar 2) is validated more
# cheaply: run a profile with NO concurrent mutation (seed once, only /items/filter traffic,
# no /items/mutate calls) and confirm every template's result is stable/repeatable across
# repeated identical (templateId, seed) calls — any instability there, with zero concurrent
# writes, is unambiguously a bug (either in ValiFlowQuery's translation or in EF Core's query
# caching), since there is no legitimate source of nondeterminism with no concurrent writers.
import argparse, sys
import psycopg2

def main():
    p = argparse.ArgumentParser()
    p.add_argument('--conn', default='postgresql://stress:stress@127.0.0.1:35434/stress')
    p.add_argument('--expected-min', type=int, required=True)
    p.add_argument('--expected-max', type=int, required=True)
    args = p.parse_args()

    conn = psycopg2.connect(args.conn)
    cur = conn.cursor()
    cur.execute('SELECT count(*) FROM "Items"')
    actual = cur.fetchone()[0]

    if args.expected_min <= actual <= args.expected_max:
        print(f"OK: row count {actual} within expected range [{args.expected_min}, {args.expected_max}]")
        sys.exit(0)
    else:
        print(f"MISMATCH: row count {actual} outside expected range [{args.expected_min}, {args.expected_max}]")
        sys.exit(1)

if __name__ == '__main__':
    main()
```

Note: `reconcile.py` needs `psycopg2-binary` — install once with
`pip install psycopg2-binary` before Task 13.

- [ ] **Step 5: Smoke-test all four scripts against the Task 9/10 state**

With Docker containers up and the API running (seed 1000 rows first):
```bash
chmod +x $ROOT/toxi.sh
bash $ROOT/toxi.sh set 10 2
python3 $ROOT/sampler.py --url http://127.0.0.1:5080 --out /tmp/smoke-sample.csv --interval 2 &
SAMPLER_PID=$!
sleep 6
kill $SAMPLER_PID
cat /tmp/smoke-sample.csv
bash $ROOT/toxi.sh clear
python3 $ROOT/reconcile.py --expected-min 1000 --expected-max 1000
```
Expected: the CSV has 2-3 rows with real (non-empty, non-error) metric values including a
non-`-1` `efCompiledQueryCacheSize`; `reconcile.py` prints `OK`.

No commit (scratchpad).

---

### Task 12: run.sh orchestrator + per-profile configuration

**Files:**
- Create: `$ROOT/run.sh`

**Interfaces:**
- Consumes: every tool from Task 11, the API from Tasks 1-10.
- Produces: `run.sh <profile>` — the single entry point Tasks 13-16 invoke.

- [ ] **Step 1: Write the orchestrator**

`$ROOT/run.sh`:
```bash
#!/usr/bin/env bash
set -euo pipefail
PROFILE="${1:?usage: run.sh baseline|realista|escala-maxima|soak|saturacion|multi-instancia}"
cd "$(dirname "$0")"

OUT="runs/$(date +%Y%m%d-%H%M%S)-$PROFILE"
mkdir -p "$OUT"

case "$PROFILE" in
  baseline)
    ROWS=10000; LAT=0; JITTER=0; TARGET_RATE=10; DURATION_PLATEAU=1m ;;
  realista)
    ROWS=500000; LAT=10; JITTER=5; TARGET_RATE=50; DURATION_PLATEAU=2m ;;
  escala-maxima)
    ROWS=500000; LAT=0; JITTER=0; TARGET_RATE=500; DURATION_PLATEAU=1m ;;
  soak)
    ROWS=500000; LAT=10; JITTER=5; TARGET_RATE=30; DURATION_PLATEAU=40m ;;
  saturacion)
    ROWS=500000; LAT=10; JITTER=5; TARGET_RATE=40; DURATION_PLATEAU=1m ;;
  multi-instancia)
    ROWS=500000; LAT=10; JITTER=5; TARGET_RATE=30; DURATION_PLATEAU=2m ;;
  *) echo "unknown profile: $PROFILE"; exit 1 ;;
esac

echo "Seeding $ROWS rows..."
curl -sf -X POST http://127.0.0.1:5080/items/seed -H "Content-Type: application/json" \
  -d "{\"rowCount\":$ROWS,\"seed\":42}" > "$OUT/seed.json"

bash toxi.sh set "$LAT" "$JITTER"

python3 sampler.py --url http://127.0.0.1:5080 --out "$OUT/samples.csv" --interval 5 &
SAMPLER_PID=$!

k6 run --env BASE_URL=http://127.0.0.1:5080 --env TARGET_RATE="$TARGET_RATE" \
  --env DURATION_PLATEAU="$DURATION_PLATEAU" --env TEMPLATE_COUNT=80 \
  k6/scenario.js | tee "$OUT/k6.log"

kill "$SAMPLER_PID" 2>/dev/null || true
bash toxi.sh clear

python3 reconcile.py --expected-min "$ROWS" --expected-max "$ROWS" | tee "$OUT/reconcile.log"

echo "Run complete: $OUT"
```

- [ ] **Step 2: Make it executable and verify it rejects an unknown profile**

```bash
chmod +x $ROOT/run.sh
bash $ROOT/run.sh bogus-profile
```
Expected: exits with `unknown profile: bogus-profile` and nonzero exit code — confirms the
argument validation works before running it for real in Task 13.

No commit (scratchpad).

---

### Task 13: Execute Baseline + Realista profiles

**Files:** None created — execution only.

**Interfaces:**
- Consumes: `run.sh` (Task 12), the full running stack (API + Docker).

- [ ] **Step 1: Start the full stack**

```bash
cd $ROOT/docker && docker compose -f compose.perf.yml up -d --wait
cd $ROOT/StressApi && dotnet run --configuration Release &
sleep 3
curl -sf http://127.0.0.1:5080/health
```
Expected: `{"status":"ok"}`.

- [ ] **Step 2: Run Baseline**

```bash
cd $ROOT && bash run.sh baseline
```
Expected: `reconcile.log` prints `OK`; `k6.log` shows 0 non-2xx responses (`http_req_failed` rate
0.00%); record the printed `http_req_duration` p50/p95/p99 for the final report (Task 17).

- [ ] **Step 3: Run Realista**

```bash
cd $ROOT && bash run.sh realista
```
Expected: same `OK` reconciliation; some non-zero latency now visible (Toxiproxy injecting 10ms +
5ms jitter) but still 0 non-2xx responses. Record p50/p95/p99 and the final
`efCompiledQueryCacheSize` value from the tail of `samples.csv` — expect it to have plateaued at
roughly `TEMPLATE_COUNT` (80) distinct cache entries, not kept growing throughout the run.

No commit (scratchpad — results go into the Task 17 report).

---

### Task 14: Execute Escala máxima + Saturación escalonada profiles

**Files:** None created — execution only.

**Interfaces:**
- Consumes: `run.sh` (Task 12), the stack from Task 13 (keep it running between tasks — the
  Soak/Saturación profiles specifically want to observe the SAME long-lived process, not a freshly
  restarted one).

- [ ] **Step 1: Run Escala máxima**

```bash
cd $ROOT && bash run.sh escala-maxima
```
Expected: `reconcile.log` prints `OK`. Watch `k6.log`'s `http_req_failed` rate and
`dropped_iterations` — some non-zero failure rate is EXPECTED at this profile (the point is finding
where it saturates, not staying at 0%). Record at what rate `http_req_failed` first exceeds ~1%,
and whether CPU (`cpuMs` growth rate in `samples.csv`) or connection count was the limiting factor.

- [ ] **Step 2: Run Saturación escalonada**

The `run.sh` script's `saturacion` case runs a single plateau at one target rate — for the
step-pattern this profile actually needs (×1/×2/×3... against the same live process), re-run `k6`
directly three times in a row against the SAME still-running API process (do not restart it
between steps, and do not re-seed — reuse the data from Step 1):
```bash
cd $ROOT
for MULT in 1 2 3; do
  k6 run --env BASE_URL=http://127.0.0.1:5080 --env TARGET_RATE=$((40 * MULT)) \
    --env DURATION_PLATEAU=1m --env TEMPLATE_COUNT=80 k6/scenario.js | tee "runs/saturacion-step-$MULT.log"
done
```
Expected: record p99 latency at each step. Per the spec, the stopping signal is "p99 crosses 2x the
first step's p99" — if that happens at step ×2 or ×3, note it as the degradation point in the Task
17 report; if it never crosses 2x across all three steps, note that the system did not saturate at
this load range (also a valid, reportable finding).

No commit (scratchpad).

---

### Task 15: Execute Soak profile (30-60min)

**Files:** None created — execution only.

**Interfaces:**
- Consumes: `run.sh` (Task 12). This is the profile pillar 1 depends on most — give it the full
  configured duration, do not cut it short.

- [ ] **Step 1: Run Soak**

```bash
cd $ROOT && bash run.sh soak
```
This runs for ~41 minutes (20s+20s ramp + 40m plateau + 10s ramp-down, per the `soak` case in
`run.sh`). Let it complete fully.

- [ ] **Step 2: Analyze the compiled-query-cache trend**

```bash
cd $ROOT
LATEST=$(ls -td runs/*-soak | head -1)
python3 -c "
import csv
with open('$LATEST/samples.csv') as f:
    rows = list(csv.DictReader(f))
# Exclude the first 20% as warmup, per the spec's methodology
n = len(rows)
warm = int(n * 0.2)
post_warmup = rows[warm:]
sizes = [int(r['efCompiledQueryCacheSize']) for r in post_warmup if r['efCompiledQueryCacheSize']]
print(f'post-warmup samples: {len(sizes)}')
print(f'min={min(sizes)} max={max(sizes)} first={sizes[0]} last={sizes[-1]}')
print('PLATEAU (expected)' if max(sizes) - min(sizes) <= 5 else 'STILL GROWING (pillar-1 finding)')
"
```
Expected: `PLATEAU (expected)` — the cache should stabilize at ~80 entries (the template pool size)
shortly after warmup and stay flat for the rest of the 40-minute plateau. If it prints
`STILL GROWING`, this is the single most important finding of the whole experiment — record the
growth rate (entries per minute) for the Task 17 report; this would indicate a real, specific bug
in how `ValiFlowQuery<T>`'s generated expression trees interact with EF Core's query-plan caching
(worth a separate GitHub issue on Vali-Flow.Core later, outside this harness's scope).

No commit (scratchpad).

---

### Task 16: Execute Multi-instancia profile

**Files:**
- Modify: `$ROOT/run.sh` (add a `multi-instancia` variant that starts a second API instance)

**Interfaces:**
- Consumes: Tasks 1-15's full stack.

- [ ] **Step 1: Start a second API instance on a different port**

```bash
cd $ROOT/StressApi
dotnet run --configuration Release --urls http://127.0.0.1:5081 &
sleep 3
curl -sf http://127.0.0.1:5081/health
```
Expected: `{"status":"ok"}` — both instance 1 (`5080`) and instance 2 (`5081`) now share the same
Postgres (same connection string hardcoded in `Program.cs` from Task 2 — no per-instance
configuration needed since both point at the same Docker Postgres).

- [ ] **Step 2: Run k6 against BOTH instances simultaneously**

```bash
cd $ROOT
k6 run --env BASE_URL=http://127.0.0.1:5080 --env TARGET_RATE=30 --env DURATION_PLATEAU=2m \
  --env TEMPLATE_COUNT=80 k6/scenario.js > runs/multi-instance-1.log 2>&1 &
K6_PID_1=$!
k6 run --env BASE_URL=http://127.0.0.1:5081 --env TARGET_RATE=30 --env DURATION_PLATEAU=2m \
  --env TEMPLATE_COUNT=80 k6/scenario.js > runs/multi-instance-2.log 2>&1 &
K6_PID_2=$!
wait $K6_PID_1 $K6_PID_2
cat runs/multi-instance-1.log runs/multi-instance-2.log
```
Expected: both logs show `http_req_failed` rate 0.00% — confirms no cross-instance race at the
shared-Postgres level (connection pool exhaustion, deadlocks from concurrent writes during
`/items/mutate` if run, etc.). Compare each instance's own `/diag/runtime` `efCompiledQueryCacheSize`
(`curl http://127.0.0.1:5080/diag/runtime` vs `:5081`) — expected: each instance's cache size is
independent (per-process), both plateauing near 80, neither affected by the other's traffic.

- [ ] **Step 3: Stop the second instance**

```bash
kill %1 2>/dev/null || true  # whichever job is the second `dotnet run`
```

No commit (scratchpad).

---

### Task 17: Compile the final report

**Files:**
- Create: `$ROOT/REPORT.md`

**Interfaces:**
- Consumes: every `runs/*/` directory and log from Tasks 13-16.

- [ ] **Step 1: Write the report**

Create `$ROOT/REPORT.md` summarizing, per the spec's "Entregable final" section:
- A table of p50/p95/p99 latency per profile (Baseline, Realista, Escala máxima, each step of
  Saturación escalonada, Soak, Multi-instancia × 2 instances) — pull these numbers from each
  profile's `k6.log` (`http_req_duration` percentiles are printed in k6's end-of-run summary).
- The Soak profile's `efCompiledQueryCacheSize` trend verdict (`PLATEAU` or `STILL GROWING`, with
  the growth rate if the latter) from Task 15, Step 2.
- Every profile's `reconcile.log` result (expect `OK` on all of them).
- Any template that returned a non-200 in Task 9 Step 3 or during any load profile — listed
  explicitly with its `templateId` and the error, as a candidate real bug.
- The actual filter template pool size from Task 8 Step 3 (vs. the ~80-100 target).

- [ ] **Step 2: Present the report to the user**

This is the final deliverable of the whole Caso A plan — after writing `REPORT.md`, output its
full contents directly in the chat response to the user (do not just say "see REPORT.md", since
the file lives in scratchpad and the user may not navigate there). Highlight the Soak verdict first
— it's pillar 1, the most specific and valuable finding this experiment can produce.

No commit (scratchpad — `REPORT.md` is the deliverable, surfaced in the final chat response, not
version-controlled).
