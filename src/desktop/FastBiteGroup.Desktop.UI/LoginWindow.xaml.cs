using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FastBiteGroup.Desktop.UI.Services;
using FastBiteGroup.Desktop.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FastBiteGroup.Desktop.UI;

public partial class LoginWindow : Window
{
    private readonly IThemeService? _themeService;

    public LoginWindow(
        LoginViewModel loginViewModel, 
        RegisterViewModel registerViewModel,
        ForgotPasswordViewModel forgotPasswordViewModel)
    {
        InitializeComponent();

        _themeService = App.AppHost?.Services.GetService<IThemeService>();
        UpdateThemeButtonVisuals();

        // Set dynamic culture and language for the window to format dates as dd/MM/yyyy
        var langService = App.AppHost?.Services.GetService<ILanguageService>();
        if (langService != null)
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(
                langService.CurrentLanguage == "en" ? "en-GB" : "vi-VN");
        }
        
        // Wire up individual data contexts to their respective grids
        LoginGrid.DataContext = loginViewModel;
        RegisterGrid.DataContext = registerViewModel;
        ForgotPasswordGrid.DataContext = forgotPasswordViewModel;

        // Subscribing to transition event from LoginViewModel (successful auth)
        loginViewModel.LoginSuccessful += () =>
        {
            var mainWindow = App.AppHost?.Services.GetRequiredService<MainWindow>();
            if (mainWindow != null)
            {
                mainWindow.Show();
                this.Close();
            }
        };

        // Subscribing to register view transition
        loginViewModel.NavigateToRegister += SwitchToRegister;

        // Subscribing to login view transition from RegisterViewModel
        registerViewModel.NavigateToLogin += SwitchToLogin;

