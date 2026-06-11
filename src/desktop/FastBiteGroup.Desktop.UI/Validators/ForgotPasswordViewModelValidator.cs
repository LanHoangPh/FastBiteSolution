using FluentValidation;
using FastBiteGroup.Desktop.UI.ViewModels;

namespace FastBiteGroup.Desktop.UI.Validators;

public class ForgotPasswordViewModelValidator : AbstractValidator<ForgotPasswordViewModel>
{
    public ForgotPasswordViewModelValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(_ => GetLocalizedString("ValidationErrorEmailEmpty", "Email không được để trống."))
            .EmailAddress().WithMessage(_ => GetLocalizedString("ValidationErrorEmailInvalid", "Định dạng Email không hợp lệ."));

        RuleFor(x => x.Otp)
            .NotEmpty().When(x => x.IsOtpSent).WithMessage(_ => GetLocalizedString("ValidationErrorOtpEmpty", "Mã OTP không được để trống."))
            .Length(6).When(x => x.IsOtpSent).WithMessage(_ => GetLocalizedString("ValidationErrorOtpLength", "Mã OTP phải có đúng 6 ký tự."));

        RuleFor(x => x.NewPassword)
            .NotEmpty().When(x => x.IsOtpSent).WithMessage(_ => GetLocalizedString("ValidationErrorPasswordEmpty", "Mật khẩu không được để trống."))
            .MinimumLength(6).When(x => x.IsOtpSent).WithMessage(_ => GetLocalizedString("ValidationErrorPasswordLength", "Mật khẩu phải chứa ít nhất 6 ký tự."));

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().When(x => x.IsOtpSent).WithMessage(_ => GetLocalizedString("ValidationErrorConfirmPasswordEmpty", "Vui lòng xác nhận mật khẩu."))
            .Equal(x => x.NewPassword).When(x => x.IsOtpSent).WithMessage(_ => GetLocalizedString("ValidationErrorConfirmPasswordMismatch", "Mật khẩu xác nhận không khớp."));
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
