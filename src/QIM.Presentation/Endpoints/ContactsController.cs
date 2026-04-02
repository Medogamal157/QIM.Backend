using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Business;
using QIM.Application.Features.Contacts;
using QIM.Domain.Common.Enums;

namespace QIM.Presentation.Endpoints;

// ── Admin Contact Requests Controller ──

[Route("api/admin/contacts")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminContactsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AdminContactsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ContactStatus? status = null)
        => FromResult(await _mediator.Send(new GetAllContactRequestsQuery(page, pageSize, status)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await _mediator.Send(new GetContactRequestByIdQuery(id)));

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] ContactStatus status, [FromQuery] string? notes = null)
        => FromResult(await _mediator.Send(new UpdateContactStatusCommand(id, status, notes)));
}

// ── Admin Suggestions Controller ──

[Route("api/admin/suggestions")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminSuggestionsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AdminSuggestionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] SuggestionStatus? status = null)
        => FromResult(await _mediator.Send(new GetAllSuggestionsQuery(page, pageSize, status)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await _mediator.Send(new GetSuggestionByIdQuery(id)));

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] SuggestionStatus status)
        => FromResult(await _mediator.Send(new UpdateSuggestionStatusCommand(id, status)));
}

// ── Public Contact / Suggestion Controller ──

[Route("api/public")]
public class PublicContactController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PublicContactController(IMediator mediator) => _mediator = mediator;

    [HttpPost("contacts")]
    public async Task<IActionResult> CreateContact([FromBody] CreateContactRequest request)
        => FromResult(await _mediator.Send(new CreateContactRequestCommand(request)));

    [HttpPost("suggestions")]
    public async Task<IActionResult> CreateSuggestion([FromBody] CreateSuggestionRequest request)
        => FromResult(await _mediator.Send(new CreateSuggestionCommand(request)));
}
