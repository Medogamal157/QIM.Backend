using QIM.Domain.Common;

namespace QIM.Domain.Entities;

public class PlatformSetting : BaseEntity
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? Group { get; set; }
}
