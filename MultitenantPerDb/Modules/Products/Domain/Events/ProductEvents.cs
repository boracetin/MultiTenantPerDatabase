using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Modules.Products.Domain.Entities;

namespace MultitenantPerDb.Modules.Products.Domain.Events;

public class ProductCreatedEvent : IDomainEvent
{
    public Product Product { get; }
    public DateTime OccurredOn { get; }

    public ProductCreatedEvent(Product product)
    {
        Product = product;
        OccurredOn = DateTime.UtcNow;
    }
}

public class ProductUpdatedEvent : IDomainEvent
{
    public Product Product { get; }
    public DateTime OccurredOn { get; }

    public ProductUpdatedEvent(Product product)
    {
        Product = product;
        OccurredOn = DateTime.UtcNow;
    }
}

public class ProductStockUpdatedEvent : IDomainEvent
{
    public Product Product { get; }
    public int QuantityChanged { get; }
    public DateTime OccurredOn { get; }

    public ProductStockUpdatedEvent(Product product, int quantityChanged)
    {
        Product = product;
        QuantityChanged = quantityChanged;
        OccurredOn = DateTime.UtcNow;
    }
}

