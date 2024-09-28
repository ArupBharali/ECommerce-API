namespace ECommerceAPI.ECommerceProductService.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Total { get; set; }
        public DateTime Date { get; set; }
        // Additional fields like OrderItems, ShippingAddress, etc.

        public ICollection<Item> Items { get; set; } = new List<Item>(); // Initialize to avoid null reference issues

    }

}
