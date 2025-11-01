# 🏗️ CQRS'te Service Kullanımı - Mimari Rehber

## 📊 Service Katmanları

```
┌─────────────────────────────────────────────────────────────┐
│                    API Controllers                           │
│                   (Thin Controllers)                         │
└────────────────────────┬────────────────────────────────────┘
                         │ IMediator.Send()
                         ↓
┌─────────────────────────────────────────────────────────────┐
│              Command/Query Handlers                          │
│              (Orchestration Logic)                           │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ • CreateProductCommandHandler                        │   │
│  │ • GetProductByIdQueryHandler                         │   │
│  │ • UpdateProductStockCommandHandler                   │   │
│  └─────────────────────────────────────────────────────┘   │
└──────┬──────────────────────┬─────────────────┬────────────┘
       │                      │                 │
       ↓                      ↓                 ↓
┌──────────────┐    ┌────────────────┐   ┌──────────────────┐
│ Domain       │    │ Application    │   │ Infrastructure   │
│ Services     │    │ Services       │   │ Services         │
│              │    │                │   │                  │
│ • Price      │    │ • Product      │   │ • Email          │
│   Calculation│    │   Orchestration│   │   Notification   │
│ • Validation │    │ • Order        │   │ • SMS            │
│   Rules      │    │   Processing   │   │ • File Storage   │
│ • Business   │    │ • Workflow     │   │ • External API   │
│   Logic      │    │   Coordination │   │ • Cache          │
└──────────────┘    └────────────────┘   └──────────────────┘
```

## 🎯 3 Tip Service ve Kullanım Yerleri

### 1️⃣ Domain Services (Domain Layer)
**Ne zaman kullanılır?**
- Birden fazla entity/aggregate ile çalışan business logic
- Domain model'e ait ama tek bir entity'de yer alamayan kurallar
- Pure business logic (infrastructure dependency yok)

**Örnekler:**
```csharp
// ✅ Domain Service
public interface IPriceCalculationService
{
    decimal CalculateFinalPrice(decimal basePrice, decimal taxRate, decimal discount);
    bool ShouldApplyBulkDiscount(int quantity, decimal amount);
}

// ✅ Domain Service
public interface IOrderValidationService  
{
    bool CanCreateOrder(Customer customer, List<Product> products);
    bool IsEligibleForFreeShipping(decimal totalAmount);
}
```

**Handler'da kullanım:**
```csharp
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderValidationService _orderValidation; // Domain Service
    private readonly IPriceCalculationService _priceCalculation; // Domain Service
    
    public async Task<OrderDto> Handle(CreateOrderCommand request, ...)
    {
        // Domain service ile validation
        if (!_orderValidation.CanCreateOrder(customer, products))
            throw new BusinessException("Cannot create order");
            
        // Domain service ile hesaplama
        var totalPrice = _priceCalculation.CalculateFinalPrice(...);
        
        // Entity oluştur
        var order = Order.Create(customer, products, totalPrice);
        
        // Repository'e kaydet
        await _repository.AddAsync(order);
    }
}
```

---

### 2️⃣ Application Services (Application Layer)
**Ne zaman kullanılır?**
- Karmaşık workflow orchestration
- Birden fazla handler'ı koordine etme
- Transaction yönetimi gereken kompleks işlemler
- Cross-module operations

**Örnekler:**
```csharp
// ✅ Application Service - Orchestration
public interface IOrderOrchestrationService
{
    Task<OrderResult> CreateOrderWithPaymentAsync(
        CreateOrderCommand orderCommand,
        PaymentInfo paymentInfo
    );
}

// Implementation
public class OrderOrchestrationService : IOrderOrchestrationService
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<OrderResult> CreateOrderWithPaymentAsync(...)
    {
        // 1. Validate order
        var validationResult = await _mediator.Send(new ValidateOrderQuery(...));
        
        // 2. Reserve stock
        await _mediator.Send(new ReserveStockCommand(...));
        
        // 3. Process payment
        var paymentResult = await _mediator.Send(new ProcessPaymentCommand(...));
        
        // 4. Create order
        var order = await _mediator.Send(new CreateOrderCommand(...));
        
        // 5. Send confirmation
        await _mediator.Send(new SendOrderConfirmationCommand(...));
        
        return new OrderResult { OrderId = order.Id, Success = true };
    }
}
```

**❌ YAPILAMAZ - Handler'dan Handler çağrısı:**
```csharp
// ❌ YANLIŞ - Handler'dan başka handler çağırma
public class CreateOrderCommandHandler
{
    private readonly IMediator _mediator;
    
    public async Task<OrderDto> Handle(...)
    {
        // ❌ Handler'dan başka command/query gönderme
        await _mediator.Send(new ReserveStockCommand(...));
        await _mediator.Send(new ProcessPaymentCommand(...));
    }
}
```

