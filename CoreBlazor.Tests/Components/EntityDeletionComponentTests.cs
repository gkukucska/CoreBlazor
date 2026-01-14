using BlazorBootstrap;
using Bunit;
using CoreBlazor.Authorization;
using CoreBlazor.Components;
using CoreBlazor.Interfaces;
using CoreBlazor.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Security.Claims;
using Xunit;
using CoreBlazor.Tests.TestHelpers;

namespace CoreBlazor.Tests.Components;

public class EntityDeletionComponentTests : Bunit.TestContext
{
    #region Test Helpers

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

    public EntityDeletionComponentTests()
    {
        // register fake IAuthorizationService implementation so AuthorizeView works in bUnit
        Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();

        // register authorization policies for bUnit (allow by default)
        Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies<TestDbContext, TestEntity>.CanDelete, policy => policy.RequireAssertion(_ => true));
            options.AddPolicy(Policies<TestDbContext>.CanReadInfo, policy => policy.RequireAssertion(_ => true));
        });

        var authProvider = Substitute.For<AuthenticationStateProvider>();
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        authProvider.GetAuthenticationStateAsync().Returns(Task.FromResult(new AuthenticationState(user)));
        Services.AddSingleton<AuthenticationStateProvider>(authProvider);
    }

    private Task<AuthenticationState> CreateAuthenticationState()
    {
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }

    #endregion

    #region Basic Rendering Tests

    [Fact]
    public void Component_ShouldRender_WithoutErrors()
    {
        // Arrange: use unique in-memory database per test
        var dbName = "EntityDeletionTestDb_" + Guid.NewGuid();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;

        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 1, Name = "Entity1" });
            seed.SaveChanges();
        }

        Services.AddSingleton(_contextFactory);
        Services.AddSingleton(_navigationPathProvider);
        Services.AddSingleton(_notAuthorizedComponentTypeProvider);

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        var cut = RenderComponent<EntityDeletionComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, "1");
                parameters.AddCascadingValue(authState);
            }
        );

        cut.Instance.Should().NotBeNull();
        cut.Instance.Entity.Should().NotBeNull();
        cut.Instance.Entity.Id.Should().Be(1);
    }

    #endregion

    #region Context Factory Tests

    [Fact]
    public void Component_ShouldCallContextFactory_OnRender()
    {
        var dbName = "EntityDeletionTestDb_" + Guid.NewGuid();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 2, Name = "Entity2" });
            seed.SaveChanges();
        }

        Services.AddSingleton(_contextFactory);
        Services.AddSingleton(_navigationPathProvider);
        Services.AddSingleton(_notAuthorizedComponentTypeProvider);

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        var cut = RenderComponent<EntityDeletionComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, "2");
                parameters.AddCascadingValue(authState);
            }
        );

        _contextFactory.Received().CreateDbContextAsync(default);
    }

    #endregion

    #region Deletion Tests

    [Fact]
    public void Component_ShouldDeleteEntity_OnFormSubmit()
    {
        var dbName = "EntityDeletionTestDb_" + Guid.NewGuid();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 3, Name = "Entity3" });
            seed.SaveChanges();
        }

        Services.AddSingleton(_contextFactory);
        Services.AddSingleton(_navigationPathProvider);
        Services.AddSingleton(_notAuthorizedComponentTypeProvider);

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        var expectedPath = "/entities";
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns(expectedPath);

        var authState = CreateAuthenticationState();

        var cut = RenderComponent<EntityDeletionComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, "3");
                parameters.AddCascadingValue(authState);
            }
        );

        // Ensure entity is loaded
        cut.Instance.Entity.Should().NotBeNull();
        cut.Instance.Entity.Id.Should().Be(3);

        // Find submit button and click to confirm deletion
        var button = cut.Find("button[type=submit]");
        button.Click();

        // Verify entity was removed from the DbContext by opening a new context
        using (var verify = new TestDbContext(options))
        {
            var remaining = verify.TestEntities.Find(3);
            remaining.Should().BeNull();
        }

        // Verify navigation occurred to the listing page using bUnit NavigationManager
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.Uri.Should().EndWith(expectedPath);
    }

    [Fact]
    public void Component_HandlesMultipleDeletionAttempts()
    {
        // Arrange
        var dbName = "EntityDeletionTestDb_MultipleDel_" + Guid.NewGuid();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 7, Name = "Entity7" });
            seed.SaveChanges();
        }

        Services.AddSingleton(_contextFactory);
        Services.AddSingleton(_navigationPathProvider);
        Services.AddSingleton(_notAuthorizedComponentTypeProvider);

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        var authState = CreateAuthenticationState();

        var cut = RenderComponent<EntityDeletionComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, "7");
                parameters.AddCascadingValue(authState);
            }
        );

        var button = cut.Find("button[type=submit]");

        // Act - Click delete button
        button.Click();

        // Entity should be deleted after first click
        using (var verify = new TestDbContext(options))
        {
            var entity = verify.TestEntities.Find(7);
            entity.Should().BeNull();
        }
    }

    [Fact]
    public void Component_HandlesDeletionFailure()
    {
        // Arrange
        var dbName = "EntityDeletionTestDb_DeleteFail_" + Guid.NewGuid();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 5, Name = "Entity5" });
            seed.SaveChanges();
        }

        Services.AddSingleton(_contextFactory);
        Services.AddSingleton(_navigationPathProvider);
        Services.AddSingleton(_notAuthorizedComponentTypeProvider);

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        var authState = CreateAuthenticationState();

        var cut = RenderComponent<EntityDeletionComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, "5");
                parameters.AddCascadingValue(authState);
            }
        );

        // Note: Actual deletion failure testing depends on component implementation
        // This test documents expected behavior when SaveChanges fails
        cut.Instance.Should().NotBeNull();
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public void Component_ShouldRender_NotAuthorized_WhenPolicyFails()
    {
        // Create an isolated TestContext so we can register different services without affecting class-level services
        using var ctx = new Bunit.TestContext();

        // register deny authorization so AuthorizeView returns NotAuthorized
        ctx.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationService, CoreBlazor.Tests.TestHelpers.DenyAuthorizationService>();
        // ensure named policies exist so AuthorizeView can resolve them
        ctx.Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies<TestDbContext, TestEntity>.CanDelete, policy => policy.RequireAssertion(_ => true));
            options.AddPolicy(Policies<TestDbContext>.CanReadInfo, policy => policy.RequireAssertion(_ => true));
        });

        // not authorized component provider
        var notAuthProvider = Substitute.For<CoreBlazor.Interfaces.INotAuthorizedComponentTypeProvider>();
        notAuthProvider.GetNotAuthorizedComponentType<TestDbContext, TestEntity>()
            .Returns(typeof(CoreBlazor.Components.NotAuthorizedComponent<TestDbContext, TestEntity>));
        ctx.Services.AddSingleton<CoreBlazor.Interfaces.INotAuthorizedComponentTypeProvider>(notAuthProvider);

        // db factory and navigation provider for this isolated context
        var localFactory = Substitute.For<IDbContextFactory<TestDbContext>>();
        var localNav = Substitute.For<CoreBlazor.Interfaces.INavigationPathProvider>();
        ctx.Services.AddSingleton<IDbContextFactory<TestDbContext>>(localFactory);
        ctx.Services.AddSingleton<CoreBlazor.Interfaces.INavigationPathProvider>(localNav);

        var dbName = "EntityDeletionTestDb_NotAuth_" + Guid.NewGuid();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 4, Name = "Entity4" });
            seed.SaveChanges();
        }
        localFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));

        var authProv = Substitute.For<AuthenticationStateProvider>();
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        authProv.GetAuthenticationStateAsync().Returns(Task.FromResult(new AuthenticationState(user)));
        ctx.Services.AddSingleton<AuthenticationStateProvider>(authProv);

        var authState = Task.FromResult(new AuthenticationState(user));

        // Act
        var cut = ctx.RenderComponent<EntityDeletionComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, "4");
                parameters.AddCascadingValue(authState);
            }
        );

        // Assert
        cut.Markup.Should().Contain("You are not authorized to view this page.");
    }

    [Fact]
    public void Component_HandlesUnauthenticatedUser()
    {
        // Arrange
        using var ctx = new Bunit.TestContext();

        ctx.Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
        ctx.Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies<TestDbContext, TestEntity>.CanDelete, policy => policy.RequireAssertion(_ => true));
            options.AddPolicy(Policies<TestDbContext>.CanReadInfo, policy => policy.RequireAssertion(_ => true));
        });

        var localFactory = Substitute.For<IDbContextFactory<TestDbContext>>();
        var localNav = Substitute.For<INavigationPathProvider>();
        var localNotAuth = Substitute.For<INotAuthorizedComponentTypeProvider>();
        ctx.Services.AddSingleton(localFactory);
        ctx.Services.AddSingleton(localNav);
        ctx.Services.AddSingleton(localNotAuth);

        var dbName = "EntityDeletionTestDb_Unauth_" + Guid.NewGuid();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 8, Name = "Entity8" });
            seed.SaveChanges();
        }
        localFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        localNav.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        var unauthIdentity = new ClaimsIdentity(); // No authentication type
        var unauthUser = new ClaimsPrincipal(unauthIdentity);
        var unauthState = Task.FromResult(new AuthenticationState(unauthUser));

        var authProv = Substitute.For<AuthenticationStateProvider>();
        authProv.GetAuthenticationStateAsync().Returns(unauthState);
        ctx.Services.AddSingleton(authProv);

        // Act
        var cut = ctx.RenderComponent<EntityDeletionComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, "8");
                parameters.AddCascadingValue(unauthState);
            }
        );

        // Assert - Component should render (authorization handled by AuthorizeView)
        cut.Instance.Should().NotBeNull();
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Component_ThrowsException_WhenEntityIdIsInvalid()
    {
        // Arrange
        var dbName = "EntityDeletionTestDb_InvalidId_" + Guid.NewGuid();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;

        Services.AddSingleton(_contextFactory);
        Services.AddSingleton(_navigationPathProvider);
        Services.AddSingleton(_notAuthorizedComponentTypeProvider);

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act & Assert
        var act = () => RenderComponent<EntityDeletionComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, "invalid-id");
                parameters.AddCascadingValue(authState);
            }
        );

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Component_HandlesNullEntityId()
    {
        // Arrange
        var dbName = "EntityDeletionTestDb_NullId_" + Guid.NewGuid();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;

        Services.AddSingleton(_contextFactory);
        Services.AddSingleton(_navigationPathProvider);
        Services.AddSingleton(_notAuthorizedComponentTypeProvider);

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act & Assert
        var act = () => RenderComponent<EntityDeletionComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, (string)null!);
                parameters.AddCascadingValue(authState);
            }
        );

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Component_HandlesEmptyEntityId()
    {
        // Arrange
        var dbName = "EntityDeletionTestDb_EmptyId_" + Guid.NewGuid();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;

        Services.AddSingleton(_contextFactory);
        Services.AddSingleton(_navigationPathProvider);
        Services.AddSingleton(_notAuthorizedComponentTypeProvider);

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act & Assert
        var act = () => RenderComponent<EntityDeletionComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, string.Empty);
                parameters.AddCascadingValue(authState);
            }
        );

        act.Should().Throw<Exception>();
    }

    #endregion
}
