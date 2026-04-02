using QIM.Domain.Common;

namespace QIM.Domain.Entities;

public class Activity : BaseAuditableEntity
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? IconUrl { get; set; }
    public string? Color { get; set; }
    public int? ParentActivityId { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Navigation
    public Activity? ParentActivity { get; set; }
    public ICollection<Activity> SubActivities { get; set; } = new List<Activity>();
    public ICollection<Speciality> Specialities { get; set; } = new List<Speciality>();
    public ICollection<Business> Businesses { get; set; } = new List<Business>();
}
