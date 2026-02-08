using Microsoft.EntityFrameworkCore;
using RateEngine.Domain.Entities;

namespace RateEngine.Infrastructure.Persistence;

public class RateEngineDbContext : DbContext
{
    public RateEngineDbContext(DbContextOptions<RateEngineDbContext> options)
        : base(options)
    {
    }

    // This tells EF Core: "There is a table called Hotels"
    public DbSet<Hotel> Hotels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);

            // COMPLEX MAPPING: "Owned Entity"
            // This takes the "Address" object and flattens it into columns 
            // inside the Hotels table (Address_Street, Address_ZipCode, etc.)
            entity.OwnsOne(e => e.Address);
        });

        base.OnModelCreating(modelBuilder);
    }
}