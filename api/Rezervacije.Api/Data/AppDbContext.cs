using Microsoft.EntityFrameworkCore;
using Rezervacije.Api.Models;

namespace Rezervacije.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<VenueEvent> Events => Set<VenueEvent>();
    public DbSet<TablePackage> Packages => Set<TablePackage>();
    public DbSet<FloorElementEntity> FloorElements => Set<FloorElementEntity>();
    public DbSet<FloorTableEntity> Tables => Set<FloorTableEntity>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venue>().HasIndex(v => v.Slug).IsUnique();

        modelBuilder.Entity<VenueEvent>()
            .HasOne(e => e.Venue)
            .WithMany(v => v.Events)
            .HasForeignKey(e => e.VenueId);

        modelBuilder.Entity<TablePackage>()
            .HasOne<VenueEvent>()
            .WithMany(e => e.Packages)
            .HasForeignKey(p => p.EventId);

        modelBuilder.Entity<FloorElementEntity>()
            .HasOne<VenueEvent>()
            .WithMany(e => e.Elements)
            .HasForeignKey(el => el.EventId);

        modelBuilder.Entity<FloorTableEntity>()
            .HasOne<VenueEvent>()
            .WithMany(e => e.Tables)
            .HasForeignKey(t => t.EventId);
        modelBuilder.Entity<FloorTableEntity>()
            .HasIndex(t => new { t.EventId, t.TableKey }).IsUnique();

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Event)
            .WithMany()
            .HasForeignKey(r => r.EventId);
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Table)
            .WithMany()
            .HasForeignKey(r => r.TableEntityId);
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Package)
            .WithMany()
            .HasForeignKey(r => r.PackageId);
    }
}
