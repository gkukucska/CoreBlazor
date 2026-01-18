namespace CoreBlazor.Interfaces;

public interface IEntityDisplayComponent<TEntity> where TEntity : class
{
    public TEntity? Entity { get; set; }
}
