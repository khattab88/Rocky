using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rocky.Models;
using Rocky.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Data.Seed
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            // 1. Apply any pending migrations
            try
            {
                if (_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // TODO: log exceptions
                throw;
            }

            // 2. Create roles
            if (!_roleManager.RoleExistsAsync(Constants.Roles.AdminRole).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Constants.Roles.AdminRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Constants.Roles.CustomerRole)).GetAwaiter().GetResult();
            }
            else
            {
                return;
            }

            // 3. Create App user (super admin)
            var result = _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@rocky.com",
                    Email = "admin@rocky.com",
                    EmailConfirmed = true,
                    FullName = "Admin",
                    PhoneNumber = "1234567890",
                }, "Admin@123")
            .GetAwaiter().GetResult();
            
            ApplicationUser admin = _context.ApplicationUSer.FirstOrDefault(u => u.Email == "admin@rocky.com");
            _userManager.AddToRoleAsync(admin, Constants.Roles.AdminRole).GetAwaiter().GetResult();
        }
    }
}
