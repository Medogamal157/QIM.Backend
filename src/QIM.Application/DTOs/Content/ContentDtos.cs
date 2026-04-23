using QIM.Domain.Common.Enums;

namespace QIM.Application.DTOs.Content;

public class PlatformSettingDto
{
    public int Id { get; set; }
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? Group { get; set; }
}

public class UpdatePlatformSettingRequest
{
    public string Value { get; set; } = null!;
}

public class BlogPostDto
{
    public int Id { get; set; }
    public string TitleAr { get; set; } = null!;
    public string TitleEn { get; set; } = null!;
    public string ContentAr { get; set; } = null!;
    public string ContentEn { get; set; } = null!;
    public string? Excerpt { get; set; }
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public string AuthorId { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
    public BlogPostStatus Status { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBlogPostRequest
{
    public string TitleAr { get; set; } = null!;
    public string TitleEn { get; set; } = null!;
    public string ContentAr { get; set; } = null!;
    public string ContentEn { get; set; } = null!;
    public string? Excerpt { get; set; }
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateBlogPostRequest
{
    public string TitleAr { get; set; } = null!;
    public string TitleEn { get; set; } = null!;
    public string ContentAr { get; set; } = null!;
    public string ContentEn { get; set; } = null!;
    public string? Excerpt { get; set; }
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
}
