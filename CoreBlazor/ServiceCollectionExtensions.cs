using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreBlazor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreBlazor(this IServiceCollection services)
    {
        foreach (var descriptor in services.Where(x=>typeof(DbContext).IsAssignableFrom(x.ServiceType)).ToList())
        {
            var type = descriptor.ImplementationType ?? descriptor.ServiceType;
            var sets = type.GetProperties().Where(x=>x.PropertyType.Name.StartsWith("DbSet") && x.PropertyType.GenericTypeArguments.Length==1)
                .Select(x=>x.PropertyType.GetGenericArguments()[0])
                .Select(x=> new DiscoveredSet(){EntityType = x});
            var context = new DiscoveredContext()
            {
                ContextType = type,
                Sets = sets.ToList()
            };
            services.AddSingleton(_=> context);
        }
        return services;
    }
}