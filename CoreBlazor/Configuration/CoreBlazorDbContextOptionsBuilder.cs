using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreBlazor.Configuration;

public class CoreBlazorDbContextOptionsBuilder<TContext> where TContext : DbContext 
{
    public CoreBlazorDbContextOptionsBuilder(IServiceCollection services, CoreBlazorOptionsBuilder coreBlazorOptionsBuilder)
    {
        Services = services;
        CoreBlazorOptionsBuilder = coreBlazorOptionsBuilder;
    }

    internal IServiceCollection Services { get; }
    internal CoreBlazorOptionsBuilder CoreBlazorOptionsBuilder { get; }
    internal CoreBlazorDbContextOptions<TContext> Options { get; } = new();
}
