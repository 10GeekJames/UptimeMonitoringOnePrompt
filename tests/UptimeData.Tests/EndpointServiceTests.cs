using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UptimeData;
using Xunit;

namespace UptimeData.Tests;

public class EndpointServiceTests : IDisposable
{
    private readonly UptimeDbContext _context;
    private readonly EndpointService _endpointService;
    private readonly Mock<ILogger<EndpointService>> _mockLogger;

    public EndpointServiceTests()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<UptimeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new UptimeDbContext(options);
        _mockLogger = new Mock<ILogger<EndpointService>>();
        _endpointService = new EndpointService(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateEndpointAsync_ValidEndpoint_ShouldCreateSuccessfully()
    {
        // Arrange
        var endpoint = new MonitoredEndpoint
        {
            Url = "https://example.com",
            Frequency = Frequency.Realtime,
            IsActive = true,
            LastHit = DateTime.UtcNow,
            DelayUntilNextHit = DateTime.UtcNow.AddMinutes(1)
        };

        // Act
        var result = await _endpointService.CreateEndpointAsync(endpoint);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(endpoint.Url, result.Url);
        Assert.Equal(endpoint.Frequency, result.Frequency);
        Assert.Equal(endpoint.IsActive, result.IsActive);
        
        var savedEndpoint = await _context.MonitoredEndpoints.FirstOrDefaultAsync(e => e.Id == result.Id);
        Assert.NotNull(savedEndpoint);
        Assert.Equal(endpoint.Url, savedEndpoint.Url);
    }

    [Fact]
    public async Task GetActiveEndpointsAsync_ShouldReturnOnlyActiveEndpoints()
    {
        // Arrange
        var activeEndpoint = new MonitoredEndpoint
        {
            Url = "https://active.com",
            Frequency = Frequency.Realtime,
            IsActive = true,
            LastHit = DateTime.UtcNow,
            DelayUntilNextHit = DateTime.UtcNow.AddMinutes(1)
        };

        var inactiveEndpoint = new MonitoredEndpoint
        {
            Url = "https://inactive.com",
            Frequency = Frequency.Paused,
            IsActive = false,
            LastHit = DateTime.UtcNow,
            DelayUntilNextHit = DateTime.UtcNow.AddMinutes(1)
        };

        await _endpointService.CreateEndpointAsync(activeEndpoint);
        await _endpointService.CreateEndpointAsync(inactiveEndpoint);

        // Act
        var result = await _endpointService.GetActiveEndpointsAsync();

        // Assert
        Assert.Single(result);
        Assert.True(result.First().IsActive);
        Assert.Equal("https://active.com", result.First().Url);
    }

    [Fact]
    public async Task CreateHitAsync_ValidHit_ShouldCreateSuccessfully()
    {
        // Arrange
        var endpoint = new MonitoredEndpoint
        {
            Url = "https://test.com",
            Frequency = Frequency.Realtime,
            IsActive = true,
            LastHit = DateTime.UtcNow,
            DelayUntilNextHit = DateTime.UtcNow.AddMinutes(1)
        };

        await _endpointService.CreateEndpointAsync(endpoint);

        var hit = new MonitoredEndpointHit
        {
            MonitoredEndpointId = endpoint.Id,
            ImageId = Guid.NewGuid(),
            HitDate = DateTime.UtcNow,
            ReturnCode = 200
        };

        // Act
        var result = await _endpointService.CreateHitAsync(hit);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(hit.MonitoredEndpointId, result.MonitoredEndpointId);
        Assert.Equal(hit.ReturnCode, result.ReturnCode);
        Assert.True(result.WasSuccessful); // Should be true for 200 status code
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, true)]
    [InlineData(299, true)]
    [InlineData(404, false)]
    [InlineData(500, false)]
    [InlineData(199, false)]
    [InlineData(300, false)]
    public void MonitoredEndpointHit_WasSuccessful_ShouldReturnCorrectValue(int statusCode, bool expectedResult)
    {
        // Arrange
        var hit = new MonitoredEndpointHit
        {
            MonitoredEndpointId = Guid.NewGuid(),
            ImageId = Guid.NewGuid(),
            HitDate = DateTime.UtcNow,
            ReturnCode = statusCode
        };

        // Act & Assert
        Assert.Equal(expectedResult, hit.WasSuccessful);
    }

