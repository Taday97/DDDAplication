using DDDAplication.Api.IntegrationTests.Common;
using DDDAplication.Api.IntegrationTests.Helper;
using DDDAplication.Application.DTOs;
using DDDAplication.API;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace DDDAplication.Api.IntegrationTests.Validation
{
    [TestFixture]
    public class UserControllerValidationTests
    {
        private HttpClient _client;
        private readonly ApiApplicationFactory<Program> _factory;

        public UserControllerValidationTests()
        {
            _factory = new ApiApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [Test]
        public async Task UpdateUser_Should_Return_BadRequest_When_Model_Is_Invalid()
        {
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var userId = Guid.NewGuid().ToString();

            var invalidDto = new UserDto
            {
                Id = userId,
                UserName = "",
                Email = "invalidemail"
            };

            var response = await _client.PutAsJsonAsync($"/api/user/{userId}", invalidDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseBody = await ResponseHelper.GetValidationErrors(response);

            responseBody.Errors.Should().ContainKey("UserName").WhoseValue.Should().Contain("UserName is required.");
            responseBody.Errors.Should().ContainKey("Email").WhoseValue.Should().Contain("Invalid email format.");
        }

        [Test]
        public async Task UpdateUser_Should_Return_BadRequest_When_Ids_Do_Not_Match()
        {
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var dto = new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "ValidName",
                Email = "valid@email.com"
            };

            var differentId = Guid.NewGuid().ToString();
            var response = await _client.PutAsJsonAsync($"/api/user/{differentId}", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

            body.Should().ContainKey("message").WhoseValue.Should().Contain("The ID in the URL does not match the ID in the request body.");
        }

        [Test]
        public async Task UpdateUser_Should_Return_BadRequest_When_Body_Is_Null()
        {
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var userId = Guid.NewGuid().ToString();

            UserDto invalidDto = null;

            var response = await _client.PutAsJsonAsync($"/api/user/{userId}", invalidDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseBody = await ResponseHelper.GetValidationErrors(response);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseBody.Errors.Should().ContainKey("userUpdateDto").WhoseValue.Should().Contain("The userUpdateDto field is required.");
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}
