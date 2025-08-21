using BlazorBootstrap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;

namespace CoreBlazor.Utils;

public static class DbContextExtensions
{
    public static IEnumerable<INavigation> GetNavigations<TEntity>(this DbContext context) where TEntity : class 
        => context.Model.FindEntityType(typeof(TEntity))?.GetNavigations() ?? [];

    public static bool IsNavigation<TEntity>(this DbContext context, PropertyInfo propertyInfo) where TEntity : class 
        => context.GetNavigations<TEntity>().Any(x => x.Name == propertyInfo.Name);

    public static IQueryable<TEntity> DbSetWithDisplayableNavigations<TEntity>(this DbContext context) where TEntity : class 
        => context.GetNavigations<TEntity>().Where(x=>x.PropertyInfo?.IsDisplayable() ?? false)
        .Aggregate(context.Set<TEntity>().AsQueryable(),(query, navigation) => query.Include(navigation.Name));

    public static IProperty GetPrimaryKey<TEntity>(this DbContext context) where TEntity : class
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        if (entityType is null)
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} not found in the model.");
        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey is null or {Properties.Count: 0 })
            throw new InvalidOperationException($"Primary key for entity type {typeof(TEntity).Name} not found.");
        return primaryKey.Properties[0];
    }

    public async static Task<GridDataProviderResult<TEntity>> ApplyTo<TEntity>(this DbContext context, GridDataProviderRequest<TEntity> request) where TEntity : class
        => await context.DbSetWithDisplayableNavigations<TEntity>()
                                  .AsNoTracking()
                                  .WithFiltering(request.Filters)
                                  .WithSorting(request.Sorting)
                                  .ToGridResultsAsync(request);
}