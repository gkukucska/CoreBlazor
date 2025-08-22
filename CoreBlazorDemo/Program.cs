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
               .ConfigureSet<DemoDbContext,JobEntity>(jobOptions => jobOptions.WithStringDisplay(job => $"{job?.Name}, salary: {job?.Salary}")
                                                                              .WithPropertyHidden(job=>job.Id)
                                                                              .WithTitle("Jobs"))
               .ConfigureSet<DemoDbContext,Person>(personOptions => personOptions.WithStringDisplay(person => person.Name)
                                                                                 .WithPropertyHidden(person => person.Id)
                                                                                 .WithPropertyHidden(person => person.JobId)
                                                                                 .WithTitle("People"))).Services
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
