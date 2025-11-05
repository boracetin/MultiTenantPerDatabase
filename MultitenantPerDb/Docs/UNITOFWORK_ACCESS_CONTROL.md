# Database Access Control Pattern

## 🎯 Amaç
Geliştiricilerin `DbContext`'e doğrudan erişmesini engellemek ve tüm database operasyonlarını **Repository Pattern** üzerinden zorlamak. Bu sayede:
- ✅ Database işlemleri merkezi olarak kontrol edilir
- ✅ Bypass edilemez iş kuralları (audit, soft delete, vb.) garanti edilir
- ✅ Mimari kurallar compile-time'da zorunlu kılınır

## 🔒 Nasıl Çalışır?

### 1. Marker Interface: `ICanAccessDbContext`
```csharp
public interface ICanAccessDbContext
{
    // Marker interface - metod gerektirmez
    // Yalnızca DbContext erişim kontrolü için kullanılır
    // Sadece Repository ve infrastructure bileşenleri implement eder
}
```

### 1.2. Marker Interface: `ICanAccessUnitOfWork`
```csharp
public interface ICanAccessUnitOfWork
{
    // Service'lerin UnitOfWork'e erişebilmesi için marker interface
}
```

### 2. Infrastructure Components (DbContext Access)
```csharp
// Repository - DbContext'e erişebilir
public class Repository<TEntity> : IRepository<TEntity>, ICanAccessDbContext
{
    protected readonly DbContext _context; // ✅ İzin verildi
}

// UnitOfWork - DbContext'e erişebilir
public class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext>, ICanAccessDbContext
{
    private TDbContext? _context; // ✅ İzin verildi
}

// DbContext Factory - DbContext oluşturabilir
public class ApplicationDbContextFactory : ICanAccessDbContext
{
    public async Task<ApplicationDbContext> CreateDbContextAsync() // ✅ İzin verildi
}

// Transaction Behaviors - UnitOfWork'e erişebilir
public class ApplicationDbTransactionBehavior : ICanAccessUnitOfWork
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork; // ✅ İzin verildi
}
```

### 3. Base Service Class: `BaseService<TDbContext>`
```csharp
public abstract class BaseService<TDbContext> : ICanAccessUnitOfWork
    where TDbContext : DbContext
{
    protected readonly IUnitOfWork<TDbContext> _unitOfWork;
    
    protected BaseService(IUnitOfWork<TDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork; // UnitOfWork üzerinden Repository'ye erişir
    }
}
```

### 3. Service Interface'leri `ICanAccessUnitOfWork` implement eder
```csharp
public interface IProductService : ICanAccessUnitOfWork
public interface IUserService : ICanAccessUnitOfWork
public interface ITenantService : ICanAccessUnitOfWork
```

### 4. Service Implementation'ları `BaseService` extends eder
```csharp
public class ProductService : BaseService<ApplicationDbContext>, IProductService
{
    public ProductService(IUnitOfWork<ApplicationDbContext> unitOfWork) 
        : base(unitOfWork)
    {
    }
    
    // _unitOfWork'e artık BaseService'den erişebilir
}
```

## ✅ Faydalar

### 🎯 Ana Fayda: Repository Pattern Bypass Edilemez
1. **DbContext Doğrudan Erişilemez**: Service'ler ve Controller'lar `DbContext` inject edemez
2. **Tüm DB İşlemleri Repository Üzerinden**: Audit logging, soft delete gibi kurallar bypass edilemez
3. **Mimari Tutarlılık**: Database erişimi yalnızca belirlenen katmanlarda
4. **Code Review Kolaylığı**: `ICanAccessDbContext` marker'ı ile erişim kontrolü açık
5. **Convention Zorlaması**: Service'ler `BaseService`'den türemeli, UnitOfWork → Repository pattern

### 🔒 İkincil Faydalar
6. **Kod Tekrarını Azaltır**: `_unitOfWork` field'ı tüm service'lerde ortak
7. **Transaction Yönetimi**: Behavior'lar merkezi olarak transaction kontrol eder
8. **Test Edilebilirlik**: Repository interface'i mock'lanabilir
9. **Açık Kontrat**: `ICanAccessUnitOfWork` ve `ICanAccessDbContext` rolleri açıkça belirtir

## 🚫 Engellenen Senaryolar

