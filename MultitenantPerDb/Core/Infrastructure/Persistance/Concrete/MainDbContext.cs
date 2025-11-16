namespace Core.Infrastructure.Persistence.Concrete
{
    using Microsoft.EntityFrameworkCore;
    using MultitenantPerDb.Core.Domain.Entity.Concrete;

    public class MainDbContext<T> : DbContext where T : BaseTenant
    {
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