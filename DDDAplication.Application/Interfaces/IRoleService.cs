using DDDAplication.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Application.Interfaces
{
    public interface IRoleService
    {
        Task<ApiResponse<IEnumerable<RoleDto>>> GetAllAsync();
        Task<ApiResponse<RoleDto>> GetByIdAsync(string id);
        Task<ApiResponse<RoleDto>> AddAsync(RoleDto RoleDto);
        Task<ApiResponse<RoleDto>> Update(RoleDto RoleDto);
        Task<ApiResponse<RoleDto>> Delete(string id);
    }
}
