namespace CoreBlazor.Configuration;

public class CoreBlazorDbSetOptionsBuilder<TEntity> where TEntity: class
{
    internal CoreBlazorDbSetOptions<TEntity> Options { get; } = new();
}
