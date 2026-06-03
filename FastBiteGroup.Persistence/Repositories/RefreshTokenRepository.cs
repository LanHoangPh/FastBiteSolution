using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FastBiteGroup.Persistence.Repositories;

internal sealed class RefreshTokenRepository
    : RepositoryBase<AppRefreshToken, long>, IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        => _context = context;

    /// <inheritdoc />
    public Task<AppRefreshToken?> FindByTokenAsync(string token, CancellationToken ct = default)
        => _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == token, ct);

    /// <inheritdoc />
    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        await _context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ExecuteUpdateAsync(
                s => s.SetProperty(r => r.IsRevoked, true),
                ct);
    }
}
