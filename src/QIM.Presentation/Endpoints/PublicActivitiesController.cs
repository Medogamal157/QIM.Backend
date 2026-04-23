using MediatR;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.Features.Activities;
using QIM.Application.Features.Specialities;

namespace QIM.Presentation.Endpoints;

[Route("api/public")]
public class PublicActivitiesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PublicActivitiesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("activities")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "search" })]
    public async Task<IActionResult> GetActivities([FromQuery] string? search)
        => FromResult(await _mediator.Send(new GetPublicActivityTreeQuery(search)));

    [HttpGet("specialities")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "activityId", "search" })]
    public async Task<IActionResult> GetSpecialities([FromQuery] int? activityId, [FromQuery] string? search)
    {
        if (activityId.HasValue)
            return FromResult(await _mediator.Send(new GetPublicSpecialitiesByActivityQuery(activityId.Value, search)));
        return FromResult(await _mediator.Send(new GetPublicAllSpecialitiesQuery(search)));
    }
}
