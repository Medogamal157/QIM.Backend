using QIM.Domain.Common;

namespace QIM.Domain.Entities;

public class BusinessImage : BaseEntity
{
    public int BusinessId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsCover { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public Business Business { get; set; } = null!;
}
