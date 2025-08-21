using Microsoft.EntityFrameworkCore;

namespace CoreBlazor.Configuration
{
    public class CoreBlazorDbContextOptions<T> where T: DbContext
    {
        public string DisplayTitle { get; set; } = typeof(T).Name;
    }
}
