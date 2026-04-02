using QIM.Application.Interfaces.Repositories;
using QIM.Domain.Entities;

namespace QIM.Application.Interfaces;

/// <summary>
/// Unit of Work — exposes repositories and commits changes atomically.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Country> Countries { get; }
    IGenericRepository<City> Cities { get; }
    IGenericRepository<District> Districts { get; }
    IGenericRepository<Activity> Activities { get; }
    IGenericRepository<Speciality> Specialities { get; }
    IGenericRepository<PlatformSetting> PlatformSettings { get; }
    IGenericRepository<BlogPost> BlogPosts { get; }
    IGenericRepository<Business> Businesses { get; }
    IGenericRepository<BusinessAddress> BusinessAddresses { get; }
    IGenericRepository<BusinessWorkHours> BusinessWorkHoursRepo { get; }
    IGenericRepository<BusinessImage> BusinessImages { get; }
    IGenericRepository<Review> Reviews { get; }
    IGenericRepository<ContactRequest> ContactRequests { get; }
    IGenericRepository<Suggestion> Suggestions { get; }
    IGenericRepository<BusinessClaim> BusinessClaims { get; }
    IGenericRepository<BusinessKeyword> BusinessKeywords { get; }
    IGenericRepository<Advertisement> Advertisements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
