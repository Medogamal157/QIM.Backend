using Microsoft.Extensions.DependencyInjection;
using QIM.Application.Common.Mappings;
using QIM.Application.DTOs.Business;
using QIM.Application.DTOs.Activity;
using QIM.Application.DTOs.Location;
using QIM.Application.Features.Businesses;
using QIM.Application.Features.Activities;
using QIM.Application.Features.Countries;
using QIM.Application.Features.Reviews;
using QIM.Application.Features.Users;
using QIM.Application.Interfaces;
using QIM.Application.Interfaces.Repositories;
using QIM.Application.DTOs.Auth;
using QIM.Application.Interfaces.Auth;
using QIM.Application.Interfaces.Services;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities;
using QIM.Domain.Entities.Identity;
using QIM.Infrastructure.Services.Auth;
using QIM.Persistence.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace QIM.Tests.Phase8;

[TestClass]
public class ModerationTests : TestBase
{
    private IUnitOfWork _uow = null!;
    private IMapper _mapper = null!;
    private UserManager<ApplicationUser> _userManager = null!;
    private string _ownerId = null!;
    private int _activityId;

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

        var user = new ApplicationUser
        {
            UserName = "mod@test.com",
            Email = "mod@test.com",
            FullName = "Mod User",
            PhoneNumber = "0790000000",
            UserType = UserType.Provider,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userManager.CreateAsync(user, "Test123!");
        _ownerId = user.Id;

        var cat = new Activity { NameAr = "فئة", NameEn = "TestCat", IsEnabled = true };
        await _uow.Activities.AddAsync(cat);
        await _uow.SaveChangesAsync(CancellationToken.None);
        _activityId = cat.Id;
    }

    // ── 8.13: Approved business appears in search, Pending/Rejected do not ──

    [TestMethod]
    public async Task SearchBusinesses_PendingDoesNotAppear()
    {
        await _uow.Businesses.AddAsync(new Business
        {
            NameAr = "قيد المراجعة",
            NameEn = "PendingBiz",
            OwnerId = _ownerId,
            ActivityId = _activityId,
            Status = BusinessStatus.Pending
        });
        await _uow.SaveChangesAsync(CancellationToken.None);

        var handler = new SearchBusinessesHandler(_uow, _mapper);
        var result = await handler.Handle(new SearchBusinessesQuery(Keyword: "PendingBiz"), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.TotalCount);
    }

    [TestMethod]
    public async Task SearchBusinesses_RejectedDoesNotAppear()
    {
        await _uow.Businesses.AddAsync(new Business
        {
            NameAr = "مرفوض",
            NameEn = "RejectedBiz",
            OwnerId = _ownerId,
            ActivityId = _activityId,
            Status = BusinessStatus.Rejected,
            RejectionReason = "Incomplete"
        });
        await _uow.SaveChangesAsync(CancellationToken.None);

        var handler = new SearchBusinessesHandler(_uow, _mapper);
        var result = await handler.Handle(new SearchBusinessesQuery(Keyword: "RejectedBiz"), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.TotalCount);
    }

    [TestMethod]
    public async Task SearchBusinesses_ApprovedAppears()
    {
        await _uow.Businesses.AddAsync(new Business
        {
            NameAr = "مقبول",
            NameEn = "ApprovedBiz",
            OwnerId = _ownerId,
            ActivityId = _activityId,
            Status = BusinessStatus.Approved
        });
        await _uow.SaveChangesAsync(CancellationToken.None);

        var handler = new SearchBusinessesHandler(_uow, _mapper);
        var result = await handler.Handle(new SearchBusinessesQuery(Keyword: "ApprovedBiz"), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.TotalCount >= 1);
    }

    // ── 8.14: Disabled activity excluded from public list ──

