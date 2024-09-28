namespace ECommerceProductAPI.Services
{
    public interface IAuthService
    {
        Task<bool> IsTokenBlacklisted(string tokenId);
    }
}
