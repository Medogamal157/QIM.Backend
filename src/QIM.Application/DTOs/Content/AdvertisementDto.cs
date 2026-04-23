namespace QIM.Application.DTOs.Content;

public class AdvertisementDto
{
    public int Id { get; set; }
    public string TitleAr { get; set; } = null!;
    public string TitleEn { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string? TargetUrl { get; set; }
    public string? Position { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CreateAdvertisementRequest
{
    public string TitleAr { get; set; } = null!;
    public string TitleEn { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string? TargetUrl { get; set; }
    public string? Position { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateAdvertisementRequest
{
    public string TitleAr { get; set; } = null!;
    public string TitleEn { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string? TargetUrl { get; set; }
    public string? Position { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
