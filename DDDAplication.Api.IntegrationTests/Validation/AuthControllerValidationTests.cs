using DDDAplication.Api.IntegrationTests.Common;
using DDDAplication.Api.IntegrationTests.Helper;
using DDDAplication.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DDDAplication.API;
using FluentAssertions;

namespace DDDAplication.Api.IntegrationTests.Validation
{
    [TestFixture]
    public class AuthControllerValidationTests
    {
        private HttpClient _client;
        private readonly ApiApplicationFactory<Program> _factory;

        public AuthControllerValidationTests()
        {
            _factory = new ApiApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }


        [Test]
        public async Task Register_Should_Return_BadRequest_When_Invalid_Request()
        {
            var requestBody = new RegisterModelDto
            {
                Username = "",
                Password = "short",
                Email = "invalidemail"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", requestBody);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseBody = await ResponseHelper.GetValidationErrors(response);

            responseBody.Errors.Should().ContainKey("Email").WhoseValue.Should().Contain("Invalid email format.");
            responseBody.Errors.Should().ContainKey("Password");
            responseBody.Errors["Password"].Should().Contain("Password must contain at least one uppercase letter.");
            responseBody.Errors["Password"].Should().Contain("Password must contain at least one number.");
            responseBody.Errors["Password"].Should().Contain("Password must contain at least one special character.");
            responseBody.Errors.Should().ContainKey("Username").WhoseValue.Should().Contain("Username is required.");
        }

        [Test]
        public async Task Login_Should_Return_BadRequest_When_Model_Invalid()
        {
            var requestBody = new LoginModelDto
            {
                Username = "",
                Password = ""
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", requestBody);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseBody = await ResponseHelper.GetValidationErrors(response);

            responseBody.Errors.Should().ContainKey("Username").WhoseValue.Should().Contain("Username is required.");
            responseBody.Errors.Should().ContainKey("Password").WhoseValue.Should().Contain("Password is required.");
        }

        [Test]
        public async Task SendResetLink_Should_Return_BadRequest_When_Email_Invalid()
        {
            var requestBody = new SendResetLinkModelDto
            {
                Email = "notanemail"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/send-reset-link", requestBody);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseBody = await ResponseHelper.GetValidationErrors(response);

            responseBody.Errors.Should().ContainKey("Email").WhoseValue.Should().Contain("Invalid email format.");
        }

        [Test]
        public async Task ResetPassword_Should_Return_BadRequest_When_Model_Invalid()
        {
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestBody = new ResetPasswordModelDto
            {
                Username = "",
                Token = "",
                NewPassword = "short"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/reset-password", requestBody);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseBody = await ResponseHelper.GetValidationErrors(response);

            responseBody.Errors.Should().ContainKey("Username").WhoseValue.Should().Contain("Username is required.");
            responseBody.Errors.Should().ContainKey("Token").WhoseValue.Should().Contain("Token is required.");
            responseBody.Errors.Should().ContainKey("NewPassword");
            responseBody.Errors["NewPassword"].Should().Contain("Password must contain at least one uppercase letter.");
        }

        [Test]
        public async Task ConfirmEmail_Should_Return_BadRequest_When_Model_Invalid()
        {
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestBody = new ConfirmEmailModelDto
            {
                UserId = "",
                Token = ""
            };

            var response = await _client.PostAsJsonAsync("/api/auth/confirm-email", requestBody);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseBody = await ResponseHelper.GetValidationErrors(response);

            responseBody.Errors.Should().ContainKey("UserId").WhoseValue.Should().Contain("UserId is required.");
            responseBody.Errors.Should().ContainKey("Token").WhoseValue.Should().Contain("Token is required.");
        }
        [Test]
        public async Task Register_Should_Return_Ok_When_Request_Is_Valid()
        {
            var requestBody = new RegisterModelDto
            {
                Username = "validuser",
                Password = "Valid@123",
                Email = "valid@example.com"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", requestBody);

            response.StatusCode.Should().Be(HttpStatusCode.OK); // O Created según implementación
        }


        [OneTimeTearDown]
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}
