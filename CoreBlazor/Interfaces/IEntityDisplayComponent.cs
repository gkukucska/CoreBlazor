using Microsoft.AspNetCore.Components;

namespace CoreBlazor.Interfaces;

public interface IEntityDisplayComponent<TEntity> where TEntity : class
{
    [Parameter]
    public TEntity Entity { get; set; }
}
