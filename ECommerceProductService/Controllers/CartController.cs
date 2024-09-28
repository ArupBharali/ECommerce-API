using ECommerceAPI.ECommerceProductService.Data;
using ECommerceAPI.ECommerceProductService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.ECommerceProductService.Controllers
{
    // Controllers/CartController.cs
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ECommerceProductDbContext _context;

        public CartController(ECommerceProductDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public IActionResult GetCart(int userId)
        {
            var cart = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound(new { message = "Cart not found" });
            }

            return Ok(cart);
        }

        [HttpPost("{userId}")]
        public IActionResult AddToCart(int userId, [FromBody] CartItem cartItem)
        {
            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                _context.Carts.Add(cart);
            }

            cart.Items.Add(cartItem);
            _context.SaveChanges();

            return Ok(cart);
        }

        [HttpDelete("{userId}/item/{itemId}")]
        public IActionResult RemoveFromCart(int userId, int itemId)
        {
            var cart = _context.Carts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                return NotFound(new { message = "Cart not found" });
            }

            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                return NotFound(new { message = "Item not found" });
            }

            cart.Items.Remove(item);
            _context.SaveChanges();

            return Ok(cart);
        }
    }

}
