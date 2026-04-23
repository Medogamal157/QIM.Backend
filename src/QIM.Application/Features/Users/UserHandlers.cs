using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using QIM.Application.DTOs.Business;
using QIM.Application.Interfaces;
using QIM.Application.Interfaces.Services;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities.Identity;
using QIM.Shared.Models;

namespace QIM.Application.Features.Users;

// ══════════════════════════════════════════════
// ── User Profile Queries ──
// ══════════════════════════════════════════════

public record GetUserProfileQuery(string UserId) : IRequest<Result<UserProfileDto>>;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserProfileHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result<UserProfileDto>.Failure("User not found.");

        var dto = new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            UserType = user.UserType,
            IsVerified = user.IsVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Result<UserProfileDto>.Success(dto);
    }
}

// ══════════════════════════════════════════════
// ── User Profile Commands ──
// ══════════════════════════════════════════════

public record UpdateUserProfileCommand(string UserId, UpdateProfileRequest Data) : IRequest<Result<UserProfileDto>>;

public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, Result<UserProfileDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateUserProfileHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<Result<UserProfileDto>> Handle(UpdateUserProfileCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result<UserProfileDto>.Failure("User not found.");

        if (request.Data.FullName is not null)
            user.FullName = request.Data.FullName;
        if (request.Data.PhoneNumber is not null)
            user.PhoneNumber = request.Data.PhoneNumber;

        await _userManager.UpdateAsync(user);

        var dto = new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            UserType = user.UserType,
            IsVerified = user.IsVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Result<UserProfileDto>.Success(dto);
    }
}

public record UpdateProfileImageCommand(string UserId, string ImageUrl) : IRequest<Result<UserProfileDto>>;

public class UpdateProfileImageHandler : IRequestHandler<UpdateProfileImageCommand, Result<UserProfileDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _storage;

    public UpdateProfileImageHandler(UserManager<ApplicationUser> userManager, IFileStorageService storage)
    {
        _userManager = userManager;
        _storage = storage;
    }

    public async Task<Result<UserProfileDto>> Handle(UpdateProfileImageCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result<UserProfileDto>.Failure("User not found.");

        // Delete old image if exists
        if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            await _storage.DeleteAsync(user.ProfileImageUrl);

        user.ProfileImageUrl = request.ImageUrl;
        await _userManager.UpdateAsync(user);

        var dto = new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            UserType = user.UserType,
            IsVerified = user.IsVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Result<UserProfileDto>.Success(dto);
    }
}

// ══════════════════════════════════════════════
// ── Admin Toggle User Active ──
// ══════════════════════════════════════════════

public record ToggleUserActiveCommand(string UserId) : IRequest<Result<UserProfileDto>>;

public class ToggleUserActiveHandler : IRequestHandler<ToggleUserActiveCommand, Result<UserProfileDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ToggleUserActiveHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<Result<UserProfileDto>> Handle(ToggleUserActiveCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result<UserProfileDto>.Failure("User not found.");

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        var dto = new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            UserType = user.UserType,
            IsVerified = user.IsVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Result<UserProfileDto>.Success(dto);
    }
}

// ══════════════════════════════════════════════
// ── Admin Soft-Delete User ──
// ══════════════════════════════════════════════

public record SoftDeleteUserCommand(string UserId) : IRequest<Result>;

public class SoftDeleteUserHandler : IRequestHandler<SoftDeleteUserCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SoftDeleteUserHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<Result> Handle(SoftDeleteUserCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result.Failure("User not found.");

        user.IsDeleted = true;
        user.IsActive = false;
        await _userManager.UpdateAsync(user);
        return Result.Success("User deleted.");
    }
}

// ══════════════════════════════════════════════
// ── Admin Reset User Password ──
// ══════════════════════════════════════════════

public record ResetUserPasswordCommand(string UserId, string NewPassword) : IRequest<Result>;

public class ResetUserPasswordHandler : IRequestHandler<ResetUserPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetUserPasswordHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<Result> Handle(ResetUserPasswordCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result.Failure("User not found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!result.Succeeded)
            return Result.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));

        return Result.Success("Password reset successfully.");
    }
}

// ══════════════════════════════════════════════
// ── Analytics / Dashboard ──
// ══════════════════════════════════════════════

public record GetDashboardStatsQuery() : IRequest<Result<DashboardStatsDto>>;

public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetDashboardStatsHandler(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        var dto = new DashboardStatsDto
        {
            TotalUsers = _userManager.Users.Count(),
            TotalBusinesses = await _uow.Businesses.CountAsync(),
            TotalReviews = await _uow.Reviews.CountAsync(),
            PendingBusinesses = await _uow.Businesses.CountAsync(b => b.Status == BusinessStatus.Pending),
            PendingClaims = await _uow.BusinessClaims.CountAsync(c => c.Status == ClaimStatus.Pending),
            NewContacts = await _uow.ContactRequests.CountAsync(c => c.Status == ContactStatus.New)
        };

        return Result<DashboardStatsDto>.Success(dto);
    }
}

// ══════════════════════════════════════════════
// ── Provider Account ──
// ══════════════════════════════════════════════

public record GetProviderAccountQuery(string UserId) : IRequest<Result<ProviderAccountDto>>;

