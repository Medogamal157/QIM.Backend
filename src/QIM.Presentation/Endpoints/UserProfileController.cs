using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Business;
using QIM.Application.Features.Users;

namespace QIM.Presentation.Endpoints;

// ── User Profile Controller ──

[Route("api/profile")]
[Authorize]
public class ProfileController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new GetUserProfileQuery(userId)));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new UpdateUserProfileCommand(userId, request)));
    }

    [HttpPatch("image")]
    public async Task<IActionResult> UpdateImage([FromQuery] string imageUrl)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new UpdateProfileImageCommand(userId, imageUrl)));
    }

    [HttpGet("provider-account")]
    public async Task<IActionResult> GetProviderAccount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new GetProviderAccountQuery(userId)));
    }
}

// ── Admin Dashboard / Analytics Controller ──

[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DashboardController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
        => FromResult(await _mediator.Send(new GetDashboardStatsQuery()));

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
        => FromResult(await _mediator.Send(new GetDetailedAnalyticsQuery()));
}

// ── Admin toggle user active ──

[Route("api/admin/platform-users")]
[Authorize(Roles = "SuperAdmin")]
public class AdminUserManagementController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AdminUserManagementController(IMediator mediator) => _mediator = mediator;

    [HttpPatch("{userId}/toggle-active")]
    public async Task<IActionResult> ToggleActive(string userId)
        => FromResult(await _mediator.Send(new ToggleUserActiveCommand(userId)));

    [HttpPatch("{userId}/verify")]
    public async Task<IActionResult> VerifyUser(string userId)
        => FromResult(await _mediator.Send(new VerifyUserCommand(userId)));

    [HttpPatch("{userId}/reject-verification")]
    public async Task<IActionResult> RejectVerification(string userId)
        => FromResult(await _mediator.Send(new RejectUserVerificationCommand(userId)));

    [HttpDelete("{userId}")]
    public async Task<IActionResult> SoftDeleteUser(string userId)
        => FromResult(await _mediator.Send(new SoftDeleteUserCommand(userId)));

    [HttpPatch("{userId}/reset-password")]
    public async Task<IActionResult> ResetPassword(string userId, [FromBody] AdminResetPasswordRequest request)
        => FromResult(await _mediator.Send(new ResetUserPasswordCommand(userId, request.NewPassword)));
}

public class AdminResetPasswordRequest
{
    public string NewPassword { get; set; } = null!;
}
