namespace ECommerceAPI.ECommerceProductService.Models
{
    public class FilterParameters
    {
        public string? Query { get; set; } = null;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortOrder { get; set; } = null;
        public string? SortBy { get; set; } = null;
    }
}
