using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastBiteGroup.Desktop.Application.Abstractions;
using FastBiteGroup.Desktop.Application.Models.Auth;
using FastBiteGroup.Desktop.UI.Validators;
using Serilog;

namespace FastBiteGroup.Desktop.UI.ViewModels;

public partial class LoginViewModel : ValidationViewModelBase<LoginViewModel>
{
    private readonly IAuthService _authService;

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

    // Event triggered to transition to ForgotPassword Grid
    public event Action? NavigateToForgotPassword;

    public LoginViewModel(IAuthService authService, LoginViewModelValidator validator) : base(validator)
    {
        _authService = authService;
    }

    [RelayCommand]
    private void NavigateToRegisterView()
    {
        Log.Information("Navigation to register view triggered.");
        NavigateToRegister?.Invoke();
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (!ValidateAll())
        {
            return;
        }

        Log.Information("User attempted login with Email: {Email}", Email);
        IsLoading = true;

        try
        {
            var result = await _authService.LoginAsync(new LoginRequest(Email, Password));
            if (result.IsSuccess)
            {
                // Trigger transition to MainWindow
                LoginSuccessful?.Invoke();
            }
            else
            {
                MessageBox.Show(result.Error.Message, "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred during login");
            MessageBox.Show("Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
        NavigateToForgotPassword?.Invoke();
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
