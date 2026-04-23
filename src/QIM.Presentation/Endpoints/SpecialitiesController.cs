using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Activity;
using QIM.Application.Features.Specialities;

namespace QIM.Presentation.Endpoints;

[Route("api/admin/specialities")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class SpecialitiesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public SpecialitiesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => FromResult(await _mediator.Send(new GetAllSpecialitiesQuery()));

    [HttpGet("by-activity/{activityId:int}")]
    public async Task<IActionResult> GetByActivity(int activityId)
        => FromResult(await _mediator.Send(new GetSpecialitiesByActivityQuery(activityId)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await _mediator.Send(new GetSpecialityByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSpecialityRequest request)
        => FromResult(await _mediator.Send(new CreateSpecialityCommand(request)));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSpecialityRequest request)
        => FromResult(await _mediator.Send(new UpdateSpecialityCommand(id, request)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await _mediator.Send(new DeleteSpecialityCommand(id)));

    [HttpPatch("{id:int}/toggle-enabled")]
    public async Task<IActionResult> ToggleEnabled(int id)
        => FromResult(await _mediator.Send(new ToggleSpecialityEnabledCommand(id)));
}
