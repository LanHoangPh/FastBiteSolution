namespace FastBiteGroup.Application.Abstractions.Authentication;

public interface IJwtTokenService
{
    (string Token, string Jti, DateTime ExpiresAt) GenerateAccessToken(
        Guid userId,
        string email,
        string userName,
        string firstName,
        string lastName,
        IEnumerable<string> roles);

    string GenerateRefreshToken();

    /// <summary>
    /// Extracts the JTI from an access token without validating lifetime.
    /// Returns null if the token is malformed or signature is invalid.
    /// </summary>
    string? GetJtiFromExpiredToken(string accessToken);

    /// <summary>
    /// Returns the remaining lifetime of an access token based on its 'exp' claim.
    /// Returns TimeSpan.Zero if the token is expired or cannot be parsed.
    /// Used to set an accurate TTL when blacklisting a token in Redis.
    /// </summary>
    TimeSpan GetAccessTokenRemainingLifetime(string accessToken);
}
