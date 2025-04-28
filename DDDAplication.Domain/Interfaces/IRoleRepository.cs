using DDDAplication.Domain.Entities;
using System.Linq.Expressions;

namespace DDDAplication.Domain.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(Guid id, bool asNoTracking = true);

        Task<IEnumerable<Role>> GetRolesAsync(Expression<Func<Role, bool>> filter = null, bool asNoTracking = true);

        Task<Role?> FindFirstRoleAsync(Expression<Func<Role, bool>> filter, bool asNoTracking = true);

        Task<Role> AddAsync(Role role);

        Task<Role?> UpdateAsync(Role role);

        Task<Role?> DeleteAsync(Guid id);
    }
}
