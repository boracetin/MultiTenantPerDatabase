using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Modules.Products.Domain.Events;

namespace MultitenantPerDb.Modules.Products.Domain.Entities;

/// <summary>
/// Product aggregate root
/// </summary>
public class Product : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    // EF Core için parameterless constructor
    private Product() : base()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    // Factory method - Business logic ile oluşturma
    public static Product Create(string name, string description, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(stock));

        var product = new Product
        {
            Name = name,
            Description = description ?? string.Empty,
            Price = price,
            Stock = stock
        };

        // Domain event ekle
        product.AddDomainEvent(new ProductCreatedEvent(product));

        return product;
    }

    // Business methods
    public void UpdateDetails(string name, string description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        Name = name;
        Description = description ?? string.Empty;
        Price = price;
        SetUpdatedAt();

        AddDomainEvent(new ProductUpdatedEvent(this));
    }

    public void UpdateStock(int quantity)
    {
        if (Stock + quantity < 0)
            throw new InvalidOperationException("Insufficient stock");

        Stock += quantity;
        SetUpdatedAt();

        AddDomainEvent(new ProductStockUpdatedEvent(this, quantity));
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        UpdateStock(quantity);
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        UpdateStock(-quantity);
    }

    public bool IsInStock => Stock > 0;
    public bool IsLowStock(int threshold = 10) => Stock > 0 && Stock <= threshold;
}

