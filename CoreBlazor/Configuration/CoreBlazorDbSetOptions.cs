using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CoreBlazor.Configuration;

public abstract class CoreBlazorDbSetOptions<TEntity> where TEntity : class
{
    public string DisplayTitle { get; set; } = typeof(TEntity).Name;

    public Func<TEntity, string>? StringDisplay { get; set; }

    public Type? ComponentDisplay { get; set; }

    public List<PropertyInfo> HiddenProperties { get; set; } = [];

    public List<KeyValuePair<PropertyInfo, Type>> DisplayTypes { get; set; } = [];

    public List<KeyValuePair<PropertyInfo, Type>> EditingTypes { get; set; } = [];
}

public class CoreBlazorDbSetOptions<TContext, TEntity> : CoreBlazorDbSetOptions<TEntity> where TContext : DbContext where TEntity : class;
