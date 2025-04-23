using Newtonsoft.Json.Linq;
using System.Net.Http.Json;

namespace DDDAplication.Api.IntegrationTests.Helper
{
    public static class TokenHelper
    {
        public static async Task<string> GetJwtTokenAsync(HttpClient client)
        {
            var registerModel = new
            {
                Username = $"testuser{Guid.NewGuid()}",
                Email = $"testuser{Guid.NewGuid()}@example.com",
                Password = "Test@123"
            };

            var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerModel);
            registerResponse.EnsureSuccessStatusCode();

            var loginModel = new
            {
                Username = registerModel.Username,
                Password = registerModel.Password
            };

            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginModel);

            if (!loginResponse.IsSuccessStatusCode)
            {
                var content = await loginResponse.Content.ReadAsStringAsync();
                throw new Exception($"Login failed. Status: {loginResponse.StatusCode}, Content: {content}");
            }

            var loginData = await loginResponse.Content.ReadFromJsonAsync<Token>();
            var token = loginData?.token.ToString();

            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Token is null or empty");
            }

            return token;
        }
        public class Token
        {
            public string? token { get; set; }
        }
    }
}
