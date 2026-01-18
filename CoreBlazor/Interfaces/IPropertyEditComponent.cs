using Microsoft.AspNetCore.Components;

namespace CoreBlazor.Interfaces;

public interface IPropertyEditComponent<TEntity>
{
    public TEntity? Entity { get; set; }

    public EventCallback? ValueSelected { get; set; }

    public bool IsDisabled { get; set; }
}
