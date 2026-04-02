using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using QIM.Application.DTOs.Auth;
using QIM.Application.Interfaces.Auth;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities.Identity;
using QIM.Shared.Models;

namespace QIM.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenStore refreshTokenStore,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenStore = refreshTokenStore;
        _configuration = configuration;
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
            UserType = request.UserType,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Result<AuthResponse>.Failure(result.Errors.Select(e => e.Description).ToList());

        // Assign role based on UserType
        var roleName = request.UserType switch
        {
            UserType.Provider => "Provider",
            UserType.Admin => "Admin",
            _ => "Client"
        };
        await _userManager.AddToRoleAsync(user, roleName);

        return await GenerateAuthResponseAsync(user);
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

        // TODO: Phase 5+ will send email via IEmailService
        // For now, return the token in the response (dev/test only)
        return Result.Success($"Password reset token generated. Token: {token}");
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
