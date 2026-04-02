using Microsoft.Extensions.DependencyInjection;
using QIM.Application.Common.Mappings;
using QIM.Application.Features.Users;
using QIM.Application.Interfaces;
using QIM.Application.Interfaces.Repositories;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities;
using QIM.Domain.Entities.Identity;
using QIM.Persistence.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace QIM.Tests.Phase4;

[TestClass]
public class UserHandlerTests : TestBase
{
    private IUnitOfWork _uow = null!;
    private IMapper _mapper = null!;
    private UserManager<ApplicationUser> _userManager = null!;
    private string _userId = null!;

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
            UserName = "profile@test.com",
            Email = "profile@test.com",
            FullName = "Profile User",
            PhoneNumber = "0790000000",
            UserType = UserType.Client,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userManager.CreateAsync(user, "Test123!");
        _userId = user.Id;
    }

    [TestMethod]
    public async Task GetUserProfile_ReturnsCorrectData()
    {
        var handler = new GetUserProfileHandler(_userManager);
        var result = await handler.Handle(new GetUserProfileQuery(_userId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Profile User", result.Data!.FullName);
        Assert.AreEqual("profile@test.com", result.Data.Email);
    }

    [TestMethod]
    public async Task GetUserProfile_NotFound_ReturnsFailure()
    {
        var handler = new GetUserProfileHandler(_userManager);
        var result = await handler.Handle(new GetUserProfileQuery("non-existent-id"), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task UpdateUserProfile_ReturnsUpdated()
    {
        var handler = new UpdateUserProfileHandler(_userManager);
        var result = await handler.Handle(
            new UpdateUserProfileCommand(_userId, new Application.DTOs.Business.UpdateProfileRequest
            {
                FullName = "Updated Name",
                PhoneNumber = "0791111111"
            }), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Updated Name", result.Data!.FullName);
    }

    [TestMethod]
    public async Task ToggleUserActive_FlipsState()
    {
        var handler = new ToggleUserActiveHandler(_userManager);
        var result = await handler.Handle(new ToggleUserActiveCommand(_userId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);

        var user = await _userManager.FindByIdAsync(_userId);
        Assert.IsFalse(user!.IsActive);
    }

    [TestMethod]
    public async Task GetDashboardStats_ReturnsCounts()
    {
        // Seed some data
        var cat = new Activity { NameAr = "فئة", NameEn = "Cat" };
        await _uow.Activities.AddAsync(cat);
        await _uow.SaveChangesAsync(CancellationToken.None);

        var biz = new Business
        {
            NameAr = "مطعم",
            NameEn = "Biz",
            OwnerId = _userId,
            ActivityId = cat.Id,
            Status = BusinessStatus.Pending
        };
        await _uow.Businesses.AddAsync(biz);
        await _uow.SaveChangesAsync(CancellationToken.None);

        var handler = new GetDashboardStatsHandler(_uow, _userManager);
        var result = await handler.Handle(new GetDashboardStatsQuery(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data!.TotalUsers >= 1);
        Assert.IsTrue(result.Data.TotalBusinesses >= 1);
        Assert.IsTrue(result.Data.PendingBusinesses >= 1);
    }
}
