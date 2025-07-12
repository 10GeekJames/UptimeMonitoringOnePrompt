using System.ComponentModel.DataAnnotations;

namespace UptimeData;

public class MonitoredEndpoint
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(2000)]
    public string Url { get; set; } = string.Empty;
    
    public Frequency Frequency { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime LastHit { get; set; }
    
    public DateTime DelayUntilNextHit { get; set; }
    
    // Navigation property
    public virtual ICollection<MonitoredEndpointHit> Hits { get; set; } = new List<MonitoredEndpointHit>();
}