using CoreBlazor.Authorization;
using CoreBlazor.Configuration;
using CoreBlazor.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace CoreBlazor.Tests.Configuration;

public class ConfigurationBuilderTests
{
    #region Test Helpers

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    private class RelatedEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<RelatedEntity> RelatedEntities { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    }

    private class TestEntityDisplayComponent : ComponentBase, IEntityDisplayComponent<TestEntity>
    {
        [Parameter]
        public TestEntity? Entity { get; set; }
    }

    private class TestPropertyDisplayComponent : ComponentBase, IPropertyDisplayComponent<string>
    {
        [Parameter]
        public string? PropertyValue { get; set; }
    }

    private class TestPropertyEditComponent : ComponentBase, IPropertyEditComponent<TestEntity>
    {
        [Parameter]
        public TestEntity? Entity { get; set; }

        [Parameter]
        public EventCallback ValueSelected { get; set; }

        [Parameter]
        public bool IsDisabled { get; set; }
    }

    #endregion

    #region CoreBlazorOptionsBuilder Tests

    [Fact]
    public void CoreBlazorOptionsBuilder_ConfigureContext_ActionOverload_RegistersOptionsSingleton()
    {
        // Arrange
        ConfigurationHelper.DisplayTitles.Clear();
        var services = new ServiceCollection();
        var contexts = Enumerable.Empty<DiscoveredContext>();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, contexts);

