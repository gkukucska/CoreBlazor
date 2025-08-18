using System.Linq.Expressions;
using System.Reflection;

namespace CoreBlazor.Utils;

public static class PropertyInfoExtensions
{

    private static readonly MethodInfo _stringContainsMethod = typeof(string).GetMethod(nameof(string.Contains), BindingFlags.Instance | BindingFlags.Public, [typeof(string)])!;
    private static readonly MethodInfo _stringStartsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), BindingFlags.Instance | BindingFlags.Public, [typeof(string)])!;
    private static readonly MethodInfo _stringEndsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), BindingFlags.Instance | BindingFlags.Public, [typeof(string)])!;
    private static readonly MethodInfo _stringEqualsMethod = typeof(string).GetMethod(nameof(string.Equals), BindingFlags.Instance | BindingFlags.Public, [typeof(string)])!;


    public static Expression<Func<T, TProperty>> GetMemberAccessExpressionWithConversion<T, TProperty>(this PropertyInfo pi)
    {
        var defaultExpression = Expression.Parameter(typeof(T));
        Expression property = Expression.Property(defaultExpression, pi);
        if (pi.PropertyType != typeof(TProperty))
            property = Expression.Convert(property, typeof(TProperty));
        return Expression.Lambda<Func<T,TProperty>>(property, defaultExpression);
    }
    public static Expression<Func<T, TProperty>> GetMemberAccessExpression<T, TProperty>(this PropertyInfo pi)
    {
        var defaultExpression = Expression.Parameter(typeof(T));
        Expression property = Expression.Property(defaultExpression, pi);
        return Expression.Lambda<Func<T, TProperty>>(property, defaultExpression);
    }
    public static Expression<Func<TProperty>> GetValueAccessExpressionWithConversion<T, TProperty>(this PropertyInfo pi, T source)
    {
        var defaultExpression = Expression.Constant(source, typeof(T));
        Expression property = Expression.Property(defaultExpression, pi);
        if (pi.PropertyType != typeof(TProperty))
            property = Expression.Convert(property, typeof(TProperty));
        return Expression.Lambda<Func<TProperty>>(property);
    }
    public static Expression<Func<TProperty>> GetValueAccessExpression<T, TProperty>(this PropertyInfo pi, T source)
    {
        var defaultExpression = Expression.Constant(source, typeof(T));
        Expression property = Expression.Property(defaultExpression, pi);
        return Expression.Lambda<Func<TProperty>>(property);
    }

    public static bool IsDisplayable(this PropertyInfo pi) => !pi.PropertyType.IsGenericType;

    public static Expression<Func<TEntity, bool>> ContainsPredicate<TEntity>(this PropertyInfo pi, string filter) where TEntity : class
        => pi.FilterWithMethod<TEntity>(filter, _stringContainsMethod);
    public static Expression<Func<TEntity, bool>> NotContainsPredicate<TEntity>(this PropertyInfo pi, string filter) where TEntity : class
        => pi.NegateFilterWithMethod<TEntity>(filter, _stringContainsMethod);
    public static Expression<Func<TEntity, bool>> EqualsPredicate<TEntity>(this PropertyInfo pi, string filter) where TEntity : class
        => pi.FilterWithMethod<TEntity>(filter, _stringEqualsMethod);
    public static Expression<Func<TEntity, bool>> NotEqualsPredicate<TEntity>(this PropertyInfo pi, string filter) where TEntity : class
        => pi.NegateFilterWithMethod<TEntity>(filter, _stringEqualsMethod);
    public static Expression<Func<TEntity, bool>> StartsWithPredicate<TEntity>(this PropertyInfo pi, string filter) where TEntity : class
        => pi.FilterWithMethod<TEntity>(filter, _stringStartsWithMethod);
    public static Expression<Func<TEntity, bool>> EndsWithPredicate<TEntity>(this PropertyInfo pi, string filter) where TEntity : class
        => pi.FilterWithMethod<TEntity>(filter, _stringEndsWithMethod);

    private static Expression<Func<TEntity, bool>> FilterWithMethod<TEntity>(this PropertyInfo pi, string filter, MethodInfo method) where TEntity : class
    {
        ParameterExpression entity = Expression.Parameter(typeof(TEntity));
        MemberExpression member = Expression.Property(entity, pi);
        var filterExpr = Expression.Call(member, method, Expression.Constant(filter, typeof(string)));
        return Expression.Lambda<Func<TEntity, bool>>(filterExpr, entity);
    }

    private static Expression<Func<TEntity, bool>> NegateFilterWithMethod<TEntity>(this PropertyInfo pi, string filter, MethodInfo method) where TEntity : class
    {
        ParameterExpression entity = Expression.Parameter(typeof(TEntity));
        MemberExpression member = Expression.Property(entity, pi);
        Expression filterExpr = Expression.Call(member, method, Expression.Constant(filter, typeof(string)));
        filterExpr = Expression.Not(filterExpr);
        return Expression.Lambda<Func<TEntity, bool>>(filterExpr, entity);
    }
}