### ❌ Controller'da DbContext kullanımı
```csharp
// ✗ MİMARİ İHLAL - Controller DbContext'e doğrudan erişemez
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _context; // ❌ YANLIŞ!
    
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id); // ❌ Repository bypass!
    }
}
```

### ❌ Service'de DbContext kullanımı
```csharp
// ✗ MİMARİ İHLAL - Service DbContext'e doğrudan erişemez
public class ProductService
{
    private readonly ApplicationDbContext _context; // ❌ YANLIŞ!
    
    public async Task<Product> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id); // ❌ Repository bypass!
    }
}
```

### ❌ BaseService'den türemeyen servis
```csharp
// ✗ Convention ihlali - BaseService'den türemiyor
public class CustomService // ICanAccessUnitOfWork yok
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork; // ⚠️ Convention ihlali
}
```

## ✅ İzin Verilen Senaryolar

### ✓ Doğru Service Implementation
```csharp
public class ProductService : BaseService<ApplicationDbContext>, IProductService
{
    public ProductService(IUnitOfWork<ApplicationDbContext> unitOfWork) 
        : base(unitOfWork)
    {
    }
    
    public async Task<Product> CreateProductAsync(...)
    {
        var repository = _unitOfWork.GetRepository<Product>();
        // ... business logic
    }
}
```

## 📁 Dosya Yapısı

```
Shared/Kernel/
├── Domain/
│   ├── ICanAccessUnitOfWork.cs      ← Marker interface
│   └── IUnitOfWork.cs                ← UnitOfWork interface
├── Application/
│   └── BaseService.cs                ← Base service with protected UnitOfWork
└── Infrastructure/
    └── UnitOfWork.cs                 ← UnitOfWork implementation
```

## 🎓 Kullanım Kuralları

1. **Service Interface** → `ICanAccessUnitOfWork` implement et
2. **Service Implementation** → `BaseService<TDbContext>` extend et
3. **Constructor** → `base(unitOfWork)` çağır
4. **UnitOfWork Access** → `_unitOfWork` field'ını kullan (protected)

## 🔧 Yeni Service Ekleme

```csharp
// 1. Interface tanımla
public interface IOrderService : ICanAccessUnitOfWork
{
    Task<Order> CreateOrderAsync(...);
}

// 2. Implementation yap
public class OrderService : BaseService<ApplicationDbContext>, IOrderService
{
    public OrderService(IUnitOfWork<ApplicationDbContext> unitOfWork)
        : base(unitOfWork)
    {
    }
    
    public async Task<Order> CreateOrderAsync(...)
    {
        var repository = _unitOfWork.GetRepository<Order>();
        // Business logic...
    }
}
```

## 🎯 Sonuç

Bu pattern ile:
- ✅ **DbContext bypass edilemez** - Tüm DB işlemleri Repository'den geçer
- ✅ **Audit, soft delete gibi kurallar garanti edilir** - Repository katmanında zorunlu
- ✅ **Mimari kurallar convention ile zorunlu kılınır** - `ICanAccessDbContext` marker
- ✅ **Developer hataları önlenir** - DbContext doğrudan inject edilemez
- ✅ **Code review kolaylaşır** - Erişim noktaları açıkça işaretli
- ✅ **Clean Architecture prensipleri korunur** - Katmanlar arası sınırlar net
- ✅ **Repository Pattern zorunlu** - Alternative DB access yolu yok

## 📊 Erişim Hiyerarşisi

```
┌─────────────────────────────────────────────────────┐
│  Controller / API Layer                             │
│  ❌ DbContext YOK                                   │
│  ❌ UnitOfWork YOK                                  │
│  ✅ Service Interface (Dependency Injection)        │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│  Service Layer (BaseService)                        │
│  ❌ DbContext YOK                                   │
│  ✅ UnitOfWork (ICanAccessUnitOfWork)              │
│  ✅ Repository (UnitOfWork.GetRepository<T>())     │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│  Repository Layer (Repository<T>)                   │
│  ✅ DbContext (ICanAccessDbContext)                │
│  ✅ DbSet<T> operations                            │
│  ✅ Audit, SoftDelete enforcement                  │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│  Infrastructure (UnitOfWork, Factory, Behaviors)    │
│  ✅ DbContext (ICanAccessDbContext)                │
│  ✅ Transaction Management                         │
└─────────────────────────────────────────────────────┘
```
