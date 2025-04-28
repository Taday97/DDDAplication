using AutoMapper;
using DDDAplication.Api.IntegrationTests.Helper;
using DDDAplication.API;
using DDDAplication.API.Controllers;
using DDDAplication.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using DDDAplication.Api.IntegrationTests.Common;
using DDDAplication.Domain.Entities;
using DDDAplication.Infrastructure.Data;
using DDDAplication.Infrastructure.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DDDAplication.Application.DTOs.Rol;

namespace DDDAplication.Api.IntegrationTests
{
    [TestFixture]
    public class RoleControllerIntegrationTests
    {
        private HttpClient _client;
        private readonly ApiApplicationFactory<Program> _factory;
        private JwtService _jwtService;
        private string AdminToken;
        private string TestToken;
        private string ExistingRoleId;
        public string RoleDeveloperId ; 

        public RoleControllerIntegrationTests()
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
                ExistingRoleId= context.Roles.FirstOrDefault(r => r.Name == "Admin")?.Id.ToString() ??"";
                RoleDeveloperId = context.Roles.FirstOrDefault(r => r.Name == "Developer")?.Id.ToString() ??"";

                var adminUser = await userManager.FindByNameAsync("admin");
                AdminToken = await _jwtService.GenerateTokenAsync(adminUser);

                var testUser = await userManager.FindByNameAsync("test");
                TestToken = await _jwtService.GenerateTokenAsync(testUser);
            }


        }
        [Test]
        public async Task CreateRole_Should_Return_Forbidden_When_UserIsNotAdmin()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

            var roleDto = new RoleDto
            {
                Name = "NewRole"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/role", roleDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        [Test]
        public async Task CreateRole_Should_Return_Created_When_RoleDataIsValid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
            var roleDto = new CreateRoleDto { Name = "NewRole" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/role", roleDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
        [Test]
        public async Task CreateRole_Should_Return_BadRequest_When_RoleDataIsNull()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
            CreateRoleDto createRoleDto = null;
            // Act
            var response = await _client.PostAsJsonAsync("/api/role", createRoleDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblem.Should().NotBeNull();
            validationProblem.Errors.Should().ContainKey("createRoleDto");
            validationProblem.Errors["createRoleDto"].Should().Contain("The createRoleDto field is required.");
        }

        [Test]
        public async Task CreateRole_Should_Return_BadRequest_When_Name_Is_Empty()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            var invalidRole = new CreateRoleDto
            {
                Name = "" 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/role", invalidRole);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblem.Errors.Should().ContainKey("Name");
            validationProblem.Errors["Name"].First().Should().Be("The Name field is required.");
        }
        [Test]
        public async Task CreateRole_Should_Return_BadRequest_When_Name_AlreadyExists()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            var existingRoleName = "Admin"; 

            var createRoleDto = new CreateRoleDto
            {
                Name = existingRoleName
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/role", createRoleDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblem.Should().NotBeNull();
            validationProblem.Errors.Should().ContainKey("Name");
            validationProblem.Errors["Name"].First().Should().Contain("already exists");
        }

        [Test]
        public async Task UpdateRole_Should_Return_BadRequest_When_Id_Is_Invalid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            var invalidEditRole = new EditRoleDto
            {
                Id = "not-a-guid",
                Name = "ValidName"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/role/not-a-guid", invalidEditRole);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblem.Errors.Should().ContainKey("id");
            validationProblem.Errors["id"].First().Should().Be("Role ID must be a valid GUID.");
        }

        [Test]
        public async Task UpdateRole_Should_Return_BadRequest_When_Name_AlreadyExists()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            var existingRoleName = "Admin"; 
            var roleIdToUpdate = RoleDeveloperId;

            var editRoleDto = new EditRoleDto
            {
                Id = roleIdToUpdate,
                Name = existingRoleName
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/role/{roleIdToUpdate}", editRoleDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblem.Should().NotBeNull();
            validationProblem.Errors.Should().ContainKey("Name");
            validationProblem.Errors["Name"].First().Should().Contain("already exists");
        }

        [Test]
        public async Task UpdateRole_Should_Return_BadRequest_When_RoleIdMismatch()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
            var editRoleDto = new EditRoleDto { Id = "wrong-id", Name = "UpdatedRole" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/role/{ExistingRoleId}", editRoleDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task UpdateRole_Should_Return_Ok_When_RoleDataIsValid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
            var editRoleDto = new EditRoleDto { Id = RoleDeveloperId, Name = "UpdatedRole" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/role/{RoleDeveloperId}", editRoleDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        [Test]
        public async Task GetAllRoles_Should_Return_Forbidden_When_UserIsNotAdmin()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestToken);

            // Act
            var response = await _client.GetAsync("/api/role");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetAllRoles_Should_Return_Ok_When_UserIsAdmin()
        {
            // Arrange

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            // Act
            var response = await _client.GetAsync("/api/role");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        [Test]
        public async Task GetRoleById_Should_Return_BadRequest_When_RoleIdIsNull()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            // Act
            var response = await _client.GetAsync($"/api/role/null");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetRoleById_Should_Return_NotFound_When_RoleDoesNotExist()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            // Act
            var response = await _client.GetAsync("/api/role/nonexistent-id");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetRoleById_Should_Return_Ok_When_RoleExists()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
            var existingRoleId = ExistingRoleId; 

            // Act
            var response = await _client.GetAsync($"/api/role/{existingRoleId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task DeleteRole_Should_Return_NotFound_When_RoleIdIsNull()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            // Act
            var response = await _client.DeleteAsync("/api/role/null");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task DeleteRole_Should_Return_NotFound_When_RoleDoesNotExist()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

            // Act
            var response = await _client.DeleteAsync("/api/role/nonexistent-role-id");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task DeleteRole_Should_Return_Ok_When_RoleExists()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
            var existingRoleId = RoleDeveloperId; 

            // Act
            var response = await _client.DeleteAsync($"/api/role/{existingRoleId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [OneTimeTearDown] 
        public void Cleanup()
        {
            _client.Dispose(); 
            _factory.Dispose(); 
        }
    }
}