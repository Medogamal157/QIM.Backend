using QIM.Domain.Common.Enums;

namespace QIM.Application.DTOs.Business;

// ── Business DTOs ──

public class BusinessDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string OwnerId { get; set; } = null!;
    public string OwnerName { get; set; } = null!;
    public int ActivityId { get; set; }
    public string ActivityNameAr { get; set; } = null!;
    public string ActivityNameEn { get; set; } = null!;
    public int? SpecialityId { get; set; }
    public string? SpecialityName { get; set; }
    public BusinessStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public string? LogoUrl { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? WhatsApp { get; set; }
    public string? Phones { get; set; }
    public string? AccountCode { get; set; }
    public bool IsVerified { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<BusinessAddressDto> Addresses { get; set; } = [];
    public List<BusinessWorkHoursDto> WorkHours { get; set; } = [];
    public List<BusinessImageDto> Images { get; set; } = [];
    public List<string> Keywords { get; set; } = [];
}

public class BusinessListDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? LogoUrl { get; set; }
    public string? AccountCode { get; set; }
    public BusinessStatus Status { get; set; }
    public bool IsVerified { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public string ActivityNameAr { get; set; } = null!;
    public string ActivityNameEn { get; set; } = null!;
    public string? CityName { get; set; }
}

public class BusinessAutoCompleteDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
}

public class CreateBusinessRequest
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public int ActivityId { get; set; }
    public int? SpecialityId { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? WhatsApp { get; set; }
    public string? Phones { get; set; }
    public List<string>? Keywords { get; set; }
}

public class UpdateBusinessRequest
{
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public int? ActivityId { get; set; }
    public int? SpecialityId { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? WhatsApp { get; set; }
    public string? Phones { get; set; }
    public List<string>? Keywords { get; set; }
}

// ── BusinessAddress DTOs ──

public class BusinessAddressDto
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public int? CountryId { get; set; }
    public string? CountryNameAr { get; set; }
    public string? CountryNameEn { get; set; }
    public int? CityId { get; set; }
    public string? CityNameAr { get; set; }
    public string? CityNameEn { get; set; }
    public int? DistrictId { get; set; }
    public string? DistrictNameAr { get; set; }
    public string? DistrictNameEn { get; set; }
    public string? StreetName { get; set; }
    public string? BuildingNumber { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? MapUrl { get; set; }
    public bool IsPrimary { get; set; }
}

public class CreateBusinessAddressRequest
{
    public int? CountryId { get; set; }
    public int? CityId { get; set; }
    public int? DistrictId { get; set; }
    public string? StreetName { get; set; }
    public string? BuildingNumber { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? MapUrl { get; set; }
    public bool IsPrimary { get; set; }
}

public class UpdateBusinessAddressRequest
{
    public int? CountryId { get; set; }
    public int? CityId { get; set; }
    public int? DistrictId { get; set; }
    public string? StreetName { get; set; }
    public string? BuildingNumber { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? MapUrl { get; set; }
    public bool? IsPrimary { get; set; }
}

// ── BusinessWorkHours DTOs ──

public class BusinessWorkHoursDto
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public bool IsClosed { get; set; }
}

public class SetWorkHoursRequest
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public bool IsClosed { get; set; }
}

// ── BusinessImage DTOs ──

public class BusinessImageDto
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsCover { get; set; }
    public int SortOrder { get; set; }
}

// ── Review DTOs ──

public class ReviewDto
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public ReviewStatus Status { get; set; }
    public string? FlagReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewRequest
{
    public int BusinessId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class FlagReviewRequest
{
    public string Reason { get; set; } = null!;
}

// ── BusinessClaim DTOs ──

public class BusinessClaimDto
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Message { get; set; }
    public string? DocumentUrls { get; set; }
    public ClaimStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBusinessClaimRequest
{
    public int BusinessId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Message { get; set; }
    public string? DocumentUrls { get; set; }
}

// ── Contact & Suggestion DTOs ──

public class ContactRequestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Message { get; set; } = null!;
    public ContactStatus Status { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateContactRequest
{
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Message { get; set; } = null!;
}

public class SuggestionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Message { get; set; } = null!;
    public SuggestionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSuggestionRequest
{
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Message { get; set; } = null!;
}

// ── User Profile DTOs ──

public class UserProfileDto
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public UserType UserType { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
}

// ── Analytics DTOs ──

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalBusinesses { get; set; }
    public int TotalReviews { get; set; }
    public int PendingBusinesses { get; set; }
    public int PendingClaims { get; set; }
    public int NewContacts { get; set; }
}

// ── Provider Account DTO ──

public class ProviderAccountDto
{
    public string UserId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public bool IsVerified { get; set; }
    public int TotalBusinesses { get; set; }
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public List<BusinessListDto> Businesses { get; set; } = [];
}

public class AnalyticsDto
{
    public List<GrowthDataPoint> UserGrowth { get; set; } = [];
    public List<GrowthDataPoint> BusinessTrend { get; set; } = [];
    public List<ActivityDistribution> TopActivities { get; set; } = [];
    public Dictionary<int, int> ReviewDistribution { get; set; } = [];
}

public class GrowthDataPoint
{
    public string Period { get; set; } = null!;
    public int Count { get; set; }
}

public class ActivityDistribution
{
    public string ActivityName { get; set; } = null!;
    public int BusinessCount { get; set; }
}
