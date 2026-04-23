using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Location;
using QIM.Application.Features.Cities;

namespace QIM.Presentation.Endpoints;

[Route("api/admin/cities")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class CitiesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CitiesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => FromResult(await _mediator.Send(new GetAllCitiesQuery()));

    [HttpGet("by-country/{countryId:int}")]
    public async Task<IActionResult> GetByCountry(int countryId)
        => FromResult(await _mediator.Send(new GetCitiesByCountryQuery(countryId)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await _mediator.Send(new GetCityByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCityRequest request)
        => FromResult(await _mediator.Send(new CreateCityCommand(request)));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCityRequest request)
        => FromResult(await _mediator.Send(new UpdateCityCommand(id, request)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await _mediator.Send(new DeleteCityCommand(id)));

    [HttpPatch("{id:int}/toggle-enabled")]
    public async Task<IActionResult> ToggleEnabled(int id)
        => FromResult(await _mediator.Send(new ToggleCityEnabledCommand(id)));
}
