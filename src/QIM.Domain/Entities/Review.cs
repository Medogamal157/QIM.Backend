using QIM.Domain.Common;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities.Identity;

namespace QIM.Domain.Entities;

public class Review : BaseAuditableEntity
{
    public int BusinessId { get; set; }
    public string UserId { get; set; } = null!;
    public int Rating { get; set; }  // 1-5
    public string? Comment { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public string? FlagReason { get; set; }
    public string? FlaggedByUserId { get; set; }

    // Navigation
    public Business Business { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public ApplicationUser? FlaggedByUser { get; set; }
}
