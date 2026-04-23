using QIM.Domain.Common;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities.Identity;

namespace QIM.Domain.Entities;

public class BlogPost : BaseAuditableEntity
{
    public string TitleAr { get; set; } = null!;
    public string TitleEn { get; set; } = null!;
    public string ContentAr { get; set; } = null!;
    public string ContentEn { get; set; } = null!;
    public string? Excerpt { get; set; }
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public string AuthorId { get; set; } = null!;
    public BlogPostStatus Status { get; set; } = BlogPostStatus.Draft;
    public DateTime? PublishedAt { get; set; }

    // Navigation
    public ApplicationUser Author { get; set; } = null!;
}
