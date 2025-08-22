using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CoreBlazor.Configuration
{
    public class CoreBlazorDbSetOptions<TContext,TEntity> where TContext: DbContext where TEntity: class
    {
        public string DisplayTitle { get; set; } = typeof(TEntity).Name;

        public Func<TEntity, string> StringDisplay { get; set; } = entity => entity.ToString()!;

        public List<PropertyInfo> HiddenProperties { get; set; } = [];

        public List<KeyValuePair<PropertyInfo, Type>> DisplayTypes { get; set; } = [];
    }
}
