using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Domain.Abstractions.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class LogoutCommandHandler : ICommandHandler<AuthCommands.LogoutCommand>
{
    private readonly ICacheService _cacheService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        ICacheService cacheService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<LogoutCommandHandler>? logger = null)
    {
        _cacheService = cacheService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger ?? NullLogger<LogoutCommandHandler>.Instance;
    }

    public async Task<Result> Handle(
        AuthCommands.LogoutCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Compute accurate TTL from the token's actual exp claim.
        //    Falls back to a safe minimum (5 min) if the token is already expired — ensures
        //    the key is written to Redis (with a short TTL) so race conditions don't leave
        //    a revoked token appearing valid momentarily.
        var ttl = _jwtTokenService.GetAccessTokenRemainingLifetime(request.AccessToken);
        if (ttl == TimeSpan.Zero)
            ttl = TimeSpan.FromMinutes(5);

        await _cacheService.BlacklistTokenAsync(request.Jti, ttl, cancellationToken);

        // 2. Revoke the refresh token — load as tracked entity (single query)
        var refreshToken = await _refreshTokenRepository
            .FindSingleAsync(r => r.Token == request.RefreshToken, cancellationToken);

        if (refreshToken is not null && refreshToken.IsActive)
        {
            refreshToken.Revoke();
            _refreshTokenRepository.Update(refreshToken);
        }

        _logger.LogInformation("User logged out successfully. Jti: {Jti}", request.Jti);

        return Result.Success();
    }
}
