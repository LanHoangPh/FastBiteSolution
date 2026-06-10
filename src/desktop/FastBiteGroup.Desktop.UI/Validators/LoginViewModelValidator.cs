using FluentValidation;
using FastBiteGroup.Desktop.UI.ViewModels;

namespace FastBiteGroup.Desktop.UI.Validators;

public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
{
    public LoginViewModelValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(_ => GetLocalizedString("ValidationErrorEmailEmpty", "Email không được để trống."))
            .EmailAddress().WithMessage(_ => GetLocalizedString("ValidationErrorEmailInvalid", "Định dạng Email không hợp lệ."));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_ => GetLocalizedString("ValidationErrorPasswordEmpty", "Mật khẩu không được để trống."))
            .MinimumLength(6).WithMessage(_ => GetLocalizedString("ValidationErrorPasswordLength", "Mật khẩu phải chứa ít nhất 6 ký tự."));
    }

    private static string GetLocalizedString(string resourceKey, string defaultValue)
    {
        if (System.Windows.Application.Current != null)
        {
            return System.Windows.Application.Current.TryFindResource(resourceKey) as string ?? defaultValue;
        }
        return defaultValue;
    }
}
