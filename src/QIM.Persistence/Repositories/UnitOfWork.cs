using QIM.Application.Interfaces;
using QIM.Application.Interfaces.Repositories;
using QIM.Domain.Entities;
using QIM.Persistence.Contexts;

namespace QIM.Persistence.Repositories;

/// <summary>
/// Unit of Work implementation — manages repositories and atomic saves.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly QimDbContext _context;

    private IGenericRepository<Country>? _countries;
    private IGenericRepository<City>? _cities;
    private IGenericRepository<District>? _districts;
    private IGenericRepository<Activity>? _activities;
    private IGenericRepository<Speciality>? _specialities;
    private IGenericRepository<PlatformSetting>? _platformSettings;
    private IGenericRepository<BlogPost>? _blogPosts;
    private IGenericRepository<Business>? _businesses;
    private IGenericRepository<BusinessAddress>? _businessAddresses;
    private IGenericRepository<BusinessWorkHours>? _businessWorkHours;
    private IGenericRepository<BusinessImage>? _businessImages;
    private IGenericRepository<Review>? _reviews;
    private IGenericRepository<ContactRequest>? _contactRequests;
    private IGenericRepository<Suggestion>? _suggestions;
    private IGenericRepository<BusinessClaim>? _businessClaims;
    private IGenericRepository<BusinessKeyword>? _businessKeywords;
    private IGenericRepository<Advertisement>? _advertisements;

    public UnitOfWork(QimDbContext context) => _context = context;

    public IGenericRepository<Country> Countries =>
        _countries ??= new GenericRepository<Country>(_context);
    public IGenericRepository<City> Cities =>
        _cities ??= new GenericRepository<City>(_context);
    public IGenericRepository<District> Districts =>
        _districts ??= new GenericRepository<District>(_context);
    public IGenericRepository<Activity> Activities =>
        _activities ??= new GenericRepository<Activity>(_context);
    public IGenericRepository<Speciality> Specialities =>
        _specialities ??= new GenericRepository<Speciality>(_context);
    public IGenericRepository<PlatformSetting> PlatformSettings =>
        _platformSettings ??= new GenericRepository<PlatformSetting>(_context);
    public IGenericRepository<BlogPost> BlogPosts =>
        _blogPosts ??= new GenericRepository<BlogPost>(_context);
    public IGenericRepository<Business> Businesses =>
        _businesses ??= new GenericRepository<Business>(_context);
    public IGenericRepository<BusinessAddress> BusinessAddresses =>
        _businessAddresses ??= new GenericRepository<BusinessAddress>(_context);
    public IGenericRepository<BusinessWorkHours> BusinessWorkHoursRepo =>
        _businessWorkHours ??= new GenericRepository<BusinessWorkHours>(_context);
    public IGenericRepository<BusinessImage> BusinessImages =>
        _businessImages ??= new GenericRepository<BusinessImage>(_context);
    public IGenericRepository<Review> Reviews =>
        _reviews ??= new GenericRepository<Review>(_context);
    public IGenericRepository<ContactRequest> ContactRequests =>
        _contactRequests ??= new GenericRepository<ContactRequest>(_context);
    public IGenericRepository<Suggestion> Suggestions =>
        _suggestions ??= new GenericRepository<Suggestion>(_context);
    public IGenericRepository<BusinessClaim> BusinessClaims =>
        _businessClaims ??= new GenericRepository<BusinessClaim>(_context);
    public IGenericRepository<BusinessKeyword> BusinessKeywords =>
        _businessKeywords ??= new GenericRepository<BusinessKeyword>(_context);
    public IGenericRepository<Advertisement> Advertisements =>
        _advertisements ??= new GenericRepository<Advertisement>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
