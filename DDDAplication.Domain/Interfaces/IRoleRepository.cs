using DDDAplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Domain.Interfaces
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role> GetByIdAsync(string id);
        Task<Role> AddAsync(Role user);

        Task<Role> UpdateAsync(Role user);
        Task<Role> DeleteAsync(string id);
    }
}
