using QIM.Application.DTOs.Auth;
using QIM.Shared.Models;

namespace QIM.Application.Interfaces.Auth;

/// <summary>
/// Authentication service contract — register, login, JWT, refresh, password management.
/// </summary>
public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<AuthResponse>> AdminLoginAsync(LoginRequest request);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<Result> LogoutAsync(string refreshToken);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
    Task<Result<UserDto>> GetProfileAsync(string userId);
}
