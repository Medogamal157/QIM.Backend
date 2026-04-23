using QIM.Domain.Common;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities.Identity;

namespace QIM.Domain.Entities;

public class Business : BaseAuditableEntity
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string OwnerId { get; set; } = null!;
    public int ActivityId { get; set; }
    public int? SpecialityId { get; set; }
    public BusinessStatus Status { get; set; } = BusinessStatus.Pending;
    public string? RejectionReason { get; set; }
    public string? LogoUrl { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? WhatsApp { get; set; }
    public string? Phones { get; set; }  // JSON array
    public string? AccountCode { get; set; }
    public bool IsVerified { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }

    // Navigation
    public ApplicationUser Owner { get; set; } = null!;
    public Activity Activity { get; set; } = null!;
    public Speciality? Speciality { get; set; }
    public ICollection<BusinessAddress> Addresses { get; set; } = new List<BusinessAddress>();
    public ICollection<BusinessWorkHours> WorkHours { get; set; } = new List<BusinessWorkHours>();
    public ICollection<BusinessImage> Images { get; set; } = new List<BusinessImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<BusinessClaim> Claims { get; set; } = new List<BusinessClaim>();
    public ICollection<BusinessKeyword> Keywords { get; set; } = new List<BusinessKeyword>();
}
