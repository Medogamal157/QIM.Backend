using QIM.Domain.Common;

namespace QIM.Domain.Entities;

public class BusinessAddress : BaseEntity
{
    public int BusinessId { get; set; }
    public int? CountryId { get; set; }
    public int? CityId { get; set; }
    public int? DistrictId { get; set; }
    public string? StreetName { get; set; }
    public string? BuildingNumber { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? MapUrl { get; set; }
    public bool IsPrimary { get; set; }

    // Navigation
    public Business Business { get; set; } = null!;
    public Country? Country { get; set; }
    public City? City { get; set; }
    public District? District { get; set; }
}
