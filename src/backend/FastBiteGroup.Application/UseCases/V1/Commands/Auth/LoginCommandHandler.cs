using FastBiteGroup.Contract.Services.V1.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class LoginCommandHandler(
    IUserAuthService userAuthService,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<LoginCommandHandler>? logger = null)
    : ICommandHandler<AuthCommands.LoginCommand, AuthResponse>
{
    private readonly ILogger<LoginCommandHandler> _logger = logger ?? NullLogger<LoginCommandHandler>.Instance;

    public async Task<Result<AuthResponse>> Handle(
        AuthCommands.LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userAuthService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Login failed: invalid credentials.");
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);
        }

        if (!user.EmailConfirmed || !user.IsActive)
        {
            _logger.LogWarning("Login failed: account is inactive or email not confirmed. UserId: {UserId}", user.Id);
            return Result.Failure<AuthResponse>(AuthErrors.AccountInactive);
        }

        // Check lockout first
        if (await userAuthService.IsLockedOutAsync(user.Id, cancellationToken))
        {
            _logger.LogWarning("Login blocked for locked account. UserId: {UserId}", user.Id);
            return Result.Failure<AuthResponse>(AuthErrors.AccountLocked);
        }

        // Verify password
        var passwordValid = await userAuthService.CheckPasswordAsync(user.Id, request.Password, cancellationToken);
        if (!passwordValid)
        {
            _logger.LogWarning("Login failed: invalid credentials. UserId: {UserId}", user.Id);
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);
        }

        // Generate tokens
        var (accessToken, jti, accessExpiresAt) = jwtTokenService.GenerateAccessToken(
            user.Id, user.Email, user.UserName, user.FirstName, user.LastName, user.Roles);
        var refreshTokenString = jwtTokenService.GenerateRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.AddDays(30);

        var refreshToken = AppRefreshToken.Create(refreshTokenString, jti, user.Id, refreshExpiresAt);
        refreshTokenRepository.Add(refreshToken);

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

        _logger.LogInformation("User logged in successfully. UserId: {UserId}", user.Id);

        return Result.Success(response);
    }
}
