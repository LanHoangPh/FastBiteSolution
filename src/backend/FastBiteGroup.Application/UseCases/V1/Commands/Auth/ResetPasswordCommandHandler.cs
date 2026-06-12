using FastBiteGroup.Contract.Services.V1.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

public sealed class ResetPasswordCommandHandler(
    IUserAuthService userAuthService,
    IOtpService otpService,
    IRefreshTokenRepository refreshTokenRepository)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userAuthService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        // Validate OTP from IOtpService
        var validationResult = await otpService.ValidateOtpAsync("RESET_PWD", request.Email, request.Otp, maxAttempts: 5, ct: cancellationToken);

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
        var resetResult = await userAuthService.ResetPasswordAsync(request.Email, request.NewPassword, cancellationToken);

        if (resetResult.IsFailure)
        {
            return resetResult;
        }

        // Security best practice: Revoke all existing refresh tokens after password reset
        await refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);

        return Result.Success();
    }
}
