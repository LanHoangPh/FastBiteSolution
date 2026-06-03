using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Domain.Abstractions.Repositories;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class RevokeAllSessionsCommandHandler
    : ICommandHandler<AuthCommands.RevokeAllSessionsCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RevokeAllSessionsCommandHandler(IRefreshTokenRepository refreshTokenRepository)
        => _refreshTokenRepository = refreshTokenRepository;

    public async Task<Result> Handle(
        AuthCommands.RevokeAllSessionsCommand request,
        CancellationToken cancellationToken)
    {
        // Bulk revoke all refresh tokens — uses ExecuteUpdateAsync (single SQL statement)
        await _refreshTokenRepository.RevokeAllForUserAsync(request.UserId, cancellationToken);

        return Result.Success();
    }
}
