using AutoMapper;
using DDDAplication.API.Controllers;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DDDAplication.NUnitTests.UnitTests
{
    [TestFixture]
    public class RoleControllerTests
    {
        private Mock<IRoleService> _mockRoleService;
        private Mock<IMapper> _mapperMock;
        private RoleController _controller;

        [SetUp]
        public void Setup()
        {
            _mockRoleService = new Mock<IRoleService>();
            _mapperMock = new Mock<IMapper>();
            _controller = new RoleController(_mockRoleService.Object, _mapperMock.Object);
        }
        [Test]
        public async Task CreateRole_ReturnsCreatedAtAction_WhenCreationIsSuccessful()
        {
            // Arrange
            var newRole = new RoleDto { Name = "New Role" };
            var createdRole = new RoleDto { Id = "789", Name = "New Role" };

            var expectedResponse = ApiResponse<RoleDto>.CreateSuccessResponse("Role created successfully.", createdRole);

            _mockRoleService.Setup(s => s.AddAsync(newRole))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateRole(newRole);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;

            createdResult!.ActionName.Should().Be(nameof(_controller.GetRoleById));
            createdResult.RouteValues!["id"].Should().Be("789");

            var responseData = createdResult.Value as RoleDto;
            responseData.Should().BeEquivalentTo(createdRole);
        }
        [Test]
        public async Task CreateRole_ReturnsBadRequest_WhenRoleDtoIsNull()
        {
            // Act
            var result = await _controller.CreateRole(null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest!.Value.Should().Be("Role data cannot be null.");
        }
        [Test]
        public async Task CreateRole_ReturnsBadRequest_WhenServiceReturnsFailure()
        {
            // Arrange
            var roleDto = new RoleDto { Name = "Invalid Role" };
            var failedResponse = ApiResponse<RoleDto>.CreateErrorResponse("Failed to create role.");

            _mockRoleService.Setup(s => s.AddAsync(roleDto))
                .ReturnsAsync(failedResponse);

            // Act
            var result = await _controller.CreateRole(roleDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest!.Value.Should().Be("Failed to create role.");
        }


        [Test]
        public async Task CreateRole_ReturnsCreated_WhenRoleIsSuccessfullyCreated()
        {
            // Arrange
            var roleDto = new RoleDto { Name = "Admin" };
            var expectedResponse = ApiResponse<RoleDto>.CreateSuccessResponse("Role created successfully.", roleDto);

            _mockRoleService.Setup(s => s.AddAsync(roleDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateRole(roleDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.StatusCode.Should().Be(201); // 201 Created
            createdResult.Value.Should().BeEquivalentTo(expectedResponse.Data);
        }
        [Test]
        public async Task CreateRole_ReturnsBadRequest_WhenRoleDataIsNull()
        {
            // Arrange
            RoleDto? roleDto = null;

            // Act
            var result = await _controller.CreateRole(roleDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Role data cannot be null.");
        }

        [Test]
        public async Task GetAllRoles_ReturnsOk_WhenRolesExist()
        {
            // Arrange
            var users = new List<RoleDto>
            {
                new RoleDto { Id = "123", Name = "Role 1" },
                new RoleDto { Id = "456", Name = "Role 2" }
            };

            var expectedResponse = ApiResponse<IEnumerable<RoleDto>>.CreateSuccessResponse("Roles retrieved successfully.", users);

            _mockRoleService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAllRoles();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;


            var apiResponse = okResult!.Value as ApiResponse<IEnumerable<RoleDto>>;
            apiResponse!.Data.Should().BeEquivalentTo(users);
        }

        [Test]
        public async Task GetAllRoles_ReturnsOk_WithEmptyList()
        {
            // Arrange
            var roles = new List<RoleDto>();
            var expectedResponse = ApiResponse<IEnumerable<RoleDto>>.CreateSuccessResponse("No roles available.", roles);

            _mockRoleService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAllRoles();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            var apiResponse = okResult!.Value as ApiResponse<IEnumerable<RoleDto>>;
            apiResponse!.Data.Should().BeEmpty();
        }

        [Test]
        public async Task GetAllRoles_ReturnsNotFound_WhenNoRolesExist()
        {
            // Arrange
            var expectedResponse = ApiResponse<IEnumerable<RoleDto>>.CreateErrorResponse("No roles found.");

            _mockRoleService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAllRoles();

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;

            notFoundResult!.Value.Should().Be("No roles found.");
        }

        [Test]
        public async Task GetRoleById_ReturnsOk_WhenRoleExists()
        {
            // Arrange
            var validRoleId = "123";  // ID válido de un rol
            var roleDto = new RoleDto { Id = validRoleId, Name = "Admin" };
            var expectedResponse = ApiResponse<RoleDto>.CreateSuccessResponse("Role found successfully.", roleDto);

            _mockRoleService.Setup(s => s.GetByIdAsync(validRoleId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetRoleById(validRoleId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();  // Esperamos OkObjectResult
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse.Data);  // Verificamos que los datos coinciden
        }

        [Test]
        public async Task GetRoleById_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var userId = "123";
            var expectedResponse = ApiResponse<RoleDto>.CreateErrorResponse("Role with id 123 not found.");

            _mockRoleService.Setup(s => s.GetByIdAsync(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetRoleById(userId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(expectedResponse.Message);
        }

        [Test]
        [TestCase(null, "Role ID is required.")]
        [TestCase("", "Role ID is required.")]
        public async Task GetUserById_ReturnsBadRequest_WhenUserIdIsNullOrInvalid(string? invalidUserId, string expectedMessage)
        {
            // Act
            var result = await _controller.GetRoleById(invalidUserId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be(expectedMessage);
        }
        [Test]
        public async Task UpdateRole_ReturnsOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var userId = "123";
            var userDto = new RoleDto
            {
                Id = userId,
                Name = "Updated Role",
            };

            var expectedResponse = ApiResponse<RoleDto>.CreateSuccessResponse("Profile updated successfully.", userDto);

            _mockRoleService.Setup(s => s.Update(userDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateRole(userId, userDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            okResult!.Value.Should().BeOfType<ApiResponse<RoleDto>>();
            var apiResponse = okResult.Value as ApiResponse<RoleDto>;
            apiResponse!.Data.Should().BeEquivalentTo(userDto, options => options
                .ComparingByMembers<RoleDto>());
        }
        [Test]
        public async Task UpdateRole_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var userId = "999"; // Rol que no existe
            var userDto = new RoleDto
            {
                Id = userId,
                Name = "Updated Role",
            };

            var expectedResponse = ApiResponse<RoleDto>.CreateErrorResponse($"Role with id {userId} not found.");

            _mockRoleService.Setup(s => s.Update(userDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateRole(userId, userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var notFoundResult = result as BadRequestObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(expectedResponse.Message);
        }
       
        [Test]
        public async Task UpdateRole_ReturnsBadRequest_WhenRoleDataIsNull()
        {
            // Arrange
            RoleDto? roleDto = null;

            // Act
            var result = await _controller.UpdateRole("123", roleDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Role data cannot be null.");
        }
        [Test]
        public async Task UpdateRole_ReturnsBadRequest_WhenIdInRequestBodyDoesNotMatchIdInUrl()
        {
            // Arrange
            var urlId = "123";
            var dto = new RoleDto { Id = "456", Name = "Updated Role" }; 

            // Act
            var result = await _controller.UpdateRole(urlId, dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Role ID in the request body does not match the ID in the URL.");
        }


        [Test]
        public async Task DeleteRole_ReturnsOk_WhenRoleIsDeleted()
        {
            // Arrange
            var userId = "123";
            var userDto = new RoleDto { Id = userId, Name = "Test Role" };
            var expectedResponse = ApiResponse<RoleDto>.CreateSuccessResponse("Role deleted successfully.", userDto);

            _mockRoleService.Setup(s => s.Delete(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteRole(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public async Task DeleteRole_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = "999"; // Rol que no existe
            var expectedResponse = ApiResponse<RoleDto>.CreateErrorResponse($"Role with id {roleId} not found.");

            _mockRoleService.Setup(s => s.Delete(roleId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteRole(roleId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(expectedResponse.Message);
        }

        [Test]
        public async Task DeleteRole_ReturnsBadRequest_WhenRoleIdIsInvalid()
        {
            // Arrange
            var invalidRoleId = ""; 
            var expectedResponse = ApiResponse<RoleDto>.CreateErrorResponse("Role ID is required.");

            _mockRoleService.Setup(s => s.Delete(invalidRoleId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DeleteRole(invalidRoleId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(expectedResponse.Message);
        }


    }
}
