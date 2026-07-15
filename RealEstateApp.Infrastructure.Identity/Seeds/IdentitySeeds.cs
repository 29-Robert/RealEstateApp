using Microsoft.AspNetCore.Identity;
using RealEstateApp.Domain.Enums; // Para usar tu Enum de Roles
using RealEstateApp.Infrastructure.Identity.Entities;
using System.Data;

namespace RealEstateApp.Infrastructure.Identity.Seeds
{
    public class IdentitySeeds : IIdentitySeeds
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentitySeeds(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            // 1. Sembrado de Roles
            foreach (Roles role in Enum.GetValues(typeof(Roles))) 
            {
                string roleName = role.ToString();
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Sembrado de Usuario Administrador por defecto
            var defaultAdmin = new ApplicationUser
            {
                UserName = "adminuser",
                Email = "admin@realestate.com",
                FirstName = "Pedro",
                LastName = "Rosario",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true
            };

            if (await _userManager.FindByEmailAsync(defaultAdmin.Email) == null)
            {
                var result = await _userManager.CreateAsync(defaultAdmin, "Admin123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(defaultAdmin, Roles.Administrador.ToString());
                }
            }

            // 3. Sembrado de Usuario Desarrollador por defecto
            var defaultDev = new ApplicationUser
            {
                UserName = "devuser",
                Email = "developer@realestate.com",
                FirstName = "Emmanuel",
                LastName = "Sánchez",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true
            };

            if (await _userManager.FindByEmailAsync(defaultDev.Email) == null)
            {
                var result = await _userManager.CreateAsync(defaultDev, "Admin123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(defaultDev, Roles.Desarrollador.ToString()); 
                }
            }
        }
    }
}