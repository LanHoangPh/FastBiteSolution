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

internal sealed class RegisterCommandHandler
    : ICommandHandler<AuthCommands.RegisterCommand, AuthResponse>
{
    private readonly IUserAuthService _userAuthService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserAuthService userAuthService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<RegisterCommandHandler>? logger = null)
    {
        _userAuthService = userAuthService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger ?? NullLogger<RegisterCommandHandler>.Instance;
    }

    public async Task<Result<AuthResponse>> Handle(
        AuthCommands.RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check if email already exists (single read query)
        var existing = await _userAuthService.FindByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            _logger.LogWarning("Registration failed: email already exists.");
            return Result.Failure<AuthResponse>(
                new Error("Auth.EmailAlreadyExists", $"Email '{request.Email}' is already registered."));
        }

        // 2. Create user (returns UserDto directly — no second round-trip needed)
        var (user, errorMessage) = await _userAuthService.CreateUserAsync(
            request.Email, request.Password, request.FirstName, request.LastName,
            request.DayOfBirth, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Registration failed while creating user. Error: {Error}", errorMessage);
            return Result.Failure<AuthResponse>(new Error("Auth.RegistrationFailed", errorMessage!));
        }

        // 3. Generate tokens
        var (accessToken, jti, accessExpiresAt) = _jwtTokenService.GenerateAccessToken(
            user.Id, user.Email, user.UserName, user.FirstName, user.LastName, user.Roles);
        var refreshTokenString = _jwtTokenService.GenerateRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.AddDays(30);

        // 4. Persist refresh token
        var refreshToken = AppRefreshToken.Create(refreshTokenString, jti, user.Id, refreshExpiresAt);
        _refreshTokenRepository.Add(refreshToken);

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

        _logger.LogInformation("User registered successfully. UserId: {UserId}", user.Id);

        return Result.Success(response);
    }
}
