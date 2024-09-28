using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using ECommerceAPI.ECommerceProductService.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.ECommerceProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ensure that the user is authenticated
    public class OrdersController : ControllerBase
    {
        private readonly ECommerceProductDbContext _context;

        public OrdersController(ECommerceProductDbContext context)
        {
            _context = context;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<IActionResult> GetUserOrders()
        {
            // Get the authenticated user's ID from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }

            // Convert user ID from claim to integer (assuming ID is an integer)
            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            // Fetch the user's orders from the database
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Select(o => new
                {
                    o.Id,
                    o.Total,
                    o.Date
                    // Add other properties if needed
                })
                .ToListAsync();

            if (!orders.Any())
            {
                return NotFound(new { message = "No orders found for this user" });
            }

            return Ok(orders);
        }
        [HttpGet("{id}")]
        public IActionResult GetOrderDetails(int id)
        {
            var order = _context.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            return Ok(new
            {
                order.Id,
                order.Total,
                order.Date,
                Items = order.Items.Select(i => new
                {
                    i.Id,
                    i.ProductName,
                    i.Price,
                    i.Quantity
                }).ToList()
            });
        }
    }
}
