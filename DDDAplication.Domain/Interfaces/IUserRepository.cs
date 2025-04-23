using DDDAplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser> GetByIdAsync(string id);
        Task<ApplicationUser> AddAsync(ApplicationUser user);

        Task<ApplicationUser> UpdateAsync(ApplicationUser user);
        Task<ApplicationUser> DeleteAsync(string id);
        Task<bool> AssignRolesToUserAsync(ApplicationUser user, IEnumerable<Role> rolesEntities);
        Task<bool> RemoveRolesFromUserAsync(ApplicationUser user, IEnumerable<Role> rolesEntities);
    }
}