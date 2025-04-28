using Azure;
using DDDAplication.API.Controllers;
using DDDAplication.Application.DTOs.ApiResponse;
using DDDAplication.Application.DTOs.Auth;
using DDDAplication.Application.DTOs.User;
using DDDAplication.Application.Interfaces;
using DDDAplication.Application.Services;
using DDDAplication.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DDDAplication.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerUnitTests
    {
        private Mock<IUserService> _mockUserService;
        private AuthController _controller;

        [SetUp]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new AuthController(_mockUserService.Object);
        }

        [Test]
        public async Task Register_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");

            // Act
            var result = await _controller.Register(new RegisterModelDto());

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Register_ServiceFails_ReturnsBadRequestWithErrorMessage()
        {
            // Arrange
            var model = new RegisterModelDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

           
            var apiResponse = ApiResponse<string>.CreateErrorResponse("User registration failed.", new List<string> { "Password too weak" });


            _mockUserService.Setup(x => x.RegisterUser(It.IsAny<RegisterModelDto>()))
                            .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Register(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();  
            var badRequest = result as BadRequestObjectResult;

            badRequest!.Value.Should().Be("User registration failed.");
        }

        [Test]
        public async Task Register_ServiceSuccess_ReturnsOk()
        {
            // Arrange
            var dto = new RegisterModelDto { Username = "test", Email = "test@test.com", Password = "Password123!" };

            var expectedResponse = ApiResponse<string>.CreateSuccessResponse("User registered successfully.");
           
            _mockUserService.Setup(s => s.RegisterUser(dto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Register(dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task Register_EmailAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var model = new RegisterModelDto
            {
                Username = "newuser",
                Email = "existing@email.com",
                Password = "Password123!"
            };

            var apiResponse = ApiResponse<string>.CreateErrorResponse("This email is already registered.");

            _mockUserService.Setup(x => x.RegisterUser(It.IsAny<RegisterModelDto>()))
                            .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Register(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest!.Value.Should().Be("This email is already registered.");
        }
        [Test]
        public async Task Register_UsernameAlreadyTaken_ReturnsBadRequest()
        {
            // Arrange
            var model = new RegisterModelDto
            {
                Username = "existinguser",
                Email = "new@email.com",
                Password = "Password123!"
            };

            var apiResponse = ApiResponse<string>.CreateErrorResponse("This username is already taken.");

            _mockUserService.Setup(x => x.RegisterUser(It.IsAny<RegisterModelDto>()))
                            .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Register(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest!.Value.Should().Be("This username is already taken.");
        }


        [Test]
        public async Task Login_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Password", "Required");

            // Act
            var result = await _controller.Login(new LoginModelDto());

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Login_FailedAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new LoginModelDto { Username = "Test1", Password = "wrongpass" };
            var expectedResponse = ApiResponse<LoginResultDto>.CreateErrorResponse("Invalid credentials.");
            _mockUserService.Setup(s => s.LoginAsync(dto))
                .ReturnsAsync(expectedResponse);
            // Act
            var result = await _controller.Login(dto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Test]
        public async Task Login_SuccessfulAuthentication_ReturnsOkWithToken()
        {
            // Arrange
            var dto = new LoginModelDto { Username = "Test1", Password = "Password123!" };
            var tokenData = new LoginResultDto { Token = "mocked-token" };
            var expectedResponse = ApiResponse<LoginResultDto>.CreateSuccessResponse("Login successful.", tokenData);

            _mockUserService.Setup(s => s.LoginAsync(dto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult).Value.Should().Be(tokenData);
        }

        [Test]
        public async Task ConfirmEmail_ValidTokenAndEmail_ReturnsOk()
        {
            // Arrange
            string token = "valid-token";
            string email = "test@example.com";

            var response = ApiResponse<string>.CreateSuccessResponse("Email confirmed successfully.");

            _mockUserService.Setup(s => s.ConfirmEmailAsync(It.Is<ConfirmEmailModelDto>(
                m => m.Token == token && m.Email == email)))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.ConfirmEmailViaLink(token, email);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be("Email confirmed successfully.");
        }
        [Test]
        public async Task ConfirmEmail_InvalidToken_ReturnsBadRequest()
        {
            // Arrange
            string token = "invalid-token";
            string email = "test@example.com";

            var response = ApiResponse<string>.CreateErrorResponse("Email confirmation failed.");

            _mockUserService.Setup(s => s.ConfirmEmailAsync(It.IsAny<ConfirmEmailModelDto>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.ConfirmEmailViaLink(token, email);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest!.Value.Should().Be("Email confirmation failed.");
        }
        [Test]
        public async Task ConfirmEmail_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            string token = "any-token";
            string email = "notfound@example.com";

            var response = ApiResponse<string>.CreateErrorResponse("User not found.");

            _mockUserService.Setup(s => s.ConfirmEmailAsync(It.IsAny<ConfirmEmailModelDto>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.ConfirmEmailViaLink(token, email);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest!.Value.Should().Be("User not found.");
        }

        [Test]
        public async Task RefreshToken_ValidToken_ReturnsOkWithNewToken()
        {
            // Arrange
            var model = new RefreshTokenModelDto { Token = "valid-token" };
            var refreshTokenResponse = ApiResponse<RefreshTokenModelDto>.CreateSuccessResponse("Token refreshed successfully.",
                new RefreshTokenModelDto { Token = "new-valid-token" });

            _mockUserService.Setup(s => s.RefreshTokenAsync(It.Is<RefreshTokenModelDto>(m => m.Token == model.Token)))
                            .ReturnsAsync(refreshTokenResponse);

            // Act
            var result = await _controller.RefreshToken(model);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var value = okResult!.Value as RefreshTokenModelDto;
            value?.Token.Should().Be("new-valid-token");
        }
        [Test]
        public async Task RefreshToken_InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var model = new RefreshTokenModelDto { Token = "invalid-token" };
            var refreshTokenResponse = ApiResponse<RefreshTokenModelDto>.CreateErrorResponse("Invalid token.");

            _mockUserService.Setup(s => s.RefreshTokenAsync(It.Is<RefreshTokenModelDto>(m => m.Token == model.Token)))
                            .ReturnsAsync(refreshTokenResponse);

            // Act
            var result = await _controller.RefreshToken(model);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;

            var json = JObject.FromObject(unauthorizedResult!.Value!);
            string message = json["message"]?.ToString();

            message.Should().Be("Invalid token.");
        }

        [Test]
        public async Task RefreshToken_TokenRefreshedButDataIsNull_ReturnsOkWithNull()
        {
            // Arrange
            var model = new RefreshTokenModelDto { Token = "valid-token" };

            var response = ApiResponse<RefreshTokenModelDto>.CreateSuccessResponse("Token refreshed successfully.", null!);

            _mockUserService.Setup(s => s.RefreshTokenAsync(It.IsAny<RefreshTokenModelDto>()))
                            .ReturnsAsync(response);

            // Act
            var result = await _controller.RefreshToken(model);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeNull();
        }

        [Test]
        public async Task RefreshToken_EmptyToken_ReturnsUnauthorized()
        {
            // Arrange
            var model = new RefreshTokenModelDto { Token = "" };
            var response = ApiResponse<RefreshTokenModelDto>.CreateErrorResponse("Invalid token.");

            _mockUserService.Setup(s => s.RefreshTokenAsync(It.IsAny<RefreshTokenModelDto>()))
                            .ReturnsAsync(response);

            // Act
            var result = await _controller.RefreshToken(model);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;

            var json = JObject.FromObject(unauthorizedResult!.Value!);
            json["message"]!.ToString().Should().Be("Invalid token.");
        }

        [Test]
        public async Task GetProfile_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new AuthController(_mockUserService.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // Sin claims
            };

            // Act
            var result = await controller.GetProfile();

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorized = result as UnauthorizedObjectResult;
            unauthorized!.Value.Should().BeEquivalentTo(new { message = "User not authenticated." });
        }

        [Test]
        public async Task GetProfile_ServiceReturnsError_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId)}, "mock"));

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            _mockUserService.Setup(x => x.GetProfileAsync(userId))
                .ReturnsAsync(ApiResponse<UserPorfileDto>.CreateErrorResponse("User not found."));

            var controller = new AuthController(_mockUserService.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };

            // Act
            var result = await controller.GetProfile();

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFound = result as NotFoundObjectResult;
            notFound!.Value.Should().BeEquivalentTo(new { message = "User not found." });
        }

        [Test]
        public async Task GetProfile_ServiceReturnsSuccess_ReturnsOkWithProfile()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            { new Claim(ClaimTypes.NameIdentifier, userId) }, "mock"));

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            var expectedProfile = new UserPorfileDto
            {
                UserName = "testuser",
                Email = "test@example.com",
            };
            var expectedResponse = ApiResponse<UserPorfileDto>.CreateSuccessResponse("Profile retrieved successfully.", expectedProfile); ;
            _mockUserService.Setup(x => x.GetProfileAsync(userId))
                .ReturnsAsync(expectedResponse);

            var controller = new AuthController(_mockUserService.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };

            // Act
            var result = await controller.GetProfile();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedProfile);
        }

        [Test]
        public async Task SendResetLink_InvalidModelState_ReturnsBadRequest()
        {
            // Assert
            var model = new SendResetLinkModelDto();
            var expectedResponse = ApiResponse<string>.CreateErrorResponse("SendResetLinkModelDto cannot be null.");
            _mockUserService.Setup(x => x.SendResetLinkAsync(model))
                .ReturnsAsync(expectedResponse);

           
            // Act
            var result = await _controller.SendResetLink(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var okResult = result as BadRequestObjectResult;
            okResult!.Value.Should().BeEquivalentTo("SendResetLinkModelDto cannot be null.");

        }
        [Test]
        public async Task SendResetLink_ServiceFails_ReturnsBadRequestWithError()
        {
            // Arrange
            var model = new SendResetLinkModelDto { Email = "notfound@example.com" };

            var apiResponse = ApiResponse<string>.CreateErrorResponse("Email not registered.");

            _mockUserService.Setup(s => s.SendResetLinkAsync(model))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.SendResetLink(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest!.Value.Should().Be("Email not registered.");
        }
        [Test]
        public async Task SendResetLink_ServiceSuccess_ReturnsOk()
        {
            // Arrange
            var model = new SendResetLinkModelDto { Email = "user@example.com" };

            var apiResponse = ApiResponse<string>.CreateSuccessResponse("Reset link sent.");

            _mockUserService.Setup(s => s.SendResetLinkAsync(model))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.SendResetLink(model);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be("Reset link sent.");
        }

        [Test]
        public async Task ResetPassword_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var model = new ResetPasswordModelDto(); 

            var expectedResponse = ApiResponse<string>.CreateErrorResponse("ResetPasswordModelDto cannot be null.");
            _mockUserService.Setup(x => x.ResetPasswordAsync(model))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            var list= new List<string> ();
            badRequest!.Value.Should().BeEquivalentTo(
                new
                {
                    message = "ResetPasswordModelDto cannot be null.",
                    errors = list
                });
        }

        [Test]
        public async Task ResetPassword_ServiceFails_ReturnsBadRequestWithErrors()
        {
            // Arrange
            var model = new ResetPasswordModelDto
            {
                Username = "user@example.com",
                Token = "bad-token",
                NewPassword = "weakpass"
            };

            var apiResponse = ApiResponse<string>.CreateErrorResponse("Password reset failed.", new List<string>
             {
                 "Token is invalid.",
                 "Password too weak."
              });

            _mockUserService.Setup(s => s.ResetPasswordAsync(model))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;

            badRequest!.Value.Should().BeEquivalentTo(new
            {
                message = "Password reset failed.",
                errors = new List<string> { "Token is invalid.", "Password too weak." }
            });
        }

        [Test]
        public async Task ResetPassword_ServiceSuccess_ReturnsOk()
        {
            // Arrange
            var model = new ResetPasswordModelDto
            {
                Username = "user@example.com",
                Token = "valid-token",
                NewPassword = "StrongPass@123"
            };

            var apiResponse = ApiResponse<string>.CreateSuccessResponse("Password has been reset successfully.");

            _mockUserService.Setup(s => s.ResetPasswordAsync(model))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be("Password has been reset successfully.");
        }

        [Test]
        public async Task ResetPassword_ValidToken_ReturnsOk()
        {
            // Arrange
            var model = new ResetPasswordModelDto
            {
                Username = "existinguser",
                Token = "valid-token",
                NewPassword = "NewPassword123!"
            };

            var apiResponse = ApiResponse<string>.CreateSuccessResponse("Password has been reset successfully.");

            _mockUserService.Setup(s => s.ResetPasswordAsync(It.Is<ResetPasswordModelDto>(m => m.Token == model.Token)))
                            .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be("Password has been reset successfully.");
        }
        [Test]
        public async Task ResetPassword_InvalidToken_ReturnsBadRequest()
        {
            // Arrange
            var model = new ResetPasswordModelDto
            {
                Username = "existinguser",
                Token = "invalid-token",
                NewPassword = "NewPassword123!"
            };

            var apiResponse = ApiResponse<string>.CreateErrorResponse("Invalid token.");

            _mockUserService.Setup(s => s.ResetPasswordAsync(It.Is<ResetPasswordModelDto>(m => m.Token == model.Token)))
                            .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var json = JObject.FromObject(badRequestResult!.Value!);
            json["message"]!.ToString().Should().Be("Invalid token.");
        }
        [Test]
        public async Task ResetPassword_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var model = new ResetPasswordModelDto
            {
                Username = "nonexistentuser",
                Token = "valid-token",
                NewPassword = "NewPassword123!"
            };

            var apiResponse = ApiResponse<string>.CreateErrorResponse("User not found.");

            _mockUserService.Setup(s => s.ResetPasswordAsync(It.Is<ResetPasswordModelDto>(m => m.Username == model.Username)))
                            .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var json = JObject.FromObject(badRequestResult!.Value!);
            json["message"]!.ToString().Should().Be("User not found.");
        }
        [Test]
        public async Task ResetPassword_FailureOnPasswordReset_ReturnsBadRequestWithErrors()
        {
            // Arrange
            var model = new ResetPasswordModelDto
            {
                Username = "existinguser",
                Token = "valid-token",
                NewPassword = "NewPassword123!"
            };

            var apiResponse = ApiResponse<string>.CreateErrorResponse("Password reset failed.", new List<string> { "Token expired." });

            _mockUserService.Setup(s => s.ResetPasswordAsync(It.Is<ResetPasswordModelDto>(m => m.Token == model.Token)))
                            .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var json = JObject.FromObject(badRequestResult!.Value!);
            json["message"]!.ToString().Should().Be("Password reset failed.");
            var errors = json["errors"]!.ToObject<List<string>>();
            errors!.Should().Contain("Token expired.");
        }
        [Test]
        public async Task ResetPassword_EmptyModel_ReturnsBadRequest()
        {
            // Arrange
            var model = new ResetPasswordModelDto(); 

            var apiResponse = ApiResponse<string>.CreateErrorResponse("ResetPasswordModelDto cannot be null.");

            _mockUserService.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordModelDto>()))
                            .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var json = JObject.FromObject(badRequestResult!.Value!);
            json["message"]!.ToString().Should().Be("ResetPasswordModelDto cannot be null.");
        }


        [Test]
        public async Task ChangePassword_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var model = new ChangePasswordModelDto();

            var apiResponse = ApiResponse<string>.CreateErrorResponse("ChangePasswordModelDto is invalid.");

            _mockUserService.Setup(x => x.ChangePasswordAsync(model))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest!.Value.Should().Be("ChangePasswordModelDto is invalid.");
        }
        [Test]
        public async Task ChangePassword_ServiceFails_ReturnsBadRequestWithMessage()
        {
            // Arrange
            var model = new ChangePasswordModelDto
            {
                Username = "existinguser",
                OldPassword = "wrongpass",
                NewPassword = "NewPass@123"
            };

            var apiResponse = ApiResponse<string>.CreateErrorResponse("Old password is incorrect.");

            _mockUserService.Setup(x => x.ChangePasswordAsync(model))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest!.Value.Should().Be("Old password is incorrect.");
        }
        [Test]
        public async Task ChangePassword_ServiceSuccess_ReturnsOk()
        {
            // Arrange
            var model = new ChangePasswordModelDto
            {
                Username = "existinguser",
                OldPassword = "OldPass@123",
                NewPassword = "NewPass@123"
            };

            var apiResponse = ApiResponse<string>.CreateSuccessResponse("Password changed successfully.");

            _mockUserService.Setup(x => x.ChangePasswordAsync(model))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be("Password changed successfully.");
        }

        [Test]
        public async Task ChangePassword_InvalidOldPassword_ReturnsBadRequest()
        {
            // Arrange
            var model = new ChangePasswordModelDto
            {
                Username = "existinguser",
                OldPassword = "wrongOldPassword",
                NewPassword = "NewPassword123!"
            };

            var apiResponse = ApiResponse<string>.CreateErrorResponse("Old password is incorrect.");

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(x => x.ChangePasswordAsync(model))
                .ReturnsAsync(apiResponse);

            var controller = new AuthController(mockUserService.Object);

            // Act
            var result = await controller.ChangePassword(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Old password is incorrect.");
        }
        [Test]
        public async Task ChangePassword_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var model = new ChangePasswordModelDto
            {
                Username = "nonexistentuser",
                OldPassword = "OldPass123!",
                NewPassword = "NewPass123!"
            };

            var apiResponse = ApiResponse<string>.CreateErrorResponse("User not found.");

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(x => x.ChangePasswordAsync(model))
                .ReturnsAsync(apiResponse);

            var controller = new AuthController(mockUserService.Object);

            // Act
            var result = await controller.ChangePassword(model);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("User not found.");
        }
        [Test]
        public async Task ChangePassword_Success_ReturnsOk()
        {
            // Arrange
            var model = new ChangePasswordModelDto
            {
                Username = "existinguser",
                OldPassword = "OldPass123!",
                NewPassword = "NewPassword123!"
            };

            var apiResponse = ApiResponse<string>.CreateSuccessResponse("Password changed successfully.");

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(x => x.ChangePasswordAsync(model))
                .ReturnsAsync(apiResponse);

            var controller = new AuthController(mockUserService.Object);

            // Act
            var result = await controller.ChangePassword(model);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be("Password changed successfully.");
        }



    }
}
