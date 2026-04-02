using QIM.Domain.Entities.Identity;

namespace QIM.Application.Interfaces.Auth;

/// <summary>
/// JWT token generation contract. Implemented in QIM.Infrastructure.
/// </summary>
public interface IJwtTokenGenerator
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
}
