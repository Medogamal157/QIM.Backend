using QIM.Domain.Entities.Identity;

namespace QIM.Application.Interfaces.Auth;

/// <summary>
/// Abstraction for refresh-token persistence. Implemented in QIM.Persistence.
/// </summary>
public interface IRefreshTokenStore
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task SaveAsync(RefreshToken refreshToken);
    Task RevokeAsync(string token);
    Task RevokeAllForUserAsync(string userId);
}
