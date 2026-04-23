using QIM.Domain.Common;

namespace QIM.Domain.Entities;

public class Advertisement : BaseAuditableEntity
{
    public string TitleAr { get; set; } = null!;
    public string TitleEn { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string? TargetUrl { get; set; }
    public string? Position { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
