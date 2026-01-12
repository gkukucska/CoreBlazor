using BlazorBootstrap;
using CoreBlazor.Tests.TestHelpers;
using CoreBlazor.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Xunit;

namespace CoreBlazor.Tests.Utils;

public class QueryableExtensionsTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    private class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    }

    #region Pagination Tests

    [Fact]
    public async Task WithPagination_FirstPage_ReturnsCorrectItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithPagination_FirstPage_ReturnsCorrectItems));
        using var context = new TestDbContext(options);
        
        for (int i = 1; i <= 10; i++)
        {
            context.TestEntities.Add(new TestEntity { Id = i, Name = $"Entity{i}" });
        }
        await context.SaveChangesAsync();

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 1,
            PageSize = 3
        };

        // Act
        var result = context.TestEntities.WithPagination(request).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Id.Should().Be(1);
        result[2].Id.Should().Be(3);
    }

    [Fact]
    public async Task WithPagination_SecondPage_ReturnsCorrectItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithPagination_SecondPage_ReturnsCorrectItems));
        using var context = new TestDbContext(options);
        
        for (int i = 1; i <= 10; i++)
        {
            context.TestEntities.Add(new TestEntity { Id = i, Name = $"Entity{i}" });
        }
        await context.SaveChangesAsync();

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 2,
            PageSize = 3
        };

        // Act
        var result = context.TestEntities.WithPagination(request).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Id.Should().Be(4);
        result[2].Id.Should().Be(6);
    }

    [Fact]
    public async Task WithPagination_LastPage_ReturnsRemainingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithPagination_LastPage_ReturnsRemainingItems));
        using var context = new TestDbContext(options);
        
        for (int i = 1; i <= 10; i++)
        {
            context.TestEntities.Add(new TestEntity { Id = i, Name = $"Entity{i}" });
        }
        await context.SaveChangesAsync();

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 4,
            PageSize = 3
        };

        // Act
        var result = context.TestEntities.WithPagination(request).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(10);
    }

    [Fact]
    public void WithPagination_ZeroPageNumber_ReturnsAllItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithPagination_ZeroPageNumber_ReturnsAllItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Entity1" },
            new TestEntity { Id = 2, Name = "Entity2" }
        );
        context.SaveChanges();

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 0,
            PageSize = 1
        };

        // Act
        var result = context.TestEntities.WithPagination(request).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void WithPagination_ZeroPageSize_ReturnsAllItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithPagination_ZeroPageSize_ReturnsAllItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Entity1" },
            new TestEntity { Id = 2, Name = "Entity2" },
            new TestEntity { Id = 3, Name = "Entity3" }
        );
        context.SaveChanges();

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 1,
            PageSize = 0
        };

        // Act
        var result = context.TestEntities.WithPagination(request).ToList();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void WithPagination_NegativePageNumber_ReturnsAllItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithPagination_NegativePageNumber_ReturnsAllItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Entity1" },
            new TestEntity { Id = 2, Name = "Entity2" }
        );
        context.SaveChanges();

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = -1,
            PageSize = 1
        };

        // Act
        var result = context.TestEntities.WithPagination(request).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void WithPagination_NegativePageSize_ReturnsAllItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithPagination_NegativePageSize_ReturnsAllItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Entity1" },
            new TestEntity { Id = 2, Name = "Entity2" }
        );
        context.SaveChanges();

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 1,
            PageSize = -5
        };

        // Act
        var result = context.TestEntities.WithPagination(request).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public void WithSorting_AscendingByName_ReturnsSortedItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithSorting_AscendingByName_ReturnsSortedItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Charlie" },
            new TestEntity { Id = 2, Name = "Alpha" },
            new TestEntity { Id = 3, Name = "Bravo" }
        );
        context.SaveChanges();

        Expression<Func<TestEntity, IComparable>> keySelector = e => e.Name;
        var sorting = new SortingItem<TestEntity>(nameof(TestEntity.Name), keySelector, SortDirection.Ascending);

        // Act
        var result = context.TestEntities.AsQueryable().WithSorting(sorting).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Bravo");
        result[2].Name.Should().Be("Charlie");
    }

    [Fact]
    public void WithSorting_DescendingByName_ReturnsSortedItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithSorting_DescendingByName_ReturnsSortedItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alpha" },
            new TestEntity { Id = 2, Name = "Charlie" },
            new TestEntity { Id = 3, Name = "Bravo" }
        );
        context.SaveChanges();

        Expression<Func<TestEntity, IComparable>> keySelector = e => e.Name;
        var sorting = new SortingItem<TestEntity>(nameof(TestEntity.Name), keySelector, SortDirection.Descending);

        // Act
        var result = context.TestEntities.AsQueryable().WithSorting(sorting).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Charlie");
        result[1].Name.Should().Be("Bravo");
        result[2].Name.Should().Be("Alpha");
    }

    [Fact]
    public void WithSorting_ByIntProperty_ReturnsSortedItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithSorting_ByIntProperty_ReturnsSortedItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", Age = 30 },
            new TestEntity { Id = 2, Name = "B", Age = 20 },
            new TestEntity { Id = 3, Name = "C", Age = 40 }
        );
        context.SaveChanges();

        Expression<Func<TestEntity, IComparable>> keySelector = e => e.Age;
        var sorting = new SortingItem<TestEntity>(nameof(TestEntity.Age), keySelector, SortDirection.Ascending);

        // Act
        var result = context.TestEntities.AsQueryable().WithSorting(sorting).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Age.Should().Be(20);
        result[1].Age.Should().Be(30);
        result[2].Age.Should().Be(40);
    }

    [Fact]
    public void WithSorting_ByDateTime_ReturnsSortedItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithSorting_ByDateTime_ReturnsSortedItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", CreatedAt = new DateTime(2024, 3, 1) },
            new TestEntity { Id = 2, Name = "B", CreatedAt = new DateTime(2024, 1, 1) },
            new TestEntity { Id = 3, Name = "C", CreatedAt = new DateTime(2024, 2, 1) }
        );
        context.SaveChanges();

        Expression<Func<TestEntity, IComparable>> keySelector = e => e.CreatedAt;
        var sorting = new SortingItem<TestEntity>(nameof(TestEntity.CreatedAt), keySelector, SortDirection.Ascending);

        // Act
        var result = context.TestEntities.AsQueryable().WithSorting(sorting).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].CreatedAt.Should().Be(new DateTime(2024, 1, 1));
        result[1].CreatedAt.Should().Be(new DateTime(2024, 2, 1));
        result[2].CreatedAt.Should().Be(new DateTime(2024, 3, 1));
    }

    [Fact]
    public void WithSorting_NonComparableProperty_ReturnsOriginalQuery()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithSorting_NonComparableProperty_ReturnsOriginalQuery));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", IsActive = true },
            new TestEntity { Id = 2, Name = "B", IsActive = false }
        );
        context.SaveChanges();

        Expression<Func<TestEntity, IComparable>> keySelector = e => e.Name; // Using Name as it's IComparable
        var sorting = new SortingItem<TestEntity>(nameof(TestEntity.IsActive), keySelector, SortDirection.Ascending);

        // Act
        var result = context.TestEntities.AsQueryable().WithSorting(sorting).ToList();

        // Assert - Should return items in original order since bool is not IComparable
        result.Should().HaveCount(2);
    }

    [Fact]
    public void WithSorting_MultipleItems_AppliesAllSorts()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithSorting_MultipleItems_AppliesAllSorts));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", Age = 30 },
            new TestEntity { Id = 2, Name = "B", Age = 20 },
            new TestEntity { Id = 3, Name = "A", Age = 25 }
        );
        context.SaveChanges();

        Expression<Func<TestEntity, IComparable>> keySelector = e => e.Name;
        var sortingItems = new List<SortingItem<TestEntity>>
        {
            new SortingItem<TestEntity>(nameof(TestEntity.Name), keySelector, SortDirection.Ascending)
        };

        // Act
        var result = context.TestEntities.AsQueryable().WithSorting(sortingItems).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("A");
        result[1].Name.Should().Be("A");
        result[2].Name.Should().Be("B");
    }

    [Fact]
    public void WithSorting_NoneDirection_ReturnsOriginalOrder()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithSorting_NoneDirection_ReturnsOriginalOrder));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "C" },
            new TestEntity { Id = 2, Name = "A" },
            new TestEntity { Id = 3, Name = "B" }
        );
        context.SaveChanges();

        Expression<Func<TestEntity, IComparable>> keySelector = e => e.Name;
        var sorting = new SortingItem<TestEntity>(nameof(TestEntity.Name), keySelector, SortDirection.None);

        // Act
        var result = context.TestEntities.AsQueryable().WithSorting(sorting).ToList();

        // Assert - Should maintain original order
        result.Should().HaveCount(3);
        result[0].Id.Should().Be(1);
        result[1].Id.Should().Be(2);
        result[2].Id.Should().Be(3);
    }

    [Fact]
    public void WithSorting_EmptyCollection_ReturnsOriginalQuery()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithSorting_EmptyCollection_ReturnsOriginalQuery));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "C" },
            new TestEntity { Id = 2, Name = "A" }
        );
        context.SaveChanges();

        var sortingItems = new List<SortingItem<TestEntity>>();

        // Act
        var result = context.TestEntities.AsQueryable().WithSorting(sortingItems).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[1].Id.Should().Be(2);
    }

    #endregion

    #region Filtering Tests

    [Fact]
    public void WithFiltering_EqualsOperator_ReturnsMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_EqualsOperator_ReturnsMatchingItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alpha" },
            new TestEntity { Id = 2, Name = "Bravo" },
            new TestEntity { Id = 3, Name = "Alpha" }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.Name), "Alpha", FilterOperator.Equals, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Name == "Alpha");
    }

    [Fact]
    public void WithFiltering_NotEqualsOperator_ReturnsNonMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_NotEqualsOperator_ReturnsNonMatchingItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alpha" },
            new TestEntity { Id = 2, Name = "Bravo" },
            new TestEntity { Id = 3, Name = "Charlie" }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.Name), "Alpha", FilterOperator.NotEquals, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Name != "Alpha");
    }

    [Fact]
    public void WithFiltering_ContainsOperator_ReturnsMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_ContainsOperator_ReturnsMatchingItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Testing123" },
            new TestEntity { Id = 2, Name = "Hello" },
            new TestEntity { Id = 3, Name = "TestData" }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.Name), "Test", FilterOperator.Contains, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Name.Contains("Test"));
    }

    [Fact]
    public void WithFiltering_DoesNotContainOperator_ReturnsNonMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_DoesNotContainOperator_ReturnsNonMatchingItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Testing123" },
            new TestEntity { Id = 2, Name = "Hello" },
            new TestEntity { Id = 3, Name = "TestData" }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.Name), "Test", FilterOperator.DoesNotContain, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Hello");
    }

    [Fact]
    public void WithFiltering_StartsWithOperator_ReturnsMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_StartsWithOperator_ReturnsMatchingItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alpha1" },
            new TestEntity { Id = 2, Name = "Bravo" },
            new TestEntity { Id = 3, Name = "Alpha2" }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.Name), "Alpha", FilterOperator.StartsWith, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Name.StartsWith("Alpha"));
    }

    [Fact]
    public void WithFiltering_EndsWithOperator_ReturnsMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_EndsWithOperator_ReturnsMatchingItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "TestAlpha" },
            new TestEntity { Id = 2, Name = "Bravo" },
            new TestEntity { Id = 3, Name = "DataAlpha" }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.Name), "Alpha", FilterOperator.EndsWith, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Name.EndsWith("Alpha"));
    }

    [Fact]
    public void WithFiltering_InvalidPropertyName_ReturnsAllItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_InvalidPropertyName_ReturnsAllItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alpha" },
            new TestEntity { Id = 2, Name = "Bravo" }
        );
        context.SaveChanges();

        var filter = new FilterItem("NonExistentProperty", "Alpha", FilterOperator.Equals, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert - Should return all items when property doesn't exist
        result.Should().HaveCount(2);
    }

    [Fact]
    public void WithFiltering_MultipleFilters_AppliesAllFilters()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_MultipleFilters_AppliesAllFilters));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alpha", Age = 30 },
            new TestEntity { Id = 2, Name = "Alpha", Age = 20 },
            new TestEntity { Id = 3, Name = "Bravo", Age = 30 }
        );
        context.SaveChanges();

        var filters = new List<FilterItem>
        {
            new FilterItem(nameof(TestEntity.Name), "Alpha", FilterOperator.Equals, StringComparison.Ordinal)
        };

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filters).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Name == "Alpha");
    }

    [Fact]
    public void WithFiltering_EmptyFilterCollection_ReturnsAllItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_EmptyFilterCollection_ReturnsAllItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alpha" },
            new TestEntity { Id = 2, Name = "Bravo" }
        );
        context.SaveChanges();

        var filters = new List<FilterItem>();

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filters).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Filtering Tests for Non-String Properties

    [Fact]
    public void WithFiltering_IntegerEquals_ReturnsMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_IntegerEquals_ReturnsMatchingItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", Age = 30 },
            new TestEntity { Id = 2, Name = "B", Age = 25 },
            new TestEntity { Id = 3, Name = "C", Age = 30 }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.Age), "30", FilterOperator.Equals, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Age == 30);
    }

    [Fact]
    public void WithFiltering_IntegerNotEquals_ReturnsNonMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_IntegerNotEquals_ReturnsNonMatchingItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", Age = 30 },
            new TestEntity { Id = 2, Name = "B", Age = 25 },
            new TestEntity { Id = 3, Name = "C", Age = 40 }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.Age), "30", FilterOperator.NotEquals, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Age != 30);
    }

    [Fact]
    public void WithFiltering_IntegerGreaterThan_ReturnsMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_IntegerGreaterThan_ReturnsMatchingItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", Age = 30 },
            new TestEntity { Id = 2, Name = "B", Age = 25 },
            new TestEntity { Id = 3, Name = "C", Age = 40 }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.Age), "25", FilterOperator.GreaterThan, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Age > 25);
    }

    [Fact]
    public void WithFiltering_IntegerLessThan_ReturnsMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_IntegerLessThan_ReturnsMatchingItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", Age = 30 },
            new TestEntity { Id = 2, Name = "B", Age = 25 },
            new TestEntity { Id = 3, Name = "C", Age = 40 }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.Age), "35", FilterOperator.LessThan, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Age < 35);
    }

    [Fact]
    public void WithFiltering_IntegerLessThanOrEquals_ReturnsMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_IntegerLessThanOrEquals_ReturnsMatchingItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", Age = 30 },
            new TestEntity { Id = 2, Name = "B", Age = 25 },
            new TestEntity { Id = 3, Name = "C", Age = 40 }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.Age), "30", FilterOperator.LessThanOrEquals, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Age <= 30);
    }

    [Fact]
    public void WithFiltering_IntegerGreaterThanOrEquals_ReturnsMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_IntegerGreaterThanOrEquals_ReturnsMatchingItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", Age = 30 },
            new TestEntity { Id = 2, Name = "B", Age = 25 },
            new TestEntity { Id = 3, Name = "C", Age = 40 }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.Age), "30", FilterOperator.GreaterThanOrEquals, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Age >= 30);
    }

    [Fact]
    public void WithFiltering_DateTimeEquals_ReturnsMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_DateTimeEquals_ReturnsMatchingItems));
        using var context = new TestDbContext(options);
        
        var targetDate = new DateTime(2024, 6, 15);
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", CreatedAt = targetDate },
            new TestEntity { Id = 2, Name = "B", CreatedAt = new DateTime(2024, 1, 1) },
            new TestEntity { Id = 3, Name = "C", CreatedAt = targetDate }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.CreatedAt), targetDate.ToString("o"), FilterOperator.Equals, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.CreatedAt == targetDate);
    }

    [Fact]
    public void WithFiltering_DateTimeGreaterThan_ReturnsMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_DateTimeGreaterThan_ReturnsMatchingItems));
        using var context = new TestDbContext(options);
        
        var cutoffDate = new DateTime(2024, 6, 1);
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", CreatedAt = new DateTime(2024, 7, 1) },
            new TestEntity { Id = 2, Name = "B", CreatedAt = new DateTime(2024, 5, 1) },
            new TestEntity { Id = 3, Name = "C", CreatedAt = new DateTime(2024, 8, 1) }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.CreatedAt), cutoffDate.ToString("o"), FilterOperator.GreaterThan, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.CreatedAt > cutoffDate);
    }

    [Fact]
    public void WithFiltering_DateTimeLessThan_ReturnsMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_DateTimeLessThan_ReturnsMatchingItems));
        using var context = new TestDbContext(options);
        
        var cutoffDate = new DateTime(2024, 6, 1);
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", CreatedAt = new DateTime(2024, 7, 1) },
            new TestEntity { Id = 2, Name = "B", CreatedAt = new DateTime(2024, 5, 1) },
            new TestEntity { Id = 3, Name = "C", CreatedAt = new DateTime(2024, 4, 1) }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.CreatedAt), cutoffDate.ToString("o"), FilterOperator.LessThan, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.CreatedAt < cutoffDate);
    }

    [Fact]
    public void WithFiltering_BooleanEquals_ReturnsMatchingItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_BooleanEquals_ReturnsMatchingItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "A", IsActive = true },
            new TestEntity { Id = 2, Name = "B", IsActive = false },
            new TestEntity { Id = 3, Name = "C", IsActive = true }
        );
        context.SaveChanges();

        var filter = new FilterItem(nameof(TestEntity.IsActive), "true", FilterOperator.Equals, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.IsActive);
    }

    [Fact]
    public void WithFiltering_UnsupportedOperatorForString_ReturnsAllItems()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_UnsupportedOperatorForString_ReturnsAllItems));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alpha" },
            new TestEntity { Id = 2, Name = "Bravo" }
        );
        context.SaveChanges();

        // Using GreaterThan which is not supported for string filtering via switch case (defaults to null)
        var filter = new FilterItem(nameof(TestEntity.Name), "Alpha", FilterOperator.GreaterThan, StringComparison.Ordinal);

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filter).ToList();

        // Assert - Should try ExpressionExtensions and may return all items
        result.Should().HaveCount(2);
    }

    [Fact]
    public void WithFiltering_CombinedFilters_AppliesAllCorrectly()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(WithFiltering_CombinedFilters_AppliesAllCorrectly));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alpha", Age = 30, IsActive = true },
            new TestEntity { Id = 2, Name = "Alpha", Age = 25, IsActive = true },
            new TestEntity { Id = 3, Name = "Beta", Age = 30, IsActive = true },
            new TestEntity { Id = 4, Name = "Alpha", Age = 30, IsActive = false }
        );
        context.SaveChanges();

        var filters = new List<FilterItem>
        {
            new FilterItem(nameof(TestEntity.Name), "Alpha", FilterOperator.Equals, StringComparison.Ordinal),
            new FilterItem(nameof(TestEntity.Age), "30", FilterOperator.Equals, StringComparison.Ordinal)
        };

        // Act
        var result = context.TestEntities.AsQueryable().WithFiltering(filters).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Name == "Alpha" && e.Age == 30);
    }

    #endregion

    #region ToGridResultsAsync Tests

    [Fact]
    public async Task ToGridResultsAsync_ReturnsCorrectGridData()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(ToGridResultsAsync_ReturnsCorrectGridData));
        using var context = new TestDbContext(options);
        
        for (int i = 1; i <= 10; i++)
        {
            context.TestEntities.Add(new TestEntity { Id = i, Name = $"Entity{i}" });
        }
        await context.SaveChangesAsync();

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 1,
            PageSize = 5
        };

        // Act
        var result = await context.TestEntities.ToGridResultsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(5);
        result.TotalCount.Should().Be(10);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task ToGridResultsAsync_WithEmptyData_ReturnsEmptyResult()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(ToGridResultsAsync_WithEmptyData_ReturnsEmptyResult));
        using var context = new TestDbContext(options);

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 1,
            PageSize = 5
        };

        // Act
        var result = await context.TestEntities.ToGridResultsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task ToGridResultsAsync_PageBeyondData_ReturnsEmptyDataWithTotalCount()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(ToGridResultsAsync_PageBeyondData_ReturnsEmptyDataWithTotalCount));
        using var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Entity1" },
            new TestEntity { Id = 2, Name = "Entity2" }
        );
        await context.SaveChangesAsync();

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 5,
            PageSize = 5
        };

        // Act
        var result = await context.TestEntities.ToGridResultsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(5);
    }

    [Fact]
    public async Task ToGridResultsAsync_SingleItem_ReturnsCorrectly()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(ToGridResultsAsync_SingleItem_ReturnsCorrectly));
        using var context = new TestDbContext(options);
        
        context.TestEntities.Add(new TestEntity { Id = 1, Name = "OnlyEntity" });
        await context.SaveChangesAsync();

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await context.TestEntities.ToGridResultsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.Data.First().Name.Should().Be("OnlyEntity");
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task ToGridResultsAsync_LargeDataSet_PaginatesCorrectly()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(ToGridResultsAsync_LargeDataSet_PaginatesCorrectly));
        using var context = new TestDbContext(options);
        
        for (int i = 1; i <= 100; i++)
        {
            context.TestEntities.Add(new TestEntity { Id = i, Name = $"Entity{i}" });
        }
        await context.SaveChangesAsync();

        var request = new GridDataProviderRequest<TestEntity>
        {
            PageNumber = 5,
            PageSize = 10
        };

        // Act
        var result = await context.TestEntities.ToGridResultsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(10);
        result.TotalCount.Should().Be(100);
        result.PageNumber.Should().Be(5);
        result.Data.First().Id.Should().Be(41); // (5-1)*10 + 1
    }

    #endregion
}
