using CoreBlazor;
using CoreBlazor.Configuration;
using CoreBlazorDemo.Components;
using DemoDb;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddBlazorBootstrap()
    .AddDbContextFactory<DemoDbContext>()
    .AddCoreBlazor().ConfigureContext<DemoDbContext>(options =>
        options.WithTitle("Demo Db")
               .ConfigureSet<DemoDbContext,Job>(jobOptions => jobOptions.WithDisplay<DemoDbContext,Job,JobDisplayComponent>()
                                                                              .WithPropertyHidden(job=>job.Id)
                                                                              .WithTitle("Jobs"))
               .ConfigureSet(context=> context.People, personOptions => personOptions.WithDisplay(person => person.Name)
                                                                                 .WithPropertyHidden(person => person.Id)
                                                                                 .WithPropertyHidden(person => person.JobId)
                                                                                 .WithPropertyDisplay(person =>person.Gender, typeof(GenderDisplayComponent))
                                                                                 .WithTitle("People"))
               .ConfigureSet(context=>context.Images, imageOptions => imageOptions.WithTitle("Profile images")
                                                                                  .WithDisplay(typeof(ImageDataDisplayComponent))
                                                                                  .WithPropertyHidden(image=>image.Id)
                                                                                  .WithPropertyDisplay(image => image.Data, typeof(ImageDisplayComponent))
                                                                                  .WithPropertyEditor(image => image.Data, typeof(ImageEditorComponent)))).Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(ServiceCollectionExtensions).Assembly);

app.Run();
