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
    [TestFixture] // Ensure this is present for NUnit
    public class UserControllerTest
    {
        private HttpClient _client;
        private readonly WebApplicationFactory<Startup> _factory; // We store the factory

        public UserControllerTest()
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
        public async Task GetAllUsers_ShouldReturnOk_WithUsersList()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            var usersList = new List<UserDto>
            {
                new UserDto { Id = "1", UserName = "John Doe" },
                new UserDto { Id = "2", UserName = "Jane Smith" }
            };
            mockService.Setup(service => service.GetAllAsync()).ReturnsAsync(usersList);
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.GetAllUsers();
            var okResult = result as OkObjectResult;

            // Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(usersList);
        }

        [Test]
        public async Task GetUserById_ShouldReturnOk_WhenUserExists()
        {
            // Arrange
            var userId = "1";
            var userDto = new UserDto { Id = userId, UserName = "John Doe" };
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.GetByIdAsync(userId)).ReturnsAsync(userDto);
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.GetUserById(userId);
            var okResult = result as OkObjectResult;

            // Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(userDto);
        }

        [Test]
        public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "invalid-id";
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.GetByIdAsync(userId)).ReturnsAsync((UserDto)null);
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.GetUserById(userId);
            var notFoundResult = result as NotFoundObjectResult;

            // Assert
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Test]
        public async Task UpdateUser_ShouldReturnOk_WhenUserIsUpdated()
        {
            // Arrange
            var userId = "1";
            var userDto = new UserDto { Id = userId, UserName = "Updated User" };
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.GetByIdAsync(userId)).ReturnsAsync(userDto);
            mockService.Setup(service => service.Update(userDto)).ReturnsAsync(userDto);
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.UpdateUser(userId, userDto);
            var okResult = result as OkObjectResult;

            // Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(userDto);
        }

        [Test]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenIdMismatch()
        {
            // Arrange
            var userId = "1";
            var userDto = new UserDto { Id = "2", UserName = "User" }; // Different ID
            var mockService = new Mock<IUserService>();
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.UpdateUser(userId, userDto);
            var badRequestResult = result as BadRequestObjectResult;

            // Assert
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task DeleteUser_ShouldReturnNoContent_WhenUserIsDeleted()
        {
            // Arrange
            var userId = "1";
            var userDto = new UserDto { Id = userId, UserName = "User to Delete" };
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.GetByIdAsync(userId)).ReturnsAsync(userDto);
            mockService.Setup(service => service.Delete(userId)).ReturnsAsync(userDto);
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.DeleteUser(userId);
            var noContentResult = result as NoContentResult;

            // Assert
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(204);
        }

        [Test]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "invalid-id";
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.GetByIdAsync(userId)).ReturnsAsync((UserDto)null);
            var controller = new UserController(mockService.Object);

            // Act
            var result = await controller.DeleteUser(userId);
            var notFoundResult = result as NotFoundObjectResult;

            // Assert
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }

        [OneTimeTearDown] // This method runs once after all tests
        public void Cleanup()
        {
            _client.Dispose(); // Release the HttpClient at the end
            _factory.Dispose(); // Release the factory
        }
    }
}