using ECommerceProductAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.IdentityModel.Tokens.Jwt;

namespace ECommerceProductAPI
{
    public class CustomJwtBearerEvents : JwtBearerEvents
    {
        //private readonly IAuthService _authService;
        private readonly IDistributedCache _cache;
        public CustomJwtBearerEvents(
            IDistributedCache cache
            //, IAuthService authService
            )
        {
            _cache = cache;
            //_authService = authService;
            Console.WriteLine("CustomJwtBearerEvents instantiated");
        }
        public override async Task TokenValidated(TokenValidatedContext context)
        {
            Console.WriteLine("TokenValidated called");

            var options = new DistributedCacheEntryOptions()
    .SetAbsoluteExpiration(TimeSpan.FromHours(2)); // Token expiration time

            await _cache.SetStringAsync("sample-token", "testing", options);

            var tokenId = context.Principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            // Check Redis cache for token validity
            var cachedToken = await _cache.GetStringAsync(tokenId);
            if (cachedToken != null)
            {
                context.Fail("Token has been revoked."); // Token is blacklisted
            }

        }
    }
}
