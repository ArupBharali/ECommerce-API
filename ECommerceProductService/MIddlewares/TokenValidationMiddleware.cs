using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace ECommerceProductAPI.Middlewares
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TokenValidationMiddleware(RequestDelegate next, HttpClient httpClient, IConfiguration configuration)
        {
            _next = next;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request has an authorization header
            if (context.Request.Headers.TryGetValue("Authorization", out var tokenHeader))
            {
                var token = tokenHeader.ToString().Replace("Bearer ", string.Empty);
                var tokenId = GetTokenIdFromJwt(token); // Extract jti or unique identifier from token

                // Call the authentication service to check if the token is blacklisted
                var isBlacklisted = await CheckIfTokenIsBlacklisted(tokenId);
                if (isBlacklisted)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token has been revoked.");
                    return;
                }
            }

            await _next(context); // Call the next middleware in the pipeline
        }

        private string GetTokenIdFromJwt(string token)
        {
            // Decode the JWT token and extract the jti claim
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(token);
            return jwtToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        }

        private async Task<bool> CheckIfTokenIsBlacklisted(string tokenId)
        {
            var response = await _httpClient.GetAsync($"{_configuration.GetValue<string>("AuthService:AuthServiceURL")}/{tokenId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            // Handle error response appropriately
            return false;
        }
    }
}
