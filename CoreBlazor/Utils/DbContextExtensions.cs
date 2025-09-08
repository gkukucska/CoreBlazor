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

    public static IProperty GetPrimaryKey<TEntity>(this DbContext context) where TEntity : class
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        if (entityType is null)
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} not found in the model.");
        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey is null or { Properties.Count: 0 })
            throw new InvalidOperationException($"Primary key for entity type {typeof(TEntity).Name} not found.");
        return primaryKey.Properties[0];
    }

    public static bool IsPrimaryKey<TEntity>(this DbContext context, PropertyInfo propertyInfo) where TEntity : class
    {
        var pk = context.GetPrimaryKey<TEntity>().PropertyInfo;
        return pk.Name == propertyInfo.Name && pk.PropertyType == propertyInfo.PropertyType;
    }

    public static bool IsGeneratedPrimaryKey<TEntity>(this DbContext context, PropertyInfo propertyInfo) where TEntity : class
    {
        var pk = context.GetPrimaryKey<TEntity>();
        return pk.PropertyInfo.Name == propertyInfo.Name && pk.PropertyInfo.PropertyType == propertyInfo.PropertyType && pk.ValueGenerated != ValueGenerated.Never;
    }

    public static IEnumerable<IForeignKey> GetForeignKeys<TEntity>(this DbContext context) where TEntity : class
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        if (entityType is null)
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} not found in the model.");
        return entityType.GetDeclaredForeignKeys();
    }

    public static bool IsForeignKey<TEntity>(this DbContext context, PropertyInfo propertyInfo) where TEntity : class
    {
        var foreignKeys = context.GetForeignKeys<TEntity>();
        return foreignKeys.Select(fk=> fk.Properties[0].PropertyInfo).Any(fk=> fk.Name == propertyInfo.Name && fk.PropertyType == propertyInfo.PropertyType);
    }

    public static IQueryable<TEntity> DbSetWithDisplayableNavigations<TEntity>(this DbContext context, bool useSplitQueries) where TEntity : class
    {
        var query= context.GetNavigations<TEntity>().Where(x => x.PropertyInfo?.IsDisplayable() ?? false)
            .Aggregate(context.Set<TEntity>().AsQueryable(), (query, navigation) => query.Include(navigation.Name));
        if (useSplitQueries)
        {
            query = query.AsSplitQuery();
        }
        return query;
    }

    public async static Task<GridDataProviderResult<TEntity>> ApplyTo<TEntity>(this DbContext context, GridDataProviderRequest<TEntity> request, bool useSplitQueries) where TEntity : class
        => await context.DbSetWithDisplayableNavigations<TEntity>(useSplitQueries)
                                  .AsNoTracking()
                                  .WithFiltering(request.Filters)
                                  .WithSorting(request.Sorting)
                                  .ToGridResultsAsync(request);

    public static IEnumerable<PropertyInfo> GetDbSets(this Type contextType)
    {
        return contextType.GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));
    }
}