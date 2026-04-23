using QIM.Domain.Common;

namespace QIM.Domain.Entities;

public class City : BaseAuditableEntity
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int CountryId { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Navigation
    public Country Country { get; set; } = null!;
    public ICollection<District> Districts { get; set; } = new List<District>();
}
