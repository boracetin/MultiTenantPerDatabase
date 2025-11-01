# Advanced Repository Pattern - Usage Guide

## 📋 Overview

Enhanced generic repository with **DTO projection** support using Mapster. This enables efficient database queries by selecting only required fields.

## 🎯 Key Features

### 1. **DTO Projection with Mapster**
- ✅ Database-level SELECT optimization
- ✅ Only required fields are queried
- ✅ Automatic mapping using Mapster
- ✅ Better performance for read operations

### 2. **CancellationToken Support**
- ✅ All async methods support cancellation
- ✅ Proper async/await patterns
- ✅ Better for long-running queries

### 3. **Flexible Querying**
- ✅ AsNoTracking option for read-only queries
- ✅ Pagination support with `PagedResult<T>`
- ✅ Complex queries with includes (eager loading)
- ✅ Raw SQL support when needed

### 4. **Advanced Operations**
- ✅ Soft delete support (if entity has IsDeleted property)
- ✅ Bulk operations (AddRange, UpdateRange, RemoveRange)
- ✅ Count and existence checks
- ✅ Single vs FirstOrDefault (strict vs lenient)

## 📖 Usage Examples

### Basic Entity Queries

#### Get by ID (Full Entity)
```csharp
// Get full entity - all properties loaded
var product = await _productRepository.GetByIdAsync(1);
```

#### Get All Entities
```csharp
// Get all with no tracking (read-only)
var products = await _productRepository.GetAllAsync(asNoTracking: true);

// Get all with tracking (for updates)
var products = await _productRepository.GetAllAsync(asNoTracking: false);
```

#### Find with Predicate
```csharp
// Find products with stock > 10
var products = await _productRepository.FindAsync(p => p.Stock > 10);

// Find with tracking (for updates)
var products = await _productRepository.FindAsync(
    predicate: p => p.Price > 100m,
    asNoTracking: false
);
```

### DTO Projection (Efficient Queries)

#### Get by ID with DTO Projection
```csharp
// ✅ EFFICIENT - Only DTO fields in SELECT
var productDto = await _productRepository.GetByIdAsync<ProductDto>(1);

// Generated SQL (example):
// SELECT p.Id, p.Name, p.Price FROM Products WHERE Id = 1
// (Only DTO properties are selected!)
```

**ProductDto Example:**
```csharp
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    // Only these 3 fields will be selected from database!
}
```

#### Get All with DTO Projection
```csharp
// ✅ EFFICIENT - Only DTO fields in SELECT
var productDtos = await _productRepository.GetAllAsync<ProductListDto>();

// Generated SQL:
// SELECT p.Id, p.Name, p.Price, p.Stock FROM Products
```

#### Find with DTO Projection
```csharp
// ✅ EFFICIENT - Predicate + Projection
var expensiveProducts = await _productRepository.FindAsync<ProductDto>(
    predicate: p => p.Price > 1000m
);

// Generated SQL:
// SELECT p.Id, p.Name, p.Price FROM Products WHERE Price > 1000
```

#### FirstOrDefault with DTO Projection
```csharp
// Get first matching product as DTO
var product = await _productRepository.FirstOrDefaultAsync<ProductDto>(
    predicate: p => p.Name.Contains("iPhone")
);
```

### Pagination with DTO Projection

```csharp
// Get page 2 with 20 items per page
var pagedResult = await _productRepository.GetPagedAsync<ProductListDto>(
    pageNumber: 2,
    pageSize: 20,
    predicate: p => p.Stock > 0, // Optional filter
    orderBy: p => p.CreatedAt,   // Optional ordering
    ascending: false             // Descending order
);

// Access results
var products = pagedResult.Items;
var totalCount = pagedResult.TotalCount;
var totalPages = pagedResult.TotalPages;
var hasNextPage = pagedResult.HasNextPage;
var hasPreviousPage = pagedResult.HasPreviousPage;
```

**PagedResult Properties:**
```csharp
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }
}
```

### Command Operations

