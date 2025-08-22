using Microsoft.EntityFrameworkCore;

namespace CoreBlazor.Configuration;

public class CoreBlazorDbSetOptionsBuilder<TContext, T> where TContext : DbContext where T : class
{
    internal CoreBlazorDbSetOptions<TContext,T> Options { get; } = new();
}
