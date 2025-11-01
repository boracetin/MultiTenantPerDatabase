using Microsoft.AspNetCore.Mvc;
using MultitenantPerDb.Modules.Products.Domain.Entities;
using MultitenantPerDb.Modules.Products.Domain.Repositories;
using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Modules.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Products.API;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Tüm ürünleri listeler
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        try
        {
            var repository = _unitOfWork.GetRepository<IProductRepository>();
            var products = await repository.GetAllAsync();
            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                IsInStock = p.IsInStock,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });
            return Ok(productDtos);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// ID'ye göre ürün getirir
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        try
        {
            var repository = _unitOfWork.GetRepository<IProductRepository>();
            var product = await repository.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Stokta olan ürünleri getirir
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpGet("in-stock")]
    public async Task<ActionResult<IEnumerable<Product>>> GetInStockProducts()
    {
        try
        {
            var repository = _unitOfWork.GetRepository<IProductRepository>();
            var products = await repository.GetInStockProductsAsync();
            return Ok(products);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Fiyat aralığına göre ürünleri getirir
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpGet("price-range")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductsByPriceRange([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
    {
        try
        {
            var repository = _unitOfWork.GetRepository<IProductRepository>();
            var products = await repository.GetProductsByPriceRangeAsync(minPrice, maxPrice);
            return Ok(products);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Yeni ürün ekler
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto request)
    {
        try
        {
            var repository = _unitOfWork.GetRepository<IProductRepository>();
            
            // DDD factory method kullan
            var product = Product.Create(request.Name, request.Description, request.Price, request.Stock);
            
            await repository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                IsInStock = product.IsInStock,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Ürün günceller
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto request)
    {
        try
        {
            var repository = _unitOfWork.GetRepository<IProductRepository>();
            var product = await repository.GetByIdAsync(id);
            
            if (product == null)
            {
                return NotFound();
            }

            // DDD business method kullan
            product.UpdateDetails(request.Name, request.Description, request.Price);
            
            repository.Update(product);
            await _unitOfWork.SaveChangesAsync();
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Ürün siler
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var repository = _unitOfWork.GetRepository<IProductRepository>();
            var product = await repository.GetByIdAsync(id);
            
            if (product == null)
            {
                return NotFound();
            }

            repository.Remove(product);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Toplu ürün ekleme - Transaction otomatik yönetilir
    /// Authorization: Bearer {token}
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult> CreateMultipleProducts([FromBody] List<CreateProductDto> requests)
    {
        try
        {
            var repository = _unitOfWork.GetRepository<IProductRepository>();
            
            foreach (var request in requests)
            {
                // DDD factory method kullan
                var product = Product.Create(request.Name, request.Description, request.Price, request.Stock);
                await repository.AddAsync(product);
            }

            // Transaction otomatik olarak başlatılır, commit edilir veya rollback olur
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = $"{requests.Count} ürün başarıyla eklendi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

