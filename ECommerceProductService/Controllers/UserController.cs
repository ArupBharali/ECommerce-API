using ECommerceAPI.Data;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;


namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ECommerceDbContext _context;

        public UserController(ECommerceDbContext context)
        {
            _context = context;
        }

       

        // GET: api/user/orders
        [HttpGet("orders")]
        public IActionResult GetUserOrders()
        {
            // Simulate getting the authenticated user's ID
            var userId = 1; // Replace this with actual logic for authenticated user

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .Select(o => new
                {
                    o.Id,
                    o.Total,
                    o.Date
                })
                .ToList();

            if (!orders.Any())
            {
                return NotFound(new { message = "No orders found for this user" });
            }

            return Ok(orders);
        }
        [HttpPut("update")]
        public IActionResult UpdateUserDetails([FromBody] UserUpdateDto userUpdateDto)
        {
            var userId = 1; // Simulate getting the authenticated user's ID
            //var user = _context.ApplicationUsers.FirstOrDefault(u => u.Id == userId);
            var user = _context.ApplicationUsers.FirstOrDefault(u => 1 == userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.UserName = userUpdateDto.Name;
            user.Email = userUpdateDto.Email;

            _context.SaveChanges();

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email
            });
        }

        

    }
}
