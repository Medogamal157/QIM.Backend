using QIM.Domain.Common;

namespace QIM.Domain.Entities;

public class BusinessKeyword : BaseEntity
{
    public int BusinessId { get; set; }
    public string Keyword { get; set; } = null!;

    // Navigation
    public Business Business { get; set; } = null!;
}
