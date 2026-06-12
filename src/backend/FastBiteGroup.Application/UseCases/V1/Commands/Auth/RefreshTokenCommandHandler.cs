using FastBiteGroup.Contract.Services.V1.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class RefreshTokenCommandHandler(
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUserAuthService userAuthService,
    ILogger<RefreshTokenCommandHandler>? logger = null)
    : ICommandHandler<AuthCommands.RefreshTokenCommand, AuthResponse>
{
    private readonly ILogger<RefreshTokenCommandHandler> _logger = logger ?? NullLogger<RefreshTokenCommandHandler>.Instance;

    public async Task<Result<AuthResponse>> Handle(
        AuthCommands.RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        //Extract JTI from the (possibly expired) access token
        var jti = jwtTokenService.GetJtiFromExpiredToken(request.AccessToken);
        if (jti is null)
        {
            _logger.LogWarning("Refresh token failed: access token could not be parsed.");
            return Result.Failure<AuthResponse>(AuthErrors.InvalidToken);
        }

        //Load the refresh token as a tracked entity (single query — avoids double round-trip)
        var refreshToken = await refreshTokenRepository
            .FindSingleAsync(r => r.Token == request.RefreshToken, cancellationToken);

        if (refreshToken is null)
        {
            _logger.LogWarning("Refresh token failed: refresh token was not found.");
            return Result.Failure<AuthResponse>(AuthErrors.InvalidRefreshToken);
        }

        if (!refreshToken.IsActive)
        {
            _logger.LogWarning("Refresh token failed: refresh token is expired, used, or revoked. UserId: {UserId}", refreshToken.UserId);

            // SECURITY CHECK: Refresh Token Reuse Detection (RFC 6819)
            if (refreshToken.IsUsed)
            {
                _logger.LogWarning("SECURITY ALERT: Suspected Refresh Token Hijacking. Revoking all sessions for UserId: {UserId}", refreshToken.UserId);
                await refreshTokenRepository.RevokeAllForUserAsync(refreshToken.UserId, cancellationToken);
            }

            return Result.Failure<AuthResponse>(AuthErrors.RefreshTokenExpiredOrRevoked);
        }

        if (refreshToken.Jti != jti)
        {
            _logger.LogWarning("Refresh token failed: access token and refresh token mismatch. UserId: {UserId}", refreshToken.UserId);
            return Result.Failure<AuthResponse>(AuthErrors.TokenMismatch);
        }

        var user = await userAuthService.FindByIdAsync(refreshToken.UserId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Refresh token failed: associated user was not found. UserId: {UserId}", refreshToken.UserId);
            return Result.Failure<AuthResponse>(AuthErrors.AssociatedUserNotFound);
        }

        var (newAccessToken, newJti, newAccessExpiresAt) = jwtTokenService.GenerateAccessToken(
            user.Id, user.Email, user.UserName, user.FirstName, user.LastName, user.Roles);
        var newRefreshTokenString = jwtTokenService.GenerateRefreshToken();
        var newRefreshExpiresAt = DateTime.UtcNow.AddDays(30);

        //Rotate — mark old token as used (already tracked, no extra query needed)
        refreshToken.MarkUsed(newRefreshTokenString);
        refreshTokenRepository.Update(refreshToken);

        var newRefreshToken = AppRefreshToken.Create(newRefreshTokenString, newJti, user.Id, newRefreshExpiresAt);
        refreshTokenRepository.Add(newRefreshToken);

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
