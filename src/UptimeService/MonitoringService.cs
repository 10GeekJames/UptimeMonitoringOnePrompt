using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using UptimeData;

namespace UptimeService;

public class MonitoringService : BackgroundService
{
    private readonly IEndpointService _endpointService;
    private readonly ISmtpAlertService _alertService;
    private readonly Configuration _config;
    private readonly ILogger<MonitoringService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MonitoringService(
        IEndpointService endpointService,
        ISmtpAlertService alertService,
        Configuration config,
        ILogger<MonitoringService> logger,
        IServiceProvider serviceProvider)
    {
        _endpointService = endpointService;
        _alertService = alertService;
        _config = config;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Monitoring service started. Interval: {Interval} seconds", _config.MonitoringIntervalSeconds);

        // Install Playwright browsers on first run
        await InstallPlaywrightBrowsersAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMonitoringTickAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during monitoring tick");
            }

            await Task.Delay(TimeSpan.FromSeconds(_config.MonitoringIntervalSeconds), stoppingToken);
        }
    }

    private async Task InstallPlaywrightBrowsersAsync()
    {
        try
        {
            _logger.LogInformation("Installing Playwright browsers...");
            await Task.Run(() => Microsoft.Playwright.Program.Main(new[] { "install", "chromium" }));
            _logger.LogInformation("Playwright browsers installed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install Playwright browsers");
            throw;
        }
    }

    private async Task ProcessMonitoringTickAsync()
    {
        var endpoints = await _endpointService.GetEndpointsDueForMonitoringAsync();
        
        if (!endpoints.Any())
        {
            _logger.LogDebug("No endpoints due for monitoring");
            return;
        }

        _logger.LogInformation("Processing {Count} endpoints for monitoring", endpoints.Count());

        foreach (var endpoint in endpoints)
        {
            try
            {
                await MonitorEndpointAsync(endpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring endpoint {Url}", endpoint.Url);
            }
        }
    }

    private async Task MonitorEndpointAsync(MonitoredEndpoint endpoint)
    {
        var correlationId = Guid.NewGuid();
        _logger.LogInformation("Monitoring endpoint {Url} (Correlation: {CorrelationId})", endpoint.Url, correlationId);

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
        });

        var page = await browser.NewPageAsync();
        
        var hit = new MonitoredEndpointHit
        {
            Id = Guid.NewGuid(),
            MonitoredEndpointId = endpoint.Id,
            ImageId = Guid.NewGuid(),
            HitDate = DateTime.UtcNow,
            ReturnCode = 0
        };

        string? errorMessage = null;

        try
        {
            // Navigate to the URL with timeout
            var response = await page.GotoAsync(endpoint.Url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000 // 30 seconds timeout
            });

            if (response != null)
            {
                hit.ReturnCode = response.Status;
                _logger.LogInformation("Endpoint {Url} returned status {Status} (Correlation: {CorrelationId})", 
                    endpoint.Url, response.Status, correlationId);
            }
            else
            {
                hit.ReturnCode = 0;
                errorMessage = "No response received";
                _logger.LogWarning("Endpoint {Url} returned no response (Correlation: {CorrelationId})", 
                    endpoint.Url, correlationId);
            }

            // Take screenshot
            var screenshotPath = Path.Combine(_config.ImagesPath, $"{hit.ImageId}.png");
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });

            _logger.LogDebug("Screenshot saved for {Url} at {Path} (Correlation: {CorrelationId})", 
                endpoint.Url, screenshotPath, correlationId);
        }
        catch (PlaywrightException ex)
        {
            hit.ReturnCode = 0;
            errorMessage = ex.Message;
            _logger.LogError(ex, "Playwright error for {Url} (Correlation: {CorrelationId})", endpoint.Url, correlationId);
        }
        catch (TimeoutException ex)
        {
            hit.ReturnCode = 408; // Request Timeout
            errorMessage = "Request timed out";
            _logger.LogError(ex, "Timeout error for {Url} (Correlation: {CorrelationId})", endpoint.Url, correlationId);
        }
        catch (Exception ex)
        {
            hit.ReturnCode = 0;
            errorMessage = ex.Message;
            _logger.LogError(ex, "Unexpected error for {Url} (Correlation: {CorrelationId})", endpoint.Url, correlationId);
        }

        // Save hit to database
        await _endpointService.CreateHitAsync(hit);

        // Update endpoint's last hit and next hit times
        var nextHit = CalculateNextHit(endpoint.Frequency);
        await _endpointService.UpdateEndpointLastHitAsync(endpoint.Id, hit.HitDate, nextHit);

        // Send alert if failed
        if (!hit.WasSuccessful)
        {
            try
            {
                await _alertService.SendFailureAlertAsync(endpoint.Url, hit.ReturnCode, errorMessage ?? "Unknown error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send alert for {Url} (Correlation: {CorrelationId})", endpoint.Url, correlationId);
            }
        }

        _logger.LogInformation("Monitoring completed for {Url}: {Status} (Correlation: {CorrelationId})", 
            endpoint.Url, hit.WasSuccessful ? "Success" : "Failed", correlationId);
    }

    private DateTime CalculateNextHit(Frequency frequency)
    {
        var now = DateTime.UtcNow;
        
        return frequency switch
        {
            Frequency.Realtime => now.AddMinutes(1), // Check every minute for realtime
            Frequency.Daily => now.AddDays(1),         // Check once per day
            Frequency.Paused => now.AddYears(1),      // Effectively never
            _ => now.AddMinutes(5)                     // Default fallback
        };
    }
}