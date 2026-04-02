using Microsoft.EntityFrameworkCore;
using QIM.Application.Interfaces.Auth;
using QIM.Domain.Entities.Identity;
using QIM.Persistence.Contexts;

namespace QIM.Persistence.Repositories;

public class RefreshTokenStore : IRefreshTokenStore
{
    private readonly QimDbContext _context;

    public RefreshTokenStore(QimDbContext context) => _context = context;

    public async Task<RefreshToken?> GetByTokenAsync(string token) =>
        await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);

    public async Task SaveAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAsync(string token)
    {
        var existing = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (existing is not null)
        {
            existing.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllForUserAsync(string userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync();

        foreach (var t in tokens)
            t.IsRevoked = true;

        await _context.SaveChangesAsync();
    }
}
