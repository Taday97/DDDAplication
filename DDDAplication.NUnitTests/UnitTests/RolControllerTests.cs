using AutoMapper;
using DDDAplication.API.Controllers;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;

namespace DDDAplication.NUnitTests.UnitTests
{
    [TestFixture]
    public class RoleControllerTests
    {
        private Mock<IRoleService> _rolServiceMock;
        private Mock<IMapper> _mapperMock;
        private RoleController _RoleController;

        [SetUp]
        public void Setup()
        {
            _rolServiceMock = new Mock<IRoleService>();
            _mapperMock = new Mock<IMapper>();
            _RoleController = new RoleController(_rolServiceMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task GetAllRoles_ShouldReturnOk_WhenRolesExist()
        {
            var roles = new List<RoleDto>
            {
                new RoleDto { Id = "1", Name = "Admin" },
                new RoleDto { Id = "2", Name = "User" }
            };

            _rolServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(roles);

            var result = await _RoleController.GetAllRoles();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(roles, okResult.Value);
        }

        [Test]
        public async Task GetRoleById_ShouldReturnOk_WhenRoleExists()
        {
            var roleId = "1";
            var role = new RoleDto { Id = roleId, Name = "Admin" };
            _rolServiceMock.Setup(s => s.GetByIdAsync(roleId)).ReturnsAsync(role);

            var result = await _RoleController.GetRoleById(roleId);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(role, okResult.Value);
        }

        [Test]
        public async Task GetRoleById_ShouldReturnNotFound_WhenRoleDoesNotExist()
        {
            var roleId = "non-existent-id";
            _rolServiceMock.Setup(s => s.GetByIdAsync(roleId)).ReturnsAsync((RoleDto)null);

            var result = await _RoleController.GetRoleById(roleId);

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            var jsonObject = JsonConvert.SerializeObject(notFoundResult.Value);
            string mensaje = JsonConvert.DeserializeObject<dynamic>(jsonObject).Message;
            Assert.AreEqual($"Role with ID {roleId} not found.", mensaje);
        }

        [Test]
        public async Task CreateRole_ShouldReturnCreated_WhenRoleIsSuccessfullyCreated()
        {
            var rolCreateDto = new RoleDto { Id = "3", Name = "Editor" };
            _rolServiceMock.Setup(s => s.AddAsync(rolCreateDto)).ReturnsAsync(rolCreateDto);

            var result = await _RoleController.CreateRole(rolCreateDto);

            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(rolCreateDto, createdResult.Value);
        }

        [Test]
        public async Task UpdateRole_ShouldReturnOk_WhenRoleIsUpdatedSuccessfully()
        {
            var roleId = "1";
            var rolUpdateDto = new RoleDto { Id = roleId, Name = "UpdatedRole" };
            _rolServiceMock.Setup(s => s.GetByIdAsync(roleId)).ReturnsAsync(rolUpdateDto);
            _rolServiceMock.Setup(s => s.Update(rolUpdateDto)).ReturnsAsync(rolUpdateDto);

            var result = await _RoleController.UpdateRole(roleId, rolUpdateDto);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(rolUpdateDto, okResult.Value);
        }

        [Test]
        public async Task UpdateRole_ShouldReturnNotFound_WhenRoleDoesNotExist()
        {
            var roleId = "non-existent-id";
            var rolUpdateDto = new RoleDto { Id = roleId, Name = "UpdatedRole" };
            _rolServiceMock.Setup(s => s.GetByIdAsync(roleId)).ReturnsAsync((RoleDto)null);

            var result = await _RoleController.UpdateRole(roleId, rolUpdateDto);

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            var jsonObject = JsonConvert.SerializeObject(notFoundResult.Value);
            string mensaje = JsonConvert.DeserializeObject<dynamic>(jsonObject).Message;
            Assert.AreEqual($"Role with ID {roleId} not found.", mensaje);
        }

        [Test]
        public async Task DeleteRole_ShouldReturnOk_WhenRoleIsDeleted()
        {
            var roleId = "1";
            var existingRole = new RoleDto { Id = roleId, Name = "Admin" };
            _rolServiceMock.Setup(s => s.GetByIdAsync(roleId)).ReturnsAsync(existingRole);
            _rolServiceMock.Setup(s => s.Delete(roleId)).ReturnsAsync(existingRole);

            var result = await _RoleController.DeleteRole(roleId);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(existingRole, okResult.Value);
        }

        [Test]
        public async Task DeleteRole_ShouldReturnNotFound_WhenRoleDoesNotExist()
        {
            var roleId = "non-existent-id";
            _rolServiceMock.Setup(s => s.GetByIdAsync(roleId)).ReturnsAsync((RoleDto)null);

            var result = await _RoleController.DeleteRole(roleId);

            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            var jsonObject = JsonConvert.SerializeObject(notFoundResult.Value);
            string mensaje = JsonConvert.DeserializeObject<dynamic>(jsonObject).Message;
            Assert.AreEqual($"Role with ID {roleId} not found.", mensaje);
        }
    }
}
