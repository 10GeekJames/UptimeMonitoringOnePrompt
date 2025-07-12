using System.ComponentModel.DataAnnotations;

namespace UptimeService;

public class Configuration
{
    [Required]
    public string DatabasePath { get; set; } = string.Empty;
    
    [Required]
    public string ImagesPath { get; set; } = string.Empty;
    
    [Required]
    public string LogDirectory { get; set; } = string.Empty;
    
    public int MonitoringIntervalSeconds { get; set; } = 15;
    
    public SmtpConfiguration Smtp { get; set; } = new();
    
    public void Validate()
    {
        var validationContext = new ValidationContext(this);
        var results = new List<ValidationResult>();
        
        if (!Validator.TryValidateObject(this, validationContext, results, true))
        {
            var errors = string.Join(", ", results.Select(r => r.ErrorMessage));
            throw new InvalidOperationException($"Configuration validation failed: {errors}");
        }
        
        // Validate database path directory exists
        var dbDirectory = Path.GetDirectoryName(DatabasePath);
        if (!Directory.Exists(dbDirectory))
        {
            throw new InvalidOperationException($"Database directory does not exist: {dbDirectory}");
        }
        
        // Validate images path directory exists
        if (!Directory.Exists(ImagesPath))
        {
            throw new InvalidOperationException($"Images directory does not exist: {ImagesPath}");
        }
        
        // Validate log directory exists
        if (!Directory.Exists(LogDirectory))
        {
            throw new InvalidOperationException($"Log directory does not exist: {LogDirectory}");
        }
    }
}

public class SmtpConfiguration
{
    public bool Enabled { get; set; } = false;
    
    public string SmtpHost { get; set; } = string.Empty;
    
    public int SmtpPort { get; set; } = 587;
    
    public string SmtpUser { get; set; } = string.Empty;
    
    public string SmtpPass { get; set; } = string.Empty;
    
    public bool UseSSL { get; set; } = true;
    
    public List<string> RecipientList { get; set; } = new();
    
    public string FromAddress { get; set; } = string.Empty;
    
    public string FromName { get; set; } = "Uptime Monitor";
}