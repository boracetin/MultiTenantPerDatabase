using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Application.Products.Domain.Entities;
using MultitenantPerDb.Modules.Main.Identity.Domain.Entities;

namespace MultitenantPerDb.Modules.Main.Tenancy.Infrastructure.Persistence;

/// <summary>
/// ApplicationDbContext - Her tenant için ayrı database'de kullanılacak DbContext
/// Contains tenant-specific data: Products, Users, Orders, etc.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }

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

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Seed data - Demo users (Tenant-specific, each tenant DB will have its own users)
        // Note: In real deployment, you'll create separate migrations for each tenant DB
        modelBuilder.Entity<User>().HasData(
            new
            {
                Id = 1,
                Username = "admin",
                Email = "admin@tenant.local",
                PasswordHash = "123456", // Demo password - production'da BCrypt hash kullanılmalı
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 2,
                Username = "user",
                Email = "user@tenant.local",
                PasswordHash = "123456", // Demo password
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null
            }
        );

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
