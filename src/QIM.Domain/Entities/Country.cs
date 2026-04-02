using QIM.Domain.Common;

namespace QIM.Domain.Entities;

public class Country : BaseAuditableEntity
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool IsEnabled { get; set; } = true;
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public ICollection<City> Cities { get; set; } = new List<City>();
}
