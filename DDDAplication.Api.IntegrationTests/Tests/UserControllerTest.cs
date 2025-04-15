using DDDAplication.Api.IntegrationTests.Common;
using DDDAplication.API;
using DDDAplication.API.Controllers;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DDDAplication.Api.IntegrationTests
{
    [TestFixture] 
    public class UserControllerTest
    {
        private HttpClient _client;
        private readonly ApiApplicationFactory<Program> _factory; 

        public UserControllerTest()
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
        public async Task GetAllUsers_ShouldReturnOk_WithUsersList()
        {
          
            var mockService = new Mock<IUserService>();
            var usersList = new List<UserDto>
            {
                new UserDto { Id = "1", UserName = "John Doe" },
                new UserDto { Id = "2", UserName = "Jane Smith" }
            };
            mockService.Setup(service => service.GetAllAsync()).ReturnsAsync(usersList);
            var controller = new UserController(mockService.Object);

          
            var result = await controller.GetAllUsers();
            var okResult = result as OkObjectResult;

           
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(usersList);
        }

        [Test]
        public async Task GetUserById_ShouldReturnOk_WhenUserExists()
        {
           
            var userId = "1";
            var userDto = new UserDto { Id = userId, UserName = "John Doe" };
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.GetByIdAsync(userId)).ReturnsAsync(userDto);
            var controller = new UserController(mockService.Object);

          
            var result = await controller.GetUserById(userId);
            var okResult = result as OkObjectResult;

           
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(userDto);
        }

        [Test]
        public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
           
            var userId = "invalid-id";
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.GetByIdAsync(userId)).ReturnsAsync((UserDto)null);
            var controller = new UserController(mockService.Object);

           
            var result = await controller.GetUserById(userId);
            var notFoundResult = result as NotFoundObjectResult;

         
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Test]
        public async Task UpdateUser_ShouldReturnOk_WhenUserIsUpdated()
        {
           
            var userId = "1";
            var userDto = new UserDto { Id = userId, UserName = "Updated User" };
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.GetByIdAsync(userId)).ReturnsAsync(userDto);
            mockService.Setup(service => service.Update(userDto)).ReturnsAsync(userDto);
            var controller = new UserController(mockService.Object);

           
            var result = await controller.UpdateUser(userId, userDto);
            var okResult = result as OkObjectResult;

          
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(userDto);
        }

        [Test]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenIdMismatch()
        {
         
            var userId = "1";
            var userDto = new UserDto { Id = "2", UserName = "User" }; // Different ID
            var mockService = new Mock<IUserService>();
            var controller = new UserController(mockService.Object);

           
            var result = await controller.UpdateUser(userId, userDto);
            var badRequestResult = result as BadRequestObjectResult;

          
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task DeleteUser_ShouldReturnNoContent_WhenUserIsDeleted()
        {
           
            var userId = "1";
            var userDto = new UserDto { Id = userId, UserName = "User to Delete" };
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.GetByIdAsync(userId)).ReturnsAsync(userDto);
            mockService.Setup(service => service.Delete(userId)).ReturnsAsync(userDto);
            var controller = new UserController(mockService.Object);

        
            var result = await controller.DeleteUser(userId);
            var noContentResult = result as NoContentResult;

        
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(204);
        }

        [Test]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
         
            var userId = "invalid-id";
            var mockService = new Mock<IUserService>();
            mockService.Setup(service => service.GetByIdAsync(userId)).ReturnsAsync((UserDto)null);
            var controller = new UserController(mockService.Object);

       
            var result = await controller.DeleteUser(userId);
            var notFoundResult = result as NotFoundObjectResult;

          
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }

        [OneTimeTearDown] 
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose(); 
        }
    }
}