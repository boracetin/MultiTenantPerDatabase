# Roslyn Analyzer - Compile-Time Architecture Control

## 🎯 Başarı! Gerçek Compile-Time Kontrol Aktif

Bu projede **3 adet Roslyn Analyzer** ile mimari kurallar derleme zamanında zorunlu kılınıyor.

---

## 📦 Analyzer Projesi

### Dosya Yapısı
```
MultitenantPerDb.Analyzers/
├── DbContextAccessAnalyzer.cs         ← MTDB001
├── UnitOfWorkAccessAnalyzer.cs        ← MTDB002
├── ServiceInheritanceAnalyzer.cs      ← MTDB003
└── MultitenantPerDb.Analyzers.csproj
```

### NuGet Paketleri
```xml
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" />
<PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" />
```

---

## 🔍 Analyzer Detayları

### MTDB001: Unauthorized DbContext Access ❌ ERROR

**Kural:** DbContext'e yalnızca `ICanAccessDbContext` implement eden sınıflar erişebilir.

**Kontrol Edilen Yerler:**
- Constructor parameters
- Field declarations

**Örnek İhlal:**
```csharp
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _context; // ❌ MTDB001: ERROR!
    
    public ProductController(ApplicationDbContext context) // ❌ DERLENMEZ!
    {
        _context = context;
    }
}
```

**Hata Mesajı:**
```
error MTDB001: Type 'ProductController' cannot access DbContext 'ApplicationDbContext'. 
Only types implementing ICanAccessDbContext are allowed to inject DbContext.
```

**Doğru Kullanım:**
```csharp
// ✅ Repository - ICanAccessDbContext implement ediyor
public class Repository<TEntity> : IRepository<TEntity>, ICanAccessDbContext
{
    protected readonly DbContext _context; // ✅ İzin verildi
}

// ✅ Controller - Service kullanıyor
public class ProductController : ControllerBase
{
    private readonly IProductService _productService; // ✅ Doğru yaklaşım
}
```

---

### MTDB002: Unauthorized UnitOfWork Access in Controller ❌ ERROR

**Kural:** Controller'lar `IUnitOfWork` kullanamaz. Service layer kullanmalılar.

**Kontrol Edilen Yerler:**
- Controller constructor parameters

**Örnek İhlal:**
```csharp
public class ProductController : ControllerBase
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork; // ❌ MTDB002: ERROR!
    
    public ProductController(IUnitOfWork<ApplicationDbContext> unitOfWork) // ❌ DERLENMEZ!
    {
        _unitOfWork = unitOfWork;
    }
}
```

**Hata Mesajı:**
```
error MTDB002: Controller 'ProductController' cannot access IUnitOfWork. 
Controllers should use Service layer, not UnitOfWork directly.
```

**Doğru Kullanım:**
```csharp
// ✅ Controller - Service inject ediyor
public class ProductController : ControllerBase
{
    private readonly IProductService _productService; // ✅ Doğru
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        return Ok(product);
    }
}
```

---

### MTDB003: Service must inherit from BaseService ⚠️ WARNING

**Kural:** Service sınıfları `BaseService<TDbContext>` extend etmeli.

**Kontrol Edilen Yerler:**
- `.Application.Services` namespace'indeki
- `*Service` ile biten
- Concrete (non-abstract) sınıflar

**Örnek İhlal:**
```csharp
// ⚠️ MTDB003: WARNING
public class OrderService : IOrderService
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    
    public OrderService(IUnitOfWork<ApplicationDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
```

**Uyarı Mesajı:**
```
warning MTDB003: Service class 'OrderService' should inherit from BaseService<TDbContext>. 
This ensures proper UnitOfWork access control.
```

**Doğru Kullanım:**
```csharp
// ✅ BaseService'den türüyor
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

---

## 🛠️ Kullanım

### 1. Analyzer Projesini Referans Etme

Ana projenin `.csproj` dosyasına:

```xml
<ItemGroup>
  <ProjectReference Include="..\MultitenantPerDb.Analyzers\MultitenantPerDb.Analyzers.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### 2. Build ile Otomatik Kontrol

```bash
dotnet build
```

**Çıktı:**
```
error MTDB001: Type 'ProductController' cannot access DbContext 'ApplicationDbContext'...
error MTDB002: Controller 'ProductController' cannot access IUnitOfWork...
warning MTDB003: Service class 'OrderService' should inherit from BaseService<TDbContext>...

Build FAILED.
```

### 3. IDE Entegrasyonu

- ✅ Visual Studio: Otomatik olarak gösterir
- ✅ VS Code: C# extension ile çalışır
- ✅ Rider: Analyzer'ları algılar

**IDE'de Görünüm:**
```
ProductController.cs(19,22): error MTDB001 ━━━━━━━━━━━
                                           ⚠️ Red squiggly line
    private readonly ApplicationDbContext _context;
                     ^^^^^^^^^^^^^^^^^^^^
```

---

## ✅ Faydalar

### 1. Gerçek Compile-Time Kontrol 🔒
- ❌ Yanlış kod **DERLENMEZ**
- ❌ CI/CD pipeline'da **build fail**
- ❌ Production'a **hatalı kod gitmez**

### 2. IDE Entegrasyonu 💡
- Anında kırmızı çizgi
- Hover ile açıklama
- Code fix suggestions (ileride eklenebilir)

### 3. Zero Runtime Cost ⚡
- Compile-time kontrolü
- Production'da performans kaybı yok
- Analyzer binary'si production'a gitmez

### 4. Ekip İçin Otomatik Zorlama 👥
- Code review gerektirmez
- Developer hataları engellenir
- Mimari kurallar garanti edilir

### 5. Dokümantasyon Yerini Tutar 📚
- Hata mesajları açıklayıcı
- Doğru kullanım örnekleri
- Link ile detaylı doküman

---

## 📊 Test Sonuçları

### Başarılı Tespit Edilen İhlaller (Build sırasında)

1. ✅ **TenantBrandingController**: `MainDbContext` inject ediyordu → **MTDB001 ERROR**
2. ✅ **AuthService**: `MainDbContext` inject ediyordu → **MTDB001 ERROR**
3. ✅ **AuthService**: `BaseService`'den türemiyordu → **MTDB003 WARNING**
4. ✅ **ProductNotificationService**: `BaseService`'den türemiyordu → **MTDB003 WARNING**

### Düzeltmeler Sonrası

```bash
dotnet build
# ✅ Build başarılı!
# ⚠️ Sadece 2 warning (AuthService ve ProductNotificationService)
# ❌ Hiç error yok!
```

---

## 🔧 Genişletme

### Yeni Analyzer Ekleme

```csharp
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RepositoryAccessAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MTDB004";
    
    // ... implementation
}
```

### Code Fix Provider Ekleme (İleride)

```csharp
[ExportCodeFixProvider(LanguageNames.CSharp)]
public class DbContextAccessCodeFixProvider : CodeFixProvider
{
    // Otomatik düzeltme önerileri
    // Örn: "IProductService inject et" butonu
}
```

---

## 🎯 Sonuç

✅ **Gerçek compile-time kontrol aktif**
✅ **DbContext bypass edilemiyor**
✅ **Controller'lar UnitOfWork kullanamıyor**
✅ **IDE entegrasyonu çalışıyor**
✅ **CI/CD otomatik kontrolü**
✅ **Zero runtime cost**

**Mimari kurallar artık zorlanıyor, tercih değil!** 🛡️
