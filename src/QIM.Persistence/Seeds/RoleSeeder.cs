using Microsoft.AspNetCore.Identity;

namespace QIM.Persistence.Seeds;

public static class RoleSeeder
{
    private static readonly string[] Roles = { "SuperAdmin", "Admin", "Moderator", "Support", "Client", "Provider" };

    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
