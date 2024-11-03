using DDDAplication.Application.DTOs;
using DDDAplication.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DDDAplication.Api.IntegrationTests
{
    [TestFixture]
    public class AuthControllerTests
    {
        private HttpClient _client;
        private readonly WebApplicationFactory<Startup> _factory;

        public AuthControllerTests()
        {
            _factory = new WebApplicationFactory<Startup>();
            _client = _factory.CreateClient();
        }

        [OneTimeSetUp]
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public async Task Login_Should_Return_Token_On_Valid_Credentials()
        {
            // Register user first to ensure login works
            var newUser = new RegisterModelDto
            {
                Username = $"testuser_{Guid.NewGuid()}",
                Password = "ValidPassword123!",
                Email = "testuser@example.com"
            };
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", newUser);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK, because: "User registered successfully");

            // Set up login credentials
            var loginUser = new LoginModelDto
            {
                Username = newUser.Username,
                Password = newUser.Password,
            };

            // Send login request
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginUser);

            // Verify response
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, because: "login should succeed with correct credentials");
            var responseBody = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            responseBody.Should().ContainKey("token", because: "a token should be returned for a successful login");
        }

        [Test]
        public async Task Register_Should_Return_Ok_When_Valid_Request()
        {
            var uniqueUserName = $"testuser_{Guid.NewGuid()}";
            var requestBody = new RegisterModelDto
            {
                Username = uniqueUserName,
                Password = "Test@123",
                Email = $"{uniqueUserName}@example.com"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", requestBody);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
            responseBody.Should().NotBeNull();
            var message = responseBody["message"].GetString();
            var success = responseBody["success"].GetBoolean();
            message.Should().Be("User registered successfully.");
            success.Should().BeTrue();
        }

        [Test]
        public async Task Register_Should_Return_BadRequest_When_User_Already_Exists()
        {
            var existingUserName = $"testuser_{Guid.NewGuid()}";
            var initialRequestBody = new RegisterModelDto
            {
                Username = existingUserName,
                Password = "Test@123",
                Email = $"{existingUserName}@example.com"
            };

            var initialResponse = await _client.PostAsJsonAsync("/api/auth/register", initialRequestBody);
            initialResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var duplicateRequestBody = new RegisterModelDto
            {
                Username = existingUserName,
                Password = "AnotherPassword@123",
                Email = $"{existingUserName}@example.com"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateRequestBody);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseBody = await response.Content.ReadFromJsonAsync<List<Dictionary<string, string>>>();
            responseBody.Should().NotBeNull();
            responseBody.Should().HaveCount(1);
            var dic = responseBody[0];
            dic["code"].Should().Be("DuplicateUserName");
            dic["description"].Should().Contain($"Username '{existingUserName}' is already taken.");
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
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Test]
        public async Task Register_Should_Return_BadRequest_When_Username_Taken()
        {
            var initialRequestBody = new RegisterModelDto
            {
                Username = "testuser",
                Password = "Test@123",
                Email = "testuser@example.com"
            };
            await _client.PostAsJsonAsync("/api/auth/register", initialRequestBody);

            var duplicateRequestBody = new RegisterModelDto
            {
                Username = "testuser",
                Password = "AnotherPassword123!",
                Email = "anotheruser@example.com"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateRequestBody);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Test]
        public async Task Login_Should_Return_Ok_When_Valid_Credentials()
        {
            var existingUserName = $"testuser_{Guid.NewGuid()}";
            var requestBody = new RegisterModelDto
            {
                Username = existingUserName,
                Password = "Test@123",
                Email = $"{existingUserName}@example.com"
            };
            await _client.PostAsJsonAsync("/api/auth/register", requestBody);

            var loginRequestBody = new LoginModelDto
            {
                Username = existingUserName,
                Password = "Test@123"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequestBody);

            var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            responseBody.Should().NotBeNull();
            responseBody.Should().ContainKey("token");
            responseBody["token"].Should().NotBeNull();
        }

        [Test]
        public async Task Login_Should_Return_Unauthorized_When_Invalid_Credentials()
        {
            var requestBody = new LoginModelDto
            {
                Username = "nonexistentuser",
                Password = "WrongPassword"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", requestBody);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            responseBody.Should().NotBeNull();
            responseBody.Should().ContainKey("message");
            responseBody["message"].Should().Be("Invalid credentials.");
        }

        [Test]
        public async Task ConfirmEmail_Should_Return_Ok_When_Valid_Token()
        {
            var existingUserName = $"testuser_{Guid.NewGuid()}";
            var userEmail = $"{existingUserName}@example.com";
            var password = "Test@123";
            ApplicationUser user;

            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                user = new ApplicationUser
                {
                    UserName = existingUserName,
                    Email = userEmail,
                    EmailConfirmed = false
                };
                await userManager.CreateAsync(user, password);
            }

            string token;
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            }

            var confirmEmailRequestBody = new ConfirmEmailModelDto
            {
                UserId = user.Id.ToString(),
                Token = token
            };

            var response = await _client.PostAsJsonAsync("/api/auth/confirm-email", confirmEmailRequestBody);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            responseBody.Should().NotBeNull();
            responseBody["message"].Should().Be("Email confirmed successfully.");
        }

        [Test]
        public async Task ConfirmEmail_Should_Return_NotFound_When_User_Not_Exists()
        {
            var invalidUserId = Guid.NewGuid().ToString();
            var confirmEmailRequest = new ConfirmEmailModelDto
            {
                UserId = invalidUserId,
                Token = "someToken"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/confirm-email", confirmEmailRequest);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            responseBody.Should().NotBeNull();
            responseBody["message"].Should().Be("User not found.");
        }

        [Test]
        public async Task ResetPassword_Should_Return_Ok_When_Valid_Token()
        {
            var existingUserName = $"testuser_{Guid.NewGuid()}";
            var userEmail = $"{existingUserName}@example.com";
            var password = "Test@123";
            ApplicationUser user;
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                user = new ApplicationUser { UserName = existingUserName, Email = userEmail };
                await userManager.CreateAsync(user, password);
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetPasswordRequestBody = new ResetPasswordModelDto
                {
                    Username = existingUserName,
                    Token = token,
                    NewPassword = "NewTest@123"
                };

                var response = await _client.PostAsJsonAsync("/api/auth/reset-password", resetPasswordRequestBody);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                responseBody["message"].Should().Be("Password has been reset successfully.");
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}