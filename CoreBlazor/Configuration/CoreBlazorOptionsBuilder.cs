using Microsoft.Extensions.DependencyInjection;

namespace CoreBlazor.Configuration;

public class CoreBlazorOptionsBuilder
{
    public IServiceCollection Services { get; }

    internal CoreBlazorOptionsBuilder(IServiceCollection services)
    {
        Services = services;
    }

}
