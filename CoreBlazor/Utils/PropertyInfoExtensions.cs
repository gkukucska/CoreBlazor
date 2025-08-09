using System.Linq.Expressions;
using System.Reflection;

namespace CoreBlazor.Utils;

public static class PropertyInfoExtensions
{
    public static Expression<Func<T, TProperty>> GetMemberAccessExpressionWithConversion<T, TProperty>(this PropertyInfo pi)
    {
        var defaultExpression = Expression.Parameter(typeof(T));
        Expression propertyOrField = Expression.Property(defaultExpression, pi);
        if (pi.PropertyType != typeof(TProperty))
            propertyOrField = Expression.Convert(propertyOrField, typeof(TProperty));
        return Expression.Lambda<Func<T,TProperty>>(propertyOrField, defaultExpression);
    }
    public static Expression<Func<T, TProperty>> GetMemberAccessExpression<T, TProperty>(this PropertyInfo pi)
    {
        var defaultExpression = Expression.Parameter(typeof(T));
        Expression propertyOrField = Expression.Property(defaultExpression, pi);
        return Expression.Lambda<Func<T,TProperty>>(propertyOrField, defaultExpression);
    }

    public static bool IsDisplayable(this PropertyInfo pi) => !pi.PropertyType.IsGenericType;
}