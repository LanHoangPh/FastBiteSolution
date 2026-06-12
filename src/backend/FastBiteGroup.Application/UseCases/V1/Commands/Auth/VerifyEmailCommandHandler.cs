using FastBiteGroup.Contract.Services.V1.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class VerifyEmailCommandHandler(
    IUserAuthService userAuthService,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<VerifyEmailCommandHandler>? logger = null)
    : ICommandHandler<AuthCommands.VerifyEmailCommand, AuthResponse>
{
    private readonly ILogger<VerifyEmailCommandHandler> _logger = logger ?? NullLogger<VerifyEmailCommandHandler>.Instance;

    public async Task<Result<AuthResponse>> Handle(
        AuthCommands.VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userAuthService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AuthResponse>(AuthErrors.UserNotFound);
        }

        if (user.EmailConfirmed)
        {
            return Result.Failure<AuthResponse>(AuthErrors.EmailAlreadyConfirmed);
        }

        //Confirm email using ASP.NET Identity's purpose-specific token.
        var isValid = await userAuthService.ConfirmEmailWithTokenAsync(
            request.Email,
            request.Token,
            cancellationToken);

        if (!isValid)
        {
            _logger.LogWarning("Verification failed for email: {Email}", request.Email);
            return Result.Failure<AuthResponse>(AuthErrors.InvalidEmailConfirmationToken);
        }

        // Refetch user to get updated state (IsActive, EmailConfirmed)
        user = await userAuthService.FindByEmailAsync(request.Email, cancellationToken);

        var (accessToken, jti, accessExpiresAt) = jwtTokenService.GenerateAccessToken(
            user!.Id, user.Email, user.UserName, user.FirstName, user.LastName, user.Roles);
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

        _logger.LogInformation("Email verified and user logged in successfully. UserId: {UserId}", user.Id);

        return Result.Success(response);
    }
}