        // Act
        coreOptionsBuilder.ConfigureContext<TestDbContext>(ctx => ctx.WithTitle("ctx title").WithSplitQueries(true));

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetService<CoreBlazorDbContextOptions<TestDbContext>>();
        options.Should().NotBeNull();
        options!.DisplayTitle.Should().Be("ctx title");
        options.UseSplitQueries.Should().BeTrue();
    }

    [Fact]
    public void CoreBlazorOptionsBuilder_ConfigureContext_DirectOverload_RegistersProvidedOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var contexts = Enumerable.Empty<DiscoveredContext>();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, contexts);
        var provided = new CoreBlazorDbContextOptions<TestDbContext> { DisplayTitle = "provided" };

        // Act
        coreOptionsBuilder.ConfigureContext(provided);

        // Assert
        var provider = services.BuildServiceProvider();
        var resolved = provider.GetService<CoreBlazorDbContextOptions<TestDbContext>>();
        resolved.Should().NotBeNull();
        resolved!.DisplayTitle.Should().Be("provided");
    }

    [Fact]
    public void CoreBlazorOptionsBuilder_WithAuthorizationCallback_RegistersPolicies_ForDiscoveredContextsAndSets()
    {
        // Arrange
        var services = new ServiceCollection();
        var discovered = new List<DiscoveredContext>
        {
            new DiscoveredContext
            {
                ContextType = typeof(TestDbContext),
                Sets = new List<DiscoveredSet> { new DiscoveredSet { EntityType = typeof(TestEntity) } }
            }
        };
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, discovered);

        // Act
        coreOptionsBuilder.WithAuthorizationCallback((actionInfo, user) => true);

        // Assert
        var provider = services.BuildServiceProvider();
        var authOptions = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        var expectedInfo = Policies.CanReadInfo(typeof(TestDbContext));
        var expectedRead = Policies.CanRead(typeof(TestDbContext), typeof(TestEntity));
        var expectedCreate = Policies.CanCreate(typeof(TestDbContext), typeof(TestEntity));
        var expectedEdit = Policies.CanEdit(typeof(TestDbContext), typeof(TestEntity));
        var expectedDelete = Policies.CanDelete(typeof(TestDbContext), typeof(TestEntity));

        authOptions.GetPolicy(expectedInfo).Should().NotBeNull();
        authOptions.GetPolicy(expectedRead).Should().NotBeNull();
        authOptions.GetPolicy(expectedCreate).Should().NotBeNull();
        authOptions.GetPolicy(expectedEdit).Should().NotBeNull();
        authOptions.GetPolicy(expectedDelete).Should().NotBeNull();
    }

    #endregion

    #region CoreBlazorDbContextOptions Tests

    [Fact]
    public void CoreBlazorDbContextOptions_DefaultDisplayTitle_IsContextTypeName()
    {
        // Arrange & Act
        var options = new CoreBlazorDbContextOptions<TestDbContext>();

        // Assert
        options.DisplayTitle.Should().Be(nameof(TestDbContext));
    }

    #endregion

    #region CoreBlazorDbContextOptionsBuilder Tests

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_WithTitle_PopulatesConfigurationHelper()
    {
        // Arrange
        ConfigurationHelper.DisplayTitles.Clear();
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var builder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);

        // Act
        builder.WithTitle("My Context Title");

        // Assert
        ConfigurationHelper.DisplayTitles.Should().ContainKey(typeof(TestDbContext).Name);
        ConfigurationHelper.DisplayTitles[typeof(TestDbContext).Name].Should().Be("My Context Title");
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_WithSplitQueries_SetsOption()
    {
        // Arrange
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var builder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);

        // Act
        builder.WithSplitQueries(true);

        // Assert
        builder.Options.UseSplitQueries.Should().BeTrue();
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_WithSplitQueries_DefaultsToFalse()
    {
        // Arrange
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var builder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);

        // Act
        // Nothing to do, just checking default

        // Assert
        builder.Options.UseSplitQueries.Should().BeFalse();
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_UserCanReadIf_RegistersPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var builder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);

        // Act
        builder.UserCanReadIf(user => user.IsInRole("Admin"));

        // Assert
        var provider = services.BuildServiceProvider();
        var authOptions = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        var policy = authOptions.GetPolicy(Policies.CanReadInfo(typeof(TestDbContext)));
        policy.Should().NotBeNull();
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_ConfigureContext_ReturnsMainBuilder()
    {
        // Arrange
        ConfigurationHelper.DisplayTitles.Clear();
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var contextBuilder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);

        // Act
        var result = contextBuilder.ConfigureContext(ctx => ctx.WithTitle("Test"));

        // Assert
        result.Should().BeSameAs(coreOptionsBuilder);
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_ConfigureContext_WithOptions_ReturnsToMainBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var contextBuilder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);
        var options = new CoreBlazorDbContextOptions<TestDbContext> { DisplayTitle = "Options" };

        // Act
        var result = contextBuilder.ConfigureContext(options);

        // Assert
        result.Should().BeSameAs(coreOptionsBuilder);
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_ConfigureSet_WithAction_RegistersOptions()
    {
        // Arrange
        ConfigurationHelper.DisplayTitles.Clear();
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var builder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);

        // Act
        builder.ConfigureSet<TestEntity>(set => set.WithTitle("Test Entities"));

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetService<CoreBlazorDbSetOptions<TestDbContext, TestEntity>>();
        options.Should().NotBeNull();
        options!.DisplayTitle.Should().Be("Test Entities");
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_ConfigureSet_WithDirectOptions_RegistersOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var builder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);
        var options = new CoreBlazorDbSetOptions<TestDbContext, TestEntity> { DisplayTitle = "Direct Options" };

        // Act
        builder.ConfigureSet(options);

        // Assert
        var provider = services.BuildServiceProvider();
        var resolved = provider.GetService<CoreBlazorDbSetOptions<TestDbContext, TestEntity>>();
        resolved.Should().BeSameAs(options);
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_ConfigureSet_InheritsSplitQueriesSetting()
    {
        // Arrange
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var builder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);
        builder.WithSplitQueries(true);

        // Act
        builder.ConfigureSet<TestEntity>(set => set.WithTitle("Test"));

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetService<CoreBlazorDbSetOptions<TestDbContext, TestEntity>>();
        options!.UseSplitQueries.Should().BeTrue();
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_ConfigureSet_WithPropertyAccessorAndOptions_RegistersOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var builder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);
        var options = new CoreBlazorDbSetOptions<TestDbContext, TestEntity> { DisplayTitle = "Direct Set Options" };

        // Act
        builder.ConfigureSet(ctx => ctx.TestEntities, options);

        // Assert
        var provider = services.BuildServiceProvider();
        var resolved = provider.GetService<CoreBlazorDbSetOptions<TestDbContext, TestEntity>>();
        resolved.Should().BeSameAs(options);
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_ConfigureSet_WithPropertyAccessorAndAction_RegistersOptions()
    {
        // Arrange
        ConfigurationHelper.DisplayTitles.Clear();
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var builder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);

        // Act
        builder.ConfigureSet(ctx => ctx.TestEntities, set => set.WithTitle("Set Title"));

        // Assert
        var provider = services.BuildServiceProvider();
        var resolved = provider.GetService<CoreBlazorDbSetOptions<TestDbContext, TestEntity>>();
        resolved.Should().NotBeNull();
        resolved!.DisplayTitle.Should().Be("Set Title");
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_ConfigureSet_WithPropertyAccessor_InvalidExpression_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var builder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);
        var options = new CoreBlazorDbSetOptions<TestDbContext, TestEntity>();

        // Act & Assert
        var act = () => builder.ConfigureSet(ctx => ctx.TestEntities.Where(x => true).AsQueryable() as DbSet<TestEntity>, options);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*simple member expression*");
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_ConfigureSet_WithPropertyAccessorAndAction_InvalidExpression_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var builder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);

        // Act & Assert
        var act = () => builder.ConfigureSet(ctx => ctx.TestEntities.Where(x => true).AsQueryable() as DbSet<TestEntity>, set => { });
        act.Should().Throw<ArgumentException>()
            .WithMessage("*simple member expression*");
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_ConfigureSet_WithSplitQueriesEnabled_InheritsToSetOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var builder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);
        builder.WithSplitQueries(true);

        // Act
        builder.ConfigureSet(ctx => ctx.TestEntities, set => set.WithTitle("Test"));

        // Assert
        var provider = services.BuildServiceProvider();
        var resolved = provider.GetService<CoreBlazorDbSetOptions<TestDbContext, TestEntity>>();
        resolved!.UseSplitQueries.Should().BeTrue();
    }

    [Fact]
    public void CoreBlazorDbContextOptionsBuilder_ChainedCalls_WorksCorrectly()
    {
        // Arrange
        ConfigurationHelper.DisplayTitles.Clear();
        var services = new ServiceCollection();
        var coreOptionsBuilder = new CoreBlazorOptionsBuilder(services, Enumerable.Empty<DiscoveredContext>());
        var builder = new CoreBlazorDbContextOptionsBuilder<TestDbContext>(services, coreOptionsBuilder);

        // Act
        var result = builder
            .WithTitle("Context Title")
            .WithSplitQueries(true)
            .UserCanReadIf(user => user.IsInRole("Admin"))
            .ConfigureSet<TestEntity>(set => set.WithTitle("Set Title"));

        // Assert
        result.Should().BeSameAs(builder);
        builder.Options.DisplayTitle.Should().Be("Context Title");
        builder.Options.UseSplitQueries.Should().BeTrue();

        var provider = services.BuildServiceProvider();
        var setOptions = provider.GetService<CoreBlazorDbSetOptions<TestDbContext, TestEntity>>();
        setOptions!.DisplayTitle.Should().Be("Set Title");
        setOptions.UseSplitQueries.Should().BeTrue();
    }

    #endregion

    #region CoreBlazorDbSetOptionsBuilder Tests

    [Fact]
    public void CoreBlazorDbSetOptionsBuilder_WithTitle_PopulatesConfigurationHelper_WithContextAndEntityKey()
    {
        // Arrange
        ConfigurationHelper.DisplayTitles.Clear();
        var services = new ServiceCollection();
        var setBuilder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        setBuilder.WithTitle("My Set Title");

        // Assert
        var expectedKey = typeof(TestDbContext).Name + typeof(TestEntity).Name;
        ConfigurationHelper.DisplayTitles.Should().ContainKey(expectedKey);
        ConfigurationHelper.DisplayTitles[expectedKey].Should().Be("My Set Title");
    }

    [Fact]
    public void CoreBlazorDbSetOptionsBuilder_WithEntityDisplay_Function_SetsStringDisplay()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.WithEntityDisplay(e => e.Name);

        // Assert
        builder.Options.StringDisplay.Should().NotBeNull();
        builder.Options.StringDisplay!(new TestEntity { Name = "Test" }).Should().Be("Test");
    }

    [Fact]
    public void CoreBlazorDbSetOptionsBuilder_WithEntityDisplay_Generic_SetsComponentDisplay()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.WithEntityDisplay<TestEntityDisplayComponent>();

        // Assert
        builder.Options.ComponentDisplay.Should().Be(typeof(TestEntityDisplayComponent));
    }

    [Fact]
    public void CoreBlazorDbSetOptionsBuilder_WithEntityDisplay_Type_SetsComponentDisplay()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.WithEntityDisplay(typeof(TestEntityDisplayComponent));

        // Assert
        builder.Options.ComponentDisplay.Should().Be(typeof(TestEntityDisplayComponent));
    }

    [Fact]
    public void CoreBlazorDbSetOptionsBuilder_WithEntityDisplay_InvalidType_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act & Assert
        var act = () => builder.WithEntityDisplay(typeof(string));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CoreBlazorDbSetOptionsBuilder_UserCanReadIf_RegistersPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.UserCanReadIf(user => user.IsInRole("Reader"));

        // Assert
        var provider = services.BuildServiceProvider();
        var authOptions = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        var policy = authOptions.GetPolicy(Policies.CanRead(typeof(TestDbContext), typeof(TestEntity)));
        policy.Should().NotBeNull();
    }

    [Fact]
    public void CoreBlazorDbSetOptionsBuilder_UserCanCreateIf_RegistersPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.UserCanCreateIf(user => user.IsInRole("Creator"));

        // Assert
        var provider = services.BuildServiceProvider();
        var authOptions = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        var policy = authOptions.GetPolicy(Policies.CanCreate(typeof(TestDbContext), typeof(TestEntity)));
        policy.Should().NotBeNull();
    }

    [Fact]
    public void CoreBlazorDbSetOptionsBuilder_UserCanEditIf_RegistersPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.UserCanEditIf(user => user.IsInRole("Editor"));

        // Assert
        var provider = services.BuildServiceProvider();
        var authOptions = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        var policy = authOptions.GetPolicy(Policies.CanEdit(typeof(TestDbContext), typeof(TestEntity)));
        policy.Should().NotBeNull();
    }

    [Fact]
    public void CoreBlazorDbSetOptionsBuilder_UserCanDeleteIf_RegistersPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.UserCanDeleteIf(user => user.IsInRole("Deleter"));

        // Assert
        var provider = services.BuildServiceProvider();
        var authOptions = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        var policy = authOptions.GetPolicy(Policies.CanDelete(typeof(TestDbContext), typeof(TestEntity)));
        policy.Should().NotBeNull();
    }

    [Fact]
    public void CoreBlazorDbSetOptionsBuilder_ConfigureProperty_ReturnsPropertyOptionsBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        var propertyBuilder = builder.ConfigureProperty(e => e.Name);

        // Assert
        propertyBuilder.Should().NotBeNull();
        propertyBuilder.Should().BeOfType<CoreBlazorPropertyOptionsBuilder<TestEntity, string>>();
    }

    [Fact]
    public void CoreBlazorDbSetOptionsBuilder_ChainedConfiguration_WorksCorrectly()
    {
        // Arrange
        ConfigurationHelper.DisplayTitles.Clear();
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.WithTitle("Test Entities")
               .WithSplitQueries(true)
               .WithEntityDisplay(e => e.Name)
               .UserCanReadIf(user => user.IsInRole("Reader"))
               .UserCanCreateIf(user => user.IsInRole("Creator"))
               .UserCanEditIf(user => user.IsInRole("Editor"))
               .UserCanDeleteIf(user => user.IsInRole("Deleter"));

        // Assert
        builder.Options.DisplayTitle.Should().Be("Test Entities");
        builder.Options.UseSplitQueries.Should().BeTrue();
        builder.Options.StringDisplay.Should().NotBeNull();

        var provider = services.BuildServiceProvider();
        var authOptions = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        authOptions.GetPolicy(Policies.CanRead(typeof(TestDbContext), typeof(TestEntity))).Should().NotBeNull();
        authOptions.GetPolicy(Policies.CanCreate(typeof(TestDbContext), typeof(TestEntity))).Should().NotBeNull();
        authOptions.GetPolicy(Policies.CanEdit(typeof(TestDbContext), typeof(TestEntity))).Should().NotBeNull();
        authOptions.GetPolicy(Policies.CanDelete(typeof(TestDbContext), typeof(TestEntity))).Should().NotBeNull();
    }

    #endregion

    #region CoreBlazorPropertyOptionsBuilder Tests

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_Hidden_AddsPropertyToHiddenList()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.ConfigureProperty(e => e.Name).Hidden();

        // Assert
        var nameProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        builder.Options.HiddenProperties.Should().Contain(nameProperty);
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_Hidden_DoesNotDuplicateProperty()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.ConfigureProperty(e => e.Name).Hidden().Hidden();

        // Assert
        var nameProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        builder.Options.HiddenProperties.Count(p => p == nameProperty).Should().Be(1);
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_WithDisplay_Generic_AddsDisplayType()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.ConfigureProperty(e => e.Name).WithDisplay<TestPropertyDisplayComponent>();

        // Assert
        var nameProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        builder.Options.DisplayTypes.Should().Contain(kv =>
            kv.Key == nameProperty && kv.Value == typeof(TestPropertyDisplayComponent));
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_WithDisplay_Type_AddsDisplayType()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.ConfigureProperty(e => e.Name).WithDisplay(typeof(TestPropertyDisplayComponent));

        // Assert
        var nameProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        builder.Options.DisplayTypes.Should().Contain(kv =>
            kv.Key == nameProperty && kv.Value == typeof(TestPropertyDisplayComponent));
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_WithDisplay_InvalidType_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act & Assert
        var act = () => builder.ConfigureProperty(e => e.Name).WithDisplay(typeof(string));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_WithDisplay_DoesNotDuplicateDisplayType()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act - Add same display twice
        builder.ConfigureProperty(e => e.Name)
               .WithDisplay<TestPropertyDisplayComponent>()
               .WithDisplay<TestPropertyDisplayComponent>();

        // Assert
        var nameProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        builder.Options.DisplayTypes.Count(kv => kv.Key == nameProperty).Should().Be(1);
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_WithEditor_Generic_AddsEditingType()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.ConfigureProperty(e => e.Name).WithEditor<TestPropertyEditComponent>();

        // Assert
        var nameProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        builder.Options.EditingTypes.Should().Contain(kv =>
            kv.Key == nameProperty && kv.Value == typeof(TestPropertyEditComponent));
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_WithEditor_Type_AddsEditingType()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.ConfigureProperty(e => e.Name).WithEditor(typeof(TestPropertyEditComponent));

        // Assert
        var nameProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        builder.Options.EditingTypes.Should().Contain(kv =>
            kv.Key == nameProperty && kv.Value == typeof(TestPropertyEditComponent));
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_WithEditor_InvalidType_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act & Assert
        var act = () => builder.ConfigureProperty(e => e.Name).WithEditor(typeof(string));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_WithEditor_DoesNotDuplicateEditingType()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act - Add same editor twice
        builder.ConfigureProperty(e => e.Name)
               .WithEditor<TestPropertyEditComponent>()
               .WithEditor<TestPropertyEditComponent>();

        // Assert
        var nameProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        builder.Options.EditingTypes.Count(kv => kv.Key == nameProperty).Should().Be(1);
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_WithEntityDisplay_Generic_ReturnsDbSetBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        var result = builder.ConfigureProperty(e => e.Name)
            .WithEntityDisplay<TestEntityDisplayComponent>();

        // Assert
        result.Should().BeSameAs(builder);
        builder.Options.ComponentDisplay.Should().Be(typeof(TestEntityDisplayComponent));
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_WithEntityDisplay_Type_ReturnsDbSetBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        var result = builder.ConfigureProperty(e => e.Name)
            .WithEntityDisplay(typeof(TestEntityDisplayComponent));

        // Assert
        result.Should().BeSameAs(builder);
        builder.Options.ComponentDisplay.Should().Be(typeof(TestEntityDisplayComponent));
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_WithEntityDisplay_InvalidType_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act & Assert
        var act = () => builder.ConfigureProperty(e => e.Name).WithEntityDisplay(typeof(string));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_WithTitle_ReturnsDbSetBuilder()
    {
        // Arrange
        ConfigurationHelper.DisplayTitles.Clear();
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        var result = builder.ConfigureProperty(e => e.Name).WithTitle("Entities");

        // Assert
        result.Should().BeSameAs(builder);
        builder.Options.DisplayTitle.Should().Be("Entities");
    }

    [Fact]
    public void CoreBlazorPropertyOptionsBuilder_ConfigureProperty_AllowsChainingMultipleProperties()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CoreBlazorDbSetOptionsBuilder<TestDbContext, TestEntity>(services);

        // Act
        builder.ConfigureProperty(e => e.Name).Hidden()
               .ConfigureProperty(e => e.IsActive).Hidden();

        // Assert
        var nameProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        var isActiveProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.IsActive))!;
        builder.Options.HiddenProperties.Should().Contain(nameProperty);
        builder.Options.HiddenProperties.Should().Contain(isActiveProperty);
    }

    #endregion

    #region Integration Tests (AddCoreBlazor Extension)

    [Fact]
    public void AddCoreBlazor_DiscoveredContexts_AreRegistered_AsSingletons()
    {
        // Arrange
        ConfigurationHelper.DisplayTitles.Clear();
        var services = new ServiceCollection();
        services.AddSingleton<TestDbContext>();

        // Act
        var builder = services.AddCoreBlazor();

        // Assert
        var provider = services.BuildServiceProvider();
        var discovered = provider.GetServices<DiscoveredContext>().ToList();
        discovered.Should().NotBeEmpty();
        discovered.Any(d => d.ContextType == typeof(TestDbContext)).Should().BeTrue();
        builder.Services.Should().BeSameAs(services);
    }

    #endregion
}
