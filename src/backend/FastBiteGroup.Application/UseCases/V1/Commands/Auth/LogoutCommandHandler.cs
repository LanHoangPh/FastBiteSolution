using FastBiteGroup.Contract.Services.V1.Auth.Commands;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class LogoutCommandHandler(
    ICacheService cacheService,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<LogoutCommandHandler>? logger = null)
    : ICommandHandler<AuthCommands.LogoutCommand>
{
    private readonly ILogger<LogoutCommandHandler> _logger = logger ?? NullLogger<LogoutCommandHandler>.Instance;

    public async Task<Result> Handle(
        AuthCommands.LogoutCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Compute accurate TTL from the token's actual exp claim.
        //    Falls back to a safe minimum (5 min) if the token is already expired — ensures
        //    the key is written to Redis (with a short TTL) so race conditions don't leave
        //    a revoked token appearing valid momentarily.
        var ttl = jwtTokenService.GetAccessTokenRemainingLifetime(request.AccessToken);
        if (ttl == TimeSpan.Zero)
            ttl = TimeSpan.FromMinutes(5);

        await cacheService.BlacklistTokenAsync(request.Jti, ttl, cancellationToken);
        
        var refreshToken = await refreshTokenRepository
            .FindSingleAsync(r => r.Token == request.RefreshToken, cancellationToken);

        if (refreshToken is not null && refreshToken.IsActive)
        {
            refreshToken.Revoke();
            refreshTokenRepository.Update(refreshToken);
        }

        _logger.LogInformation("User logged out successfully. Jti: {Jti}", request.Jti);

        return Result.Success();
    }
}
