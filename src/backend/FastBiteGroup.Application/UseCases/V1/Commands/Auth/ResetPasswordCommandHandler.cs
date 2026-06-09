using System;
using System.Threading;
using System.Threading.Tasks;
using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Domain.Abstractions.Repositories;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

public sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly IUserAuthService _userAuthService;
    private readonly IOtpService _otpService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public ResetPasswordCommandHandler(
        IUserAuthService userAuthService,
        IOtpService otpService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userAuthService = userAuthService;
        _otpService = otpService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userAuthService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        // Validate OTP from IOtpService
        var validationResult = await _otpService.ValidateOtpAsync("RESET_PWD", request.Email, request.Otp, maxAttempts: 5, ct: cancellationToken);

        if (validationResult == OtpValidationResult.MaxAttemptsReached)
        {
            return Result.Failure(AuthErrors.AccountBlocked);
        }

        if (validationResult != OtpValidationResult.Success)
        {
            return Result.Failure(AuthErrors.InvalidOtp);
        }

        // OTP is valid, proceed to reset password. 
        // This method will be implemented by another Agent.
        var resetResult = await _userAuthService.ResetPasswordAsync(request.Email, request.NewPassword, cancellationToken);

        if (resetResult.IsFailure)
        {
            return resetResult;
        }

        // Security best practice: Revoke all existing refresh tokens after password reset
        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);

        return Result.Success();
    }
}
