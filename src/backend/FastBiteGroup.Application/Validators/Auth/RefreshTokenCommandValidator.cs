using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FluentValidation;

namespace FastBiteGroup.Application.Validators.Auth;

public sealed class RefreshTokenCommandValidator : AbstractValidator<AuthCommands.RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
