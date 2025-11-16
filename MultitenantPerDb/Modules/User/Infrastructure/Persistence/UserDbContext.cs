using Microsoft.EntityFrameworkCore;
using UserEntity = MultitenantPerDb.Modules.User.Domain.Entities.User;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Domain.Constants;

namespace MultitenantPerDb.Modules.User.Infrastructure.Persistence;

/// <summary>
/// UserDbContext - Tenant-specific database for User management
/// Contains: User entities
/// Lifecycle: Runtime - Created via factory per tenant
/// DatabaseType: Application (tenant-specific data)
/// </summary>
public class UserDbContext : DbContext, ITransactionContext
{
    public static DatabaseType DatabaseType => DatabaseType.Application;

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IsActive).IsRequired();
            
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}
