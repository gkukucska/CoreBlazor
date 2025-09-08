using Microsoft.EntityFrameworkCore;

namespace CoreBlazor.Configuration;

public class CoreBlazorDbContextOptions<TContext> where TContext: DbContext
{
    public string DisplayTitle { get; set; } = typeof(TContext).Name;

    public bool UseSplitQueries { get; set; }
}
