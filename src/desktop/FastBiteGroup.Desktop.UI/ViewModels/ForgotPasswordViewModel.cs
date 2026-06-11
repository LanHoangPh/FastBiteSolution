using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastBiteGroup.Desktop.Application.Abstractions;
using FastBiteGroup.Desktop.UI.Validators;
using Serilog;

namespace FastBiteGroup.Desktop.UI.ViewModels;

public partial class ForgotPasswordViewModel : ValidationViewModelBase<ForgotPasswordViewModel>
{
    private readonly IAuthService _authService;

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

    // Navigation event to return to Login Window
    public event Action? NavigateToLogin;

    public ForgotPasswordViewModel(
        IAuthService authService,
        ForgotPasswordViewModelValidator validator) 
        : base(validator)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task SendOtpAsync(CancellationToken cancellationToken)
    {
        // We only want to validate the Email property for sending OTP
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
                MessageBox.Show(
                    "Một mã xác thực OTP gồm 6 chữ số đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.",
                    "Gửi OTP thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result.Error.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error sending password reset OTP");
            MessageBox.Show("Đã xảy ra lỗi khi gửi yêu cầu. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(
                    "Mật khẩu của bạn đã được đặt lại thành công. Vui lòng đăng nhập bằng mật khẩu mới.",
                    "Đặt lại thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Clear fields and go back to login
                ResetState();
                NavigateToLogin?.Invoke();
            }
            else
            {
                MessageBox.Show(result.Error.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error resetting password");
            MessageBox.Show("Đã xảy ra lỗi khi đặt lại mật khẩu. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
