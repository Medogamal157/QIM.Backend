using QIM.Domain.Common;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities;
using QIM.Domain.Entities.Identity;

namespace QIM.Tests.Phase1;

/// <summary>
/// Task 1.36 — Verify every entity can be instantiated and default values are correct.
/// </summary>
[TestClass]
public class EntityInstantiationTests
{
    [TestMethod]
    public void Country_DefaultValues_AreCorrect()
    {
        var entity = new Country();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.IsNull(entity.NameAr);
        Assert.IsNull(entity.NameEn);
        Assert.IsFalse(entity.IsDefault);
        Assert.AreEqual(0, entity.SortOrder);
        Assert.IsNotNull(entity.Cities);
    }

    [TestMethod]
    public void City_DefaultValues_AreCorrect()
    {
        var entity = new City();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.IsNull(entity.NameAr);
        Assert.IsNull(entity.NameEn);
        Assert.AreEqual(0, entity.CountryId);
        Assert.IsNotNull(entity.Districts);
    }

    [TestMethod]
    public void District_DefaultValues_AreCorrect()
    {
        var entity = new District();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.IsNull(entity.NameAr);
        Assert.IsNull(entity.NameEn);
        Assert.AreEqual(0, entity.CityId);
    }

    [TestMethod]
    public void Activity_DefaultValues_AreCorrect()
    {
        var entity = new Activity();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.IsNull(entity.NameAr);
        Assert.IsNull(entity.NameEn);
        Assert.IsNull(entity.ParentActivityId);
        Assert.IsNotNull(entity.SubActivities);
        Assert.IsNotNull(entity.Specialities);
    }

    [TestMethod]
    public void Speciality_DefaultValues_AreCorrect()
    {
        var entity = new Speciality();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.IsNull(entity.NameAr);
        Assert.IsNull(entity.NameEn);
        Assert.AreEqual(0, entity.ActivityId);
    }

    [TestMethod]
    public void Business_DefaultValues_AreCorrect()
    {
        var entity = new Business();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.AreEqual(BusinessStatus.Pending, entity.Status);
        Assert.AreEqual(0.0, entity.Rating);
        Assert.AreEqual(0, entity.ReviewCount);
        Assert.IsNotNull(entity.Addresses);
        Assert.IsNotNull(entity.WorkHours);
        Assert.IsNotNull(entity.Images);
        Assert.IsNotNull(entity.Reviews);
        Assert.IsNotNull(entity.Claims);
    }

    [TestMethod]
    public void BusinessAddress_DefaultValues_AreCorrect()
    {
        var entity = new BusinessAddress();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.IsFalse(entity.IsPrimary);
    }

    [TestMethod]
    public void BusinessWorkHours_DefaultValues_AreCorrect()
    {
        var entity = new BusinessWorkHours();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
    }

    [TestMethod]
    public void BusinessImage_DefaultValues_AreCorrect()
    {
        var entity = new BusinessImage();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.IsFalse(entity.IsCover);
        Assert.AreEqual(0, entity.SortOrder);
    }

    [TestMethod]
    public void Review_DefaultValues_AreCorrect()
    {
        var entity = new Review();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.AreEqual(ReviewStatus.Pending, entity.Status);
    }

    [TestMethod]
    public void BlogPost_DefaultValues_AreCorrect()
    {
        var entity = new BlogPost();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.AreEqual(BlogPostStatus.Draft, entity.Status);
    }

    [TestMethod]
    public void ContactRequest_DefaultValues_AreCorrect()
    {
        var entity = new ContactRequest();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.AreEqual(ContactStatus.New, entity.Status);
    }

    [TestMethod]
    public void Suggestion_DefaultValues_AreCorrect()
    {
        var entity = new Suggestion();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.AreEqual(SuggestionStatus.New, entity.Status);
    }

    [TestMethod]
    public void BusinessClaim_DefaultValues_AreCorrect()
    {
        var entity = new BusinessClaim();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.AreEqual(ClaimStatus.Pending, entity.Status);
    }

    [TestMethod]
    public void PlatformSetting_DefaultValues_AreCorrect()
    {
        var entity = new PlatformSetting();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
    }

    [TestMethod]
    public void Advertisement_DefaultValues_AreCorrect()
    {
        var entity = new Advertisement();
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsDeleted);
        Assert.IsTrue(entity.IsActive);
    }

    [TestMethod]
    public void ApplicationUser_DefaultValues_AreCorrect()
    {
        var user = new ApplicationUser();
        Assert.IsFalse(user.IsVerified);
        Assert.IsTrue(user.IsActive);
        Assert.AreEqual(UserType.Client, user.UserType);
        Assert.IsNotNull(user.Businesses);
        Assert.IsNotNull(user.Reviews);
    }

    [TestMethod]
    public void RefreshToken_DefaultValues_AreCorrect()
    {
        var token = new RefreshToken();
        Assert.AreEqual(0, token.Id);
        Assert.IsFalse(token.IsRevoked);
    }

    [TestMethod]
    public void BaseEntity_SoftDelete_DefaultFalse()
    {
        var entity = new Country();
        Assert.IsFalse(entity.IsDeleted);
        entity.IsDeleted = true;
        Assert.IsTrue(entity.IsDeleted);
    }

    [TestMethod]
    public void BaseAuditableEntity_AuditFields_DefaultNull()
    {
        var entity = new Country();
        Assert.AreEqual(default(DateTime), entity.CreatedAt);
        Assert.IsNull(entity.CreatedBy);
        Assert.IsNull(entity.UpdatedAt);
        Assert.IsNull(entity.UpdatedBy);
    }

    [TestMethod]
    public void Enums_HaveExpectedValues()
    {
        Assert.AreEqual(0, (int)BusinessStatus.Pending);
        Assert.AreEqual(1, (int)BusinessStatus.Approved);

        Assert.AreEqual(0, (int)UserType.Client);
        Assert.AreEqual(1, (int)UserType.Provider);
        Assert.AreEqual(2, (int)UserType.Admin);

        Assert.AreEqual(0, (int)ReviewStatus.Pending);
        Assert.AreEqual(0, (int)ClaimStatus.Pending);
        Assert.AreEqual(0, (int)ContactStatus.New);
        Assert.AreEqual(0, (int)SuggestionStatus.New);
        Assert.AreEqual(0, (int)BlogPostStatus.Draft);
    }
}
