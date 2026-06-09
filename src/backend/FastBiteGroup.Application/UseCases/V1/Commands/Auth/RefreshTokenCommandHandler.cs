using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class RefreshTokenCommandHandler
    : ICommandHandler<AuthCommands.RefreshTokenCommand, AuthResponse>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserAuthService _userAuthService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUserAuthService userAuthService,
        ILogger<RefreshTokenCommandHandler>? logger = null)
    {
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _userAuthService = userAuthService;
        _logger = logger ?? NullLogger<RefreshTokenCommandHandler>.Instance;
    }

    public async Task<Result<AuthResponse>> Handle(
        AuthCommands.RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Extract JTI from the (possibly expired) access token
        var jti = _jwtTokenService.GetJtiFromExpiredToken(request.AccessToken);
        if (jti is null)
        {
            _logger.LogWarning("Refresh token failed: access token could not be parsed.");
            return Result.Failure<AuthResponse>(AuthErrors.InvalidToken);
        }

        // 2. Load the refresh token as a tracked entity (single query — avoids double round-trip)
        var refreshToken = await _refreshTokenRepository
            .FindSingleAsync(r => r.Token == request.RefreshToken, cancellationToken);

        if (refreshToken is null)
        {
            _logger.LogWarning("Refresh token failed: refresh token was not found.");
            return Result.Failure<AuthResponse>(AuthErrors.InvalidRefreshToken);
        }

        // 3. Validate state
        if (!refreshToken.IsActive)
        {
            _logger.LogWarning("Refresh token failed: refresh token is expired, used, or revoked. UserId: {UserId}", refreshToken.UserId);

            // SECURITY CHECK: Refresh Token Reuse Detection (RFC 6819)
            if (refreshToken.IsUsed)
            {
                _logger.LogWarning("SECURITY ALERT: Suspected Refresh Token Hijacking. Revoking all sessions for UserId: {UserId}", refreshToken.UserId);
                await _refreshTokenRepository.RevokeAllForUserAsync(refreshToken.UserId, cancellationToken);
            }

            return Result.Failure<AuthResponse>(AuthErrors.RefreshTokenExpiredOrRevoked);
        }

        // 4. Validate JTI linkage
        if (refreshToken.Jti != jti)
        {
            _logger.LogWarning("Refresh token failed: access token and refresh token mismatch. UserId: {UserId}", refreshToken.UserId);
            return Result.Failure<AuthResponse>(AuthErrors.TokenMismatch);
        }

        // 5. Get user
        var user = await _userAuthService.FindByIdAsync(refreshToken.UserId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Refresh token failed: associated user was not found. UserId: {UserId}", refreshToken.UserId);
            return Result.Failure<AuthResponse>(AuthErrors.AssociatedUserNotFound);
        }

        // 6. Generate new tokens
        var (newAccessToken, newJti, newAccessExpiresAt) = _jwtTokenService.GenerateAccessToken(
            user.Id, user.Email, user.UserName, user.FirstName, user.LastName, user.Roles);
        var newRefreshTokenString = _jwtTokenService.GenerateRefreshToken();
        var newRefreshExpiresAt = DateTime.UtcNow.AddDays(30);

        // 7. Rotate — mark old token as used (already tracked, no extra query needed)
        refreshToken.MarkUsed(newRefreshTokenString);
        _refreshTokenRepository.Update(refreshToken);

        var newRefreshToken = AppRefreshToken.Create(newRefreshTokenString, newJti, user.Id, newRefreshExpiresAt);
        _refreshTokenRepository.Add(newRefreshToken);

        var response = new AuthResponse(
            newAccessToken,
            newRefreshTokenString,
            newAccessExpiresAt,
            newRefreshExpiresAt,
            new UserInfoResponse(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.FullName,
                user.AvatarUrl,
                user.Bio,
                user.IsActive,
                user.Roles));

        _logger.LogInformation("Refresh token rotated successfully. UserId: {UserId}", user.Id);

        return Result.Success(response);
    }
}
