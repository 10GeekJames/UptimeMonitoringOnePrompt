using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace UptimeData;

public class DatabaseSeeder
{
    private readonly UptimeDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(UptimeDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await _context.MonitoredEndpoints.AnyAsync())
            {
                _logger.LogInformation("Database already contains data. Skipping seeding.");
                return;
            }

            _logger.LogInformation("Seeding database with initial data...");

            // Create sample endpoints
            var endpoints = new List<MonitoredEndpoint>
            {
                new MonitoredEndpoint
                {
                    Id = Guid.NewGuid(),
                    Url = "https://github.com",
                    Frequency = Frequency.Realtime,
                    IsActive = true,
                    LastHit = DateTime.UtcNow.AddHours(-1),
                    DelayUntilNextHit = DateTime.UtcNow.AddMinutes(1)
                },
                new MonitoredEndpoint
                {
                    Id = Guid.NewGuid(),
                    Url = "https://google.com",
                    Frequency = Frequency.Daily,
                    IsActive = true,
                    LastHit = DateTime.UtcNow.AddDays(-1),
                    DelayUntilNextHit = DateTime.UtcNow.AddHours(1)
                },
                new MonitoredEndpoint
                {
                    Id = Guid.NewGuid(),
                    Url = "https://microsoft.com",
                    Frequency = Frequency.Realtime,
                    IsActive = true,
                    LastHit = DateTime.UtcNow.AddMinutes(-30),
                    DelayUntilNextHit = DateTime.UtcNow.AddMinutes(2)
                },
                new MonitoredEndpoint
                {
                    Id = Guid.NewGuid(),
                    Url = "https://stackoverflow.com",
                    Frequency = Frequency.Daily,
                    IsActive = false,
                    LastHit = DateTime.UtcNow.AddDays(-2),
                    DelayUntilNextHit = DateTime.UtcNow.AddDays(1)
                }
            };

            _context.MonitoredEndpoints.AddRange(endpoints);
            await _context.SaveChangesAsync();

            // Create sample hits for GitHub endpoint
            var githubEndpoint = endpoints.First(e => e.Url == "https://github.com");
            var sampleHits = new List<MonitoredEndpointHit>
            {
                new MonitoredEndpointHit
                {
                    Id = Guid.NewGuid(),
                    MonitoredEndpointId = githubEndpoint.Id,
                    ImageId = Guid.NewGuid(),
                    HitDate = DateTime.UtcNow.AddHours(-2),
                    ReturnCode = 200
                },
                new MonitoredEndpointHit
                {
                    Id = Guid.NewGuid(),
                    MonitoredEndpointId = githubEndpoint.Id,
                    ImageId = Guid.NewGuid(),
                    HitDate = DateTime.UtcNow.AddHours(-1),
                    ReturnCode = 200
                },
                new MonitoredEndpointHit
                {
                    Id = Guid.NewGuid(),
                    MonitoredEndpointId = githubEndpoint.Id,
                    ImageId = Guid.NewGuid(),
                    HitDate = DateTime.UtcNow.AddMinutes(-30),
                    ReturnCode = 503
                },
                new MonitoredEndpointHit
                {
                    Id = Guid.NewGuid(),
                    MonitoredEndpointId = githubEndpoint.Id,
                    ImageId = Guid.NewGuid(),
                    HitDate = DateTime.UtcNow.AddMinutes(-15),
                    ReturnCode = 200
                }
            };

            _context.MonitoredEndpointHits.AddRange(sampleHits);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Database seeding completed successfully. Created {EndpointCount} endpoints and {HitCount} hits.", 
                endpoints.Count, sampleHits.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}