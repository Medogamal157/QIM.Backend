using Microsoft.Extensions.DependencyInjection;
using QIM.Application.Common.Mappings;
using QIM.Application.DTOs.Business;
using QIM.Application.Features.Contacts;
using QIM.Application.Interfaces;
using QIM.Application.Interfaces.Repositories;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities;
using QIM.Persistence.Repositories;
using AutoMapper;

namespace QIM.Tests.Phase4;

[TestClass]
public class ContactSuggestionHandlerTests : TestBase
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

    // ── Contact Requests ──

    [TestMethod]
    public async Task CreateContactRequest_ReturnsSuccess()
    {
        var handler = new CreateContactRequestHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateContactRequestCommand(new CreateContactRequest
            {
                Name = "Ali",
                Email = "ali@test.com",
                Phone = "0790000000",
                Message = "Need help"
            }), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(ContactStatus.New, result.Data!.Status);
        Assert.AreEqual("Ali", result.Data.Name);
    }

    [TestMethod]
    public async Task GetAllContactRequests_ReturnsList()
    {
        var createHandler = new CreateContactRequestHandler(_uow, _mapper);
        await createHandler.Handle(
            new CreateContactRequestCommand(new CreateContactRequest
            {
                Name = "Ali",
                Message = "Msg1"
            }), CancellationToken.None);

        var handler = new GetAllContactRequestsHandler(_uow, _mapper);
        var result = await handler.Handle(new GetAllContactRequestsQuery(1, 10, null), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.TotalCount);
    }

    [TestMethod]
    public async Task UpdateContactRequestStatus_ReturnsUpdated()
    {
        var createHandler = new CreateContactRequestHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateContactRequestCommand(new CreateContactRequest
            {
                Name = "Ali",
                Message = "Help"
            }), CancellationToken.None);

        var handler = new UpdateContactStatusHandler(_uow, _mapper);
        var result = await handler.Handle(
            new UpdateContactStatusCommand(created.Data!.Id, ContactStatus.InProgress, "Working on it"),
            CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(ContactStatus.InProgress, result.Data!.Status);
        Assert.AreEqual("Working on it", result.Data.AdminNotes);
    }

    [TestMethod]
    public async Task GetContactRequestById_ReturnsCorrect()
    {
        var createHandler = new CreateContactRequestHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateContactRequestCommand(new CreateContactRequest
            {
                Name = "Sara",
                Message = "Question"
            }), CancellationToken.None);

        var handler = new GetContactRequestByIdHandler(_uow, _mapper);
        var result = await handler.Handle(new GetContactRequestByIdQuery(created.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Sara", result.Data!.Name);
    }

    // ── Suggestions ──

    [TestMethod]
    public async Task CreateSuggestion_ReturnsSuccess()
    {
        var handler = new CreateSuggestionHandler(_uow, _mapper);
        var result = await handler.Handle(
            new CreateSuggestionCommand(new CreateSuggestionRequest
            {
                Name = "Omar",
                Email = "omar@test.com",
                Message = "Add feature X"
            }), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(SuggestionStatus.New, result.Data!.Status);
    }

    [TestMethod]
    public async Task GetAllSuggestions_ReturnsList()
    {
        var createHandler = new CreateSuggestionHandler(_uow, _mapper);
        await createHandler.Handle(
            new CreateSuggestionCommand(new CreateSuggestionRequest
            {
                Name = "Layla",
                Message = "Suggestion"
            }), CancellationToken.None);

        var handler = new GetAllSuggestionsHandler(_uow, _mapper);
        var result = await handler.Handle(new GetAllSuggestionsQuery(1, 10, null), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.TotalCount);
    }

    [TestMethod]
    public async Task UpdateSuggestionStatus_ReturnsUpdated()
    {
        var createHandler = new CreateSuggestionHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateSuggestionCommand(new CreateSuggestionRequest
            {
                Name = "Layla",
                Message = "Suggestion"
            }), CancellationToken.None);

        var handler = new UpdateSuggestionStatusHandler(_uow, _mapper);
        var result = await handler.Handle(
            new UpdateSuggestionStatusCommand(created.Data!.Id, SuggestionStatus.Reviewed),
            CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(SuggestionStatus.Reviewed, result.Data!.Status);
    }

    [TestMethod]
    public async Task GetSuggestionById_ReturnsCorrect()
    {
        var createHandler = new CreateSuggestionHandler(_uow, _mapper);
        var created = await createHandler.Handle(
            new CreateSuggestionCommand(new CreateSuggestionRequest
            {
                Name = "Ahmad",
                Message = "Idea"
            }), CancellationToken.None);

        var handler = new GetSuggestionByIdHandler(_uow, _mapper);
        var result = await handler.Handle(new GetSuggestionByIdQuery(created.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Ahmad", result.Data!.Name);
    }
}
