using DDDAplication.Api.IntegrationTests.Common;
using DDDAplication.API;
using DDDAplication.Application.DTOs.User;
using DDDAplication.Domain.Entities;
using DDDAplication.Infrastructure.Data;
using DDDAplication.Infrastructure.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NUnit.Framework.Internal;
using DDDAplication.Application.DTOs.ApiResponse;

namespace DDDAplication.Api.IntegrationTests
{
    [TestFixture] 
    public class UserControllerIntegrationTests
    {
        private HttpClient _client;
        private readonly ApiApplicationFactory<Program> _factory;
        private JwtService _jwtService;
        private string AdminId;
        private string AdminToken;
        private string TestId;
        private string TestToken;
        private string DeveloperId;

        public UserControllerIntegrationTests()
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

                var developerUser = await userManager.FindByNameAsync("Developer");
                DeveloperId = developerUser?.Id.ToString() ?? "";

                

                var adminUser = await userManager.FindByNameAsync("admin");
                AdminId = adminUser?.Id.ToString() ?? "";
                AdminToken = await _jwtService.GenerateTokenAsync(adminUser);

                var testUser = await userManager.FindByNameAsync("test");
                TestId = developerUser?.Id.ToString() ?? "";
                TestToken = await _jwtService.GenerateTokenAsync(testUser);
            }

        }
        [Test]
        public async Task GetMyProfile_Should_Return_Ok_When_UserExists()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            var response = await _client.GetAsync("/api/user/me");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var userProfile = await response.Content.ReadFromJsonAsync<UserDto>();
            userProfile.Should().NotBeNull();
            userProfile.Id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public async Task UpdateMyProfile_Should_Return_Ok_When_ValidData()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

            var updatedUser = new UserDto
            {
                Id = "test",
                UserName = "NewTestName",
                Email = "newTestName@gmail.com",
            };

            var response = await _client.PutAsJsonAsync("/api/user/me", updatedUser);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedProfile = await response.Content.ReadFromJsonAsync<UserDto>();
            updatedProfile.UserName.Should().Be("NewTestName");
        }
        [Test]
        public async Task UpdateMyProfile_Should_Return_BadRequest_When_InvalidData()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            var updatedUser = new UserDto
            {
                Id = "test",
                UserName = "Developer",//UserName exists
                Email = "newTestName",//Email invalid
            };

            var response = await _client.PutAsJsonAsync("/api/user/me", updatedUser);
            var content = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonConvert.DeserializeObject<ValidationProblemDetails>(content);

            problemDetails.Should().NotBeNull();
            problemDetails.Errors.Should().ContainKey("Email");
            problemDetails.Errors["Email"].Should().Contain("Invalid email format.");
            problemDetails.Errors.Should().ContainKey("UserName");
            problemDetails.Errors["UserName"].Should().Contain("A user with the name already exists.");
        }
        [Test]
        public async Task UpdateMyProfile_Should_Return_BadRequest_When_IdIsMissingInUserDto()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

            var updatedUser = new UserDto
            {
                UserName = "NewTestName",
                Email = "newTestName@gmail.com",
            };


            var response = await _client.PutAsJsonAsync("/api/user/me", updatedUser);

            var content = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonConvert.DeserializeObject<ValidationProblemDetails>(content);

            problemDetails.Should().NotBeNull();
            problemDetails.Errors.Should().ContainKey("Id");
            problemDetails.Errors["Id"].Should().Contain("The Id field is required.");
        }

        [Test]
        public async Task GetAllUsers_Should_Return_Ok_For_Admin()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            var response = await _client.GetAsync("/api/user");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            users.Should().NotBeNullOrEmpty();
        }
        [Test]
        public async Task GetUserById_Should_Return_Ok_When_UserExists()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            var userId = DeveloperId;

            var response = await _client.GetAsync($"/api/user/{userId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            user.Should().NotBeNull();
        }

        [Test]
        public async Task AssignRolesToUser_Should_Return_Ok_When_RolesAssigned()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            var userId = DeveloperId;
            var roles = new List<string> { "Admin" };

            var response = await _client.PostAsJsonAsync($"/api/user/{userId}/roles", roles);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Test]
        public async Task AssignRolesToUser_Should_Return_NotFound_When_UserDoesNotExist()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            var userId = "sdfwg4534tfdvdf";
            var roles = new List<string> { "Admin" };

            var response = await _client.PostAsJsonAsync($"/api/user/{userId}/roles", roles);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("User not found.");
        }
        [Test]
        public async Task RemoveRolesFromUser_Should_Return_Ok_When_RolesRemoved()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            var userId = DeveloperId;
            var roles = new List<string> { "Admin" };

            var response = await _client.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"/api/user/{userId}/roles", UriKind.Relative),
                Content = JsonContent.Create(roles)
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        [Test]
        public async Task RemoveRolesFromUser_Should_Return_NotFound_When_UserDoesNotExist()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            var userId = "sdsdgdfg";
            var roles = new List<string> { "Admin" };

            var response = await _client.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"/api/user/{userId}/roles", UriKind.Relative),
                Content = JsonContent.Create(roles)
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("User not found.");
        }


        [Test]
        public async Task GetMyProfile_Should_Return_Unauthorized_When_UserIdIsNull()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "");

            // Act
            var response = await _client.GetAsync("/api/user/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task UpdateMyProfile_Should_Return_Unauthorized_When_TokenIsNotValid()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "user-not-exist");
            var updatedUser = new UserDto
            {
                Id = "errthdferhrthsdfgeh654641",
                UserName = "NewTestName",
                Email = "newTestName@gmail.com",
            };

            var response = await _client.PutAsJsonAsync("/api/user/me", updatedUser);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetAllUsers_Should_Return_Forbidden_When_UserIsNotAdmin()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

            // Act
            var response = await _client.GetAsync("/api/user");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetUserById_Should_Return_BadRequest_When_UserIdIsNull()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
            var valueNull = "null"; 
            // Act
            var response = await _client.GetAsync($"/api/user/{valueNull}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain($"User with id {valueNull} not found.");
        }


        [Test]
        public async Task UpdateUser_Should_Return_ValidationProblem_When_RequestBodyIsNull()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            // Arrange
            UserDto updatedUser = null;

            // Act
            var response = await _client.PutAsJsonAsync("/api/user/me", updatedUser);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonConvert.DeserializeObject<ValidationProblemDetails>(content);

            problemDetails.Should().NotBeNull();
            problemDetails.Errors.Should().ContainKey("dto");
            problemDetails.Errors["dto"].Should().Contain("The dto field is required.");
        }


        [Test]
        public async Task DeleteRole_Should_Return_NotFound_When_RoleIdIsNull()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            // Act
            var response = await _client.DeleteAsync("/api/user/null");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetUserRoles_Should_Return_BadRequest_When_UserIdIsNotFound()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            // Act
            var response = await _client.GetAsync($"/api/user/{"sdf"}/roles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("User not found.");
        }
        [Test]
        public async Task GetUserRoles_Should_Return_Ok_When_UserExists()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            // Act
            var response = await _client.GetAsync($"/api/user/{AdminId}/roles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var rolesResponse = JsonConvert.DeserializeObject<ApiResponse<List<string>>>(content);

            rolesResponse.Should().NotBeNull();
            rolesResponse.Success.Should().BeTrue();
            rolesResponse.Message.Should().Be("Roles retrieved successfully.");
            rolesResponse.Errors.Should().BeEmpty();
            rolesResponse.Data.Should().NotBeNullOrEmpty();
            rolesResponse.Data.Should().Contain("Admin");
        }


        [Test]
        public async Task AssignRolesToUser_Should_Return_BadRequest_When_RolesListIsEmpty()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
            var roles = new List<string>();

            // Act
            var response = await _client.PostAsJsonAsync($"/api/user/{DeveloperId}/roles", roles);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Users list cannot be empty.");
        }

        [Test]
        public async Task AssignRolesToUser_Should_Return_BadRequest_When_RoleDoesNotExist()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
            var roles = new List<string>  { "Role-Not-Exist" };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/user/{DeveloperId}/roles", roles);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("One or more roles not found.");

        }

        [Test]
        public async Task RemoveRolesFromUser_Should_Return_BadRequest_When_RolesListIsEmpty()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
            var roles = new List<string>();

            // Act
            var response = await _client.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"/api/user/{TestId}/roles", UriKind.Relative),
                Content = JsonContent.Create(roles)
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Users list cannot be empty.");
        }

        [OneTimeTearDown] 
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose(); 
        }
    }
}