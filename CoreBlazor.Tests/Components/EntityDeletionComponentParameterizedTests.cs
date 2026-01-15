using CoreBlazor.Authorization;
using CoreBlazor.Components;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using System.Security.Claims;

namespace CoreBlazor.Tests.Components;

public class EntityDeletionComponentParameterizedTests : Bunit.TestContext
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

    private readonly CoreBlazor.Interfaces.INavigationPathProvider _navigationPathProvider = Substitute.For<CoreBlazor.Interfaces.INavigationPathProvider>();
    private readonly CoreBlazor.Interfaces.INotAuthorizedComponentTypeProvider _notAuthorized_component_type_provider = Substitute.For<CoreBlazor.Interfaces.INotAuthorizedComponentTypeProvider>();
    private readonly IDbContextFactory<TestDbContext> _contextFactory = Substitute.For<IDbContextFactory<TestDbContext>>();

    public EntityDeletionComponentParameterizedTests()
    {
        // default allow policies
        Services.AddAuthorizationCore();

        var authProvider = Substitute.For<AuthenticationStateProvider>();
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        authProvider.GetAuthenticationStateAsync().Returns(Task.FromResult(new AuthenticationState(user)));
        Services.AddSingleton<AuthenticationStateProvider>(authProvider);

        Services.AddSingleton<CoreBlazor.Interfaces.INavigationPathProvider>(_navigationPathProvider);
        Services.AddSingleton<CoreBlazor.Interfaces.INotAuthorizedComponentTypeProvider>(_notAuthorized_component_type_provider);
        Services.AddSingleton<IDbContextFactory<TestDbContext>>(_contextFactory);
    }

    private Task<AuthenticationState> CreateAuthenticationState()
    {
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Component_NotAuthorized_Combinations(bool canDelete, bool canReadInfo)
    {
        // Arrange
        var dbName = "ParamTestDb_" + Guid.NewGuid();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;
        using (var seed = new TestDbContext(options))
        {
            seed.TestEntities.Add(new TestEntity { Id = 5, Name = "Entity5" });
            seed.SaveChanges();
        }

        // configure which policies should fail
        var failing = new List<string>();
        if (!canDelete) failing.Add(Policies<TestDbContext, TestEntity>.CanDelete);
        if (!canReadInfo) failing.Add(Policies<TestDbContext>.CanReadInfo);

        var existingAuth = Services.FirstOrDefault(d => d.ServiceType == typeof(Microsoft.AspNetCore.Authorization.IAuthorizationService));
        if (existingAuth is not null) Services.Remove(existingAuth);

        // Ensure the named policies exist in the AuthorizationOptions used by AuthorizeView
        Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies<TestDbContext, TestEntity>.CanDelete, policy => policy.RequireAssertion(_ => true));
            options.AddPolicy(Policies<TestDbContext>.CanReadInfo, policy => policy.RequireAssertion(_ => true));
        });

        if (failing.Count > 0)
        {
            Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationService, CoreBlazor.Tests.TestHelpers.DenyAuthorizationService>();
        }
        else
        {
            Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationService, CoreBlazor.Tests.TestHelpers.FakeAuthorizationService>();
        }

        _contextFactory.CreateDbContextAsync(default).Returns(ci => Task.FromResult(new TestDbContext(options)));
        _navigationPathProvider.GetPathToReadEntities(nameof(TestDbContext), nameof(TestEntity)).Returns("/entities");
        _notAuthorized_component_type_provider.GetNotAuthorizedComponentType<TestDbContext, TestEntity>()
            .Returns(typeof(CoreBlazor.Components.NotAuthorizedComponent<TestDbContext, TestEntity>));

        var authState = CreateAuthenticationState();

        // Act
        var cut = RenderComponent<EntityDeletionComponent<TestDbContext, TestEntity>>(
            parameters =>
            {
                parameters.Add(p => p.EntityId, "5");
                parameters.AddCascadingValue(authState);
            }
        );

        // Assert: if either policy fails, NotAuthorized should be shown
        if (!canDelete || !canReadInfo)
        {
            cut.Markup.Should().Contain("You are not authorized to view this page.");
        }
        else
        {
            cut.Instance.Should().NotBeNull();
            cut.Instance.Entity.Should().NotBeNull();
            cut.Instance.Entity.Id.Should().Be(5);
        }
    }
}
