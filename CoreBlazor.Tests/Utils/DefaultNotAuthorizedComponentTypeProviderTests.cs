using CoreBlazor.Components;
using CoreBlazor.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoreBlazor.Tests.Utils;

public class DefaultNotAuthorizedComponentTypeProviderTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; } = null!;
    }

    private class AnotherTestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; } = null!;
    }

    [Fact]
    public void GetNotAuthorizedComponentType_ReturnsNotAuthorizedComponentType()
    {
        // Arrange
        var provider = new DefaultNotAuthorizedComponentTypeProvider();

        // Act
        var componentType = provider.GetNotAuthorizedComponentType<TestDbContext, TestEntity>();

        // Assert
        componentType.Should().NotBeNull();
        componentType.Should().Be(typeof(NotAuthorizedComponent<TestDbContext, TestDbContext>));
    }

    [Fact]
    public void GetNotAuthorizedComponentType_WithDifferentContext_ReturnsSameBaseType()
    {
        // Arrange
        var provider = new DefaultNotAuthorizedComponentTypeProvider();

        // Act
        var componentType = provider.GetNotAuthorizedComponentType<AnotherTestDbContext, TestEntity>();

        // Assert
        componentType.Should().NotBeNull();
        componentType.Should().Be(typeof(NotAuthorizedComponent<AnotherTestDbContext, AnotherTestDbContext>));
    }

    [Fact]
    public void GetNotAuthorizedComponentType_IsGenericType()
    {
        // Arrange
        var provider = new DefaultNotAuthorizedComponentTypeProvider();

        // Act
        var componentType = provider.GetNotAuthorizedComponentType<TestDbContext, TestEntity>();

        // Assert
        componentType.IsGenericType.Should().BeTrue();
        componentType.GetGenericTypeDefinition().Should().Be(typeof(NotAuthorizedComponent<,>));
    }

    [Fact]
    public void GetNotAuthorizedComponentType_MultipleCallsSameTypes_ReturnsSameType()
    {
        // Arrange
        var provider = new DefaultNotAuthorizedComponentTypeProvider();

        // Act
        var type1 = provider.GetNotAuthorizedComponentType<TestDbContext, TestEntity>();
        var type2 = provider.GetNotAuthorizedComponentType<TestDbContext, TestEntity>();

        // Assert
        type1.Should().Be(type2);
    }

    [Fact]
    public void GetNotAuthorizedComponentType_ReturnsTypeWithCorrectGenericArguments()
    {
        // Arrange
        var provider = new DefaultNotAuthorizedComponentTypeProvider();

        // Act
        var componentType = provider.GetNotAuthorizedComponentType<TestDbContext, TestEntity>();

        // Assert
        var genericArgs = componentType.GetGenericArguments();
        genericArgs.Should().HaveCount(2);
        genericArgs[0].Should().Be(typeof(TestDbContext));
        genericArgs[1].Should().Be(typeof(TestDbContext));
    }
}
