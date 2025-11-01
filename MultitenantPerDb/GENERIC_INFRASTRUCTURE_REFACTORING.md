# Generic Infrastructure Services Refactoring

## 📋 Overview

This document describes the architectural refactoring to separate **generic infrastructure concerns** from **domain-specific application logic**.

## 🎯 Problem Statement

**Before Refactoring:**
```
❌ Infrastructure Layer (Shared Kernel)
   └── IEmailService.SendProductCreatedNotificationAsync(productId, productName)
   └── ISmsService.SendOrderStatusAsync(phoneNumber, orderId, status)
   └── IInventoryApiService.SyncInventoryAsync(productId, stock)
```

**Issues:**
1. ❌ Infrastructure services contain domain knowledge (Product, Order, Inventory concepts)
2. ❌ Not reusable across different modules (Products, Orders, Users, etc.)
3. ❌ Violates Clean Architecture principles (inner layer depends on outer layer concepts)
4. ❌ Every module would need to add domain-specific methods to shared services

## ✅ Solution: Generic Infrastructure + Application Services

**After Refactoring:**
```
✅ Infrastructure Layer (Shared Kernel) - GENERIC
   └── IEmailService.SendEmailAsync(to, subject, body)
   └── ISmsService.SendSmsAsync(phoneNumber, message)
   └── IHttpClientService.PostAsync<TRequest, TResponse>(url, data)

✅ Application Layer (Products Module) - DOMAIN-SPECIFIC
   └── IProductNotificationService.SendProductCreatedNotificationAsync(productId, productName)
       └── Uses IEmailService internally
       └── Uses ISmsService internally
```

## 🏗️ New Architecture

### 1. Infrastructure Layer (Shared Kernel)

**Generic services - no domain knowledge:**

#### IEmailService
```csharp
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendTemplatedEmailAsync(string to, string subject, string templateName, Dictionary<string, string> templateData);
    Task SendEmailWithAttachmentsAsync(string to, string subject, string body, List<EmailAttachment> attachments);
}
```

#### ISmsService
```csharp
public interface ISmsService
{
    Task SendSmsAsync(string phoneNumber, string message);
    Task SendTemplatedSmsAsync(string phoneNumber, string templateKey, Dictionary<string, string> templateData);
}
```

#### IHttpClientService (Replaces IInventoryApiService)
```csharp
public interface IHttpClientService
{
    Task<TResponse?> GetAsync<TResponse>(string url, Dictionary<string, string>? headers = null);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data, Dictionary<string, string>? headers = null);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data, Dictionary<string, string>? headers = null);
    Task<bool> DeleteAsync(string url, Dictionary<string, string>? headers = null);
}
```

#### IFileStorageService
```csharp
public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string? contentType = null);
    Task<Stream> DownloadFileAsync(string fileUrl);
    Task DeleteFileAsync(string fileUrl);
    Task<bool> FileExistsAsync(string fileUrl);
    Task<string> GetFileUrlAsync(string fileName);
}
```

### 2. Application Layer (Products Module)

**Domain-specific orchestration:**

#### IProductNotificationService
```csharp
public interface IProductNotificationService
{
    Task SendProductCreatedNotificationAsync(int productId, string productName);
    Task SendLowStockAlertAsync(int productId, string productName, int currentStock);
    Task SendPriceChangedNotificationAsync(int productId, string productName, decimal oldPrice, decimal newPrice);
}
```

**Implementation:**
```csharp
public class ProductNotificationService : IProductNotificationService
{
    private readonly IEmailService _emailService; // Generic
    private readonly ISmsService _smsService; // Generic
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductNotificationService> _logger;

    public async Task SendProductCreatedNotificationAsync(int productId, string productName)
    {
        var adminEmail = _configuration["AdminEmail"] ?? "admin@example.com";
        
        // Compose domain-specific email content
        var subject = "New Product Created";
        var body = $@"
            <h2>New Product Added</h2>
            <p><strong>Product ID:</strong> {productId}</p>
            <p><strong>Product Name:</strong> {productName}</p>
            <p>This is an automated notification from the Product Management System.</p>
        ";

        // Use generic email service
        await _emailService.SendEmailAsync(adminEmail, subject, body);
    }

    public async Task SendLowStockAlertAsync(int productId, string productName, int currentStock)
    {
        var adminEmail = _configuration["AdminEmail"] ?? "admin@example.com";
        var adminPhone = _configuration["AdminPhone"] ?? "+905551234567";

        // Multi-channel notification for critical alerts
        
        // Email notification
        var emailSubject = $"⚠️ Low Stock Alert - {productName}";
        var emailBody = $@"
            <h2 style='color: red;'>Low Stock Warning</h2>
            <p><strong>Product:</strong> {productName} (ID: {productId})</p>
            <p><strong>Current Stock:</strong> {currentStock}</p>
            <p style='color: red;'><strong>Action Required:</strong> Please restock this item immediately.</p>
        ";
        await _emailService.SendEmailAsync(adminEmail, emailSubject, emailBody);

        // SMS notification for critical alerts
        var smsMessage = $"⚠️ LOW STOCK: {productName} - Only {currentStock} left in stock!";
        await _smsService.SendSmsAsync(adminPhone, smsMessage);
    }
}
```

## 📊 Comparison

