using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class RegisterCommandHandler
    : ICommandHandler<AuthCommands.RegisterCommand, AuthResponse>
{
    private readonly IUserAuthService _userAuthService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RegisterCommandHandler(
        IUserAuthService userAuthService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userAuthService = userAuthService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result<AuthResponse>> Handle(
        AuthCommands.RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check if email already exists
        var existing = await _userAuthService.FindByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            return Result.Failure<AuthResponse>(
                new Error("Auth.EmailAlreadyExists", $"Email '{request.Email}' is already registered."));

        // 2. Create user via service (assigns Customer role)
        var (success, errorMessage) = await _userAuthService.CreateUserAsync(
            request.Email, request.Password, request.FirstName, request.LastName,
            request.DayOfBirth, cancellationToken);

        if (!success)
            return Result.Failure<AuthResponse>(new Error("Auth.RegistrationFailed", errorMessage!));

        // 3. Get the created user to generate tokens
        var user = await _userAuthService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return Result.Failure<AuthResponse>(
                new Error("Auth.RegistrationFailed", "User was created but could not be retrieved."));

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

        return Result.Success(response);
    }
}
