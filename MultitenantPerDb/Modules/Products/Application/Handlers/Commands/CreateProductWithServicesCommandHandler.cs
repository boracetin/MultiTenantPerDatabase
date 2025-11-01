using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Products.Application.Commands;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Domain.Entities;
using MultitenantPerDb.Modules.Products.Domain.Repositories;
using MultitenantPerDb.Modules.Products.Domain.Services;
using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Shared.Kernel.Infrastructure.Services;

namespace MultitenantPerDb.Modules.Products.Application.Handlers.Commands;

/// <summary>
/// ÖRNEK: Service'lerin Handler'da kullanımı
/// Handler = Orchestrator (yönetici)
/// Services = Specific tasks (belirli işler)
/// </summary>
public class CreateProductWithServicesCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    // Domain Service - Business logic
    private readonly IPriceCalculationService _priceCalculationService;
    
    // Infrastructure Service - External systems
    private readonly IEmailService _emailService;
    private readonly IInventoryApiService _inventoryApiService;

    public CreateProductWithServicesCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPriceCalculationService priceCalculationService,
        IEmailService emailService,
        IInventoryApiService inventoryApiService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _priceCalculationService = priceCalculationService;
        _emailService = emailService;
        _inventoryApiService = inventoryApiService;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Domain Service kullanımı - Price calculation
        var finalPrice = _priceCalculationService.CalculateFinalPrice(
            basePrice: request.Price,
            taxRate: 18m, // KDV
            discountPercentage: 10m
        );

        // 2. Create product using domain factory
        var product = Product.Create(
            request.Name,
            request.Description,
            finalPrice, // Calculated price kullan
            request.Stock
        );

        // 3. Save to database
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        await repository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Infrastructure Service - External API sync
        try
        {
            await _inventoryApiService.SyncInventoryAsync(product.Id, product.Stock);
        }
        catch (Exception ex)
        {
            // External service failure doesn't break the flow
            // Log the error but continue
            Console.WriteLine($"Inventory sync failed: {ex.Message}");
        }

        // 5. Infrastructure Service - Send notification
        try
        {
            await _emailService.SendProductCreatedNotificationAsync(
                product.Id,
                product.Name
            );
        }
        catch (Exception ex)
        {
            // Email failure doesn't break the flow
            Console.WriteLine($"Email notification failed: {ex.Message}");
        }

        return _mapper.Map<ProductDto>(product);
    }
}
