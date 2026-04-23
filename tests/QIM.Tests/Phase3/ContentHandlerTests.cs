using Microsoft.Extensions.DependencyInjection;
using QIM.Application.Common.Mappings;
using QIM.Application.Features.PlatformSettings;
using QIM.Application.Features.BlogPosts;
using QIM.Application.DTOs.Content;
using QIM.Application.Interfaces;
using QIM.Application.Interfaces.Repositories;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities.Identity;
using QIM.Persistence.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace QIM.Tests.Phase3;

[TestClass]
public class ContentHandlerTests : TestBase
{
    private IUnitOfWork _uow = null!;
    private IMapper _mapper = null!;
    private UserManager<ApplicationUser> _userManager = null!;

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

        // Seed a platform setting
        var db = GetDbContext();
        db.PlatformSettings.Add(new Domain.Entities.PlatformSetting { Key = "site_name", Value = "QIM", Group = "general" });
        db.PlatformSettings.Add(new Domain.Entities.PlatformSetting { Key = "contact_email", Value = "info@qim.com", Group = "contact" });
        await db.SaveChangesAsync();
    }

    // ── PlatformSettings Tests ──

    [TestMethod]
    public async Task GetAllPlatformSettings_ReturnsAll()
    {
        var handler = new GetAllPlatformSettingsHandler(_uow, _mapper);
        var result = await handler.Handle(new GetAllPlatformSettingsQuery(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data!.Count >= 2);
    }

    [TestMethod]
    public async Task GetPlatformSettingsByGroup_FiltersCorrectly()
    {
        var handler = new GetPlatformSettingsByGroupHandler(_uow, _mapper);
        var result = await handler.Handle(new GetPlatformSettingsByGroupQuery("general"), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Data!.Count);
        Assert.AreEqual("site_name", result.Data[0].Key);
    }

    [TestMethod]
    public async Task UpdatePlatformSetting_ChangesValue()
    {
        var getHandler = new GetAllPlatformSettingsHandler(_uow, _mapper);
        var all = await getHandler.Handle(new GetAllPlatformSettingsQuery(), CancellationToken.None);
        var setting = all.Data!.First(s => s.Key == "site_name");

        var handler = new UpdatePlatformSettingHandler(_uow, _mapper);
        var result = await handler.Handle(
            new UpdatePlatformSettingCommand(setting.Id, new UpdatePlatformSettingRequest { Value = "QIM Platform" }), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("QIM Platform", result.Data!.Value);
    }

    [TestMethod]
    public async Task UpdatePlatformSetting_NotFound_ReturnsFailure()
    {
        var handler = new UpdatePlatformSettingHandler(_uow, _mapper);
        var result = await handler.Handle(
            new UpdatePlatformSettingCommand(999, new UpdatePlatformSettingRequest { Value = "x" }), CancellationToken.None);
        Assert.IsFalse(result.IsSuccess);
    }

    // ── BlogPost Tests ──

    [TestMethod]
    public async Task CreateBlogPost_ReturnsSuccess_WithDraftStatus()
    {
        var author = new ApplicationUser
        {
            FullName = "Admin User",
            Email = "admin@qim.com",
            UserName = "admin@qim.com",
            UserType = UserType.Admin,
            IsActive = true
        };
        await _userManager.CreateAsync(author, "Password123!");

        var handler = new CreateBlogPostHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateBlogPostCommand(
                new CreateBlogPostRequest
                {
                    TitleAr = "مقال",
                    TitleEn = "Article",
                    ContentAr = "محتوى",
                    ContentEn = "Content",
                    Category = "Tech"
                }, author.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Article", result.Data!.TitleEn);
        Assert.AreEqual(BlogPostStatus.Draft, result.Data.Status);
    }

    [TestMethod]
    public async Task ToggleBlogPostPublish_PublishesAndUnpublishes()
    {
        var author = new ApplicationUser
        {
            FullName = "Author",
            Email = "author@qim.com",
            UserName = "author@qim.com",
            UserType = UserType.Admin,
            IsActive = true
        };
        await _userManager.CreateAsync(author, "Password123!");

        var createHandler = new CreateBlogPostHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateBlogPostCommand(new CreateBlogPostRequest
            {
                TitleAr = "أ",
                TitleEn = "Post",
                ContentAr = "م",
                ContentEn = "C",
                Category = "X"
            }, author.Id), CancellationToken.None);

        var handler = new ToggleBlogPostPublishHandler(_uow, _mapper);

        // Publish
        var publishResult = await handler.Handle(new ToggleBlogPostPublishCommand(created.Data!.Id), CancellationToken.None);
        Assert.IsTrue(publishResult.IsSuccess);
        Assert.AreEqual(BlogPostStatus.Published, publishResult.Data!.Status);
        Assert.IsNotNull(publishResult.Data.PublishedAt);

        // Unpublish
        var unpublishResult = await handler.Handle(new ToggleBlogPostPublishCommand(created.Data!.Id), CancellationToken.None);
        Assert.IsTrue(unpublishResult.IsSuccess);
        Assert.AreEqual(BlogPostStatus.Draft, unpublishResult.Data!.Status);
    }

    [TestMethod]
    public async Task DeleteBlogPost_SoftDeletes()
    {
        var author = new ApplicationUser
        {
            FullName = "Del Author",
            Email = "del@qim.com",
            UserName = "del@qim.com",
            UserType = UserType.Admin,
            IsActive = true
        };
        await _userManager.CreateAsync(author, "Password123!");

        var createHandler = new CreateBlogPostHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateBlogPostCommand(new CreateBlogPostRequest
            {
                TitleAr = "أ",
                TitleEn = "ToDelete",
                ContentAr = "م",
                ContentEn = "C",
                Category = "X"
            }, author.Id), CancellationToken.None);

        var deleteHandler = new DeleteBlogPostHandler(_uow);
        var result = await deleteHandler.Handle(new DeleteBlogPostCommand(created.Data!.Id), CancellationToken.None);
        Assert.IsTrue(result.IsSuccess);

        // Should return not found now
        var getHandler = new GetBlogPostByIdHandler(_uow, _mapper);
        var getResult = await getHandler.Handle(new GetBlogPostByIdQuery(created.Data.Id), CancellationToken.None);
        Assert.IsFalse(getResult.IsSuccess);
    }
}
