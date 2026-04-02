using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Business;
using QIM.Application.Features.Reviews;

namespace QIM.Presentation.Endpoints;

// ── Admin Reviews Controller ──

[Route("api/admin/reviews")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminReviewsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AdminReviewsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Domain.Common.Enums.ReviewStatus? status = null)
        => FromResult(await _mediator.Send(new GetAllReviewsQuery(page, pageSize, status)));

    [HttpPatch("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
        => FromResult(await _mediator.Send(new ApproveReviewCommand(id)));

    [HttpPatch("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id)
        => FromResult(await _mediator.Send(new RejectReviewCommand(id)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await _mediator.Send(new DeleteReviewCommand(id)));
}

// ── Public / Auth Reviews Controller ──

[Route("api/reviews")]
public class ReviewsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("business/{businessId:int}")]
    public async Task<IActionResult> GetBusinessReviews(
        int businessId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        => FromResult(await _mediator.Send(new GetBusinessReviewsQuery(businessId, page, pageSize)));

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new CreateReviewCommand(request, userId)));
    }

    [Authorize]
    [HttpPost("{id:int}/flag")]
    public async Task<IActionResult> Flag(int id, [FromBody] FlagReviewRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new FlagReviewCommand(id, request.Reason, userId)));
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new GetUserReviewsQuery(userId, page, pageSize)));
    }
}
