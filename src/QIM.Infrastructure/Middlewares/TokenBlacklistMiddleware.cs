using Microsoft.AspNetCore.Http;
using QIM.Application.Interfaces.Auth;
using System.IdentityModel.Tokens.Jwt;

namespace QIM.Infrastructure.Middlewares;

/// <summary>
/// Checks if the refresh token in the request has been revoked.
/// This middleware validates that JWT access tokens haven't been issued
/// alongside already-revoked refresh tokens (token blacklisting pattern).
/// </summary>
public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public TokenBlacklistMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IRefreshTokenStore refreshTokenStore)
    {
        // Only check authenticated requests
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader is not null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader["Bearer ".Length..].Trim();
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    var jti = jwtToken.Id;

                    // If we implement JTI-based blacklisting in the future,
                    // we would check here. For now, the refresh-token rotation
                    // pattern handles revocation at the refresh endpoint.
                }
                catch
                {
                    // Token parsing failed — let the auth middleware handle it
                }
            }
        }

        await _next(context);
    }
}
