using BlazorBootstrap;
using CoreBlazor.Tests.TestHelpers;
using CoreBlazor.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace CoreBlazor.Tests.Utils;

public class DbContextExtensionsTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int RelatedId { get; set; }
        public RelatedEntity? Related { get; set; }
    }

    private class RelatedEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    private class EntityWithoutPrimaryKey
    {
        public string Name { get; set; } = string.Empty;
    }

    private class EntityWithCompositePrimaryKey
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class EntityWithNoPrimaryKey
    {
        public string Description { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<RelatedEntity> RelatedEntities { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
            modelBuilder.Entity<TestEntity>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();
            
            modelBuilder.Entity<RelatedEntity>().HasKey(r => r.Id);
            
            modelBuilder.Entity<TestEntity>()
                .HasOne(e => e.Related)
                .WithMany()
                .HasForeignKey(e => e.RelatedId);
            
            base.OnModelCreating(modelBuilder);
        }
    }

    private class CompositeKeyDbContext : DbContext
    {
        public DbSet<EntityWithCompositePrimaryKey> CompositeEntities { get; set; }

        public CompositeKeyDbContext(DbContextOptions<CompositeKeyDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityWithCompositePrimaryKey>()
                .HasKey(e => new { e.Id1, e.Id2 });
            
            base.OnModelCreating(modelBuilder);
        }
    }

    private class NoPrimaryKeyDbContext : DbContext
    {
        public DbSet<EntityWithNoPrimaryKey> Entities { get; set; }

        public NoPrimaryKeyDbContext(DbContextOptions<NoPrimaryKeyDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityWithNoPrimaryKey>().HasNoKey();
            base.OnModelCreating(modelBuilder);
        }
    }

    [Fact]
    public void GetNavigations_ReturnsNavigationProperties()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(GetNavigations_ReturnsNavigationProperties));
        using var context = new TestDbContext(options);

        // Act
        var navigations = context.GetNavigations<TestEntity>().ToList();

        // Assert
        navigations.Should().NotBeEmpty();
        navigations.Should().ContainSingle(n => n.Name == nameof(TestEntity.Related));
    }

    [Fact]
    public void GetNavigations_EntityNotInModel_ReturnsEmpty()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(GetNavigations_EntityNotInModel_ReturnsEmpty));
        using var context = new TestDbContext(options);

        // Act
        var navigations = context.GetNavigations<EntityWithoutPrimaryKey>().ToList();

        // Assert
        navigations.Should().BeEmpty();
    }

    [Fact]
    public void IsNavigation_WhenPropertyIsNavigation_ReturnsTrue()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(IsNavigation_WhenPropertyIsNavigation_ReturnsTrue));
        using var context = new TestDbContext(options);
        var relatedProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Related))!;

        // Act
        var result = context.IsNavigation<TestEntity>(relatedProperty);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNavigation_WhenPropertyIsNotNavigation_ReturnsFalse()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(IsNavigation_WhenPropertyIsNotNavigation_ReturnsFalse));
        using var context = new TestDbContext(options);
        var nameProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;

        // Act
        var result = context.IsNavigation<TestEntity>(nameProperty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetPrimaryKey_ReturnsPrimaryKeyProperty()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(GetPrimaryKey_ReturnsPrimaryKeyProperty));
        using var context = new TestDbContext(options);

        // Act
        var pk = context.GetPrimaryKey<TestEntity>();

        // Assert
        pk.Should().NotBeNull();
        pk.PropertyInfo!.Name.Should().Be(nameof(TestEntity.Id));
    }

    [Fact]
    public void GetPrimaryKey_EntityNotInModel_ThrowsException()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(GetPrimaryKey_EntityNotInModel_ThrowsException));
        using var context = new TestDbContext(options);

        // Act
        var act = () => context.GetPrimaryKey<EntityWithoutPrimaryKey>();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found in the model*");
    }

    [Fact]
    public void GetPrimaryKey_EntityWithCompositeKey_ReturnsFirstKey()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<CompositeKeyDbContext>(nameof(GetPrimaryKey_EntityWithCompositeKey_ReturnsFirstKey));
        using var context = new CompositeKeyDbContext(options);

        // Act
        var pk = context.GetPrimaryKey<EntityWithCompositePrimaryKey>();

        // Assert
        pk.Should().NotBeNull();
        pk.PropertyInfo!.Name.Should().Be(nameof(EntityWithCompositePrimaryKey.Id1));
    }

    [Fact]
    public void GetPrimaryKey_EntityWithNoPrimaryKey_ThrowsException()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<NoPrimaryKeyDbContext>(nameof(GetPrimaryKey_EntityWithNoPrimaryKey_ThrowsException));
        using var context = new NoPrimaryKeyDbContext(options);

        // Act
        var act = () => context.GetPrimaryKey<EntityWithNoPrimaryKey>();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Primary key for entity type*not found*");
    }

    [Fact]
    public void IsPrimaryKey_WhenPropertyIsPrimaryKey_ReturnsTrue()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(IsPrimaryKey_WhenPropertyIsPrimaryKey_ReturnsTrue));
        using var context = new TestDbContext(options);
        var idProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Id))!;

        // Act
        var result = context.IsPrimaryKey<TestEntity>(idProperty);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPrimaryKey_WhenPropertyIsNotPrimaryKey_ReturnsFalse()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(IsPrimaryKey_WhenPropertyIsNotPrimaryKey_ReturnsFalse));
        using var context = new TestDbContext(options);
        var nameProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;

        // Act
        var result = context.IsPrimaryKey<TestEntity>(nameProperty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsGeneratedPrimaryKey_WhenPrimaryKeyIsGenerated_ReturnsTrue()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(IsGeneratedPrimaryKey_WhenPrimaryKeyIsGenerated_ReturnsTrue));
        using var context = new TestDbContext(options);
        var idProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Id))!;

        // Act
        var result = context.IsGeneratedPrimaryKey<TestEntity>(idProperty);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsGeneratedPrimaryKey_WhenPropertyIsNotPrimaryKey_ReturnsFalse()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(IsGeneratedPrimaryKey_WhenPropertyIsNotPrimaryKey_ReturnsFalse));
        using var context = new TestDbContext(options);
        var nameProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;

        // Act
        var result = context.IsGeneratedPrimaryKey<TestEntity>(nameProperty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetForeignKeys_ReturnsForeignKeys()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(GetForeignKeys_ReturnsForeignKeys));
        using var context = new TestDbContext(options);

        // Act
        var foreignKeys = context.GetForeignKeys<TestEntity>().ToList();

        // Assert
        foreignKeys.Should().NotBeEmpty();
        foreignKeys.Should().ContainSingle();
    }

    [Fact]
    public void GetForeignKeys_EntityNotInModel_ThrowsException()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(GetForeignKeys_EntityNotInModel_ThrowsException));
        using var context = new TestDbContext(options);

        // Act
        var act = () => context.GetForeignKeys<EntityWithoutPrimaryKey>().ToList();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found in the model*");
    }

    [Fact]
    public void IsForeignKey_WhenPropertyIsForeignKey_ReturnsTrue()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(IsForeignKey_WhenPropertyIsForeignKey_ReturnsTrue));
        using var context = new TestDbContext(options);
        var relatedIdProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.RelatedId))!;

        // Act
        var result = context.IsForeignKey<TestEntity>(relatedIdProperty);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsForeignKey_WhenPropertyIsNotForeignKey_ReturnsFalse()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(IsForeignKey_WhenPropertyIsNotForeignKey_ReturnsFalse));
        using var context = new TestDbContext(options);
        var nameProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;

        // Act
        var result = context.IsForeignKey<TestEntity>(nameProperty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DbSetWithDisplayableNavigations_WithoutSplitQueries_ReturnsQueryWithIncludes()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(DbSetWithDisplayableNavigations_WithoutSplitQueries_ReturnsQueryWithIncludes));
        using var context = new TestDbContext(options);

        // Act
        var query = context.DbSetWithDisplayableNavigations<TestEntity>(false);

        // Assert
        query.Should().NotBeNull();
        query.Should().BeAssignableTo<IQueryable<TestEntity>>();
    }

    [Fact]
    public void DbSetWithDisplayableNavigations_WithSplitQueries_ReturnsQueryWithSplitQuery()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(DbSetWithDisplayableNavigations_WithSplitQueries_ReturnsQueryWithSplitQuery));
        using var context = new TestDbContext(options);

        // Act
        var query = context.DbSetWithDisplayableNavigations<TestEntity>(true);

        // Assert
        query.Should().NotBeNull();
        query.Should().BeAssignableTo<IQueryable<TestEntity>>();
    }

    [Fact]
    public void GetDbSets_ReturnsDbSetProperties()
    {
        // Arrange
        var contextType = typeof(TestDbContext);

        // Act
        var dbSets = DbContextExtensions.GetDbSets(contextType).ToList();

        // Assert
        dbSets.Should().NotBeEmpty();
        dbSets.Should().HaveCount(2);
        dbSets.Should().Contain(p => p.Name == nameof(TestDbContext.TestEntities));
        dbSets.Should().Contain(p => p.Name == nameof(TestDbContext.RelatedEntities));
    }

    [Fact]
    public void GetDbSets_NonDbContextType_ReturnsEmpty()
    {
        // Arrange
        var type = typeof(string);

        // Act
        var dbSets = DbContextExtensions.GetDbSets(type).ToList();

        // Assert
        dbSets.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyTo_WithValidRequest_ReturnsGridDataProviderResult()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(ApplyTo_WithValidRequest_ReturnsGridDataProviderResult));
        using var context = new TestDbContext(options);
        
        var related = new RelatedEntity { Id = 1, Title = "Related Item" };
        context.RelatedEntities.Add(related);
        context.TestEntities.Add(new TestEntity { Id = 1, Name = "Test 1", RelatedId = 1, Related = related });
        context.TestEntities.Add(new TestEntity { Id = 2, Name = "Test 2", RelatedId = 1, Related = related });
        await context.SaveChangesAsync();

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 1,
            PageSize = 10,
            Filters = [],
            Sorting = []
        };

        // Act
        var result = await context.ApplyTo(request, useSplitQueries: false);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task ApplyTo_WithSplitQueries_ReturnsGridDataProviderResult()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(ApplyTo_WithSplitQueries_ReturnsGridDataProviderResult));
        using var context = new TestDbContext(options);
        
        var related = new RelatedEntity { Id = 1, Title = "Related Item" };
        context.RelatedEntities.Add(related);
        context.TestEntities.Add(new TestEntity { Id = 1, Name = "Test Item", RelatedId = 1, Related = related });
        await context.SaveChangesAsync();

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 1,
            PageSize = 10,
            Filters = [],
            Sorting = []
        };

        // Act
        var result = await context.ApplyTo(request, useSplitQueries: true);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
    }

    [Fact]
    public async Task ApplyTo_WithEmptyDatabase_ReturnsEmptyResult()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(ApplyTo_WithEmptyDatabase_ReturnsEmptyResult));
        using var context = new TestDbContext(options);

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 1,
            PageSize = 10,
            Filters = [],
            Sorting = []
        };

        // Act
        var result = await context.ApplyTo(request, useSplitQueries: false);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }
}