    [Fact]
    public async Task GetSuccessRateAsync_WithHits_ShouldCalculateCorrectly()
    {
        // Arrange
        var endpoint = new MonitoredEndpoint
        {
            Url = "https://test.com",
            Frequency = Frequency.Realtime,
            IsActive = true,
            LastHit = DateTime.UtcNow,
            DelayUntilNextHit = DateTime.UtcNow.AddMinutes(1)
        };

        await _endpointService.CreateEndpointAsync(endpoint);

        // Create 7 successful hits and 3 failed hits
        for (int i = 0; i < 7; i++)
        {
            await _endpointService.CreateHitAsync(new MonitoredEndpointHit
            {
                MonitoredEndpointId = endpoint.Id,
                ImageId = Guid.NewGuid(),
                HitDate = DateTime.UtcNow,
                ReturnCode = 200
            });
        }

        for (int i = 0; i < 3; i++)
        {
            await _endpointService.CreateHitAsync(new MonitoredEndpointHit
            {
                MonitoredEndpointId = endpoint.Id,
                ImageId = Guid.NewGuid(),
                HitDate = DateTime.UtcNow,
                ReturnCode = 500
            });
        }

        // Act
        var successRate = await _endpointService.GetSuccessRateAsync();

        // Assert
        Assert.Equal(70.0, successRate, 1); // 7 out of 10 = 70%
    }

    [Fact]
    public async Task GetEndpointsDueForMonitoringAsync_ShouldReturnEndpointsPastDueTime()
    {
        // Arrange
        var dueEndpoint = new MonitoredEndpoint
        {
            Url = "https://due.com",
            Frequency = Frequency.Realtime,
            IsActive = true,
            LastHit = DateTime.UtcNow.AddMinutes(-10),
            DelayUntilNextHit = DateTime.UtcNow.AddMinutes(-5) // Past due
        };

        var futureEndpoint = new MonitoredEndpoint
        {
            Url = "https://future.com",
            Frequency = Frequency.Realtime,
            IsActive = true,
            LastHit = DateTime.UtcNow,
            DelayUntilNextHit = DateTime.UtcNow.AddMinutes(5) // Future
        };

        await _endpointService.CreateEndpointAsync(dueEndpoint);
        await _endpointService.CreateEndpointAsync(futureEndpoint);

        // Act
        var result = await _endpointService.GetEndpointsDueForMonitoringAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("https://due.com", result.First().Url);
    }

    [Fact]
    public async Task DeleteEndpointAsync_ExistingEndpoint_ShouldDeleteSuccessfully()
    {
        // Arrange
        var endpoint = new MonitoredEndpoint
        {
            Url = "https://delete.com",
            Frequency = Frequency.Realtime,
            IsActive = true,
            LastHit = DateTime.UtcNow,
            DelayUntilNextHit = DateTime.UtcNow.AddMinutes(1)
        };

        await _endpointService.CreateEndpointAsync(endpoint);

        // Act
        var result = await _endpointService.DeleteEndpointAsync(endpoint.Id);

        // Assert
        Assert.True(result);
        
        var deletedEndpoint = await _context.MonitoredEndpoints.FirstOrDefaultAsync(e => e.Id == endpoint.Id);
        Assert.Null(deletedEndpoint);
    }

    [Fact]
    public async Task DeleteEndpointAsync_NonExistingEndpoint_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _endpointService.DeleteEndpointAsync(nonExistingId);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}