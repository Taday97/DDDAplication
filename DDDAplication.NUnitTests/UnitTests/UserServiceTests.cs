using AutoMapper;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Profiles;
using DDDAplication.Application.Services;
using DDDAplication.Domain.Entities;
using DDDAplication.Domain.Interfaces;
using Moq;

namespace DDDAplication.NUnitTests.UnitTests
{
    public class UserServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private UserService _userService;

        [SetUp]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ApplicationProfile());
            });
            var mapper = config.CreateMapper();

            _userService = new UserService(_userRepositoryMock.Object, mapper);
        }

        [Test]
        public async Task GetUserById_ReturnsUser_WhenUserExists()
        {
            Guid userId = Guid.Parse("09C6BF8C-DFD6-4ED7-6E4D-08DCFAF90F9F");
            var expectedUser = new ApplicationUser { Id = userId, UserName = "Test User" };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId.ToString()))
                               .ReturnsAsync(expectedUser);

            var result = await _userService.GetByIdAsync(userId.ToString());

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedUser.UserName, result.UserName);
        }

        [Test]
        public async Task GetUserById_ReturnsNull_WhenUserDoesNotExist()
        {
            Guid userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId.ToString()))
                               .ReturnsAsync((ApplicationUser)null);

            var result = await _userService.GetByIdAsync(userId.ToString());

            Assert.IsNull(result);
        }
        [Test]
        public async Task GetAllAsync_ReturnsAllUsers()
        {
            var users = new List<ApplicationUser>
                {
                    new ApplicationUser { Id = Guid.NewGuid(), UserName = "User One" },
                    new ApplicationUser { Id = Guid.NewGuid(), UserName = "User Two" }
                };
            _userRepositoryMock.Setup(repo => repo.GetAllAsync())
                               .ReturnsAsync(users);

            var result = await _userService.GetAllAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task UpdateAsync_UpdatesUserSuccessfully()
        {
            var existingUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "Existing User" };
            UserDto updatedUser = new UserDto { Id = existingUser.Id.ToString(), UserName = "Updated User" };
            ApplicationUser updatedUserReport = new ApplicationUser { Id = existingUser.Id, UserName = "Updated User" };
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(existingUser.Id.ToString()))
                               .ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(updatedUserReport))
                               .ReturnsAsync(updatedUserReport);

            var result = await _userService.Update(updatedUser);

            Assert.IsNotNull(result);
            Assert.AreEqual(updatedUser.UserName, result.UserName);
        }

        [Test]
        public async Task UpdateAsync_ThrowsException_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            var nonExistentUser = new UserDto { Id = userId.ToString(), UserName = "User" };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId.ToString()))
                               .ReturnsAsync((ApplicationUser)null);

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _userService.Update(nonExistentUser));

            Assert.AreEqual($"User with id {userId} not found.", ex.Message);
        }

        [Test]
        public async Task DeleteAsync_DeletesUserSuccessfully()
        {
            var userId = Guid.NewGuid();
            var existingUser = new ApplicationUser { Id = userId, UserName = "Existing User" };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId.ToString()))
                               .ReturnsAsync(existingUser);

            var result = await _userService.Delete(userId.ToString());

            Assert.IsNotNull(result);
            Assert.AreEqual(existingUser.UserName, result.UserName);
        }

        [Test]
        public async Task DeleteAsync_ThrowsException_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId.ToString()))
                               .ReturnsAsync((ApplicationUser)null);

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _userService.Delete(userId.ToString()));

            Assert.AreEqual($"User with id {userId} not found.", ex.Message);
        }
    }
}
