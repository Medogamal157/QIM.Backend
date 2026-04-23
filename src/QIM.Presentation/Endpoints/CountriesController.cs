using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Location;
using QIM.Application.Features.Countries;

namespace QIM.Presentation.Endpoints;

[Route("api/admin/countries")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class CountriesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CountriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => FromResult(await _mediator.Send(new GetAllCountriesQuery()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await _mediator.Send(new GetCountryByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCountryRequest request)
        => FromResult(await _mediator.Send(new CreateCountryCommand(request)));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCountryRequest request)
        => FromResult(await _mediator.Send(new UpdateCountryCommand(id, request)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await _mediator.Send(new DeleteCountryCommand(id)));

    [HttpPatch("{id:int}/toggle-enabled")]
    public async Task<IActionResult> ToggleEnabled(int id)
        => FromResult(await _mediator.Send(new ToggleCountryEnabledCommand(id)));

    [HttpPatch("{id:int}/set-default")]
    public async Task<IActionResult> SetDefault(int id)
        => FromResult(await _mediator.Send(new SetDefaultCountryCommand(id)));

    [HttpPatch("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<int> orderedIds)
        => FromResult(await _mediator.Send(new ReorderCountriesCommand(orderedIds)));
}
