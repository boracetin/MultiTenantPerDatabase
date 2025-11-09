using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Tenancy.Domain.Entities;

namespace MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;

/// <summary>
/// TenancyDbContext - Master DB - Only tenant metadata
/// Contains: Tenant configurations, connection strings, branding settings
/// Lifecycle: Singleton - Connection string ile başlangıçta ayağa kalkar
/// Migration managed by TenancyModule.MigrateAsync()
/// </summary>
public class TenancyDbContext : DbContext
{
    public TenancyDbContext(DbContextOptions<TenancyDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ConnectionString).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Subdomain).HasMaxLength(50);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.BackgroundImageUrl).HasMaxLength(500);
            entity.Property(e => e.PrimaryColor).HasMaxLength(7); // #RRGGBB
            entity.Property(e => e.SecondaryColor).HasMaxLength(7);
            entity.Property(e => e.CustomCss).HasMaxLength(4000);
            
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Subdomain).IsUnique();
        });

        // Seed data - Demo tenant'lar (Anonim objeler kullanılıyor)
        modelBuilder.Entity<Tenant>().HasData(
            new
            {
                Id = 1,
                Name = "Tenant1",
                Subdomain = "tenant1",
                DisplayName = "Tenant 1 Company",
                ConnectionString = "Server=BORA\\BRCTN;Database=Tenant1Db;Integrated Security=true;Encrypt=False;TrustServerCertificate=True;Max Pool Size=2000;",
                LogoUrl = (string?)null,
                BackgroundImageUrl = (string?)null,
                PrimaryColor = "#1976D2",
                SecondaryColor = "#424242",
                CustomCss = (string?)null,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                DeletedAt = (DateTime?)null
            },
            new
            {
                Id = 2,
                Name = "Tenant2",
                Subdomain = "tenant2",
                DisplayName = "Tenant 2 Corporation",
                ConnectionString = "Server=BORA\\BRCTN;Database=Tenant2Db;Integrated Security=true;Encrypt=False;TrustServerCertificate=True;Max Pool Size=2000;",
                LogoUrl = (string?)null,
                BackgroundImageUrl = (string?)null,
                PrimaryColor = "#D32F2F",
                SecondaryColor = "#616161",
                CustomCss = (string?)null,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                DeletedAt = (DateTime?)null
            }
        );
        
        // Note: Users moved to ApplicationDbContext (tenant-specific database)
    }
}
