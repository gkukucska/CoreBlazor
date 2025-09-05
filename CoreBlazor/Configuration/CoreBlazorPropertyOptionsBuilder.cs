using CoreBlazor.Interfaces;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;

namespace CoreBlazor.Configuration;

public class CoreBlazorPropertyOptionsBuilder<TEntity, TProperty> where TEntity : class
{
    private readonly PropertyInfo _property;
    private readonly CoreBlazorDbSetOptionsBuilder<TEntity> _optionsBuilder;

    internal CoreBlazorDbSetOptionsBuilder<TEntity> OptionsBuilder => _optionsBuilder;

    public CoreBlazorPropertyOptionsBuilder(PropertyInfo property, CoreBlazorDbSetOptionsBuilder<TEntity> optionsBuilder)
    {
        _property = property;
        _optionsBuilder = optionsBuilder;
    }

    public CoreBlazorPropertyOptionsBuilder<TEntity,TProperty> Hidden()
    {
        if (!_optionsBuilder.Options.HiddenProperties.Contains(_property))
        {
            _optionsBuilder.Options.HiddenProperties.Add(_property);
        }
        return this;
    }
    public CoreBlazorPropertyOptionsBuilder<TEntity, TProperty> WithDisplay(Type displayComponent)
    {
        if (!typeof(IPropertyDisplayComponent<TProperty>).IsAssignableFrom(displayComponent))
        {
            throw new ArgumentOutOfRangeException(nameof(displayComponent),$"Display component must implement {typeof(IPropertyDisplayComponent<TProperty>).Name}");
        }
        if (!_optionsBuilder.Options.DisplayTypes.Any(kv => kv.Key == _property))
        {
            _optionsBuilder.Options.DisplayTypes.Add(new KeyValuePair<PropertyInfo, Type>(_property, displayComponent));
        }
        return this;
    }
    public CoreBlazorPropertyOptionsBuilder<TEntity, TProperty> WithDisplay<TDisplay>() where TDisplay: IPropertyDisplayComponent<TProperty>
    {
        if (!_optionsBuilder.Options.DisplayTypes.Any(kv => kv.Key == _property))
        {
            _optionsBuilder.Options.DisplayTypes.Add(new KeyValuePair<PropertyInfo, Type>(_property, typeof(TDisplay)));
        }
        return this;
    }
    public CoreBlazorDbSetOptionsBuilder<TEntity> WithEntityDisplay<TDisplay>() where TDisplay : IEntityDisplayComponent<TEntity>
    {
        _optionsBuilder.Options.ComponentDisplay = typeof(TDisplay);    
        return OptionsBuilder;
    }
    public CoreBlazorDbSetOptionsBuilder<TEntity> WithEntityDisplay(Type displayType)
    {
        if (!typeof(IEntityDisplayComponent<TEntity>).IsAssignableFrom(displayType))
        {
            throw new ArgumentOutOfRangeException(nameof(displayType), $"Display component must implement {typeof(IEntityDisplayComponent<TEntity>).Name}");
        }
        _optionsBuilder.Options.ComponentDisplay = displayType;
        return OptionsBuilder;
    }
    public CoreBlazorPropertyOptionsBuilder<TEntity, TProperty> WithEditor(Type editorComponent)
    {
        if (!typeof(IPropertyEditComponent<TEntity>).IsAssignableFrom(editorComponent))
        {
            throw new ArgumentOutOfRangeException(nameof(editorComponent), $"Display component must implement {typeof(IPropertyEditComponent<TEntity>).Name}");
        }
        if (!_optionsBuilder.Options.EditingTypes.Any(kv => kv.Key == _property))
        {
            _optionsBuilder.Options.EditingTypes.Add(new KeyValuePair<PropertyInfo, Type>(_property, editorComponent));
        }
        return this;
    }
    public CoreBlazorPropertyOptionsBuilder<TEntity, TProperty> WithEditor<TEditorComponent>() where TEditorComponent : IPropertyEditComponent<TEntity>
    {
        if (!_optionsBuilder.Options.EditingTypes.Any(kv => kv.Key == _property))
        {
            _optionsBuilder.Options.EditingTypes.Add(new KeyValuePair<PropertyInfo, Type>(_property, typeof(TEditorComponent)));
        }
        return this;
    }


    public CoreBlazorPropertyOptionsBuilder<TEntity, TOtherProperty> ConfigureProperty<TOtherProperty>(Expression<Func<TEntity, TOtherProperty>> propertyAccessor) 
    {
        if (propertyAccessor is not { Body: MemberExpression { Member: PropertyInfo property } })
        {
            throw new ArgumentException("Property accessor must be a simple member expression", nameof(propertyAccessor));
        }
        return new CoreBlazorPropertyOptionsBuilder<TEntity, TOtherProperty>(property, OptionsBuilder);
    }

    public CoreBlazorDbSetOptionsBuilder<TEntity> WithTitle(string title)
    {
        return OptionsBuilder.WithTitle(title);
    }

    public CoreBlazorPropertyOptionsBuilder<TEntity, TProperty> UserCanReadIf(Predicate<ClaimsPrincipal> predicate)
    {
        OptionsBuilder.UserCanReadIf(predicate);
        return this;
    }

    public CoreBlazorPropertyOptionsBuilder<TEntity, TProperty> UserCanCreateIf(Predicate<ClaimsPrincipal> predicate)
    {
        OptionsBuilder.UserCanCreateIf(predicate);
        return this;
    }

    public CoreBlazorPropertyOptionsBuilder<TEntity, TProperty> UserCanEditIf(Predicate<ClaimsPrincipal> predicate)
    {
        OptionsBuilder.UserCanEditIf(predicate);
        return this;
    }

    public CoreBlazorPropertyOptionsBuilder<TEntity, TProperty> UserCanDeleteIf(Predicate<ClaimsPrincipal> predicate)
    {

        OptionsBuilder.UserCanDeleteIf(predicate);
        return this;
    }
}