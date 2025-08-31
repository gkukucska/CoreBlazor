using CoreBlazor.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace CoreBlazor.Configuration;

public abstract class CoreBlazorDbSetOptionsBuilder<TEntity>  where TEntity : class
{
    protected CoreBlazorDbSetOptionsBuilder(CoreBlazorDbSetOptions<TEntity> options)
    {
        Options = options;
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
}

public class CoreBlazorDbSetOptionsBuilder<TContext, TEntity>() : CoreBlazorDbSetOptionsBuilder<TEntity>(new CoreBlazorDbSetOptions<TContext, TEntity>()) where TContext : DbContext where TEntity : class;