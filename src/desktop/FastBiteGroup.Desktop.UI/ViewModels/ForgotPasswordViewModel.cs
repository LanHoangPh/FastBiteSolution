using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastBiteGroup.Desktop.Application.Abstractions;
using FastBiteGroup.Desktop.UI.Services;
using FastBiteGroup.Desktop.UI.Validators;
using Serilog;

namespace FastBiteGroup.Desktop.UI.ViewModels;

public partial class ForgotPasswordViewModel : ValidationViewModelBase<ForgotPasswordViewModel>
{
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmailEmpty))]
    private string _email = string.Empty;

    public bool IsEmailEmpty => string.IsNullOrEmpty(Email);

    [ObservableProperty]
    private string _otp = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _isOtpSent;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isPasswordVisible;

    public event Action? NavigateToLogin;

    public ForgotPasswordViewModel(
        IAuthService authService,
        IDialogService dialogService,
        ForgotPasswordViewModelValidator validator)
        : base(validator)
    {
        _authService = authService;
        _dialogService = dialogService;
    }

    [RelayCommand]
    private async Task SendOtpAsync(CancellationToken cancellationToken)
    {
        if (!ValidateProperty(nameof(Email)))
        {
            return;
        }

        Log.Information("User requested password reset OTP for: {Email}", Email);
        IsLoading = true;

        try
        {
            var result = await _authService.ForgotPasswordAsync(Email, cancellationToken);
            if (result.IsSuccess)
            {
                IsOtpSent = true;
                _dialogService.ShowInformationResource("DialogMessageOtpSent", "DialogTitleOtpSent");
            }
            else
            {
                _dialogService.ShowError(result.Error.Message, "DialogTitleError");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error sending password reset OTP");
            _dialogService.ShowErrorResource("DialogMessageUnexpectedSendOtpError", "DialogTitleError");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ResetPasswordAsync(CancellationToken cancellationToken)
    {
        if (!ValidateAll())
        {
            return;
        }

        Log.Information("User resetting password for: {Email} using OTP", Email);
        IsLoading = true;

        try
        {
            var result = await _authService.ResetPasswordAsync(Email, Otp, NewPassword, cancellationToken);
            if (result.IsSuccess)
            {
                _dialogService.ShowInformationResource(
                    "DialogMessageResetPasswordSuccess",
                    "DialogTitleResetPasswordSuccess");

                ResetState();
                NavigateToLogin?.Invoke();
            }
            else
            {
                _dialogService.ShowError(result.Error.Message, "DialogTitleError");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error resetting password");
            _dialogService.ShowErrorResource("DialogMessageUnexpectedResetPasswordError", "DialogTitleError");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void BackToLogin()
    {
        ResetState();
        NavigateToLogin?.Invoke();
    }

    private void ResetState()
    {
        Email = string.Empty;
        Otp = string.Empty;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
        IsOtpSent = false;
        ClearErrors();
    }
}
