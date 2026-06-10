using System.Windows;
using FastBiteGroup.Desktop.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FastBiteGroup.Desktop.UI;

public partial class RegisterWindow : Window
{
    public RegisterWindow(RegisterViewModel viewModel)
    {
        InitializeComponent();
        
        DataContext = viewModel;

        // Subscribing to back to login event from ViewModel
        viewModel.NavigateToLogin += () =>
        {
            var loginWindow = App.AppHost?.Services.GetRequiredService<LoginWindow>();
            if (loginWindow != null)
            {
                loginWindow.Show();
                this.Close();
            }
        };
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
