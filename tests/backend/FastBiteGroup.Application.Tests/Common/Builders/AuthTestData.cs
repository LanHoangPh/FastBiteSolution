using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Domain.Entities;

namespace FastBiteGroup.Application.Tests.Common.Builders;

internal static class AuthTestData
{
    public const string ValidEmail = "test@test.com";
    public const string ValidPassword = "Password123!";
    public const string AccessToken = "access-token";
    public const string RefreshToken = "refresh-token";
    public const string Jti = "jti-123";

    public static UserDto User(
        Guid? id = null,
        string email = ValidEmail,
        string firstName = "First",
        string lastName = "Last",
        IReadOnlyList<string>? roles = null)
        => new(
            Id: id ?? Guid.NewGuid(),
            Email: email,
            UserName: email,
            FirstName: firstName,
            LastName: lastName,
            FullName: $"{firstName} {lastName}",
            AvatarUrl: null,
            Bio: null,
            IsActive: true,
            LastSeenAt: null,
            Roles: roles?.ToList() ?? new List<string> { "Customer" });

    public static AuthCommands.LoginCommand LoginCommand(
        string email = ValidEmail,
        string password = ValidPassword)
        => new(email, password);

    public static AuthCommands.RegisterCommand RegisterCommand(
        string email = ValidEmail,
        string password = ValidPassword,
        string firstName = "First",
        string lastName = "Last",
        DateTime? dayOfBirth = null)
        => new(email, password, firstName, lastName, dayOfBirth ?? new DateTime(1990, 1, 1));

    public static AuthCommands.RefreshTokenCommand RefreshTokenCommand(
        string accessToken = AccessToken,
        string refreshToken = RefreshToken)
        => new(accessToken, refreshToken);

    public static AuthCommands.LogoutCommand LogoutCommand(
        Guid? userId = null,
        string accessToken = AccessToken,
        string jti = Jti,
        string refreshToken = RefreshToken)
        => new(accessToken, jti, refreshToken, userId ?? Guid.NewGuid());

    public static AppRefreshToken ActiveRefreshToken(
        Guid userId,
        string token = RefreshToken,
        string jti = Jti,
        DateTime? expiresAt = null)
        => AppRefreshToken.Create(token, jti, userId, expiresAt ?? DateTime.UtcNow.AddDays(1));
}
