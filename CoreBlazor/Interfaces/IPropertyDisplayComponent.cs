namespace CoreBlazor.Interfaces;

public interface IPropertyDisplayComponent<TProperty>
{
    public TProperty? PropertyValue { get; set; }
}
