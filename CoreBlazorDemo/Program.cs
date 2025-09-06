using CoreBlazor;
using CoreBlazorDemo.Authentication;
using CoreBlazorDemo.Components;
using DemoDb;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddBlazorBootstrap()
    .AddAuthorization()
    .AddScoped<DemoAuthenticationStateProvider>()
    .AddScoped<AuthenticationStateProvider, DemoAuthenticationStateProvider>(sp=> sp.GetRequiredService<DemoAuthenticationStateProvider>())
    .AddDbContextFactory<DemoDbContext>()
    .AddDbContextFactory<SensitiveDbContext>()
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
                    .ConfigureContext<SensitiveDbContext>(options =>
                        options.WithTitle("Sensitive Db")
                               .UserCanReadIf(user 
                               => { 
                                   return user is CustomUser; })
                               .ConfigureSet<Job>(jobOptions => jobOptions.WithEntityDisplay<JobDisplayComponent>()
                                                                          .ConfigureProperty(job => job.Id).Hidden()
                                                                          .WithTitle("Jobs")
                                                                          .UserCanReadIf(user => user is CustomUser)
                                                                          .UserCanCreateIf(user => user == CustomUser.Admin || user == CustomUser.Engineer)
                                                                          .UserCanEditIf(user => user == CustomUser.Admin || user == CustomUser.Engineer)
                                                                          .UserCanDeleteIf(user => user == CustomUser.Admin))
                               .ConfigureSet(context => context.People, personOptions => personOptions.WithEntityDisplay(person => person.Name)
                                                                                                      .ConfigureProperty(person => person.Id).Hidden()
                                                                                                      .ConfigureProperty(person => person.JobId).Hidden()
                                                                                                      .ConfigureProperty(person => person.Gender).WithDisplay(typeof(GenderDisplayComponent))
                                                                                                      .WithTitle("People")
                                                                                                      .UserCanReadIf(user => user is CustomUser)
                                                                                                      .UserCanCreateIf(user => user == CustomUser.Admin || user == CustomUser.Engineer)
                                                                                                      .UserCanEditIf(user => user == CustomUser.Admin || user == CustomUser.Engineer)
                                                                                                      .UserCanDeleteIf(user => user == CustomUser.Admin))
                               .ConfigureSet(context => context.Images, imageOptions => imageOptions.WithTitle("Profile images")
                                                                                                    .UserCanReadIf(user => user == CustomUser.Admin)
                                                                                                    .UserCanCreateIf(user => user == CustomUser.Admin)
                                                                                                    .UserCanEditIf(user => user == CustomUser.Admin)
                                                                                                    .UserCanDeleteIf(user => user == CustomUser.Admin)
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
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(ServiceCollectionExtensions).Assembly);

app.Run();
