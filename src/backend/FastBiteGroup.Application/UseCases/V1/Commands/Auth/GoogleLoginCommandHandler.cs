using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;
using FastBiteGroup.Domain.Abstractions.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, Result<AuthResponse>>
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserAuthService _userAuthService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<GoogleLoginCommandHandler> _logger;

    public GoogleLoginCommandHandler(
        IGoogleAuthService googleAuthService,
        IUserAuthService userAuthService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<GoogleLoginCommandHandler> logger)
    {
        _googleAuthService = googleAuthService;
        _userAuthService = userAuthService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify Google ID Token
        var googlePayloadResult = await _googleAuthService.ValidateAsync(request.IdToken, cancellationToken);
        if (googlePayloadResult.IsFailure)
        {
            return Result.Failure<AuthResponse>(googlePayloadResult.Error);
        }

        var payload = googlePayloadResult.Value;

        // 2. Check if user exists
        var user = await _userAuthService.FindByEmailAsync(payload.Email, cancellationToken);

        if (user is null)
        {
            // 3. Auto-Register if not found
            var (newUser, error) = await _userAuthService.CreateUserFromGoogleAsync(
                payload.Email,
                payload.FirstName,
                payload.LastName,
                payload.Picture,
                cancellationToken);

            if (newUser is null)
            {
                _logger.LogError("Failed to auto-register user from Google payload: {Error}", error);
                return Result.Failure<AuthResponse>(new Error("GoogleLogin.RegistrationFailed", error ?? "Auto-registration failed."));
            }

            user = newUser;
        }
        else if (!user.IsActive)
        {
            // If user registered normally but hasn't activated, logging in via Google confirms and activates them
            await _userAuthService.ActivateUserAsync(user.Email, cancellationToken);
            user = user with { IsActive = true, EmailConfirmed = true };
        }

        // 4. Generate Tokens
        var (accessToken, jti, accessExpiresAt) = _jwtTokenService.GenerateAccessToken(
            user.Id, user.Email, user.UserName, user.FirstName, user.LastName, user.Roles);
        var refreshTokenString = _jwtTokenService.GenerateRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.AddDays(30);

        // 5. Store Refresh Token
        var refreshTokenEntity = FastBiteGroup.Domain.Entities.AppRefreshToken.Create(refreshTokenString, jti, user.Id, refreshExpiresAt);
        _refreshTokenRepository.Add(refreshTokenEntity);

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
