namespace MultitenantPerDb.Modules.Products.Domain.Constants;

/// <summary>
/// Constants specific to Products module
/// </summary>
public static class ProductsConstants
{
    public static class ProductStatus
    {
        public const string Active = "Active";
        public const string Inactive = "Inactive";
        public const string OutOfStock = "OutOfStock";
        public const string Discontinued = "Discontinued";
    }

    public static class ErrorMessages
    {
        public const string ProductNotFound = "Product not found";
        public const string InvalidPrice = "Price must be greater than zero";
        public const string InvalidStock = "Stock quantity cannot be negative";
        public const string DuplicateProductCode = "Product code already exists";
    }

    public static class Validation
    {
        public const int ProductNameMaxLength = 200;
        public const int ProductNameMinLength = 3;
        public const int ProductCodeMaxLength = 50;
        public const int DescriptionMaxLength = 1000;
        public const decimal MinPrice = 0.01m;
        public const decimal MaxPrice = 999999.99m;
        public const int MinStock = 0;
        public const int MaxStock = 999999;
    }

    public static class DefaultValues
    {
        public const decimal DefaultPrice = 0.00m;
        public const int DefaultStock = 0;
        public const string DefaultStatus = ProductStatus.Active;
    }
}
