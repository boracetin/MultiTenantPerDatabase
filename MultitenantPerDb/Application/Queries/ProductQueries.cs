namespace MultitenantPerDb.Application.Queries;

public record GetAllProductsQuery
{
}

public record GetProductByIdQuery
{
    public int Id { get; init; }
}

public record GetInStockProductsQuery
{
}

public record GetProductsByPriceRangeQuery
{
    public decimal MinPrice { get; init; }
    public decimal MaxPrice { get; init; }
}
