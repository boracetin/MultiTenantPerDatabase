namespace Core.Infrastructure.Persistence.Concrete
{
    using Microsoft.EntityFrameworkCore;
    using MultitenantPerDb.Core.Domain.Entity.Concrete;
    using MultitenantPerDb.Core.Domain;
    using MultitenantPerDb.Core.Domain.Constants;

    /// <summary>
    /// MainDbContext - Generic tenant database context
    /// DatabaseType: Main (master data)
    /// NOTE: Cannot inherit BaseDbContext due to generic constraint
    /// </summary>
    public class MainDbContext<T> : DbContext, IDbContextTransactionType where T : BaseTenant
    {
        public static DatabaseType DatabaseType => DatabaseType.Main;
        public MainDbContext(DbContextOptions<MainDbContext<T>> options) : base(options)
        {
        }

        public DbSet<T> Tenants { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional model configurations can be added here
        }
    }
}