# Transaction Behavior - Usage Examples

## 📚 Kullanım Şekilleri

### **1. Auto-Discovery (Önerilen - Tüm Aktif Contextler)**
```csharp
[Transactional]
public class CreateOrderCommand : IRequest<Result>
{
    public string ProductName { get; set; }
    public decimal Price { get; set; }
}

// Behavior otomatik olarak request scope'daki TÜM UnitOfWork'leri bulur ve yönetir
// Örnek: ProductsDbContext + TenancyDbContext (eğer inject edildiyse)
```

---

### **2. Specific DbContexts (Belirli Contextler)**
```csharp
[Transactional(typeof(TenancyDbContext), typeof(ProductsDbContext))]
public class CreateTenantWithProductCommand : IRequest<Result>
{
    public string TenantName { get; set; }
    public List<Product> InitialProducts { get; set; }
}

// Sadece TenancyDbContext ve ProductsDbContext için transaction açar
// Diğer contextler ignore edilir
```

---

### **3. Exclude Specific Contexts**
```csharp
[Transactional(ExcludedDbContextTypes = new[] { typeof(AuditLogDbContext) })]
public class UpdateProductCommand : IRequest<Result>
{
    public int ProductId { get; set; }
    public string Name { get; set; }
}

// AuditLogDbContext hariç tüm aktif contextler için transaction açar
// AuditLog hatası product update'i rollback etmez
```

---

### **4. Interface-Based (Legacy Support)**
```csharp
public class DeleteProductCommand : IRequest<Result>, ITransactionalCommand
{
    public int ProductId { get; set; }
}

// ITransactionalCommand implement eder = auto-discovery aktif
// Attribute kullanmaya gerek yok
```

---

### **5. No Transaction**
```csharp
public class GetProductQuery : IRequest<ProductDto>
{
    public int ProductId { get; set; }
}

// Attribute yok = Transaction YOK
// Sadece read operation için kullanılır
```

---

### **6. Manual Context Control**
```csharp
[Transactional(DbContextTypes = new[] { typeof(ProductsDbContext) })]
public class BulkUpdateProductsCommand : IRequest<Result>
{
    public List<int> ProductIds { get; set; }
    public decimal NewPrice { get; set; }
}

// Sadece ProductsDbContext için transaction
// TenancyDbContext'e dokunmaz
```

---

## 🎯 Avantajlar

### ✅ Eski Yapı (Hardcoded):
```csharp
// ❌ Static type names
private static Type GetTenancyDbContextType()
{
    return Type.GetType("MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence.TenancyDbContext, MultitenantPerDb");
}

// ❌ Yeni module eklediğinde kod değişikliği gerekiyor
```

### ✅ Yeni Yapı (Generic):
```csharp
// ✅ Attribute-based
[Transactional(typeof(NewModuleDbContext))]

// ✅ Auto-discovery - kod değişikliği YOK!
[Transactional] // Tüm aktif contextleri otomatik bulur

// ✅ Flexible exclusions
[Transactional(ExcludedDbContextTypes = new[] { typeof(LogDbContext) })]
```

---

## 🚀 Advanced Scenarios

### **Scenario 1: Non-Critical Operations (Audit Log)**
```csharp
// Main Command - Transaction YOK
public class CreateOrderCommand : IRequest<Result>
{
    // ...
}

// Handler içinde:
public async Task<Result> Handle(CreateOrderCommand request, CancellationToken ct)
{
    // Critical operation WITH transaction
    await _productUow.BeginTransactionAsync();
    try
    {
        await _productUow.GetRepository<Order, int>().AddAsync(order);
        await _productUow.SaveChangesAsync();
        await _productUow.CommitTransactionAsync();
    }
    catch
    {
        await _productUow.RollbackTransactionAsync();
        throw;
    }
    
    // Non-critical - separate context, NO rollback
    try
    {
        await _auditLogService.LogOrderCreatedAsync(order.Id);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Audit log failed but order created successfully");
    }
}
```

### **Scenario 2: Partial Commit (Savepoint Alternative)**
```csharp
[Transactional(typeof(OrderDbContext))]
public class ProcessOrderWithNotificationCommand : IRequest<Result>
{
    // ...
}

// Handler:
public async Task<Result> Handle(...)
{
    // Order insert - critical
    await _orderRepository.AddAsync(order);
    await _unitOfWork.SaveChangesAsync();
    
    // Notification - non-critical (başka bir command ile)
    await _mediator.Send(new SendOrderNotificationCommand { OrderId = order.Id });
    // ^ Bu command attribute'sız, transaction YOK
    
    return Result.Success();
}
```

---

## 📊 Performance Comparison

| Yaklaşım | Context Discovery | Maintainability | Flexibility |
|----------|------------------|----------------|-------------|
| **Eski (Hardcoded)** | Static type reflection | ❌ Düşük | ❌ Düşük |
| **Yeni (Generic)** | Auto-discovery + Attribute | ✅ Yüksek | ✅ Yüksek |

---

## 🔥 Migration Guide

### Eski Kod:
```csharp
// Her yeni module için kod değişikliği
private List<IUnitOfWorkBase> GetActiveUnitOfWorks()
{
    var unitOfWorks = new List<IUnitOfWorkBase>();
    TryAddUnitOfWork(unitOfWorks, GetTenancyDbContextType(), "TenancyDbContext");
    TryAddUnitOfWork(unitOfWorks, GetProductsDbContextType(), "ProductsDbContext");
    TryAddUnitOfWork(unitOfWorks, GetNewModuleDbContextType(), "NewModuleDbContext"); // ❌ Manuel ekleme
    return unitOfWorks;
}
```

### Yeni Kod:
```csharp
// Attribute ekle, transaction otomatik yönetilir
[Transactional] // ✅ Tüm contextler otomatik bulunur
public class MyCommand : IRequest<Result> { }

// VEYA specific:
[Transactional(typeof(TenancyDbContext), typeof(NewModuleDbContext))]
public class MyCommand : IRequest<Result> { }
```
