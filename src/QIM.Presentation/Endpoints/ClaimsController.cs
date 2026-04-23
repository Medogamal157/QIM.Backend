using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Business;
using QIM.Application.Features.BusinessClaims;

namespace QIM.Presentation.Endpoints;

// ── Admin Claims Controller ──

[Route("api/admin/claims")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminClaimsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AdminClaimsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Domain.Common.Enums.ClaimStatus? status = null)
        => FromResult(await _mediator.Send(new GetAllClaimsQuery(page, pageSize, status)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await _mediator.Send(new GetClaimByIdQuery(id)));

    [HttpPatch("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
        => FromResult(await _mediator.Send(new ApproveClaimCommand(id)));

    [HttpPatch("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id)
        => FromResult(await _mediator.Send(new RejectClaimCommand(id)));
}

// ── Auth Claims Controller (submit claims) ──

[Route("api/claims")]
[Authorize]
public class ClaimsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ClaimsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBusinessClaimRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new CreateBusinessClaimCommand(request, userId)));
    }
}
