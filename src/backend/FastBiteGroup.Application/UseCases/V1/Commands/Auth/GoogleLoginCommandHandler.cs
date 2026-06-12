using FastBiteGroup.Contract.Services.V1.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class GoogleLoginCommandHandler(
    IGoogleAuthService googleAuthService,
    IUserAuthService userAuthService,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<GoogleLoginCommandHandler> logger)
    : IRequestHandler<GoogleLoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var googlePayloadResult = await googleAuthService.ValidateAsync(request.IdToken, cancellationToken);
        if (googlePayloadResult.IsFailure)
        {
            return Result.Failure<AuthResponse>(googlePayloadResult.Error);
        }

        var payload = googlePayloadResult.Value;

        var user = await userAuthService.FindByEmailAsync(payload.Email, cancellationToken);

        if (user is null)
        {
            var (newUser, error) = await userAuthService.CreateUserFromGoogleAsync(
                payload.Email,
                payload.FirstName,
                payload.LastName,
                payload.Picture,
                cancellationToken);

            if (newUser is null)
            {
                logger.LogError("Failed to auto-register user from Google payload: {Error}", error);
                return Result.Failure<AuthResponse>(AuthErrors.GoogleRegistrationFailed(error));
            }

            user = newUser;
        }
        else if (!user.IsActive)
        {
            // If user registered normally but hasn't activated, logging in via Google confirms and activates them
            await userAuthService.ActivateUserAsync(user.Email, cancellationToken);
            user = user with { IsActive = true, EmailConfirmed = true };
        }

        var (accessToken, jti, accessExpiresAt) = jwtTokenService.GenerateAccessToken(
            user.Id, user.Email, user.UserName, user.FirstName, user.LastName, user.Roles);
        var refreshTokenString = jwtTokenService.GenerateRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.AddDays(30);

        var refreshTokenEntity = FastBiteGroup.Domain.Entities.AppRefreshToken.Create(refreshTokenString, jti, user.Id, refreshExpiresAt);
        refreshTokenRepository.Add(refreshTokenEntity);

        var response = new AuthResponse(
            accessToken,
            refreshTokenString,
            accessExpiresAt,
            refreshExpiresAt,
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

        return Result.Success(response);
    }
}
