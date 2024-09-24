using DDDAplication.Application.DTOs;
using DDDAplication.Domain.Entities;

namespace DDDAplication.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> GetByIdAsync(int id);
        Task<User> AddAsync(UserDto UserDto);
    }
}
