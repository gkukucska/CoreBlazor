using CoreBlazor.Authorization;
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

    public CoreBlazorOptionsBuilder WithAuthorizationCallback(Func<AuthorizationInfo,ClaimsPrincipal,bool> callback)
    {

        Services.AddCascadingAuthenticationState();
        Services.AddAuthorizationCore(options =>
        {
            foreach (var discoveredContext in _contexts)
            {
                options.AddPolicy($"{discoveredContext.ContextType.Name}/{Identifiers.Info}", policy 
                    => policy.RequireAssertion(context => callback(new(DbContextAction.ReadInfo, discoveredContext.ContextType.Name,string.Empty),context.User)));
                foreach (var discoveredSet in discoveredContext.Sets)
                {
                    options.AddPolicy($"{discoveredContext.ContextType.Name}/{discoveredSet.EntityType.Name}/{Identifiers.Read}", policy
                       => policy.RequireAssertion(context => callback(new(DbContextAction.ReadEntities, discoveredContext.ContextType.Name, discoveredSet.EntityType.Name), context.User)));
                    options.AddPolicy($"{discoveredContext.ContextType.Name}/{discoveredSet.EntityType.Name}/{Identifiers.Create}", policy
                       => policy.RequireAssertion(context => callback(new(DbContextAction.CreateEntity, discoveredContext.ContextType.Name, discoveredSet.EntityType.Name), context.User)));
                    options.AddPolicy($"{discoveredContext.ContextType.Name}/{discoveredSet.EntityType.Name}/{Identifiers.Edit}", policy
                       => policy.RequireAssertion(context => callback(new(DbContextAction.EditEntity, discoveredContext.ContextType.Name, discoveredSet.EntityType.Name), context.User)));
                    options.AddPolicy($"{discoveredContext.ContextType.Name}/{discoveredSet.EntityType.Name}/{Identifiers.Delete}", policy
                       => policy.RequireAssertion(context => callback(new(DbContextAction.DeleteEntity, discoveredContext.ContextType.Name, discoveredSet.EntityType.Name), context.User)));
                    options.AddPolicy($"{discoveredContext.ContextType.Name}/{discoveredSet.EntityType.Name}/{Identifiers.EditOrDelete}", policy
                       => policy.RequireAssertion(context => callback(new(DbContextAction.EditEntity, discoveredContext.ContextType.Name, discoveredSet.EntityType.Name), context.User) ||
                                                             callback(new(DbContextAction.DeleteEntity, discoveredContext.ContextType.Name, discoveredSet.EntityType.Name), context.User)));
                }
            }
        });
        return this;
    }
}
public static class Identifiers
{
    public const string Info = nameof(Info);
    public const string Read = nameof(Read);
    public const string Create = nameof(Create);
    public const string Edit = nameof(Edit);
    public const string Delete = nameof(Delete);
    public const string EditOrDelete = nameof(EditOrDelete);
}