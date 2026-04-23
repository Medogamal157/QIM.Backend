namespace QIM.Application.DTOs.Auth;

/// <summary>
/// Combined three-step business (Provider) registration payload.
/// The server always assigns UserType=Provider and role=Provider, and
/// creates the Business in Pending status for admin approval.
/// File uploads (logo/album) are performed via separate file endpoints after registration.
/// </summary>
public class RegisterBusinessRequest
{
    // ── Account / Step 1 ──
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int ActivityId { get; set; }
    public int? SpecialityId { get; set; }
    public string? Website { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? WhatsApp { get; set; }
    public List<string> PhoneNumbers { get; set; } = new();

    // ── Details / Step 2 ──
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public List<string> Keywords { get; set; } = new();
    public List<RegisterBusinessAddressDto> Addresses { get; set; } = new();
    public List<RegisterBusinessWorkHoursDto> WorkHours { get; set; } = new();

    // ── Confirmation / Step 3 ──
    public bool AgreeTerms { get; set; }
}

public class RegisterBusinessAddressDto
{
    public int? CountryId { get; set; }
    public int? CityId { get; set; }
    public int? DistrictId { get; set; }
    public string? StreetName { get; set; }
    public string? BuildingNumber { get; set; }
    public string? MapUrl { get; set; }
}

public class RegisterBusinessWorkHoursDto
{
    public int DayOfWeek { get; set; }
    public string? OpenTime { get; set; }   // "HH:mm" or "HH:mm:ss"
    public string? CloseTime { get; set; }
    public bool IsClosed { get; set; }
}
