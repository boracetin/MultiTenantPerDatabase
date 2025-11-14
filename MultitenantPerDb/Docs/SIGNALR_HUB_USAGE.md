# SignalR Hub - Real-time Notification System

## Genel Bakış

Core katmanında generic SignalR Hub yapısı ile tüm modüller için real-time bildirim sistemi.

## Mimari

### Core Katman (Generic)

- **`IHubNotificationService<TEntity>`** - Hub notification service interface
- **`BaseHubNotificationService<TEntity, THub>`** - Generic implementation
- **`BaseTenantHub`** - Tenant-aware base Hub class
- **`HubNotificationEvent<TEntity>`** - Generic event payload

### Modül Implementasyonları

Her modül kendi Hub'ını implement eder:

1. **ProductsHub** - `/hubs/products`
2. **UserHub** - `/hubs/users`
3. **TenancyHub** - `/hubs/tenancy`

## Backend Kullanımı

### Service'den Notification Gönderme

```csharp
public class ProductService : BaseService<ProductDbContext>
{
    private readonly IHubNotificationService<Product> _hubNotification;

    public ProductService(
        IUnitOfWork<ProductDbContext> unitOfWork,
        IHubNotificationService<Product> hubNotification) 
        : base(unitOfWork)
    {
        _hubNotification = hubNotification;
    }

    public async Task<Product> CreateProductAsync(CreateProductDto dto)
    {
        var product = Product.Create(dto.Name, dto.Description, dto.Price, dto.Stock);
        
        await _unitOfWork.Repository<Product>().AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        
        // Real-time notification gönder
        await _hubNotification.NotifyCreatedAsync(product);
        
        return product;
    }

    public async Task UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        
        var oldPrice = product.Price;
        product.UpdateDetails(dto.Name, dto.Description, dto.Price);
        
        await _unitOfWork.SaveChangesAsync();
        
        // Update notification
        await _hubNotification.NotifyUpdatedAsync(product);
        
        // Custom notification for price change
        if (oldPrice != product.Price && _hubNotification is ProductHubNotificationService productHub)
        {
            await productHub.NotifyPriceChangedAsync(product, oldPrice);
        }
    }

    public async Task DeleteProductAsync(int id)
    {
        await _unitOfWork.Repository<Product>().DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
        
        // Delete notification
        await _hubNotification.NotifyDeletedAsync(id);
    }
}
```

### Özel Bildirimler

```csharp
// ProductHubNotificationService kullanımı
public class ProductStockService
{
    private readonly ProductHubNotificationService _productHub;

    public async Task CheckLowStockAsync(Product product)
    {
        if (product.IsLowStock())
        {
            await _productHub.NotifyLowStockAsync(product);
        }
    }
}

// UserHubNotificationService kullanımı
public class UserManagementService
{
    private readonly UserHubNotificationService _userHub;

    public async Task ChangeUserRoleAsync(User user, string newRole)
    {
        user.UpdateRole(newRole);
        await _unitOfWork.SaveChangesAsync();
        
        await _userHub.NotifyRoleChangedAsync(user, newRole);
    }
}

// TenantHubNotificationService kullanımı
public class TenantService
{
    private readonly TenantHubNotificationService _tenantHub;

    public async Task CheckSubscriptionExpiryAsync(Tenant tenant)
    {
        var daysRemaining = (tenant.SubscriptionEndDate - DateTime.UtcNow).Days;
        
        if (daysRemaining <= 7 && daysRemaining > 0)
        {
            await _tenantHub.NotifySubscriptionExpiringAsync(tenant, daysRemaining);
        }
    }
}
```

## Frontend Kullanımı (JavaScript/TypeScript)

### SignalR Bağlantısı

```typescript
import * as signalR from "@microsoft/signalr";

// Products Hub Connection
const productsConnection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:5231/hubs/products", {
        accessTokenFactory: () => getAuthToken() // JWT token
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Start connection
await productsConnection.start();
console.log("Connected to Products Hub");

// Listen to events
productsConnection.on("EntityCreated", (event) => {
    console.log("Product created:", event);
    // UI'ı güncelle
    addProductToList(event.entity);
});

productsConnection.on("EntityUpdated", (event) => {
    console.log("Product updated:", event);
    updateProductInList(event.entity);
});

productsConnection.on("EntityDeleted", (event) => {
    console.log("Product deleted:", event);
    removeProductFromList(event.entityId);
});

// Custom events
productsConnection.on("LowStockAlert", (data) => {
    console.warn("Low stock alert:", data);
    showNotification(`Low stock: ${data.productName} - ${data.currentStock} remaining`);
});

productsConnection.on("PriceChanged", (data) => {
    console.log("Price changed:", data);
    showNotification(`Price changed for ${data.productName}: ${data.newPrice}`);
});
```

