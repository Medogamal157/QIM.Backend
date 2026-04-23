using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Location;
using QIM.Application.Features.Districts;

namespace QIM.Presentation.Endpoints;

[Route("api/admin/districts")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DistrictsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public DistrictsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => FromResult(await _mediator.Send(new GetAllDistrictsQuery()));

    [HttpGet("by-city/{cityId:int}")]
    public async Task<IActionResult> GetByCity(int cityId)
        => FromResult(await _mediator.Send(new GetDistrictsByCityQuery(cityId)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await _mediator.Send(new GetDistrictByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDistrictRequest request)
        => FromResult(await _mediator.Send(new CreateDistrictCommand(request)));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDistrictRequest request)
        => FromResult(await _mediator.Send(new UpdateDistrictCommand(id, request)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await _mediator.Send(new DeleteDistrictCommand(id)));

    [HttpPatch("{id:int}/toggle-enabled")]
    public async Task<IActionResult> ToggleEnabled(int id)
        => FromResult(await _mediator.Send(new ToggleDistrictEnabledCommand(id)));
}
