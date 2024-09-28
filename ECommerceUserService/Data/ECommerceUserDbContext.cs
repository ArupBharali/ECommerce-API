using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ECommerceAPI.ECommerceUserAPI.Models;

namespace ECommerceAPI.ECommerceUserAPI.Data
{
    public class ECommerceUserDbContext : IdentityDbContext<ApplicationUser>
    {
        public ECommerceUserDbContext(DbContextOptions<ECommerceUserDbContext> options) : base(options) { }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
