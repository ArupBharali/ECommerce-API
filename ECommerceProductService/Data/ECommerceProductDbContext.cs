using Microsoft.EntityFrameworkCore;
using ECommerceAPI.ECommerceProductService.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ECommerceAPI.ECommerceProductService.Data
{
    public class ECommerceProductDbContext : DbContext
    {
        public ECommerceProductDbContext(DbContextOptions<ECommerceProductDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the one-to-many relationship
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId);

            modelBuilder.Entity<Cart>()
           .HasMany(c => c.Items)
           .WithOne(i => i.Cart)
           .HasForeignKey(i => i.CartId);
        }
    }
}
