using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;

namespace CoreBlazor.Configuration;

public static class ConfigurationExtensions
{
    internal static Dictionary<string,string> DisplayTitles { get; } = new();

    public static CoreBlazorOptionsBuilder ConfigureContext<TContext>(this CoreBlazorOptionsBuilder builder, CoreBlazorDbContextOptions<TContext> dbContextOptions) where TContext : DbContext
    {
        builder.Services.AddSingleton(dbContextOptions);
        return builder;
    }
    public static CoreBlazorOptionsBuilder ConfigureContext<TContext>(this CoreBlazorDbContextOptionsBuilder<TContext> builder, Action<CoreBlazorDbContextOptionsBuilder<TContext>> optionsBuilder) where TContext : DbContext
    {
        return builder.CoreBlazorOptionsBuilder.ConfigureContext(optionsBuilder);
    }
    public static CoreBlazorOptionsBuilder ConfigureContext<TContext>(this CoreBlazorDbContextOptionsBuilder<TContext> builder, CoreBlazorDbContextOptions<TContext> options) where TContext : DbContext
    {
        return builder.CoreBlazorOptionsBuilder.ConfigureContext(options);
    }
    public static CoreBlazorOptionsBuilder ConfigureContext<TContext>(this CoreBlazorOptionsBuilder builder, Action<CoreBlazorDbContextOptionsBuilder<TContext>> optionsBuilder) where TContext : DbContext
    {
        var contextOptionsBuilder = new CoreBlazorDbContextOptionsBuilder<TContext>(builder.Services, builder);
        optionsBuilder(contextOptionsBuilder);
        builder.Services.AddSingleton(contextOptionsBuilder.Options);
        return builder;
    }

    public static CoreBlazorDbContextOptionsBuilder<TContext> WithTitle<TContext>(this CoreBlazorDbContextOptionsBuilder<TContext> optionsBuilder, string title) where TContext : DbContext
    {
        optionsBuilder.Options.DisplayTitle = title;
        DisplayTitles.Add(typeof(TContext).Name, title);
        return optionsBuilder;
    }


    public static CoreBlazorDbContextOptionsBuilder<TContext> ConfigureSet<TContext, TEntity>(this CoreBlazorDbContextOptionsBuilder<TContext> contextOptionsBuilder, Expression<Func<TContext, DbSet<TEntity>>> propertyAccessor, CoreBlazorDbSetOptions<TEntity> options) where TContext : DbContext where TEntity : class
    {
        if (propertyAccessor is not { Body: MemberExpression { Member: PropertyInfo property } })
        {
            throw new ArgumentException("Property accessor must be a simple member expression", nameof(propertyAccessor));
        }
        contextOptionsBuilder.Services.AddSingleton(options);
        return contextOptionsBuilder;
    }


    public static CoreBlazorDbContextOptionsBuilder<TContext> ConfigureSet<TContext, TEntity>(this CoreBlazorDbContextOptionsBuilder<TContext> contextOptionsBuilder, Expression<Func<TContext, DbSet<TEntity>>> propertyAccessor, Action<CoreBlazorDbSetOptionsBuilder<TEntity>> optionsBuilder) where TContext : DbContext where TEntity : class
    {
        if (propertyAccessor is not{Body: MemberExpression { Member: PropertyInfo property } } )
        {
            throw new ArgumentException("Property accessor must be a simple member expression", nameof(propertyAccessor));
        }
        var setOptionsBuilder = new CoreBlazorDbSetOptionsBuilder<TEntity>();
        optionsBuilder(setOptionsBuilder);
        contextOptionsBuilder.Services.AddSingleton(setOptionsBuilder.Options);
        return contextOptionsBuilder;
    }

    public static CoreBlazorDbSetOptionsBuilder<TEntity> WithTitle<TEntity>(this CoreBlazorDbSetOptionsBuilder<TEntity> optionsBuilder, string title) where TEntity : class
    {
        optionsBuilder.Options.DisplayTitle = title;
        DisplayTitles.Add(typeof(TEntity).Name, title);
        return optionsBuilder;
    }

    public static CoreBlazorDbSetOptionsBuilder<TEntity> WithStringDisplay<TEntity>(this CoreBlazorDbSetOptionsBuilder<TEntity> optionsBuilder, Func<TEntity, string> displayColumnMethod) where TEntity : class
    {
        optionsBuilder.Options.StringDisplay=displayColumnMethod;
        return optionsBuilder;
    }

    public static CoreBlazorDbSetOptionsBuilder<TEntity> WithPropertyHidden<TEntity, TProperty>(this CoreBlazorDbSetOptionsBuilder<TEntity> optionsBuilder, Expression<Func<TEntity, TProperty>> propertyAccessor) where TEntity : class
    {
        if (propertyAccessor is not { Body: MemberExpression { Member: PropertyInfo property } })
        {
            throw new ArgumentException("Property accessor must be a simple member expression", nameof(propertyAccessor));
        }
        optionsBuilder.Options.HiddenProperties.Add(property);
        return optionsBuilder;
    }

    public static CoreBlazorDbSetOptionsBuilder<TEntity> WithPropertyDisplay<TEntity, TProperty>(this CoreBlazorDbSetOptionsBuilder<TEntity> optionsBuilder, Expression<Func<TEntity, TProperty>> propertyAccessor, Type displayComponent) where TEntity : class
    {
        if (propertyAccessor is not { Body: MemberExpression { Member: PropertyInfo property } })
        {
            throw new ArgumentException("Property accessor must be a simple member expression", nameof(propertyAccessor));
        }
        optionsBuilder.Options.DisplayTypes.Add(new(property, displayComponent));
        return optionsBuilder;
    }

    public static CoreBlazorDbSetOptionsBuilder<TEntity> WithPropertyDisplay<TEntity, TProperty, TDisplayComponent>(this CoreBlazorDbSetOptionsBuilder<TEntity> optionsBuilder, string propertyName) where TEntity : class
    {
        var property = typeof(TEntity).GetProperty(propertyName,typeof(TProperty));
        optionsBuilder.Options.DisplayTypes.Add(new(property, typeof(TDisplayComponent)));
        return optionsBuilder;
    }
}