# Otomatik Migration Sistemi

## 🎯 Özellik

Login sırasında kullanıcının tenant database'inde **otomatik migration** çalıştırılır. Her tenant'ın kendi database'i ilk login'de otomatik olarak oluşturulup güncellenir.

## 🔄 İş Akışı

```
1. Kullanıcı login yapar
   ↓
2. User bilgisi TenantDbContext'ten bulunur
   ↓
3. User'ın TenantId'si ile Tenant bilgisi çekilir
   ↓
4. Tenant'ın connection string'i ile ApplicationDbContext oluşturulur
   ↓
5. Pending migration kontrol edilir
   ↓
6. Varsa migration otomatik çalıştırılır (Database.MigrateAsync)
   ↓
7. JWT token oluşturulup döndürülür
```

## 📝 Kullanım

### 1. Master Database Oluştur
```powershell
# TenantDbContext migration oluştur
dotnet ef migrations add InitialTenantDb --context TenantDbContext --output-dir Migrations/Tenant

# Master database'i oluştur (Tenants + Users)
dotnet ef database update --context TenantDbContext
```

### 2. Tenant Database Template Oluştur
```powershell
# ApplicationDbContext migration oluştur
dotnet ef migrations add InitialApplicationDb --context ApplicationDbContext --output-dir Migrations/Application

# Bu migration tenant database'lerinde otomatik çalışacak
```

### 3. Login Yap - Otomatik Migration
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "user1",
  "password": "123456"
}
```

**İlk login'de:**
- Tenant1Db database'i otomatik oluşturulur
- Tüm migration'lar otomatik çalıştırılır
- Products tablosu ve seed data eklenir

**Sonraki login'lerde:**
- Pending migration varsa otomatik çalıştırılır
- Yoksa hiçbir şey yapılmaz

## 🗄️ Database Yapısı

### Master Database (TenantMasterDb)
- **Tenants** - Tenant bilgileri ve connection string'ler
- **Users** - Kullanıcı bilgileri ve tenant ilişkileri

### Tenant Databases (Tenant1Db, Tenant2Db, ...)
- **Products** - Her tenant'a özgü ürünler
- **__EFMigrationsHistory** - Migration geçmişi

## 🔧 AuthService.cs

```csharp
public async Task<LoginResponse?> LoginAsync(LoginRequest request)
{
    // User'ı bul
    var user = await _tenantDbContext.Users
        .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);
    
    // Tenant'ı bul
    var tenant = await _tenantDbContext.Tenants
        .FirstOrDefaultAsync(t => t.Id == user.TenantId && t.IsActive);
    
    // ✨ Otomatik migration çalıştır
    await RunTenantMigrationsAsync(tenant);
    
    // Token oluştur ve döndür
    return new LoginResponse { ... };
}

private async Task RunTenantMigrationsAsync(Tenant tenant)
{
    // Tenant connection string ile context oluştur
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseSqlServer(tenant.ConnectionString);
    
    await using var context = new ApplicationDbContext(optionsBuilder.Options);
    
    // Pending migration'ları kontrol et
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    
    if (pendingMigrations.Any())
    {
        // Migration'ları otomatik uygula
        await context.Database.MigrateAsync();
        _logger.LogInformation("Migration tamamlandı: {Count} adet", pendingMigrations.Count());
    }
}
```

## ⚙️ ApplicationDbContextFactory.cs

Design-time DbContext factory (migration oluşturmak için):

```csharp
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Template connection string (sadece migration oluşturmak için)
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=TenantTemplateDb;Trusted_Connection=True;TrustServerCertificate=True;"
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
```

## 📊 Demo Data

### Tenants
| Id | Name    | ConnectionString | IsActive |
|----|---------|-----------------|----------|
| 1  | Tenant1 | Server=localhost;Database=Tenant1Db;... | true |
| 2  | Tenant2 | Server=localhost;Database=Tenant2Db;... | true |

### Users
| Id | Username | Password | TenantId | Email |
|----|----------|----------|----------|-------|
| 1  | user1    | 123456   | 1        | user1@tenant1.com |
| 2  | user2    | 123456   | 2        | user2@tenant2.com |

## ✅ Avantajlar

- ✅ **Zero-downtime deployment** - Uygulama çalışırken migration
- ✅ **Lazy initialization** - Database sadece gerektiğinde oluşur
- ✅ **Otomatik güncelleme** - Her login'de kontrol edilir
- ✅ **Tenant isolation** - Her tenant bağımsız güncellenir
- ✅ **Error handling** - Migration hatası login'i engellemez (exception fırlatır)
- ✅ **Logging** - Tüm migration işlemleri loglanır

## 🚀 Yeni Migration Ekleme

```powershell
# 1. Yeni migration oluştur
dotnet ef migrations add AddNewFeature --context ApplicationDbContext --output-dir Migrations/Application

# 2. Build et
dotnet build

# 3. Uygulamayı çalıştır
dotnet run

# 4. Login yap - migration otomatik çalışır! 🎉
```

Her tenant login yaptığında kendi database'inde otomatik olarak yeni migration çalışacak!

## ⚠️ Önemli Notlar

1. **Production'da dikkat:** Büyük migration'lar login süresini uzatabilir
2. **Connection string güvenliği:** appsettings.json'da şifreleri saklamayın
3. **Migration rollback:** Otomatik rollback yok, dikkatli olun
4. **Concurrent migration:** Aynı tenant için paralel login'ler sorun çıkarabilir (locking düşünün)
5. **Error handling:** Migration hatası login'i bloklar (try-catch eklenebilir)
