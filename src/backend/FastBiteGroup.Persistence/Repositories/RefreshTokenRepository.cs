using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FastBiteGroup.Persistence.Repositories;

internal sealed class RefreshTokenRepository(ApplicationDbContext context)
    : RepositoryBase<AppRefreshToken, long>(context), IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    public Task<AppRefreshToken?> FindByTokenAsync(string token, CancellationToken ct = default)
        => _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == token, ct);

    /// <inheritdoc />
    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var revokedAt = DateTime.UtcNow;

        await _context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(r => r.IsRevoked, true)
                    .SetProperty(r => r.RevokedAt, revokedAt),
                ct);
    }
}
