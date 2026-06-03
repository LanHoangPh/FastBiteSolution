using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Domain.Abstractions.Repositories;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class LogoutCommandHandler : ICommandHandler<AuthCommands.LogoutCommand>
{
    private readonly ICacheService _cacheService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LogoutCommandHandler(
        ICacheService cacheService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _cacheService = cacheService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result> Handle(
        AuthCommands.LogoutCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Blacklist the access token JTI in Redis
        // TTL: 2 hours is a safe upper bound for typical access token lifetimes.
        // Ideally compute from JWT exp claim, but this is safe and simple.
        await _cacheService.BlacklistTokenAsync(
            request.Jti,
            remainingLifetime: TimeSpan.FromHours(2),
            ct: cancellationToken);

        // 2. Revoke the refresh token in DB
        var refreshToken = await _refreshTokenRepository
            .FindByTokenAsync(request.RefreshToken, cancellationToken);

        if (refreshToken is not null && refreshToken.IsActive)
        {
            // Need tracked entity for update
            var tracked = await _refreshTokenRepository
                .FindSingleAsync(r => r.Token == request.RefreshToken, cancellationToken);
            tracked.Revoke();
            _refreshTokenRepository.Update(tracked);
        }

        return Result.Success();
    }
}
