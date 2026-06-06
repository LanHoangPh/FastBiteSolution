using System.Text.Json.Serialization;
using MediatR;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;
using FastBiteGroup.Contract.Abstractions.Shared;

namespace FastBiteGroup.Contract.Services.V1.Auth.Commands;

public record GoogleLoginCommand(
    [property: JsonPropertyName("idToken")] string IdToken
) : IRequest<Result<AuthResponse>>;