| Aspect | Before (❌ Domain-Coupled) | After (✅ Generic) |
|--------|---------------------------|-------------------|
| **Reusability** | ❌ Product-specific only | ✅ Usable by all modules |
| **Maintainability** | ❌ Every module adds methods | ✅ Single responsibility |
| **Testability** | ❌ Hard to mock domain logic | ✅ Easy to mock generic services |
| **Clean Architecture** | ❌ Violates dependency rule | ✅ Follows dependency rule |
| **Separation of Concerns** | ❌ Infrastructure knows domain | ✅ Clear separation |

## 🔄 Handler Example

**Before:**
```csharp
public class CreateProductCommandHandler
{
    private readonly IEmailService _emailService;

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // ... create product logic ...

        // ❌ Direct domain-specific infrastructure call
        await _emailService.SendProductCreatedNotificationAsync(product.Id, product.Name);
    }
}
```

**After:**
```csharp
public class CreateProductCommandHandler
{
    private readonly IProductNotificationService _productNotification; // Application Service
    private readonly IHttpClientService _httpClient; // Infrastructure Service

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // ... create product logic ...

        // ✅ Domain-specific application service
        await _productNotification.SendProductCreatedNotificationAsync(product.Id, product.Name);

        // ✅ Generic HTTP client for external APIs
        await _httpClient.PostAsync<object, object>(
            "https://inventory-api.example.com/sync",
            new { ProductId = product.Id, Stock = product.Stock }
        );
    }
}
```

## 📦 Service Registration

### Infrastructure Services (Shared)
```csharp
// Shared/Kernel/Application/SharedServicesRegistration.cs
public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
{
    var useFakeServices = configuration.GetValue<bool>("UseFakeServices", true);

    // Generic Infrastructure Services
    if (useFakeServices)
    {
        services.AddScoped<IEmailService, FakeEmailService>();
        services.AddScoped<ISmsService, FakeSmsService>();
        services.AddScoped<IHttpClientService, FakeHttpClientService>();
    }
    else
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IHttpClientService, HttpClientService>();
    }

    // Domain Services (Shared)
    services.AddSingleton<IPriceCalculationService, PriceCalculationService>();

    return services;
}
```

### Application Services (Module-Specific)
```csharp
// Modules/Products/ProductsModule.cs
public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // ... other configurations ...

    // Application Services - Domain-specific orchestration
    services.AddScoped<IProductNotificationService, ProductNotificationService>();
}
```

## 🎯 Benefits

### 1. **Reusability**
Infrastructure services can be used by any module:
- Products module: Product notifications
- Orders module: Order confirmations
- Users module: Welcome emails
- Tenants module: Tenant setup notifications

### 2. **Single Responsibility**
- **Infrastructure Layer**: Handle external systems (SMTP, SMS gateway, HTTP)
- **Application Layer**: Compose domain-specific messages and orchestrate

### 3. **Easy Testing**
```csharp
// Mock generic services
var emailServiceMock = new Mock<IEmailService>();
emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
    .ReturnsAsync(true);

// Test domain logic
var service = new ProductNotificationService(emailServiceMock.Object, ...);
await service.SendProductCreatedNotificationAsync(1, "Test Product");

// Verify email was sent with correct parameters
emailServiceMock.Verify(x => x.SendEmailAsync(
    "admin@example.com",
    "New Product Created",
    It.Is<string>(body => body.Contains("Test Product"))
), Times.Once);
```

### 4. **Clean Architecture Compliance**
```
┌─────────────────────────────────────┐
│     Modules (Application Layer)    │  ← Domain-specific logic
│  ProductNotificationService         │
│  OrderNotificationService           │
└──────────────┬──────────────────────┘
               │ depends on ↓
┌──────────────▼──────────────────────┐
│  Shared Kernel (Infrastructure)    │  ← Generic services
│  IEmailService, ISmsService         │
│  IHttpClientService                 │
└─────────────────────────────────────┘
```

## 🚀 Migration Checklist

- [x] Refactor IEmailService to generic methods
- [x] Refactor ISmsService to generic methods
- [x] Remove IInventoryApiService
- [x] Create IHttpClientService
- [x] Update IFileStorageService with contentType
- [x] Create IProductNotificationService
- [x] Implement ProductNotificationService
- [x] Update EmailService implementation
- [x] Update SmsService implementation
- [x] Create HttpClientService implementation
- [x] Update FileStorageService implementation
- [x] Update SharedServicesRegistration
- [x] Register ProductNotificationService in ProductsModule
- [x] Update CreateProductCommandHandler
- [x] Update CreateProductWithServicesCommandHandler
- [x] Build and test

## 📝 Next Steps

1. **Create similar services for other modules:**
   - `IOrderNotificationService` (Orders module)
   - `IUserNotificationService` (Identity module)
   - `ITenantNotificationService` (Tenancy module)

2. **Add more generic infrastructure services:**
   - `ICacheService` (Redis, MemoryCache)
   - `IQueueService` (RabbitMQ, Azure Service Bus)
   - `IStorageService` (Azure Blob, AWS S3)

3. **Update documentation:**
   - Update SERVICE_ARCHITECTURE.md with new patterns
   - Add examples for each module

## 🎓 Key Takeaways

1. **Infrastructure services should be GENERIC** - no domain knowledge
2. **Application services orchestrate** - domain-specific logic
3. **Domain services contain** - pure business logic
4. **Handlers coordinate** - bring everything together
5. **Follow Clean Architecture** - dependencies point inward

---

**Status:** ✅ Refactoring Complete
**Date:** 2024
**Impact:** Breaking changes - all services updated
**Benefit:** Proper separation of concerns, reusability, maintainability
