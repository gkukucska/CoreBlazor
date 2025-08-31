using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreBlazor.Configuration;

public class CoreBlazorOptionsBuilder
{
    public IServiceCollection Services { get; }

    internal CoreBlazorOptionsBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public CoreBlazorOptionsBuilder ConfigureContext<TContext>(Action<CoreBlazorDbContextOptionsBuilder<TContext>> optionsBuilder) where TContext : DbContext
    {
        var contextOptionsBuilder = new CoreBlazorDbContextOptionsBuilder<TContext>(Services, this);
        optionsBuilder(contextOptionsBuilder);
        Services.AddSingleton(contextOptionsBuilder.Options);
        return this;
    }
    public CoreBlazorOptionsBuilder ConfigureContext<TContext>(CoreBlazorDbContextOptions<TContext> dbContextOptions) where TContext : DbContext
    {
        Services.AddSingleton(dbContextOptions);
        return this;
    }
}
