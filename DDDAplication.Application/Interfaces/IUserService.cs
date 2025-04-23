using DDDAplication.Application.DTOs;
using DDDAplication.Domain.Entities;

namespace DDDAplication.Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<IEnumerable<UserDto>>> GetAllAsync();
        Task<ApiResponse<UserDto>> GetByIdAsync(string id);
        Task<ApiResponse<UserDto>> Delete(string id);
        Task<ApiResponse<UserDto>> Update(UserDto userDto);

        Task<ApiResponse<string>> RegisterUser(RegisterModelDto model);
        Task<ApiResponse<LoginResultDto>> LoginAsync(LoginModelDto model);
        Task<ApiResponse<RefreshTokenModelDto>> RefreshTokenAsync(RefreshTokenModelDto model);

        Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordModelDto model);
        Task<ApiResponse<UserDto>> FindByNameAsync(string username);
        Task<ApiResponse<UserDto>> GetProfileAsync(string userId);

        Task<ApiResponse<string>> SendResetLinkAsync(SendResetLinkModelDto model);
        Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordModelDto model);
        Task<ApiResponse<string>> ConfirmEmailAsync(ConfirmEmailModelDto model);
        Task<ApiResponse<List<string>>> GetUserRolesAsync(string userId);
        Task<ApiResponse<bool>> AssignRolesToUserAsync(string userId, List<string> roles);
        Task<ApiResponse<bool>> RemoveRolesFromUserAsync(string userId, List<string> roles);
    }
}
