using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Activity;
using QIM.Application.Features.Activities;

namespace QIM.Presentation.Endpoints;

[Route("api/admin/activities")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ActivitiesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ActivitiesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => FromResult(await _mediator.Send(new GetAllActivitiesQuery()));

    [HttpGet("tree")]
    public async Task<IActionResult> GetTree()
        => FromResult(await _mediator.Send(new GetActivityTreeQuery()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await _mediator.Send(new GetActivityByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActivityRequest request)
        => FromResult(await _mediator.Send(new CreateActivityCommand(request)));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateActivityRequest request)
        => FromResult(await _mediator.Send(new UpdateActivityCommand(id, request)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await _mediator.Send(new DeleteActivityCommand(id)));
}
