using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using FastBiteGroup.Desktop.UI.Validators;

namespace FastBiteGroup.Desktop.UI.ViewModels;

public partial class LoginViewModel : ValidationViewModelBase<LoginViewModel>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmailEmpty))]
    private string _email = string.Empty;

    public bool IsEmailEmpty => string.IsNullOrEmpty(Email);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordEmpty))]
    private string _password = string.Empty;

    public bool IsPasswordEmpty => string.IsNullOrEmpty(Password);

    [ObservableProperty]
    private bool _isPasswordVisible;

    [ObservableProperty]
    private bool _isLoading;

    // Event triggered when login is successful to transition views
    public event Action? LoginSuccessful;

    // Event triggered to transition to RegisterWindow
    public event Action? NavigateToRegister;

    public LoginViewModel(LoginViewModelValidator validator) : base(validator)
    {
    }

    [RelayCommand]
    private void NavigateToRegisterView()
    {
        Log.Information("Navigation to register view triggered.");
        NavigateToRegister?.Invoke();
    }

    [RelayCommand]
    private void Login()
    {
        if (!ValidateAll())
        {
            return;
        }

        Log.Information("User attempted login with Email: {Email}", Email);
        IsLoading = true;

        try
        {
            // Simulate brief network delay
            MessageBox.Show($"Đăng nhập thành công với tài khoản:\nEmail: {Email}", "Đăng nhập thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            
            // Trigger transition to MainWindow
            LoginSuccessful?.Invoke();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ForgotPassword()
    {
        Log.Information("Forgot password clicked.");
        MessageBox.Show("Tính năng khôi phục mật khẩu đang được phát triển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void GoogleLogin()
    {
        Log.Information("Google social login clicked.");
        MessageBox.Show("Đăng nhập bằng tài khoản Google...", "Google Auth", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void MicrosoftLogin()
    {
        Log.Information("Microsoft social login clicked.");
        MessageBox.Show("Đăng nhập bằng tài khoản Microsoft...", "Microsoft Auth", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void AppleLogin()
    {
        Log.Information("Apple social login clicked.");
        MessageBox.Show("Đăng nhập bằng tài khoản Apple...", "Apple Auth", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
