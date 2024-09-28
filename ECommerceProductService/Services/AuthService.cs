namespace ECommerceProductAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> IsTokenBlacklisted(string tokenId)
        {
            var response = await _httpClient.GetAsync($"{_configuration.GetValue<string>("AuthService:AuthServiceURL")}/api/auth/IsTokenValid/{tokenId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            // Handle error response (e.g., log, throw exception)
            return false;
        }
    }
}
