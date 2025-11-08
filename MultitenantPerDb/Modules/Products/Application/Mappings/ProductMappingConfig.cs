using Mapster;
using MultitenantPerDb.Modules.Products.Domain.Entities;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Application.Features.CreateProduct;
using MultitenantPerDb.Modules.Products.Application.Features.UpdateProduct;

namespace MultitenantPerDb.Modules.Products.Application.Mappings;

/// <summary>
/// Mapster configuration for Product mappings
/// Feature-Based Architecture
/// </summary>
public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Product -> ProductDto
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.Stock, src => src.Stock)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

        // CreateProductCommand -> Product (using factory method)
        config.NewConfig<CreateProductCommand, Product>()
            .MapWith(src => Product.Create(src.Name, src.Description, src.Price, src.Stock));

        // UpdateProductCommand -> Product (will be used with existing entity)
        // Note: For updates, we'll call methods on the entity rather than mapping
    }
}
