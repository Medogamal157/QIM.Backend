namespace QIM.Application.DTOs.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string UserType { get; set; } = null!;
    public bool IsVerified { get; set; }
    public IList<string> Roles { get; set; } = [];
}
