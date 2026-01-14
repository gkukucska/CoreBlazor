# CoreBlazor

[![codecov](https://codecov.io/gh/gkukucska/CoreBlazor/branch/main/graph/badge.svg)](https://codecov.io/gh/gkukucska/CoreBlazor)
[![Build and deploy ASP.Net Core app to Azure Web App - coreblazor-demo](https://github.com/gkukucska/CoreBlazor/actions/workflows/main_coreblazor-demo.yml/badge.svg)](https://github.com/gkukucska/CoreBlazor/actions/workflows/main_coreblazor-demo.yml)

A generic entity framework core UI library written in Blazor using server side rendering. 

Feel free to check the demo app in action on this very slow website: https://coreblazor-demo.azurewebsites.net/

This package was inspired by the CoreAdmin library, but it has been completely rewritten with performance, extensibility and security in mind.

## Features
- Generic CRUD operations for any entity
- Support for navigation properties
- Server (database) side pagination, sorting, and filtering
- Customizable UI components for viewing and editing entity properties and entity display
- Policy based access control
- Configurability on table (```DbSet```) level using fluent syntax
- Multiple DbContext support
- Custom display and editor components
- Property-level configuration
- Hidden properties support
- Split query optimization

## Installation

### Prerequisites
- .NET 8.0 or later
- Entity Framework Core 8.0 or later
- BlazorBootstrap for UI components

### NuGet Package
```bash
dotnet add package CoreBlazor
```

## Quick Start

### 1. Basic Setup

For the most simple usage, add the following code to your ```Program.cs``` file:

``` csharp
builder.Services.AddDbContextFactory<YourDbContext>()
                .AddCoreBlazor();
```

This will add all required services to the dependency injection container and find all ```DbSet``` properties in all registered ```DbContext```.

### 2. Configure the Request Pipeline

Add CoreBlazor to your request pipeline in the ```Program.cs``` file:
``` csharp
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode()
   .AddCoreBlazor();

```

### 3. Configure Routes

In your ```Routes.razor``` file, add the CoreBlazor assembly to the router:

``` razor
<Router AppAssembly="typeof(Program).Assembly"
        AdditionalAssemblies="[typeof(CoreBlazor.WebApplicationExtensions).Assembly]">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
```

### 4. Install BlazorBootstrap

Follow the installation instructions for BlazorBootstrap in server side render mode in ```App.razor``` and related files. CoreBlazor uses BlazorBootstrap for its UI components.

## Advanced Configuration

### Configure Multiple DbContexts

CoreBlazor supports multiple DbContext configurations:

``` csharp
builder.Services
    .AddDbContextFactory<DemoDbContext>()
    .AddDbContextFactory<SensitiveDbContext>()
    .AddCoreBlazor()
        .ConfigureContext<DemoDbContext>(options =>
        {
            options.WithTitle("Demo Database")
                   .WithSplitQueries();
        })
        .ConfigureContext<SensitiveDbContext>(options =>
        {
            options.WithTitle("Sensitive Database")
                   .UserCanReadIf(user => user.IsInRole("Admin"));
        });
```

### Context-Level Configuration

#### Set Context Title
``` csharp
options.WithTitle("My Database")
```

#### Enable Split Queries
Improve performance for queries with navigation properties:
``` csharp
options.WithSplitQueries()
```

#### Context-Level Access Control
``` csharp
options.UserCanReadIf(user => user.IsInRole("Viewer"))
```

### DbSet (Table) Configuration

Configure individual entity sets using the `ConfigureSet` method:

``` csharp
.ConfigureContext<DemoDbContext>(options =>
    options.ConfigureSet<Person>(personOptions => 
    {
        personOptions.WithTitle("People")
                     .UserCanReadIf(user => user.Identity.IsAuthenticated)
                     .UserCanCreateIf(user => user.IsInRole("Editor"))
                     .UserCanEditIf(user => user.IsInRole("Editor"))
                     .UserCanDeleteIf(user => user.IsInRole("Admin"));
    }))
```

#### Alternative Syntax (Lambda Expression)
``` csharp
.ConfigureSet(context => context.People, personOptions => 
{
    // Configuration here
})
```

### Entity Display Configuration

#### Display Entity with Custom Component
``` csharp
personOptions.WithEntityDisplay<PersonDisplayComponent>()
```

#### Display Entity with Lambda Expression
``` csharp
personOptions.WithEntityDisplay(person => person.Name)
```

Example custom display component:
``` csharp
public class PersonDisplayComponent : ComponentBase, IEntityDisplayComponent<Person>
{
    [Parameter]
    public Person? Entity { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.AddContent(0, $"{Entity?.Name} ({Entity?.Email})");
    }
}
```

### Property Configuration

#### Hide Properties
``` csharp
personOptions.ConfigureProperty(person => person.Id).Hidden()
             .ConfigureProperty(person => person.CreatedDate).Hidden()
```

#### Custom Property Display
``` csharp
personOptions.ConfigureProperty(person => person.Gender)
             .WithDisplay<GenderDisplayComponent>()
```

Display component example:
``` csharp
public class GenderDisplayComponent : ComponentBase, IPropertyDisplayComponent<Person>
{
    [Parameter]
    public object? PropertyValue { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var gender = PropertyValue as Gender?;
        builder.AddContent(0, gender switch
        {
            Gender.Male => "? Male",
            Gender.Female => "? Female",
            _ => "Unknown"
        });
    }
}
```

#### Custom Property Editor
``` csharp
imageOptions.ConfigureProperty(image => image.Data)
            .WithDisplay<ImageDisplayComponent>()
            .WithEditor<ImageEditorComponent>()
```

Editor component example:
``` csharp
public class ImageEditorComponent : ComponentBase, IPropertyEditorComponent<Image, byte[]>
{
    [Parameter]
    public byte[]? PropertyValue { get; set; }

    [Parameter]
    public EventCallback<byte[]> PropertyValueChanged { get; set; }

    // Editor implementation
}
```

### Access Control (Security)

#### DbSet-Level Permissions

Control who can perform operations on specific entity sets:

``` csharp
jobOptions.UserCanReadIf(user => user.Identity.IsAuthenticated)
          .UserCanCreateIf(user => user.IsInRole("Editor") || user.IsInRole("Admin"))
          .UserCanEditIf(user => user.IsInRole("Editor") || user.IsInRole("Admin"))
          .UserCanDeleteIf(user => user.IsInRole("Admin"))
```

#### Custom User Objects

Use custom user objects for more complex authorization:

``` csharp
.UserCanReadIf(user => user is CustomUser)
.UserCanCreateIf(user => user == CustomUser.Admin || user == CustomUser.Editor)
.UserCanEditIf(user => user == CustomUser.Admin || user == CustomUser.Editor)
.UserCanDeleteIf(user => user == CustomUser.Admin)
```

#### Context-Level Permissions

``` csharp
options.WithTitle("Sensitive Database")
       .UserCanReadIf(user => user.IsInRole("Admin"))
```

## Complete Configuration Example

Here's a comprehensive example showing all configuration options:

``` csharp
builder.Services
    .AddDbContextFactory<DemoDbContext>()
    .AddCoreBlazor()
        .ConfigureContext<DemoDbContext>(options =>
        {
            options.WithTitle("Demo Database")
                   .WithSplitQueries()
                   
                   // Configure Jobs
                   .ConfigureSet<Job>(jobOptions => 
                   {
                       jobOptions.WithEntityDisplay<JobDisplayComponent>()
                                 .ConfigureProperty(job => job.Id).Hidden()
                                 .WithTitle("Jobs");
                   })
                   
                   // Configure People
                   .ConfigureSet<Person>(personOptions => 
                   {
                       personOptions.WithEntityDisplay(person => person.Name)
                                    .ConfigureProperty(person => person.Id).Hidden()
                                    .ConfigureProperty(person => person.JobId).Hidden()
                                    .ConfigureProperty(person => person.Gender)
                                        .WithDisplay<GenderDisplayComponent>()
                                    .WithTitle("People")
                                    .UserCanReadIf(user => user.Identity.IsAuthenticated)
                                    .UserCanCreateIf(user => user.IsInRole("Editor"))
                                    .UserCanEditIf(user => user.IsInRole("Editor"))
                                    .UserCanDeleteIf(user => user.IsInRole("Admin"));
                   })
                   
                   // Configure Images with custom display and editor
                   .ConfigureSet(context => context.Images, imageOptions => 
                   {
                       imageOptions.WithTitle("Profile Images")
                                   .ConfigureProperty(image => image.Data)
                                       .WithDisplay<ImageDisplayComponent>()
                                       .WithEditor<ImageEditorComponent>()
                                   .ConfigureProperty(image => image.Id).Hidden()
                                   .WithEntityDisplay<ImageDataDisplayComponent>();
                   });
        });
```

## Navigation Properties

CoreBlazor automatically handles navigation properties:
- Select related entities from dropdown/modal
- Display related entity information
- Automatic foreign key management

## Performance Optimization

### Split Queries

For entities with multiple navigation properties, use split queries to avoid cartesian explosion:

``` csharp
options.WithSplitQueries()
```

### Server-Side Operations

All pagination, sorting, and filtering operations are performed server-side (in the database) for optimal performance.

## UI Features

### Grid Features
- **Pagination**: Configurable page sizes
- **Sorting**: Click column headers to sort
- **Filtering**: Type to filter by column values
- **Responsive**: Mobile-friendly tables

### CRUD Operations
- **Create**: Add new entities with validation
- **Read**: View entities in grids and detail views
- **Update**: Edit entities with custom editors
- **Delete**: Confirm before deletion

## Authorization Integration

CoreBlazor integrates with ASP.NET Core's built-in authorization:

``` csharp
builder.Services.AddAuthorization()
                .AddScoped<AuthenticationStateProvider, YourAuthProvider>();
```

Access is controlled through:
- Policy-based authorization
- Role-based authorization
- Custom authorization logic

## Customization

### Custom Components

Implement these interfaces for custom components:

- `IEntityDisplayComponent<TEntity>` - Custom entity display
- `IPropertyDisplayComponent<TEntity>` - Custom property display
- `IPropertyEditorComponent<TEntity, TProperty>` - Custom property editor

### Styling

CoreBlazor uses BlazorBootstrap, so you can customize appearance using:
- Bootstrap themes
- Custom CSS
- Bootstrap utility classes

## Testing

The project includes comprehensive unit tests:
- 99+ test cases
- bUnit for component testing
- xUnit test framework
- FluentAssertions for readable assertions
- NSubstitute for mocking

See the `CoreBlazor.Tests` project for examples.

## Best Practices

1. **Hide ID properties**: Always hide technical IDs from users
2. **Custom displays**: Provide meaningful entity displays for navigation properties
3. **Access control**: Always configure appropriate permissions
4. **Split queries**: Enable for entities with multiple navigation properties
5. **Custom editors**: Use for complex properties (images, enums, etc.)

## Troubleshooting

### Routes not found
Ensure CoreBlazor assembly is added to the router's `AdditionalAssemblies`.

### Authorization not working
Check that:
- `AuthenticationStateProvider` is registered
- `app.UseAuthorization()` is called in the pipeline
- Authorization policies are configured correctly

### Performance issues
- Enable split queries for complex entities
- Ensure database indexes on frequently queried columns
- Check for N+1 query problems

## Examples

Check the `CoreBlazorDemo` project for complete working examples including:
- Multiple DbContext configuration
- Custom display components
- Custom editor components
- Authorization setup
- Image handling

## Contributing

Contributions are welcome! Please check the GitHub repository for guidelines.

## License

[Your License Here]

## Support

For issues and questions:
- GitHub Issues: https://github.com/gkukucska/CoreBlazor
- Demo: https://coreblazor-demo.azurewebsites.net/