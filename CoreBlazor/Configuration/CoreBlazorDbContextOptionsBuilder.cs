using CoreBlazor.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;

namespace CoreBlazor.Configuration;

public class CoreBlazorDbContextOptionsBuilder<TContext> where TContext : DbContext
{
    internal IServiceCollection Services { get; }
    internal CoreBlazorOptionsBuilder CoreBlazorOptionsBuilder { get; }
    internal CoreBlazorDbContextOptions<TContext> Options { get; } = new();

    public CoreBlazorDbContextOptionsBuilder(IServiceCollection services, CoreBlazorOptionsBuilder coreBlazorOptionsBuilder)
    {
        Services = services;
        CoreBlazorOptionsBuilder = coreBlazorOptionsBuilder;
    }


    public CoreBlazorDbContextOptionsBuilder<TContext> ConfigureSet<TEntity>(Expression<Func<TContext, DbSet<TEntity>>> propertyAccessor, CoreBlazorDbSetOptions<TContext, TEntity> options)where TEntity : class
    {
        if (propertyAccessor is not { Body: MemberExpression { Member: PropertyInfo property } })
        {
            throw new ArgumentException("Property accessor must be a simple member expression", nameof(propertyAccessor));
        }
        Services.AddSingleton(options);
        return this;
    }


    public CoreBlazorDbContextOptionsBuilder<TContext> ConfigureSet<TEntity>(Expression<Func<TContext, DbSet<TEntity>>> propertyAccessor, Action<CoreBlazorDbSetOptionsBuilder<TContext, TEntity>> optionsBuilder) where TEntity : class
    {
        if (propertyAccessor is not { Body: MemberExpression { Member: PropertyInfo property } })
        {
            throw new ArgumentException("Property accessor must be a simple member expression", nameof(propertyAccessor));
        }
        var setOptionsBuilder = new CoreBlazorDbSetOptionsBuilder<TContext, TEntity>(Services);
        optionsBuilder(setOptionsBuilder);
        Services.AddSingleton(setOptionsBuilder.Options as CoreBlazorDbSetOptions<TContext, TEntity>);
        return this;
    }


    public CoreBlazorDbContextOptionsBuilder<TContext> ConfigureSet< TEntity>(CoreBlazorDbSetOptions<TContext, TEntity> options) where TEntity : class
    {
        Services.AddSingleton(options);
        return this;
    }


    public CoreBlazorDbContextOptionsBuilder<TContext> ConfigureSet<TEntity>(Action<CoreBlazorDbSetOptionsBuilder<TContext, TEntity>> optionsBuilder) where TEntity : class
    {
        var setOptionsBuilder = new CoreBlazorDbSetOptionsBuilder<TContext, TEntity>(Services);
        optionsBuilder(setOptionsBuilder);
        Services.AddSingleton(setOptionsBuilder.Options as CoreBlazorDbSetOptions<TContext, TEntity>);
        return this;
    }

    public CoreBlazorDbContextOptionsBuilder<TContext> WithTitle(string title)
    {
        Options.DisplayTitle = title;
        ConfigurationHelper.DisplayTitles.Add(typeof(TContext).Name, title);
        return this;
    }

    public CoreBlazorOptionsBuilder ConfigureContext(Action<CoreBlazorDbContextOptionsBuilder<TContext>> optionsBuilder)
    {
        return CoreBlazorOptionsBuilder.ConfigureContext(optionsBuilder);
    }
    public CoreBlazorOptionsBuilder ConfigureContext(CoreBlazorDbContextOptions<TContext> options)
    {
        return CoreBlazorOptionsBuilder.ConfigureContext(options);
    }

    public CoreBlazorDbContextOptionsBuilder<TContext> UserCanReadIf(Predicate<ClaimsPrincipal> predicate)
    {
        Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Policies.CanReadInfo(typeof(TContext)), policy
                => policy.RequireAssertion(context => predicate(context.User)));
        });
        return this;
    }
}
