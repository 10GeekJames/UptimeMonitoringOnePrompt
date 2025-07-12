using Microsoft.EntityFrameworkCore;

namespace UptimeData;

public class UptimeDbContext : DbContext
{
    public UptimeDbContext(DbContextOptions<UptimeDbContext> options) : base(options)
    {
    }

    public DbSet<MonitoredEndpoint> MonitoredEndpoints => Set<MonitoredEndpoint>();
    public DbSet<MonitoredEndpointHit> MonitoredEndpointHits => Set<MonitoredEndpointHit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure MonitoredEndpoint
        modelBuilder.Entity<MonitoredEndpoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Frequency).HasConversion<string>();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.LastHit);
            entity.Property(e => e.DelayUntilNextHit);
        });

        // Configure MonitoredEndpointHit
        modelBuilder.Entity<MonitoredEndpointHit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MonitoredEndpointId).IsRequired();
            entity.Property(e => e.ImageId);
            entity.Property(e => e.HitDate).IsRequired();
            entity.Property(e => e.ReturnCode).IsRequired();
            entity.Ignore(e => e.WasSuccessful); // Computed property

            // Configure foreign key relationship
            entity.HasOne(e => e.MonitoredEndpoint)
                  .WithMany(e => e.Hits)
                  .HasForeignKey(e => e.MonitoredEndpointId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}