using Microsoft.Extensions.DependencyInjection;
using QIM.Application.Common.Mappings;
using QIM.Application.Features.Activities;
using QIM.Application.Features.Specialities;
using QIM.Application.DTOs.Activity;
using QIM.Application.Interfaces;
using QIM.Application.Interfaces.Repositories;
using QIM.Persistence.Repositories;
using AutoMapper;

namespace QIM.Tests.Phase3;

[TestClass]
public class ActivityHandlerTests : TestBase
{
    private IUnitOfWork _uow = null!;
    private IMapper _mapper = null!;

    [TestInitialize]
    public void SetUp()
    {
        _builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        _builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        _builder.Services.AddAutoMapper(typeof(MappingProfile));

        _serviceProvider = _builder.Services.BuildServiceProvider();
        var context = GetDbContext();
        context.Database.EnsureCreated();

        _uow = _serviceProvider.GetRequiredService<IUnitOfWork>();
        _mapper = _serviceProvider.GetRequiredService<IMapper>();
    }

    // ── Activity Tests ──

    [TestMethod]
    public async Task CreateActivity_RootActivity_ReturnsSuccess()
    {
        var handler = new CreateActivityHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateActivityCommand(new CreateActivityRequest
            {
                NameAr = "مطاعم",
                NameEn = "Restaurants",
                SortOrder = 0
            }), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Restaurants", result.Data!.NameEn);
    }

    [TestMethod]
    public async Task CreateActivity_WithParent_ReturnsSuccess()
    {
        var handler = new CreateActivityHandler(_uow, _mapper);
        var parent = await handler.Handle(
            new CreateActivityCommand(new CreateActivityRequest { NameAr = "أ", NameEn = "Parent" }), CancellationToken.None);

        var child = await handler.Handle(
            new CreateActivityCommand(new CreateActivityRequest
            {
                NameAr = "ب",
                NameEn = "Child",
                ParentActivityId = parent.Data!.Id
            }), CancellationToken.None);

        Assert.IsTrue(child.IsSuccess);
        Assert.AreEqual("Child", child.Data!.NameEn);
    }

    [TestMethod]
    public async Task CreateActivity_InvalidParent_ReturnsFailure()
    {
        var handler = new CreateActivityHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateActivityCommand(new CreateActivityRequest
            {
                NameAr = "أ",
                NameEn = "Orphan",
                ParentActivityId = 999
            }), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task GetActivityTree_ReturnsRootWithSubActivities()
    {
        var handler = new CreateActivityHandler(_uow, _mapper);
        var parent = await handler.Handle(
            new CreateActivityCommand(new CreateActivityRequest { NameAr = "أ", NameEn = "Root", SortOrder = 0 }), CancellationToken.None);
        await handler.Handle(
            new CreateActivityCommand(new CreateActivityRequest { NameAr = "ب", NameEn = "Sub", ParentActivityId = parent.Data!.Id }), CancellationToken.None);

        var treeHandler = new GetActivityTreeHandler(_uow, _mapper);
        var result = await treeHandler.Handle(new GetActivityTreeQuery(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data!.Count >= 1);
        var root = result.Data.FirstOrDefault(c => c.NameEn == "Root");
        Assert.IsNotNull(root);
        Assert.IsTrue(root.SubActivities.Count >= 1);
    }

    [TestMethod]
    public async Task UpdateActivity_ChangesName()
    {
        var createHandler = new CreateActivityHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateActivityCommand(new CreateActivityRequest { NameAr = "أ", NameEn = "Old" }), CancellationToken.None);

        var handler = new UpdateActivityHandler(_uow, _mapper);
        var result = await handler.Handle(
            new UpdateActivityCommand(created.Data!.Id, new UpdateActivityRequest { NameEn = "New" }), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("New", result.Data!.NameEn);
    }

    [TestMethod]
    public async Task DeleteActivity_SoftDeletes()
    {
        var createHandler = new CreateActivityHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateActivityCommand(new CreateActivityRequest { NameAr = "أ", NameEn = "ToDelete" }), CancellationToken.None);

        var deleteHandler = new DeleteActivityHandler(_uow);
        var result = await deleteHandler.Handle(new DeleteActivityCommand(created.Data!.Id), CancellationToken.None);
        Assert.IsTrue(result.IsSuccess);

        var getHandler = new GetAllActivitiesHandler(_uow, _mapper);
        var all = await getHandler.Handle(new GetAllActivitiesQuery(), CancellationToken.None);
        Assert.IsFalse(all.Data!.Any(c => c.NameEn == "ToDelete"));
    }

    // ── Speciality Tests ──

    [TestMethod]
    public async Task CreateSpeciality_WithValidActivity_ReturnsSuccess()
    {
        var catHandler = new CreateActivityHandler(_uow, _mapper);
        var category = await catHandler.Handle(
            new CreateActivityCommand(new CreateActivityRequest { NameAr = "أ", NameEn = "Food" }), CancellationToken.None);

        var handler = new CreateSpecialityHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateSpecialityCommand(new CreateSpecialityRequest
            {
                NameAr = "توصيل",
                NameEn = "Delivery",
                ActivityId = category.Data!.Id
            }), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Delivery", result.Data!.NameEn);
    }

    [TestMethod]
    public async Task CreateSpeciality_InvalidActivity_ReturnsFailure()
    {
        var handler = new CreateSpecialityHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateSpecialityCommand(new CreateSpecialityRequest
            {
                NameAr = "أ",
                NameEn = "ST",
                ActivityId = 999
            }), CancellationToken.None);
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task GetSpecialitiesByActivity_FiltersCorrectly()
    {
        var catHandler = new CreateActivityHandler(_uow, _mapper);
        var cat1 = await catHandler.Handle(new CreateActivityCommand(new CreateActivityRequest { NameAr = "أ", NameEn = "Cat1" }), CancellationToken.None);
        var cat2 = await catHandler.Handle(new CreateActivityCommand(new CreateActivityRequest { NameAr = "ب", NameEn = "Cat2" }), CancellationToken.None);

        var stHandler = new CreateSpecialityHandler(_uow, _mapper);
        await stHandler.Handle(new CreateSpecialityCommand(new CreateSpecialityRequest { NameAr = "أ", NameEn = "ST1", ActivityId = cat1.Data!.Id }), CancellationToken.None);
        await stHandler.Handle(new CreateSpecialityCommand(new CreateSpecialityRequest { NameAr = "ب", NameEn = "ST2", ActivityId = cat2.Data!.Id }), CancellationToken.None);

        var handler = new GetSpecialitiesByActivityHandler(_uow, _mapper);
        var result = await handler.Handle(new GetSpecialitiesByActivityQuery(cat1.Data.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Data!.Count);
        Assert.AreEqual("ST1", result.Data[0].NameEn);
    }

    [TestMethod]
    public async Task ToggleSpecialityEnabled_FlipsValue()
    {
        var catHandler = new CreateActivityHandler(_uow, _mapper);
        var cat = await catHandler.Handle(new CreateActivityCommand(new CreateActivityRequest { NameAr = "أ", NameEn = "Cat" }), CancellationToken.None);

        var stHandler = new CreateSpecialityHandler(_uow, _mapper);
        var st = await stHandler.Handle(new CreateSpecialityCommand(new CreateSpecialityRequest { NameAr = "أ", NameEn = "ST", ActivityId = cat.Data!.Id }), CancellationToken.None);

        var handler = new ToggleSpecialityEnabledHandler(_uow, _mapper);
        var result = await handler.Handle(new ToggleSpecialityEnabledCommand(st.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.Data!.IsEnabled);
    }
}
