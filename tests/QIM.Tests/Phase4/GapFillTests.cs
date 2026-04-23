using Microsoft.Extensions.DependencyInjection;
using QIM.Application.Common.Mappings;
using QIM.Application.DTOs.Business;
using QIM.Application.Features.Businesses;
using QIM.Application.Features.Reviews;
using QIM.Application.Features.Users;
using QIM.Application.Interfaces;
using QIM.Application.Interfaces.Repositories;
using QIM.Application.Interfaces.Services;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities;
using QIM.Domain.Entities.Identity;
using QIM.Infrastructure.Services;
using QIM.Persistence.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace QIM.Tests.Phase4;

[TestClass]
public class GapFillTests : TestBase
{
    private IUnitOfWork _uow = null!;
    private IMapper _mapper = null!;
    private UserManager<ApplicationUser> _userManager = null!;
    private string _ownerAId = null!;
    private string _ownerBId = null!;
    private int _activityId;
    private int _businessId;

    [TestInitialize]
    public async Task SetUp()
    {
        _builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        _builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        _builder.Services.AddAutoMapper(typeof(MappingProfile));

        _serviceProvider = _builder.Services.BuildServiceProvider();
        var context = GetDbContext();
        context.Database.EnsureCreated();

        _uow = _serviceProvider.GetRequiredService<IUnitOfWork>();
        _mapper = _serviceProvider.GetRequiredService<IMapper>();
        _userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed two users
        var ownerA = new ApplicationUser
        {
            UserName = "ownerA@test.com",
            Email = "ownerA@test.com",
            FullName = "Owner A",
            UserType = UserType.Provider,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userManager.CreateAsync(ownerA, "Test123!");
        _ownerAId = ownerA.Id;

        var ownerB = new ApplicationUser
        {
            UserName = "ownerB@test.com",
            Email = "ownerB@test.com",
            FullName = "Owner B",
            UserType = UserType.Provider,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userManager.CreateAsync(ownerB, "Test123!");
        _ownerBId = ownerB.Id;

        // Seed category + business
        var cat = new Activity { NameAr = "فئة", NameEn = "TestCat" };
        await _uow.Activities.AddAsync(cat);
        await _uow.SaveChangesAsync(CancellationToken.None);
        _activityId = cat.Id;

        var biz = new Business
        {
            NameAr = "مطعم",
            NameEn = "TestBiz",
            OwnerId = _ownerAId,
            ActivityId = _activityId,
            Status = BusinessStatus.Approved
        };
        await _uow.Businesses.AddAsync(biz);
        await _uow.SaveChangesAsync(CancellationToken.None);
        _businessId = biz.Id;
    }

    // ═══════════════════════════════════════════
    // ── 4.29: GetUserReviews (my reviews) ──
    // ═══════════════════════════════════════════

    [TestMethod]
    public async Task GetUserReviews_ReturnsOnlyMyReviews()
    {
        // Seed reviews by ownerA
        var review = new Review
        {
            BusinessId = _businessId,
            UserId = _ownerAId,
            Rating = 4,
            Comment = "Nice!",
            Status = ReviewStatus.Approved
        };
        await _uow.Reviews.AddAsync(review);
        await _uow.SaveChangesAsync(CancellationToken.None);

        var handler = new GetUserReviewsHandler(_uow, _mapper);
        var result = await handler.Handle(new GetUserReviewsQuery(_ownerAId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.TotalCount);
    }

    [TestMethod]
    public async Task GetUserReviews_DifferentUser_ReturnsEmpty()
    {
        // Seed reviews by ownerA only
        var review = new Review
        {
            BusinessId = _businessId,
            UserId = _ownerAId,
            Rating = 3,
            Status = ReviewStatus.Approved
        };
        await _uow.Reviews.AddAsync(review);
        await _uow.SaveChangesAsync(CancellationToken.None);

        var handler = new GetUserReviewsHandler(_uow, _mapper);
        var result = await handler.Handle(new GetUserReviewsQuery(_ownerBId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.TotalCount);
    }

    // ═══════════════════════════════════════════
    // ── 4.30: Provider Account ──
    // ═══════════════════════════════════════════

    [TestMethod]
    public async Task GetProviderAccount_ReturnsBusinessesAndStats()
    {
        // Update business to have reviews
        var biz = await _uow.Businesses.GetByIdAsync(_businessId);
        biz!.Rating = 4.5;
        biz.ReviewCount = 2;
        _uow.Businesses.Update(biz);
        await _uow.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProviderAccountHandler(_userManager, _uow, _mapper);
        var result = await handler.Handle(new GetProviderAccountQuery(_ownerAId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Owner A", result.Data!.FullName);
        Assert.AreEqual(1, result.Data.TotalBusinesses);
        Assert.AreEqual(2, result.Data.TotalReviews);
        Assert.AreEqual(4.5, result.Data.AverageRating, 0.01);
    }

    [TestMethod]
    public async Task GetProviderAccount_NoBusinesses_ReturnsZeroStats()
    {
        var handler = new GetProviderAccountHandler(_userManager, _uow, _mapper);
        var result = await handler.Handle(new GetProviderAccountQuery(_ownerBId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Data!.TotalBusinesses);
        Assert.AreEqual(0, result.Data.TotalReviews);
        Assert.AreEqual(0, result.Data.AverageRating);
    }

    [TestMethod]
    public async Task GetProviderAccount_UserNotFound_ReturnsFailure()
    {
        var handler = new GetProviderAccountHandler(_userManager, _uow, _mapper);
        var result = await handler.Handle(new GetProviderAccountQuery("non-existent"), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
    }

    // ═══════════════════════════════════════════
    // ── 4.31: User Verification (Admin) ──
    // ═══════════════════════════════════════════

    [TestMethod]
    public async Task VerifyUser_SetsIsVerifiedTrue()
    {
        var handler = new VerifyUserHandler(_userManager);
        var result = await handler.Handle(new VerifyUserCommand(_ownerAId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data!.IsVerified);

        var user = await _userManager.FindByIdAsync(_ownerAId);
        Assert.IsTrue(user!.IsVerified);
    }

    [TestMethod]
    public async Task RejectUserVerification_SetsIsVerifiedFalse()
    {
        // First verify
        var user = await _userManager.FindByIdAsync(_ownerAId);
        user!.IsVerified = true;
        await _userManager.UpdateAsync(user);

        var handler = new RejectUserVerificationHandler(_userManager);
        var result = await handler.Handle(new RejectUserVerificationCommand(_ownerAId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.Data!.IsVerified);
    }

    [TestMethod]
    public async Task VerifyUser_NotFound_ReturnsFailure()
    {
        var handler = new VerifyUserHandler(_userManager);
        var result = await handler.Handle(new VerifyUserCommand("non-existent"), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
    }

    // ═══════════════════════════════════════════
    // ── 4.35: Detailed Analytics ──
    // ═══════════════════════════════════════════

    [TestMethod]
    public async Task GetDetailedAnalytics_ReturnsAllSections()
    {
        // Seed a review for distribution
        var review = new Review
        {
            BusinessId = _businessId,
            UserId = _ownerBId,
            Rating = 5,
            Status = ReviewStatus.Approved
        };
        await _uow.Reviews.AddAsync(review);
        await _uow.SaveChangesAsync(CancellationToken.None);

        var handler = new GetDetailedAnalyticsHandler(_uow, _userManager);
        var result = await handler.Handle(new GetDetailedAnalyticsQuery(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data!.UserGrowth);
        Assert.AreEqual(6, result.Data.UserGrowth.Count); // Last 6 months
        Assert.IsNotNull(result.Data!.BusinessTrend);
        Assert.AreEqual(6, result.Data.BusinessTrend.Count);
        Assert.IsNotNull(result.Data!.TopActivities);
        Assert.IsTrue(result.Data.TopActivities.Count >= 1); // TestCat has 1 business
        Assert.IsNotNull(result.Data!.ReviewDistribution);
        Assert.AreEqual(1, result.Data.ReviewDistribution[5]); // 1 five-star review
        Assert.AreEqual(0, result.Data.ReviewDistribution[1]); // 0 one-star reviews
    }

    // ═══════════════════════════════════════════
    // ── 4.37: File Upload Test ──
    // ═══════════════════════════════════════════

    [TestMethod]
    public async Task FileUpload_MockIFormFile_ReturnsUrl()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"qim_test_{Guid.NewGuid():N}");
        try
        {
            var storage = new LocalFileStorageService(tempDir);
            var file = MockImageFormFile();
            using var stream = file.OpenReadStream();

            var url = await storage.UploadAsync(stream, file.FileName, "profile-images");

            Assert.IsNotNull(url);
            Assert.IsTrue(url.StartsWith("/uploads/profile-images/"));
            Assert.IsTrue(url.EndsWith("test-image.jpg"));

            // Verify file actually exists on disk
            var fullPath = Path.Combine(tempDir, url.TrimStart('/'));
            Assert.IsTrue(File.Exists(fullPath));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    // ═══════════════════════════════════════════
    // ── 4.49: Ownership Guard Test ──
    // ═══════════════════════════════════════════

    [TestMethod]
    public async Task UpdateBusiness_DifferentOwner_ReturnsFailure()
    {
        // Business belongs to ownerA; try to update as ownerB
        var handler = new UpdateBusinessHandler(_uow, _mapper);
        var result = await handler.Handle(
            new UpdateBusinessCommand(_businessId, new UpdateBusinessRequest
            {
                NameEn = "Hacked Name"
            }, _ownerBId), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("not authorized")));
    }

    [TestMethod]
    public async Task UpdateBusiness_SameOwner_Succeeds()
    {
        var handler = new UpdateBusinessHandler(_uow, _mapper);
        var result = await handler.Handle(
            new UpdateBusinessCommand(_businessId, new UpdateBusinessRequest
            {
                NameEn = "Updated By Owner"
            }, _ownerAId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Updated By Owner", result.Data!.NameEn);
    }
}
