using Microsoft.AspNetCore.Components;

namespace CoreBlazor.Interfaces;

public interface IPropertyDisplayComponent<TProperty>
{
    [Parameter]
    public TProperty PropertyValue { get; set; }
}
