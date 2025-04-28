using DDDAplication.Application.DTOs.ApiResponse;
using DDDAplication.Application.DTOs.Rol;
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
        Task<ApiResponse<RoleDto>> AddAsync(CreateRoleDto RoleDto);
        Task<ApiResponse<RoleDto>> Update(EditRoleDto RoleDto);
        Task<ApiResponse<RoleDto>> Delete(string id);
    }
}
