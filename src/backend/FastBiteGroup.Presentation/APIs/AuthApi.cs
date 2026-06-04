using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;
using FastBiteGroup.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FastBiteGroup.Presentation.APIs;

public class AuthApi : ApiEndpoint, IEndpoint
{
    private const string BaseUrl = "/api/v{version:apiVersion}/auth";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.NewVersionedApi("Authentication")
                       .MapGroup(BaseUrl)
                       .HasApiVersion(1)
                       .WithTags("Authentication");

        // POST /api/v1/auth/register
        group.MapPost("/register", Register)
            .AllowAnonymous()
            .WithSummary("Register a new user account")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        // POST /api/v1/auth/login
        group.MapPost("/login", Login)
            .AllowAnonymous()
            .WithSummary("Authenticate and receive JWT token pair")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        // POST /api/v1/auth/refresh
        group.MapPost("/refresh", RefreshToken)
            .AllowAnonymous()
            .WithSummary("Rotate refresh token and receive new JWT pair")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        // POST /api/v1/auth/logout
        group.MapPost("/logout", Logout)
            .RequireAuthorization()
            .WithSummary("Logout — blacklists the access token and revokes the refresh token")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        // POST /api/v1/auth/revoke-all
        group.MapPost("/revoke-all", RevokeAll)
            .RequireAuthorization()
            .WithSummary("Revoke all active sessions for the current user")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    // ─── Handlers ─────────────────────────────────────────────────────────

    private static async Task<IResult> Register(
        [FromBody] AuthCommands.RegisterCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsFailure ? HandleFailure(result) : Results.Ok(result.Value);
    }

    private static async Task<IResult> Login(
        [FromBody] AuthCommands.LoginCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsFailure ? HandleFailure(result) : Results.Ok(result.Value);
    }

    private static async Task<IResult> RefreshToken(
        [FromBody] AuthCommands.RefreshTokenCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsFailure ? HandleFailure(result) : Results.Ok(result.Value);
    }

    private static async Task<IResult> Logout(
        ClaimsPrincipal user,
        [FromBody] LogoutRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var jti = user.FindFirstValue(JwtRegisteredClaimNames.Jti) ?? string.Empty;
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

        var command = new AuthCommands.LogoutCommand(jti, request.RefreshToken, userId);
        var result = await sender.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : Results.NoContent();
    }

    private static async Task<IResult> RevokeAll(
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

        var command = new AuthCommands.RevokeAllSessionsCommand(userId);
        var result = await sender.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : Results.NoContent();
    }
}

public sealed record LogoutRequest(string RefreshToken);
