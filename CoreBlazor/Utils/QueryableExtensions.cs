using BlazorBootstrap;
using System.Linq.Expressions;
using System.Reflection;

namespace CoreBlazor.Utils
{
    public static class QueryableExtensions
    {
        #region Sorting
        private static IQueryable<TEntity> ApplySorting<TEntity, TProperty>(this IQueryable<TEntity> query, SortingItem<TEntity> sorting, PropertyInfo property)
        => sorting.SortDirection switch
        {
            SortDirection.Ascending => query.OrderBy(property.GetMemberAccessExpression<TEntity, TProperty>()),
            SortDirection.Descending => query.OrderByDescending(property.GetMemberAccessExpression<TEntity, TProperty>()),
            _ => query
        };

        public static IQueryable<TEntity> WithSorting<TEntity>(this IQueryable<TEntity> query, SortingItem<TEntity> sorting)
        {
            //This is ugly but necessary, since Lambda compile within GetMemberAccessExpression will not convert correctly to IComparable, but throw an error for value types and enums
            var property = typeof(TEntity).GetProperty(sorting.SortString)!;
            if (!typeof(IComparable).IsAssignableFrom(property.PropertyType))
                return query;
            var method = typeof(QueryableExtensions).GetMethod(nameof(ApplySorting), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(typeof(TEntity), property.PropertyType);
            return (method.Invoke(null, [query, sorting, property]) as IQueryable<TEntity>)!;
        }

        public static IQueryable<TEntity> WithSorting<TEntity>(this IQueryable<TEntity> query, IEnumerable<SortingItem<TEntity>> sortingCollection)
            => sortingCollection.Aggregate(query, static (current, sorting) => current.WithSorting(sorting));

        #endregion

        #region Filtering

        private static Expression<Func<TEntity, bool>>? GetExpressionDelegate<TEntity>(this FilterItem filter) where TEntity : class
        {
            var property = typeof(TEntity).GetProperty(filter.PropertyName);
            if (property is null)
            {
                return null;
            }
            if (property.PropertyType == typeof(string))
            {
                return filter.Operator switch
                {
                    FilterOperator.Equals => property.EqualsPredicate<TEntity>(filter.Value),
                    FilterOperator.NotEquals => property.NotEqualsPredicate<TEntity>(filter.Value),
                    FilterOperator.StartsWith => property.StartsWithPredicate<TEntity>(filter.Value),
                    FilterOperator.EndsWith => property.EndsWithPredicate<TEntity>(filter.Value),
                    FilterOperator.Contains => property.ContainsPredicate<TEntity>(filter.Value),
                    FilterOperator.DoesNotContain => property.NotContainsPredicate<TEntity>(filter.Value),
                    _ => null
                };
            }
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity));
            return ExpressionExtensions.GetExpressionDelegate<TEntity>(parameterExpression, filter);
        }

        public static IQueryable<TEntity> WithFiltering<TEntity>(this IQueryable<TEntity> query, FilterItem filter) where TEntity : class
        {
            var expression = filter.GetExpressionDelegate<TEntity>();
            if (expression is null)
            {
                return query;
            }
            return query.Where(expression);
        }

        public static IQueryable<TEntity> WithFiltering<TEntity>(this IQueryable<TEntity> query, IEnumerable<FilterItem> filterCollection) where TEntity : class
            => filterCollection.Aggregate(query, static (current, filter) => current.WithFiltering(filter));

        #endregion

        #region Pagination

        public static IQueryable<TEntity> WithPagination<TEntity>(this IQueryable<TEntity> query, GridDataProviderRequest<TEntity> request) where TEntity : class
            => request switch
            {
                { PageNumber: > 0, PageSize: > 0 } => query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize),
                _ => query
            };

        #endregion


    }
}
