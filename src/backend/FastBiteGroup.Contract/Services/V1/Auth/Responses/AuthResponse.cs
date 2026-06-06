namespace FastBiteGroup.Contract.Services.V1.Auth.Responses;

public sealed record AuthResponse(
    string TokenType,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    UserInfoResponse User)
{
    /// <summary>OAuth2-compatible constructor. TokenType is always "Bearer".</summary>
    public AuthResponse(
        string accessToken,
        string refreshToken,
        DateTime accessTokenExpiresAt,
        DateTime refreshTokenExpiresAt,
        UserInfoResponse user)
        : this("Bearer", accessToken, refreshToken, accessTokenExpiresAt, refreshTokenExpiresAt, user) { }
}

public sealed record UserInfoResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? FullName,
    string? AvatarUrl,
    string? Bio,
    bool IsActive,
    IEnumerable<string> Roles);

/// <summary>
/// Request body for the logout endpoint. Placed in Contract so it can be referenced
/// across layers without coupling to Presentation.
/// </summary>
public sealed record LogoutRequest(string RefreshToken);
