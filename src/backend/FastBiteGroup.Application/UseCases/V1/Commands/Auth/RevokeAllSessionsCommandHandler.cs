using FastBiteGroup.Contract.Services.V1.Auth.Commands;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class RevokeAllSessionsCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<RevokeAllSessionsCommandHandler>? logger = null)
    : ICommandHandler<AuthCommands.RevokeAllSessionsCommand>
{
    private readonly ILogger<RevokeAllSessionsCommandHandler> _logger = logger ?? NullLogger<RevokeAllSessionsCommandHandler>.Instance;

    public async Task<Result> Handle(
        AuthCommands.RevokeAllSessionsCommand request,
        CancellationToken cancellationToken)
    {
        await refreshTokenRepository.RevokeAllForUserAsync(request.UserId, cancellationToken);

        _logger.LogInformation("All sessions revoked for user. UserId: {UserId}", request.UserId);

        return Result.Success();
    }
}
