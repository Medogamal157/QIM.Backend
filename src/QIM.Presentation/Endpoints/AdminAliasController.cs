using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QIM.Application.DTOs.Business;
using QIM.Application.Features.AdminUsers;
using QIM.Application.Features.Users;
using QIM.Domain.Entities.Identity;
using QIM.Shared.Models;

namespace QIM.Presentation.Endpoints;

// ── Admin route aliases (DEF-NEW-011/012/016) ──
// These routes mirror the URLs the admin sidebar uses, so probing tools and
// alternate clients reach a real endpoint instead of receiving 404.

[Route("api/admin/admin-users")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminUsersAliasController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public AdminUsersAliasController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => FromResult(await _mediator.Send(new GetAllAdminUsersQuery()));
}

[Route("api/admin/analytics")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminAnalyticsAliasController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public AdminAnalyticsAliasController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Get()
        => FromResult(await _mediator.Send(new GetDetailedAnalyticsQuery()));
}

[Route("api/admin/account-verification")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AccountVerificationController : ApiControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    public AccountVerificationController(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    // Returns platform users (Provider/Client) that have not been verified yet.
    [HttpGet]
    public async Task<IActionResult> GetPending()
    {
        var pending = await _userManager.Users
            .Where(u => !u.IsVerified
                && (u.UserType == QIM.Domain.Common.Enums.UserType.Provider
                 || u.UserType == QIM.Domain.Common.Enums.UserType.Client))
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email!,
                PhoneNumber = u.PhoneNumber,
                ProfileImageUrl = u.ProfileImageUrl,
                UserType = u.UserType,
                IsVerified = u.IsVerified,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return FromResult(Result<List<UserProfileDto>>.Success(pending));
    }
}
