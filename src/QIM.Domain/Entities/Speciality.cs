using QIM.Domain.Common;

namespace QIM.Domain.Entities;

public class Speciality : BaseAuditableEntity
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int ActivityId { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Navigation
    public Activity Activity { get; set; } = null!;
    public ICollection<Business> Businesses { get; set; } = new List<Business>();
}
