using Microsoft.AspNetCore.Mvc;
using MediatR;
using MultitenantPerDb.Modules.Application.Products.Application.Features.Products.CreateProduct;
using MultitenantPerDb.Modules.Application.Products.Application.Features.Products.UpdateProduct;
using MultitenantPerDb.Modules.Application.Products.Application.Features.Products.DeleteProduct;
using MultitenantPerDb.Modules.Application.Products.Application.Features.Products.GetProductById;
using MultitenantPerDb.Modules.Application.Products.Application.Features.Products.GetProducts;
using MultitenantPerDb.Modules.Application.Products.Application.Features.Products.GetInStockProducts;
using MultitenantPerDb.Modules.Application.Products.Application.Features.Products.GetProductsByPriceRange;
using MultitenantPerDb.Modules.Application.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Application.Products.API;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Tüm ürünleri listeler
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var query = new GetProductsQuery();
        var products = await _mediator.Send(query);
        return Ok(products);
    }

    /// <summary>
    /// ID'ye göre ürün getirir
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var query = new GetProductByIdQuery(id);
        var product = await _mediator.Send(query);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    /// <summary>
    /// Stokta olan ürünleri getirir
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpGet("in-stock")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetInStockProducts()
    {
        var query = new GetInStockProductsQuery();
        var products = await _mediator.Send(query);
        return Ok(products);
    }

    /// <summary>
    /// Fiyat aralığına göre ürünleri getirir
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpGet("price-range")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByPriceRange([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
    {
        var query = new GetProductsByPriceRangeQuery(minPrice, maxPrice);
        var products = await _mediator.Send(query);
        return Ok(products);
    }

    /// <summary>
    /// Yeni ürün ekler
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductCommand command)
    {
        var product = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// Ürün günceller
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        var product = await _mediator.Send(command);
        return Ok(product);
    }

    /// <summary>
    /// Ürün siler
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var command = new DeleteProductCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Toplu ürün ekleme - Transaction otomatik yönetilir
    /// Authorization: Bearer {token}
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult> CreateMultipleProducts([FromBody] List<CreateProductCommand> commands)
    {
        var results = new List<ProductDto>();
        
        foreach (var command in commands)
        {
            var product = await _mediator.Send(command);
            results.Add(product);
        }

        return Ok(new { message = $"{results.Count} ürün başarıyla eklendi", products = results });
    }
}

