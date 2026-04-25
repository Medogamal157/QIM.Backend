namespace QIM.Application.DTOs.Admin;

public class AdminUserDto
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public string Role => Roles.FirstOrDefault() ?? "";
}

public class CreateAdminUserRequest
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = "Admin"; // Admin or SuperAdmin
}

public class UpdateAdminUserRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsActive { get; set; }
}

public class ChangeAdminRoleRequest
{
    public string Role { get; set; } = null!; // Admin or SuperAdmin
}
