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

public partial class RegisterViewModel : ValidationViewModelBase<RegisterViewModel>
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private DateTime? _dayOfBirth = DateTime.Today.AddYears(-18); // Default to 18 years ago

    [ObservableProperty]
    private bool _isPasswordVisible;

    [ObservableProperty]
    private bool _isLoading;

    // Navigation event to return to Login Window
    public event Action? NavigateToLogin;

    public RegisterViewModel(IAuthService authService, RegisterViewModelValidator validator) : base(validator)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (!ValidateAll())
        {
            return;
        }

        var dob = DayOfBirth;
        if (dob == null)
        {
            return;
        }

        Log.Information("User attempted registration with Email: {Email}, Name: {LastName} {FirstName}, DoB: {DoB}", 
            Email, LastName, FirstName, dob.Value.ToString("yyyy-MM-dd"));
        
        IsLoading = true;

        try
        {
            // Create the record model using the imported one from Application.Models.Auth
            var request = new RegisterRequest(Email, Password, FirstName, LastName, dob.Value);
            var result = await _authService.RegisterAsync(request);

            if (result.IsSuccess)
            {
                MessageBox.Show(result.Value.Message, 
                    "Đăng ký thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Go back to login screen upon successful registration
                NavigateToLogin?.Invoke();
            }
            else
            {
                MessageBox.Show(result.Error.Message, "Lỗi đăng ký", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred during registration");
            MessageBox.Show("Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void BackToLogin()
    {
        Log.Information("Navigation back to login triggered.");
        NavigateToLogin?.Invoke();
    }

    [RelayCommand]
    private void GoogleRegister()
    {
        Log.Information("Google social register clicked.");
        MessageBox.Show("Đăng ký bằng tài khoản Google...", "Google Auth", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void MicrosoftRegister()
    {
        Log.Information("Microsoft social register clicked.");
        MessageBox.Show("Đăng ký bằng tài khoản Microsoft...", "Microsoft Auth", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void AppleRegister()
    {
        Log.Information("Apple social register clicked.");
        MessageBox.Show("Đăng ký bằng tài khoản Apple...", "Apple Auth", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
