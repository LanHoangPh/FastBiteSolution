using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;

namespace FastBiteGroup.Contract.Services.V1.Auth.Commands;

public static class AuthCommands
{
    public sealed record LoginCommand(
        string Email,
        string Password) : ICommand<AuthResponse>;

    public sealed record RefreshTokenCommand(
        string AccessToken,
        string RefreshToken) : ICommand<AuthResponse>;

    public sealed record LogoutCommand(
        string Jti,                    // Access Token jti để blacklist
        string RefreshToken,           // Thu hồi Refresh Token
        Guid UserId) : ICommand;

    public sealed record RegisterCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        DateTime DayOfBirth) : ICommand<AuthResponse>;

    public sealed record RevokeAllSessionsCommand(
        Guid UserId) : ICommand;
}
