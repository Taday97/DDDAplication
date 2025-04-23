using DDDAplication.Api.IntegrationTests.Common;
using DDDAplication.Api.IntegrationTests.Helper;
using DDDAplication.API;
using DDDAplication.Application.DTOs;
using DDDAplication.Domain.Entities;
using DDDAplication.Infrastructure.Data;
using DDDAplication.Infrastructure.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace DDDAplication.Api.IntegrationTests
{
    [TestFixture]
    public class AuthControllerIntegrationTests
    {
        private HttpClient _client;
        private readonly ApiApplicationFactory<Program> _factory;

        public AuthControllerIntegrationTests()
        {
            _factory = new ApiApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }



        [OneTimeSetUp]
        [SetUp]
        public async Task SetUp()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();
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
            // Arrange
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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

            string tokenNew;
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                tokenNew = await userManager.GenerateEmailConfirmationTokenAsync(user);
            }

            // Act
             var encodedToken = WebUtility.UrlEncode(tokenNew);
             var encodedEmail = WebUtility.UrlEncode(userEmail);

             var response = await _client.GetAsync($"/api/auth/confirm?token={encodedToken}&email={encodedEmail}");


            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseBody = await response.Content.ReadAsStringAsync();
            responseBody.Should().NotBeNullOrEmpty();
            responseBody.Should().Contain("Email confirmed successfully.");
        }
        [Test]
        public async Task ConfirmEmail_Should_Return_Ok_When_Email_Already_Confirmed()
        {
            // Arrange
            var userEmail = $"confirmed_{Guid.NewGuid()}@example.com";
            var password = "Test@123";
            ApplicationUser user;

            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                user = new ApplicationUser
                {
                    UserName = $"confirmed_{Guid.NewGuid()}",
                    Email = userEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, password);
            }

            string tokenNew;
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                tokenNew = await userManager.GenerateEmailConfirmationTokenAsync(user);
            }

            var encodedToken = WebUtility.UrlEncode(tokenNew);
            var encodedEmail = WebUtility.UrlEncode(userEmail);

            // Act
            var response = await _client.GetAsync($"/api/auth/confirm?token={encodedToken}&email={encodedEmail}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Email already confirmed.");
        }
        [Test]
        public async Task ConfirmEmail_Should_Return_BadRequest_When_Token_Is_Invalid()
        {
            // Arrange
            var userEmail = $"invalid_token_{Guid.NewGuid()}@example.com";
            var password = "Test@123";
            ApplicationUser user;

            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                user = new ApplicationUser
                {
                    UserName = $"invalid_token_{Guid.NewGuid()}",
                    Email = userEmail,
                    EmailConfirmed = false
                };
                await userManager.CreateAsync(user, password);
            }

            var encodedToken = WebUtility.UrlEncode("invalid_token_123");
            var encodedEmail = WebUtility.UrlEncode(userEmail);

            // Act
            var response = await _client.GetAsync($"/api/auth/confirm?token={encodedToken}&email={encodedEmail}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Email confirmation failed.");
        }
        [Test]
        public async Task ConfirmEmail_Should_Return_BadRequest_When_User_Not_Found()
        {
            // Arrange
            var encodedToken = WebUtility.UrlEncode("some_valid_like_token_but_user_missing");
            var encodedEmail = WebUtility.UrlEncode("notfound@example.com");

            // Act
            var response = await _client.GetAsync($"/api/auth/confirm?token={encodedToken}&email={encodedEmail}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("User not found.");
        }
        [Test]
        public async Task ConfirmEmail_Should_Return_BadRequest_When_Token_Is_Missing()
        {
            var email = WebUtility.UrlEncode("test@example.com");
            var response = await _client.GetAsync($"/api/auth/confirm?token=&email={email}");

            var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblem.Should().NotBeNull();
            validationProblem.Errors.Should().ContainKey("token");
            validationProblem.Errors["token"].First().Should().Be("The token field is required.");

        }
        [Test]
        public async Task ChangePassword_Should_Return_Ok_When_Valid()
        {
            // Arrange
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var username = $"user_{Guid.NewGuid()}";
            var email = $"{username}@example.com";
            var password = "OldPass@123";
            var newPassword = "NewPass@123";

            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = new ApplicationUser { UserName = username, Email = email };
                await userManager.CreateAsync(user, password);
            }

            var model = new ChangePasswordModelDto
            {
                Username = username,
                OldPassword = password,
                NewPassword = newPassword
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/change-password", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("Password changed successfully");
        }

        [Test]
        public async Task ChangePassword_Should_Return_BadRequest_When_Model_Is_Invalid()
        {
            // Arrange
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var model = new ChangePasswordModelDto(); // Sin datos

            var response = await _client.PostAsJsonAsync("/api/auth/change-password", model);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problem.Should().NotBeNull();
            problem.Errors.Should().ContainKey("Username");
            problem.Errors.Should().ContainKey("OldPassword");
            problem.Errors.Should().ContainKey("NewPassword");
        }
        [Test]
        public async Task ChangePassword_Should_Return_BadRequest_When_User_Does_Not_Exist()
        {
            // Arrange
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var model = new ChangePasswordModelDto
            {
                Username = "nonexistentuser",
                OldPassword = "whatever",
                NewPassword = "NewPass@123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/change-password", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseBody = await response.Content.ReadAsStringAsync();
            responseBody.Should().Contain("User not found");
        }


        [Test]
        public async Task ResetPassword_Should_Return_Ok_When_Successful()
        {

            // Arrange
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var username = $"user_{Guid.NewGuid()}";
            var email = $"{username}@example.com";
            var password = "OldPass@123";
            var newPassword = "NewPass@123";
            var tokenNew = "";
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = new ApplicationUser { UserName = username, Email = email };
                await userManager.CreateAsync(user, password);
               
                 tokenNew = await userManager.GeneratePasswordResetTokenAsync(user);
            }
            var model = new ResetPasswordModelDto
            {
                Username = username,
                Token = tokenNew,
                NewPassword = newPassword
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/reset-password", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("Password has been reset successfully.");
        }


        [OneTimeTearDown]
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}

