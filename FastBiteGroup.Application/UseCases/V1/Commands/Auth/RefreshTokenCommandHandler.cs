using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class RefreshTokenCommandHandler
    : ICommandHandler<AuthCommands.RefreshTokenCommand, AuthResponse>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserAuthService _userAuthService;

    public RefreshTokenCommandHandler(
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUserAuthService userAuthService)
    {
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _userAuthService = userAuthService;
    }

    public async Task<Result<AuthResponse>> Handle(
        AuthCommands.RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Extract JTI from the (possibly expired) access token
        var jti = _jwtTokenService.GetJtiFromExpiredToken(request.AccessToken);
        if (jti is null)
            return Result.Failure<AuthResponse>(
                new Error("Auth.InvalidToken", "Access token is invalid or cannot be parsed."));

        // 2. Find the refresh token in DB (read-only)
        var refreshToken = await _refreshTokenRepository
            .FindByTokenAsync(request.RefreshToken, cancellationToken);

        if (refreshToken is null)
            return Result.Failure<AuthResponse>(
                new Error("Auth.InvalidRefreshToken", "Refresh token not found."));

        // 3. Validate state
        if (!refreshToken.IsActive)
            return Result.Failure<AuthResponse>(
                new Error("Auth.RefreshTokenExpiredOrRevoked",
                    "Refresh token has expired or been revoked. Please log in again."));

        // 4. Validate JTI linkage
        if (refreshToken.Jti != jti)
            return Result.Failure<AuthResponse>(
                new Error("Auth.TokenMismatch", "Access token and refresh token do not match."));

        // 5. Get user
        var user = await _userAuthService.FindByIdAsync(refreshToken.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<AuthResponse>(
                new Error("Auth.UserNotFound", "Associated user was not found."));

        // 6. Generate new tokens
        var (newAccessToken, newJti, newAccessExpiresAt) = _jwtTokenService.GenerateAccessToken(
            user.Id, user.Email, user.UserName, user.FirstName, user.LastName, user.Roles);
        var newRefreshTokenString = _jwtTokenService.GenerateRefreshToken();
        var newRefreshExpiresAt = DateTime.UtcNow.AddDays(30);

        // 7. Rotate — load tracked entity for mutation
        var trackedToken = await _refreshTokenRepository
            .FindSingleAsync(r => r.Token == request.RefreshToken, cancellationToken);
        trackedToken.MarkUsed(newRefreshTokenString);
        _refreshTokenRepository.Update(trackedToken);

        var newRefreshToken = AppRefreshToken.Create(newRefreshTokenString, newJti, user.Id, newRefreshExpiresAt);
        _refreshTokenRepository.Add(newRefreshToken);

        var response = new AuthResponse(
            newAccessToken,
            newRefreshTokenString,
            newAccessExpiresAt,
            newRefreshExpiresAt,
            new UserInfoResponse(user.Id, user.Email, user.FirstName, user.LastName, user.Roles));

        return Result.Success(response);
    }
}
