using DDDAplication.API.Controllers;
using DDDAplication.Application.DTOs;
using DDDAplication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace DDDAplication.NUnitTests.UnitTests
{
    public class AuthControllerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<IConfiguration> _configurationMock;
        private AuthController _authController;

        [SetUp]
        public void Setup()
        {
            // Mock IUserStore
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

            // Setup the UserManager with the mocked UserStore
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null, // options
                null, // passwordHasher
                null, // userValidators
                null, // passwordValidators
                null, // lookupNormalizer
                null, // errors
                null, // services
                null  // logger
            );

            var configurationMock = new Mock<IConfiguration>();
            // Mock del IConfiguration
             var jwtSettingsMock = new Mock<IConfigurationSection>();
            jwtSettingsMock.Setup(x => x["Secret"]).Returns("una_clave_secreta_lo_bastante_larga_y_segura");
            jwtSettingsMock.Setup(x => x["Issuer"]).Returns("tu_emisor");
            jwtSettingsMock.Setup(x => x["Audience"]).Returns("tu_audiencia");
            jwtSettingsMock.Setup(x => x["TokenLifetimeMinutes"]).Returns("60");

            configurationMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSettingsMock.Object);
            // Setup any configuration needed
            _configurationMock = configurationMock;
            _authController = new AuthController(_userManagerMock.Object, configurationMock.Object);
        }

        [Test]
        public async Task Register_ShouldReturnOk_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange
            var registerModel = new RegisterModelDto
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = "Password123!"
            };
            var user = new ApplicationUser
            {
                UserName = registerModel.Username,
                Email = registerModel.Email
            };

            // Setup UserManager to return success when CreateAsync is called
            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), registerModel.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authController.Register(registerModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var jsonObject = JsonConvert.SerializeObject(okResult.Value);
            string mensaje = JsonConvert.DeserializeObject<dynamic>(jsonObject).message;

            Assert.AreEqual("User registered successfully.", mensaje);
        }

        [Test]
        public async Task Register_ShouldReturnBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _authController.Register(null);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task Login_ShouldReturnOkWithToken_WhenCredentialsAreValid()
        {
            // Arrange
            var loginModel = new LoginModelDto { Username = "TestUser", Password = "Password123!" };
            var user = new ApplicationUser { UserName = loginModel.Username };

            _userManagerMock.Setup(um => um.FindByNameAsync(loginModel.Username)).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, loginModel.Password)).ReturnsAsync(true);



            // Act
            var result = await _authController.Login(loginModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var jsonObject = JsonConvert.SerializeObject(okResult.Value);
            string token = JsonConvert.DeserializeObject<dynamic>(jsonObject).token;
            Assert.IsNotNull(token); // Asegúrate de que se devuelve un token
        }


        [Test]
        public async Task ConfirmEmail_ShouldReturnOk_WhenEmailIsConfirmedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var confirmEmailModel = new ConfirmEmailModelDto
            {
                UserId = userId.ToString(),
                Token = "valid-token"
            };

            var user = new ApplicationUser { Id = userId };
            _userManagerMock.Setup(um => um.FindByIdAsync(confirmEmailModel.UserId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.ConfirmEmailAsync(user, confirmEmailModel.Token))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authController.ConfirmEmail(confirmEmailModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var jsonObject = JsonConvert.SerializeObject(okResult.Value);
            string mensaje = JsonConvert.DeserializeObject<dynamic>(jsonObject).message;
            Assert.AreEqual("Email confirmed successfully.", mensaje);
        }

        [Test]
        public async Task ConfirmEmail_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var confirmEmailModel = new ConfirmEmailModelDto
            {
                UserId = "non-existent-user-id",
                Token = "valid-token"
            };

            _userManagerMock.Setup(um => um.FindByIdAsync(confirmEmailModel.UserId))
                .ReturnsAsync((ApplicationUser)null); // User not found

            // Act
            var result = await _authController.ConfirmEmail(confirmEmailModel);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task ResetPassword_ShouldReturnOk_WhenPasswordIsResetSuccessfully()
        {
            // Arrange
            var resetPasswordModel = new ResetPasswordModelDto
            {
                Username = "TestUser",
                Token = "valid-token",
                NewPassword = "NewPassword123!"
            };

            var user = new ApplicationUser { UserName = resetPasswordModel.Username };
            _userManagerMock.Setup(um => um.FindByNameAsync(resetPasswordModel.Username))
                .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authController.ResetPassword(resetPasswordModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var jsonObject = JsonConvert.SerializeObject(okResult.Value);
            string mensaje = JsonConvert.DeserializeObject<dynamic>(jsonObject).message;
            Assert.AreEqual("Password has been reset successfully.", mensaje);
        }

        [Test]
        public async Task ResetPassword_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var resetPasswordModel = new ResetPasswordModelDto
            {
                Username = "NonExistentUser",
                Token = "valid-token",
                NewPassword = "NewPassword123!"
            };

            _userManagerMock.Setup(um => um.FindByNameAsync(resetPasswordModel.Username))
                .ReturnsAsync((ApplicationUser)null); // User not found

            // Act
            var result = await _authController.ResetPassword(resetPasswordModel);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task SendResetLink_ShouldReturnOk_WhenUserExists()
        {
            // Arrange
            var sendResetLinkModel = new SendResetLinkModelDto
            {
                Email = "test@example.com"
            };

            var user = new ApplicationUser { Email = sendResetLinkModel.Email };
            _userManagerMock.Setup(um => um.FindByEmailAsync(sendResetLinkModel.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");

            // Act
            var result = await _authController.SendResetLink(sendResetLinkModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var jsonObject = JsonConvert.SerializeObject(okResult.Value);
            string mensaje = JsonConvert.DeserializeObject<dynamic>(jsonObject).message;
            Assert.AreEqual("Password reset link has been sent.", mensaje);
        }

        [Test]
        public async Task SendResetLink_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var sendResetLinkModel = new SendResetLinkModelDto
            {
                Email = "nonexistent@example.com"
            };

            _userManagerMock.Setup(um => um.FindByEmailAsync(sendResetLinkModel.Email))
                .ReturnsAsync((ApplicationUser)null); // User not found

            // Act
            var result = await _authController.SendResetLink(sendResetLinkModel);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

    }
}
