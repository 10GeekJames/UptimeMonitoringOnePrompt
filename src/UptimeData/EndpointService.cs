using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace UptimeData;

public class EndpointService : IEndpointService
{
    private readonly UptimeDbContext _context;
    private readonly ILogger<EndpointService> _logger;

    public EndpointService(UptimeDbContext context, ILogger<EndpointService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // CRUD operations
    public async Task<IEnumerable<MonitoredEndpoint>> GetAllEndpointsAsync()
    {
        return await _context.MonitoredEndpoints.ToListAsync();
    }

    public async Task<IEnumerable<MonitoredEndpoint>> GetActiveEndpointsAsync()
    {
        return await _context.MonitoredEndpoints
            .Where(e => e.IsActive)
            .ToListAsync();
    }

    public async Task<MonitoredEndpoint?> GetEndpointByIdAsync(Guid id)
    {
        return await _context.MonitoredEndpoints
            .Include(e => e.Hits)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<MonitoredEndpoint> CreateEndpointAsync(MonitoredEndpoint endpoint)
    {
        _context.MonitoredEndpoints.Add(endpoint);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created endpoint: {Url}", endpoint.Url);
        return endpoint;
    }

    public async Task<MonitoredEndpoint> UpdateEndpointAsync(MonitoredEndpoint endpoint)
    {
        _context.MonitoredEndpoints.Update(endpoint);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated endpoint: {Url}", endpoint.Url);
        return endpoint;
    }

    public async Task<bool> DeleteEndpointAsync(Guid id)
    {
        var endpoint = await _context.MonitoredEndpoints.FindAsync(id);
        if (endpoint == null)
            return false;

        _context.MonitoredEndpoints.Remove(endpoint);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted endpoint: {Url}", endpoint.Url);
        return true;
    }

    // Hit operations
    public async Task<MonitoredEndpointHit> CreateHitAsync(MonitoredEndpointHit hit)
    {
        _context.MonitoredEndpointHits.Add(hit);
        await _context.SaveChangesAsync();
        return hit;
    }

    public async Task<IEnumerable<MonitoredEndpointHit>> GetHitsByEndpointIdAsync(Guid endpointId)
    {
        return await _context.MonitoredEndpointHits
            .Where(h => h.MonitoredEndpointId == endpointId)
            .OrderByDescending(h => h.HitDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MonitoredEndpointHit>> GetHitsByEndpointIdAsync(Guid endpointId, DateTime from, DateTime to)
    {
        return await _context.MonitoredEndpointHits
            .Where(h => h.MonitoredEndpointId == endpointId && h.HitDate >= from && h.HitDate <= to)
            .OrderByDescending(h => h.HitDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MonitoredEndpointHit>> GetRecentHitsAsync(int count = 100)
    {
        return await _context.MonitoredEndpointHits
            .OrderByDescending(h => h.HitDate)
            .Take(count)
            .Include(h => h.MonitoredEndpoint)
            .ToListAsync();
    }

    // Statistical queries
    public async Task<int> GetTotalEndpointsCountAsync()
    {
        return await _context.MonitoredEndpoints.CountAsync();
    }

    public async Task<int> GetActiveEndpointsCountAsync()
    {
        return await _context.MonitoredEndpoints.CountAsync(e => e.IsActive);
    }

    public async Task<int> GetTotalHitsCountAsync()
    {
        return await _context.MonitoredEndpointHits.CountAsync();
    }

    public async Task<int> GetSuccessfulHitsCountAsync()
    {
        return await _context.MonitoredEndpointHits.CountAsync(h => h.ReturnCode >= 200 && h.ReturnCode <= 299);
    }

    public async Task<int> GetFailedHitsCountAsync()
    {
        return await _context.MonitoredEndpointHits.CountAsync(h => h.ReturnCode < 200 || h.ReturnCode > 299);
    }

    public async Task<double> GetSuccessRateAsync()
    {
        var total = await GetTotalHitsCountAsync();
        if (total == 0) return 0.0;
        
        var successful = await GetSuccessfulHitsCountAsync();
        return (double)successful / total * 100;
    }

    public async Task<int> GetHitsTodayCountAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        return await _context.MonitoredEndpointHits
            .CountAsync(h => h.HitDate >= today && h.HitDate < tomorrow);
    }

    public async Task<int> GetFailuresTodayCountAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        return await _context.MonitoredEndpointHits
            .CountAsync(h => h.HitDate >= today && h.HitDate < tomorrow 
                         && (h.ReturnCode < 200 || h.ReturnCode > 299));
    }

    public async Task<DateTime?> GetLastSuccessfulHitAsync()
    {
        var lastHit = await _context.MonitoredEndpointHits
            .Where(h => h.ReturnCode >= 200 && h.ReturnCode <= 299)
            .OrderByDescending(h => h.HitDate)
            .FirstOrDefaultAsync();
        
        return lastHit?.HitDate;
    }

    public async Task<DateTime?> GetLastFailedHitAsync()
    {
        var lastHit = await _context.MonitoredEndpointHits
            .Where(h => h.ReturnCode < 200 || h.ReturnCode > 299)
            .OrderByDescending(h => h.HitDate)
            .FirstOrDefaultAsync();
        
        return lastHit?.HitDate;
    }

    // Monitoring queries
    public async Task<IEnumerable<MonitoredEndpoint>> GetEndpointsDueForMonitoringAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.MonitoredEndpoints
            .Where(e => e.IsActive && e.Frequency != Frequency.Paused && e.DelayUntilNextHit <= now)
            .ToListAsync();
    }

    public async Task UpdateEndpointLastHitAsync(Guid endpointId, DateTime lastHit, DateTime nextHit)
    {
        var endpoint = await _context.MonitoredEndpoints.FindAsync(endpointId);
        if (endpoint != null)
        {
            endpoint.LastHit = lastHit;
            endpoint.DelayUntilNextHit = nextHit;
            await _context.SaveChangesAsync();
        }
    }

    // Time-based statistics
    public async Task<(int Total, int Successful, int Failed, double SuccessRate)> GetStatsForPeriodAsync(DateTime from, DateTime to)
    {
        var hits = await _context.MonitoredEndpointHits
            .Where(h => h.HitDate >= from && h.HitDate <= to)
            .ToListAsync();

        var total = hits.Count;
        var successful = hits.Count(h => h.ReturnCode >= 200 && h.ReturnCode <= 299);
        var failed = total - successful;
        var successRate = total > 0 ? (double)successful / total * 100 : 0.0;

        return (total, successful, failed, successRate);
    }

    public async Task<(int Total, int Successful, int Failed, double SuccessRate)> GetStatsLast24HoursAsync()
    {
        var from = DateTime.UtcNow.AddHours(-24);
        var to = DateTime.UtcNow;
        return await GetStatsForPeriodAsync(from, to);
    }

    public async Task<(int Total, int Successful, int Failed, double SuccessRate)> GetStatsLast72HoursAsync()
    {
        var from = DateTime.UtcNow.AddHours(-72);
        var to = DateTime.UtcNow;
        return await GetStatsForPeriodAsync(from, to);
    }

    public async Task<(int Total, int Successful, int Failed, double SuccessRate)> GetStatsLast2WeeksAsync()
    {
        var from = DateTime.UtcNow.AddDays(-14);
        var to = DateTime.UtcNow;
        return await GetStatsForPeriodAsync(from, to);
    }
}