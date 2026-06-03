using FastBiteGroup.Domain.Entities;

namespace FastBiteGroup.Domain.Abstractions.Repositories;

public interface IRefreshTokenRepository : IRepositoryBase<AppRefreshToken, long>
{
    Task<AppRefreshToken?> FindByTokenAsync(string token, CancellationToken ct = default);

    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
}
