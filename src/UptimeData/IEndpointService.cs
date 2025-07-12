namespace UptimeData;

public interface IEndpointService
{
    // CRUD operations
    Task<IEnumerable<MonitoredEndpoint>> GetAllEndpointsAsync();
    Task<IEnumerable<MonitoredEndpoint>> GetActiveEndpointsAsync();
    Task<MonitoredEndpoint?> GetEndpointByIdAsync(Guid id);
    Task<MonitoredEndpoint> CreateEndpointAsync(MonitoredEndpoint endpoint);
    Task<MonitoredEndpoint> UpdateEndpointAsync(MonitoredEndpoint endpoint);
    Task<bool> DeleteEndpointAsync(Guid id);
    
    // Hit operations
    Task<MonitoredEndpointHit> CreateHitAsync(MonitoredEndpointHit hit);
    Task<IEnumerable<MonitoredEndpointHit>> GetHitsByEndpointIdAsync(Guid endpointId);
    Task<IEnumerable<MonitoredEndpointHit>> GetHitsByEndpointIdAsync(Guid endpointId, DateTime from, DateTime to);
    Task<IEnumerable<MonitoredEndpointHit>> GetRecentHitsAsync(int count = 100);
    
    // Statistical queries
    Task<int> GetTotalEndpointsCountAsync();
    Task<int> GetActiveEndpointsCountAsync();
    Task<int> GetTotalHitsCountAsync();
    Task<int> GetSuccessfulHitsCountAsync();
    Task<int> GetFailedHitsCountAsync();
    Task<double> GetSuccessRateAsync();
    Task<int> GetHitsTodayCountAsync();
    Task<int> GetFailuresTodayCountAsync();
    Task<DateTime?> GetLastSuccessfulHitAsync();
    Task<DateTime?> GetLastFailedHitAsync();
    
    // Monitoring queries
    Task<IEnumerable<MonitoredEndpoint>> GetEndpointsDueForMonitoringAsync();
    Task UpdateEndpointLastHitAsync(Guid endpointId, DateTime lastHit, DateTime nextHit);
    
    // Time-based statistics
    Task<(int Total, int Successful, int Failed, double SuccessRate)> GetStatsForPeriodAsync(DateTime from, DateTime to);
    Task<(int Total, int Successful, int Failed, double SuccessRate)> GetStatsLast24HoursAsync();
    Task<(int Total, int Successful, int Failed, double SuccessRate)> GetStatsLast72HoursAsync();
    Task<(int Total, int Successful, int Failed, double SuccessRate)> GetStatsLast2WeeksAsync();
}