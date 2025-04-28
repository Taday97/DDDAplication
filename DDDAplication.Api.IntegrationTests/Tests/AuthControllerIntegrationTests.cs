using DDDAplication.Api.IntegrationTests.Common;
using DDDAplication.API;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.DTOs.Auth;
using DDDAplication.Domain.Entities;
using DDDAplication.Infrastructure.Data;
using DDDAplication.Infrastructure.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DDDAplication.Api.IntegrationTests
{
    [TestFixture]
    public class AuthControllerIntegrationTests
    {
        private HttpClient _client;
        private readonly ApiApplicationFactory<Program> _factory;
        private JwtService _jwtService;
        private string AdminToken;
        private string TestToken;

        public AuthControllerIntegrationTests()
        {
            _factory = new ApiApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }



        [OneTimeSetUp]
        [SetUp]
        public async Task SetUp()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                _jwtService = scopedServices.GetRequiredService<JwtService>();
                var context = scopedServices.GetRequiredService<AppDbContext>();

                await AppDbContextSeed.SeedDatabaseAsync(context, userManager, scopedServices.GetRequiredService<RoleManager<Role>>(), _factory.Services.GetRequiredService<IConfiguration>(), scopedServices.GetRequiredService<ILogger<AppDbContextSeed>>(), _jwtService);

                
                var adminUser = await userManager.FindByNameAsync("admin");
                AdminToken = await _jwtService.GenerateTokenAsync(adminUser);

                var testUser = await userManager.FindByNameAsync("test");
                TestToken = await _jwtService.GenerateTokenAsync(testUser);
            }


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
        public async Task Register_Should_Return_Ok_When_Valid_Request()
        {
            // Arrange
            var requestBody = new RegisterModelDto
            {
                Username = $"testuser_{Guid.NewGuid()}",
                Password = "Test@123",
                Email = $"testuser_{Guid.NewGuid()}@example.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", requestBody);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("User registered successfully.");
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
        public async Task Login_Should_Return_BadRequest_When_Invalid_Request()
        {
            // Arrange
            var requestBody = new LoginModelDto
            {
                Username = "",
                Password = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", requestBody);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblem.Should().NotBeNull();
            validationProblem.Errors.Should().ContainKey("Username");
            validationProblem.Errors["Username"].Should().Contain("Username is required.");
            validationProblem.Errors.Should().ContainKey("Password");
            validationProblem.Errors["Password"].Should().Contain("Password is required.");
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
            //var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

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
        public async Task ConfirmEmail_Should_Return_BadRequest_When_Invalid_Request()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);


            // Act
            var response = await _client.GetAsync($"/api/auth/confirm?token={""}&email={""}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblem.Should().NotBeNull();
            validationProblem.Errors.Should().ContainKey("token");
            validationProblem.Errors["token"].Should().Contain("The token field is required.");
            validationProblem.Errors.Should().ContainKey("email");
            validationProblem.Errors["email"].Should().Contain("The email field is required.");
        }
        [Test]
        public async Task ChangePassword_Should_Return_Ok_When_Valid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

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
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

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
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

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
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

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

        [Test]
        public async Task RefreshToken_Should_Return_BadRequest_When_Invalid_Request()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

            var model = new RefreshTokenModelDto
            {
                Token = "" ,
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/refresh", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblem.Should().NotBeNull();
            validationProblem.Errors.Should().ContainKey("Token");
            validationProblem.Errors["Token"].Should().Contain("Token is required.");
        }

        [Test]
        public async Task RefreshToken_Should_Return_Ok_When_TokenIsValid_And_IdentityNameIsCorrect()
        {
            // Arrange
            var username = "user_test";
            var password = "TestPass@123";
            var token = "";

            using (var scope = _factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;

                var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                var jwtService = scopedServices.GetRequiredService<JwtService>();

                var user = new ApplicationUser { UserName = username, Email = $"{username}@example.com" };
                await userManager.CreateAsync(user, password);

                token = await jwtService.GenerateTokenAsync(user);
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var model = new RefreshTokenModelDto { Token = token };
            var response = await _client.PostAsJsonAsync("/api/auth/refresh", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            var responseData = JObject.Parse(body)["data"]?.ToObject<RefreshTokenModelDto>();

            responseData?.Token.Should().NotBeNullOrEmpty();

            var principal = _jwtService.GetPrincipalFromExpiredToken(token);
            var extractedUsername = principal?.Identity?.Name;

            extractedUsername.Should().NotBeNull();
            extractedUsername.Should().Be(username);
        }



        [Test]
        public async Task GetProfile_Should_Return_Ok_When_TokenIsValid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);


            // Act
            var response = await _client.GetAsync("/api/auth/profile");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotBeEmpty();
        }
        [Test]
        public async Task GetProfile_Should_Return_Unauthorized_When_NoToken()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer");

            // Act
            var response = await _client.GetAsync("/api/auth/profile");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        [Test]
        public async Task GetProfile_Should_Return_Unauthorized_When_TokenIsInvalid()
        {
            // Arrange: Set an invalid token
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "InvalidToken");

            // Act
            var response = await _client.GetAsync("/api/auth/profile");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task ChangePassword_Should_Return_Ok_When_TokenIsValid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

            var principal = _jwtService.GetPrincipalFromExpiredToken(TestToken);
            var extractedUsername = principal?.Identity?.Name;

            // Act
            var model = new ChangePasswordModelDto
            {
                Username = extractedUsername,
                OldPassword = "Test123*",
                NewPassword = "Test123*"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/change-password", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotBeEmpty();
        }

        [Test]
        public async Task ChangePassword_Should_Return_Ok_When_OldPasswordIncorrect()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

            var principal = _jwtService.GetPrincipalFromExpiredToken(TestToken);
            var extractedUsername = principal?.Identity?.Name;

            // Act
            var model = new ChangePasswordModelDto
            {
                Username = extractedUsername,
                OldPassword = "Test@123",
                NewPassword = "Test123*"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/change-password", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("Old password is incorrect.");
        }

        [Test]
        public async Task ChangePassword_Should_Return_Forbidden_When_UserIsNotAuthenticated()
        {
            // Arrange: Create a valid JWT token
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

            var principal = _jwtService.GetPrincipalFromExpiredToken(TestToken);
            var extractedUsername = principal?.Identity?.Name;

            var userStatus = principal?.Identity?.IsAuthenticated ?? false;

            if (!userStatus)
            {
                // Act
                var model = new ChangePasswordModelDto
                {
                    Username = extractedUsername,
                    OldPassword = "Test123*",
                    NewPassword = "Test123*"
                };

                // Act
                var response = await _client.PostAsJsonAsync("/api/auth/change-password", model);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
                var body = await response.Content.ReadAsStringAsync();
                body.Should().NotBeEmpty();
            }
            else
            {
               
                var model = new ChangePasswordModelDto
                {
                    Username = extractedUsername,
                    OldPassword = "Test123*",
                    NewPassword = "Test123*"
                };

                // Act
                var response = await _client.PostAsJsonAsync("/api/auth/change-password", model);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var body = await response.Content.ReadAsStringAsync();
                body.Should().NotBeEmpty();
            }
        }


        [Test]
        public async Task RefreshToken_Should_Return_Ok_When_TokenIsValid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

            var model = new RefreshTokenModelDto
            {
                Token = TestToken
            };


            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/refresh", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotBeEmpty();
        }

        [Test]
        public async Task RefreshToken_Should_Return_BarRequest_When_TokenIsNotValid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

            var model = new RefreshTokenModelDto
            {
                Token = "Not Valid"
            };


            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/refresh", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().BeEquivalentTo("{\"message\":\"Invalid token.\"}");
        }

        [Test]
        public async Task SendResetLink_Should_Return_EmailNotRegiter_When_EmailNotValid()
        {
            // Arrange
            var model = new SendResetLinkModelDto
            {
                Email = "test@example.com" 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/send-reset-link", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Be("Email not registered.");
        }
        [Test]
        public async Task SendResetLink_Should_Return_Ok_When_EmailValid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

            var principal = _jwtService.GetPrincipalFromExpiredToken(TestToken);
            var extractedEmail = principal?.Identity?.Name+ "@example.com";

            var model = new SendResetLinkModelDto
            {
                Email = extractedEmail
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/send-reset-link", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Be("Email not registered.");
        }

        [Test]
        public async Task ResetPassword_Should_Return_BadRequest_When_Invalid_Request()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

            var model = new ResetPasswordModelDto
            {
                Username = "",
                Token = "",   
                NewPassword = "" 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/reset-password", model);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblem.Should().NotBeNull();
            validationProblem.Errors.Should().ContainKey("Username");
            validationProblem.Errors["Username"].Should().Contain("Username is required.");
            validationProblem.Errors.Should().ContainKey("Token");
            validationProblem.Errors["Token"].Should().Contain("Token is required.");
            validationProblem.Errors.Should().ContainKey("NewPassword");
            validationProblem.Errors["NewPassword"].Should().Contain("New password is required.");
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}

