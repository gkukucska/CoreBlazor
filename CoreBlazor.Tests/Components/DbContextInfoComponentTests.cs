using Bunit;
using CoreBlazor.Authorization;
using CoreBlazor.Components;
using CoreBlazor.Interfaces;
using CoreBlazor.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace CoreBlazor.Tests.Components;

public class DbContextInfoComponentTests : Bunit.TestContext
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    }

    private readonly IDbContextFactory<TestDbContext> _contextFactory = Substitute.For<IDbContextFactory<TestDbContext>>();
    private readonly INotAuthorizedComponentTypeProvider _notAuthorizedProvider = Substitute.For<INotAuthorizedComponentTypeProvider>();

    public DbContextInfoComponentTests()
    {
        Services.AddSingleton(_contextFactory);
        Services.AddSingleton(_notAuthorizedProvider);
        Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationService, FakeAuthorizationService>();
        Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies<TestDbContext>.CanReadInfo, policy => policy.RequireAssertion(_ => true));
        });

        var authProvider = Substitute.For<AuthenticationStateProvider>();
        authProvider.GetAuthenticationStateAsync().Returns(AuthenticationHelper.CreateAuthenticationState());
        Services.AddSingleton(authProvider);

        _notAuthorizedProvider.GetNotAuthorizedComponentType<TestDbContext, object>()
            .Returns(typeof(NotAuthorizedComponent<TestDbContext, object>));
    }

    [Fact]
    public void Component_ShouldRender_WithContextName()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_ShouldRender_WithContextName));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        // Act
        var cut = RenderComponent<DbContextInfoComponent<TestDbContext>>(parameters =>
        {
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Markup.Should().Contain("TestDbContext");
        cut.Instance.ContextName.Should().Be("TestDbContext");
    }

    [Fact]
    public void Component_ShouldDisplay_Provider()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_ShouldDisplay_Provider));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        // Act
        var cut = RenderComponent<DbContextInfoComponent<TestDbContext>>(parameters =>
        {
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Instance.Provider.Should().NotBeNullOrEmpty();
        cut.Markup.Should().Contain("Provider");
    }

    [Fact]
    public void Component_CallsContextFactory_OnParametersSet()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_CallsContextFactory_OnParametersSet));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        // Act
        var cut = RenderComponent<DbContextInfoComponent<TestDbContext>>(parameters =>
        {
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        _contextFactory.Received(1).CreateDbContextAsync(default);
    }

    [Fact]
    public void Component_RendersTable_WithProperStructure()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_RendersTable_WithProperStructure));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        // Act
        var cut = RenderComponent<DbContextInfoComponent<TestDbContext>>(parameters =>
        {
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Markup.Should().Contain("<table");
        cut.Markup.Should().Contain("table-hover");
        cut.Markup.Should().Contain("table-bordered");
        cut.Markup.Should().Contain("table-striped");
        cut.Markup.Should().Contain("<tbody>");
    }

    [Fact]
    public void Component_ShowsNotAuthorized_WhenPolicyFails()
    {
        // Isolated test context
        using var ctx = new Bunit.TestContext();

        ctx.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationService, DenyAuthorizationService>();
        ctx.Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies<TestDbContext>.CanReadInfo, policy => policy.RequireAssertion(_ => false));
        });

        var localFactory = Substitute.For<IDbContextFactory<TestDbContext>>();
        var localNotAuth = Substitute.For<INotAuthorizedComponentTypeProvider>();
        localNotAuth.GetNotAuthorizedComponentType<TestDbContext, object>()
            .Returns(typeof(NotAuthorizedComponent<TestDbContext, object>));

        ctx.Services.AddSingleton(localFactory);
        ctx.Services.AddSingleton(localNotAuth);

        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_ShowsNotAuthorized_WhenPolicyFails));
        var context = new TestDbContext(options);
        localFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        var authProv = Substitute.For<AuthenticationStateProvider>();
        authProv.GetAuthenticationStateAsync().Returns(AuthenticationHelper.CreateAuthenticationState());
        ctx.Services.AddSingleton(authProv);

        // Act
        var cut = ctx.RenderComponent<DbContextInfoComponent<TestDbContext>>(parameters =>
        {
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Markup.Should().Contain("You are not authorized to view this page.");
    }

    [Fact]
    public void Component_PropertiesSet_AfterInitialization()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_PropertiesSet_AfterInitialization));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        // Act
        var cut = RenderComponent<DbContextInfoComponent<TestDbContext>>(parameters =>
        {
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Instance.ContextName.Should().NotBeNullOrEmpty();
        cut.Instance.Provider.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Component_DoesNotShowDatabaseInfo_ForInMemoryProvider()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_DoesNotShowDatabaseInfo_ForInMemoryProvider));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        // Act
        var cut = RenderComponent<DbContextInfoComponent<TestDbContext>>(parameters =>
        {
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert - InMemory provider is not relational, so Database and Source should be null
        cut.Instance.DataBase.Should().BeNull();
        cut.Instance.Source.Should().BeNull();
    }

    [Fact]
    public void Component_ShowsContextNameRow_Always()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_ShowsContextNameRow_Always));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        // Act
        var cut = RenderComponent<DbContextInfoComponent<TestDbContext>>(parameters =>
        {
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Markup.Should().Contain("ContextName");
        cut.Find("th").TextContent.Should().Be("ContextName");
    }

    [Fact]
    public void Component_ShowsProviderRow_WhenProviderIsSet()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_ShowsProviderRow_WhenProviderIsSet));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        // Act
        var cut = RenderComponent<DbContextInfoComponent<TestDbContext>>(parameters =>
        {
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        cut.Instance.Provider.Should().NotBeNullOrEmpty();
        var rows = cut.FindAll("tr");
        rows.Count.Should().BeGreaterThanOrEqualTo(2); // At least ContextName and Provider rows
    }

    [Fact]
    public void Component_UsesTableHeaders_ForLabels()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_UsesTableHeaders_ForLabels));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        // Act
        var cut = RenderComponent<DbContextInfoComponent<TestDbContext>>(parameters =>
        {
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        var headers = cut.FindAll("th");
        headers.Should().NotBeEmpty();
        headers[0].TextContent.Trim().Should().Be("ContextName");
    }

    [Fact]
    public void Component_UsesTableData_ForValues()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_UsesTableData_ForValues));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        // Act
        var cut = RenderComponent<DbContextInfoComponent<TestDbContext>>(parameters =>
        {
            parameters.AddCascadingValue(AuthenticationHelper.CreateAuthenticationState());
        });

        // Assert
        var dataCells = cut.FindAll("td");
        dataCells.Should().NotBeEmpty();
        dataCells[0].TextContent.Trim().Should().Be("TestDbContext");
    }
}
