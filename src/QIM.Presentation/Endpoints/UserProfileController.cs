using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QIM.Application.DTOs.Business;
using QIM.Application.Features.Users;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities.Identity;
using QIM.Shared.Models;

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
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminUserManagementController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserManagementController(IMediator mediator, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetPlatformUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isVerified = null,
        [FromQuery] UserType? userType = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _userManager.Users
            .Where(u => !u.IsDeleted && (u.UserType == UserType.Client || u.UserType == UserType.Provider));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(q) ||
                (u.Email != null && u.Email.ToLower().Contains(q)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(q)));
        }

        if (isActive.HasValue) query = query.Where(u => u.IsActive == isActive.Value);
        if (isVerified.HasValue) query = query.Where(u => u.IsVerified == isVerified.Value);
        if (userType.HasValue) query = query.Where(u => u.UserType == userType.Value);

        var total = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                ProfileImageUrl = u.ProfileImageUrl,
                UserType = u.UserType,
                IsVerified = u.IsVerified,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return FromResult(PaginatedResult<UserProfileDto>.Success(users, total, page, pageSize));
    }

    [HttpPatch("{userId}/toggle-active")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ToggleActive(string userId)
        => FromResult(await _mediator.Send(new ToggleUserActiveCommand(userId)));

    [HttpPatch("{userId}/verify")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> VerifyUser(string userId)
        => FromResult(await _mediator.Send(new VerifyUserCommand(userId)));

    [HttpPatch("{userId}/reject-verification")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> RejectVerification(string userId)
        => FromResult(await _mediator.Send(new RejectUserVerificationCommand(userId)));

    [HttpDelete("{userId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SoftDeleteUser(string userId)
        => FromResult(await _mediator.Send(new SoftDeleteUserCommand(userId)));

    [HttpPatch("{userId}/reset-password")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ResetPassword(string userId, [FromBody] AdminResetPasswordRequest request)
        => FromResult(await _mediator.Send(new ResetUserPasswordCommand(userId, request.NewPassword)));
}

public class AdminResetPasswordRequest
{
    public string NewPassword { get; set; } = null!;
}
