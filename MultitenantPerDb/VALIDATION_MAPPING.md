# Validation ve Mapping Kullanımı

## 📦 Yüklü Paketler

- ✅ **FluentValidation** (12.0.0)
- ✅ **FluentValidation.DependencyInjectionExtensions** (12.0.0)
- ✅ **Mapster** (7.4.0)
- ✅ **Mapster.DependencyInjection** (1.0.1)

## 📁 Klasör Yapısı

Her modülde validasyon ve mapping için ayrı klasörler:

```
Modules/
├── Products/
│   └── Application/
│       ├── Validators/           ← FluentValidation validators
│       │   ├── CreateProductCommandValidator.cs
│       │   └── UpdateProductCommandValidator.cs
│       └── Mappings/              ← Mapster configurations
│           └── ProductMappingConfig.cs
│
├── Identity/
│   └── Application/
│       ├── Validators/
│       │   └── LoginRequestValidator.cs
│       └── Mappings/
│           └── UserMappingConfig.cs
│
└── Tenancy/
    └── Application/
        ├── Validators/            ← (Gerekirse eklenebilir)
        └── Mappings/              ← (Gerekirse eklenebilir)
```

## 🎯 FluentValidation Kullanımı

### 1. Validator Oluşturma

```csharp
using FluentValidation;
using MultitenantPerDb.Modules.Products.Application.Commands;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz")
            .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır");
    }
}
```

### 2. Controller'da Kullanım

```csharp
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IValidator<CreateProductCommand> _validator;

    public ProductsController(IValidator<CreateProductCommand> validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        // Manuel validasyon
        var validationResult = await _validator.ValidateAsync(command);
        
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        // İş mantığı...
        return Ok();
    }
}
```

### 3. Otomatik Validasyon (Action Filter ile)

```csharp
// Startup/Program.cs'de:
builder.Services.AddControllers()
    .AddFluentValidation(fv => 
    {
        fv.RegisterValidatorsFromAssemblyContaining<Program>();
        fv.AutomaticValidationEnabled = true;
    });
```

## 🗺️ Mapster Kullanımı

### 1. Mapping Configuration

```csharp
using Mapster;
using MultitenantPerDb.Modules.Products.Domain.Entities;
using MultitenantPerDb.Modules.Products.Application.DTOs;

public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Entity -> DTO
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        // Command -> Entity (factory method kullanarak)
        config.NewConfig<CreateProductCommand, Product>()
            .MapWith(src => Product.Create(src.Name, src.Description, src.Price, src.Stock));
    }
}
```

### 2. Controller'da Kullanım

```csharp
using MapsterMapper;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        var product = await repository.GetByIdAsync(id);
        
        if (product == null)
            return NotFound();

        // Entity -> DTO mapping
        var dto = _mapper.Map<ProductDto>(product);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        // Validation otomatik çalışacak (FluentValidation ile)
        
        // Command -> Entity mapping
        var product = _mapper.Map<Product>(command);
        
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        await repository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<ProductDto>(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, dto);
    }
}
```

### 3. Service'de Kullanım

```csharp
public class ProductService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        var products = await repository.GetAllAsync();

        // Collection mapping
        return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductCommand command)
    {
        var product = _mapper.Map<Product>(command);
        
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        await repository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }
}
```

## 🔧 Module Registration

Her modülde otomatik olarak kaydedilir:

```csharp
// ProductsModule.cs
public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var assembly = Assembly.GetExecutingAssembly();
    
    // FluentValidation - Modüldeki tüm validator'ları kaydet
    services.AddValidatorsFromAssembly(assembly);
    
    // Mapster - Mapping konfigürasyonlarını kaydet
    var config = TypeAdapterConfig.GlobalSettings;
    config.Scan(assembly);
    services.AddSingleton(config);
    services.AddScoped<IMapper, ServiceMapper>();
}
```

## 🎨 Validation Rules Örnekleri

### String Validations
```csharp
RuleFor(x => x.Name)
    .NotEmpty().WithMessage("Boş olamaz")
    .MinimumLength(3).WithMessage("En az 3 karakter")
    .MaximumLength(200).WithMessage("En fazla 200 karakter")
    .Matches(@"^[a-zA-Z0-9\s]+$").WithMessage("Sadece alfanumerik karakterler");
```

### Number Validations
```csharp
RuleFor(x => x.Price)
    .GreaterThan(0).WithMessage("0'dan büyük olmalı")
    .LessThanOrEqualTo(1000000).WithMessage("1.000.000'dan küçük olmalı")
    .PrecisionScale(18, 2, false).WithMessage("2 ondalık basamak");

RuleFor(x => x.Stock)
    .GreaterThanOrEqualTo(0).WithMessage("Negatif olamaz")
    .LessThan(100000).WithMessage("100.000'den küçük olmalı");
```

### Email Validation
```csharp
RuleFor(x => x.Email)
    .NotEmpty().WithMessage("Email boş olamaz")
    .EmailAddress().WithMessage("Geçerli bir email adresi giriniz");
```

### Custom Validation
```csharp
RuleFor(x => x.Username)
    .Must(BeUniqueUsername).WithMessage("Kullanıcı adı zaten kullanılıyor");

private bool BeUniqueUsername(string username)
{
    // Database kontrolü
    return !_userRepository.AnyAsync(u => u.Username == username).Result;
}
```

## 📝 Best Practices

### ✅ DO:
- Her DTO/Command için ayrı validator oluştur
- Mapping configuration'ları modül bazında organize et
- Business rule validation'ları validator'lara ekle
- İyi hata mesajları yaz (Türkçe/İngilizce)

### ❌ DON'T:
- Controller'da manuel mapping yapma (Mapster kullan)
- Validation mantığını controller'a yazma
- Global validator'lar oluşturma (modül bazlı tut)
- Complex business logic'i validator'a yazma (domain'e taşı)

## 🚀 Sonraki Adımlar

1. **Action Filter oluştur** - Otomatik validation için
2. **Global exception handler** ekle - Validation hatalarını yakala
3. **Response DTO'lar** oluştur - Tutarlı API response'ları
4. **Async validators** ekle - Database kontrolü gibi async işlemler için

