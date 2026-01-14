using Microsoft.EntityFrameworkCore;

namespace CoreBlazor.Tests.TestHelpers;

/// <summary>
/// Helper for creating in-memory test databases with unique names
/// </summary>
public static class TestDbContextHelper
{
    /// <summary>
    /// Creates DbContextOptions for an in-memory database with a unique name
    /// </summary>
    public static DbContextOptions<TContext> CreateInMemoryOptions<TContext>(string? testName = null) 
        where TContext : DbContext
    {
        var dbName = $"{typeof(TContext).Name}_{testName ?? "Test"}_{Guid.NewGuid()}";
        return new DbContextOptionsBuilder<TContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
    }
}
