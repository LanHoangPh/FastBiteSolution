namespace FastBiteGroup.Desktop.Application.Models.Auth;

public record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string? AvatarUrl,
    string? Bio,
    bool IsActive,
    string[] Roles);

public record AuthResponse(
    string TokenType,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    UserDto User);
