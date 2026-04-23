using QIM.Domain.Common;

namespace QIM.Domain.Entities;

public class District : BaseAuditableEntity
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int CityId { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Navigation
    public City City { get; set; } = null!;
}
