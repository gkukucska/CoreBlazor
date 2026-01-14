using Bunit;
using CoreBlazor.Components;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoreBlazor.Tests.Components;

public class NotAuthorizedComponentTests : Bunit.TestContext
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
    }

    #endregion

    #region Rendering Tests

    [Fact]
    public void Component_ShouldRender_WithoutErrors()
    {
        // Act
        var cut = RenderComponent<NotAuthorizedComponent<TestDbContext, TestEntity>>();

        // Assert
        cut.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Component_Renders_NotAuthorizedMessage()
    {
        // Act
        var cut = RenderComponent<NotAuthorizedComponent<TestDbContext, TestEntity>>();

        // Assert
        cut.Markup.Should().Contain("You are not authorized to view this page.");
    }

    [Fact]
    public void Component_Renders_H3Tag()
    {
        // Act
        var cut = RenderComponent<NotAuthorizedComponent<TestDbContext, TestEntity>>();

        // Assert
        cut.Markup.Should().Contain("<h3>");
        cut.Markup.Should().Contain("</h3>");
    }

    [Fact]
    public void Component_Message_IsConsistent()
    {
        // Act
        var cut1 = RenderComponent<NotAuthorizedComponent<TestDbContext, TestEntity>>();
        var cut2 = RenderComponent<NotAuthorizedComponent<TestDbContext, TestEntity>>();

        // Assert
        cut1.Markup.Should().Be(cut2.Markup);
    }

    [Fact]
    public void Component_RendersCorrectly_WithDifferentEntityType()
    {
        // Act
        var cut = RenderComponent<NotAuthorizedComponent<TestDbContext, object>>();

        // Assert
        cut.Markup.Should().Contain("You are not authorized to view this page.");
    }

    #endregion
}
