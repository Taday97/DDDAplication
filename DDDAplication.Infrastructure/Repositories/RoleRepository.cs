using DDDAplication.Domain.Entities;
using DDDAplication.Domain.Interfaces;
using DDDAplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DDDAplication.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        // Add a new role to the database
        public async Task<Role> AddAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
            return role;
        }

        // Delete a role from the database
        public async Task<Role> DeleteAsync(string id)
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

        // Get all roles from the database
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        // Get a specific role by its ID
        public async Task<Role> GetByIdAsync(string id)
        {
            return await _context.Roles.FindAsync(id);
        }

        // Update an existing role
        public async Task<Role> UpdateAsync(Role role)
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
    }
}
