using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Domain.Entities;

namespace MultitenantPerDb.Infrastructure.Persistence;

/// <summary>
/// ApplicationDbContext - Her tenant için ayrı database'de kullanılacak DbContext (Product entity)
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });

        // Seed data - Demo ürünler (Anonim objeler kullanılıyor, entity encapsulation için)
        modelBuilder.Entity<Product>().HasData(
            new
            {
                Id = 1,
                Name = "Laptop",
                Description = "High performance laptop",
                Price = 1299.99m,
                Stock = 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 2,
                Name = "Mouse",
                Description = "Wireless mouse",
                Price = 29.99m,
                Stock = 50,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null
            }
        );
    }
}
