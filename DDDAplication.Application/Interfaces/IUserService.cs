using DDDAplication.Application.DTOs;
using DDDAplication.Domain.Entities;

namespace DDDAplication.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto> GetByIdAsync(string id);
        Task<UserDto> Update(UserDto UserDto);
        Task<UserDto> Delete(string id);
        
    }
}
