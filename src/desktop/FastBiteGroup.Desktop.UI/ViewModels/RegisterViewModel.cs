using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using FastBiteGroup.Desktop.UI.Validators;

namespace FastBiteGroup.Desktop.UI.ViewModels;

public partial class RegisterViewModel : ValidationViewModelBase<RegisterViewModel>
{
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

    public RegisterViewModel(RegisterViewModelValidator validator) : base(validator)
    {
    }

    [RelayCommand]
    private void Register()
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
            // Create the record model as requested
            var request = new RegisterRequest(Email, Password, FirstName, LastName, dob.Value);

            MessageBox.Show($"Đăng ký tài khoản thành công!\nChào mừng {request.LastName} {request.FirstName} tham gia FastBite.", 
                "Đăng ký thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            
            // Go back to login screen upon successful registration
            NavigateToLogin?.Invoke();
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

// Define the record as requested by the user
public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateTime DayOfBirth);
