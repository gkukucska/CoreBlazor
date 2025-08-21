using System.Reflection;

namespace CoreBlazor.Configuration
{
    public class CoreBlazorDbSetOptions<T> where T: class
    {
        public string DisplayTitle { get; set; } = typeof(T).Name;

        public Func<T, string> StringDisplay { get; set; } = entity => entity.ToString()!;

        public List<PropertyInfo> HiddenProperties { get; set; } = [];

        public List<KeyValuePair<PropertyInfo, Type>> DisplayTypes { get; set; } = [];
    }
}
