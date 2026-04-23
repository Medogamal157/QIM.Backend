namespace QIM.Application.DTOs.Auth;

/// <summary>
/// Client self-registration request. UserType is determined by the endpoint (always Client).
/// Business owners must use the dedicated /api/auth/register-business endpoint.
/// </summary>
public class RegisterRequest
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}
