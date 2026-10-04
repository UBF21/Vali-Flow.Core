using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Vali_Flow.Core.Builder;
using Xunit;

namespace Vali_Flow.Core.Tests;

/// <summary>
/// Regression tests for ValiFlowQuery&lt;T&gt;'s "EF Core-safe" contract against a REAL relational
/// provider (SQLite), not <c>UseInMemoryDatabase</c>. EF Core's InMemory provider evaluates most
/// expressions client-side and does not exercise the LINQ-to-SQL translation pipeline the same
/// way a relational provider does — it would not have caught the bug these tests guard against
/// (IsLastDayOfMonth building <see cref="DateTime.DaysInMonth(int, int)"/>, which no relational
/// EF Core provider can translate). SQLite is used here purely as a lightweight, Docker-free
/// stand-in for "any real relational provider" — the translatability failure this guards against
/// happens in EF Core's shared, provider-agnostic LINQ translation layer, before any
/// provider-specific SQL is generated, so a fix verified here holds for SQL Server/PostgreSQL/
/// MySQL too.
/// </summary>
public class EfCoreRelationalTranslationTests
{
    private record StubItem(int Id, DateTime CreatedAt, DateTimeOffset UpdatedAt, DateOnly BirthDate);

    private class StubDbContext : DbContext
    {
        public StubDbContext(DbContextOptions<StubDbContext> options) : base(options) { }
        public DbSet<StubItem> Items => Set<StubItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StubItem>().HasKey(x => x.Id);
        }
    }

    private static StubDbContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<StubDbContext>().UseSqlite(connection).Options;
        var context = new StubDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void IsLastDayOfMonth_DateTime_TranslatesToSqlAndMatchesCorrectly()
    {
        using var db = CreateContext();
        db.Items.AddRange(
            new StubItem(1, new DateTime(2025, 4, 30), DateTimeOffset.UtcNow, DateOnly.FromDateTime(DateTime.UtcNow)),
            new StubItem(2, new DateTime(2025, 4, 29), DateTimeOffset.UtcNow, DateOnly.FromDateTime(DateTime.UtcNow)));
        db.SaveChanges();

        Expression<Func<StubItem, bool>> predicate =
            new ValiFlowQuery<StubItem>().IsLastDayOfMonth(x => x.CreatedAt).Build();

        var act = () => db.Items.Where(predicate).Select(x => x.Id).ToList();
        var ids = act.Should().NotThrow().Subject;

        ids.Should().ContainSingle().Which.Should().Be(1);
    }

    // NOTE: IsLastDayOfMonth for DateTimeOffset is intentionally NOT covered here via SQLite.
    // SQLite's own EF Core provider cannot translate ANY DateTimeOffset member access at all —
    // confirmed by probing even the pre-existing, untouched IsFirstDayOfMonth's `val.Day == 1`
    // pattern, which fails translation identically. This is a documented SQLite-specific
    // limitation (SQLite has no native DateTimeOffset column type), not something introduced by
    // this fix, and not representative of production-grade relational providers (SQL Server,
    // PostgreSQL) which map DateTimeOffset to real datetimeoffset/timestamptz columns with full
    // Year/Month/Day extraction support. The DateTimeOffset fix is verified instead directly
    // against a real PostgreSQL instance as part of the Caso A stress-test harness.

    [Fact]
    public void IsLastDayOfMonth_DateOnly_TranslatesToSqlAndMatchesCorrectly()
    {
        using var db = CreateContext();
        db.Items.AddRange(
            new StubItem(1, DateTime.UtcNow, DateTimeOffset.UtcNow, new DateOnly(2025, 4, 30)),
            new StubItem(2, DateTime.UtcNow, DateTimeOffset.UtcNow, new DateOnly(2025, 4, 29)));
        db.SaveChanges();

        Expression<Func<StubItem, bool>> predicate =
            new ValiFlowQuery<StubItem>().IsLastDayOfMonth(x => x.BirthDate).Build();

        var act = () => db.Items.Where(predicate).Select(x => x.Id).ToList();
        var ids = act.Should().NotThrow().Subject;

        ids.Should().ContainSingle().Which.Should().Be(1);
    }
}
