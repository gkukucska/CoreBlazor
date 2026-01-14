using CoreBlazor.Utils;
using FluentAssertions;
using Xunit;

namespace CoreBlazor.Tests.Utils;

public class NavigationPathProviderTests
{
    private readonly DefaultNavigationPathProvider _provider;

    public NavigationPathProviderTests()
    {
        _provider = new DefaultNavigationPathProvider();
    }

    [Fact]
    public void GetPathToCreateEntity_ReturnsCorrectPath()
    {
        // Arrange
        var dbContextName = "TestContext";
        var dbSetName = "Users";

        // Act
        var path = _provider.GetPathToCreateEntity(dbContextName, dbSetName);

        // Assert
        path.Should().Be("/DbContext/TestContext/DbSet/Users/Create");
    }

    [Fact]
    public void GetPathToDeleteEntity_ReturnsCorrectPath()
    {
        // Arrange
        var dbContextName = "TestContext";
        var dbSetName = "Users";
        var entityId = "123";

        // Act
        var path = _provider.GetPathToDeleteEntity(dbContextName, dbSetName, entityId);

        // Assert
        path.Should().Be("/DbContext/TestContext/DbSet/Users/Delete/123");
    }

    [Fact]
    public void GetPathToEditEntity_ReturnsCorrectPath()
    {
        // Arrange
        var dbContextName = "TestContext";
        var dbSetName = "Users";
        var entityId = "456";

        // Act
        var path = _provider.GetPathToEditEntity(dbContextName, dbSetName, entityId);

        // Assert
        path.Should().Be("/DbContext/TestContext/DbSet/Users/Edit/456");
    }

    [Fact]
    public void GetPathToReadDbContextInfo_ReturnsCorrectPath()
    {
        // Arrange
        var dbContextName = "TestContext";

        // Act
        var path = _provider.GetPathToReadDbContextInfo(dbContextName);

        // Assert
        path.Should().Be("/DbContext/TestContext/Info");
    }

    [Fact]
    public void GetPathToReadEntities_ReturnsCorrectPath()
    {
        // Arrange
        var dbContextName = "TestContext";
        var dbSetName = "Users";

        // Act
        var path = _provider.GetPathToReadEntities(dbContextName, dbSetName);

        // Assert
        path.Should().Be("/DbContext/TestContext/DbSet/Users");
    }

    [Fact]
    public void GetPathToCreateEntity_WithEmptyStrings_ReturnsPathWithEmptySegments()
    {
        // Arrange
        var dbContextName = "";
        var dbSetName = "";

        // Act
        var path = _provider.GetPathToCreateEntity(dbContextName, dbSetName);

        // Assert
        path.Should().Be("/DbContext//DbSet//Create");
    }

    [Fact]
    public void GetPathToDeleteEntity_WithSpecialCharacters_IncludesCharactersInPath()
    {
        // Arrange
        var dbContextName = "Test-Context";
        var dbSetName = "User_Set";
        var entityId = "abc-123";

        // Act
        var path = _provider.GetPathToDeleteEntity(dbContextName, dbSetName, entityId);

        // Assert
        path.Should().Be("/DbContext/Test-Context/DbSet/User_Set/Delete/abc-123");
    }

    [Fact]
    public void GetPathToEditEntity_WithGuidEntityId_ReturnsCorrectPath()
    {
        // Arrange
        var dbContextName = "TestContext";
        var dbSetName = "Users";
        var entityId = "550e8400-e29b-41d4-a716-446655440000";

        // Act
        var path = _provider.GetPathToEditEntity(dbContextName, dbSetName, entityId);

        // Assert
        path.Should().Be("/DbContext/TestContext/DbSet/Users/Edit/550e8400-e29b-41d4-a716-446655440000");
    }

    [Fact]
    public void GetPathToReadDbContextInfo_WithLongName_ReturnsCorrectPath()
    {
        // Arrange
        var dbContextName = "VeryLongDbContextNameForTestingPurposes";

        // Act
        var path = _provider.GetPathToReadDbContextInfo(dbContextName);

        // Assert
        path.Should().Be("/DbContext/VeryLongDbContextNameForTestingPurposes/Info");
    }

    [Theory]
    [InlineData("Context1", "Set1", "1", "/DbContext/Context1/DbSet/Set1/Edit/1")]
    [InlineData("MyContext", "Products", "999", "/DbContext/MyContext/DbSet/Products/Edit/999")]
    [InlineData("AppDbContext", "Orders", "order-123", "/DbContext/AppDbContext/DbSet/Orders/Edit/order-123")]
    public void GetPathToEditEntity_VariousInputs_ReturnsExpectedPaths(
        string contextName, string setName, string entityId, string expected)
    {
        // Act
        var path = _provider.GetPathToEditEntity(contextName, setName, entityId);

        // Assert
        path.Should().Be(expected);
    }

    [Theory]
    [InlineData("Context1", "Set1", "/DbContext/Context1/DbSet/Set1/Create")]
    [InlineData("MyContext", "Products", "/DbContext/MyContext/DbSet/Products/Create")]
    public void GetPathToCreateEntity_VariousInputs_ReturnsExpectedPaths(
        string contextName, string setName, string expected)
    {
        // Act
        var path = _provider.GetPathToCreateEntity(contextName, setName);

        // Assert
        path.Should().Be(expected);
    }

    [Theory]
    [InlineData("Context1", "/DbContext/Context1/Info")]
    [InlineData("MyContext", "/DbContext/MyContext/Info")]
    [InlineData("ApplicationDbContext", "/DbContext/ApplicationDbContext/Info")]
    public void GetPathToReadDbContextInfo_VariousInputs_ReturnsExpectedPaths(
        string contextName, string expected)
    {
        // Act
        var path = _provider.GetPathToReadDbContextInfo(contextName);

        // Assert
        path.Should().Be(expected);
    }
}
