using System.Windows;
using FastBiteGroup.Desktop.UI.Services;
using FastBiteGroup.Desktop.UI.ViewModels;

namespace FastBiteGroup.Desktop.UI;

public partial class MainWindow : Window
{
    private readonly IThemeService _themeService;

    public MainWindow(IThemeService themeService, MainWindowViewModel viewModel)
    {
        _themeService = themeService;
        
        InitializeComponent();
        
        DataContext = viewModel;
        
        // Apply syncfusion theme initially
        _themeService.ApplySyncfusionTheme(this);
        
        // Listen to theme changes to re-apply syncfusion theme
        viewModel.ThemeChanged += () =>
        {
            _themeService.ApplySyncfusionTheme(this);
        };
    }
}
