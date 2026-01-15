using CoreBlazor.Utils;
using FluentAssertions;
using Xunit;

namespace CoreBlazor.Tests.Utils;

public class PropertyInfoExtensionsTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string>? Tags { get; set; }
        public double Amount { get; set; }
    }

    [Fact]
    public void GetMemberAccessExpressionWithConversion_ReturnsCorrectExpression()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Id))!;

        // Act
        var expression = property.GetMemberAccessExpressionWithConversion<TestEntity, object>();

        // Assert
        expression.Should().NotBeNull();
        
        var entity = new TestEntity { Id = 42 };
        var compiled = expression.Compile();
        var result = compiled(entity);
        result.Should().Be(42);
    }

    [Fact]
    public void GetMemberAccessExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;

        // Act
        var expression = property.GetMemberAccessExpression<TestEntity, string>();

        // Assert
        expression.Should().NotBeNull();
        
        var entity = new TestEntity { Name = "Test" };
        var compiled = expression.Compile();
        var result = compiled(entity);
        result.Should().Be("Test");
    }

    [Fact]
    public void GetValueAccessExpressionWithConversion_ReturnsCorrectExpression()
    {
        // Arrange
        var entity = new TestEntity { Id = 123 };
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Id))!;

        // Act
        var expression = property.GetValueAccessExpressionWithConversion<TestEntity, object>(entity);

        // Assert
        expression.Should().NotBeNull();
        var compiled = expression.Compile();
        var result = compiled();
        result.Should().Be(123);
    }

    [Fact]
    public void GetValueAccessExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var entity = new TestEntity { Name = "TestName" };
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;

        // Act
        var expression = property.GetValueAccessExpression<TestEntity, string>(entity);

        // Assert
        expression.Should().NotBeNull();
        var compiled = expression.Compile();
        var result = compiled();
        result.Should().Be("TestName");
    }

    [Fact]
    public void IsDisplayable_NonGenericType_ReturnsTrue()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;

        // Act
        var result = property.IsDisplayable();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDisplayable_GenericType_ReturnsFalse()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Tags))!;

        // Act
        var result = property.IsDisplayable();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsPredicate_CreatesCorrectPredicate()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        var filter = "Test";

        // Act
        var predicate = property.ContainsPredicate<TestEntity>(filter);

        // Assert
        predicate.Should().NotBeNull();
        var compiled = predicate.Compile();
        
        var matchingEntity = new TestEntity { Name = "TestValue" };
        var nonMatchingEntity = new TestEntity { Name = "Other" };
        
        compiled(matchingEntity).Should().BeTrue();
        compiled(nonMatchingEntity).Should().BeFalse();
    }

    [Fact]
    public void NotContainsPredicate_CreatesCorrectPredicate()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        var filter = "Test";

        // Act
        var predicate = property.NotContainsPredicate<TestEntity>(filter);

        // Assert
        predicate.Should().NotBeNull();
        var compiled = predicate.Compile();
        
        var matchingEntity = new TestEntity { Name = "Other" };
        var nonMatchingEntity = new TestEntity { Name = "TestValue" };
        
        compiled(matchingEntity).Should().BeTrue();
        compiled(nonMatchingEntity).Should().BeFalse();
    }

    [Fact]
    public void EqualsPredicate_CreatesCorrectPredicate()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        var filter = "Exact";

        // Act
        var predicate = property.EqualsPredicate<TestEntity>(filter);

        // Assert
        predicate.Should().NotBeNull();
        var compiled = predicate.Compile();
        
        var matchingEntity = new TestEntity { Name = "Exact" };
        var nonMatchingEntity = new TestEntity { Name = "NotExact" };
        
        compiled(matchingEntity).Should().BeTrue();
        compiled(nonMatchingEntity).Should().BeFalse();
    }

    [Fact]
    public void NotEqualsPredicate_CreatesCorrectPredicate()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        var filter = "Exact";

        // Act
        var predicate = property.NotEqualsPredicate<TestEntity>(filter);

        // Assert
        predicate.Should().NotBeNull();
        var compiled = predicate.Compile();
        
        var matchingEntity = new TestEntity { Name = "Different" };
        var nonMatchingEntity = new TestEntity { Name = "Exact" };
        
        compiled(matchingEntity).Should().BeTrue();
        compiled(nonMatchingEntity).Should().BeFalse();
    }

    [Fact]
    public void StartsWithPredicate_CreatesCorrectPredicate()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        var filter = "Start";

        // Act
        var predicate = property.StartsWithPredicate<TestEntity>(filter);

        // Assert
        predicate.Should().NotBeNull();
        var compiled = predicate.Compile();
        
        var matchingEntity = new TestEntity { Name = "StartValue" };
        var nonMatchingEntity = new TestEntity { Name = "ValueStart" };
        
        compiled(matchingEntity).Should().BeTrue();
        compiled(nonMatchingEntity).Should().BeFalse();
    }

    [Fact]
    public void EndsWithPredicate_CreatesCorrectPredicate()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        var filter = "End";

        // Act
        var predicate = property.EndsWithPredicate<TestEntity>(filter);

        // Assert
        predicate.Should().NotBeNull();
        var compiled = predicate.Compile();
        
        var matchingEntity = new TestEntity { Name = "ValueEnd" };
        var nonMatchingEntity = new TestEntity { Name = "EndValue" };
        
        compiled(matchingEntity).Should().BeTrue();
        compiled(nonMatchingEntity).Should().BeFalse();
    }

    [Fact]
    public void GetMemberAccessExpression_DifferentPropertyTypes_WorksCorrectly()
    {
        // Arrange
        var boolProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.IsActive))!;
        var dateProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.CreatedAt))!;

        // Act
        var boolExpression = boolProperty.GetMemberAccessExpression<TestEntity, bool>();
        var dateExpression = dateProperty.GetMemberAccessExpression<TestEntity, DateTime>();

        // Assert
        var entity = new TestEntity { IsActive = true, CreatedAt = new DateTime(2024, 1, 1) };
        
        boolExpression.Compile()(entity).Should().BeTrue();
        dateExpression.Compile()(entity).Should().Be(new DateTime(2024, 1, 1));
    }

    [Fact]
    public void ContainsPredicate_EmptyFilter_MatchesAll()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        var filter = string.Empty;

        // Act
        var predicate = property.ContainsPredicate<TestEntity>(filter);

        // Assert
        var compiled = predicate.Compile();
        var entity1 = new TestEntity { Name = "Test" };
        var entity2 = new TestEntity { Name = "Other" };
        var entity3 = new TestEntity { Name = "" };
        
        compiled(entity1).Should().BeTrue();
        compiled(entity2).Should().BeTrue();
        compiled(entity3).Should().BeTrue();
    }

    [Fact]
    public void EqualsPredicate_CaseSensitive_WorksCorrectly()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        var filter = "Test";

        // Act
        var predicate = property.EqualsPredicate<TestEntity>(filter);

        // Assert
        var compiled = predicate.Compile();
        var exactMatch = new TestEntity { Name = "Test" };
        var differentCase = new TestEntity { Name = "test" };
        
        compiled(exactMatch).Should().BeTrue();
        compiled(differentCase).Should().BeFalse();
    }

    [Fact]
    public void GetMemberAccessExpressionWithConversion_IntToObject_ReturnsBoxedValue()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Id))!;

        // Act
        var expression = property.GetMemberAccessExpressionWithConversion<TestEntity, object>();

        // Assert
        var entity = new TestEntity { Id = 999 };
        var compiled = expression.Compile();
        var result = compiled(entity);
        result.Should().BeOfType<int>();
        result.Should().Be(999);
    }

    [Fact]
    public void GetMemberAccessExpressionWithConversion_SameType_NoConversion()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;

        // Act
        var expression = property.GetMemberAccessExpressionWithConversion<TestEntity, string>();

        // Assert
        var entity = new TestEntity { Name = "NoConversion" };
        var compiled = expression.Compile();
        var result = compiled(entity);
        result.Should().Be("NoConversion");
    }

    [Fact]
    public void GetValueAccessExpressionWithConversion_DoubleToObject_ReturnsBoxedValue()
    {
        // Arrange
        var entity = new TestEntity { Amount = 123.45 };
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Amount))!;

        // Act
        var expression = property.GetValueAccessExpressionWithConversion<TestEntity, object>(entity);

        // Assert
        var compiled = expression.Compile();
        var result = compiled();
        result.Should().BeOfType<double>();
        result.Should().Be(123.45);
    }

    [Fact]
    public void GetValueAccessExpression_BoolProperty_ReturnsCorrectValue()
    {
        // Arrange
        var entity = new TestEntity { IsActive = true };
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.IsActive))!;

        // Act
        var expression = property.GetValueAccessExpression<TestEntity, bool>(entity);

        // Assert
        var compiled = expression.Compile();
        var result = compiled();
        result.Should().BeTrue();
    }

    [Fact]
    public void GetValueAccessExpression_DateTimeProperty_ReturnsCorrectValue()
    {
        // Arrange
        var expectedDate = new DateTime(2024, 12, 25, 10, 30, 0);
        var entity = new TestEntity { CreatedAt = expectedDate };
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.CreatedAt))!;

        // Act
        var expression = property.GetValueAccessExpression<TestEntity, DateTime>(entity);

        // Assert
        var compiled = expression.Compile();
        var result = compiled();
        result.Should().Be(expectedDate);
    }

    [Fact]
    public void IsDisplayable_IntProperty_ReturnsTrue()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Id))!;

        // Act
        var result = property.IsDisplayable();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDisplayable_BoolProperty_ReturnsTrue()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.IsActive))!;

        // Act
        var result = property.IsDisplayable();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDisplayable_DateTimeProperty_ReturnsTrue()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.CreatedAt))!;

        // Act
        var result = property.IsDisplayable();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsPredicate_PartialMatch_ReturnsTrue()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        var filter = "middle";

        // Act
        var predicate = property.ContainsPredicate<TestEntity>(filter);

        // Assert
        var compiled = predicate.Compile();
        var entity = new TestEntity { Name = "start_middle_end" };
        compiled(entity).Should().BeTrue();
    }

    [Fact]
    public void StartsWithPredicate_EmptyFilter_MatchesAll()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        var filter = string.Empty;

        // Act
        var predicate = property.StartsWithPredicate<TestEntity>(filter);

        // Assert
        var compiled = predicate.Compile();
        var entity = new TestEntity { Name = "AnyValue" };
        compiled(entity).Should().BeTrue();
    }

    [Fact]
    public void EndsWithPredicate_EmptyFilter_MatchesAll()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        var filter = string.Empty;

        // Act
        var predicate = property.EndsWithPredicate<TestEntity>(filter);

        // Assert
        var compiled = predicate.Compile();
        var entity = new TestEntity { Name = "AnyValue" };
        compiled(entity).Should().BeTrue();
    }

    [Fact]
    public void NotContainsPredicate_EmptyFilter_MatchesNone()
    {
        // Arrange
        var property = typeof(TestEntity).GetProperty(nameof(TestEntity.Name))!;
        var filter = string.Empty;

        // Act
        var predicate = property.NotContainsPredicate<TestEntity>(filter);

        // Assert
        var compiled = predicate.Compile();
        // Empty string is contained in all strings, so NotContains returns false
        var entity = new TestEntity { Name = "Test" };
        compiled(entity).Should().BeFalse();
    }
}
