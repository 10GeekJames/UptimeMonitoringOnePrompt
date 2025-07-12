using Microsoft.EntityFrameworkCore;
using UptimeData;
using UptimeUI.Components;
using Blazored.Toast;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add toast notifications
builder.Services.AddBlazoredToast();

// Configure database
var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UptimeYo", "UptimeYo.db");
if (builder.Environment.IsDevelopment())
{
    // For development, use a local database in temp directory
    databasePath = "/tmp/UptimeYo/UptimeYo.db";
}

var dbDirectory = Path.GetDirectoryName(databasePath);
if (!Directory.Exists(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory!);
}

builder.Services.AddDbContext<UptimeDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

// Add services
builder.Services.AddScoped<IEndpointService, EndpointService>();
builder.Services.AddScoped<DatabaseSeeder>();

// Add HTTP client for API calls
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UptimeDbContext>();
    await context.Database.EnsureCreatedAsync();
    
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();
