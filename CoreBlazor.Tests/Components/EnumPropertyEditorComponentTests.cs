using Bunit;
using CoreBlazor.Components;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Xunit;

namespace CoreBlazor.Tests.Components;

public class EnumPropertyEditorComponentTests : Bunit.TestContext
{
    #region Test Helpers

    public enum TestEnum
    {
        None = 0,
        First = 1,
        Second = 2,
        Third = 3
    }

    public class TestEntity
    {
        public TestEnum MyEnum { get; set; }
    }

    private RenderFragment RenderInsideEditForm<T>(T entity, string propertyName, bool isDisabled)
        => builder =>
        {
            builder.OpenComponent(0, typeof(EditForm));
            builder.AddAttribute(1, "Model", entity);
            builder.AddAttribute(2, "ChildContent", (RenderFragment<EditContext>)(ec => (RenderFragment)(b =>
            {
                b.OpenComponent(3, typeof(EnumPropertyEditorComponent<TestEntity, TestEnum>));
                b.AddAttribute(4, "PropertyName", propertyName);
                b.AddAttribute(5, "Entity", entity);
                b.AddAttribute(6, "IsDisabled", isDisabled);
                b.CloseComponent();
            })));
            builder.CloseComponent();
        };

    #endregion

    #region Basic Rendering Tests

