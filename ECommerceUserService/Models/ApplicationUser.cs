using Microsoft.AspNetCore.Identity;

namespace ECommerceAPI.ECommerceUserAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add custom properties if needed
        // For example:
        public string? FullName { get; set; } // Mark as nullable
        public DateTime DateOfBirth { get; set; }
    }

}
