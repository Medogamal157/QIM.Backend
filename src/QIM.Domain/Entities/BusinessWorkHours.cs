using QIM.Domain.Common;

namespace QIM.Domain.Entities;

public class BusinessWorkHours : BaseEntity
{
    public int BusinessId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public bool IsClosed { get; set; }

    // Navigation
    public Business Business { get; set; } = null!;
}
