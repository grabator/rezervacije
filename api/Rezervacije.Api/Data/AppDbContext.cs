using Microsoft.EntityFrameworkCore;
using Rezervacije.Api.Models;

namespace Rezervacije.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<VenueEvent> Events => Set<VenueEvent>();
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
        // Concurrency token na Status - sprecava da dva istovremena zahtjeva oba "uspiju"
        // rezervisati isti sto (EF ubaci "WHERE Status = stara_vrijednost" u UPDATE; ako
        // je neko drugi vec promijenio status u medjuvremenu, affected rows = 0 i EF baci
        // DbUpdateConcurrencyException umjesto da tiho prepise podatke).
        modelBuilder.Entity<FloorTableEntity>()
            .Property(t => t.Status).IsConcurrencyToken();

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Event)
            .WithMany()
            .HasForeignKey(r => r.EventId);
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Table)
            .WithMany()
            .HasForeignKey(r => r.TableEntityId);
    }
}
