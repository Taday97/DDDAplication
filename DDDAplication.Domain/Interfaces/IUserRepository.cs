using DDDAplication.Domain.Entities;
using System.Linq.Expressions;

namespace DDDAplication.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync(bool asNoTracking = true);

        Task<ApplicationUser> GetByIdAsync(string id, bool asNoTracking = true);

        Task<ApplicationUser> AddAsync(ApplicationUser user);

        Task<ApplicationUser> UpdateAsync(ApplicationUser user);

        Task<ApplicationUser> DeleteAsync(string id);

        Task<bool> AssignRolesToUserAsync(ApplicationUser user, IEnumerable<Role> rolesEntities);

        Task<bool> RemoveRolesFromUserAsync(ApplicationUser user, IEnumerable<Role> rolesEntities);

        Task<IEnumerable<ApplicationUser>> GetUsersAsync(Expression<Func<ApplicationUser, bool>> filter = null, bool asNoTracking = true);

        Task<ApplicationUser?> FindFirstUserAsync(Expression<Func<ApplicationUser, bool>> filter, bool asNoTracking = true);
    }
}
