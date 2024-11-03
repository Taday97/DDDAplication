using AutoMapper;
using DDDAplication.API.Controllers;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;

namespace DDDAplication.Api.IntegrationTests
{
    [TestFixture]
    public class RoleControllerTest
    {
        private HttpClient _client;
        private readonly WebApplicationFactory<Startup> _factory;

        public RoleControllerTest()
        {
            _factory = new WebApplicationFactory<Startup>(); // Replace Startup with your actual startup class
            _client = _factory.CreateClient();
        }

        [OneTimeSetUp] // This method runs once before all tests
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public async Task GetAllRoles_ShouldReturnOk_WithRolesList()
        {
            // Arrange
            var mockService = new Mock<IRoleService>();
            var mockMapper = new Mock<IMapper>();
            var rolesList = new List<RoleDto>
            {
                new RoleDto { Id = "1", Name = "Admin" },
                new RoleDto { Id = "2", Name = "User" }
            };
            mockService.Setup(service => service.GetAllAsync()).ReturnsAsync(rolesList);
            var controller = new RoleController(mockService.Object, mockMapper.Object);

            // Act
            var result = await controller.GetAllRoles();
            var okResult = result as OkObjectResult;

            // Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(rolesList);
        }

        [Test]
        public async Task GetRoleById_ShouldReturnOk_WhenRoleExists()
        {
            // Arrange
            var roleId = "1";
            var roleDto = new RoleDto { Id = roleId, Name = "Admin" };
            var mockService = new Mock<IRoleService>();
            var mockMapper = new Mock<IMapper>();
            mockService.Setup(service => service.GetByIdAsync(roleId)).ReturnsAsync(roleDto);
            var controller = new RoleController(mockService.Object, mockMapper.Object);

            // Act
            var result = await controller.GetRoleById(roleId);
            var okResult = result as OkObjectResult;

            // Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(roleDto);
        }

        [Test]
        public async Task GetRoleById_ShouldReturnNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = "invalid-id";
            var mockService = new Mock<IRoleService>();
            var mockMapper = new Mock<IMapper>();
            mockService.Setup(service => service.GetByIdAsync(roleId)).ReturnsAsync((RoleDto)null);
            var controller = new RoleController(mockService.Object, mockMapper.Object);

            // Act
            var result = await controller.GetRoleById(roleId);
            var notFoundResult = result as NotFoundObjectResult;

            // Assert
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Test]
        public async Task CreateRole_ShouldReturnCreated_WhenRoleIsValid()
        {
            // Arrange
            var roleDto = new RoleDto { Id = "3", Name = "Moderator" };
            var mockService = new Mock<IRoleService>();
            var mockMapper = new Mock<IMapper>();
            mockService.Setup(service => service.AddAsync(roleDto)).ReturnsAsync(roleDto);
            var controller = new RoleController(mockService.Object, mockMapper.Object);

            // Act
            var result = await controller.CreateRole(roleDto);
            var createdResult = result as CreatedAtActionResult;

            // Assert
            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(201);
            createdResult.Value.Should().BeEquivalentTo(roleDto);
        }

        [Test]
        public async Task UpdateRole_ShouldReturnOk_WhenRoleIsUpdated()
        {
            // Arrange
            var roleId = "1";
            var roleDto = new RoleDto { Id = roleId, Name = "UpdatedRoleName" };
            var mockService = new Mock<IRoleService>();
            var mockMapper = new Mock<IMapper>();
            mockService.Setup(service => service.GetByIdAsync(roleId)).ReturnsAsync(roleDto);
            mockService.Setup(service => service.Update(roleDto)).ReturnsAsync(roleDto);
            var controller = new RoleController(mockService.Object, mockMapper.Object);

            // Act
            var result = await controller.UpdateRole(roleId, roleDto);
            var okResult = result as OkObjectResult;

            // Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(roleDto);
        }

        [Test]
        public async Task DeleteRole_ShouldReturnOk_WhenRoleIsDeleted()
        {
            // Arrange
            var roleId = "1";
            var roleDto = new RoleDto { Id = roleId, Name = "RoleToDelete" };
            var mockService = new Mock<IRoleService>();
            var mockMapper = new Mock<IMapper>();
            mockService.Setup(service => service.GetByIdAsync(roleId)).ReturnsAsync(roleDto);
            mockService.Setup(service => service.Delete(roleId)).ReturnsAsync(roleDto);
            var controller = new RoleController(mockService.Object, mockMapper.Object);

            // Act
            var result = await controller.DeleteRole(roleId);
            var okResult = result as OkObjectResult;

            // Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(roleDto);
        }

        [OneTimeTearDown] // This method runs once after all tests
        public void Cleanup()
        {
            _client.Dispose(); // Release the HttpClient at the end
            _factory.Dispose(); // Release the factory
        }
    }
}