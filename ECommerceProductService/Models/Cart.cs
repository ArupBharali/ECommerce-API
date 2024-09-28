namespace ECommerceAPI.ECommerceProductService.Models
{
    // Models/Cart.cs
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ICollection<CartItem> Items { get; set; }
    }
}
