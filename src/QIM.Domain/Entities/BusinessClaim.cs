using QIM.Domain.Common;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities.Identity;

namespace QIM.Domain.Entities;

public class BusinessClaim : BaseAuditableEntity
{
    public int BusinessId { get; set; }
    public string UserId { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Message { get; set; }
    public string? DocumentUrls { get; set; }  // JSON array
    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

    // Navigation
    public Business Business { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
