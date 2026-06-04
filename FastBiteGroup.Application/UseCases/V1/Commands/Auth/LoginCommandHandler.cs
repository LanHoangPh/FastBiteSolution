using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class LoginCommandHandler
    : ICommandHandler<AuthCommands.LoginCommand, AuthResponse>
{
    private readonly IUserAuthService _userAuthService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserAuthService userAuthService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<LoginCommandHandler>? logger = null)
    {
        _userAuthService = userAuthService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger ?? NullLogger<LoginCommandHandler>.Instance;
    }

    public async Task<Result<AuthResponse>> Handle(
        AuthCommands.LoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find user
        var user = await _userAuthService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Login failed: invalid credentials.");
            return Result.Failure<AuthResponse>(
                new Error("Auth.InvalidCredentials", "Email or password is incorrect."));
        }

        // 2. Check lockout first
        if (await _userAuthService.IsLockedOutAsync(user.Id, cancellationToken))
        {
            _logger.LogWarning("Login blocked for locked account. UserId: {UserId}", user.Id);
            return Result.Failure<AuthResponse>(
                new Error("Auth.AccountLocked", "Account is locked. Please try again later."));
        }

        // 3. Verify password
        var passwordValid = await _userAuthService.CheckPasswordAsync(user.Id, request.Password, cancellationToken);
        if (!passwordValid)
        {
            _logger.LogWarning("Login failed: invalid credentials. UserId: {UserId}", user.Id);
            return Result.Failure<AuthResponse>(
                new Error("Auth.InvalidCredentials", "Email or password is incorrect."));
        }

        // 4. Generate tokens
        var (accessToken, jti, accessExpiresAt) = _jwtTokenService.GenerateAccessToken(
            user.Id, user.Email, user.UserName, user.FirstName, user.LastName, user.Roles);
        var refreshTokenString = _jwtTokenService.GenerateRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.AddDays(30);

        // 5. Persist refresh token
        var refreshToken = AppRefreshToken.Create(refreshTokenString, jti, user.Id, refreshExpiresAt);
        _refreshTokenRepository.Add(refreshToken);

        var response = new AuthResponse(
            accessToken,
            refreshTokenString,
            accessExpiresAt,
            refreshExpiresAt,
            new UserInfoResponse(user.Id, user.Email, user.FirstName, user.LastName, user.Roles));

        _logger.LogInformation("User logged in successfully. UserId: {UserId}", user.Id);

        return Result.Success(response);
    }
}
