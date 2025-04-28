using DDDAplication.Domain.Entities;
using DDDAplication.Domain.Interfaces;
using DDDAplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DDDAplication.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByIdAsync(Guid id, bool asNoTracking = true)
        {
            var query = _context.Roles.AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Role>> GetRolesAsync(Expression<Func<Role, bool>>? filter = null, bool asNoTracking = true)
        {
            IQueryable<Role> query = _context.Roles;

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync();
        }

        public async Task<Role?> FindFirstRoleAsync(Expression<Func<Role, bool>> filter, bool asNoTracking = true)
        {
            var query = _context.Roles.AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(filter);
        }

        public async Task<Role> AddAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role?> UpdateAsync(Role role)
        {
            var existingRole = await _context.Roles.FindAsync(role.Id);
            if (existingRole == null)
            {
                return null;
            }

            _context.Entry(existingRole).CurrentValues.SetValues(role);
            await _context.SaveChangesAsync();
            return existingRole;
        }

        public async Task<Role?> DeleteAsync(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return null;
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return role;
        }
    }
}
