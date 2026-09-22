using System.Text.Json;

namespace MtlsBlazorClient.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> GetTokenAsync()
        {
            var tokenEndpoint = _config["Jwt:TokenEndpoint"];
            var clientId = _config["Jwt:ClientId"];
            var clientSecret = _config["Jwt:ClientSecret"];
            var scope = _config["Jwt:Scope"];

            var http = new HttpClient();

            var response = await http.PostAsync(tokenEndpoint,
                            new FormUrlEncodedContent(new Dictionary<string, string>
                            {
                                ["grant_type"] = "client_credentials",
                                ["client_id"] = clientId,
                                ["client_secret"] = clientSecret,
                                ["scope"] = scope
                            }));

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("access_token").GetString();
        }
    }
}
