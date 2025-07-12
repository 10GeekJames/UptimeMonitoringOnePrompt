using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UptimeData;

public class MonitoredEndpointHit
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid MonitoredEndpointId { get; set; }
    
    public Guid ImageId { get; set; }
    
    public DateTime HitDate { get; set; }
    
    public int ReturnCode { get; set; }
    
    public bool WasSuccessful => ReturnCode >= 200 && ReturnCode <= 299;
    
    // Navigation property
    [ForeignKey(nameof(MonitoredEndpointId))]
    public virtual MonitoredEndpoint MonitoredEndpoint { get; set; } = null!;
}