public class GetProviderAccountHandler : IRequestHandler<GetProviderAccountQuery, Result<ProviderAccountDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetProviderAccountHandler(UserManager<ApplicationUser> userManager, IUnitOfWork uow, IMapper mapper)
    {
        _userManager = userManager;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<ProviderAccountDto>> Handle(GetProviderAccountQuery request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result<ProviderAccountDto>.Failure("User not found.");

        var businesses = await _uow.Businesses.GetAllAsync(b => b.OwnerId == request.UserId);
        var totalReviews = businesses.Sum(b => b.ReviewCount);
        var avgRating = businesses.Count > 0
            ? businesses.Where(b => b.ReviewCount > 0).DefaultIfEmpty().Average(b => b?.Rating ?? 0)
            : 0;

        var dto = new ProviderAccountDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            IsVerified = user.IsVerified,
            TotalBusinesses = businesses.Count,
            TotalReviews = totalReviews,
            AverageRating = Math.Round(avgRating, 2),
            Businesses = _mapper.Map<List<BusinessListDto>>(businesses)
        };

        return Result<ProviderAccountDto>.Success(dto);
    }
}

// ══════════════════════════════════════════════
// ── Admin User Verification ──
// ══════════════════════════════════════════════

public record VerifyUserCommand(string UserId) : IRequest<Result<UserProfileDto>>;

public class VerifyUserHandler : IRequestHandler<VerifyUserCommand, Result<UserProfileDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public VerifyUserHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<Result<UserProfileDto>> Handle(VerifyUserCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result<UserProfileDto>.Failure("User not found.");

        user.IsVerified = true;
        await _userManager.UpdateAsync(user);

        return Result<UserProfileDto>.Success(MapToDto(user));
    }

    private static UserProfileDto MapToDto(ApplicationUser u) => new()
    {
        Id = u.Id, FullName = u.FullName, Email = u.Email,
        PhoneNumber = u.PhoneNumber, ProfileImageUrl = u.ProfileImageUrl,
        UserType = u.UserType, IsVerified = u.IsVerified, IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };
}

public record RejectUserVerificationCommand(string UserId) : IRequest<Result<UserProfileDto>>;

public class RejectUserVerificationHandler : IRequestHandler<RejectUserVerificationCommand, Result<UserProfileDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RejectUserVerificationHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<Result<UserProfileDto>> Handle(RejectUserVerificationCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result<UserProfileDto>.Failure("User not found.");

        user.IsVerified = false;
        await _userManager.UpdateAsync(user);

        return Result<UserProfileDto>.Success(MapToDto(user));
    }

    private static UserProfileDto MapToDto(ApplicationUser u) => new()
    {
        Id = u.Id, FullName = u.FullName, Email = u.Email,
        PhoneNumber = u.PhoneNumber, ProfileImageUrl = u.ProfileImageUrl,
        UserType = u.UserType, IsVerified = u.IsVerified, IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };
}

// ══════════════════════════════════════════════
// ── Detailed Analytics ──
// ══════════════════════════════════════════════

public record GetDetailedAnalyticsQuery() : IRequest<Result<AnalyticsDto>>;

public class GetDetailedAnalyticsHandler : IRequestHandler<GetDetailedAnalyticsQuery, Result<AnalyticsDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetDetailedAnalyticsHandler(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async Task<Result<AnalyticsDto>> Handle(GetDetailedAnalyticsQuery request, CancellationToken ct)
    {
        // User growth – last 6 months
        var now = DateTime.UtcNow;
        var userGrowth = new List<GrowthDataPoint>();
        for (int i = 5; i >= 0; i--)
        {
            var month = now.AddMonths(-i);
            var start = new DateTime(month.Year, month.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);
            var count = _userManager.Users.Count(u => u.CreatedAt >= start && u.CreatedAt < end);
            userGrowth.Add(new GrowthDataPoint { Period = start.ToString("yyyy-MM"), Count = count });
        }

        // Business trend – last 6 months
        var allBusinesses = await _uow.Businesses.GetAllAsync();
        var businessTrend = new List<GrowthDataPoint>();
        for (int i = 5; i >= 0; i--)
        {
            var month = now.AddMonths(-i);
            var start = new DateTime(month.Year, month.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);
            var count = allBusinesses.Count(b => b.CreatedAt >= start && b.CreatedAt < end);
            businessTrend.Add(new GrowthDataPoint { Period = start.ToString("yyyy-MM"), Count = count });
        }

        // Top activities
        var categories = await _uow.Activities.GetAllAsync();
        var topActivities = new List<ActivityDistribution>();
        foreach (var cat in categories)
        {
            var bizCount = await _uow.Businesses.CountAsync(b => b.ActivityId == cat.Id);
            if (bizCount > 0)
                topActivities.Add(new ActivityDistribution { ActivityName = cat.NameEn, BusinessCount = bizCount });
        }
        topActivities = topActivities.OrderByDescending(c => c.BusinessCount).Take(10).ToList();

        // Review distribution (1-5 stars)
        var allReviews = await _uow.Reviews.GetAllAsync();
        var reviewDistribution = new Dictionary<int, int>();
        for (int star = 1; star <= 5; star++)
            reviewDistribution[star] = allReviews.Count(r => r.Rating == star);

        var dto = new AnalyticsDto
        {
            UserGrowth = userGrowth,
            BusinessTrend = businessTrend,
            TopActivities = topActivities,
            ReviewDistribution = reviewDistribution
        };

        return Result<AnalyticsDto>.Success(dto);
    }
}
