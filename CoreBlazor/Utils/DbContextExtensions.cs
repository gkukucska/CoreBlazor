using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CoreBlazor.Utils;

public static class DbContextExtensions
{
    public static IEnumerable<INavigation> GetNavigations<TEntity>(this DbContext context) where TEntity : class
    {
        return context.Model.FindEntityType(typeof(TEntity))?.GetNavigations() ?? [];
    }
    public static bool IsNavigation<TEntity>(this DbContext context, PropertyInfo propertyInfo) where TEntity : class
    {
        return context.GetNavigations<TEntity>().Any(x=>x.Name == propertyInfo.Name);
    }

    public static IQueryable<TEntity> IncludeNavigations<TEntity>(this DbContext context)
        where TEntity : class
    {
        var navigations = context.GetNavigations<TEntity>();
        return navigations.Aggregate(context.Set<TEntity>().AsQueryable(),
            (query, navigation) => query.Include(navigation.Name));
    }
}