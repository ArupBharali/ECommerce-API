using ECommerceUserAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace ECommerceUserAPI.Services
{
    public class TokenBlacklistService
    {
        private readonly IDistributedCache _cache;

        public TokenBlacklistService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task LogoutAsync([FromBody] string tokenId)
        {
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(2)); // Token expiration time

            await _cache.SetStringAsync(tokenId, "blacklisted", options);
        }

        public async Task<bool> IsTokenBlacklistedAsync(string tokenId)
        {
            var result = await _cache.GetStringAsync(tokenId);
            return result != null; // If result is not null, the token is blacklisted
        }
    }
}
