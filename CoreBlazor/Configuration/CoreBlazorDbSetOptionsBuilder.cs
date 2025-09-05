using CoreBlazor.Authorization;
using CoreBlazor.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;

namespace CoreBlazor.Configuration;

public abstract class CoreBlazorDbSetOptionsBuilder<TEntity> where TEntity : class
{
    protected readonly IServiceCollection _services;
    private readonly Type _contextType;
    private readonly Type _entityType;

    protected CoreBlazorDbSetOptionsBuilder(CoreBlazorDbSetOptions<TEntity> options, IServiceCollection services, Type contextType)
    {
        Options = options;
        _services = services;
        _contextType = contextType;
        _entityType = typeof(TEntity);
    }
    internal CoreBlazorDbSetOptions<TEntity> Options { get; }
    public CoreBlazorDbSetOptionsBuilder<TEntity> WithEntityDisplay(Func<TEntity, string> displayColumnMethod)
    {
        Options.StringDisplay = displayColumnMethod;
        return this;
    }

    public CoreBlazorDbSetOptionsBuilder<TEntity> WithEntityDisplay<TDisplay>() where TDisplay : IEntityDisplayComponent<TEntity>
    {
        Options.ComponentDisplay = typeof(TDisplay);
        return this;
    }
    public CoreBlazorPropertyOptionsBuilder<TEntity, TProperty> ConfigureProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyAccessor)
    {
        if (propertyAccessor is not { Body: MemberExpression { Member: PropertyInfo property } })
        {
            throw new ArgumentException("Property accessor must be a simple member expression", nameof(propertyAccessor));
        }
        return new CoreBlazorPropertyOptionsBuilder<TEntity, TProperty>(property, this);
    }

    public CoreBlazorDbSetOptionsBuilder<TEntity> WithEntityDisplay(Type displayType)
    {
        if (!typeof(IEntityDisplayComponent<TEntity>).IsAssignableFrom(displayType))
        {
            throw new ArgumentException($"Display type must implement {typeof(IEntityDisplayComponent<TEntity>).Name}>", nameof(displayType));
        }
        Options.ComponentDisplay = displayType;
        return this;
    }

    public CoreBlazorDbSetOptionsBuilder<TEntity> WithTitle(string title)
    {
        Options.DisplayTitle = title;
        ConfigurationHelper.DisplayTitles.Add(typeof(TEntity).Name, title);
        return this;
    }

    public CoreBlazorDbSetOptionsBuilder<TEntity> UserCanReadIf(Predicate<ClaimsPrincipal> predicate)
    {
        _services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies.CanRead(_contextType,_entityType), policy
                => policy.RequireAssertion(context => predicate(context.User)));
        });
        return this;
    }

    public CoreBlazorDbSetOptionsBuilder<TEntity> UserCanCreateIf(Predicate<ClaimsPrincipal> predicate)
    {
        _services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies.CanCreate(_contextType, _entityType), policy
                => policy.RequireAssertion(context => predicate(context.User)));
        });
        return this;
    }

    public CoreBlazorDbSetOptionsBuilder<TEntity> UserCanEditIf(Predicate<ClaimsPrincipal> predicate)
    {
        _services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies.CanEdit(_contextType, _entityType), policy
                => policy.RequireAssertion(context => predicate(context.User)));
        });
        return this;
    }

    public CoreBlazorDbSetOptionsBuilder<TEntity> UserCanDeleteIf(Predicate<ClaimsPrincipal> predicate)
    {
        _services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies.CanDelete(_contextType, _entityType), policy
                => policy.RequireAssertion(context => predicate(context.User)));
        });
        return this;
    }
}

public class CoreBlazorDbSetOptionsBuilder<TContext, TEntity>(IServiceCollection services) : CoreBlazorDbSetOptionsBuilder<TEntity>(new CoreBlazorDbSetOptions<TContext, TEntity>(), services, typeof(TContext)) where TContext : DbContext where TEntity : class;