    [TestMethod]
    public async Task PublicActivityTree_ExcludesDisabledActivities()
    {
        // Create an enabled root
        var enabledCat = new Activity { NameAr = "مفعّل", NameEn = "EnabledRoot", IsEnabled = true };
        await _uow.Activities.AddAsync(enabledCat);
        // Create a disabled root
        var disabledCat = new Activity { NameAr = "معطّل", NameEn = "DisabledRoot", IsEnabled = false };
        await _uow.Activities.AddAsync(disabledCat);
        await _uow.SaveChangesAsync(CancellationToken.None);

        var handler = new GetPublicActivityTreeHandler(_uow, _mapper);
        var result = await handler.Handle(new GetPublicActivityTreeQuery(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data!.Any(c => c.NameEn == "EnabledRoot"));
        Assert.IsFalse(result.Data!.Any(c => c.NameEn == "DisabledRoot"));
    }

    [TestMethod]
    public async Task PublicCountries_ExcludesDisabledCountries()
    {
        var enabled = new Country { NameAr = "مفعّل", NameEn = "EnabledCountry", IsEnabled = true };
        await _uow.Countries.AddAsync(enabled);
        var disabled = new Country { NameAr = "معطّل", NameEn = "DisabledCountry", IsEnabled = false };
        await _uow.Countries.AddAsync(disabled);
        await _uow.SaveChangesAsync(CancellationToken.None);

        var handler = new GetPublicCountriesHandler(_uow, _mapper);
        var result = await handler.Handle(new GetPublicCountriesQuery(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data!.Any(c => c.NameEn == "EnabledCountry"));
        Assert.IsFalse(result.Data!.Any(c => c.NameEn == "DisabledCountry"));
    }

    // ── 8.15: Disabled user cannot login ──

    [TestMethod]
    public async Task DisabledUser_CannotLogin()
    {
        // Seed required roles
        var roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Client", "Provider", "Admin", "SuperAdmin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
        await _userManager.AddToRoleAsync(await _userManager.FindByIdAsync(_ownerId)!, "Provider");

        // Disable the user
        var toggleHandler = new ToggleUserActiveHandler(_userManager);
        var toggleResult = await toggleHandler.Handle(new ToggleUserActiveCommand(_ownerId), CancellationToken.None);
        Assert.IsTrue(toggleResult.IsSuccess);
        Assert.IsFalse(toggleResult.Data!.IsActive);

        // Attempt login via AuthService
        var authService = _serviceProvider.GetRequiredService<IAuthService>();
        var loginResult = await authService.LoginAsync(new LoginRequest { Email = "mod@test.com", Password = "Test123!" });

        Assert.IsFalse(loginResult.IsSuccess);
        Assert.IsTrue(loginResult.Errors.Any(e => e.Contains("deactivated")));
    }

    // ── 8.16: Rejected review excluded from business reviews ──

    [TestMethod]
    public async Task RejectedReview_ExcludedFromBusinessReviews()
    {
        // Create an approved business
        var biz = new Business
        {
            NameAr = "أعمال",
            NameEn = "BizForReview",
            OwnerId = _ownerId,
            ActivityId = _activityId,
            Status = BusinessStatus.Approved
        };
        await _uow.Businesses.AddAsync(biz);
        await _uow.SaveChangesAsync(CancellationToken.None);

        // Create a reviewer user
        var reviewer = new ApplicationUser
        {
            UserName = "reviewer@test.com",
            Email = "reviewer@test.com",
            FullName = "Reviewer",
            UserType = UserType.Client,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userManager.CreateAsync(reviewer, "Test123!");

        // Create and approve a review
        var createHandler = new CreateReviewHandler(_uow, _mapper);
        var review = await createHandler.Handle(
            new CreateReviewCommand(new CreateReviewRequest
            {
                BusinessId = biz.Id,
                Rating = 5,
                Comment = "Good"
            }, reviewer.Id), CancellationToken.None);

        // Reject it
        var rejectHandler = new RejectReviewHandler(_uow, _mapper);
        await rejectHandler.Handle(new RejectReviewCommand(review.Data!.Id), CancellationToken.None);

        // Fetch business reviews (public) - should not include rejected
        var getHandler = new GetBusinessReviewsHandler(_uow, _mapper);
        var result = await getHandler.Handle(new GetBusinessReviewsQuery(biz.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.TotalCount);
    }

    // ── Rejection reason stored on reject ──

    [TestMethod]
    public async Task RejectBusiness_StoresRejectionReason()
    {
        var createHandler = new CreateBusinessHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateBusinessCommand(new CreateBusinessRequest
            {
                NameAr = "للرفض",
                NameEn = "RejectWithReason",
                ActivityId = _activityId
            }, _ownerId), CancellationToken.None);

        var rejectHandler = new RejectBusinessHandler(_uow, _mapper);
        var result = await rejectHandler.Handle(
            new RejectBusinessCommand(created.Data!.Id, "Missing license"), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(BusinessStatus.Rejected, result.Data!.Status);
        Assert.AreEqual("Missing license", result.Data!.RejectionReason);
    }
}
