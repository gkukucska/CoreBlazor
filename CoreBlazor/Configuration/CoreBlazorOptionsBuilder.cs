using CoreBlazor.Authorization;
using CoreBlazor.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace CoreBlazor.Configuration;

public class CoreBlazorOptionsBuilder
{
    private readonly IEnumerable<DiscoveredContext> _contexts;

    public IServiceCollection Services { get; }

    internal CoreBlazorOptionsBuilder(IServiceCollection services, IEnumerable<DiscoveredContext> contexts)
    {
        Services = services;
        _contexts = contexts;
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

    public CoreBlazorOptionsBuilder WithAuthorizationCallback(Func<ActionInfo,ClaimsPrincipal,bool> callback)
    {
        Services.AddAuthorizationCore(options =>
        {
            foreach (var discoveredContext in _contexts)
            {
                options.AddPolicy(Policies.CanReadInfo(discoveredContext.ContextType), policy 
                    => policy.RequireAssertion(context => callback(new(DbAction.ReadInfo, discoveredContext.ContextType.Name,string.Empty),context.User)));
                foreach (var discoveredSet in discoveredContext.Sets)
                {
                    options.AddPolicy(Policies.CanRead(discoveredContext.ContextType, discoveredSet.EntityType), policy
                       => policy.RequireAssertion(context => callback(new(DbAction.ReadEntities, discoveredContext.ContextType.Name, discoveredSet.EntityType.Name), context.User)));
                    options.AddPolicy(Policies.CanCreate(discoveredContext.ContextType, discoveredSet.EntityType), policy
                       => policy.RequireAssertion(context => callback(new(DbAction.CreateEntity, discoveredContext.ContextType.Name, discoveredSet.EntityType.Name), context.User)));
                    options.AddPolicy(Policies.CanEdit(discoveredContext.ContextType, discoveredSet.EntityType), policy
                       => policy.RequireAssertion(context => callback(new(DbAction.EditEntity, discoveredContext.ContextType.Name, discoveredSet.EntityType.Name), context.User)));
                    options.AddPolicy(Policies.CanDelete(discoveredContext.ContextType, discoveredSet.EntityType), policy
                       => policy.RequireAssertion(context => callback(new(DbAction.DeleteEntity, discoveredContext.ContextType.Name, discoveredSet.EntityType.Name), context.User)));
                }
            }
        });
        return this;
    }
}