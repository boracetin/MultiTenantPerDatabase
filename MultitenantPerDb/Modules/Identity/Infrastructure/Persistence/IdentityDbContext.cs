using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MultitenantPerDb.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// ApplicationIdentityDbContext - Tenant-specific database context with ASP.NET Core Identity
/// Contains: AspNetUsers, AspNetRoles, AspNetUserRoles, etc.
/// Lifecycle: Scoped - Runtime'da tenant bazlı oluşturulur
/// </summary>
public class ApplicationIdentityDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed data - Demo users with hashed passwords
        var hasher = new PasswordHasher<IdentityUser>();
        
        var adminUser = new IdentityUser
        {
            Id = "1",
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@tenant.local",
            NormalizedEmail = "ADMIN@TENANT.LOCAL",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");

        var regularUser = new IdentityUser
        {
            Id = "2",
            UserName = "user",
            NormalizedUserName = "USER",
            Email = "user@tenant.local",
            NormalizedEmail = "USER@TENANT.LOCAL",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        regularUser.PasswordHash = hasher.HashPassword(regularUser, "User123!");

        modelBuilder.Entity<IdentityUser>().HasData(adminUser, regularUser);
    }
}
