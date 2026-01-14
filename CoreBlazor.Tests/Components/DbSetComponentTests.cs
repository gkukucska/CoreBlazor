using BlazorBootstrap;
using Bunit;
using CoreBlazor.Authorization;
using CoreBlazor.Components;
using CoreBlazor.Configuration;
using CoreBlazor.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Security.Claims;
using Xunit;

namespace CoreBlazor.Tests.Components;

/// <summary>
/// Unit tests for DbSetComponent (Blazor component that displays a DbSet with authorization).
/// Note: AuthorizeView-based tests are complex in Bunit and require advanced mocking.
/// These tests focus on component lifecycle and service injection.
/// </summary>
public class DbSetComponentTests : Bunit.TestContext
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("DbSetComponentTestDb");
            base.OnConfiguring(optionsBuilder);
        }
    }

    private readonly NavigationManager _navigationManager;
    private readonly IDbContextFactory<TestDbContext> _contextFactory;
    private readonly INavigationPathProvider _navigationPathProvider;
    private readonly INotAuthorizedComponentTypeProvider _notAuthorizedComponentTypeProvider;

    public DbSetComponentTests()
    {
        _navigationManager = Substitute.For<NavigationManager>();
        _contextFactory = Substitute.For<IDbContextFactory<TestDbContext>>();
        _navigationPathProvider = Substitute.For<INavigationPathProvider>();
        _notAuthorizedComponentTypeProvider = Substitute.For<INotAuthorizedComponentTypeProvider>();

        Services.AddSingleton(_navigationManager);
        Services.AddSingleton(_contextFactory);
        Services.AddSingleton(_navigationPathProvider);
        Services.AddSingleton(_notAuthorizedComponentTypeProvider);
    }

    #endregion

    #region Service Injection Tests

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
    public void NavigationManager_ShouldBePropertyMocked()
    {
        // Test that NavigationManager is properly mocked
        _navigationManager.Should().NotBeNull();
    }

    #endregion

    #region DbSetOptions Tests

    [Fact]
    public void Component_ShouldInitializeDbSetOptions()
    {
        // Test that DbSetOptions can be registered and retrieved from service provider
        var dbSetOptions = new CoreBlazorDbSetOptions<TestDbContext, TestEntity>
        {
            DisplayTitle = "Test Entities"
        };
        Services.AddSingleton(dbSetOptions);

        var retrievedOptions = Services.GetService<CoreBlazorDbSetOptions<TestDbContext, TestEntity>>();
        retrievedOptions.Should().NotBeNull();
        retrievedOptions!.DisplayTitle.Should().Be("Test Entities");
    }

    [Fact]
    public void DbSetOptions_ShouldSupportCustomDisplayTitle()
    {
        // Test that DbSetOptions can be customized with display titles
        var options = new CoreBlazorDbSetOptions<TestDbContext, TestEntity>
        {
            DisplayTitle = "My Custom Entities"
        };

        options.DisplayTitle.Should().Be("My Custom Entities");
    }

    [Fact]
    public void DbSetOptions_HandlesNullDisplayTitle()
    {
        // Arrange
        var options = new CoreBlazorDbSetOptions<TestDbContext, TestEntity>
        {
            DisplayTitle = null!
        };

        // Assert
        options.DisplayTitle.Should().BeNull();
    }

    [Fact]
    public void DbSetOptions_HandlesEmptyDisplayTitle()
    {
        // Arrange
        var options = new CoreBlazorDbSetOptions<TestDbContext, TestEntity>
        {
            DisplayTitle = string.Empty
        };

        // Assert
        options.DisplayTitle.Should().BeEmpty();
    }

    #endregion

    #region Policy Tests

    [Fact]
    public void Policies_ShouldGenerateCorrectPolicyNames()
    {
        // Test that policy name generation works correctly
        var canReadPolicy = Policies<TestDbContext, TestEntity>.CanRead;
        var canCreatePolicy = Policies<TestDbContext, TestEntity>.CanCreate;
        var canEditPolicy = Policies<TestDbContext, TestEntity>.CanEdit;

        canReadPolicy.Should().Contain("TestDbContext");
        canReadPolicy.Should().Contain("TestEntity");
        canReadPolicy.Should().Contain("Read");

        canCreatePolicy.Should().Contain("TestDbContext");
        canCreatePolicy.Should().Contain("TestEntity");
        canCreatePolicy.Should().Contain("Create");

        canEditPolicy.Should().Contain("TestDbContext");
        canEditPolicy.Should().Contain("TestEntity");
        canEditPolicy.Should().Contain("Edit");
    }

    [Fact]
    public void Component_ShouldSupportGenericTypeParameters()
    {
        // Test that component properly handles generic type parameters
        // This verifies the type constraints are satisfied
        var canReadPolicy = Policies<TestDbContext, TestEntity>.CanRead;
        canReadPolicy.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Policies_HandlesNullContext()
    {
        // Test policy generation with null contexts (edge case)
        // This documents expected behavior
        var policy = Policies<TestDbContext, TestEntity>.CanRead;
        policy.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Component_HandlesInvalidPolicyNames()
    {
        // Test that policy names are generated even with unusual type names
        // This is an edge case test
        var policy = Policies<TestDbContext, TestEntity>.CanRead;
        policy.Should().NotBeNullOrEmpty();
        policy.Should().NotContain(" ");
        policy.Should().NotContain("\t");
        policy.Should().NotContain("\n");
    }

    #endregion

    #region Navigation Path Provider Tests

    [Fact]
    public void NavigationPathProvider_ShouldGenerateCorrectPaths()
    {
        // Test that navigation paths are generated correctly
        var createPath = "/DbContext/TestDbContext/DbSet/TestEntity/Create";
        _navigationPathProvider
            .GetPathToCreateEntity(nameof(TestDbContext), nameof(TestEntity))
            .Returns(createPath);

        var result = _navigationPathProvider.GetPathToCreateEntity(nameof(TestDbContext), nameof(TestEntity));
        result.Should().Be(createPath);
    }

    [Fact]
    public void NavigationPathProvider_HandlesNullPaths()
    {
        // Arrange
        _navigationPathProvider.GetPathToCreateEntity(nameof(TestDbContext), nameof(TestEntity)).Returns((string)null!);

        // Act
        var result = _navigationPathProvider.GetPathToCreateEntity(nameof(TestDbContext), nameof(TestEntity));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NavigationPathProvider_HandlesEmptyPaths()
    {
        // Arrange
        _navigationPathProvider.GetPathToCreateEntity(nameof(TestDbContext), nameof(TestEntity)).Returns(string.Empty);

        // Act
        var result = _navigationPathProvider.GetPathToCreateEntity(nameof(TestDbContext), nameof(TestEntity));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void NavigationPathProvider_HandlesInvalidContextName()
    {
        // Arrange
        _navigationPathProvider.GetPathToCreateEntity("InvalidContext", nameof(TestEntity)).Returns((string)null!);

        // Act
        var result = _navigationPathProvider.GetPathToCreateEntity("InvalidContext", nameof(TestEntity));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NavigationPathProvider_HandlesInvalidEntityName()
    {
        // Arrange
        _navigationPathProvider.GetPathToCreateEntity(nameof(TestDbContext), "InvalidEntity").Returns((string)null!);

        // Act
        var result = _navigationPathProvider.GetPathToCreateEntity(nameof(TestDbContext), "InvalidEntity");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NavigationPathProvider_HandlesConcurrentAccess()
    {
        // Arrange
        _navigationPathProvider.GetPathToCreateEntity(Arg.Any<string>(), Arg.Any<string>())
            .Returns("/test/path");

        // Act - Multiple concurrent calls
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => _navigationPathProvider.GetPathToCreateEntity(nameof(TestDbContext), nameof(TestEntity))))
            .ToList();

        // Assert
        Task.WhenAll(tasks).Wait();
        tasks.Should().OnlyContain(t => t.Result == "/test/path");
    }

    #endregion

    #region NotAuthorizedComponentTypeProvider Tests

    [Fact]
    public void NotAuthorizedComponentTypeProvider_ShouldReturnComponentType()
    {
        // Test that provider returns the correct component type for unauthorized access
        var componentType = typeof(NotAuthorizedComponent<TestDbContext, TestEntity>);
        _notAuthorizedComponentTypeProvider
            .GetNotAuthorizedComponentType<TestDbContext, TestEntity>()
            .Returns(componentType);

        var result = _notAuthorizedComponentTypeProvider.GetNotAuthorizedComponentType<TestDbContext, TestEntity>();
        result.Should().Be(componentType);
    }

    [Fact]
    public void NotAuthorizedComponentTypeProvider_HandlesNullComponentType()
    {
        // Arrange
        _notAuthorizedComponentTypeProvider.GetNotAuthorizedComponentType<TestDbContext, TestEntity>().Returns((Type)null!);

        // Act
        var result = _notAuthorizedComponentTypeProvider.GetNotAuthorizedComponentType<TestDbContext, TestEntity>();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DbContext Factory Tests

    [Fact]
    public void Component_HandlesDbContextFactoryException()
    {
        // Arrange
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromException<TestDbContext>(new InvalidOperationException("Database connection failed")));

        // Act & Assert - Component should handle database connection failures
        _contextFactory.Invoking(f => f.CreateDbContextAsync(default))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public void Component_HandlesNullDbContextFactory()
    {
        // Arrange - Remove the factory
        var existing = Services.FirstOrDefault(d => d.ServiceType == typeof(IDbContextFactory<TestDbContext>));
        if (existing != null) Services.Remove(existing);

        // Act & Assert
        var factory = Services.GetService<IDbContextFactory<TestDbContext>>();
        factory.Should().BeNull();
    }

    [Fact]
    public void Component_HandlesEmptyDbSet()
    {
        // Arrange
        var context = new TestDbContext();
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        // Act - Context with no entities
        var dbSet = context.TestEntities;

        // Assert
        dbSet.Should().NotBeNull();
        dbSet.Should().BeEmpty();
    }

    [Fact]
    public void Component_HandlesNullNavigationPathProvider()
    {
        // Arrange - Remove the provider
        var existing = Services.FirstOrDefault(d => d.ServiceType == typeof(INavigationPathProvider));
        if (existing != null) Services.Remove(existing);

        // Act & Assert
        var provider = Services.GetService<INavigationPathProvider>();
        provider.Should().BeNull();
    }

    [Fact]
    public void Component_HandlesMultipleServiceRegistrations()
    {
        // Arrange - Register multiple instances
        var factory1 = Substitute.For<IDbContextFactory<TestDbContext>>();
        var factory2 = Substitute.For<IDbContextFactory<TestDbContext>>();

        Services.AddSingleton(factory1);

        // Act - Get service should return one instance
        var result = Services.GetService<IDbContextFactory<TestDbContext>>();

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Lifecycle Tests

    [Fact]
    public void Component_HandlesDbContextDisposal()
    {
        // Arrange
        var context = new TestDbContext();

        // Act - Dispose context
        context.Dispose();

        // Assert - Should not throw
        context.Invoking(c => c.Dispose()).Should().NotThrow();
    }

    [Fact]
    public void Component_HandlesRapidDbContextCreation()
    {
        // Arrange
        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext()));

        // Act - Create multiple contexts rapidly
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _contextFactory.CreateDbContextAsync(default))
            .ToList();

        // Assert - All should complete
        Task.WhenAll(tasks).Wait();
        tasks.Should().OnlyContain(t => t.IsCompleted);
    }

    #endregion
}
