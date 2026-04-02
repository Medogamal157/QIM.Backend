using AutoMapper;
using QIM.Application.DTOs.Business;
using QIM.Application.DTOs.Activity;
using QIM.Application.DTOs.Content;
using QIM.Application.DTOs.Location;
using QIM.Domain.Entities;

namespace QIM.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Location mappings
        CreateMap<Country, CountryDto>();
        CreateMap<CreateCountryRequest, Country>();
        CreateMap<UpdateCountryRequest, Country>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<City, CityDto>();
        CreateMap<CreateCityRequest, City>();
        CreateMap<UpdateCityRequest, City>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<District, DistrictDto>();
        CreateMap<CreateDistrictRequest, District>();
        CreateMap<UpdateDistrictRequest, District>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Activity mappings
        CreateMap<Domain.Entities.Activity, ActivityDto>();
        CreateMap<CreateActivityRequest, Domain.Entities.Activity>();
        CreateMap<UpdateActivityRequest, Domain.Entities.Activity>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Speciality mappings
        CreateMap<Speciality, SpecialityDto>();
        CreateMap<CreateSpecialityRequest, Speciality>();
        CreateMap<UpdateSpecialityRequest, Speciality>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // PlatformSetting mappings
        CreateMap<PlatformSetting, PlatformSettingDto>();
        CreateMap<UpdatePlatformSettingRequest, PlatformSetting>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // BlogPost mappings
        CreateMap<BlogPost, BlogPostDto>()
            .ForMember(d => d.AuthorName, opt => opt.MapFrom(s => s.Author != null ? s.Author.FullName : ""));
        CreateMap<CreateBlogPostRequest, BlogPost>();
        CreateMap<UpdateBlogPostRequest, BlogPost>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // ── Phase 4 ── Business mappings
        CreateMap<Business, BusinessDto>()
            .ForMember(d => d.OwnerName, opt => opt.MapFrom(s => s.Owner != null ? s.Owner.FullName : ""))
            .ForMember(d => d.ActivityNameAr, opt => opt.MapFrom(s => s.Activity != null ? s.Activity.NameAr : ""))
            .ForMember(d => d.ActivityNameEn, opt => opt.MapFrom(s => s.Activity != null ? s.Activity.NameEn : ""))
            .ForMember(d => d.SpecialityName, opt => opt.MapFrom(s => s.Speciality != null ? s.Speciality.NameEn : null))
            .ForMember(d => d.Keywords, opt => opt.MapFrom(s => s.Keywords != null ? s.Keywords.Select(k => k.Keyword).ToList() : new List<string>()));

        CreateMap<Business, BusinessListDto>()
            .ForMember(d => d.ActivityNameAr, opt => opt.MapFrom(s => s.Activity != null ? s.Activity.NameAr : ""))
            .ForMember(d => d.ActivityNameEn, opt => opt.MapFrom(s => s.Activity != null ? s.Activity.NameEn : ""))
            .ForMember(d => d.CityName, opt => opt.MapFrom(s =>
                s.Addresses != null && s.Addresses.Any(a => a.IsPrimary && a.City != null)
                    ? s.Addresses.First(a => a.IsPrimary && a.City != null).City!.NameEn
                    : null));

        CreateMap<CreateBusinessRequest, Business>();
        CreateMap<UpdateBusinessRequest, Business>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // BusinessAddress mappings
        CreateMap<BusinessAddress, BusinessAddressDto>()
            .ForMember(d => d.CountryNameAr, opt => opt.MapFrom(s => s.Country != null ? s.Country.NameAr : null))
            .ForMember(d => d.CountryNameEn, opt => opt.MapFrom(s => s.Country != null ? s.Country.NameEn : null))
            .ForMember(d => d.CityNameAr, opt => opt.MapFrom(s => s.City != null ? s.City.NameAr : null))
            .ForMember(d => d.CityNameEn, opt => opt.MapFrom(s => s.City != null ? s.City.NameEn : null))
            .ForMember(d => d.DistrictNameAr, opt => opt.MapFrom(s => s.District != null ? s.District.NameAr : null))
            .ForMember(d => d.DistrictNameEn, opt => opt.MapFrom(s => s.District != null ? s.District.NameEn : null));
        CreateMap<CreateBusinessAddressRequest, BusinessAddress>();
        CreateMap<UpdateBusinessAddressRequest, BusinessAddress>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // BusinessWorkHours mappings
        CreateMap<BusinessWorkHours, BusinessWorkHoursDto>();
        CreateMap<SetWorkHoursRequest, BusinessWorkHours>();

        // BusinessImage mappings
        CreateMap<BusinessImage, BusinessImageDto>();

        // Review mappings
        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User != null ? s.User.FullName : ""));
        CreateMap<CreateReviewRequest, Review>();

        // BusinessClaim mappings
        CreateMap<BusinessClaim, BusinessClaimDto>()
            .ForMember(d => d.BusinessName, opt => opt.MapFrom(s => s.Business != null ? s.Business.NameEn : ""))
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User != null ? s.User.FullName : ""));
        CreateMap<CreateBusinessClaimRequest, BusinessClaim>();

        // ContactRequest mappings
        CreateMap<ContactRequest, ContactRequestDto>();
        CreateMap<CreateContactRequest, ContactRequest>();

        // Suggestion mappings
        CreateMap<Suggestion, SuggestionDto>();
        CreateMap<CreateSuggestionRequest, Suggestion>();

        // Advertisement mappings
        CreateMap<Advertisement, AdvertisementDto>();
        CreateMap<CreateAdvertisementRequest, Advertisement>();
        CreateMap<UpdateAdvertisementRequest, Advertisement>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
