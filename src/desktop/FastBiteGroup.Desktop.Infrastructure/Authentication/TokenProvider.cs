using FastBiteGroup.Desktop.Application.Abstractions;

namespace FastBiteGroup.Desktop.Infrastructure.Authentication;

public sealed class TokenProvider : ITokenProvider
{
    private string? _accessToken;
    private DateTimeOffset? _accessTokenExpiresAt;

    public string? AccessToken => _accessToken;
    public DateTimeOffset? AccessTokenExpiresAt => _accessTokenExpiresAt;

    public bool HasAccessToken => !string.IsNullOrWhiteSpace(_accessToken);

    public bool IsAccessTokenExpired =>
        _accessTokenExpiresAt is null ||
        DateTimeOffset.UtcNow >= _accessTokenExpiresAt.Value;

    public void SetAccessToken(string accessToken, DateTimeOffset expiresAt)
    {
        _accessToken = accessToken;
        _accessTokenExpiresAt = expiresAt;
    }

    public void Clear()
    {
        _accessToken = null;
        _accessTokenExpiresAt = null;
    }
}
