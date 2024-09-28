namespace ECommerceUserAPI.Models
{
    public class TokenBlacklist
    {
        public string TokenId { get; set; }
        public DateTime Expiration { get; set; }
    }
}
