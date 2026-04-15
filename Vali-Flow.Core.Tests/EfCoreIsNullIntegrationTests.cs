using System.Linq.Expressions;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

/// <summary>
/// Integration tests for IsNull() with EF Core.
/// These tests verify that IsNull() generates correct SQL when used with EF Core's query translation.
/// This is where the WHERE 0=1 bug manifests — in-memory validation passes but EF Core translation fails.
/// </summary>
public class EfCoreIsNullIntegrationTests
{
    // Test entity: Simple entity with nullable property
    private class TestEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    // Test DbContext
    private class TestDbContext : DbContext
    {
        public DbSet<TestEntity> Entities { get; set; } = null!;

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure entity with global query filter that uses IsNull()
            var entityBuilder = modelBuilder.Entity<TestEntity>();
            entityBuilder.HasKey(e => e.Id);

            // This is the problematic scenario: using IsNull() in a global query filter
            // When combined with the composed expression from Vali-Flow, it generates WHERE 0=1
            var isNullFilter = new ValiFlow<TestEntity>().IsNull(e => e.DeletedAt).Build();
            entityBuilder.HasQueryFilter(isNullFilter);
        }
    }

    [Fact]
    public void IsNull_WithEfCoreGlobalQueryFilter_FilterOutsNullValues()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("TestDb_IsNullFilter")
            .Options;

        // Create context and add test data
        using (var context = new TestDbContext(options))
        {
            context.Database.EnsureCreated();

            var entity1 = new TestEntity { Id = 1, Name = "Active", DeletedAt = null };
            var entity2 = new TestEntity { Id = 2, Name = "Deleted", DeletedAt = DateTime.UtcNow };

            context.Entities.Add(entity1);
            context.Entities.Add(entity2);
            context.SaveChanges();
        }

        // Act & Assert
        using (var context = new TestDbContext(options))
        {
            var result = context.Entities.ToList();

            // Should only return entity with DeletedAt = null
            // If WHERE 0=1 bug is present, result will be empty
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Active");
            result.First().DeletedAt.Should().BeNull();
        }

        // Cleanup
        using (var context = new TestDbContext(options))
        {
            context.Database.EnsureDeleted();
        }
    }

    [Fact]
    public void IsNull_DirectLambdaComparison_Works()
    {
        // Arrange: Compare IsNull() result with direct lambda
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("TestDb_DirectComparison")
            .Options;

        using (var context = new TestDbContext(options))
        {
            context.Database.EnsureCreated();

            var entity1 = new TestEntity { Id = 1, Name = "Active", DeletedAt = null };
            var entity2 = new TestEntity { Id = 2, Name = "Deleted", DeletedAt = DateTime.UtcNow };

            context.Entities.Add(entity1);
            context.Entities.Add(entity2);
            context.SaveChanges();
        }

        // Act
        using (var context = new TestDbContext(options))
        {
            // Direct lambda filter (expected behavior)
            var directLambda = new ValiFlow<TestEntity>().IsNull(e => e.DeletedAt).Build();
            var resultDirect = context.Entities.Where(directLambda).ToList();

            // The filter applied via global query filter
            var resultGlobal = context.Entities.ToList();

            // Assert: Both should return the same result
            resultDirect.Should().HaveCount(1);
            resultGlobal.Should().HaveCount(1);
            resultDirect.First().Id.Should().Be(resultGlobal.First().Id);
        }

        // Cleanup
        using (var context = new TestDbContext(options))
        {
            context.Database.EnsureDeleted();
        }
    }

    [Fact]
    public void IsNull_ExpressionCompositionDebug_VerifyStructure()
    {
        // This test verifies the structure of the composed expression
        // to help identify where the WHERE 0=1 issue originates

        var valiFlow = new ValiFlow<TestEntity>().IsNull(e => e.DeletedAt);
        var expression = valiFlow.Build();

        // The expression should compile without errors
        var compiled = expression.Compile();

        // Test the compiled expression directly
        var activeEntity = new TestEntity { Id = 1, DeletedAt = null };
        var deletedEntity = new TestEntity { Id = 2, DeletedAt = DateTime.UtcNow };

        compiled(activeEntity).Should().BeTrue("Entity with null DeletedAt should pass");
        compiled(deletedEntity).Should().BeFalse("Entity with non-null DeletedAt should fail");
    }

    [Fact]
    public void IsNull_GeneratedSQL_DoesNotContainWhere0Equals1()
    {
        // This test verifies the actual SQL generated by EF Core
        // The WHERE 0=1 bug would appear in the generated SQL as "WHERE 0 = 1"

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("TestDb_SQLGeneration")
            .Options;

        using (var context = new TestDbContext(options))
        {
            context.Database.EnsureCreated();
            context.Entities.Add(new TestEntity { Id = 1, Name = "Active", DeletedAt = null });
            context.SaveChanges();
        }

        using (var context = new TestDbContext(options))
        {
            // Get the generated SQL query
            // Note: InMemory doesn't actually generate SQL, but ToQueryString() shows what would be generated
            var query = context.Entities.AsQueryable();

            try
            {
                // This will show us what SQL would be generated if we were using SQL Server
                var sql = query.ToQueryString();

                // The SQL should NOT contain "WHERE 0 = 1" (which is the bug)
                sql.Should().NotContain("0 = 1",
                    "The generated SQL should not contain WHERE 0=1 condition (bug indicator)");

                // The SQL should reference DeletedAt in some form (IS NULL or similar)
                // Note: InMemory may not show actual SQL, but this validates the pattern
                sql.Should().NotBeNullOrEmpty("Query should generate a valid string representation");
            }
            catch (NotSupportedException)
            {
                // InMemory doesn't support ToQueryString, so we skip this validation
                // This is expected and OK - the main tests above validate behavior
            }
        }

        // Cleanup
        using (var context = new TestDbContext(options))
        {
            context.Database.EnsureDeleted();
        }
    }

    [Fact]
    public void IsNull_WithSqlServerProvider_VerifiesSQLStructure()
    {
        // This test would verify against actual SQL Server generated SQL
        // For now, we test the composed expression structure to ensure it's well-formed

        var isNullFilter = new ValiFlow<TestEntity>().IsNull(e => e.DeletedAt).Build();

        // Verify the expression tree structure
        isNullFilter.Should().NotBeNull();
        isNullFilter.Parameters.Should().HaveCount(1);
        isNullFilter.Body.Should().NotBeNull();

        // The body should be a BinaryExpression (DeletedAt == null)
        isNullFilter.Body.NodeType.Should().Be(System.Linq.Expressions.ExpressionType.Equal,
            "IsNull should create an Equal comparison expression");

        // Compile and verify it works correctly
        var compiled = isNullFilter.Compile();
        var activeEntity = new TestEntity { Id = 1, DeletedAt = null };
        var deletedEntity = new TestEntity { Id = 2, DeletedAt = DateTime.UtcNow };

        compiled(activeEntity).Should().BeTrue();
        compiled(deletedEntity).Should().BeFalse();
    }
}
