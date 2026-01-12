using Bunit;
using CoreBlazor.Authorization;
using CoreBlazor.Configuration;
using CoreBlazor.Interfaces;
using CoreBlazor.Pages;
using CoreBlazor.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace CoreBlazor.Tests.Pages;

public class PageComponentTests : Bunit.TestContext
{
    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }
    }

    private readonly IDbContextFactory<TestDbContext> _contextFactory = Substitute.For<IDbContextFactory<TestDbContext>>();
    private readonly INavigationPathProvider _navigationPathProvider = Substitute.For<INavigationPathProvider>();
    private readonly INotAuthorizedComponentTypeProvider _notAuthorizedComponentTypeProvider = Substitute.For<INotAuthorizedComponentTypeProvider>();

    public PageComponentTests()
    {
        Services.AddSingleton(_contextFactory);
        Services.AddSingleton(_navigationPathProvider);
        Services.AddSingleton(_notAuthorizedComponentTypeProvider);

        Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationService, FakeAuthorizationService>();
        Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies<TestDbContext>.CanReadInfo, policy => policy.RequireAssertion(_ => true));
            options.AddPolicy(Policies<TestDbContext, TestEntity>.CanRead, policy => policy.RequireAssertion(_ => true));
            options.AddPolicy(Policies<TestDbContext, TestEntity>.CanCreate, policy => policy.RequireAssertion(_ => true));
            options.AddPolicy(Policies<TestDbContext, TestEntity>.CanEdit, policy => policy.RequireAssertion(_ => true));
            options.AddPolicy(Policies<TestDbContext, TestEntity>.CanDelete, policy => policy.RequireAssertion(_ => true));
        });

        var authProvider = Substitute.For<AuthenticationStateProvider>();
        authProvider.GetAuthenticationStateAsync().Returns(AuthenticationHelper.CreateAuthenticationState());
        Services.AddSingleton(authProvider);

        // Add discovered contexts for page routing
        var discoveredContext = new DiscoveredContext
        {
            ContextType = typeof(TestDbContext),
            Sets = new List<DiscoveredSet> { new DiscoveredSet { EntityType = typeof(TestEntity) } }
        };
        Services.AddSingleton<IEnumerable<DiscoveredContext>>(new List<DiscoveredContext> { discoveredContext });
    }

    [Fact]
    public void DbContextInfoPage_ShouldRender_WithValidContext()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(DbContextInfoPage_ShouldRender_WithValidContext));
        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));

        // Act
        var cut = RenderComponent<DbContextInfoPage>(parameters =>
        {
            parameters.Add(p => p.DbContext, nameof(TestDbContext));
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Instance.Should().NotBeNull();
        cut.Instance.DbContext.Should().Be(nameof(TestDbContext));
        cut.Instance.ComponentType.Should().NotBeNull();
    }

    [Fact]
    public void DbContextInfoPage_ShouldSetComponentType_Correctly()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(DbContextInfoPage_ShouldSetComponentType_Correctly));
        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));

        // Act
        var cut = RenderComponent<DbContextInfoPage>(parameters =>
        {
            parameters.Add(p => p.DbContext, nameof(TestDbContext));
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Instance.ComponentType.Should().NotBeNull();
        cut.Instance.ComponentType.IsGenericType.Should().BeTrue();
    }

    [Fact]
    public void DbSetPage_ShouldRender_WithValidParameters()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(DbSetPage_ShouldRender_WithValidParameters));
        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(Arg.Any<string>(), Arg.Any<string>()).Returns("/entities");
        _navigationPathProvider.GetPathToCreateEntity(Arg.Any<string>(), Arg.Any<string>()).Returns("/create");
        _navigationPathProvider.GetPathToEditEntity(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("/edit/1");
        _navigationPathProvider.GetPathToDeleteEntity(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("/delete/1");

        // Act
        var cut = RenderComponent<DbSetPage>(parameters =>
        {
            parameters.Add(p => p.DbContext, nameof(TestDbContext));
            parameters.Add(p => p.DbSet, nameof(TestEntity));
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Instance.Should().NotBeNull();
        cut.Instance.DbContext.Should().Be(nameof(TestDbContext));
        cut.Instance.DbSet.Should().Be(nameof(TestEntity));
        cut.Instance.ComponentType.Should().NotBeNull();
    }

    [Fact]
    public void DbSetPage_ShouldSetComponentType_ForDbSetComponent()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(DbSetPage_ShouldSetComponentType_ForDbSetComponent));
        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(Arg.Any<string>(), Arg.Any<string>()).Returns("/entities");
        _navigationPathProvider.GetPathToCreateEntity(Arg.Any<string>(), Arg.Any<string>()).Returns("/create");
        _navigationPathProvider.GetPathToEditEntity(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("/edit/1");
        _navigationPathProvider.GetPathToDeleteEntity(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("/delete/1");

        // Act
        var cut = RenderComponent<DbSetPage>(parameters =>
        {
            parameters.Add(p => p.DbContext, nameof(TestDbContext));
            parameters.Add(p => p.DbSet, nameof(TestEntity));
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Instance.ComponentType.IsGenericType.Should().BeTrue();
        cut.Instance.ComponentType.GetGenericTypeDefinition().Name.Should().Contain("DbSetComponent");
    }

    [Fact]
    public void EntityCreationPage_ShouldRender_WithValidParameters()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(EntityCreationPage_ShouldRender_WithValidParameters));
        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(Arg.Any<string>(), Arg.Any<string>()).Returns("/entities");

        // Act
        var cut = RenderComponent<EntityCreationPage>(parameters =>
        {
            parameters.Add(p => p.DbContext, nameof(TestDbContext));
            parameters.Add(p => p.DbSet, nameof(TestEntity));
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Instance.Should().NotBeNull();
        cut.Instance.DbContext.Should().Be(nameof(TestDbContext));
        cut.Instance.DbSet.Should().Be(nameof(TestEntity));
    }

    [Fact]
    public void EntityEditorPage_ShouldRender_WithValidParameters()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(EntityEditorPage_ShouldRender_WithValidParameters));
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 1, Name = "Test" });
            seed.SaveChanges();
        }
        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(Arg.Any<string>(), Arg.Any<string>()).Returns("/entities");

        // Act
        var cut = RenderComponent<EntityEditorPage>(parameters =>
        {
            parameters.Add(p => p.DbContext, nameof(TestDbContext));
            parameters.Add(p => p.DbSet, nameof(TestEntity));
            parameters.Add(p => p.EntityId, "1");
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Instance.Should().NotBeNull();
        cut.Instance.DbContext.Should().Be(nameof(TestDbContext));
        cut.Instance.DbSet.Should().Be(nameof(TestEntity));
        cut.Instance.EntityId.Should().Be("1");
    }

    [Fact]
    public void EntityDeletionPage_ShouldRender_WithValidParameters()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(EntityDeletionPage_ShouldRender_WithValidParameters));
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 1, Name = "Test" });
            seed.SaveChanges();
        }
        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(Arg.Any<string>(), Arg.Any<string>()).Returns("/entities");

        // Act
        var cut = RenderComponent<EntityDeletionPage>(parameters =>
        {
            parameters.Add(p => p.DbContext, nameof(TestDbContext));
            parameters.Add(p => p.DbSet, nameof(TestEntity));
            parameters.Add(p => p.EntityId, "1");
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Instance.Should().NotBeNull();
        cut.Instance.DbContext.Should().Be(nameof(TestDbContext));
        cut.Instance.DbSet.Should().Be(nameof(TestEntity));
        cut.Instance.EntityId.Should().Be("1");
    }
}
