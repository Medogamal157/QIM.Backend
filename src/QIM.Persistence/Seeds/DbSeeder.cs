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
        var existing = await userManager.FindByEmailAsync("admin@qim.com");
        if (existing != null)
        {
            // DEF-NEW-001: ensure the seeded SuperAdmin always carries the SuperAdmin role
            // even when the user row pre-existed from an earlier (buggy) seed.
            var existingRoles = await userManager.GetRolesAsync(existing);
            if (!existingRoles.Contains("SuperAdmin"))
            {
                if (existingRoles.Count > 0)
                    await userManager.RemoveFromRolesAsync(existing, existingRoles);
                await userManager.AddToRoleAsync(existing, "SuperAdmin");
            }
            return;
        }

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
        // (Email, FullName, UserType, Role, Password, IsVerified, IsActive, IsDeleted, EmailConfirmed, ProfileImageUrl)
        var seedUsers = new (string Email, string FullName, UserType Type, string Role, string Password,
            bool IsVerified, bool IsActive, bool IsDeleted, bool EmailConfirmed, string? ProfileImageUrl)[]
        {
            // Active staff
            ("moderator@qim.com", "QIM Moderator", UserType.Admin, "Moderator", "Qim@Mod2024", true, true, false, true, null),
            ("support@qim.com",   "QIM Support",   UserType.Admin, "Support",   "Qim@Sup2024", true, true, false, true, null),
            ("admin2@qim.com",    "Admin User",    UserType.Admin, "Admin",     "Qim@Adm2024", true, true, false, true, "/images/users/admin2.png"),
            // Inactive admin (locked-out scenario)
            ("admin.inactive@qim.com", "Inactive Admin", UserType.Admin, "Admin", "Qim@Adm2024", true, false, false, true, null),

            // Providers (1-5 fully active & verified — own businesses)
            ("provider1@qim.com", "Ahmad Construction",  UserType.Provider, "Provider", "Qim@Pro2024", true, true, false, true, "/images/users/p1.png"),
            ("provider2@qim.com", "Sara Tech Solutions", UserType.Provider, "Provider", "Qim@Pro2024", true, true, false, true, null),
            ("provider3@qim.com", "Khaled Real Estate",  UserType.Provider, "Provider", "Qim@Pro2024", true, true, false, true, null),
            ("provider4@qim.com", "Rania Medical",       UserType.Provider, "Provider", "Qim@Pro2024", true, true, false, true, null),
            ("provider5@qim.com", "Fadi Auto Services",  UserType.Provider, "Provider", "Qim@Pro2024", true, true, false, true, null),
            // Provider — pending verification (account verification flow)
            ("provider.pending@qim.com", "Pending Provider", UserType.Provider, "Provider", "Qim@Pro2024", false, true,  false, false, null),
            // Provider — inactive (suspended)
            ("provider.inactive@qim.com", "Inactive Provider", UserType.Provider, "Provider", "Qim@Pro2024", true, false, false, true,  null),

            // Clients
            ("client1@qim.com", "Omar Client",  UserType.Client, "Client", "Qim@Cli2024", true, true, false, true, null),
            ("client2@qim.com", "Layla Client", UserType.Client, "Client", "Qim@Cli2024", true, true, false, true, "/images/users/c2.png"),
            ("client3@qim.com", "Yazan Client", UserType.Client, "Client", "Qim@Cli2024", true, true, false, true, null),
            ("client4@qim.com", "Nour Client",  UserType.Client, "Client", "Qim@Cli2024", true, true, false, true, null),
            ("client5@qim.com", "Hana Client",  UserType.Client, "Client", "Qim@Cli2024", true, true, false, true, null),
            // Client — unverified email
            ("client.pending@qim.com",  "Pending Client",  UserType.Client, "Client", "Qim@Cli2024", false, true, false, false, null),
            // Client — soft-deleted
            ("client.deleted@qim.com",  "Deleted Client",  UserType.Client, "Client", "Qim@Cli2024", true, false, true,  true, null),
        };

        foreach (var (email, fullName, userType, role, password, isVerified, isActive, isDeleted, emailConfirmed, profileImageUrl) in seedUsers)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                // DEF-NEW-001/002: ensure each seeded admin/provider/client carries exactly
                // the role we want, correcting any earlier mis-seed.
                var existingRoles = await userManager.GetRolesAsync(existing);
                if (!existingRoles.Contains(role))
                {
                    if (existingRoles.Count > 0)
                        await userManager.RemoveFromRolesAsync(existing, existingRoles);
                    await userManager.AddToRoleAsync(existing, role);
                }
                continue;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                UserType = userType,
                IsVerified = isVerified,
                IsActive = isActive,
                IsDeleted = isDeleted,
                EmailConfirmed = emailConfirmed,
                ProfileImageUrl = profileImageUrl,
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
    }
}