#### Add Entity
```csharp
var product = Product.Create("iPhone 15", "Latest iPhone", 999m, 50);
await _productRepository.AddAsync(product, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

#### Add Multiple Entities
```csharp
var products = new List<Product>
{
    Product.Create("Product 1", "Desc 1", 100m, 10),
    Product.Create("Product 2", "Desc 2", 200m, 20)
};

await _productRepository.AddRangeAsync(products, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

#### Update Entity
```csharp
var product = await _productRepository.GetByIdAsync(1);
product.UpdatePrice(1099m);

_productRepository.Update(product);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

#### Update Multiple Entities
```csharp
var products = await _productRepository.FindAsync(p => p.Stock < 10);
foreach (var product in products)
{
    product.UpdateStock(product.Stock + 100);
}

_productRepository.UpdateRange(products);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

#### Remove Entity
```csharp
var product = await _productRepository.GetByIdAsync(1);
_productRepository.Remove(product);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

#### Soft Delete
```csharp
// If entity has IsDeleted property, it will be marked as deleted
// Otherwise, it falls back to hard delete
var product = await _productRepository.GetByIdAsync(1);
_productRepository.SoftDelete(product);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

### Advanced Queries

#### Check Existence
```csharp
var exists = await _productRepository.AnyAsync(p => p.Name == "iPhone 15");

if (exists)
{
    // Product with this name already exists
}
```

#### Count Entities
```csharp
// Count all products
var totalProducts = await _productRepository.CountAsync();

// Count with predicate
var lowStockCount = await _productRepository.CountAsync(p => p.Stock < 10);
```

#### Single (Strict)
```csharp
// Throws exception if 0 or multiple results found
var product = await _productRepository.SingleAsync(p => p.Id == 1);
```

#### Get with Includes (Eager Loading)
```csharp
// Load product with related entities
var products = await _productRepository.GetWithIncludesAsync(
    predicate: p => p.CategoryId == 1,
    includes: new Expression<Func<Product, object>>[]
    {
        p => p.Category,
        p => p.Reviews,
        p => p.Images
    }
);
```

#### Complex Queries (GetQueryable)
```csharp
// For complex queries not covered by repository methods
var query = _productRepository.GetQueryable(asNoTracking: true);

var result = await query
    .Where(p => p.Stock > 0)
    .Where(p => p.Price > 100m)
    .OrderByDescending(p => p.CreatedAt)
    .Take(10)
    .ProjectToType<ProductDto>() // Mapster projection
    .ToListAsync();
```

#### Raw SQL Query
```csharp
var products = await _productRepository.FromSqlRawAsync(
    "SELECT * FROM Products WHERE Price > {0} AND Stock > {1}",
    1000m,
    10
);
```

## 🎭 Handler Examples

### Query Handler with DTO Projection

```csharp
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        
        // ✅ EFFICIENT - Only ProductDto fields selected from database
        var product = await repository.GetByIdAsync<ProductDto>(request.Id, cancellationToken);
        
        if (product == null)
            throw new NotFoundException($"Product with ID {request.Id} not found");
        
        return product;
    }
}
```

### Query Handler with Pagination

```csharp
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductListDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ProductListDto>> Handle(
        GetProductsQuery request, 
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        
        // ✅ EFFICIENT - Paginated query with DTO projection
        return await repository.GetPagedAsync<ProductListDto>(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            predicate: p => p.Stock > 0, // Optional filter
            orderBy: p => p.CreatedAt,
            ascending: false,
            cancellationToken: cancellationToken
        );
    }
}
```

### Command Handler

```csharp
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(
        CreateProductCommand request, 
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        
        // Create entity
        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.Stock
        );
        
        // Add to repository
        await repository.AddAsync(product, cancellationToken);
        
        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Return DTO (or use GetByIdAsync<ProductDto> for efficiency)
        return _mapper.Map<ProductDto>(product);
    }
}
```

## 🚀 Performance Benefits

### Without DTO Projection (❌ Inefficient)
```csharp
// Get full entity with all properties
var product = await _productRepository.GetByIdAsync(1);
var dto = _mapper.Map<ProductDto>(product);

// Generated SQL:
// SELECT * FROM Products WHERE Id = 1
// (All columns selected even if only 3 are needed!)
```

### With DTO Projection (✅ Efficient)
```csharp
// Get only required fields
var dto = await _productRepository.GetByIdAsync<ProductDto>(1);

// Generated SQL:
// SELECT p.Id, p.Name, p.Price FROM Products WHERE Id = 1
// (Only 3 columns selected!)
```

**Benefits:**
- ✅ Reduced network traffic (less data transferred)
- ✅ Faster query execution (less data to read)
- ✅ Better memory usage (smaller objects)
- ✅ Automatic mapping (no manual mapping code)

## 📊 Comparison Table

| Scenario | Without DTO Projection | With DTO Projection |
|----------|----------------------|-------------------|
| **Database Query** | SELECT * (all columns) | SELECT Id, Name, Price (only needed) |
| **Network Traffic** | ❌ High (all data) | ✅ Low (only needed data) |
| **Memory Usage** | ❌ High (full entities) | ✅ Low (DTOs only) |
| **Mapping** | Manual (_mapper.Map) | ✅ Automatic (Mapster.ProjectToType) |
| **Performance** | ❌ Slower | ✅ Faster |
| **Use Case** | Updates, deletes | ✅ **Read operations** |

## 🎯 Best Practices

### 1. Use DTO Projection for Read Operations
```csharp
// ✅ GOOD - For queries (read-only)
var dto = await repository.GetByIdAsync<ProductDto>(1);

// ❌ BAD - For updates (need full entity)
var entity = await repository.GetByIdAsync<ProductDto>(1); // Can't update DTO!
```

### 2. Use Full Entity for Updates
```csharp
// ✅ GOOD - Get full entity for updates
var product = await repository.GetByIdAsync(1);
product.UpdatePrice(newPrice);
repository.Update(product);
```

### 3. Use AsNoTracking for Read-Only Queries
```csharp
// ✅ GOOD - No tracking for read-only
var products = await repository.GetAllAsync(asNoTracking: true);

// ❌ BAD - Tracking when not needed (slower)
var products = await repository.GetAllAsync(asNoTracking: false);
```

### 4. Use Pagination for Large Datasets
```csharp
// ✅ GOOD - Paginated results
var pagedResult = await repository.GetPagedAsync<ProductDto>(1, 20);

// ❌ BAD - Load all records (memory issues)
var allProducts = await repository.GetAllAsync<ProductDto>();
```

### 5. Use CancellationToken
```csharp
// ✅ GOOD - Support cancellation
public async Task<ProductDto> Handle(Query request, CancellationToken cancellationToken)
{
    return await repository.GetByIdAsync<ProductDto>(1, cancellationToken);
}
```

## 🔧 Configuration

### Mapster Setup (Already Configured)
```csharp
// In ProductsModule.cs or Program.cs
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(assembly); // Auto-discover mappings
services.AddSingleton(config);
```

### Example Mapster Mapping Configuration
```csharp
public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.CategoryName, src => src.Category.Name)
            .Map(dest => dest.IsLowStock, src => src.Stock < 10);
    }
}
```

## 📝 Summary

| Feature | Description | Benefit |
|---------|-------------|---------|
| **DTO Projection** | `GetByIdAsync<TDto>()` | 🚀 Database-level SELECT optimization |
| **Pagination** | `GetPagedAsync<TDto>()` | 📄 Efficient large dataset handling |
| **CancellationToken** | All async methods | ⏱️ Proper async/await patterns |
| **AsNoTracking** | Read-only queries | ⚡ Better performance |
| **Soft Delete** | `SoftDelete()` | 🗑️ Logical delete support |
| **Eager Loading** | `GetWithIncludesAsync()` | 🔗 N+1 query prevention |
| **Flexible Querying** | `GetQueryable()` | 🎯 Complex custom queries |

---

**Status:** ✅ Repository Enhanced
**Version:** 2.0
**Date:** November 2024
**Impact:** Breaking changes - method signatures updated with CancellationToken
**Migration:** Update all repository calls to include CancellationToken parameter
