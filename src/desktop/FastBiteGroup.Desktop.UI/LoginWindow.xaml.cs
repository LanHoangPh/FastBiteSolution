using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FastBiteGroup.Desktop.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FastBiteGroup.Desktop.UI;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel loginViewModel, RegisterViewModel registerViewModel)
    {
        InitializeComponent();
        
        // Wire up individual data contexts to their respective grids
        LoginGrid.DataContext = loginViewModel;
        RegisterGrid.DataContext = registerViewModel;

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

    private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.MinimizeWindow(this);
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }
}
