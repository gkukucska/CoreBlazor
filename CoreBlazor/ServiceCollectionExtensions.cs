using CoreBlazor.Configuration;
using CoreBlazor.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreBlazor;

public static class ServiceCollectionExtensions
{
    public static CoreBlazorOptionsBuilder AddCoreBlazor(this IServiceCollection services)
    {
        var discoveredContexts = new List<DiscoveredContext>();
        foreach (var descriptor in services.Where(x => typeof(DbContext).IsAssignableFrom(x.ServiceType)).ToList())
        {
            var type = descriptor.ImplementationType ?? descriptor.ServiceType;
            var sets = type.GetDbSets()
                .Select(x => x.PropertyType.GetGenericArguments()[0])
                .Select(x => new DiscoveredSet() { EntityType = x });
            var context = new DiscoveredContext()
            {
                ContextType = type,
                Sets = [.. sets]
            };
            services.AddSingleton(_ => context);
            discoveredContexts.Add(context);
        }
        return new CoreBlazorOptionsBuilder(services, discoveredContexts).WithAuthorizationCallback((_,_)=>true);
    }
}
