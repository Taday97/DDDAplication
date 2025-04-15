using AutoMapper;
using DDDAplication.API;
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
        private readonly WebApplicationFactory<Program> _factory;

        public RoleControllerTest()
        {
            _factory = new WebApplicationFactory<Program>(); 
            _client = _factory.CreateClient();
        }

        [OneTimeSetUp] 
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public async Task GetAllRoles_ShouldReturnOk_WithRolesList()
        {
           
            var mockService = new Mock<IRoleService>();
            var mockMapper = new Mock<IMapper>();
            var rolesList = new List<RoleDto>
            {
                new RoleDto { Id = "1", Name = "Admin" },
                new RoleDto { Id = "2", Name = "User" }
            };
            mockService.Setup(service => service.GetAllAsync()).ReturnsAsync(rolesList);
            var controller = new RoleController(mockService.Object, mockMapper.Object);

           
            var result = await controller.GetAllRoles();
            var okResult = result as OkObjectResult;

            
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(rolesList);
        }

        [Test]
        public async Task GetRoleById_ShouldReturnOk_WhenRoleExists()
        {
            
            var roleId = "1";
            var roleDto = new RoleDto { Id = roleId, Name = "Admin" };
            var mockService = new Mock<IRoleService>();
            var mockMapper = new Mock<IMapper>();
            mockService.Setup(service => service.GetByIdAsync(roleId)).ReturnsAsync(roleDto);
            var controller = new RoleController(mockService.Object, mockMapper.Object);

           
            var result = await controller.GetRoleById(roleId);
            var okResult = result as OkObjectResult;

           
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(roleDto);
        }

        [Test]
        public async Task GetRoleById_ShouldReturnNotFound_WhenRoleDoesNotExist()
        {
            
            var roleId = "invalid-id";
            var mockService = new Mock<IRoleService>();
            var mockMapper = new Mock<IMapper>();
            mockService.Setup(service => service.GetByIdAsync(roleId)).ReturnsAsync((RoleDto)null);
            var controller = new RoleController(mockService.Object, mockMapper.Object);

            
            var result = await controller.GetRoleById(roleId);
            var notFoundResult = result as NotFoundObjectResult;

           
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Test]
        public async Task CreateRole_ShouldReturnCreated_WhenRoleIsValid()
        {
            
            var roleDto = new RoleDto { Id = "3", Name = "Moderator" };
            var mockService = new Mock<IRoleService>();
            var mockMapper = new Mock<IMapper>();
            mockService.Setup(service => service.AddAsync(roleDto)).ReturnsAsync(roleDto);
            var controller = new RoleController(mockService.Object, mockMapper.Object);

            
            var result = await controller.CreateRole(roleDto);
            var createdResult = result as CreatedAtActionResult;

           
            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(201);
            createdResult.Value.Should().BeEquivalentTo(roleDto);
        }

        [Test]
        public async Task UpdateRole_ShouldReturnOk_WhenRoleIsUpdated()
        {
            
            var roleId = "1";
            var roleDto = new RoleDto { Id = roleId, Name = "UpdatedRoleName" };
            var mockService = new Mock<IRoleService>();
            var mockMapper = new Mock<IMapper>();
            mockService.Setup(service => service.GetByIdAsync(roleId)).ReturnsAsync(roleDto);
            mockService.Setup(service => service.Update(roleDto)).ReturnsAsync(roleDto);
            var controller = new RoleController(mockService.Object, mockMapper.Object);

            
            var result = await controller.UpdateRole(roleId, roleDto);
            var okResult = result as OkObjectResult;

          
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(roleDto);
        }

        [Test]
        public async Task DeleteRole_ShouldReturnOk_WhenRoleIsDeleted()
        {
           
            var roleId = "1";
            var roleDto = new RoleDto { Id = roleId, Name = "RoleToDelete" };
            var mockService = new Mock<IRoleService>();
            var mockMapper = new Mock<IMapper>();
            mockService.Setup(service => service.GetByIdAsync(roleId)).ReturnsAsync(roleDto);
            mockService.Setup(service => service.Delete(roleId)).ReturnsAsync(roleDto);
            var controller = new RoleController(mockService.Object, mockMapper.Object);

      
            var result = await controller.DeleteRole(roleId);
            var okResult = result as OkObjectResult;

           
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(roleDto);
        }

        [OneTimeTearDown] 
        public void Cleanup()
        {
            _client.Dispose(); 
            _factory.Dispose(); 
        }
    }
}