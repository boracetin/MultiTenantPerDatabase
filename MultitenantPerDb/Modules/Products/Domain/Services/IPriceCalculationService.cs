namespace MultitenantPerDb.Modules.Products.Domain.Services;

/// <summary>
/// Domain Service - Price calculation logic that involves multiple entities
/// Domain service'ler pure business logic içerir, infrastructure dependency'si olmaz
/// </summary>
public interface IPriceCalculationService
{
    /// <summary>
    /// Calculates final price with tax and discounts
    /// </summary>
    decimal CalculateFinalPrice(decimal basePrice, decimal taxRate, decimal discountPercentage);

    /// <summary>
    /// Checks if bulk discount should be applied
    /// </summary>
    bool ShouldApplyBulkDiscount(int quantity, decimal totalAmount);
    
    /// <summary>
    /// Calculate bulk discount percentage based on quantity
    /// </summary>
    decimal GetBulkDiscountPercentage(int quantity);
}

public class PriceCalculationService : IPriceCalculationService
{
    public decimal CalculateFinalPrice(decimal basePrice, decimal taxRate, decimal discountPercentage)
    {
        if (basePrice <= 0)
            throw new ArgumentException("Base price must be greater than zero", nameof(basePrice));
            
        if (taxRate < 0 || taxRate > 100)
            throw new ArgumentException("Tax rate must be between 0 and 100", nameof(taxRate));
            
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Discount percentage must be between 0 and 100", nameof(discountPercentage));

        var discountAmount = basePrice * (discountPercentage / 100);
        var priceAfterDiscount = basePrice - discountAmount;
        var taxAmount = priceAfterDiscount * (taxRate / 100);
        return Math.Round(priceAfterDiscount + taxAmount, 2);
    }

    public bool ShouldApplyBulkDiscount(int quantity, decimal totalAmount)
    {
        // Business rule: Bulk discount if quantity > 10 or total > 1000
        return quantity > 10 || totalAmount > 1000;
    }
    
    public decimal GetBulkDiscountPercentage(int quantity)
    {
        // Business rules for bulk discounts
        return quantity switch
        {
            >= 100 => 20m,  // 100+ items = 20% discount
            >= 50 => 15m,   // 50-99 items = 15% discount
            >= 20 => 10m,   // 20-49 items = 10% discount
            >= 10 => 5m,    // 10-19 items = 5% discount
            _ => 0m         // Less than 10 = no discount
        };
    }
}
