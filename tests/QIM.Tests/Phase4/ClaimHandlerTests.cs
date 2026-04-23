using Microsoft.Extensions.DependencyInjection;
using QIM.Application.Common.Mappings;
using QIM.Application.DTOs.Business;
using QIM.Application.Features.BusinessClaims;
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
public class ClaimHandlerTests : TestBase
{
    private IUnitOfWork _uow = null!;
    private IMapper _mapper = null!;
    private UserManager<ApplicationUser> _userManager = null!;
    private string _userId = null!;
    private string _user2Id = null!;
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

        var user = new ApplicationUser
        {
            UserName = "claimer@test.com",
            Email = "claimer@test.com",
            FullName = "Claimer",
            UserType = UserType.Client,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userManager.CreateAsync(user, "Test123!");
        _userId = user.Id;

        var user2 = new ApplicationUser
        {
            UserName = "claimer2@test.com",
            Email = "claimer2@test.com",
            FullName = "Claimer2",
            UserType = UserType.Client,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userManager.CreateAsync(user2, "Test123!");
        _user2Id = user2.Id;

        var cat = new Activity { NameAr = "فئة", NameEn = "Cat" };
        await _uow.Activities.AddAsync(cat);
        await _uow.SaveChangesAsync(CancellationToken.None);

        var biz = new Business
        {
            NameAr = "مطعم",
            NameEn = "TestBiz",
            OwnerId = _userId,
            ActivityId = cat.Id,
            Status = BusinessStatus.Approved
        };
        await _uow.Businesses.AddAsync(biz);
        await _uow.SaveChangesAsync(CancellationToken.None);
        _businessId = biz.Id;
    }

    [TestMethod]
    public async Task CreateClaim_ReturnsSuccess()
    {
        var handler = new CreateBusinessClaimHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateBusinessClaimCommand(new CreateBusinessClaimRequest
            {
                BusinessId = _businessId,
                Phone = "0790000000",
                Email = "claimer@test.com",
                Message = "I own this"
            }, _userId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(ClaimStatus.Pending, result.Data!.Status);
    }

    [TestMethod]
    public async Task CreateClaim_DuplicatePending_ReturnsFailure()
    {
        var handler = new CreateBusinessClaimHandler(_uow, _mapper);
        await handler.Handle(
            new CreateBusinessClaimCommand(new CreateBusinessClaimRequest
            {
                BusinessId = _businessId,
                Message = "First claim"
            }, _userId), CancellationToken.None);

        var result = await handler.Handle(
            new CreateBusinessClaimCommand(new CreateBusinessClaimRequest
            {
                BusinessId = _businessId,
                Message = "Second claim"
            }, _userId), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task ApproveClaim_TransfersOwnership()
    {
        var handler = new CreateBusinessClaimHandler(_uow, _mapper);
        var created = await handler.Handle(
            new CreateBusinessClaimCommand(new CreateBusinessClaimRequest
            {
                BusinessId = _businessId,
                Message = "I own this"
            }, _user2Id), CancellationToken.None);

        var approveHandler = new ApproveClaimHandler(_uow, _mapper);
        var result = await approveHandler.Handle(
            new ApproveClaimCommand(created.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(ClaimStatus.Approved, result.Data!.Status);

        var biz = await _uow.Businesses.GetByIdAsync(_businessId);
        Assert.AreEqual(_user2Id, biz!.OwnerId);
        Assert.IsTrue(biz.IsVerified);
    }

    [TestMethod]
    public async Task RejectClaim_SetsRejected()
    {
        var handler = new CreateBusinessClaimHandler(_uow, _mapper);
        var created = await handler.Handle(
            new CreateBusinessClaimCommand(new CreateBusinessClaimRequest
            {
                BusinessId = _businessId,
                Message = "My claim"
            }, _userId), CancellationToken.None);

        var rejectHandler = new RejectClaimHandler(_uow, _mapper);
        var result = await rejectHandler.Handle(
            new RejectClaimCommand(created.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(ClaimStatus.Rejected, result.Data!.Status);
    }

    [TestMethod]
    public async Task GetAllClaims_ReturnsList()
    {
        var createHandler = new CreateBusinessClaimHandler(_uow, _mapper);
        await createHandler.Handle(
            new CreateBusinessClaimCommand(new CreateBusinessClaimRequest
            {
                BusinessId = _businessId,
                Message = "Claim"
            }, _userId), CancellationToken.None);

        var handler = new GetAllClaimsHandler(_uow, _mapper);
        var result = await handler.Handle(new GetAllClaimsQuery(1, 10, null), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.TotalCount);
    }
}
