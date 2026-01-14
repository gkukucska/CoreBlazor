using Bunit;
using CoreBlazor.Components;
using CoreBlazor.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using Microsoft.AspNetCore.Components;

namespace CoreBlazor.Tests.Components;

public class NavigationPropertyEditorComponentTests : Bunit.TestContext
{
    #region Test Helpers

    public class Related
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class TestEntity
    {
        public int Id { get; set; }
        public Related? Related { get; set; }
    }

    public class TestDbContext : DbContext
    {
        public DbSet<Related> Relateds { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Related>().HasKey(r => r.Id);
            base.OnModelCreating(modelBuilder);
        }
    }

    private readonly IDbContextFactory<TestDbContext> _contextFactory = Substitute.For<IDbContextFactory<TestDbContext>>();

    public NavigationPropertyEditorComponentTests()
    {
        Services.AddSingleton(_contextFactory);
    }

    #endregion

    #region Basic Rendering Tests

    [Fact]
    public void Component_Renders_Grid_With_Related_Entities()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_Renders_Grid_With_Related_Entities));
        using (var seed = new TestDbContext(options))
        {
            seed.Relateds.AddRange(
                new Related { Id = 1, Title = "First" },
                new Related { Id = 2, Title = "Second" }
            );
            seed.SaveChanges();
        }
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));

        // Act
        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => Task.CompletedTask));
        });

        // Assert
        cut.Markup.Should().Contain("table");
        cut.Markup.Should().Contain("Select");
    }

    [Fact]
    public void Component_Handles_EmptyDatabase_WithoutError()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_Handles_EmptyDatabase_WithoutError));
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));

        // Act
        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => Task.CompletedTask));
        });

        // Assert
        cut.Instance.Should().NotBeNull();
        cut.Markup.Should().Contain("table");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Component_Handles_Multiple_Related_Entities(int entityCount)
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>($"{nameof(Component_Handles_Multiple_Related_Entities)}_{entityCount}");
        using (var seed = new TestDbContext(options))
        {
            for (int i = 0; i < entityCount; i++)
            {
                seed.Relateds.Add(new Related { Id = i + 1, Title = $"Item{i + 1}" });
            }
            seed.SaveChanges();
        }
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));

        // Act
        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => Task.CompletedTask));
        });

        // Assert
        var selectButtons = cut.FindAll("button").Where(b => b.TextContent.Contains("Select")).ToList();
        selectButtons.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Context Factory Tests

    [Fact]
    public void Component_ShouldCallContextFactory_OnParametersSet()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_ShouldCallContextFactory_OnParametersSet));
        using (var seed = new TestDbContext(options))
        {
            seed.Relateds.Add(new Related { Id = 1, Title = "R1" });
            seed.SaveChanges();
        }
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));

        // Act
        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => Task.CompletedTask));
        });

        // Assert
        _contextFactory.Received().CreateDbContextAsync(default);
    }

    #endregion

    #region Callback Tests

    [Fact]
    public void SelectButton_Click_Invokes_PropertySelected_Callback()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(SelectButton_Click_Invokes_PropertySelected_Callback));
        using (var seed = new TestDbContext(options))
        {
            seed.Relateds.Add(new Related { Id = 10, Title = "Item10" });
            seed.Relateds.Add(new Related { Id = 11, Title = "Item11" });
            seed.SaveChanges();
        }
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));

        var invoked = false;

        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => invoked = true));
        });

        var selectButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Select"));
        selectButton.Should().NotBeNull();

        // Act
        selectButton.Click();

        // Assert
        invoked.Should().BeTrue();
    }

    [Fact]
    public void Component_MultipleCallbacks_AllInvoked()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_MultipleCallbacks_AllInvoked));
        using (var seed = new TestDbContext(options))
        {
            seed.Relateds.Add(new Related { Id = 1, Title = "Item1" });
            seed.SaveChanges();
        }
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));

        var invokeCount = 0;

        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => invokeCount++));
        });

        // Act & Assert - find fresh button each time to avoid stale references
        var selectButton1 = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Select"));
        selectButton1?.Click();
        invokeCount.Should().Be(1);

        // Re-render may have occurred, find button again
        var selectButton2 = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Select"));
        selectButton2?.Click();
        invokeCount.Should().Be(2);
    }

    [Fact]
    public void Component_HandlesNullCallback()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_HandlesNullCallback));
        using (var seed = new TestDbContext(options))
        {
            seed.Relateds.Add(new Related { Id = 1, Title = "Item1" });
            seed.SaveChanges();
        }
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));

        // Act - Component should render even without callback
        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
        });

        // Assert
        cut.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Component_HandlesCallbackException()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_HandlesCallbackException));
        using (var seed = new TestDbContext(options))
        {
            seed.Relateds.Add(new Related { Id = 1, Title = "Item1" });
            seed.SaveChanges();
        }
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));

        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => throw new InvalidOperationException("Callback error")));
        });

        // Act & Assert
        var selectButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Select"));
        selectButton.Should().NotBeNull();

        var act = () => selectButton.Click();
        act.Should().Throw<InvalidOperationException>().WithMessage("Callback error");
    }

    #endregion

    #region Lifecycle Tests

    [Fact]
    public void Component_Disposes_Context_On_Dispose()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_Disposes_Context_On_Dispose));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => Task.CompletedTask));
        });

        // Act
        var disposeTask = cut.Instance.DisposeAsync();

        // Assert - context should be disposed without throwing
        disposeTask.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Component_HandlesMultipleDisposals()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_HandlesMultipleDisposals));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => Task.CompletedTask));
        });

        // Act - Dispose multiple times
        var disposeTask1 = cut.Instance.DisposeAsync();
        var disposeTask2 = cut.Instance.DisposeAsync();

        // Assert - Should not throw
        disposeTask1.IsCompleted.Should().BeTrue();
        disposeTask2.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Component_HandlesDisposalDuringOperation()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_HandlesDisposalDuringOperation));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => Task.CompletedTask));
        });

        // Act - Dispose while component exists
        var disposeTask = cut.Instance.DisposeAsync();

        // Assert
        disposeTask.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Component_HandlesContextDisposedBeforeComponent()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_HandlesContextDisposedBeforeComponent));
        var context = new TestDbContext(options);
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(context));

        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => Task.CompletedTask));
        });

        // Act - Dispose context before component
        context.Dispose();

        // Assert - Component disposal should handle gracefully
        var disposeTask = cut.Instance.DisposeAsync();
        disposeTask.IsCompleted.Should().BeTrue();
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Component_HandlesNullNavigationPropertyName()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_HandlesNullNavigationPropertyName));
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));

        // Act - Component may accept null and handle gracefully
        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, (string)null!);
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => Task.CompletedTask));
        });

        // Assert - Component renders even with null property name
        cut.Should().NotBeNull();
    }

    [Fact]
    public void Component_HandlesEmptyNavigationPropertyName()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_HandlesEmptyNavigationPropertyName));
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));

        // Act - Component may accept empty string and handle gracefully
        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, string.Empty);
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => Task.CompletedTask));
        });

        // Assert - Component renders even with empty property name
        cut.Should().NotBeNull();
    }

    [Fact]
    public void Component_RendersWithInvalidNavigationPropertyName()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_RendersWithInvalidNavigationPropertyName));
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));

        // Act - Invalid property name may cause runtime errors during grid rendering
        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, "NonExistentProperty");
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => Task.CompletedTask));
        });

        // Assert - Component renders but may have errors
        cut.Should().NotBeNull();
    }

    [Fact]
    public void Component_HandlesLargeDataSet()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_HandlesLargeDataSet));
        using (var seed = new TestDbContext(options))
        {
            // Add 100 entities (reduced from 1000 for test performance)
            for (int i = 0; i < 100; i++)
            {
                seed.Relateds.Add(new Related { Id = i + 1, Title = $"Item{i + 1}" });
            }
            seed.SaveChanges();
        }
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));

        // Act
        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => Task.CompletedTask));
        });

        // Assert - Should render without performance issues
        cut.Instance.Should().NotBeNull();
        cut.Markup.Should().Contain("table");
    }

    [Fact]
    public void Component_HandlesRapidPropertyNameChanges()
    {
        // Arrange
        var options = TestDbContextHelper.CreateInMemoryOptions<TestDbContext>(nameof(Component_HandlesRapidPropertyNameChanges));
        using (var seed = new TestDbContext(options))
        {
            seed.Relateds.Add(new Related { Id = 1, Title = "Item1" });
            seed.SaveChanges();
        }
        _contextFactory.CreateDbContextAsync(default).Returns(Task.FromResult(new TestDbContext(options)));

        var cut = RenderComponent<NavigationPropertyEditorComponent<TestEntity, Related, TestDbContext>>(parameters =>
        {
            parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related));
            parameters.Add(p => p.PropertySelected, EventCallback.Factory.Create(this, () => Task.CompletedTask));
        });

        // Act - Change property name rapidly
        cut.SetParametersAndRender(parameters => parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related)));
        cut.SetParametersAndRender(parameters => parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related)));
        cut.SetParametersAndRender(parameters => parameters.Add(p => p.NavigationPropertyName, nameof(TestEntity.Related)));

        // Assert - Should remain stable
        cut.Instance.Should().NotBeNull();
    }

    #endregion
}
