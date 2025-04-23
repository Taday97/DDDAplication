using DDDAplication.API.Controllers;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DDDAplication.Tests.Controllers
{
    [TestFixture]
    public class UserControllerUnitTests
    {
        private Mock<IUserService> _mockUserService;
        private UserController _controller;
        private ClaimsPrincipal _mockUser;

        [SetUp]
        public void Setup()
        {

            _mockUserService = new Mock<IUserService>();


            _mockUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123"),
                new Claim(ClaimTypes.Name, "Test User")
            }, "mock"));


            _controller = new UserController(_mockUserService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _mockUser }
                }
            };
        }

        [Test]
        public async Task GetMyProfile_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var userId = "123";
            var userDto = new UserDto { Id = userId, UserName = "Test User" };

            var expectedResponse = ApiResponse<UserDto>.CreateSuccessResponse("User profile retrieved successfully.", userDto);

            _mockUserService.Setup(s => s.GetProfileAsync(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetMyProfile();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(userDto);
        }
        [Test]
        public async Task GetMyProfile_ReturnsNotFound_WhenServiceFails()
        {
            // Arrange
            var fakeUserId = "user123";
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
               new Claim(ClaimTypes.NameIdentifier, fakeUserId)
            }, "TestAuth"));

            _mockUserService.Setup(s => s.GetProfileAsync(fakeUserId))
                .ReturnsAsync(ApiResponse<UserDto>.CreateErrorResponse("User not found."));

            var controller = new UserController(_mockUserService.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.GetMyProfile();

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            (result as NotFoundObjectResult)!.Value.Should().Be("User not found.");
        }

        [Test]
        public async Task GetMyProfile_ReturnsUnauthorized_WhenUserIdIsMissing()
        {
            // Arrange
            var controller = new UserController(_mockUserService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.GetMyProfile();

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            (result as UnauthorizedObjectResult)!.Value.Should().Be("User ID not found.");
        }


        [Test]
        public async Task GetAllUsers_ReturnsOk_WhenUsersExist()
        {
            // Arrange
            var mockUsers = new List<UserDto>
            {
                new UserDto { Id = "1", UserName = "Alice" },
                new UserDto { Id = "2", UserName = "Bob" }
            };

            var response = ApiResponse<IEnumerable<UserDto>>.CreateSuccessResponse("Users retrieved.", mockUsers);

            _mockUserService.Setup(s => s.GetAllAsync())
                            .ReturnsAsync(response);

            var controller = new UserController(_mockUserService.Object); // o el nombre de tu controller

            // Act
            var result = await controller.GetAllUsers();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(mockUsers);
        }

        [Test]
        public async Task GetAllUsers_ReturnsNotFound_WhenServiceFails()
        {
            // Arrange
            var errorMessage = "No users found.";
            _mockUserService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(ApiResponse<IEnumerable<UserDto>>.CreateErrorResponse(errorMessage));

            var controller = new UserController(_mockUserService.Object);

            // Act
            var result = await controller.GetAllUsers();

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            (result as NotFoundObjectResult)!.Value.Should().Be(errorMessage);
        }

        [Test]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "123";
            var expectedResponse = ApiResponse<UserDto>.CreateErrorResponse("User with id 123 not found.");

            _mockUserService.Setup(s => s.GetByIdAsync(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(expectedResponse.Message);
        }

        [Test]
        public async Task UpdateMyProfile_ReturnsUnauthorized_WhenUserIdIsMissingFromClaims()
        {
            // Arrange
            var userDto = new UserDto();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.UpdateMyProfile(userDto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult!.Value.Should().Be("User ID not found.");
        }

        [Test]
        public async Task UpdateMyProfile_ReturnsOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var userId = "123";
            var userDto = new UserDto { Id = userId, UserName = "Updated Name" };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var expectedResponse = ApiResponse<UserDto>.CreateSuccessResponse("User updated successfully.", userDto);

            _mockUserService.Setup(s => s.Update(It.Is<UserDto>(dto => dto.Id == userId)))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateMyProfile(userDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(userDto);
        }
        [Test]
        public async Task UpdateMyProfile_ReturnsNotFound_WhenUpdateFails()
        {
            // Arrange
            var userId = "123";
            var userDto = new UserDto { UserName = "Updated Name" };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var expectedResponse = ApiResponse<UserDto>.CreateErrorResponse("User not found.");

            _mockUserService.Setup(s => s.Update(It.Is<UserDto>(dto => dto.Id == userId)))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateMyProfile(userDto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be("User not found.");
        }


        [Test]
        public async Task UpdateUser_ReturnsOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var userId = "123";
            var userDto = new UserDto
            {
                Id = userId,
                UserName = "Updated User",
                Email = "updateduser@example.com",
                EmailConfirmed = true
            };

            var expectedResponse = ApiResponse<UserDto>.CreateSuccessResponse("Profile updated successfully.", userDto);

            _mockUserService.Setup(s => s.Update(userDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateUser(userId, userDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            okResult!.Value.Should().BeOfType<ApiResponse<UserDto>>();
            var apiResponse = okResult.Value as ApiResponse<UserDto>;
            apiResponse!.Data.Should().BeEquivalentTo(userDto, options => options
                .ComparingByMembers<UserDto>());
        }

        [Test]
        public async Task DeleteUser_ReturnsOk_WhenUserIsDeleted()
        {
            // Arrange
            var userId = "123";
            var userDto = new UserDto { Id = userId, UserName = "Test User" };
            var expectedResponse = ApiResponse<UserDto>.CreateSuccessResponse("User deleted successfully.", userDto);

            _mockUserService.Setup(s => s.Delete(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }


        [Test]
        public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "999"; // Usuario que no existe
            var userDto = new UserDto
            {
                Id = userId,
                Email = "userUpdate@gmail.com",
            };

            var expectedResponse = ApiResponse<UserDto>.CreateErrorResponse($"User with id {userId} not found.");

            _mockUserService.Setup(s => s.Update(userDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateUser(userId, userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var notFoundResult = result as BadRequestObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(expectedResponse.Message);
        }

        [Test]
        public async Task UpdateUser_ReturnsBadRequest_WhenUserDataIsNull()
        {
            // Arrange
            UserDto? userDto = null;

            // Act
            var result = await _controller.UpdateUser("123", userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("User data cannot be null.");
        }

        [Test]
        public async Task UpdateUser_ReturnsBadRequest_WhenIdInRequestBodyDoesNotMatchIdInUrl()
        {
            // Arrange
            var urlId = "123";
            var dto = new UserDto { Id = "456", Email = "userUpdate@gmail.com" };

            // Act
            var result = await _controller.UpdateUser(urlId, dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("User ID in the request body does not match the ID in the URL.");
        }


        [Test]
        public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "999"; // Usuario que no existe
            var expectedResponse = ApiResponse<UserDto>.CreateErrorResponse($"User with id {userId} not found.");

            _mockUserService.Setup(s => s.Delete(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(expectedResponse.Message);
        }

        [Test]
        public async Task DeleteUser_ReturnsBadRequest_WhenUserIdIsInvalid()
        {
            // Arrange
            var invalidUserId = "";
            var expectedResponse = ApiResponse<UserDto>.CreateErrorResponse("User ID is required.");

            _mockUserService.Setup(s => s.Delete(invalidUserId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteUser(invalidUserId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(expectedResponse.Message);
        }

        [Test]
        public async Task GetUserById_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var validUserId = "123";
            var userDto = new UserDto { Id = validUserId, Email = "admin@gmail.com" };
            var expectedResponse = ApiResponse<UserDto>.CreateSuccessResponse("User found successfully.", userDto);

            _mockUserService.Setup(s => s.GetByIdAsync(validUserId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUserById(validUserId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();  // Esperamos OkObjectResult
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse.Data);  // Verificamos que los datos coinciden
        }

        [Test]
        [TestCase(null, "User ID cannot be null or empty.")]
        [TestCase("", "User ID cannot be null or empty.")]
        public async Task GetUserById_ReturnsBadRequest_WhenUserIdIsNullOrInvalid(string? invalidUserId, string expectedMessage)
        {
            // Act
            var result = await _controller.GetUserById(invalidUserId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be(expectedMessage);
        }

        [Test]
        public async Task AssignRolesToUser_ReturnsOk_WhenRolesAreAssignedSuccessfully()
        {
            // Arrange
            var userId = "123";
            var roles = new List<string> { "Admin", "User" };
            var expectedResponse = ApiResponse<bool>.CreateSuccessResponse("Roles assigned successfully.", true);

            _mockUserService.Setup(s => s.AssignRolesToUserAsync(userId, roles))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.AssignRolesToUser(userId, roles);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public async Task AssignRolesToUser_ReturnsBadRequest_WhenRolesListIsEmpty()
        {
            // Arrange
            var userId = "123";
            var roles = new List<string>();

            // Act
            var result = await _controller.AssignRolesToUser(userId, roles);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Users list cannot be empty.");
        }

        [Test]
        public async Task AssignRolesToUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "999";
            var roles = new List<string> { "Admin" };
            var expectedResponse = ApiResponse<bool>.CreateErrorResponse("User not found.");

            _mockUserService.Setup(s => s.AssignRolesToUserAsync(userId, roles))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.AssignRolesToUser(userId, roles);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var notFoundResult = result as BadRequestObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(expectedResponse.Message);
        }
        [Test]
        public async Task RemoveRolesFromUser_ReturnsOk_WhenRolesAreRemovedSuccessfully()
        {
            // Arrange
            var userId = "123";
            var roles = new List<string> { "Admin", "User" };
            var expectedResponse = ApiResponse<bool>.CreateSuccessResponse("Roles removed successfully.", true);

            _mockUserService.Setup(s => s.RemoveRolesFromUserAsync(userId, roles))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RemoveRolesFromUser(userId, roles);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public async Task RemoveRolesFromUser_ReturnsBadRequest_WhenRolesListIsEmpty()
        {
            // Arrange
            var userId = "123";
            var roles = new List<string>();  // Lista vacía de roles

            // Act
            var result = await _controller.RemoveRolesFromUser(userId, roles);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Users list cannot be empty.");
        }

        [Test]
        public async Task RemoveRolesFromUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "999";  // ID de usuario que no existe
            var roles = new List<string> { "Admin" };
            var expectedResponse = ApiResponse<bool>.CreateErrorResponse("User not found.");

            _mockUserService.Setup(s => s.RemoveRolesFromUserAsync(userId, roles))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RemoveRolesFromUser(userId, roles);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var notFoundResult = result as BadRequestObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(expectedResponse.Message);
        }

        [Test]
        public async Task GetUserRoles_ReturnsOk_WhenRolesAreFound()
        {
            // Arrange
            var userId = "123";
            var roles = new List<string> { "Admin", "User" };
            var expectedResponse = ApiResponse<List<string>>.CreateSuccessResponse("Roles retrieved successfully.", roles);

            _mockUserService.Setup(s => s.GetUserRolesAsync(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUserRoles(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public async Task GetUserRoles_ReturnsBadRequest_WhenUserIdIsNull()
        {
            // Arrange
            string? invalidUserId = null;
            var expectedMessage = "User ID cannot be null or empty.";

            // Act
            var result = await _controller.GetUserRoles(invalidUserId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be(expectedMessage);
        }

        [Test]
        public async Task GetUserRoles_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "999";
            var expectedResponse = ApiResponse<List<string>>.CreateErrorResponse("User not found.");

            _mockUserService.Setup(s => s.GetUserRolesAsync(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUserRoles(userId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var notFoundResult = result as BadRequestObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(expectedResponse.Message);
        }

    }
}
