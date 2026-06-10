namespace FastBiteGroup.Desktop.Application.Abstractions;

public interface ISecureTokenStore
{
    Task SaveRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
