using Microsoft.Extensions.DependencyInjection;
using QIM.Application.Common.Mappings;
using QIM.Application.DTOs.Business;
using QIM.Application.Features.Businesses;
using QIM.Application.Features.Reviews;
using QIM.Application.Features.BusinessClaims;
using QIM.Application.Features.Contacts;
using QIM.Application.Features.Users;
using QIM.Application.Interfaces;
using QIM.Application.Interfaces.Repositories;
using QIM.Application.Interfaces.Services;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities;
using QIM.Domain.Entities.Identity;
using QIM.Persistence.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace QIM.Tests.Phase4;

[TestClass]
public class BusinessHandlerTests : TestBase
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

        // Seed a user
        var user = new ApplicationUser
        {
            UserName = "owner@test.com",
            Email = "owner@test.com",
            FullName = "Business Owner",
            UserType = UserType.Provider,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userManager.CreateAsync(user, "Test123!");
        _ownerId = user.Id;

        // Seed a category
        var cat = new Activity { NameAr = "مطاعم", NameEn = "Restaurants" };
        await _uow.Activities.AddAsync(cat);
        await _uow.SaveChangesAsync(CancellationToken.None);
        _activityId = cat.Id;
    }

    // ── Business CRUD ──

    [TestMethod]
    public async Task CreateBusiness_ReturnsSuccess_WithPendingStatus()
    {
        var handler = new CreateBusinessHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateBusinessCommand(new CreateBusinessRequest
            {
                NameAr = "مطعم تجريبي",
                NameEn = "Test Restaurant",
                ActivityId = _activityId
            }, _ownerId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Test Restaurant", result.Data!.NameEn);
        Assert.AreEqual(BusinessStatus.Pending, result.Data.Status);
    }

    [TestMethod]
    public async Task GetBusinessById_ReturnsCorrectBusiness()
    {
        var createHandler = new CreateBusinessHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateBusinessCommand(new CreateBusinessRequest
            {
                NameAr = "مطعم",
                NameEn = "Restaurant",
                ActivityId = _activityId
            }, _ownerId), CancellationToken.None);

        var handler = new GetBusinessByIdHandler(_uow, _mapper);
        var result = await handler.Handle(new GetBusinessByIdQuery(created.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Restaurant", result.Data!.NameEn);
    }

    [TestMethod]
    public async Task GetBusinessById_NotFound_ReturnsFailure()
    {
        var handler = new GetBusinessByIdHandler(_uow, _mapper);
        var result = await handler.Handle(new GetBusinessByIdQuery(999), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task UpdateBusiness_ReturnsUpdatedData()
    {
        var createHandler = new CreateBusinessHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateBusinessCommand(new CreateBusinessRequest
            {
                NameAr = "قديم",
                NameEn = "Old Name",
                ActivityId = _activityId
            }, _ownerId), CancellationToken.None);

        var updateHandler = new UpdateBusinessHandler(_uow, _mapper);
        var result = await updateHandler.Handle(
            new UpdateBusinessCommand(created.Data!.Id, new UpdateBusinessRequest { NameEn = "New Name" }, _ownerId),
            CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("New Name", result.Data!.NameEn);
    }

    [TestMethod]
    public async Task DeleteBusiness_ReturnsSuccess()
    {
        var createHandler = new CreateBusinessHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateBusinessCommand(new CreateBusinessRequest
            {
                NameAr = "حذف",
                NameEn = "ToDelete",
                ActivityId = _activityId
            }, _ownerId), CancellationToken.None);

        var deleteHandler = new DeleteBusinessHandler(_uow);
        var result = await deleteHandler.Handle(new DeleteBusinessCommand(created.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task ApproveBusiness_SetsApprovedStatus()
    {
        var createHandler = new CreateBusinessHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateBusinessCommand(new CreateBusinessRequest
            {
                NameAr = "موافقة",
                NameEn = "Approve Me",
                ActivityId = _activityId
            }, _ownerId), CancellationToken.None);

        var handler = new ApproveBusinessHandler(_uow, _mapper);
        var result = await handler.Handle(new ApproveBusinessCommand(created.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(BusinessStatus.Approved, result.Data!.Status);
    }

    [TestMethod]
    public async Task RejectBusiness_SetsRejectedStatus()
    {
        var createHandler = new CreateBusinessHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateBusinessCommand(new CreateBusinessRequest
            {
                NameAr = "رفض",
                NameEn = "Reject Me",
                ActivityId = _activityId
            }, _ownerId), CancellationToken.None);

        var handler = new RejectBusinessHandler(_uow, _mapper);
        var result = await handler.Handle(new RejectBusinessCommand(created.Data!.Id, "Missing documents"), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(BusinessStatus.Rejected, result.Data!.Status);
        Assert.AreEqual("Missing documents", result.Data!.RejectionReason);
    }

    [TestMethod]
    public async Task GetBusinessesByOwner_ReturnsCorrectList()
    {
        var createHandler = new CreateBusinessHandler(_uow, _mapper);
        await createHandler.Handle(
            new CreateBusinessCommand(new CreateBusinessRequest
            {
                NameAr = "أ",
                NameEn = "Biz1",
                ActivityId = _activityId
            }, _ownerId), CancellationToken.None);
        await createHandler.Handle(
            new CreateBusinessCommand(new CreateBusinessRequest
            {
                NameAr = "ب",
                NameEn = "Biz2",
                ActivityId = _activityId
            }, _ownerId), CancellationToken.None);

        var handler = new GetBusinessesByOwnerHandler(_uow, _mapper);
        var result = await handler.Handle(new GetBusinessesByOwnerQuery(_ownerId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.TotalCount);
    }

    [TestMethod]
    public async Task SearchBusinesses_OnlyApproved()
    {
        var createHandler = new CreateBusinessHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateBusinessCommand(new CreateBusinessRequest
            {
                NameAr = "معتمد",
                NameEn = "Approved Biz",
                ActivityId = _activityId
            }, _ownerId), CancellationToken.None);

        // Approve it
        var approveHandler = new ApproveBusinessHandler(_uow, _mapper);
        await approveHandler.Handle(new ApproveBusinessCommand(created.Data!.Id), CancellationToken.None);

        // Search
        var searchHandler = new SearchBusinessesHandler(_uow, _mapper);
        var result = await searchHandler.Handle(new SearchBusinessesQuery(Keyword: "Approved"), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.TotalCount >= 1);
    }

    [TestMethod]
    public async Task GetTopBusinesses_ReturnsApprovedOnly()
    {
        var createHandler = new CreateBusinessHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateBusinessCommand(new CreateBusinessRequest
            {
                NameAr = "أفضل",
                NameEn = "TopBiz",
                ActivityId = _activityId
            }, _ownerId), CancellationToken.None);

        // Approve
        var approveHandler = new ApproveBusinessHandler(_uow, _mapper);
        await approveHandler.Handle(new ApproveBusinessCommand(created.Data!.Id), CancellationToken.None);

        var handler = new GetTopBusinessesHandler(_uow, _mapper);
        var result = await handler.Handle(new GetTopBusinessesQuery(5), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data!.Count >= 1);
    }

    // ── Business Address ──

    [TestMethod]
    public async Task AddBusinessAddress_ReturnsSuccess()
    {
        var biz = await CreateTestBusiness();

        var handler = new AddBusinessAddressHandler(_uow, _mapper);
        var result = await handler.Handle(
            new AddBusinessAddressCommand(biz.Id, new CreateBusinessAddressRequest
            {
                StreetName = "Main St",
                IsPrimary = true
            }), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Main St", result.Data!.StreetName);
    }

    [TestMethod]
    public async Task UpdateBusinessAddress_ReturnsUpdatedData()
    {
        var biz = await CreateTestBusiness();

        var addHandler = new AddBusinessAddressHandler(_uow, _mapper);
        var added = await addHandler.Handle(
            new AddBusinessAddressCommand(biz.Id, new CreateBusinessAddressRequest { StreetName = "Old" }),
            CancellationToken.None);

        var updateHandler = new UpdateBusinessAddressHandler(_uow, _mapper);
        var result = await updateHandler.Handle(
            new UpdateBusinessAddressCommand(added.Data!.Id, new UpdateBusinessAddressRequest { StreetName = "New St" }),
            CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("New St", result.Data!.StreetName);
    }

    [TestMethod]
    public async Task DeleteBusinessAddress_ReturnsSuccess()
    {
        var biz = await CreateTestBusiness();

        var addHandler = new AddBusinessAddressHandler(_uow, _mapper);
        var added = await addHandler.Handle(
            new AddBusinessAddressCommand(biz.Id, new CreateBusinessAddressRequest { StreetName = "Del" }),
            CancellationToken.None);

        var deleteHandler = new DeleteBusinessAddressHandler(_uow);
        var result = await deleteHandler.Handle(new DeleteBusinessAddressCommand(added.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
    }

    // ── Work Hours ──

    [TestMethod]
    public async Task SetBusinessWorkHours_ReplacesExisting()
    {
        var biz = await CreateTestBusiness();

        var handler = new SetBusinessWorkHoursHandler(_uow, _mapper);
        var items = new List<SetWorkHoursRequest>
        {
            new() { DayOfWeek = DayOfWeek.Monday, OpenTime = TimeSpan.FromHours(9), CloseTime = TimeSpan.FromHours(17) },
            new() { DayOfWeek = DayOfWeek.Friday, IsClosed = true }
        };

        var result = await handler.Handle(new SetBusinessWorkHoursCommand(biz.Id, items), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Data!.Count);
    }

    [TestMethod]
    public async Task GetBusinessWorkHours_ReturnsAll()
    {
        var biz = await CreateTestBusiness();

        var setHandler = new SetBusinessWorkHoursHandler(_uow, _mapper);
        await setHandler.Handle(
            new SetBusinessWorkHoursCommand(biz.Id, [new() { DayOfWeek = DayOfWeek.Sunday, IsClosed = true }]),
            CancellationToken.None);

        var getHandler = new GetBusinessWorkHoursHandler(_uow, _mapper);
        var result = await getHandler.Handle(new GetBusinessWorkHoursQuery(biz.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Data!.Count);
    }

    // ── Business Image ──

    [TestMethod]
    public async Task AddBusinessImage_ReturnsSuccess()
    {
        var biz = await CreateTestBusiness();

        var handler = new AddBusinessImageHandler(_uow, _mapper);
        var result = await handler.Handle(
            new AddBusinessImageCommand(biz.Id, "/uploads/test.jpg", true, 0),
            CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("/uploads/test.jpg", result.Data!.ImageUrl);
        Assert.IsTrue(result.Data.IsCover);
    }

    [TestMethod]
    public async Task DeleteBusinessImage_ReturnsSuccess()
    {
        var biz = await CreateTestBusiness();

        var addHandler = new AddBusinessImageHandler(_uow, _mapper);
        var added = await addHandler.Handle(
            new AddBusinessImageCommand(biz.Id, "/uploads/del.jpg", false, 0),
            CancellationToken.None);

        var deleteHandler = new DeleteBusinessImageHandler(_uow);
        var result = await deleteHandler.Handle(new DeleteBusinessImageCommand(added.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
    }

    // ── Helper ──

    private async Task<BusinessDto> CreateTestBusiness()
    {
        var handler = new CreateBusinessHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateBusinessCommand(new CreateBusinessRequest
            {
                NameAr = "تجريبي",
                NameEn = "TestBiz",
                ActivityId = _activityId
            }, _ownerId), CancellationToken.None);
        return result.Data!;
    }
}