        // Subscribing to forgot password view transitions
        loginViewModel.NavigateToForgotPassword += SwitchToForgotPassword;
        forgotPasswordViewModel.NavigateToLogin += SwitchFromForgotPassword;
    }



    private void SwitchToRegister()
    {
        // 1. Change window title dynamically
        if (TryFindResource("RegisterWindowTitle") is string regTitle)
        {
            this.Title = regTitle;
        }

        // 2. Animate out LoginGrid (opacity 1 -> 0, slide left: 0 -> -50)
        var fadeOutLogin = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.18));
        var slideOutLogin = new DoubleAnimation(0, -50, TimeSpan.FromSeconds(0.18));

        fadeOutLogin.Completed += (s, e) =>
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            RegisterGrid.Visibility = Visibility.Visible;
            RegisterTranslate.X = 50;

            // 3. Animate in RegisterGrid (opacity 0 -> 1, slide in: 50 -> 0)
            var fadeInRegister = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.22));
            var slideInRegister = new DoubleAnimation(50, 0, TimeSpan.FromSeconds(0.22))
            {
                DecelerationRatio = 0.6
            };

            RegisterGrid.BeginAnimation(UIElement.OpacityProperty, fadeInRegister);
            RegisterTranslate.BeginAnimation(TranslateTransform.XProperty, slideInRegister);
        };

        LoginGrid.BeginAnimation(UIElement.OpacityProperty, fadeOutLogin);
        LoginTranslate.BeginAnimation(TranslateTransform.XProperty, slideOutLogin);
    }

    private void SwitchToLogin()
    {
        // 1. Change window title dynamically
        if (TryFindResource("LoginWindowTitle") is string logTitle)
        {
            this.Title = logTitle;
        }

        // 2. Animate out RegisterGrid (opacity 1 -> 0, slide right: 0 -> 50)
        var fadeOutRegister = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.18));
        var slideOutRegister = new DoubleAnimation(0, 50, TimeSpan.FromSeconds(0.18));

        fadeOutRegister.Completed += (s, e) =>
        {
            RegisterGrid.Visibility = Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Visible;
            LoginTranslate.X = -50;

            // 3. Animate in LoginGrid (opacity 0 -> 1, slide in: -50 -> 0)
            var fadeInLogin = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.22));
            var slideInLogin = new DoubleAnimation(-50, 0, TimeSpan.FromSeconds(0.22))
            {
                DecelerationRatio = 0.6
            };

            LoginGrid.BeginAnimation(UIElement.OpacityProperty, fadeInLogin);
            LoginTranslate.BeginAnimation(TranslateTransform.XProperty, slideInLogin);
        };

        RegisterGrid.BeginAnimation(UIElement.OpacityProperty, fadeOutRegister);
        RegisterTranslate.BeginAnimation(TranslateTransform.XProperty, slideOutRegister);
    }

    private void SwitchToForgotPassword()
    {
        // 1. Change window title dynamically
        if (TryFindResource("ForgotPasswordWindowTitle") is string forgotTitle)
        {
            this.Title = forgotTitle;
        }

        // 2. Animate out LoginGrid (opacity 1 -> 0, slide left: 0 -> -50)
        var fadeOutLogin = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.18));
        var slideOutLogin = new DoubleAnimation(0, -50, TimeSpan.FromSeconds(0.18));

        fadeOutLogin.Completed += (s, e) =>
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            ForgotPasswordGrid.Visibility = Visibility.Visible;
            ForgotPasswordTranslate.X = 50;

            // 3. Animate in ForgotPasswordGrid (opacity 0 -> 1, slide in: 50 -> 0)
            var fadeInForgot = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.22));
            var slideInForgot = new DoubleAnimation(50, 0, TimeSpan.FromSeconds(0.22))
            {
                DecelerationRatio = 0.6
            };

            ForgotPasswordGrid.BeginAnimation(UIElement.OpacityProperty, fadeInForgot);
            ForgotPasswordTranslate.BeginAnimation(TranslateTransform.XProperty, slideInForgot);
        };

        LoginGrid.BeginAnimation(UIElement.OpacityProperty, fadeOutLogin);
        LoginTranslate.BeginAnimation(TranslateTransform.XProperty, slideOutLogin);
    }

    private void SwitchFromForgotPassword()
    {
        // 1. Change window title dynamically
        if (TryFindResource("LoginWindowTitle") is string logTitle)
        {
            this.Title = logTitle;
        }

        // 2. Animate out ForgotPasswordGrid (opacity 1 -> 0, slide right: 0 -> 50)
        var fadeOutForgot = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.18));
        var slideOutForgot = new DoubleAnimation(0, 50, TimeSpan.FromSeconds(0.18));

        fadeOutForgot.Completed += (s, e) =>
        {
            ForgotPasswordGrid.Visibility = Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Visible;
            LoginTranslate.X = -50;

            // 3. Animate in LoginGrid (opacity 0 -> 1, slide in: -50 -> 0)
            var fadeInLogin = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.22));
            var slideInLogin = new DoubleAnimation(-50, 0, TimeSpan.FromSeconds(0.22))
            {
                DecelerationRatio = 0.6
            };

            LoginGrid.BeginAnimation(UIElement.OpacityProperty, fadeInLogin);
            LoginTranslate.BeginAnimation(TranslateTransform.XProperty, slideInLogin);
        };

        ForgotPasswordGrid.BeginAnimation(UIElement.OpacityProperty, fadeOutForgot);
        ForgotPasswordTranslate.BeginAnimation(TranslateTransform.XProperty, slideOutForgot);
    }

    private void ThemeToggleBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_themeService == null) return;

        var newMode = _themeService.CurrentResolvedTheme == ResolvedTheme.Dark
            ? AppThemeMode.Light
            : AppThemeMode.Dark;

        _themeService.SetTheme(newMode);
        UpdateThemeButtonVisuals();
    }

    private void UpdateThemeButtonVisuals()
    {
        if (_themeService == null) return;

        var isDark = _themeService.CurrentResolvedTheme == ResolvedTheme.Dark;
        var geometryKey = isDark ? "WeatherSunnyIcon" : "WeatherMoonIcon";

        if (TryFindResource(geometryKey) is Geometry geometry)
        {
            ThemeToggleIcon.Data = geometry;
        }
    }

    private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.MinimizeWindow(this);
    }

    private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            SystemCommands.RestoreWindow(this);
        }
        else
        {
            SystemCommands.MaximizeWindow(this);
        }
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        var maximizeIcon = this.FindName("MaximizeIcon") as System.Windows.Shapes.Path;
        var maximizeBtn = this.FindName("MaximizeBtn") as Button;
        if (maximizeIcon != null && maximizeBtn != null)
        {
            if (WindowState == WindowState.Maximized)
            {
                maximizeIcon.Data = System.Windows.Media.Geometry.Parse("M 1 3 L 7 3 L 7 9 L 1 9 Z M 3 3 L 3 1 L 9 1 L 9 7 L 7 7");
                maximizeBtn.ToolTip = TryFindResource("RestoreTooltip") as string ?? "Restore Down";
            }
            else
            {
                maximizeIcon.Data = System.Windows.Media.Geometry.Parse("M 1 1 L 9 1 L 9 9 L 1 9 Z");
                maximizeBtn.ToolTip = TryFindResource("MaximizeTooltip") as string ?? "Maximize";
            }
        }
    }

    private void DoBInput_DateValidationError(object sender, DatePickerDateValidationErrorEventArgs e)
    {
        if (sender is DatePicker datePicker)
        {
            var text = e.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            // Keep only digits from the typed text
            var cleanText = new string(text.Where(char.IsDigit).ToArray());

            if (cleanText.Length == 8) // e.g., 10062026 -> 10/06/2026
            {
                if (DateTime.TryParseExact(cleanText, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    datePicker.SelectedDate = parsedDate;
                    e.ThrowException = false; // Prevents validation error popup/border
                    return;
                }
            }
            else if (cleanText.Length == 6) // e.g., 100685 -> 10/06/1985
            {
                if (DateTime.TryParseExact(cleanText, "ddMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    datePicker.SelectedDate = parsedDate;
                    e.ThrowException = false; // Prevents validation error popup/border
                    return;
                }
            }
        }
    }
}
