using ECommerceAPI.ECommerceUserAPI.Data;
using ECommerceAPI.ECommerceUserAPI.Data;
using ECommerceAPI.ECommerceUserAPI.Models;
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
        private readonly ECommerceUserDbContext _context;

        public UserController(ECommerceUserDbContext context)
        {
            _context = context;
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
