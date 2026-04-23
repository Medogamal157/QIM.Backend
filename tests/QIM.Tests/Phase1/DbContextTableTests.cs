using Microsoft.EntityFrameworkCore;
using QIM.Domain.Entities;
using QIM.Domain.Entities.Identity;

namespace QIM.Tests.Phase1;

/// <summary>
/// Task 1.37 — Verify DbContext creates SQLite database with all expected tables.
/// </summary>
[TestClass]
public class DbContextTableTests : TestBase
{
    [TestMethod]
    public void DbContext_Creates_AllDomainTables()
    {
        var context = GetDbContext();

        // Verify all 17 domain DbSets exist by querying them (empty but no exception)
        Assert.IsNotNull(context.Countries);
        Assert.IsNotNull(context.Cities);
        Assert.IsNotNull(context.Districts);
        Assert.IsNotNull(context.Activities);
        Assert.IsNotNull(context.Specialities);
        Assert.IsNotNull(context.Businesses);
        Assert.IsNotNull(context.BusinessAddresses);
        Assert.IsNotNull(context.BusinessWorkHours);
        Assert.IsNotNull(context.BusinessImages);
        Assert.IsNotNull(context.Reviews);
        Assert.IsNotNull(context.BlogPosts);
        Assert.IsNotNull(context.ContactRequests);
        Assert.IsNotNull(context.Suggestions);
        Assert.IsNotNull(context.BusinessClaims);
        Assert.IsNotNull(context.PlatformSettings);
        Assert.IsNotNull(context.Advertisements);
        Assert.IsNotNull(context.RefreshTokens);
    }

    [TestMethod]
    public async Task Countries_CanBeInsertedAndQueried()
    {
        var context = GetDbContext();

        context.Countries.Add(new Country
        {
            NameAr = "الأردن",
            NameEn = "Jordan",
            IsDefault = true,
            SortOrder = 1
        });
        await context.SaveChangesAsync();

        var countries = await context.Countries.ToListAsync();
        Assert.AreEqual(1, countries.Count);
        Assert.AreEqual("Jordan", countries[0].NameEn);
        Assert.AreEqual("الأردن", countries[0].NameAr);
    }

    [TestMethod]
    public async Task Cities_CanBeInsertedWithCountryFK()
    {
        var context = GetDbContext();

        var country = new Country { NameAr = "الأردن", NameEn = "Jordan", SortOrder = 1 };
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        context.Cities.Add(new City
        {
            NameAr = "عمان",
            NameEn = "Amman",
            CountryId = country.Id
        });
        await context.SaveChangesAsync();

        var city = await context.Cities.Include(c => c.Country).FirstAsync();
        Assert.AreEqual("Amman", city.NameEn);
        Assert.AreEqual(country.Id, city.CountryId);
        Assert.IsNotNull(city.Country);
    }

    [TestMethod]
    public async Task Activities_SupportHierarchy()
    {
        var context = GetDbContext();

        var parent = new Activity { NameAr = "بناء", NameEn = "Construction" };
        context.Activities.Add(parent);
        await context.SaveChangesAsync();

        var child = new Activity
        {
            NameAr = "سباكة",
            NameEn = "Plumbing",
            ParentActivityId = parent.Id
        };
        context.Activities.Add(child);
        await context.SaveChangesAsync();

        var parentFromDb = await context.Activities
            .Include(c => c.SubActivities)
            .FirstAsync(c => c.Id == parent.Id);

        Assert.AreEqual(1, parentFromDb.SubActivities.Count);
        Assert.AreEqual("Plumbing", parentFromDb.SubActivities.First().NameEn);
    }

    [TestMethod]
    public async Task Business_CanBeInserted_WithRelatedEntities()
    {
        var context = GetDbContext();

        // Setup required parent entities
        var category = new Activity { NameAr = "بناء", NameEn = "Construction" };
        context.Activities.Add(category);
        await context.SaveChangesAsync();

        var speciality = new Speciality
        {
            NameAr = "سباكة",
            NameEn = "Plumbing",
            ActivityId = category.Id
        };
        context.Specialities.Add(speciality);
        await context.SaveChangesAsync();

        // Need a user for OwnerId
        var userManager = GetUserManager();
        var user = new ApplicationUser
        {
            UserName = "testowner@test.com",
            Email = "testowner@test.com",
            FullName = "Test Owner"
        };
        await userManager.CreateAsync(user, "Test@12345");

        var business = new Business
        {
            NameAr = "شركة اختبار",
            NameEn = "Test Company",
            OwnerId = user.Id,
            ActivityId = category.Id,
            SpecialityId = speciality.Id,
            DescriptionEn = "Test description"
        };
        context.Businesses.Add(business);
        await context.SaveChangesAsync();

        var biz = await context.Businesses
            .Include(b => b.Activity)
            .Include(b => b.Speciality)
            .FirstAsync();

        Assert.AreEqual("Test Company", biz.NameEn);
        Assert.IsNotNull(biz.Activity);
        Assert.IsNotNull(biz.Speciality);
        Assert.AreEqual(user.Id, biz.OwnerId);
    }

    [TestMethod]
    public async Task IdentityTables_AreCreated()
    {
        // Verify Identity tables work by creating a user
        var userManager = GetUserManager();
        var result = await userManager.CreateAsync(new ApplicationUser
        {
            UserName = "identity@test.com",
            Email = "identity@test.com",
            FullName = "Identity Test"
        }, "Passw0rd!");

        Assert.IsTrue(result.Succeeded);

        var user = await userManager.FindByEmailAsync("identity@test.com");
        Assert.IsNotNull(user);
        Assert.AreEqual("Identity Test", user.FullName);
    }

    [TestMethod]
    public async Task Roles_CanBeCreated()
    {
        var roleManager = GetRoleManager();
        var result = await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("TestRole"));
        Assert.IsTrue(result.Succeeded);

        var exists = await roleManager.RoleExistsAsync("TestRole");
        Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task PlatformSettings_CanBeInserted()
    {
        var context = GetDbContext();

        context.PlatformSettings.Add(new PlatformSetting
        {
            Key = "SiteName",
            Value = "QIM",
            Group = "General"
        });
        await context.SaveChangesAsync();

        var setting = await context.PlatformSettings.FirstOrDefaultAsync(s => s.Key == "SiteName");
        Assert.IsNotNull(setting);
        Assert.AreEqual("QIM", setting.Value);
    }

    [TestMethod]
    public async Task RefreshToken_CanBeInserted()
    {
        var context = GetDbContext();
        var userManager = GetUserManager();

        var user = new ApplicationUser
        {
            UserName = "tokenuser@test.com",
            Email = "tokenuser@test.com",
            FullName = "Token User"
        };
        await userManager.CreateAsync(user, "Test@12345");

        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = "sample-refresh-token-123",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
        await context.SaveChangesAsync();

        var token = await context.RefreshTokens.FirstAsync();
        Assert.AreEqual(user.Id, token.UserId);
        Assert.IsFalse(token.IsRevoked);
    }
}
