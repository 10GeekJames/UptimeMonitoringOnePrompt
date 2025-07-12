using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UptimeData;

public class UptimeDbContextFactory : IDesignTimeDbContextFactory<UptimeDbContext>
{
    public UptimeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UptimeDbContext>();
        
        // Use SQLite for design-time operations
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UptimeYo", "UptimeYo.db");
        var directory = Path.GetDirectoryName(dbPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }
        
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        
        return new UptimeDbContext(optionsBuilder.Options);
    }
    
    public static UptimeDbContext CreateDbContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UptimeDbContext>();
        optionsBuilder.UseSqlite(connectionString);
        return new UptimeDbContext(optionsBuilder.Options);
    }
}