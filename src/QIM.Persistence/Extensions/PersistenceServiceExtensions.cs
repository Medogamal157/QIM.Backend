using Microsoft.Extensions.DependencyInjection;
using QIM.Application.Interfaces;
using QIM.Application.Interfaces.Auth;
using QIM.Application.Interfaces.Repositories;
using QIM.Persistence.Repositories;

namespace QIM.Persistence.Extensions;

public static class PersistenceServiceExtensions
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();

        return services;
    }
}
