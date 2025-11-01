using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Products.Application.Commands;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Application.Services;
using MultitenantPerDb.Modules.Products.Domain.Entities;
using MultitenantPerDb.Modules.Products.Domain.Repositories;
using MultitenantPerDb.Modules.Products.Domain.Services;
using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Shared.Kernel.Infrastructure.Services;

namespace MultitenantPerDb.Modules.Products.Application.Handlers.Commands;

/// <summary>
/// EXAMPLE: Service Architecture Demonstration
/// This is an ALTERNATIVE handler showing the old approach
/// For comparison with the new generic architecture
/// </summary>
public class CreateProductWithServicesCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    // Domain Service - Business logic
    private readonly IPriceCalculationService _priceCalculationService;
    
    // Application Service - Domain-specific orchestration
    private readonly IProductNotificationService _productNotificationService;
    
    // Infrastructure Service - Generic HTTP client
    private readonly IHttpClientService _httpClientService;
    private readonly IConfiguration _configuration;

    public CreateProductWithServicesCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPriceCalculationService priceCalculationService,
        IProductNotificationService productNotificationService,
        IHttpClientService httpClientService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _priceCalculationService = priceCalculationService;
        _productNotificationService = productNotificationService;
        _httpClientService = httpClientService;
        _configuration = configuration;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Domain Service - Price calculation (pure business logic)
        var finalPrice = _priceCalculationService.CalculateFinalPrice(
            basePrice: request.Price,
            taxRate: 18m, // KDV
            discountPercentage: 10m
        );

        // 2. Create product using domain factory
        var product = Product.Create(
            request.Name,
            request.Description,
            finalPrice, // Calculated price
            request.Stock
        );

        // 3. Save to database
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        await repository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Infrastructure Service - Generic HTTP client for external API
        try
        {
            var inventoryApiUrl = _configuration["ExternalApis:InventoryApi"] 
                ?? "https://inventory-api.example.com";
            
            await _httpClientService.PostAsync<object, object>(
                $"{inventoryApiUrl}/api/inventory/sync",
                new { ProductId = product.Id, Stock = product.Stock }
            );
        }
        catch (Exception ex)
        {
            // External service failure doesn't break the flow
            Console.WriteLine($"Inventory sync failed: {ex.Message}");
        }

        // 5. Application Service - Domain-specific notification
        try
        {
            await _productNotificationService.SendProductCreatedNotificationAsync(
                product.Id,
                product.Name
            );
        }
        catch (Exception ex)
        {
            // Notification failure doesn't break the flow
            Console.WriteLine($"Product notification failed: {ex.Message}");
        }

        return _mapper.Map<ProductDto>(product);
    }
}

