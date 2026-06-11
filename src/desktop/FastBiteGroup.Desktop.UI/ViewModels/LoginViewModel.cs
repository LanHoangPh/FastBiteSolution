using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastBiteGroup.Desktop.Application.Models.Auth;
using FastBiteGroup.Desktop.Application.UseCases.Auth;
using FastBiteGroup.Desktop.UI.Services;
using FastBiteGroup.Desktop.UI.Validators;
using Serilog;

namespace FastBiteGroup.Desktop.UI.ViewModels;

public partial class LoginViewModel : ValidationViewModelBase<LoginViewModel>
{
    private readonly LoginUseCase _loginUseCase;
    private readonly IDialogService _dialogService;

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

    public event Action? LoginSuccessful;
    public event Action? NavigateToRegister;
    public event Action? NavigateToForgotPassword;

    public LoginViewModel(
        LoginUseCase loginUseCase,
        IDialogService dialogService,
        LoginViewModelValidator validator)
        : base(validator)
    {
        _loginUseCase = loginUseCase;
        _dialogService = dialogService;
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
            var result = await _loginUseCase.ExecuteAsync(new LoginRequest(Email, Password));
            if (result.IsSuccess)
            {
                LoginSuccessful?.Invoke();
            }
            else
            {
                _dialogService.ShowError(result.Error.Message, "DialogTitleLoginFailed");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred during login");
            _dialogService.ShowErrorResource("DialogMessageUnexpectedLoginError", "DialogTitleError");
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
        _dialogService.ShowInformationResource("DialogMessageGoogleLogin", "DialogTitleGoogleAuth");
    }

    [RelayCommand]
    private void MicrosoftLogin()
    {
        Log.Information("Microsoft social login clicked.");
        _dialogService.ShowInformationResource("DialogMessageMicrosoftLogin", "DialogTitleMicrosoftAuth");
    }

    [RelayCommand]
    private void AppleLogin()
    {
        Log.Information("Apple social login clicked.");
        _dialogService.ShowInformationResource("DialogMessageAppleLogin", "DialogTitleAppleAuth");
    }
}
