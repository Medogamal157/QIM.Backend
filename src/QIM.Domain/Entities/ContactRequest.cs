using QIM.Domain.Common;
using QIM.Domain.Common.Enums;

namespace QIM.Domain.Entities;

public class ContactRequest : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Message { get; set; } = null!;
    public ContactStatus Status { get; set; } = ContactStatus.New;
    public string? AdminNotes { get; set; }
}
