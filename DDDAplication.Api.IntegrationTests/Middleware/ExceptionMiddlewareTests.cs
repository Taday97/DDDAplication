using DDDAplication.Api.IntegrationTests.Common;
using DDDAplication.Api.IntegrationTests.Helper;
using DDDAplication.API;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DDDAplication.Api.IntegrationTests.Middleware
{
    public class GlobalExceptionMiddlewareTests

    {
        private HttpClient _client;
        private readonly ApiApplicationFactory<Program> _factory;

        public GlobalExceptionMiddlewareTests()
        {
            _factory = new ApiApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [OneTimeSetUp]
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public async Task Middleware_Should_Handle_Generic_Exception_With_Token()
        {

            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var invalidUserId = Guid.NewGuid();
            var response = await _client.GetAsync($"/api/user/{invalidUserId}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails.Title, Is.EqualTo("An unexpected error occurred."));
            Assert.That(problemDetails.Status, Is.EqualTo((int)HttpStatusCode.InternalServerError));
        }

      

        public class TokenResponse
        {
            public string token { get; set; } = string.Empty;
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}
