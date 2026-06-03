using FastBiteGroup.Domain.Entities;

namespace FastBiteGroup.Domain.Abstractions.Repositories;

public interface IRefreshTokenRepository : IRepositoryBase<RefreshToken, long>
{
    /// <summary>Finds a token by its opaque string value. Returns null if not found.</summary>
    Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>Revokes all non-revoked refresh tokens for a given user (bulk update).</summary>
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
}
