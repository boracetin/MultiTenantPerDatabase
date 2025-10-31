using Microsoft.AspNetCore.Mvc;
using MultitenantPerDb.Models;
using MultitenantPerDb.Repositories;
using MultitenantPerDb.UnitOfWork;

namespace MultitenantPerDb.Controllers;

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
    /// Tüm ürünleri getirir (Tenant'a özgü)
    /// Header: X-Tenant-ID: 1
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        try
        {
            var repository = _unitOfWork.GetRepository<ProductRepository>();
            var products = await repository.GetAllAsync();
            return Ok(products);
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
            var repository = _unitOfWork.GetRepository<ProductRepository>();
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
            var repository = _unitOfWork.GetRepository<ProductRepository>();
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
            var repository = _unitOfWork.GetRepository<ProductRepository>();
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
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        try
        {
            var repository = _unitOfWork.GetRepository<ProductRepository>();
            product.CreatedAt = DateTime.UtcNow;
            await repository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
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
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        try
        {
            var repository = _unitOfWork.GetRepository<ProductRepository>();
            var existingProduct = await repository.GetByIdAsync(id);
            
            if (existingProduct == null)
            {
                return NotFound();
            }

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
            var repository = _unitOfWork.GetRepository<ProductRepository>();
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
    public async Task<ActionResult> CreateMultipleProducts([FromBody] List<Product> products)
    {
        try
        {
            var repository = _unitOfWork.GetRepository<ProductRepository>();
            
            foreach (var product in products)
            {
                product.CreatedAt = DateTime.UtcNow;
                await repository.AddAsync(product);
            }

            // Transaction otomatik olarak başlatılır, commit edilir veya rollback olur
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = $"{products.Count} ürün başarıyla eklendi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
