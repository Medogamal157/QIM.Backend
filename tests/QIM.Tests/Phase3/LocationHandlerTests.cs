using Microsoft.Extensions.DependencyInjection;
using QIM.Application.Common.Mappings;
using QIM.Application.Features.Countries;
using QIM.Application.Features.Cities;
using QIM.Application.Features.Districts;
using QIM.Application.DTOs.Location;
using QIM.Application.Interfaces;
using QIM.Application.Interfaces.Repositories;
using QIM.Domain.Entities;
using QIM.Persistence.Repositories;
using AutoMapper;

namespace QIM.Tests.Phase3;

[TestClass]
public class LocationHandlerTests : TestBase
{
    private IUnitOfWork _uow = null!;
    private IMapper _mapper = null!;

    [TestInitialize]
    public void SetUp()
    {
        // Register UnitOfWork + GenericRepository + AutoMapper
        _builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        _builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        _builder.Services.AddAutoMapper(typeof(MappingProfile));

        _serviceProvider = _builder.Services.BuildServiceProvider();

        // Re-create tables
        var context = GetDbContext();
        context.Database.EnsureCreated();

        _uow = _serviceProvider.GetRequiredService<IUnitOfWork>();
        _mapper = _serviceProvider.GetRequiredService<IMapper>();
    }

    // ── Country Tests ──

    [TestMethod]
    public async Task CreateCountry_ReturnsSuccess()
    {
        var handler = new CreateCountryHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateCountryCommand(new CreateCountryRequest
            {
                NameAr = "الأردن",
                NameEn = "Jordan",
                IsDefault = true,
                SortOrder = 0
            }), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Jordan", result.Data!.NameEn);
        Assert.AreEqual("الأردن", result.Data.NameAr);
        Assert.IsTrue(result.Data.IsDefault);
    }

