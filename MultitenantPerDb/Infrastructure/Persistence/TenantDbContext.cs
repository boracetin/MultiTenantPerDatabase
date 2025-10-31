using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Domain.Entities;

namespace MultitenantPerDb.Infrastructure.Persistence;

/// <summary>
/// TenantDbContext - Tenant bilgilerini ve connection string'leri yöneten DbContext
/// </summary>
public class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ConnectionString).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
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

        // Seed data - Demo tenant'lar (Anonim objeler kullanılıyor)
        modelBuilder.Entity<Tenant>().HasData(
            new
            {
                Id = 1,
                Name = "Tenant1",
                ConnectionString = "Server=BORA\\\\BRCTN;Database=Tenant1Db;Integrated Security=true;Encrypt=False;TrustServerCertificate=True;Max Pool Size=2000;",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 2,
                Name = "Tenant2",
                ConnectionString = "Server=BORA\\\\BRCTN;Database=Tenant2Db;Integrated Security=true;Encrypt=False;TrustServerCertificate=True;Max Pool Size=2000;",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null
            }
        );

        // Seed data - Demo users (Password: "123456", anonim objeler kullanılıyor)
        modelBuilder.Entity<User>().HasData(
            new
            {
                Id = 1,
                Username = "user1",
                Email = "user1@tenant1.com",
                PasswordHash = "$2a$11$5ZqJKbGjmJ5J5YqHxJH5XO5mZxJH5XO5mZxJH5XO5mZxJH5XO5mZx", // 123456
                TenantId = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 2,
                Username = "user2",
                Email = "user2@tenant2.com",
                PasswordHash = "$2a$11$5ZqJKbGjmJ5J5YqHxJH5XO5mZxJH5XO5mZxJH5XO5mZxJH5XO5mZx", // 123456
                TenantId = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null
            }
        );
    }
}
