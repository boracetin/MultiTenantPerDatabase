using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Application.Products.Application.DTOs;
using MultitenantPerDb.Modules.Application.Products.Application.Services;
using MultitenantPerDb.Modules.Application.Products.Domain.Entities;
using MultitenantPerDb.Modules.Application.Products.Domain.Services;
using MultitenantPerDb.Shared.Kernel.Infrastructure.Services;

namespace MultitenantPerDb.Modules.Application.Products.Application.Features.Products.CreateProduct;

/// <summary>
/// Handler for CreateProductCommand
/// ARCHITECTURE DEMONSTRATION:
/// - Domain Service: Price calculations (business logic)
/// - Application Service: Product notifications (orchestration)
/// - Infrastructure Service: HTTP client for external APIs (generic)
/// Uses IProductService for business logic and Repository<Product> for data access
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;
    private readonly IPriceCalculationService _priceCalculation; // Domain Service
    private readonly IProductNotificationService _productNotification; // Application Service
    private readonly IHttpClientService _httpClient; // Infrastructure Service
    private readonly IConfiguration _configuration;

    public CreateProductCommandHandler(
        IProductService productService,
        IMapper mapper,
        IPriceCalculationService priceCalculation,
        IProductNotificationService productNotification,
        IHttpClientService httpClient,
        IConfiguration configuration)
    {
        _productService = productService;
        _mapper = mapper;
        _priceCalculation = priceCalculation;
        _productNotification = productNotification;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. DOMAIN SERVICE - Calculate final price with tax and bulk discount
        var bulkDiscountPercentage = _priceCalculation.GetBulkDiscountPercentage(request.Stock);
        var finalPrice = _priceCalculation.CalculateFinalPrice(
            basePrice: request.Price,
            taxRate: 18m, // KDV %18
            discountPercentage: bulkDiscountPercentage
        );

        // 2. Create product using ProductService (handles business logic and validation)
        var product = await _productService.CreateProductAsync(
            name: request.Name,
            description: request.Description,
            price: finalPrice, // Use calculated price
            stock: request.Stock,
            cancellationToken: cancellationToken
        );

        // 4. INFRASTRUCTURE SERVICE - Sync with external inventory system (fire-and-forget)
        // Generic HTTP client - no domain knowledge
        _ = Task.Run(async () =>
        {
            try
            {
                var inventoryApiUrl = _configuration["ExternalApis:InventoryApi"] 
                    ?? "https://inventory-api.example.com";
                
                var inventoryData = new
                {
                    ProductId = product.Id,
                    Stock = product.Stock,
                    Timestamp = DateTime.UtcNow
                };

                await _httpClient.PostAsync<object, object>(
                    $"{inventoryApiUrl}/api/inventory/sync",
                    inventoryData
                );
            }
            catch (Exception ex)
            {
                // External sync failure shouldn't break the flow
                Console.WriteLine($"Inventory sync failed: {ex.Message}");
            }
        }, cancellationToken);

        // 5. APPLICATION SERVICE - Send product created notification (fire-and-forget)
        // Domain-specific orchestration - uses generic email/SMS services internally
        _ = Task.Run(async () =>
        {
            try
            {
                await _productNotification.SendProductCreatedNotificationAsync(
                    product.Id,
                    product.Name
                );
            }
            catch (Exception ex)
            {
                // Notification failure shouldn't break the flow
                Console.WriteLine($"Product notification failed: {ex.Message}");
            }
        }, cancellationToken);

        // 6. Map to DTO and return
        return _mapper.Map<ProductDto>(product);
    }
}
