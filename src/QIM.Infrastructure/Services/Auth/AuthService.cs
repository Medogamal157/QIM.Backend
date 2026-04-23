using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using QIM.Application.DTOs.Auth;
using QIM.Application.Interfaces;
using QIM.Application.Interfaces.Auth;
using QIM.Application.Interfaces.Services;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities;
using QIM.Domain.Entities.Identity;
using QIM.Shared.Models;

namespace QIM.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _uow;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenStore refreshTokenStore,
        IConfiguration configuration,
        IEmailService emailService,
        IUnitOfWork uow)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenStore = refreshTokenStore;
        _configuration = configuration;
        _emailService = emailService;
        _uow = uow;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return Result<AuthResponse>.Failure("Passwords do not match.");

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Result<AuthResponse>.Failure("An account with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            UserType = UserType.Client,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Result<AuthResponse>.Failure(result.Errors.Select(e => e.Description).ToList());

        // Client self-registration always gets the Client role — never Admin / Provider.
        await _userManager.AddToRoleAsync(user, "Client");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<Result<AuthResponse>> RegisterBusinessAsync(RegisterBusinessRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return Result<AuthResponse>.Failure("Passwords do not match.");
        if (!request.AgreeTerms)
            return Result<AuthResponse>.Failure("You must agree to the terms to continue.");
        if (string.IsNullOrWhiteSpace(request.NameAr) && string.IsNullOrWhiteSpace(request.NameEn))
            return Result<AuthResponse>.Failure("Business name is required.");
        if (request.ActivityId <= 0)
            return Result<AuthResponse>.Failure("Activity is required.");

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Result<AuthResponse>.Failure("An account with this email already exists.");

        var phoneList = (request.PhoneNumbers ?? new List<string>())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .ToList();
        var primaryPhone = request.WhatsApp ?? phoneList.FirstOrDefault();

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = !string.IsNullOrWhiteSpace(request.NameAr) ? request.NameAr : request.NameEn,
            PhoneNumber = primaryPhone,
            UserType = UserType.Provider,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return Result<AuthResponse>.Failure(createResult.Errors.Select(e => e.Description).ToList());

        // Provider role is set server-side — never trust client input for role assignment.
        await _userManager.AddToRoleAsync(user, "Provider");

        try
        {
            // ── Create the Business in Pending status (awaits admin approval) ──
            var business = new Business
            {
                NameAr = !string.IsNullOrWhiteSpace(request.NameAr) ? request.NameAr : request.NameEn,
                NameEn = !string.IsNullOrWhiteSpace(request.NameEn) ? request.NameEn : request.NameAr,
                DescriptionAr = request.DescriptionAr,
                DescriptionEn = request.DescriptionEn,
                OwnerId = user.Id,
                ActivityId = request.ActivityId,
                SpecialityId = request.SpecialityId,
                Status = BusinessStatus.Pending,
                Email = request.Email,
                Website = request.Website,
                Facebook = request.Facebook,
                Instagram = request.Instagram,
                WhatsApp = request.WhatsApp,
                Phones = phoneList.Count > 0
                    ? System.Text.Json.JsonSerializer.Serialize(phoneList)
                    : null,
                AccountCode = phoneList.FirstOrDefault(),
                Rating = 0,
                ReviewCount = 0
            };

            await _uow.Businesses.AddAsync(business);
            await _uow.SaveChangesAsync();

            // Keywords
            foreach (var kw in (request.Keywords ?? new List<string>())
                         .Where(k => !string.IsNullOrWhiteSpace(k))
                         .Select(k => k.Trim())
                         .Distinct())
            {
                await _uow.BusinessKeywords.AddAsync(new BusinessKeyword
                {
                    BusinessId = business.Id,
                    Keyword = kw
                });
            }

            // Addresses (first one is primary)
            var validAddresses = (request.Addresses ?? new List<RegisterBusinessAddressDto>())
                .Where(a => a.CountryId.HasValue || a.CityId.HasValue || a.DistrictId.HasValue
                            || !string.IsNullOrWhiteSpace(a.StreetName))
                .ToList();
            for (var i = 0; i < validAddresses.Count; i++)
            {
                var a = validAddresses[i];
                await _uow.BusinessAddresses.AddAsync(new BusinessAddress
                {
                    BusinessId = business.Id,
                    CountryId = a.CountryId,
                    CityId = a.CityId,
                    DistrictId = a.DistrictId,
                    StreetName = a.StreetName,
                    BuildingNumber = a.BuildingNumber,
                    MapUrl = a.MapUrl,
                    IsPrimary = i == 0
                });
            }

            // Work hours
            foreach (var wh in request.WorkHours ?? new List<RegisterBusinessWorkHoursDto>())
            {
                TimeSpan? open = null, close = null;
                if (!wh.IsClosed)
                {
                    if (TimeSpan.TryParse(wh.OpenTime, out var o)) open = o;
                    if (TimeSpan.TryParse(wh.CloseTime, out var c)) close = c;
                }
                await _uow.BusinessWorkHoursRepo.AddAsync(new BusinessWorkHours
                {
                    BusinessId = business.Id,
                    DayOfWeek = (DayOfWeek)wh.DayOfWeek,
                    OpenTime = open,
                    CloseTime = close,
                    IsClosed = wh.IsClosed
                });
            }

            await _uow.SaveChangesAsync();

            var auth = await GenerateAuthResponseAsync(user);
            if (auth.IsSuccess && auth.Data is not null)
                auth.Data.BusinessId = business.Id;
            return auth;
        }
        catch (Exception ex)
        {
            // Roll back the user so a half-registered provider doesn't get stuck.
            try { await _userManager.DeleteAsync(user); } catch { /* best effort */ }
            return Result<AuthResponse>.Failure($"Failed to register business: {ex.Message}");
        }
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result<AuthResponse>.Failure("Invalid email or password.");

        if (!user.IsActive)
            return Result<AuthResponse>.Failure("Your account has been deactivated.");

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            return Result<AuthResponse>.Failure("Invalid email or password.");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<Result<AuthResponse>> AdminLoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result<AuthResponse>.Failure("Invalid email or password.");

        if (!user.IsActive)
            return Result<AuthResponse>.Failure("Your account has been deactivated.");

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            return Result<AuthResponse>.Failure("Invalid email or password.");

        // Verify user has an admin-level role
        var roles = await _userManager.GetRolesAsync(user);
        var adminRoles = new[] { "SuperAdmin", "Admin", "Moderator", "Support" };
        if (!roles.Any(r => adminRoles.Contains(r)))
            return Result<AuthResponse>.Failure("Access denied. Admin privileges required.");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var storedToken = await _refreshTokenStore.GetByTokenAsync(request.RefreshToken);
        if (storedToken is null)
            return Result<AuthResponse>.Failure("Invalid refresh token.");

        if (storedToken.IsRevoked)
            return Result<AuthResponse>.Failure("Refresh token has been revoked.");

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            return Result<AuthResponse>.Failure("Refresh token has expired.");

        // Revoke old token (rotation)
        await _refreshTokenStore.RevokeAsync(request.RefreshToken);

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user is null)
            return Result<AuthResponse>.Failure("User not found.");

        if (!user.IsActive)
            return Result<AuthResponse>.Failure("Your account has been deactivated.");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<Result> LogoutAsync(string refreshToken)
    {
        await _refreshTokenStore.RevokeAsync(refreshToken);
        return Result.Success("Logged out successfully.");
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return Result.Failure(result.Errors.Select(e => e.Description).ToList());

        // Revoke all refresh tokens for security
        await _refreshTokenStore.RevokeAllForUserAsync(userId);

        return Result.Success("Password changed successfully.");
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Success("If the email exists, a reset link has been sent.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(user.Email ?? request.Email);
        var resetLink = $"{frontendUrl.TrimEnd('/')}/reset-password?email={encodedEmail}&token={encodedToken}";

        var html = $@"
<!DOCTYPE html>
<html>
<body style=""font-family: Arial, sans-serif; background:#f6f6f6; padding:24px;"">
  <div style=""max-width:560px; margin:auto; background:#fff; border-radius:8px; padding:32px;"">
    <h2 style=""color:#1a365d;"">Reset your password</h2>
    <p>Hello,</p>
    <p>We received a request to reset the password for your Quayyem account.</p>
    <p style=""text-align:center; margin:28px 0;"">
      <a href=""{resetLink}"" style=""background:#1a365d; color:#fff; padding:12px 24px; border-radius:6px; text-decoration:none; display:inline-block;"">Reset Password</a>
    </p>
    <p>Or copy this link into your browser:</p>
    <p style=""word-break:break-all; color:#555;"">{resetLink}</p>
    <p>If you did not request this, you can safely ignore this email.</p>
    <hr style=""border:none; border-top:1px solid #eee; margin:24px 0;"" />
    <p style=""color:#888; font-size:12px;"">Quayyem - {DateTime.UtcNow.Year}</p>
  </div>
</body>
</html>";

        try
        {
            await _emailService.SendEmailAsync(request.Email, "Reset your Quayyem password", html);
        }
        catch
        {
            // Swallow — do not leak email existence or SMTP failures.
        }

        return Result.Success("If the email exists, a reset link has been sent.");
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Failure("Invalid request.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return Result.Failure(result.Errors.Select(e => e.Description).ToList());

        // Revoke all refresh tokens
        await _refreshTokenStore.RevokeAllForUserAsync(user.Id);

        return Result.Success("Password has been reset successfully.");
    }

    // ─── Helper ───

    private async Task<Result<AuthResponse>> GenerateAuthResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshTokenDays = int.Parse(
            _configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");

        // Save refresh token
        await _refreshTokenStore.SaveAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays)
        });

        var expirationMinutes = int.Parse(
            _configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "60");

        return Result<AuthResponse>.Success(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                UserType = user.UserType.ToString(),
                IsVerified = user.IsVerified,
                Roles = roles
            }
        });
    }
    public async Task<Result<UserDto>> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<UserDto>.Failure("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        return Result<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            UserType = user.UserType.ToString(),
            IsVerified = user.IsVerified,
            Roles = roles
        });
    }}
