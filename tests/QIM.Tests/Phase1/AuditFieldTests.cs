using Microsoft.EntityFrameworkCore;
using QIM.Domain.Entities;

namespace QIM.Tests.Phase1;

/// <summary>
/// Task 1.40 — Verify SaveChangesAsync auto-populates audit fields.
/// </summary>
[TestClass]
public class AuditFieldTests : TestBase
{
    [TestMethod]
    public async Task SaveChanges_SetsCreatedAt_OnAdd()
    {
        var context = GetDbContext();
        var before = DateTime.UtcNow;

        var country = new Country { NameAr = "أردن", NameEn = "Jordan", SortOrder = 1 };
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        Assert.IsTrue(country.CreatedAt >= before, "CreatedAt should be set on insert");
        Assert.IsTrue(country.CreatedAt <= DateTime.UtcNow, "CreatedAt should not be in the future");
    }

    [TestMethod]
    public async Task SaveChanges_SetsUpdatedAt_OnModify()
    {
        var context = GetDbContext();

        var country = new Country { NameAr = "أردن", NameEn = "Jordan", SortOrder = 1 };
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        Assert.IsNull(country.UpdatedAt, "UpdatedAt should be null after insert");

        // Modify
        await Task.Delay(10); // tiny delay so timestamps differ
        country.NameEn = "Jordan Updated";
        await context.SaveChangesAsync();

        Assert.IsNotNull(country.UpdatedAt, "UpdatedAt should be set after update");
        Assert.IsTrue(country.UpdatedAt > country.CreatedAt, "UpdatedAt should be after CreatedAt");
    }

    [TestMethod]
    public async Task SaveChanges_DoesNotChangeCreatedAt_OnModify()
    {
        var context = GetDbContext();

        var category = new Activity { NameAr = "بناء", NameEn = "Construction" };
        context.Activities.Add(category);
        await context.SaveChangesAsync();
        var originalCreatedAt = category.CreatedAt;

        await Task.Delay(10);
        category.NameEn = "Construction Updated";
        await context.SaveChangesAsync();

        Assert.AreEqual(originalCreatedAt, category.CreatedAt, "CreatedAt should not change on update");
    }

    [TestMethod]
    public async Task SaveChanges_AuditFields_NotSetForNonAuditableEntities()
    {
        var context = GetDbContext();

        // PlatformSetting extends BaseEntity (NOT BaseAuditableEntity)
        var setting = new PlatformSetting { Key = "TestKey", Value = "TestValue" };
        context.PlatformSettings.Add(setting);
        await context.SaveChangesAsync();

        // PlatformSetting has no CreatedAt/UpdatedAt properties — just verify it was saved
        var fromDb = await context.PlatformSettings.FirstAsync(s => s.Key == "TestKey");
        Assert.IsNotNull(fromDb);
        Assert.AreEqual("TestValue", fromDb.Value);
    }

    [TestMethod]
    public async Task SaveChanges_MultipleEntities_AllGetAuditFields()
    {
        var context = GetDbContext();
        var before = DateTime.UtcNow;

        var country = new Country { NameAr = "أردن", NameEn = "Jordan", SortOrder = 1 };
        var category = new Activity { NameAr = "بناء", NameEn = "Construction" };
        context.Countries.Add(country);
        context.Activities.Add(category);
        await context.SaveChangesAsync();

        Assert.IsTrue(country.CreatedAt >= before);
        Assert.IsTrue(category.CreatedAt >= before);
    }
}