**✅ DOĞRU - Application Service kullan:**
```csharp
// ✅ DOĞRU - Orchestration için Application Service
public class CreateOrderCommandHandler
{
    private readonly IOrderOrchestrationService _orchestration;
    
    public async Task<OrderDto> Handle(...)
    {
        // Application service koordine eder
        var result = await _orchestration.CreateOrderWithPaymentAsync(...);
    }
}
```

---

### 3️⃣ Infrastructure Services (Infrastructure Layer)
**Ne zaman kullanılır?**
- External system integration (Email, SMS, API)
- File storage (Azure Blob, AWS S3)
- Caching (Redis, MemoryCache)
- Message queues (RabbitMQ, Kafka)
- Third-party API calls

**Örnekler:**
```csharp
// ✅ Infrastructure Service - Email
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendOrderConfirmationAsync(int orderId);
}

// ✅ Infrastructure Service - Storage
public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName);
    Task DeleteFileAsync(string fileUrl);
}

// ✅ Infrastructure Service - External API
public interface IPaymentGatewayService
{
    Task<PaymentResult> ProcessPaymentAsync(decimal amount, string cardNumber);
    Task<bool> RefundPaymentAsync(string transactionId);
}
```

**Handler'da kullanım:**
```csharp
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService; // Infrastructure Service
    private readonly IPaymentGatewayService _paymentGateway; // Infrastructure Service
    
    public async Task<OrderDto> Handle(...)
    {
        // 1. Business logic
        var order = Order.Create(...);
        await _repository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        
        // 2. Infrastructure service - Payment
        try 
        {
            await _paymentGateway.ProcessPaymentAsync(...);
        }
        catch (Exception ex)
        {
            // Payment failed - rollback or mark order as pending
            _logger.LogError(ex, "Payment failed for order {OrderId}", order.Id);
            throw;
        }
        
        // 3. Infrastructure service - Email (fire-and-forget)
        _ = Task.Run(async () => 
        {
            try
            {
                await _emailService.SendOrderConfirmationAsync(order.Id);
            }
            catch (Exception ex)
            {
                // Email failure doesn't break the flow
                _logger.LogError(ex, "Email failed");
            }
        });
        
        return _mapper.Map<OrderDto>(order);
    }
}
```

---

## 🔄 AuthService'i CQRS'e Göre Yeniden Yapılandırma

### ❌ ESKİ YÖNTEM (AuthService)
```csharp
public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    string GenerateJwtToken(...);
}

[HttpPost("login")]
public async Task<ActionResult> Login([FromBody] LoginRequestDto request)
{
    var result = await _authService.LoginAsync(request);
    return Ok(result);
}
```

### ✅ YENİ YÖNTEM (CQRS Handler)
```csharp
// Command
public record LoginCommand(string Username, string Password) : IRequest<LoginResponseDto>;

// Handler
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService; // Infrastructure Service
    
    public async Task<LoginResponseDto> Handle(...)
    {
        var user = await _repository.GetByUsernameAsync(request.Username);
        
        // Verify password
        if (!VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");
        
        // Infrastructure service ile token generate et
        var token = _jwtTokenService.GenerateToken(user);
        
        return new LoginResponseDto { Token = token, ... };
    }
}

// Controller
[HttpPost("login")]
public async Task<ActionResult> Login([FromBody] LoginCommand command)
{
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

---

## 📋 Service Kullanım Karar Ağacı

```
İhtiyacınız olan işlem nedir?
│
├─ Pure business logic mi?
│  ├─ Tek entity → Entity method
│  └─ Çok entity → Domain Service
│
├─ External system integration mi?
│  └─ Infrastructure Service (Email, SMS, API, etc.)
│
├─ Kompleks orchestration mi?
│  ├─ Tek işlem → Command Handler
│  └─ Multi-step workflow → Application Service
│
└─ Background job mi?
   └─ Background Job Service + Handler
```

---

## 💡 Best Practices

### ✅ DOĞRU
1. Handler'lar business logic'i domain service'lere delege eder
2. Infrastructure concerns (email, sms) ayrı service'ler
3. Kompleks workflow için Application Service kullan
4. Domain service'ler stateless ve pure function
5. Infrastructure service'ler interface'ler üzerinden

### ❌ YANLIŞ
1. Handler'dan başka handler çağırma
2. Domain service'de infrastructure dependency
3. Her şey için service oluşturma (over-engineering)
4. Controller'da business logic
5. Fat handler'lar (handler çok büyümüş)

---

## 📊 Özet

| Service Type | Layer | Dependencies | Kullanım |
|-------------|-------|--------------|----------|
| **Domain Service** | Domain | Sadece domain | Complex business rules |
| **Application Service** | Application | Domain + Infrastructure | Orchestration |
| **Infrastructure Service** | Infrastructure | External systems | Email, SMS, API |
| **Background Job** | Application | Services + Repositories | Scheduled tasks |

**Handler = Orchestrator**
- Handler'lar yöneticidir, iş dağıtır
- Asıl işi service'ler yapar
- Handler ince (thin) olmalı
