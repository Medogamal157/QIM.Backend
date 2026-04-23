using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities;
using QIM.Domain.Entities.Identity;
using QIM.Persistence.Contexts;

namespace QIM.Persistence.Seeds;

public static class DbSeeder
{
    public static async Task SeedAsync(QimDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await RoleSeeder.SeedAsync(roleManager);
        await SeedSuperAdminAsync(userManager);
        await SeedUsersAsync(userManager);
        await DataSeeder.SeedAsync(context);
    }

    private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager)
    {
        if (await userManager.Users.AnyAsync(u => u.Email == "admin@qim.com"))
            return;

        var admin = new ApplicationUser
        {
            UserName = "admin@qim.com",
            Email = "admin@qim.com",
            FullName = "Super Admin",
            UserType = UserType.Admin,
            IsVerified = true,
            IsActive = true,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(admin, "Qim@Admin2024");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "SuperAdmin");
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var seedUsers = new (string Email, string FullName, UserType Type, string Role, string Password)[]
        {
            ("moderator@qim.com", "QIM Moderator", UserType.Admin, "Moderator", "Qim@Mod2024"),
            ("support@qim.com", "QIM Support", UserType.Admin, "Support", "Qim@Sup2024"),
            ("admin2@qim.com", "Admin User", UserType.Admin, "Admin", "Qim@Adm2024"),
            ("provider1@qim.com", "Ahmad Construction", UserType.Provider, "Provider", "Qim@Pro2024"),
            ("provider2@qim.com", "Sara Tech Solutions", UserType.Provider, "Provider", "Qim@Pro2024"),
            ("provider3@qim.com", "Khaled Real Estate", UserType.Provider, "Provider", "Qim@Pro2024"),
            ("provider4@qim.com", "Rania Medical", UserType.Provider, "Provider", "Qim@Pro2024"),
            ("provider5@qim.com", "Fadi Auto Services", UserType.Provider, "Provider", "Qim@Pro2024"),
            ("client1@qim.com", "Omar Client", UserType.Client, "Client", "Qim@Cli2024"),
            ("client2@qim.com", "Layla Client", UserType.Client, "Client", "Qim@Cli2024"),
            ("client3@qim.com", "Yazan Client", UserType.Client, "Client", "Qim@Cli2024"),
            ("client4@qim.com", "Nour Client", UserType.Client, "Client", "Qim@Cli2024"),
            ("client5@qim.com", "Hana Client", UserType.Client, "Client", "Qim@Cli2024"),
        };

        foreach (var (email, fullName, userType, role, password) in seedUsers)
        {
            if (await userManager.Users.AnyAsync(u => u.Email == email))
                continue;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                UserType = userType,
                IsVerified = true,
                IsActive = true,
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
    }
}
