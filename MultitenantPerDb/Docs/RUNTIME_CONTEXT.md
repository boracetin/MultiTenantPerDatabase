# Multi-Tenant Per Database - Complete Architecture

## 🎯 Genel Bakış

Bu proje, **JWT Authentication** ile **Multi-Tenant Per Database** mimarisini birleştiren, **Repository Pattern** ve **Unit of Work Pattern** kullanan, SOLID prensiplerine uygun bir ASP.NET Core 8 Web API projesidir.

### Temel Özellikler
- ✅ **JWT Authentication** - Token tabanlı kimlik doğrulama
- ✅ **User Claims'den TenantId** - Token'dan otomatik tenant çözümleme
- ✅ **Runtime DbContext** - Dinamik connection string
- ✅ **Repository Pattern** - Generic repository yapısı
- ✅ **Unit of Work** - Otomatik transaction yönetimi
- ✅ **SOLID Principles** - Temiz ve bakımı kolay kod

## 🏗️ Mimari Bileşenler

### 1. **TenantResolver** (Scoped)
- `ITenantResolver` / `TenantResolver`
- HTTP Context'ten TenantId'yi çözer ve tutar
- Her request için yeni bir instance oluşturulur

### 2. **TenantMiddleware**
- Her request başında çalışır
- HTTP Header'dan (`X-Tenant-ID`) veya query string'den TenantId'yi alır
- TenantResolver'a set eder

### 3. **TenantDbContextFactory** (Scoped)
- `ITenantDbContextFactory` / `TenantDbContextFactory`
- Runtime'da ApplicationDbContext oluşturur
- TenantResolver'dan TenantId alır
- TenantDbContext'ten connection string'i çeker
- Dinamik olarak DbContext oluşturup döner

### 4. **ProductsController**
- Örnek API controller
- Constructor'da `ITenantDbContextFactory` inject edilir
- Her action'da `CreateDbContextAsync()` ile yeni context oluşturulur
- `await using` ile otomatik dispose edilir

## 📝 Kullanım

### 1. Login ve Token Alma

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "user1",
  "password": "123456"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "username": "user1",
  "tenantId": 1,
  "expiresAt": "2025-11-02T10:00:00Z"
}
```

### 2. Token ile API Kullanımı

```http
GET /api/products
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### 3. Controller Kullanımı (SOLID Pattern)

```csharp
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork; // Tek dependency!
    }

    [HttpPost]
    public async Task<ActionResult> CreateProduct(Product product)
    {
        var repository = _unitOfWork.GetRepository<ProductRepository>();
        await repository.AddAsync(product);
        
        // Transaction otomatik yönetilir
        await _unitOfWork.SaveChangesAsync();
        
        return Ok(product);
    }
}
```

## 🔄 İş Akışı

### Authentication Flow
1. **Login** → Kullanıcı username/password ile giriş yapar
2. **JWT Token** → TenantId claim'i içeren token oluşturulur
3. **Token Dönülür** → Client token'ı alır ve saklar

### Request Flow (Authenticated)
1. **Request** → `Authorization: Bearer {token}` header'ı ile gelir
2. **JWT Validation** → Token doğrulanır, claims çıkarılır
3. **TenantMiddleware** → User claim'den TenantId'yi okur
4. **TenantResolver** → TenantId'yi context'te tutar
5. **Unit of Work** → Repository talep edilince factory'den DbContext oluşturur
6. **Factory** → TenantResolver'dan TenantId alır → TenantDbContext'ten connection string çeker
7. **Repository** → İlgili tenant'ın database'inde işlem yapar
8. **SaveChanges** → Otomatik transaction ile commit/rollback
9. **Dispose** → Tüm kaynaklar otomatik temizlenir

## ⚙️ Konfigürasyon

### appsettings.json
```json
{
  "ConnectionStrings": {
    "TenantConnection": "Server=localhost;Database=TenantMasterDb;..."
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKey...",
    "Issuer": "MultitenantPerDb",
    "Audience": "MultitenantPerDbUsers",
    "ExpirationHours": 24
  }
}
```

### Program.cs (Dependency Injection)
```csharp
// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(...);

// DbContexts
builder.Services.AddDbContext<TenantDbContext>(...);

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantResolver, TenantResolver>();
builder.Services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Middleware Pipeline
app.UseAuthentication();
app.UseAuthorization();
app.UseTenantResolver(); // Authentication'dan sonra!
```

## �️ SOLID Prensipleri

### Single Responsibility
- `TenantResolver` → Sadece tenant çözümleme
- `AuthService` → Sadece authentication
- `ProductRepository` → Sadece product işlemleri
- `UnitOfWork` → Sadece transaction ve repository yönetimi

### Open/Closed
- Generic `IRepository<T>` → Yeni entity'ler için extend edilebilir
- `IProductRepository : IRepository<Product>` → Özel metodlar eklenebilir

### Liskov Substitution
- `IUnitOfWork` interface'i → Mock edilebilir, test edilebilir
- `IRepository<T>` → Tüm repository'ler birbirinin yerine kullanılabilir

### Interface Segregation
- `IAuthService` → Sadece auth metodları
- `ITenantResolver` → Sadece tenant çözümleme
- `IUnitOfWork` → Sadece transaction yönetimi

### Dependency Inversion
- Controller'lar concrete class'lara değil interface'lere bağımlı
- `IUnitOfWork` → `ITenantDbContextFactory` → `ITenantResolver`
- Test edilebilir, değiştirilebilir

## 🎨 Transaction Yönetimi

### Otomatik Transaction (Default)
```csharp
await _unitOfWork.SaveChangesAsync(); // Transaction otomatik başlar ve commit edilir
```

### Transaction Olmadan Kaydetme
```csharp
await _unitOfWork.SaveChangesAsync(useTransaction: false);
```

### Kompleks İşlemler
```csharp
var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
{
    // İşlem 1
    await repository.AddAsync(product1);
    await _unitOfWork.SaveChangesAsync(false);
    
    // İşlem 2
    await repository.AddAsync(product2);
    await _unitOfWork.SaveChangesAsync(false);
    
    return 1;
}); // Tüm işlem tek transaction'da, hata olursa rollback
```

## 🧪 Test

### 1. Projeyi Çalıştırın
```powershell
dotnet run
```

### 2. Login Olun
```http
POST /api/auth/login
{
  "username": "user1",
  "password": "123456"
}
```

### 3. Token'ı Alın ve Kullanın
Token'ı `Authorization: Bearer {token}` header'ında gönderin.

### 4. Swagger'dan Test
`https://localhost:5xxx/swagger` adresinden Swagger UI'ı kullanabilirsiniz.

## 📋 Demo Kullanıcılar

| Username | Password | TenantId | Database |
|----------|----------|----------|----------|
| user1    | 123456   | 1        | Tenant1Db |
| user2    | 123456   | 2        | Tenant2Db |

## ⚠️ Önemli Notlar

1. **Token Zorunlu**: Tüm API endpoint'leri JWT token gerektirir (AuthController hariç)
2. **TenantId Otomatik**: Token'dan otomatik çözümlenir, header göndermek gerekmez
3. **Transaction Otomatik**: `SaveChangesAsync()` otomatik transaction yönetir
4. **Güvenlik**: Production'da `JwtSettings:SecretKey`'i mutlaka değiştirin!
