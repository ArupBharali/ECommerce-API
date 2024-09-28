namespace ECommerceAPI.ECommerceProductService.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // Foreign key property
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }

}
