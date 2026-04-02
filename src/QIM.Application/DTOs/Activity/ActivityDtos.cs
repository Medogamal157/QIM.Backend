namespace QIM.Application.DTOs.Activity;

public class ActivityDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? IconUrl { get; set; }
    public string? Color { get; set; }
    public int? ParentActivityId { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; }
    public List<ActivityDto> SubActivities { get; set; } = [];
}

public class CreateActivityRequest
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? IconUrl { get; set; }
    public string? Color { get; set; }
    public int? ParentActivityId { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateActivityRequest
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? IconUrl { get; set; }
    public string? Color { get; set; }
    public int? ParentActivityId { get; set; }
    public int SortOrder { get; set; }
}

public class SpecialityDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int ActivityId { get; set; }
    public bool IsEnabled { get; set; }
}

public class CreateSpecialityRequest
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int ActivityId { get; set; }
}

public class UpdateSpecialityRequest
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int ActivityId { get; set; }
}
