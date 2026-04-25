using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QIM.Application.DTOs.Auth;
using QIM.Application.Interfaces.Auth;
using System.Security.Claims;

namespace QIM.Presentation.Endpoints;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>
    /// Register a new client or provider account.
    /// </summary>
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Three-step business registration. Creates the provider user (role=Provider)
    /// and a pending Business record that requires admin approval before appearing in search.
    /// </summary>
    [EnableRateLimiting("auth")]
    [HttpPost("register-business")]
    public async Task<IActionResult> RegisterBusiness([FromBody] RegisterBusinessRequest request)
    {
        var result = await _authService.RegisterBusinessAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Login with email and password. Returns JWT + refresh token.
    /// </summary>
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return result.IsSuccess ? Ok(result) : Unauthorized(result);
    }

    /// <summary>
    /// Admin-only login. Validates user has admin-level role.
    /// </summary>
    [EnableRateLimiting("auth")]
    [HttpPost("admin-login")]
    public async Task<IActionResult> AdminLogin([FromBody] LoginRequest request)
    {
        var result = await _authService.AdminLoginAsync(request);
        if (!result.IsSuccess)
        {
            // Return 403 if the error is about access denied (user is valid but not admin)
            if (result.Errors.Any(e => e.Contains("Access denied")))
                return StatusCode(403, result);
            return Unauthorized(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Refresh access token using a valid refresh token.
    /// </summary>
    // DEF-035: keep `/refresh` for backwards compatibility and expose the more conventional `/refresh-token` alias.
    [HttpPost("refresh")]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return result.IsSuccess ? Ok(result) : Unauthorized(result);
    }

    /// <summary>
    /// Logout — revokes the refresh token.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.LogoutAsync(request.RefreshToken);
        return Ok(result);
    }

    /// <summary>
    /// Change password for authenticated user.
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var result = await _authService.ChangePasswordAsync(userId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Request password reset token (sent via email in production).
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Reset password using email + token.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get the current authenticated user's profile.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var result = await _authService.GetProfileAsync(userId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
}
