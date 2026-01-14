using Bunit;
using CoreBlazor.Components;
using CoreBlazor.Configuration;
using CoreBlazor.Interfaces;
using CoreBlazor.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreBlazor.Tests.Components;

public class NavigationPropertyColumnViewerComponentTests : Bunit.TestContext
{
    #region Test Helpers

    public class RelatedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public override string ToString() => Name;
    }

    public class TestEntity
    {
        public int Id { get; set; }
        public RelatedEntity? Related { get; set; }
    }

    public class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<RelatedEntity> RelatedEntities { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
            modelBuilder.Entity<RelatedEntity>().HasKey(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }
    }

    public class TestEntityDisplayComponent : ComponentBase, IEntityDisplayComponent<RelatedEntity>
    {
        [Parameter]
        public RelatedEntity? Entity { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, $"Custom Display: {Entity?.Name}");
        }
    }

    #endregion

    #region Basic Rendering Tests

    [Fact]
    public void Component_ShouldRender_WithoutErrors()
    {
        // Arrange
        var related = new RelatedEntity { Id = 1, Name = "RelatedItem" };
        var entity = new TestEntity { Id = 1, Related = related };

        // Act
        var cut = RenderComponent<NavigationPropertyColumnViewerComponent<TestDbContext, TestEntity, RelatedEntity>>(parameters =>
        {
            parameters.Add(p => p.Entity, entity);
            parameters.Add(p => p.PropertyName, nameof(TestEntity.Related));
        });

        // Assert
        cut.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Component_HandlesNullProperty_WithoutError()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Related = null };

        // Act
        var cut = RenderComponent<NavigationPropertyColumnViewerComponent<TestDbContext, TestEntity, RelatedEntity>>(parameters =>
        {
            parameters.Add(p => p.Entity, entity);
            parameters.Add(p => p.PropertyName, nameof(TestEntity.Related));
        });

        // Assert
        cut.Instance.Should().NotBeNull();
    }

    #endregion

    #region Display Configuration Tests

    [Fact]
    public void Component_UsesComponentDisplay_WhenConfigured()
    {
        // Arrange
        var options = new CoreBlazorDbSetOptions<TestDbContext, RelatedEntity>
        {
            ComponentDisplay = typeof(TestEntityDisplayComponent)
        };
        Services.AddSingleton(options);

        var related = new RelatedEntity { Id = 1, Name = "CustomItem" };
        var entity = new TestEntity { Id = 1, Related = related };

        // Act
        var cut = RenderComponent<NavigationPropertyColumnViewerComponent<TestDbContext, TestEntity, RelatedEntity>>(parameters =>
        {
            parameters.Add(p => p.Entity, entity);
            parameters.Add(p => p.PropertyName, nameof(TestEntity.Related));
        });

        // Assert
        cut.Markup.Should().Contain("Custom Display: CustomItem");
    }

    [Fact]
    public void Component_UsesStringDisplay_WhenConfigured()
    {
        // Arrange
        var displayCalled = false;
        var options = new CoreBlazorDbSetOptions<TestDbContext, RelatedEntity>
        {
            StringDisplay = (entity) =>
            {
                displayCalled = true;
                return $"String: {entity?.Name}";
            }
        };
        Services.AddSingleton(options);

        var related = new RelatedEntity { Id = 1, Name = "StringItem" };
        var entity = new TestEntity { Id = 1, Related = related };

        // Act
        var cut = RenderComponent<NavigationPropertyColumnViewerComponent<TestDbContext, TestEntity, RelatedEntity>>(parameters =>
        {
            parameters.Add(p => p.Entity, entity);
            parameters.Add(p => p.PropertyName, nameof(TestEntity.Related));
        });

        // Assert - verify the string display function was called
        displayCalled.Should().BeTrue();
    }

    [Fact]
    public void Component_ComponentDisplay_TakesPrecedenceOver_StringDisplay()
    {
        // Arrange
        var stringDisplayCalled = false;
        var options = new CoreBlazorDbSetOptions<TestDbContext, RelatedEntity>
        {
            ComponentDisplay = typeof(TestEntityDisplayComponent),
            StringDisplay = (entity) =>
            {
                stringDisplayCalled = true;
                return $"String: {entity?.Name}";
            }
        };
        Services.AddSingleton(options);

        var related = new RelatedEntity { Id = 1, Name = "TestItem" };
        var entity = new TestEntity { Id = 1, Related = related };

        // Act
        var cut = RenderComponent<NavigationPropertyColumnViewerComponent<TestDbContext, TestEntity, RelatedEntity>>(parameters =>
        {
            parameters.Add(p => p.Entity, entity);
            parameters.Add(p => p.PropertyName, nameof(TestEntity.Related));
        });

        // Assert - ComponentDisplay is used, so StringDisplay should NOT be called
        cut.Markup.Should().Contain("Custom Display:");
        stringDisplayCalled.Should().BeFalse();
    }

    #endregion

    #region DbSetOptions Tests

    [Fact]
    public void Component_SetsDbSetOptions_WhenAvailable()
    {
        // Arrange
        var options = new CoreBlazorDbSetOptions<TestDbContext, RelatedEntity>
        {
            ComponentDisplay = typeof(TestEntityDisplayComponent)
        };
        Services.AddSingleton(options);

        var related = new RelatedEntity { Id = 1, Name = "TestItem" };
        var entity = new TestEntity { Id = 1, Related = related };

        // Act
        var cut = RenderComponent<NavigationPropertyColumnViewerComponent<TestDbContext, TestEntity, RelatedEntity>>(parameters =>
        {
            parameters.Add(p => p.Entity, entity);
            parameters.Add(p => p.PropertyName, nameof(TestEntity.Related));
        });

        // Assert
        cut.Instance.DbSetOptions.Should().NotBeNull();
        cut.Instance.DbSetOptions.Should().Be(options);
    }

    #endregion
}
