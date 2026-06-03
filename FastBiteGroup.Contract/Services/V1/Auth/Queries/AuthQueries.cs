using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;

namespace FastBiteGroup.Contract.Services.V1.Auth.Queries;

public static class AuthQueries
{
    public record Token(string? AccessToken, string? RefreshToken) : IQuery<AuthResponse>;
}
