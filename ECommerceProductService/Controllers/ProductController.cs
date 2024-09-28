using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.ECommerceProductService.Data;
using ECommerceAPI.ECommerceProductService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceAPI.ECommerceProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly ECommerceProductDbContext _context;

        public ProductController(ECommerceProductDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryParameters parameters)
        {
            var products = _context.Products.AsQueryable();

            // Global filter
            if (!string.IsNullOrEmpty(parameters.Query))
            {
                products = products.Where(p => p.Name.Contains(parameters.Query) || p.Description.Contains(parameters.Query) || p.Category.Contains(parameters.Query));
            }

            if (!string.IsNullOrEmpty(parameters.Category))
            {
                products = products.Where(p => p.Category.Contains(parameters.Category));
            }

            if (!string.IsNullOrEmpty(parameters.Description))
            {
                products = products.Where(p => p.Description.Contains(parameters.Description));
            }

            if (!string.IsNullOrEmpty(parameters.Name))
            {
                products = products.Where(p => p.Name.Contains(parameters.Name));
            }

            if (!string.IsNullOrEmpty(parameters.Price))
            {
                products = products.Where(p => p.Price.ToString().Contains(parameters.Price));
            }

            if (!string.IsNullOrEmpty(parameters.ProductId))
            {
                products = products.Where(p => p.ProductId.ToString().Contains(parameters.ProductId));
            }

            if (!string.IsNullOrEmpty(parameters.Stock))
            {
                products = products.Where(p => p.Stock.ToString().Contains(parameters.Stock));
            }

            if (parameters.MinPrice.HasValue)
            {
                products = products.Where(p => p.Price >= parameters.MinPrice.Value);
            }

            if (parameters.MaxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= parameters.MaxPrice.Value);
            }

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                switch (parameters.SortBy)
                {
                    case "price_asc":
                        products = products.OrderBy(p => p.Price);
                        break;
                    case "price_desc":
                        products = products.OrderByDescending(p => p.Price);
                        break;
                    case "name_asc":
                        products = products.OrderBy(p => p.Name);
                        break;
                    case "name_desc":
                        products = products.OrderByDescending(p => p.Name);
                        break;
                    default:
                        // Optional: Add default sorting or do nothing
                        break;
                }
            }

            // Fetch total count of products
            var totalRecords = await products.CountAsync();

            products = products.Skip((parameters.PageNumber - 1) * parameters.PageSize)
                                .Take(parameters.PageSize);
            
            var filteredProducts = await products.ToListAsync();

            return Ok(new
            {
                TotalRecords = totalRecords,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                Products = filteredProducts
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (product == null) return BadRequest();

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return Ok(product);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            if (updatedProduct == null || id != updatedProduct.ProductId)
                return BadRequest();

            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.Stock = updatedProduct.Stock;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return Ok(product);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string query)
        {
            var products = await _context.Products
                .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
                .ToListAsync();

            return Ok(products);
        }
        [HttpGet("filter")]
        public async Task<IActionResult> FilterProducts([FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
        {
            var products = _context.Products.AsQueryable();

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            return Ok(await products.ToListAsync());
        }
        [HttpGet("sort")]
        public async Task<IActionResult> SortProducts([FromQuery] string sortBy)
        {
            var products = _context.Products.AsQueryable();

            switch (sortBy)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "name_asc":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductId); // Default sorting
                    break;
            }

            return Ok(await products.ToListAsync());
        }

    }
}
