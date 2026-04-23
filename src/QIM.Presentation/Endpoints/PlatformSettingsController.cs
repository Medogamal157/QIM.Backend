using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Content;
using QIM.Application.Features.PlatformSettings;

namespace QIM.Presentation.Endpoints;

[Route("api/admin/platform-settings")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class PlatformSettingsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PlatformSettingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => FromResult(await _mediator.Send(new GetAllPlatformSettingsQuery()));

    [HttpGet("by-group/{group}")]
    public async Task<IActionResult> GetByGroup(string group)
        => FromResult(await _mediator.Send(new GetPlatformSettingsByGroupQuery(group)));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlatformSettingRequest request)
        => FromResult(await _mediator.Send(new UpdatePlatformSettingCommand(id, request)));
}
