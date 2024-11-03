using AutoMapper;
using DDDAplication.API.Controllers;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using DDDAplication.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            // Arrange
            var roles = new List<RoleDto>
            {
                new RoleDto { Id = "1", Name = "Admin" },
                new RoleDto { Id = "2", Name = "User" }
            };

            _rolServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(roles);

            // Act
            var result = await _RoleController.GetAllRoles();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(roles, okResult.Value);
        }

        [Test]
        public async Task GetRoleById_ShouldReturnOk_WhenRoleExists()
        {
            // Arrange
            var roleId = "1";
            var role = new RoleDto { Id = roleId, Name = "Admin" };
            _rolServiceMock.Setup(s => s.GetByIdAsync(roleId)).ReturnsAsync(role);

            // Act
            var result = await _RoleController.GetRoleById(roleId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(role, okResult.Value);
        }

        [Test]
        public async Task GetRoleById_ShouldReturnNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = "non-existent-id";
            _rolServiceMock.Setup(s => s.GetByIdAsync(roleId)).ReturnsAsync((RoleDto)null);

            // Act
            var result = await _RoleController.GetRoleById(roleId);

            // Assert
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
            // Arrange
            var rolCreateDto = new RoleDto { Id = "3", Name = "Editor" };
            _rolServiceMock.Setup(s => s.AddAsync(rolCreateDto)).ReturnsAsync(rolCreateDto);

            // Act
            var result = await _RoleController.CreateRole(rolCreateDto);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(rolCreateDto, createdResult.Value);
        }

        [Test]
        public async Task UpdateRole_ShouldReturnOk_WhenRoleIsUpdatedSuccessfully()
        {
            // Arrange
            var roleId = "1";
            var rolUpdateDto = new RoleDto { Id = roleId, Name = "UpdatedRole" };
            _rolServiceMock.Setup(s => s.GetByIdAsync(roleId)).ReturnsAsync(rolUpdateDto);
            _rolServiceMock.Setup(s => s.Update(rolUpdateDto)).ReturnsAsync(rolUpdateDto);

            // Act
            var result = await _RoleController.UpdateRole(roleId, rolUpdateDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(rolUpdateDto, okResult.Value);
        }

        [Test]
        public async Task UpdateRole_ShouldReturnNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = "non-existent-id";
            var rolUpdateDto = new RoleDto { Id = roleId, Name = "UpdatedRole" };
            _rolServiceMock.Setup(s => s.GetByIdAsync(roleId)).ReturnsAsync((RoleDto)null);

            // Act
            var result = await _RoleController.UpdateRole(roleId, rolUpdateDto);

            // Assert
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
            // Arrange
            var roleId = "1";
            var existingRole = new RoleDto { Id = roleId, Name = "Admin" };
            _rolServiceMock.Setup(s => s.GetByIdAsync(roleId)).ReturnsAsync(existingRole);
            _rolServiceMock.Setup(s => s.Delete(roleId)).ReturnsAsync(existingRole);

            // Act
            var result = await _RoleController.DeleteRole(roleId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(existingRole, okResult.Value);
        }

        [Test]
        public async Task DeleteRole_ShouldReturnNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = "non-existent-id";
            _rolServiceMock.Setup(s => s.GetByIdAsync(roleId)).ReturnsAsync((RoleDto)null);

            // Act
            var result = await _RoleController.DeleteRole(roleId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            var jsonObject = JsonConvert.SerializeObject(notFoundResult.Value);
            string mensaje = JsonConvert.DeserializeObject<dynamic>(jsonObject).Message;
            Assert.AreEqual($"Role with ID {roleId} not found.", mensaje);
        }
    }
}
