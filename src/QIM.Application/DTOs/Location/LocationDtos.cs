namespace QIM.Application.DTOs.Location;

public class CountryDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}

public class CreateCountryRequest
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateCountryRequest
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}

public class CityDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int CountryId { get; set; }
    public bool IsEnabled { get; set; }
}

public class CreateCityRequest
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int CountryId { get; set; }
}

public class UpdateCityRequest
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int CountryId { get; set; }
}

public class DistrictDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int CityId { get; set; }
    public bool IsEnabled { get; set; }
}

public class CreateDistrictRequest
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int CityId { get; set; }
}

public class UpdateDistrictRequest
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int CityId { get; set; }
}
