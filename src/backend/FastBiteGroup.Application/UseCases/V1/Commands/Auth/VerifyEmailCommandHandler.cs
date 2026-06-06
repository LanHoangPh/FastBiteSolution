using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class VerifyEmailCommandHandler
    : ICommandHandler<AuthCommands.VerifyEmailCommand, AuthResponse>
{
    private readonly IUserAuthService _userAuthService;
    private readonly IOtpService _otpService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<VerifyEmailCommandHandler> _logger;

    public VerifyEmailCommandHandler(
        IUserAuthService userAuthService,
        IOtpService otpService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<VerifyEmailCommandHandler>? logger = null)
    {
        _userAuthService = userAuthService;
        _otpService = otpService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger ?? NullLogger<VerifyEmailCommandHandler>.Instance;
    }

    public async Task<Result<AuthResponse>> Handle(
        AuthCommands.VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get user
        var user = await _userAuthService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AuthResponse>(new Error("Auth.UserNotFound", "User not found."));
        }

        if (user.EmailConfirmed)
        {
            return Result.Failure<AuthResponse>(new Error("Auth.EmailAlreadyConfirmed", "Email is already confirmed."));
        }

        bool isValid = false;

        // 2. Try OTP validation first
        var validationResult = await _otpService.ValidateOtpAsync("REGISTER", request.Email, request.Code, maxAttempts: 5, ct: cancellationToken);

        if (validationResult == OtpValidationResult.MaxAttemptsReached)
        {
            _logger.LogWarning("OTP brute-force attempt blocked for email: {Email}", request.Email);
            return Result.Failure<AuthResponse>(new Error("Auth.OtpLimitReached", "Too many failed OTP attempts. Please register again."));
        }

        if (validationResult == OtpValidationResult.Success)
        {
            isValid = await _userAuthService.ActivateUserAsync(request.Email, cancellationToken);
        }

        // 3. If not OTP or OTP failed, try Magic Link Token validation
        if (!isValid)
        {
            isValid = await _userAuthService.ConfirmEmailWithTokenAsync(request.Email, request.Code, cancellationToken);
        }

        if (!isValid)
        {
            _logger.LogWarning("Verification failed for email: {Email}", request.Email);
            return Result.Failure<AuthResponse>(new Error("Auth.InvalidCode", "Invalid or expired verification code."));
        }

        // Refetch user to get updated state (IsActive, EmailConfirmed)
        user = await _userAuthService.FindByEmailAsync(request.Email, cancellationToken);

        // 4. Generate tokens
        var (accessToken, jti, accessExpiresAt) = _jwtTokenService.GenerateAccessToken(
            user!.Id, user.Email, user.UserName, user.FirstName, user.LastName, user.Roles);
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
