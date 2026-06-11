using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastBiteGroup.Desktop.Application.Models.Auth;
using FastBiteGroup.Desktop.Application.UseCases.Auth;
using FastBiteGroup.Desktop.UI.Services;
using FastBiteGroup.Desktop.UI.Validators;
using Serilog;

namespace FastBiteGroup.Desktop.UI.ViewModels;

public partial class RegisterViewModel : ValidationViewModelBase<RegisterViewModel>
{
    private readonly RegisterUseCase _registerUseCase;
    private readonly IDialogService _dialogService;

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
    private DateTime? _dayOfBirth = DateTime.Today.AddYears(-18);

    [ObservableProperty]
    private bool _isPasswordVisible;

    [ObservableProperty]
    private bool _isLoading;

    public event Action? NavigateToLogin;

    public RegisterViewModel(
        RegisterUseCase registerUseCase,
        IDialogService dialogService,
        RegisterViewModelValidator validator)
        : base(validator)
    {
        _registerUseCase = registerUseCase;
        _dialogService = dialogService;
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

        Log.Information(
            "User attempted registration with Email: {Email}, Name: {LastName} {FirstName}, DoB: {DoB}",
            Email,
            LastName,
            FirstName,
            dob.Value.ToString("yyyy-MM-dd"));

        IsLoading = true;

        try
        {
            var request = new RegisterRequest(Email, Password, FirstName, LastName, dob.Value);
            var result = await _registerUseCase.ExecuteAsync(request);

            if (result.IsSuccess)
            {
                _dialogService.ShowInformation(
                    result.Value.Message,
                    "DialogTitleRegisterSuccess");

                NavigateToLogin?.Invoke();
            }
            else
            {
                _dialogService.ShowError(result.Error.Message, "DialogTitleRegisterFailed");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred during registration");
            _dialogService.ShowErrorResource("DialogMessageUnexpectedRegisterError", "DialogTitleError");
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
        _dialogService.ShowInformationResource("DialogMessageGoogleRegister", "DialogTitleGoogleAuth");
    }

    [RelayCommand]
    private void MicrosoftRegister()
    {
        Log.Information("Microsoft social register clicked.");
        _dialogService.ShowInformationResource("DialogMessageMicrosoftRegister", "DialogTitleMicrosoftAuth");
    }

    [RelayCommand]
    private void AppleRegister()
    {
        Log.Information("Apple social register clicked.");
        _dialogService.ShowInformationResource("DialogMessageAppleRegister", "DialogTitleAppleAuth");
    }
}
