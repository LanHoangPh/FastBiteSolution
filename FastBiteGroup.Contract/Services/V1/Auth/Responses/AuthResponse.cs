namespace FastBiteGroup.Contract.Services.V1.Auth.Responses;

public sealed record AuthResponse(
string AccessToken,
string RefreshToken,
DateTime AccessTokenExpiresAt,
DateTime RefreshTokenExpiresAt,
UserInfoResponse User);

public sealed record UserInfoResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles);
