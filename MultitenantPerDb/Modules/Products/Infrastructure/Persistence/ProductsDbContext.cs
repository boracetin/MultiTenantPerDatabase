using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Products.Domain.Entities;

namespace MultitenantPerDb.Modules.Products.Infrastructure.Persistence;

/// <summary>
/// ProductsDbContext - Tenant-specific database context
/// Contains: Products and related entities
/// Lifecycle: Scoped - Runtime'da tenant bazlı oluşturulur
/// </summary>
public class ProductsDbContext : DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
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

        // Seed data - Demo ürünler
        modelBuilder.Entity<Product>().HasData(
            new
            {
                Id = 1,
                Name = "Laptop",
                Description = "High performance laptop",
                Price = 1299.99m,
                Stock = 10,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                DeletedAt = (DateTime?)null
            },
            new
            {
                Id = 2,
                Name = "Mouse",
                Description = "Wireless mouse",
                Price = 29.99m,
                Stock = 50,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                DeletedAt = (DateTime?)null
            }
        );
    }
}
