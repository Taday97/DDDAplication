using Microsoft.EntityFrameworkCore;
using DDDAplication.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace DDDAplication.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, Role, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

        }
    }
}