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
                               .ConfigureSet<Job>(jobOptions => jobOptions.WithEntityDisplay<JobDisplayComponent>()
                                                                          .ConfigureProperty(job => job.Id).Hidden()
                                                                          .WithTitle("Jobs"))
                               .ConfigureSet(context => context.People, personOptions => personOptions.WithEntityDisplay(person => person.Name)
                                                                                                     .ConfigureProperty(person => person.Id).Hidden()
                                                                                                     .ConfigureProperty(person => person.JobId).Hidden()
                                                                                                     .ConfigureProperty(person => person.Gender).WithDisplay(typeof(GenderDisplayComponent))
                                                                                                     .WithTitle("People"))
                               .ConfigureSet(context => context.Images, imageOptions => imageOptions.WithTitle("Profile images")
                                                                                                  .ConfigureProperty(image => image.Data).WithDisplay<ImageDisplayComponent>()
                                                                                                                                         .WithEditor<ImageEditorComponent>()
                                                                                                  .ConfigureProperty(image => image.Id).Hidden()
                                                                                                  .WithEntityDisplay<ImageDataDisplayComponent>()))
    .Services
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
