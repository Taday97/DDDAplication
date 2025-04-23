using DDDAplication.Domain.Entities;
using DDDAplication.Domain.Interfaces;
using DDDAplication.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DDDAplication.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ApplicationUser> GetByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid parsedId))
            {
                return null;
            }

            return await _context.Users.FirstOrDefaultAsync(l => l.Id == parsedId);
        }


        public async Task<ApplicationUser> AddAsync(ApplicationUser user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<ApplicationUser> UpdateAsync(ApplicationUser user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with id {user.Id} not found.");
            }

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
            return existingUser;
        }

        public async Task<ApplicationUser> DeleteAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {id} not found.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> AssignRolesToUserAsync(ApplicationUser user, IEnumerable<Role> roles)
        {
            try
            {
                foreach (var role in roles)
                {
                    if (!await _userManager.IsInRoleAsync(user, role.Name))
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public async Task<bool> RemoveRolesFromUserAsync(ApplicationUser user, IEnumerable<Role> roles)
        {
            try
            {
                foreach (var role in roles)
                {
                    if (await _userManager.IsInRoleAsync(user, role.Name))
                    {
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}