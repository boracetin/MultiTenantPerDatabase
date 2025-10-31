using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MultitenantPerDb.Data;

/// <summary>
/// Design-time factory for ApplicationDbContext (migration oluşturmak için)
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Migration oluştururken kullanılacak dummy connection string
        // Gerçek tenant database'leri runtime'da oluşturulacak
        optionsBuilder.UseSqlServer("Server=BORA\\BRCTN;Database=TenantTemplateDb;Trusted_Connection=True;TrustServerCertificate=True;");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
