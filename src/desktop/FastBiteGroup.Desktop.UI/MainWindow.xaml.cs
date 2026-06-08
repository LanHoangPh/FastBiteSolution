using System.Windows;
using System.Windows.Controls;
using FastBiteGroup.Desktop.UI.Services;

namespace FastBiteGroup.Desktop.UI;

public partial class MainWindow : Window
{
    private readonly IThemeService _themeService;

    public MainWindow(IThemeService themeService)
    {
        _themeService = themeService;
        InitializeComponent();
        _themeService.ApplySyncfusionTheme(this);
        RefreshThemeMenu();

        // Populate sample logs in DataGrid
        AccessLogGrid.ItemsSource = new System.Collections.Generic.List<LogEntry>
        {
            new LogEntry { Time = "17:35:23", User = "Alex Mercer", IP = "192.168.1.50", Status = "Success" },
            new LogEntry { Time = "17:38:10", User = "Jane Doe", IP = "10.0.0.12", Status = "Success" },
            new LogEntry { Time = "17:40:02", User = "John Smith", IP = "172.16.2.8", Status = "Failed" }
        };
    }

    public class LogEntry
    {
        public string Time { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }


    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        SettingsPopup.IsOpen = !SettingsPopup.IsOpen;
    }

    private void OnThemeSystemClick(object sender, RoutedEventArgs e)
    {
        SetTheme(AppThemeMode.System);
    }

    private void OnThemeLightClick(object sender, RoutedEventArgs e)
    {
        SetTheme(AppThemeMode.Light);
    }

    private void OnThemeDarkClick(object sender, RoutedEventArgs e)
    {
        SetTheme(AppThemeMode.Dark);
    }

    private void SetTheme(AppThemeMode mode)
    {
        _themeService.SetTheme(mode);
        _themeService.ApplySyncfusionTheme(this);
        RefreshThemeMenu();
        SettingsPopup.IsOpen = false;
    }

    private void OnThemeToggleChecked(object sender, RoutedEventArgs e)
    {
        SetTheme(AppThemeMode.Dark);
    }

    private void OnThemeToggleUnchecked(object sender, RoutedEventArgs e)
    {
        SetTheme(AppThemeMode.Light);
    }

    private void RefreshThemeMenu()
    {
        SetThemeCheck(ThemeSystemCheck, _themeService.CurrentMode == AppThemeMode.System);
        SetThemeCheck(ThemeLightCheck, _themeService.CurrentMode == AppThemeMode.Light);
        SetThemeCheck(ThemeDarkCheck, _themeService.CurrentMode == AppThemeMode.Dark);

        ThemeStatusText.Text = _themeService.CurrentMode == AppThemeMode.System
            ? $"Theme: System ({_themeService.CurrentResolvedTheme})"
            : $"Theme: {_themeService.CurrentMode}";

        // Update the ToggleSwitch checked status
        if (ThemeToggleSwitch != null)
        {
            // Unregister events to avoid recursive triggers
            ThemeToggleSwitch.Checked -= OnThemeToggleChecked;
            ThemeToggleSwitch.Unchecked -= OnThemeToggleUnchecked;

            ThemeToggleSwitch.IsChecked = _themeService.CurrentResolvedTheme == ResolvedTheme.Dark;

            ThemeToggleSwitch.Checked += OnThemeToggleChecked;
            ThemeToggleSwitch.Unchecked += OnThemeToggleUnchecked;
        }
    }

    private static void SetThemeCheck(TextBlock checkText, bool isChecked)
    {
        checkText.Visibility = isChecked ? Visibility.Visible : Visibility.Hidden;
    }
}
