using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Content;
using QIM.Application.Features.Advertisements;

namespace QIM.Presentation.Endpoints;

[Route("api/admin/advertisements")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdvertisementsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AdvertisementsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => FromResult(await _mediator.Send(new GetAllAdvertisementsQuery()));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdvertisementRequest request)
        => FromResult(await _mediator.Send(new CreateAdvertisementCommand(request)));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAdvertisementRequest request)
        => FromResult(await _mediator.Send(new UpdateAdvertisementCommand(id, request)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await _mediator.Send(new DeleteAdvertisementCommand(id)));
}
