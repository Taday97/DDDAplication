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
        Task<IEnumerable<RoleDto>> GetAllAsync();
        Task<RoleDto> GetByIdAsync(string id);
        Task<RoleDto> AddAsync(RoleDto RoleDto);
        Task<RoleDto> Update(RoleDto RoleDto);
        Task<RoleDto> Delete(string id);
    }
}
