using AutoMapper;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using DDDAplication.Domain.Entities;
using DDDAplication.Domain.Interfaces;
using DDDAplication.Infrastructure.Helpers;
using DDDAplication.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DDDAplication.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository, IMapper mapper, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<ApiResponse<UserDto>> GetByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return ApiResponse<UserDto>.CreateErrorResponse($"User with id {id} not found.");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return ApiResponse<UserDto>.CreateSuccessResponse("User retrieved successfully.", userDto);
        }


        public async Task<ApiResponse<UserDto>> Delete(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return ApiResponse<UserDto>.CreateErrorResponse($"User with id {id} not found.");
            }

            await _userRepository.DeleteAsync(id);

            var userDto = _mapper.Map<UserDto>(user);

            return ApiResponse<UserDto>.CreateSuccessResponse("User deleted successfully.", userDto);
        }


        public async Task<ApiResponse<IEnumerable<UserDto>>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();

            if (users == null || !users.Any())
            {
                return ApiResponse<IEnumerable<UserDto>>.CreateErrorResponse("No users found.");
            }

            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);

            return ApiResponse<IEnumerable<UserDto>>.CreateSuccessResponse("Users retrieved successfully.", userDtos);
        }


        public async Task<ApiResponse<UserDto>> Update(UserDto userDto)
        {
            if (userDto == null)
            {
                return ApiResponse<UserDto>.CreateErrorResponse("User data cannot be null.");
            }

            var user = await _userRepository.GetByIdAsync(userDto.Id.ToString());
            if (user == null)
            {
                return ApiResponse<UserDto>.CreateErrorResponse($"User with id {userDto.Id} not found.");
            }

            _mapper.Map(userDto, user);

            await _userRepository.UpdateAsync(user);

            return ApiResponse<UserDto>.CreateSuccessResponse("User updated successfully.", userDto);
        }

        public async Task<ApiResponse<string>> RegisterUser(RegisterModelDto model)
        {
            var existingEmailUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmailUser != null)
                return ApiResponse<string>.CreateErrorResponse("This email is already registered.");

            var existingUsernameUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUsernameUser != null)
                return ApiResponse<string>.CreateErrorResponse("This username is already taken.");

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<string>.CreateErrorResponse("User registration failed.", errors);
            }

            return ApiResponse<string>.CreateSuccessResponse("User registered successfully.");
        }

        public async Task<ApiResponse<LoginResultDto>> LoginAsync(LoginModelDto model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return ApiResponse<LoginResultDto>.CreateErrorResponse("Invalid credentials.");
            }

            var token = JwtHelper.GenerateToken(user, _configuration);
            var roles = await _userManager.GetRolesAsync(user);

            var userDto = _mapper.Map<UserLoginResponseDto>(user);
            userDto.Roles = roles;

            var result = new LoginResultDto
            {
                Token = token,
                User = userDto
            };

            return ApiResponse<LoginResultDto>.CreateSuccessResponse("Login successful.", result);
        }

        public async Task<ApiResponse<RefreshTokenModelDto>> RefreshTokenAsync(RefreshTokenModelDto model)
        {
            var principal = JwtHelper.GetPrincipalFromExpiredToken(model.Token, _configuration);
            if (principal == null)
            {
                return ApiResponse<RefreshTokenModelDto>.CreateErrorResponse("Invalid token.");
            }

            var username = principal.Identity?.Name;
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return ApiResponse<RefreshTokenModelDto>.CreateErrorResponse("User not found.");
            }

            var newToken = JwtHelper.GenerateToken(user, _configuration);
            var response = new RefreshTokenModelDto { Token = newToken };

            return ApiResponse<RefreshTokenModelDto>.CreateSuccessResponse("Token refreshed successfully.", response);
        }


        public async Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordModelDto model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
                return ApiResponse<string>.CreateErrorResponse("User not found.");

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<string>.CreateErrorResponse("Password change failed.", errors);
            }

            return ApiResponse<string>.CreateSuccessResponse("Password changed successfully.");
        }

        public async Task<ApiResponse<UserDto>> FindByNameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return ApiResponse<UserDto>.CreateErrorResponse("Username cannot be empty.");
            }

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return ApiResponse<UserDto>.CreateErrorResponse("User not found.");
            }

            var userDto = _mapper.Map<UserDto>(user);

            return ApiResponse<UserDto>.CreateSuccessResponse("User found.", userDto);
        }

        public async Task<ApiResponse<UserDto>> GetProfileAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ApiResponse<UserDto>.CreateErrorResponse("User ID is required.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponse<UserDto>.CreateErrorResponse("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            var profileDto = _mapper.Map<UserDto>(user);

            return ApiResponse<UserDto>.CreateSuccessResponse("Profile retrieved successfully.", profileDto);
        }


        public async Task<ApiResponse<string>> SendResetLinkAsync(SendResetLinkModelDto model)
        {
            if (model == null)
                return ApiResponse<string>.CreateErrorResponse("SendResetLinkModelDto cannot be null.");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return ApiResponse<string>.CreateErrorResponse("Email not registered.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            //Send Email

            return ApiResponse<string>.CreateSuccessResponse("Reset link sent.");
        }

        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordModelDto model)
        {
            if (model == null)
                return ApiResponse<string>.CreateErrorResponse("ResetPasswordModelDto cannot be null.");

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
                return ApiResponse<string>.CreateErrorResponse("User not found.");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<string>.CreateErrorResponse("Password reset failed.", errors);
            }

            return ApiResponse<string>.CreateSuccessResponse("Password has been reset successfully.");
        }

        public async Task<ApiResponse<string>> ConfirmEmailAsync(ConfirmEmailModelDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Token))
                return ApiResponse<string>.CreateErrorResponse("Invalid request data.");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return ApiResponse<string>.CreateErrorResponse("User not found.");

            if (user.EmailConfirmed)
                return ApiResponse<string>.CreateSuccessResponse("Email already confirmed.");

            var result = await _userManager.ConfirmEmailAsync(user, model.Token);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<string>.CreateErrorResponse("Email confirmation failed.", errors);
            }

            return ApiResponse<string>.CreateSuccessResponse("Email confirmed successfully.");
        }


        public async Task<ApiResponse<bool>> AssignRolesToUserAsync(string userId, List<string> roles)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.CreateErrorResponse("User not found.");
            }

            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Count() != roles.Count)
            {
                return ApiResponse<bool>.CreateErrorResponse("One or more roles not found.");
            }

            var result = await _userManager.AddToRolesAsync(user, existingRoles);
            if (result.Succeeded)
                return ApiResponse<bool>.CreateSuccessResponse("Roles assigned successfully.", true);

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResponse<bool>.CreateErrorResponse($"Failed to assign roles: {errors}");

        }
        public async Task<ApiResponse<bool>> RemoveRolesFromUserAsync(string userId, List<string> roles)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.CreateErrorResponse("User not found.");
            }

            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Count() != roles.Count)
            {
                return ApiResponse<bool>.CreateErrorResponse("One or more roles not found.");
            }

            var result = await _userManager.RemoveFromRolesAsync(user, existingRoles);
            if (result.Succeeded)
                return ApiResponse<bool>.CreateSuccessResponse("Roles removed successfully.", true);

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResponse<bool>.CreateErrorResponse($"Failed to remove roles: {errors}");

        }

        public async Task<ApiResponse<List<string>>> GetUserRolesAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<List<string>>.CreateErrorResponse("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            return ApiResponse<List<string>>.CreateSuccessResponse("Roles retrieved successfully.", roles.ToList());
        }
       

    }
}
