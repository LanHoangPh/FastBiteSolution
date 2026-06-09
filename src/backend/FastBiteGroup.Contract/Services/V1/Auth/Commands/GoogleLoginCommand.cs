using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;
using System.Text.Json.Serialization;

namespace FastBiteGroup.Contract.Services.V1.Auth.Commands;

public record GoogleLoginCommand(
    [property: JsonPropertyName("idToken")] string IdToken
) : IRequest<Result<AuthResponse>>;
