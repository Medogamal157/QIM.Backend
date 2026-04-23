using Microsoft.Extensions.DependencyInjection;
using QIM.Application.Common.Mappings;
using QIM.Application.DTOs.Business;
using QIM.Application.Features.Reviews;
using QIM.Application.Features.Businesses;
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
public class ReviewHandlerTests : TestBase
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

        // Seed users
        var user = new ApplicationUser
        {
            UserName = "reviewer@test.com",
            Email = "reviewer@test.com",
            FullName = "Reviewer",
            UserType = UserType.Client,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userManager.CreateAsync(user, "Test123!");
        _userId = user.Id;

        var user2 = new ApplicationUser
        {
            UserName = "reviewer2@test.com",
            Email = "reviewer2@test.com",
            FullName = "Reviewer2",
            UserType = UserType.Client,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userManager.CreateAsync(user2, "Test123!");
        _user2Id = user2.Id;

        // Seed category + business
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
    public async Task CreateReview_ReturnsSuccess()
    {
        var handler = new CreateReviewHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateReviewCommand(new CreateReviewRequest
            {
                BusinessId = _businessId,
                Rating = 4,
                Comment = "Great!"
            }, _userId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(4, result.Data!.Rating);
        Assert.AreEqual(ReviewStatus.Approved, result.Data.Status);
    }

    [TestMethod]
    public async Task CreateReview_DuplicateUser_ReturnsFailure()
    {
        var handler = new CreateReviewHandler(_uow, _mapper);
        await handler.Handle(
            new CreateReviewCommand(new CreateReviewRequest
            {
                BusinessId = _businessId,
                Rating = 5
            }, _userId), CancellationToken.None);

        var result = await handler.Handle(
            new CreateReviewCommand(new CreateReviewRequest
            {
                BusinessId = _businessId,
                Rating = 3
            }, _userId), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task CreateReview_UpdatesBusinessRating()
    {
        var handler = new CreateReviewHandler(_uow, _mapper);
        await handler.Handle(
            new CreateReviewCommand(new CreateReviewRequest { BusinessId = _businessId, Rating = 4 }, _userId),
            CancellationToken.None);
        await handler.Handle(
            new CreateReviewCommand(new CreateReviewRequest { BusinessId = _businessId, Rating = 2 }, _user2Id),
            CancellationToken.None);

        var biz = await _uow.Businesses.GetByIdAsync(_businessId);
        Assert.AreEqual(2, biz!.ReviewCount);
        Assert.AreEqual(3.0, biz.Rating, 0.1);
    }

    [TestMethod]
    public async Task FlagReview_SetsStatusFlagged()
    {
        var createHandler = new CreateReviewHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateReviewCommand(new CreateReviewRequest { BusinessId = _businessId, Rating = 1, Comment = "Bad" }, _userId),
            CancellationToken.None);

        var flagHandler = new FlagReviewHandler(_uow, _mapper);
        var result = await flagHandler.Handle(
            new FlagReviewCommand(created.Data!.Id, "Inappropriate", _user2Id),
            CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(ReviewStatus.Flagged, result.Data!.Status);
        Assert.AreEqual("Inappropriate", result.Data.FlagReason);
    }

    [TestMethod]
    public async Task ApproveReview_SetsApproved()
    {
        var createHandler = new CreateReviewHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateReviewCommand(new CreateReviewRequest { BusinessId = _businessId, Rating = 3 }, _userId),
            CancellationToken.None);

        // Flag it first
        var flagHandler = new FlagReviewHandler(_uow, _mapper);
        await flagHandler.Handle(new FlagReviewCommand(created.Data!.Id, "test", _user2Id), CancellationToken.None);

        // Approve
        var handler = new ApproveReviewHandler(_uow, _mapper);
        var result = await handler.Handle(new ApproveReviewCommand(created.Data.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(ReviewStatus.Approved, result.Data!.Status);
        Assert.IsNull(result.Data.FlagReason);
    }

    [TestMethod]
    public async Task DeleteReview_UpdatesBusinessRating()
    {
        var handler = new CreateReviewHandler(_uow, _mapper);
        var created = await handler.Handle(
            new CreateReviewCommand(new CreateReviewRequest { BusinessId = _businessId, Rating = 4 }, _userId),
            CancellationToken.None);

        var deleteHandler = new DeleteReviewHandler(_uow);
        var result = await deleteHandler.Handle(new DeleteReviewCommand(created.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);

        var biz = await _uow.Businesses.GetByIdAsync(_businessId);
        Assert.AreEqual(0, biz!.ReviewCount);
        Assert.AreEqual(0, biz.Rating, 0.01);
    }

    [TestMethod]
    public async Task GetBusinessReviews_OnlyApproved()
    {
        var createHandler = new CreateReviewHandler(_uow, _mapper);
        await createHandler.Handle(
            new CreateReviewCommand(new CreateReviewRequest { BusinessId = _businessId, Rating = 5 }, _userId),
            CancellationToken.None);

        var getHandler = new GetBusinessReviewsHandler(_uow, _mapper);
        var result = await getHandler.Handle(new GetBusinessReviewsQuery(_businessId), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.TotalCount);
    }
}
