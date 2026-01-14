using CoreBlazor.Utils;
using FluentAssertions;
using Xunit;

namespace CoreBlazor.Tests.Utils;

public class ActionInfoTests
{
    [Fact]
    public void ActionInfo_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var action = DbAction.ReadEntities;
        var contextName = "TestContext";
        var setName = "Users";

        // Act
        var actionInfo = new ActionInfo(action, contextName, setName);

        // Assert
        actionInfo.ContextAction.Should().Be(DbAction.ReadEntities);
        actionInfo.DbContextName.Should().Be("TestContext");
        actionInfo.DbSetName.Should().Be("Users");
    }

    [Fact]
    public void ActionInfo_IsRecord_SupportsValueEquality()
    {
        // Arrange
        var action1 = new ActionInfo(DbAction.CreateEntity, "Context1", "Set1");
        var action2 = new ActionInfo(DbAction.CreateEntity, "Context1", "Set1");
        var action3 = new ActionInfo(DbAction.EditEntity, "Context1", "Set1");

        // Act & Assert
        action1.Should().Be(action2);
        action1.Should().NotBe(action3);
    }

    [Fact]
    public void ActionInfo_Deconstruction_WorksCorrectly()
    {
        // Arrange
        var actionInfo = new ActionInfo(DbAction.DeleteEntity, "MyContext", "Products");

        // Act
        var (action, contextName, setName) = actionInfo;

        // Assert
        action.Should().Be(DbAction.DeleteEntity);
        contextName.Should().Be("MyContext");
        setName.Should().Be("Products");
    }

    [Fact]
    public void ActionInfo_WithDifferentActions_CreatesDistinctInstances()
    {
        // Arrange & Act
        var readInfo = new ActionInfo(DbAction.ReadInfo, "Context", "");
        var readEntities = new ActionInfo(DbAction.ReadEntities, "Context", "Set");
        var create = new ActionInfo(DbAction.CreateEntity, "Context", "Set");
        var edit = new ActionInfo(DbAction.EditEntity, "Context", "Set");
        var delete = new ActionInfo(DbAction.DeleteEntity, "Context", "Set");

        // Assert
        readInfo.ContextAction.Should().Be(DbAction.ReadInfo);
        readEntities.ContextAction.Should().Be(DbAction.ReadEntities);
        create.ContextAction.Should().Be(DbAction.CreateEntity);
        edit.ContextAction.Should().Be(DbAction.EditEntity);
        delete.ContextAction.Should().Be(DbAction.DeleteEntity);
    }

    [Fact]
    public void ActionInfo_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var actionInfo = new ActionInfo(DbAction.EditEntity, "TestContext", "Users");

        // Act
        var result = actionInfo.ToString();

        // Assert
        result.Should().Contain("EditEntity");
        result.Should().Contain("TestContext");
        result.Should().Contain("Users");
    }

    [Fact]
    public void ActionInfo_GetHashCode_IsSameForEqualInstances()
    {
        // Arrange
        var action1 = new ActionInfo(DbAction.ReadEntities, "Context", "Set");
        var action2 = new ActionInfo(DbAction.ReadEntities, "Context", "Set");

        // Act
        var hash1 = action1.GetHashCode();
        var hash2 = action2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ActionInfo_GetHashCode_IsDifferentForDifferentInstances()
    {
        // Arrange
        var action1 = new ActionInfo(DbAction.ReadEntities, "Context1", "Set1");
        var action2 = new ActionInfo(DbAction.ReadEntities, "Context2", "Set2");

        // Act
        var hash1 = action1.GetHashCode();
        var hash2 = action2.GetHashCode();

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void ActionInfo_WithEmptyStrings_CreatesValidInstance()
    {
        // Arrange & Act
        var actionInfo = new ActionInfo(DbAction.ReadInfo, "", "");

        // Assert
        actionInfo.DbContextName.Should().BeEmpty();
        actionInfo.DbSetName.Should().BeEmpty();
        actionInfo.ContextAction.Should().Be(DbAction.ReadInfo);
    }
}

public class DbActionTests
{
    [Fact]
    public void DbAction_AllValuesAreDefined()
    {
        // Arrange & Act
        var values = Enum.GetValues<DbAction>();

        // Assert
        values.Should().Contain(DbAction.ReadInfo);
        values.Should().Contain(DbAction.ReadEntities);
        values.Should().Contain(DbAction.CreateEntity);
        values.Should().Contain(DbAction.EditEntity);
        values.Should().Contain(DbAction.DeleteEntity);
    }

    [Fact]
    public void DbAction_HasExpectedCount()
    {
        // Arrange & Act
        var values = Enum.GetValues<DbAction>();

        // Assert
        values.Should().HaveCount(5);
    }

    [Fact]
    public void DbAction_CanBeConvertedToString()
    {
        // Arrange
        var action = DbAction.CreateEntity;

        // Act
        var result = action.ToString();

        // Assert
        result.Should().Be("CreateEntity");
    }

    [Fact]
    public void DbAction_CanBeParsedFromString()
    {
        // Arrange
        var actionString = "EditEntity";

        // Act
        var parsed = Enum.Parse<DbAction>(actionString);

        // Assert
        parsed.Should().Be(DbAction.EditEntity);
    }

    [Theory]
    [InlineData(DbAction.ReadInfo, "ReadInfo")]
    [InlineData(DbAction.ReadEntities, "ReadEntities")]
    [InlineData(DbAction.CreateEntity, "CreateEntity")]
    [InlineData(DbAction.EditEntity, "EditEntity")]
    [InlineData(DbAction.DeleteEntity, "DeleteEntity")]
    public void DbAction_ToStringReturnsExpectedValue(DbAction action, string expected)
    {
        // Act
        var result = action.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void DbAction_IsDefined_ReturnsTrueForValidValues()
    {
        // Act & Assert
        Enum.IsDefined(typeof(DbAction), DbAction.ReadInfo).Should().BeTrue();
        Enum.IsDefined(typeof(DbAction), DbAction.ReadEntities).Should().BeTrue();
        Enum.IsDefined(typeof(DbAction), DbAction.CreateEntity).Should().BeTrue();
        Enum.IsDefined(typeof(DbAction), DbAction.EditEntity).Should().BeTrue();
        Enum.IsDefined(typeof(DbAction), DbAction.DeleteEntity).Should().BeTrue();
    }

    [Fact]
    public void DbAction_IsDefined_ReturnsFalseForInvalidValue()
    {
        // Arrange
        var invalidValue = (DbAction)999;

        // Act & Assert
        Enum.IsDefined(typeof(DbAction), invalidValue).Should().BeFalse();
    }

    [Fact]
    public void DbAction_CanBeUsedInSwitch()
    {
        // Arrange
        var action = DbAction.CreateEntity;
        var result = "";

        // Act
        switch (action)
        {
            case DbAction.ReadInfo:
                result = "Info";
                break;
            case DbAction.ReadEntities:
                result = "Read";
                break;
            case DbAction.CreateEntity:
                result = "Create";
                break;
            case DbAction.EditEntity:
                result = "Edit";
                break;
            case DbAction.DeleteEntity:
                result = "Delete";
                break;
        }

        // Assert
        result.Should().Be("Create");
    }
}
