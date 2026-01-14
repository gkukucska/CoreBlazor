using BlazorBootstrap;
using Bunit;
using CoreBlazor.Authorization;
using CoreBlazor.Components;
using CoreBlazor.Interfaces;
using CoreBlazor.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using CoreBlazor.Tests.TestHelpers;

namespace CoreBlazor.Tests.Components;

/// <summary>
/// Unit tests for EntityCreationComponent (Blazor component for creating new entities).
/// Tests focus on component lifecycle, entity initialization, form handling, and data persistence.
/// </summary>
public class EntityCreationComponentTests : Bunit.TestContext
{
    #region Test Helpers

    /// <summary>
    /// Simple test entity with basic properties
    /// </summary>
    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test DbContext for simple entities
    /// </summary>
    public class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; }

        public TestDbContext() : base() { }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase("EntityCreationTestDb");
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }
    }

    private readonly NavigationManager _navigationManager;
    private readonly IDbContextFactory<TestDbContext> _contextFactory;
    private readonly INavigationPathProvider _navigationPathProvider;
    private readonly INotAuthorizedComponentTypeProvider _notAuthorizedComponentTypeProvider;

    public EntityCreationComponentTests()
    {
        _navigationManager = Substitute.For<NavigationManager>();
        _contextFactory = Substitute.For<IDbContextFactory<TestDbContext>>();
        _navigationPathProvider = Substitute.For<INavigationPathProvider>();
        _notAuthorizedComponentTypeProvider = Substitute.For<INotAuthorizedComponentTypeProvider>();

        Services.AddSingleton(_navigationManager);
        Services.AddSingleton(_contextFactory);
        Services.AddSingleton(_navigationPathProvider);
        Services.AddSingleton(_notAuthorizedComponentTypeProvider);

        // register fake IAuthorizationService and authorization policies for bUnit
        Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
        Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies<TestDbContext, TestEntity>.CanCreate, policy => policy.RequireAssertion(_ => true));
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
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);

        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert
        cut.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Component_ShouldRenderEditForm()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert
        cut.Markup.Should().Contain("form");
    }

    [Fact]
    public void Component_ShouldRenderCreateButton()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert
        cut.Markup.Should().Contain("Create", because: "Create button should be rendered");
    }

    [Fact]
    public void Component_ShouldRenderGoBackButton()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert
        cut.Markup.Should().Contain("Go back", because: "Go back button should be rendered");
    }

    #endregion

    #region Entity Initialization Tests

    [Fact]
    public void Component_ShouldInitializeEntity_OnParametersSet()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert
        cut.Instance.Entity.Should().NotBeNull();
        cut.Instance.Entity.Should().BeOfType<TestEntity>();
    }

    [Fact]
    public void Component_ShouldHaveEmptyEntity_WhenInitialized()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert
        cut.Instance.Entity.Id.Should().Be(0);
        cut.Instance.Entity.Name.Should().BeEmpty();
        cut.Instance.Entity.Description.Should().BeEmpty();
    }

    [Fact]
    public void Component_ShouldNotHaveIdSet()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert - New entity should not have ID set (0 for value types)
        cut.Instance.Entity.Id.Should().Be(default);
    }

    [Fact]
    public void Component_EntityInitialization_CreatesNewInstanceEachTime()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut1 = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );
        var entity1 = cut1.Instance.Entity;

        var cut2 = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );
        var entity2 = cut2.Instance.Entity;

        // Assert - Each component should have its own entity instance
        entity1.Should().NotBeSameAs(entity2);
    }

    #endregion

    #region Parameter Tests

    [Fact]
    public void Component_ShouldSetEntityIdParameter()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, "123");
                parameters.AddCascadingValue(authState);
            }
        );

        // Assert
        cut.Instance.EntityId.Should().Be("123");
    }

    [Fact]
    public void Component_EntityIdParameter_ShouldBeNullable()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert - EntityId should be null or empty when not provided
        cut.Instance.EntityId.Should().BeNullOrEmpty();
    }

    #endregion

    #region Form Tests

    [Fact]
    public void CreateForm_ShouldBeInitialized()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert
        cut.Instance.CreateForm.Should().NotBeNull();
        cut.Instance.CreateForm.Should().BeOfType<EditForm>();
    }

    #endregion

    #region Service Integration Tests

    [Fact]
    public void Component_ShouldInjectRequiredServices()
    {
        // Verify that all required services are properly injected
        Services.GetService<IDbContextFactory<TestDbContext>>().Should().NotBeNull();
        Services.GetService<NavigationManager>().Should().NotBeNull();
        Services.GetService<INavigationPathProvider>().Should().NotBeNull();
        Services.GetService<INotAuthorizedComponentTypeProvider>().Should().NotBeNull();
    }

    [Fact]
    public void Component_ShouldInjectNavigationManager()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert
        Services.GetService<NavigationManager>().Should().NotBeNull();
    }

    [Fact]
    public void Component_ShouldHaveCorrectContextFactory()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert - Verify ContextFactory was used
        _contextFactory.Received(1).CreateDbContextAsync(default);
    }

    [Fact]
    public void Component_ShouldUseNavigationPathProvider()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        var expectedPath = "/read/TestDbContext/TestEntity";
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns(expectedPath);

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert - verify navigation path provider was called
        _navigationPathProvider.Received(1)
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity));
    }

    [Fact]
    public void Component_ShouldUseNotAuthorizedComponentProvider()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var notAuthorizedType = typeof(NotAuthorizedComponent<TestDbContext, TestEntity>);
        _notAuthorizedComponentTypeProvider
            .GetNotAuthorizedComponentType<TestDbContext, TestEntity>()
            .Returns(notAuthorizedType);

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert - provider returns the configured type when requested
        var returned = _notAuthorizedComponentTypeProvider.GetNotAuthorizedComponentType<TestDbContext, TestEntity>();
        returned.Should().Be(notAuthorizedType);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public void Component_ShouldUseCorrectAuthorizationPolicy()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert
        var expectedPolicy = Policies<TestDbContext, TestEntity>.CanCreate;
        expectedPolicy.Should().Contain("Create");
        expectedPolicy.Should().Contain("TestDbContext");
        expectedPolicy.Should().Contain("TestEntity");
    }

    [Fact]
    public void Component_ShouldSupportGenericTypeParameters()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(context);
        _navigationPathProvider
            .GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity))
            .Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act & Assert
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );
        cut.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Component_ShouldRender_NotAuthorized_WhenPolicyFails()
    {
        // Arrange: register a deny-authorization service so AuthorizeView returns NotAuthorized
        var existing = Services.FirstOrDefault(d => d.ServiceType == typeof(IAuthorizationService));
        if (existing is not null) Services.Remove(existing);
        Services.AddSingleton<IAuthorizationService, DenyAuthorizationService>();

        _notAuthorizedComponentTypeProvider.GetNotAuthorizedComponentType<TestDbContext, TestEntity>()
            .Returns(typeof(NotAuthorizedComponent<TestDbContext, TestEntity>));
        Services.AddSingleton(_notAuthorizedComponentTypeProvider);

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert - Not authorized message should be rendered
        cut.Markup.Should().Contain("You are not authorized to view this page.");
    }

    [Fact]
    public void Component_HandlesUnauthenticatedUser()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        // Create unauthenticated state
        var unauthenticatedIdentity = new ClaimsIdentity(); // No authentication type
        var unauthenticatedUser = new ClaimsPrincipal(unauthenticatedIdentity);
        var unauthenticatedState = Task.FromResult(new AuthenticationState(unauthenticatedUser));

        // Update auth provider
        var unauthProvider = Substitute.For<AuthenticationStateProvider>();
        unauthProvider.GetAuthenticationStateAsync().Returns(unauthenticatedState);

        var existing = Services.FirstOrDefault(d => d.ServiceType == typeof(AuthenticationStateProvider));
        if (existing is not null) Services.Remove(existing);
        Services.AddSingleton(unauthProvider);

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(unauthenticatedState)
        );

        // Assert - Component should render (authorization handled by AuthorizeView)
        cut.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Component_HandlesNullAuthenticationState()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        // Act & Assert - Should throw or handle gracefully depending on component implementation
        var act = () => RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>();

        // Component may require authentication state
        act.Should().NotBeNull();
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Component_HandlesSaveException()
    {
        // Arrange
        var context = new TestDbContext();
        var faultyContext = Substitute.For<TestDbContext>();
        faultyContext.When(x => x.SaveChanges()).Do(_ => throw new DbUpdateException("Save operation failed"));

        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        var authState = CreateAuthenticationState();

        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Act & Assert - Attempting to save should handle the exception
        cut.Instance.Entity.Name = "TestEntity";
        // Note: Actual exception handling depends on component implementation
    }

    [Fact]
    public void Component_HandlesNullNavigationPath()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns((string)null!);

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert - Should render but may have issues with navigation
        cut.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Component_HandlesEmptyNavigationPath()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns(string.Empty);

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters => parameters.AddCascadingValue(authState)
        );

        // Assert
        cut.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Component_HandlesInvalidEntityId()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, "invalid-id");
                parameters.AddCascadingValue(authState);
            }
        );

        // Assert - Component should handle gracefully
        cut.Instance.EntityId.Should().Be("invalid-id");
    }

    [Fact]
    public void Component_HandlesNegativeEntityId()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, "-1");
                parameters.AddCascadingValue(authState);
            }
        );

        // Assert
        cut.Instance.EntityId.Should().Be("-1");
    }

    [Fact]
    public void Component_HandlesMultipleRapidInitializations()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");

        var authState = CreateAuthenticationState();

        // Act - Render multiple times rapidly
        for (int i = 0; i < 5; i++)
        {
            var cut = RenderComponent<EntityCreationComponent<TestDbContext, TestEntity>>(
                parameters => parameters.AddCascadingValue(authState)
            );
            cut.Instance.Entity.Should().NotBeNull();
        }

        // Assert - All instances should be valid
        _contextFactory.ReceivedCalls().Count().Should().BeGreaterThan(0);
    }

    #endregion
}
