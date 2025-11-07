using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MultitenantPerDb.Modules.Main.Tenancy.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for ApplicationDbContext (migration oluşturmak için)
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Design-time only - migration kod generate etmek için kullanılır
        // Gerçek database bağlantısı yapılmaz, sadece model şeması oluşturulur
        // Runtime'da tenant'ların kendi connection string'leri kullanılır
        optionsBuilder.UseSqlServer("Server=.;Database=DesignTimeOnly;");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
