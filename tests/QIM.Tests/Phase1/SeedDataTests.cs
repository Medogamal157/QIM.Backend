using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QIM.Domain.Entities.Identity;
using QIM.Persistence.Seeds;

namespace QIM.Tests.Phase1;

/// <summary>
/// Task 1.38 — Verify seed data runs correctly (roles, admin, countries etc).
/// </summary>
[TestClass]
public class SeedDataTests : TestBase
{
    [TestMethod]
    public async Task DbSeeder_Seeds_AllRoles()
    {
        var context = GetDbContext();
        var userManager = GetUserManager();
        var roleManager = GetRoleManager();

        await DbSeeder.SeedAsync(context, userManager, roleManager);

        var expectedRoles = new[] { "SuperAdmin", "Admin", "Moderator", "Support", "Client", "Provider" };
        foreach (var role in expectedRoles)
        {
            Assert.IsTrue(await roleManager.RoleExistsAsync(role), $"Role '{role}' should exist");
        }
    }

    [TestMethod]
    public async Task DbSeeder_Seeds_SuperAdmin()
    {
        var context = GetDbContext();
        var userManager = GetUserManager();
        var roleManager = GetRoleManager();

        await DbSeeder.SeedAsync(context, userManager, roleManager);

        var admin = await userManager.FindByEmailAsync("admin@qim.com");
        Assert.IsNotNull(admin);
        Assert.AreEqual("Super Admin", admin.FullName);
        Assert.IsTrue(admin.IsVerified);
        Assert.IsTrue(admin.IsActive);

        var isInRole = await userManager.IsInRoleAsync(admin, "SuperAdmin");
        Assert.IsTrue(isInRole);
    }

    [TestMethod]
    public async Task DbSeeder_Seeds_Countries()
    {
        var context = GetDbContext();
        var userManager = GetUserManager();
        var roleManager = GetRoleManager();

        await DbSeeder.SeedAsync(context, userManager, roleManager);

        var countries = await context.Countries.ToListAsync();
        Assert.IsTrue(countries.Count >= 6, "Should have at least 6 countries");

        var jordan = countries.FirstOrDefault(c => c.NameEn == "Jordan");
        Assert.IsNotNull(jordan);
        Assert.AreEqual("الأردن", jordan.NameAr);
        Assert.IsTrue(jordan.IsDefault);
    }

    [TestMethod]
    public async Task DbSeeder_Seeds_Cities()
    {
        var context = GetDbContext();
        var userManager = GetUserManager();
        var roleManager = GetRoleManager();

        await DbSeeder.SeedAsync(context, userManager, roleManager);

        var cities = await context.Cities.ToListAsync();
        Assert.IsTrue(cities.Count >= 12, "Should have at least 12 cities");

        var amman = cities.FirstOrDefault(c => c.NameEn == "Amman");
        Assert.IsNotNull(amman);
        Assert.AreEqual("عمّان", amman.NameAr);
    }

    [TestMethod]
    public async Task DbSeeder_Seeds_Districts()
    {
        var context = GetDbContext();
        var userManager = GetUserManager();
        var roleManager = GetRoleManager();

        await DbSeeder.SeedAsync(context, userManager, roleManager);

        var districts = await context.Districts.ToListAsync();
        Assert.IsTrue(districts.Count >= 10, "Should have at least 10 districts");
    }

    [TestMethod]
    public async Task DbSeeder_Seeds_Activities()
    {
        var context = GetDbContext();
        var userManager = GetUserManager();
        var roleManager = GetRoleManager();

        await DbSeeder.SeedAsync(context, userManager, roleManager);

        var activities = await context.Activities.ToListAsync();
        Assert.IsTrue(activities.Count >= 10, "Should have at least 10 activities");

        var construction = activities.FirstOrDefault(c => c.NameEn == "Construction");
        Assert.IsNotNull(construction);
        Assert.AreEqual("المقاولات والبناء", construction.NameAr);
    }

    [TestMethod]
    public async Task DbSeeder_Seeds_Specialities()
    {
        var context = GetDbContext();
        var userManager = GetUserManager();
        var roleManager = GetRoleManager();

        await DbSeeder.SeedAsync(context, userManager, roleManager);

        var specialities = await context.Specialities.ToListAsync();
        Assert.IsTrue(specialities.Count >= 3, "Should have at least 3 specialities");
    }

    [TestMethod]
    public async Task DbSeeder_Seeds_PlatformSettings()
    {
        var context = GetDbContext();
        var userManager = GetUserManager();
        var roleManager = GetRoleManager();

        await DbSeeder.SeedAsync(context, userManager, roleManager);

        var settings = await context.PlatformSettings.ToListAsync();
        Assert.IsTrue(settings.Count >= 8, "Should have at least 8 platform settings");

        var siteName = settings.FirstOrDefault(s => s.Key == "SiteName");
        Assert.IsNotNull(siteName);
    }

    [TestMethod]
    public async Task DbSeeder_IsIdempotent()
    {
        var context = GetDbContext();
        var userManager = GetUserManager();
        var roleManager = GetRoleManager();

        // Seeds twice
        await DbSeeder.SeedAsync(context, userManager, roleManager);
        await DbSeeder.SeedAsync(context, userManager, roleManager);

        // Should not duplicate
        var countries = await context.Countries.ToListAsync();
        Assert.IsTrue(countries.Count >= 6);
        Assert.IsTrue(countries.Count <= 12, "Countries should not be duplicated");

        var admins = await userManager.GetUsersInRoleAsync("SuperAdmin");
        Assert.AreEqual(1, admins.Count, "Only one SuperAdmin should exist");
    }
}
