using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Domain.Abstractions.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class RevokeAllSessionsCommandHandler
    : ICommandHandler<AuthCommands.RevokeAllSessionsCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<RevokeAllSessionsCommandHandler> _logger;

    public RevokeAllSessionsCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<RevokeAllSessionsCommandHandler>? logger = null)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger ?? NullLogger<RevokeAllSessionsCommandHandler>.Instance;
    }

    public async Task<Result> Handle(
        AuthCommands.RevokeAllSessionsCommand request,
        CancellationToken cancellationToken)
    {
        await _refreshTokenRepository.RevokeAllForUserAsync(request.UserId, cancellationToken);

        _logger.LogInformation("All sessions revoked for user. UserId: {UserId}", request.UserId);

        return Result.Success();
    }
}
