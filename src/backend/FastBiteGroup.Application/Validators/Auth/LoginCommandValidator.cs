using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FluentValidation;

namespace FastBiteGroup.Application.Validators.Auth;

public sealed class LoginCommandValidator : AbstractValidator<AuthCommands.LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}
