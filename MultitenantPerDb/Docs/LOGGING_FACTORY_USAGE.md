# Logging Factory Pattern - Kullanım Kılavuzu

## Genel Bakış

Bu yapı ile `appsettings.json` dosyasında `Logging:Provider` ayarını değiştirerek **Microsoft.Extensions.Logging** veya **Serilog** arasında kolayca geçiş yapabilirsiniz.

## Konfigürasyon

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    },
    "Provider": "Microsoft"  // veya "Serilog"
  }
}
```

### Provider Seçenekleri

- `"Microsoft"` - Microsoft.Extensions.Logging kullanır (varsayılan)
- `"Serilog"` - Serilog kullanır

## Kullanım

### Constructor Injection

```csharp
using MultitenantPerDb.Core.Application.Interfaces;

public class ProductService : BaseService<ProductDbContext>
{
    private readonly IAppLogger<ProductService> _logger;

    public ProductService(
        ILoggerFactory loggerFactory,
        IUnitOfWork<ProductDbContext> unitOfWork) 
        : base(unitOfWork)
    {
        _logger = loggerFactory.CreateLogger<ProductService>();
    }

    public async Task<Product> CreateProductAsync(CreateProductDto dto)
    {
        _logger.LogInformation("Creating product: {ProductName}", dto.Name);
        
        try
        {
            var product = new Product { Name = dto.Name, Price = dto.Price };
            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Product created successfully. Id: {ProductId}", product.Id);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", dto.Name);
            throw;
        }
    }
}
```

## Log Metodları

```csharp
// Information
_logger.LogInformation("User logged in: {UserId}", userId);

// Warning
_logger.LogWarning("Invalid attempt: {Reason}", reason);

// Error
_logger.LogError("Operation failed: {Details}", details);
_logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

// Debug
_logger.LogDebug("Debug info: {Data}", data);

// Trace
_logger.LogTrace("Trace info: {Details}", details);

// Critical
_logger.LogCritical("Critical error: {Error}", error);
_logger.LogCritical(exception, "Critical exception: {Message}", exception.Message);
```

## Serilog Kurulumu (Opsiyonel)

Eğer Serilog kullanmak isterseniz:

### 1. NuGet Paketlerini Yükleyin

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

### 2. Program.cs'de Serilog'u Yapılandırın

```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog konfigürasyonu
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Logger Factory - appsettings.json'dan Provider seçimi
var loggingProvider = builder.Configuration["Logging:Provider"] ?? "Microsoft";
builder.Services.AddConfiguredLogger(loggingProvider);
```

### 3. appsettings.json'da Provider'ı Değiştirin

```json
{
  "Logging": {
    "Provider": "Serilog"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

## Avantajlar

1. ✅ **Esnek Geçiş**: Kod değişikliği yapmadan sadece config değiştirerek logger değiştirilebilir
2. ✅ **Tek Interface**: `ILoggerFactory` ve `IAppLogger<T>` her iki provider için aynı
3. ✅ **Dependency Injection**: Kolay test edilebilir ve mock'lanabilir
4. ✅ **Factory Pattern**: Yeni logger provider eklemek kolay
5. ✅ **Type-Safe**: Generic type ile her servis kendi logger'ını alır

## Yeni Logger Provider Ekleme

Yeni bir logger eklemek için:

1. `IAppLogger<T>` interface'ini implement eden bir class oluşturun
2. `ILoggerFactory` interface'ini implement eden bir factory class oluşturun
3. `LoggingServiceExtensions.cs`'e yeni bir extension method ekleyin
4. `AddConfiguredLogger` metoduna yeni case ekleyin