    [TestMethod]
    public async Task GetAllCountries_ReturnsOrdered()
    {
        var createHandler = new CreateCountryHandler(_uow, _mapper);
        await createHandler.Handle(new CreateCountryCommand(new CreateCountryRequest { NameAr = "ب", NameEn = "B", SortOrder = 2 }), CancellationToken.None);
        await createHandler.Handle(new CreateCountryCommand(new CreateCountryRequest { NameAr = "أ", NameEn = "A", SortOrder = 1 }), CancellationToken.None);

        var handler = new GetAllCountriesHandler(_uow, _mapper);
        var result = await handler.Handle(new GetAllCountriesQuery(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Data!.Count);
        Assert.AreEqual("A", result.Data[0].NameEn);
    }

    [TestMethod]
    public async Task GetCountryById_NotFound_ReturnsFailure()
    {
        var handler = new GetCountryByIdHandler(_uow, _mapper);
        var result = await handler.Handle(new GetCountryByIdQuery(999), CancellationToken.None);
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task UpdateCountry_ChangesFields()
    {
        var createHandler = new CreateCountryHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateCountryCommand(new CreateCountryRequest { NameAr = "أ", NameEn = "Old" }), CancellationToken.None);

        var handler = new UpdateCountryHandler(_uow, _mapper);
        var result = await handler.Handle(
            new UpdateCountryCommand(created.Data!.Id, new UpdateCountryRequest { NameEn = "New" }), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("New", result.Data!.NameEn);
    }

    [TestMethod]
    public async Task DeleteCountry_SoftDeletes()
    {
        var createHandler = new CreateCountryHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateCountryCommand(new CreateCountryRequest { NameAr = "أ", NameEn = "DeleteMe" }), CancellationToken.None);

        var deleteHandler = new DeleteCountryHandler(_uow);
        var result = await deleteHandler.Handle(new DeleteCountryCommand(created.Data!.Id), CancellationToken.None);
        Assert.IsTrue(result.IsSuccess);

        // Should not appear in GetAll
        var getHandler = new GetAllCountriesHandler(_uow, _mapper);
        var all = await getHandler.Handle(new GetAllCountriesQuery(), CancellationToken.None);
        Assert.IsFalse(all.Data!.Any(c => c.NameEn == "DeleteMe"));
    }

    [TestMethod]
    public async Task ToggleCountryEnabled_FlipsValue()
    {
        var createHandler = new CreateCountryHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateCountryCommand(new CreateCountryRequest { NameAr = "أ", NameEn = "Toggle" }), CancellationToken.None);

        var handler = new ToggleCountryEnabledHandler(_uow, _mapper);
        var result = await handler.Handle(new ToggleCountryEnabledCommand(created.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.Data!.IsEnabled);
    }

    [TestMethod]
    public async Task SetDefaultCountry_ClearsOtherDefaults()
    {
        var createHandler = new CreateCountryHandler(_uow, _mapper);
        var c1 = await createHandler.Handle(new CreateCountryCommand(new CreateCountryRequest { NameAr = "أ", NameEn = "C1", IsDefault = true }), CancellationToken.None);
        var c2 = await createHandler.Handle(new CreateCountryCommand(new CreateCountryRequest { NameAr = "ب", NameEn = "C2" }), CancellationToken.None);

        var handler = new SetDefaultCountryHandler(_uow, _mapper);
        await handler.Handle(new SetDefaultCountryCommand(c2.Data!.Id), CancellationToken.None);

        var getHandler = new GetAllCountriesHandler(_uow, _mapper);
        var all = await getHandler.Handle(new GetAllCountriesQuery(), CancellationToken.None);

        var defaults = all.Data!.Where(c => c.IsDefault).ToList();
        Assert.AreEqual(1, defaults.Count);
        Assert.AreEqual("C2", defaults[0].NameEn);
    }

    [TestMethod]
    public async Task ReorderCountries_UpdatesSortOrder()
    {
        var createHandler = new CreateCountryHandler(_uow, _mapper);
        var c1 = await createHandler.Handle(new CreateCountryCommand(new CreateCountryRequest { NameAr = "أ", NameEn = "First", SortOrder = 0 }), CancellationToken.None);
        var c2 = await createHandler.Handle(new CreateCountryCommand(new CreateCountryRequest { NameAr = "ب", NameEn = "Second", SortOrder = 1 }), CancellationToken.None);

        var handler = new ReorderCountriesHandler(_uow);
        await handler.Handle(new ReorderCountriesCommand(new List<int> { c2.Data!.Id, c1.Data!.Id }), CancellationToken.None);

        var getHandler = new GetAllCountriesHandler(_uow, _mapper);
        var all = await getHandler.Handle(new GetAllCountriesQuery(), CancellationToken.None);

        Assert.AreEqual("Second", all.Data![0].NameEn);
    }

    // ── City Tests ──

    [TestMethod]
    public async Task CreateCity_WithValidCountry_ReturnsSuccess()
    {
        var countryHandler = new CreateCountryHandler(_uow, _mapper);
        var country = await countryHandler.Handle(new CreateCountryCommand(new CreateCountryRequest { NameAr = "أ", NameEn = "Jordan" }), CancellationToken.None);

        var handler = new CreateCityHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateCityCommand(new CreateCityRequest { NameAr = "عمان", NameEn = "Amman", CountryId = country.Data!.Id }), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Amman", result.Data!.NameEn);
    }

    [TestMethod]
    public async Task CreateCity_InvalidCountry_ReturnsFailure()
    {
        var handler = new CreateCityHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateCityCommand(new CreateCityRequest { NameAr = "عمان", NameEn = "Amman", CountryId = 999 }), CancellationToken.None);
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task GetCitiesByCountry_FiltersCorrectly()
    {
        var countryHandler = new CreateCountryHandler(_uow, _mapper);
        var c1 = await countryHandler.Handle(new CreateCountryCommand(new CreateCountryRequest { NameAr = "أ", NameEn = "C1" }), CancellationToken.None);
        var c2 = await countryHandler.Handle(new CreateCountryCommand(new CreateCountryRequest { NameAr = "ب", NameEn = "C2" }), CancellationToken.None);

        var cityHandler = new CreateCityHandler(_uow, _mapper);
        await cityHandler.Handle(new CreateCityCommand(new CreateCityRequest { NameAr = "أ", NameEn = "City1", CountryId = c1.Data!.Id }), CancellationToken.None);
        await cityHandler.Handle(new CreateCityCommand(new CreateCityRequest { NameAr = "ب", NameEn = "City2", CountryId = c2.Data!.Id }), CancellationToken.None);

        var handler = new GetCitiesByCountryHandler(_uow, _mapper);
        var result = await handler.Handle(new GetCitiesByCountryQuery(c1.Data.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Data!.Count);
        Assert.AreEqual("City1", result.Data[0].NameEn);
    }

    [TestMethod]
    public async Task ToggleCityEnabled_FlipsValue()
    {
        var countryHandler = new CreateCountryHandler(_uow, _mapper);
        var country = await countryHandler.Handle(new CreateCountryCommand(new CreateCountryRequest { NameAr = "أ", NameEn = "J" }), CancellationToken.None);

        var cityHandler = new CreateCityHandler(_uow, _mapper);
        var city = await cityHandler.Handle(new CreateCityCommand(new CreateCityRequest { NameAr = "أ", NameEn = "Amman", CountryId = country.Data!.Id }), CancellationToken.None);

        var handler = new ToggleCityEnabledHandler(_uow, _mapper);
        var result = await handler.Handle(new ToggleCityEnabledCommand(city.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.Data!.IsEnabled);
    }

    // ── District Tests ──

    [TestMethod]
    public async Task CreateDistrict_WithValidCity_ReturnsSuccess()
    {
        var countryHandler = new CreateCountryHandler(_uow, _mapper);
        var country = await countryHandler.Handle(new CreateCountryCommand(new CreateCountryRequest { NameAr = "أ", NameEn = "J" }), CancellationToken.None);

        var cityHandler = new CreateCityHandler(_uow, _mapper);
        var city = await cityHandler.Handle(new CreateCityCommand(new CreateCityRequest { NameAr = "أ", NameEn = "Amman", CountryId = country.Data!.Id }), CancellationToken.None);

        var handler = new CreateDistrictHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateDistrictCommand(new CreateDistrictRequest { NameAr = "عبدون", NameEn = "Abdoun", CityId = city.Data!.Id }), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Abdoun", result.Data!.NameEn);
    }

    [TestMethod]
    public async Task CreateDistrict_InvalidCity_ReturnsFailure()
    {
        var handler = new CreateDistrictHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateDistrictCommand(new CreateDistrictRequest { NameAr = "أ", NameEn = "D", CityId = 999 }), CancellationToken.None);
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task GetDistrictsByCity_FiltersCorrectly()
    {
        var countryHandler = new CreateCountryHandler(_uow, _mapper);
        var country = await countryHandler.Handle(new CreateCountryCommand(new CreateCountryRequest { NameAr = "أ", NameEn = "J" }), CancellationToken.None);

        var cityHandler = new CreateCityHandler(_uow, _mapper);
        var city1 = await cityHandler.Handle(new CreateCityCommand(new CreateCityRequest { NameAr = "أ", NameEn = "C1", CountryId = country.Data!.Id }), CancellationToken.None);
        var city2 = await cityHandler.Handle(new CreateCityCommand(new CreateCityRequest { NameAr = "ب", NameEn = "C2", CountryId = country.Data!.Id }), CancellationToken.None);

        var districtHandler = new CreateDistrictHandler(_uow, _mapper);
        await districtHandler.Handle(new CreateDistrictCommand(new CreateDistrictRequest { NameAr = "أ", NameEn = "D1", CityId = city1.Data!.Id }), CancellationToken.None);
        await districtHandler.Handle(new CreateDistrictCommand(new CreateDistrictRequest { NameAr = "ب", NameEn = "D2", CityId = city2.Data!.Id }), CancellationToken.None);

        var handler = new GetDistrictsByCityHandler(_uow, _mapper);
        var result = await handler.Handle(new GetDistrictsByCityQuery(city1.Data.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Data!.Count);
        Assert.AreEqual("D1", result.Data[0].NameEn);
    }
}