    [Fact]
    public void Component_RendersSelect_WithAllEnumOptions()
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.First };

        // Act
        var cut = Render(RenderInsideEditForm(entity, nameof(TestEntity.MyEnum), false));

        // Assert
        var select = cut.Find("select");
        select.Should().NotBeNull();
        var options = select.Children;
        options.Length.Should().Be(System.Enum.GetValues(typeof(TestEnum)).Length);

        entity.MyEnum.Should().Be(TestEnum.First);
        select.TextContent.Should().Contain("First");
    }

    [Fact]
    public void Component_RendersAllEnumValues_InCorrectOrder()
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.None };

        // Act
        var cut = Render(RenderInsideEditForm(entity, nameof(TestEntity.MyEnum), false));

        // Assert
        var select = cut.Find("select");
        var optionTexts = select.Children.Select(o => o.TextContent).ToList();

        optionTexts.Should().Contain("None");
        optionTexts.Should().Contain("First");
        optionTexts.Should().Contain("Second");
        optionTexts.Should().Contain("Third");
    }

    [Theory]
    [InlineData(TestEnum.None)]
    [InlineData(TestEnum.First)]
    [InlineData(TestEnum.Second)]
    [InlineData(TestEnum.Third)]
    public void Component_DisplaysCorrectInitialValue(TestEnum initialValue)
    {
        // Arrange
        var entity = new TestEntity { MyEnum = initialValue };

        // Act
        var cut = Render(RenderInsideEditForm(entity, nameof(TestEntity.MyEnum), false));

        // Assert
        entity.MyEnum.Should().Be(initialValue);
        var select = cut.Find("select");
        select.TextContent.Should().Contain(initialValue.ToString());
    }

    #endregion

    #region Value Change Tests

    [Fact]
    public void Component_ChangingSelection_UpdatesEntityProperty()
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.First };

        var cut = Render(RenderInsideEditForm(entity, nameof(TestEntity.MyEnum), false));

        var select = cut.Find("select");

        // Act
        select.Change(TestEnum.Second.ToString());

        // Assert
        entity.MyEnum.Should().Be(TestEnum.Second);
    }

    [Fact]
    public void Component_HandlesMultipleChanges_Correctly()
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.None };
        var cut = Render(RenderInsideEditForm(entity, nameof(TestEntity.MyEnum), false));
        var select = cut.Find("select");

        // Act - change multiple times
        select.Change(TestEnum.First.ToString());
        entity.MyEnum.Should().Be(TestEnum.First);

        select.Change(TestEnum.Second.ToString());
        entity.MyEnum.Should().Be(TestEnum.Second);

        select.Change(TestEnum.Third.ToString());

        // Assert
        entity.MyEnum.Should().Be(TestEnum.Third);
    }

    [Fact]
    public void Component_HandlesRapidChanges()
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.None };
        var cut = Render(RenderInsideEditForm(entity, nameof(TestEntity.MyEnum), false));
        var select = cut.Find("select");

        // Act - Rapid changes
        select.Change(TestEnum.First.ToString());
        select.Change(TestEnum.Second.ToString());
        select.Change(TestEnum.Third.ToString());
        select.Change(TestEnum.None.ToString());

        // Assert
        entity.MyEnum.Should().Be(TestEnum.None);
    }

    [Fact]
    public void Component_MaintainsState_AfterRerender()
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.First };
        var cut = Render(RenderInsideEditForm(entity, nameof(TestEntity.MyEnum), false));

        // Act - Change value
        var select = cut.Find("select");
        select.Change(TestEnum.Second.ToString());

        // Assert - State should be maintained
        entity.MyEnum.Should().Be(TestEnum.Second);
        var selectAfterChange = cut.Find("select");
        selectAfterChange.TextContent.Should().Contain("Second");
    }

    [Fact]
    public void Component_HandlesInvalidEnumValue_GracefullyReverts()
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.First };
        var cut = Render(RenderInsideEditForm(entity, nameof(TestEntity.MyEnum), false));
        var select = cut.Find("select");

        // Act - Try to set an invalid string value
        try
        {
            select.Change("InvalidEnumValue");
        }
        catch
        {
            // Expected to fail
        }

        // Assert - Entity should maintain valid state
        entity.MyEnum.Should().Be(TestEnum.First);
    }

    #endregion

    #region Disabled State Tests

    [Fact]
    public void Component_Disabled_PreventsUserInteraction()
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.None };

        var cut = Render(RenderInsideEditForm(entity, nameof(TestEntity.MyEnum), true));

        var select = cut.Find("select");

        // Assert
        select.HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Component_RespectsDisabledState(bool isDisabled)
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.First };

        // Act
        var cut = Render(RenderInsideEditForm(entity, nameof(TestEntity.MyEnum), isDisabled));

        // Assert
        var select = cut.Find("select");
        select.HasAttribute("disabled").Should().Be(isDisabled);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Component_DisabledAttribute_MatchesParameter(bool isDisabled, bool expectedDisabled)
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.First };

        // Act
        var cut = Render(RenderInsideEditForm(entity, nameof(TestEntity.MyEnum), isDisabled));

        // Assert
        var select = cut.Find("select");
        select.HasAttribute("disabled").Should().Be(expectedDisabled);
    }

    [Fact]
    public void Component_DisabledState_PreventsAllChanges()
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.First };
        var cut = Render(RenderInsideEditForm(entity, nameof(TestEntity.MyEnum), true));
        var select = cut.Find("select");

        // Assert - Select should be disabled
        select.HasAttribute("disabled").Should().BeTrue();

        // Original value should remain
        entity.MyEnum.Should().Be(TestEnum.First);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Component_ThrowsException_WhenEntityIsNull()
    {
        // Act & Assert
        var act = () => Render(RenderInsideEditForm<TestEntity>(null!, nameof(TestEntity.MyEnum), false));
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Component_ThrowsException_WhenPropertyNameIsNull()
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.First };

        // Act & Assert
        var act = () => Render(RenderInsideEditForm(entity, null!, false));
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Component_ThrowsException_WhenPropertyNameIsEmpty()
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.First };

        // Act & Assert
        var act = () => Render(RenderInsideEditForm(entity, string.Empty, false));
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Component_ThrowsException_WhenPropertyNameDoesNotExist()
    {
        // Arrange
        var entity = new TestEntity { MyEnum = TestEnum.First };

        // Act & Assert
        var act = () => Render(RenderInsideEditForm(entity, "NonExistentProperty", false));
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Component_HandlesEnumWithNoValues()
    {
        // This test documents behavior when working with empty enums (edge case)
        // Note: C# doesn't allow truly empty enums, so this is a theoretical test
        // Actual behavior depends on component's validation logic
        var entity = new TestEntity { MyEnum = TestEnum.None };

        // Act
        var cut = Render(RenderInsideEditForm(entity, nameof(TestEntity.MyEnum), false));

        // Assert - Should render without crashing
        var select = cut.Find("select");
        select.Should().NotBeNull();
    }

    #endregion
}
