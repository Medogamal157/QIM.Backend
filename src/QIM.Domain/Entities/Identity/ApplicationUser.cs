using Microsoft.AspNetCore.Identity;
using QIM.Domain.Common.Enums;

namespace QIM.Domain.Entities.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public UserType UserType { get; set; } = UserType.Client;
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Business> Businesses { get; set; } = new List<Business>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    public ICollection<BusinessClaim> BusinessClaims { get; set; } = new List<BusinessClaim>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
