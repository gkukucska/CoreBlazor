using Microsoft.AspNetCore.Components;

namespace CoreBlazor.Interfaces
{
    public interface IPropertyEditComponent<TEntity>
    {
        [Parameter]
        public TEntity Entity { get; set; }

        [Parameter]
        public EventCallback ValueSelected { get; set; }

        [Parameter]
        public bool IsDisabled { get; set; }
    }
}
