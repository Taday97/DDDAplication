using DDDAplication.API.Controllers;
using DDDAplication.Application.DTOs;
using DDDAplication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;

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
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

            _userManagerMock = CreateMockUserManager(userStoreMock.Object);

            var configurationMock = new Mock<IConfiguration>();
            var jwtSettingsMock = new Mock<IConfigurationSection>();
            SetupJwtSettings(jwtSettingsMock);

            configurationMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSettingsMock.Object);

            _configurationMock = configurationMock;
            _authController = new AuthController(_userManagerMock.Object, configurationMock.Object);
        }

        private Mock<UserManager<ApplicationUser>> CreateMockUserManager(IUserStore<ApplicationUser> userStore)
        {
            return new Mock<UserManager<ApplicationUser>>(
                userStore,
                null, // options
                null, // passwordHasher
                null, // userValidators
                null, // passwordValidators
                null, // lookupNormalizer
                null, // errors
                null, // services
                null  // logger
            );
        }

        private void SetupJwtSettings(Mock<IConfigurationSection> jwtSettingsMock)
        {
            jwtSettingsMock.Setup(x => x["Secret"]).Returns("una_clave_secreta_lo_bastante_larga_y_segura");
            jwtSettingsMock.Setup(x => x["Issuer"]).Returns("tu_emisor");
            jwtSettingsMock.Setup(x => x["Audience"]).Returns("tu_audiencia");
            jwtSettingsMock.Setup(x => x["TokenLifetimeMinutes"]).Returns("60");
        }

        [Test]
        public async Task Register_ShouldReturnOk_WhenUserIsRegisteredSuccessfully()
        {
            var registerModel = new RegisterModelDto
            {
                Username = "TestUser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), registerModel.Password))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _authController.Register(registerModel);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("User registered successfully.", GetResponseMessage(okResult));
        }

        [Test]
        public async Task Register_ShouldReturnBadRequest_WhenModelIsNull()
        {
            var result = await _authController.Register(null);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task Login_ShouldReturnOkWithToken_WhenCredentialsAreValid()
        {
            var loginModel = new LoginModelDto { Username = "TestUser", Password = "Password123!" };
            var user = new ApplicationUser { UserName = loginModel.Username };

            _userManagerMock.Setup(um => um.FindByNameAsync(loginModel.Username)).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, loginModel.Password)).ReturnsAsync(true);

            var result = await _authController.Login(loginModel);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var token = GetResponseToken(okResult);
            Assert.IsNotNull(token);
        }

        [Test]
        public async Task ConfirmEmail_ShouldReturnOk_WhenEmailIsConfirmedSuccessfully()
        {
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

            var result = await _authController.ConfirmEmail(confirmEmailModel);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var message = GetResponseMessage(okResult);
            Assert.AreEqual("Email confirmed successfully.", message);
        }

        [Test]
        public async Task ConfirmEmail_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var confirmEmailModel = new ConfirmEmailModelDto
            {
                UserId = "non-existent-user-id",
                Token = "valid-token"
            };

            _userManagerMock.Setup(um => um.FindByIdAsync(confirmEmailModel.UserId))
                .ReturnsAsync((ApplicationUser)null);

            var result = await _authController.ConfirmEmail(confirmEmailModel);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task ResetPassword_ShouldReturnOk_WhenPasswordIsResetSuccessfully()
        {
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

            var result = await _authController.ResetPassword(resetPasswordModel);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var message = GetResponseMessage(okResult);
            Assert.AreEqual("Password has been reset successfully.", message);
        }

        [Test]
        public async Task ResetPassword_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var resetPasswordModel = new ResetPasswordModelDto
            {
                Username = "NonExistentUser",
                Token = "valid-token",
                NewPassword = "NewPassword123!"
            };

            _userManagerMock.Setup(um => um.FindByNameAsync(resetPasswordModel.Username))
                .ReturnsAsync((ApplicationUser)null);

            var result = await _authController.ResetPassword(resetPasswordModel);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task SendResetLink_ShouldReturnOk_WhenUserExists()
        {
            var sendResetLinkModel = new SendResetLinkModelDto
            {
                Email = "test@example.com"
            };

            var user = new ApplicationUser { Email = sendResetLinkModel.Email };
            _userManagerMock.Setup(um => um.FindByEmailAsync(sendResetLinkModel.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");

            var result = await _authController.SendResetLink(sendResetLinkModel);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var message = GetResponseMessage(okResult);
            Assert.AreEqual("Password reset link has been sent.", message);
        }

        [Test]
        public async Task SendResetLink_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var sendResetLinkModel = new SendResetLinkModelDto
            {
                Email = "nonexistent@example.com"
            };

            _userManagerMock.Setup(um => um.FindByEmailAsync(sendResetLinkModel.Email))
                .ReturnsAsync((ApplicationUser)null);

            var result = await _authController.SendResetLink(sendResetLinkModel);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        private string GetResponseMessage(OkObjectResult okResult)
        {
            var jsonObject = JsonConvert.SerializeObject(okResult.Value);
            return JsonConvert.DeserializeObject<dynamic>(jsonObject).message;
        }

        private string GetResponseToken(OkObjectResult okResult)
        {
            var jsonObject = JsonConvert.SerializeObject(okResult.Value);
            return JsonConvert.DeserializeObject<dynamic>(jsonObject).token;
        }
    }
}
