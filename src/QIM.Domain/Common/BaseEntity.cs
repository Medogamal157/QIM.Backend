namespace QIM.Domain.Common;

/// <summary>
/// Base entity with Id and soft delete support.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
}
