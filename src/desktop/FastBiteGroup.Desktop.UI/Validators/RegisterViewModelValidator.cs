using System;
using FluentValidation;
using FastBiteGroup.Desktop.UI.ViewModels;

namespace FastBiteGroup.Desktop.UI.Validators;

public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
{
    public RegisterViewModelValidator()
    {
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(_ => GetLocalizedString("ValidationErrorLastNameEmpty", "Họ không được để trống."));

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(_ => GetLocalizedString("ValidationErrorFirstNameEmpty", "Tên không được để trống."));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(_ => GetLocalizedString("ValidationErrorEmailEmpty", "Email không được để trống."))
            .EmailAddress().WithMessage(_ => GetLocalizedString("ValidationErrorEmailInvalid", "Định dạng Email không hợp lệ."));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_ => GetLocalizedString("ValidationErrorPasswordEmpty", "Mật khẩu không được để trống."))
            .MinimumLength(6).WithMessage(_ => GetLocalizedString("ValidationErrorPasswordLength", "Mật khẩu phải chứa ít nhất 6 ký tự."));

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage(_ => GetLocalizedString("ValidationErrorConfirmPasswordEmpty", "Vui lòng xác nhận mật khẩu."))
            .Equal(x => x.Password).WithMessage(_ => GetLocalizedString("ValidationErrorConfirmPasswordMismatch", "Mật khẩu xác nhận không khớp."));

        RuleFor(x => x.DayOfBirth)
            .NotNull().WithMessage(_ => GetLocalizedString("ValidationErrorDayOfBirthEmpty", "Vui lòng nhập Ngày sinh."))
            .Must(BeValidAge).WithMessage(_ => GetLocalizedString("ValidationErrorDayOfBirthInvalid", "Ngày sinh không hợp lệ hoặc bạn phải từ 12 tuổi trở lên."));
    }

    private bool BeValidAge(DateTime? dob)
    {
        if (dob == null) return false;
        var today = DateTime.Today;
        var age = today.Year - dob.Value.Year;
        if (dob.Value.Date > today.AddYears(-age)) age--;
        return age >= 12 && age <= 120;
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
