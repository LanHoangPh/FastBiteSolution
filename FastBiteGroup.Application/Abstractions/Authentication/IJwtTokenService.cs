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
    string? GetJtiFromExpiredToken(string accessToken);
}
