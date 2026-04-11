using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnitOfWork.Core.Models;

namespace UnitOfWork.Infrastructure.DbContextClass
{
    public static class DbInitializer
    {
        public static void Initialize(CERDBContext _context, UserManager<AppUser> _userManager, RoleManager<AppRole> _roleManager)
        {
            if (!_userManager.Users.Any())
            {
                SeedUsers(_userManager);
            }

            if (!_roleManager.Roles.Any())
            {
                SeedRoles(_roleManager);
            }

            if (!_context.UserRoles.Any())
            {
                SeedUserRoles(_userManager, _roleManager);
            }
        }
        private static void SeedUsers(UserManager<AppUser> _userManager)
        {
            var password = "abc@123";
            var user = new AppUser()
            {
                FirstName = "sheryar",
                LastName = "khan",
                UserName = "admin"
            };

            _userManager.CreateAsync(user, password).Wait();
            _userManager.SetLockoutEnabledAsync(user, false);
            _userManager.ResetAccessFailedCountAsync(user);
        }

        private static void SeedRoles(RoleManager<AppRole> _roleManager)
        {
            var role = new AppRole()
            {
                Description = "System administrator",
                Name = "superadmin"
            };
            _roleManager.CreateAsync(role).Wait();
        }

        private static void SeedUserRoles(UserManager<AppUser> _userManager, RoleManager<AppRole> _roleManager)
        {
            var user = _userManager.FindByNameAsync("admin").Result;
            var role = _roleManager.FindByNameAsync("superadmin").Result;
            if (user != null && role != null)
            {
                _userManager.AddToRoleAsync(user, role.Name).Wait();
            }
        }
    }
}
