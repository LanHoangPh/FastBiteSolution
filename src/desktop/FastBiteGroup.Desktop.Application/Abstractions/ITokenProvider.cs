namespace FastBiteGroup.Desktop.Application.Abstractions;

public interface ITokenProvider
{
    string? AccessToken { get; }
    DateTimeOffset? AccessTokenExpiresAt { get; }
    bool HasAccessToken { get; }
    bool IsAccessTokenExpired { get; }

    void SetAccessToken(string accessToken, DateTimeOffset expiresAt);
    void Clear();
}
