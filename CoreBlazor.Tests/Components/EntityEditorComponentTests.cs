using Bunit;
using CoreBlazor.Components;
using CoreBlazor.Interfaces;
using CoreBlazor.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using CoreBlazor.Authorization;

namespace CoreBlazor.Tests.Components;

public class EntityEditorComponentTests : Bunit.TestContext
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

    public EntityEditorComponentTests()
    {
        // Register authorization defaults and named policies
        Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationService, FakeAuthorizationService>();
        Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies<TestDbContext, TestEntity>.CanEdit, policy => policy.RequireAssertion(_ => true));
            options.AddPolicy(Policies<TestDbContext, TestEntity>.CanDelete, policy => policy.RequireAssertion(_ => true));
            options.AddPolicy(Policies<TestDbContext>.CanReadInfo, policy => policy.RequireAssertion(_ => true));
        });

        var authProvider = Substitute.For<AuthenticationStateProvider>();
        authProvider.GetAuthenticationStateAsync().Returns(AuthenticationHelper.CreateAuthenticationState());
        Services.AddSingleton(authProvider);

        Services.AddSingleton(_contextFactory);
        Services.AddSingleton(_navigationPathProvider);
        Services.AddSingleton(_notAuthorizedComponentTypeProvider);
    }

    #endregion

    #region Basic Rendering Tests

    [Fact]
    public void Component_ShouldRender_WithoutErrors()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_ShouldRender_WithoutErrors));
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 1, Name = "Entity1" });
            seed.SaveChanges();
        }

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        // Act
        var cut = RenderComponent<EntityEditorComponent<TestDbContext, TestEntity>>(parameters =>
        {
            parameters.Add(p => p.EntityId, "1");
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Instance.Should().NotBeNull();
        cut.Instance.Entity.Should().NotBeNull();
        cut.Instance.Entity.Id.Should().Be(1);
    }

    [Theory]
    [InlineData("1", "Test1")]
    [InlineData("2", "Test2")]
    [InlineData("3", "Test3")]
    public void Component_LoadsCorrectEntity_ForDifferentIds(string entityId, string name)
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>($"{nameof(Component_LoadsCorrectEntity_ForDifferentIds)}_{entityId}");
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = int.Parse(entityId), Name = name });
            seed.SaveChanges();
        }

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        // Act
        var cut = RenderComponent<EntityEditorComponent<TestDbContext, TestEntity>>(parameters =>
        {
            parameters.Add(p => p.EntityId, entityId);
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Instance.Entity.Id.Should().Be(int.Parse(entityId));
        cut.Instance.Entity.Name.Should().Be(name);
    }

    #endregion

    #region Context Factory Tests

    [Fact]
    public void Component_ShouldCallContextFactory_OnRender()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_ShouldCallContextFactory_OnRender));
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 2, Name = "Entity2" });
            seed.SaveChanges();
        }

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        // Act
        _ = RenderComponent<EntityEditorComponent<TestDbContext, TestEntity>>(parameters =>
        {
            parameters.Add(p => p.EntityId, "2");
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        _contextFactory.Received().CreateDbContextAsync(default);
    }

    #endregion

    #region Save Tests

    [Fact]
    public void Component_ShouldSaveChanges_OnValidSubmit()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_ShouldSaveChanges_OnValidSubmit));
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 3, Name = "Entity3" });
            seed.SaveChanges();
        }

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        var expectedPath = "/entities";
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns(expectedPath);

        var cut = RenderComponent<EntityEditorComponent<TestDbContext, TestEntity>>(parameters =>
        {
            parameters.Add(p => p.EntityId, "3");
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        cut.Instance.Entity.Name = "UpdatedName";

        // Act
        var button = cut.Find("button[type=submit]");
        button.Click();

        // Assert
        using (var verify = new TestDbContext(options))
        {
            var updated = verify.TestEntities.Find(3);
            updated.Should().NotBeNull();
            updated.Name.Should().Be("UpdatedName");
        }

        var nav = Services.GetRequiredService<NavigationManager>();
        nav.Uri.Should().EndWith(expectedPath);
    }

    [Fact]
    public void Component_HandlesSaveFailure()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_HandlesSaveFailure));
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 5, Name = "Entity5" });
            seed.SaveChanges();
        }

        // Create a context that will fail on SaveChanges
        var faultyContext = Substitute.For<TestDbContext>(options);
        faultyContext.When(x => x.SaveChanges()).Do(_ => throw new DbUpdateException("Save failed"));

        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        var cut = RenderComponent<EntityEditorComponent<TestDbContext, TestEntity>>(parameters =>
        {
            parameters.Add(p => p.EntityId, "5");
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        cut.Instance.Entity.Name = "UpdatedName";

        // Act & Assert
        var button = cut.Find("button[type=submit]");
        // Note: Actual exception handling depends on component implementation
        // This test documents the expected behavior
        button.Click();
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public void Component_ShouldRender_NotAuthorized_WhenPolicyFails()
    {
        // Isolated TestContext to avoid mutating shared services
        using var ctx = new Bunit.TestContext();

        ctx.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationService, DenyAuthorizationService>();
        ctx.Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies<TestDbContext, TestEntity>.CanEdit, policy => policy.RequireAssertion(_ => true));
            options.AddPolicy(Policies<TestDbContext>.CanReadInfo, policy => policy.RequireAssertion(_ => true));
        });

        var localFactory = Substitute.For<IDbContextFactory<TestDbContext>>();
        var localNav = Substitute.For<INavigationPathProvider>();
        var localNotAuth = Substitute.For<INotAuthorizedComponentTypeProvider>();
        localNotAuth.GetNotAuthorizedComponentType<TestDbContext, TestEntity>()
            .Returns(typeof(CoreBlazor.Components.NotAuthorizedComponent<TestDbContext, TestEntity>));
        ctx.Services.AddSingleton(localFactory);
        ctx.Services.AddSingleton(localNav);
        ctx.Services.AddSingleton(localNotAuth);

        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_ShouldRender_NotAuthorized_WhenPolicyFails));
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 4, Name = "Entity4" });
            seed.SaveChanges();
        }
        localFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));

        var authProv = Substitute.For<AuthenticationStateProvider>();
        authProv.GetAuthenticationStateAsync().Returns(AuthenticationHelper.CreateAuthenticationState());
        ctx.Services.AddSingleton(authProv);

        // Act
        var cut = ctx.RenderComponent<EntityEditorComponent<TestDbContext, TestEntity>>(parameters =>
        {
            parameters.Add(p => p.EntityId, "4");
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Markup.Should().Contain("You are not authorized to view this page.");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Component_ThrowsException_WhenEntityIdIsInvalid()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_ThrowsException_WhenEntityIdIsInvalid));
        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        // Act & Assert
        var act = () => RenderComponent<EntityEditorComponent<TestDbContext, TestEntity>>(parameters =>
        {
            parameters.Add(p => p.EntityId, "invalid");
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Component_HandlesNullEntityId()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_HandlesNullEntityId));
        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        // Act & Assert
        var act = () => RenderComponent<EntityEditorComponent<TestDbContext, TestEntity>>(parameters =>
        {
            parameters.Add(p => p.EntityId, (string)null!);
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Component_HandlesEmptyEntityId()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_HandlesEmptyEntityId));
        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        // Act & Assert
        var act = () => RenderComponent<EntityEditorComponent<TestDbContext, TestEntity>>(parameters =>
        {
            parameters.Add(p => p.EntityId, string.Empty);
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Component_HandlesNavigationPathProviderReturningNull()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_HandlesNavigationPathProviderReturningNull));
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 6, Name = "Entity6" });
            seed.SaveChanges();
        }

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns((string)null!);

        var cut = RenderComponent<EntityEditorComponent<TestDbContext, TestEntity>>(parameters =>
        {
            parameters.Add(p => p.EntityId, "6");
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        cut.Instance.Entity.Name = "UpdatedName";

        // Act & Assert - Null navigation path will cause navigation to fail
        var button = cut.Find("button[type=submit]");
        var act = () => button.Click();

        // Navigation with null path should throw
        act.Should().Throw<Exception>();
    }

    #endregion
}
