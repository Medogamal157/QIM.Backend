using Microsoft.EntityFrameworkCore;
using QIM.Domain.Entities;

namespace QIM.Tests.Phase1;

/// <summary>
/// Task 1.39 — Verify global soft-delete query filter excludes deleted entities.
/// </summary>
[TestClass]
public class SoftDeleteFilterTests : TestBase
{
    [TestMethod]
    public async Task SoftDelete_ExcludesDeletedCountries()
    {
        var context = GetDbContext();

        context.Countries.Add(new Country { NameAr = "نشط", NameEn = "Active", SortOrder = 1 });
        context.Countries.Add(new Country { NameAr = "محذوف", NameEn = "Deleted", SortOrder = 2, IsDeleted = true });
        await context.SaveChangesAsync();

        // Default query should exclude deleted
        var countries = await context.Countries.ToListAsync();
        Assert.AreEqual(1, countries.Count);
        Assert.AreEqual("Active", countries[0].NameEn);
    }

    [TestMethod]
    public async Task SoftDelete_IgnoreQueryFilters_IncludesDeleted()
    {
        var context = GetDbContext();

        context.Countries.Add(new Country { NameAr = "نشط", NameEn = "Active", SortOrder = 1 });
        context.Countries.Add(new Country { NameAr = "محذوف", NameEn = "Deleted", SortOrder = 2, IsDeleted = true });
        await context.SaveChangesAsync();

        // IgnoreQueryFilters should include ALL
        var all = await context.Countries.IgnoreQueryFilters().ToListAsync();
        Assert.AreEqual(2, all.Count);
    }

    [TestMethod]
    public async Task SoftDelete_WorksForActivities()
    {
        var context = GetDbContext();

        context.Activities.Add(new Activity { NameAr = "نشط", NameEn = "Active" });
        context.Activities.Add(new Activity { NameAr = "محذوف", NameEn = "Deleted", IsDeleted = true });
        await context.SaveChangesAsync();

        var active = await context.Activities.ToListAsync();
        Assert.AreEqual(1, active.Count);

        var all = await context.Activities.IgnoreQueryFilters().ToListAsync();
        Assert.AreEqual(2, all.Count);
    }

    [TestMethod]
    public async Task SoftDelete_WorksForPlatformSettings()
    {
        var context = GetDbContext();

        context.PlatformSettings.Add(new PlatformSetting { Key = "Active", Value = "1" });
        context.PlatformSettings.Add(new PlatformSetting { Key = "Deleted", Value = "0", IsDeleted = true });
        await context.SaveChangesAsync();

        var active = await context.PlatformSettings.ToListAsync();
        Assert.AreEqual(1, active.Count);
        Assert.AreEqual("Active", active[0].Key);
    }

    [TestMethod]
    public async Task SoftDelete_CanSoftDeleteExistingEntity()
    {
        var context = GetDbContext();

        var country = new Country { NameAr = "بلد", NameEn = "TestCountry", SortOrder = 1 };
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        // Verify it exists
        Assert.AreEqual(1, await context.Countries.CountAsync());

        // Soft delete
        country.IsDeleted = true;
        await context.SaveChangesAsync();

        // Should no longer appear in normal queries
        Assert.AreEqual(0, await context.Countries.CountAsync());

        // But still in DB with IgnoreQueryFilters
        Assert.AreEqual(1, await context.Countries.IgnoreQueryFilters().CountAsync());
    }
}
