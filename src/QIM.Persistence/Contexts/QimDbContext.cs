using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QIM.Domain.Common;
using QIM.Domain.Entities;
using QIM.Domain.Entities.Identity;
using System.Reflection;

namespace QIM.Persistence.Contexts;

public class QimDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly string? _currentUserId;

    public QimDbContext(DbContextOptions<QimDbContext> options) : base(options) { }

    public QimDbContext(DbContextOptions<QimDbContext> options, string? currentUserId)
        : base(options)
    {
        _currentUserId = currentUserId;
    }

    // Domain entities
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Speciality> Specialities => Set<Speciality>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<BusinessAddress> BusinessAddresses => Set<BusinessAddress>();
    public DbSet<BusinessWorkHours> BusinessWorkHours => Set<BusinessWorkHours>();
    public DbSet<BusinessImage> BusinessImages => Set<BusinessImage>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
    public DbSet<Suggestion> Suggestions => Set<Suggestion>();
    public DbSet<BusinessClaim> BusinessClaims => Set<BusinessClaim>();
    public DbSet<BusinessKeyword> BusinessKeywords => Set<BusinessKeyword>();
    public DbSet<PlatformSetting> PlatformSettings => Set<PlatformSetting>();
    public DbSet<Advertisement> Advertisements => Set<Advertisement>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all IEntityTypeConfiguration from this assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global query filter for soft delete
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(QimDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { builder });
            }
        }
    }

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder)
        where TEntity : BaseEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = _currentUserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = _currentUserId;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
