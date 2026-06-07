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

    private void RefreshThemeMenu()
    {
        SetThemeCheck(ThemeSystemCheck, _themeService.CurrentMode == AppThemeMode.System);
        SetThemeCheck(ThemeLightCheck, _themeService.CurrentMode == AppThemeMode.Light);
        SetThemeCheck(ThemeDarkCheck, _themeService.CurrentMode == AppThemeMode.Dark);

        ThemeStatusText.Text = _themeService.CurrentMode == AppThemeMode.System
            ? $"Theme: System ({_themeService.CurrentResolvedTheme})"
            : $"Theme: {_themeService.CurrentMode}";
    }

    private static void SetThemeCheck(TextBlock checkText, bool isChecked)
    {
        checkText.Visibility = isChecked ? Visibility.Visible : Visibility.Hidden;
    }
}
