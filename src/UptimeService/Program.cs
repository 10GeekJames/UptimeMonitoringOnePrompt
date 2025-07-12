using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using UptimeData;
using UptimeService;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("config.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var appConfig = new Configuration();
configuration.Bind(appConfig);

// Ensure directories exist
Directory.CreateDirectory(Path.GetDirectoryName(appConfig.DatabasePath)!);
Directory.CreateDirectory(appConfig.ImagesPath);
Directory.CreateDirectory(appConfig.LogDirectory);

// Validate configuration
try
{
    appConfig.Validate();
}
catch (Exception ex)
{
    Console.WriteLine($"Configuration validation failed: {ex.Message}");
    Environment.Exit(1);
}

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add NLog for file logging
var logConfigPath = Path.Combine(appConfig.LogDirectory, "NLog.config");
if (!File.Exists(logConfigPath))
{
    CreateNLogConfig(logConfigPath, appConfig.LogDirectory);
}

builder.Logging.AddNLog(logConfigPath);

// Register services
builder.Services.AddSingleton(appConfig);

// Database
builder.Services.AddDbContext<UptimeDbContext>(options =>
    options.UseSqlite($"Data Source={appConfig.DatabasePath}"));

// Services
builder.Services.AddScoped<IEndpointService, EndpointService>();
builder.Services.AddScoped<ISmtpAlertService, SmtpAlertService>();
builder.Services.AddScoped<DatabaseSeeder>();

// Background services
builder.Services.AddHostedService<MonitoringService>();

var host = builder.Build();

// Initialize database
using (var scope = host.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting Uptime Monitor Service");
        logger.LogInformation("Database: {DatabasePath}", appConfig.DatabasePath);
        logger.LogInformation("Images: {ImagesPath}", appConfig.ImagesPath);
        logger.LogInformation("Logs: {LogDirectory}", appConfig.LogDirectory);
        
        // Ensure database is created and seeded
        var context = scope.ServiceProvider.GetRequiredService<UptimeDbContext>();
        await context.Database.EnsureCreatedAsync();
        
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
        
        logger.LogInformation("Database initialization completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize database");
        Environment.Exit(1);
    }
}

await host.RunAsync();

static void CreateNLogConfig(string configPath, string logDirectory)
{
    var nlogConfig = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd""
      xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <targets>
    <target xsi:type=""File"" name=""fileTarget""
            fileName=""{logDirectory}/${{shortdate}}.log""
            layout=""${{longdate}} ${{level:uppercase=true}} ${{logger}} - ${{message}} ${{exception:format=tostring}}"" />
    <target xsi:type=""Console"" name=""consoleTarget""
            layout=""${{time}} [${{level:uppercase=true}}] ${{logger}} - ${{message}} ${{exception:format=tostring}}"" />
  </targets>
  <rules>
    <logger name=""*"" minlevel=""Debug"" writeTo=""fileTarget"" />
    <logger name=""*"" minlevel=""Info"" writeTo=""consoleTarget"" />
  </rules>
</nlog>";
    
    File.WriteAllText(configPath, nlogConfig);
}
