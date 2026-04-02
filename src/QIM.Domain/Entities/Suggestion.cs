using QIM.Domain.Common;
using QIM.Domain.Common.Enums;

namespace QIM.Domain.Entities;

public class Suggestion : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Message { get; set; } = null!;
    public SuggestionStatus Status { get; set; } = SuggestionStatus.New;
}
