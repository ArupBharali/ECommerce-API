namespace ECommerceAPI.ECommerceProductService.Models
{
    public class ProductQueryParameters : FilterParameters
    {
        public string? Category { get; set; } = null;
        public string? Description { get; set; } = null;
        public string? Name { get; set; } = null;
        public string? Price { get; set; } = null;
        public string? ProductId { get; set; } = null;
        public string? Stock { get; set; } = null;
        public decimal? MinPrice { get; set; } = null;
        public decimal? MaxPrice { get; set; } = null;
    }

}