### React Hook Örneği

```typescript
// useProductsHub.ts
import { useEffect, useState } from 'react';
import * as signalR from "@microsoft/signalr";

export const useProductsHub = () => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [products, setProducts] = useState<Product[]>([]);

    useEffect(() => {
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/products", {
                accessTokenFactory: () => localStorage.getItem("token") || ""
            })
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);
    }, []);

    useEffect(() => {
        if (connection) {
            connection.start()
                .then(() => console.log('Connected'))
                .catch(err => console.error('Connection error:', err));

            connection.on("EntityCreated", (event) => {
                setProducts(prev => [...prev, event.entity]);
            });

            connection.on("EntityUpdated", (event) => {
                setProducts(prev => 
                    prev.map(p => p.id === event.entity.id ? event.entity : p)
                );
            });

            connection.on("EntityDeleted", (event) => {
                setProducts(prev => prev.filter(p => p.id !== event.entityId));
            });

            connection.on("LowStockAlert", (data) => {
                toast.warning(`Low stock: ${data.productName}`);
            });
        }

        return () => {
            connection?.stop();
        };
    }, [connection]);

    return { connection, products };
};

// Component içinde kullanım
function ProductList() {
    const { products } = useProductsHub();

    return (
        <div>
            {products.map(product => (
                <ProductCard key={product.id} product={product} />
            ))}
        </div>
    );
}
```

### Gruplar ve Özel Kanallar

```typescript
// Belirli bir ürüne subscribe ol
await productsConnection.invoke("SubscribeToProduct", productId);

// Belirli bir kullanıcıya subscribe ol (admin)
await userConnection.invoke("SubscribeToUser", userId);

// Tenant events'e subscribe ol (admin)
await tenancyConnection.invoke("SubscribeToTenantEvents", tenantId);

// Unsubscribe
await productsConnection.invoke("UnsubscribeFromProduct", productId);
```

## Tenant İzolasyonu

Tüm Hub'lar otomatik olarak tenant-aware:

- Her bağlantı otomatik olarak `Tenant_{tenantId}` grubuna eklenir
- Bildirimler sadece kendi tenant'larına gönderilir
- JWT token'dan tenant bilgisi alınır

```csharp
// BaseTenantHub otomatik olarak yapıyor
public override async Task OnConnectedAsync()
{
    var tenantId = _tenantResolver.TenantId;
    
    if (!string.IsNullOrEmpty(tenantId))
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Tenant_{tenantId}");
    }
    
    await base.OnConnectedAsync();
}
```

## Event Tipleri

### Generic Events (Tüm Hub'larda)

- `EntityCreated` - Yeni entity oluşturuldu
- `EntityUpdated` - Entity güncellendi
- `EntityDeleted` - Entity silindi

### Products Hub Özel Events

- `LowStockAlert` - Düşük stok uyarısı
- `PriceChanged` - Fiyat değişikliği
- `ProductUpdated` - Ürün güncellendi

### User Hub Özel Events

- `RoleChanged` - Kullanıcı rolü değişti
- `AccountStatusChanged` - Hesap durumu değişti
- `UserProfileUpdated` - Profil güncellendi

### Tenancy Hub Özel Events

- `NewTenantRegistered` - Yeni tenant kaydı (admin)
- `TenantStatusChanged` - Tenant durumu değişti
- `SubscriptionExpiring` - Abonelik sona eriyor

## CORS ve Güvenlik

`Program.cs` içinde CORS yapılandırması:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

JWT Authentication ile korumalı:

```csharp
.withUrl("/hubs/products", {
    accessTokenFactory: () => getAuthToken()
})
```

## Test Etme

### Postman/HTTP Client

SignalR bağlantısını test etmek için tarayıcı console:

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:5231/hubs/products")
    .build();

await connection.start();
console.log("Connected!");

connection.on("EntityCreated", console.log);
```

### Swagger ile Test

API üzerinden işlem yapın, tarayıcı console'da notification'ları görün.

## Best Practices

1. ✅ Her zaman JWT token kullanın
2. ✅ Bağlantı koptuğunda `withAutomaticReconnect()` kullanın
3. ✅ Component unmount'ta connection'ı kapatın
4. ✅ Error handling ekleyin
5. ✅ Tenant izolasyonunu kontrol edin
6. ✅ Production'da connection pool size'ı ayarlayın
