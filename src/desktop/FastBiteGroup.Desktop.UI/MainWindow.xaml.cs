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

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (MaximizeIcon != null)
        {
            if (WindowState == WindowState.Maximized)
            {
                MaximizeIcon.Data = System.Windows.Media.Geometry.Parse("M 1 3 L 7 3 L 7 9 L 1 9 Z M 3 3 L 3 1 L 9 1 L 9 7 L 7 7");
                MaximizeBtn.ToolTip = "Restore Down";
            }
            else
            {
                MaximizeIcon.Data = System.Windows.Media.Geometry.Parse("M 1 1 L 9 1 L 9 9 L 1 9 Z");
                MaximizeBtn.ToolTip = "Maximize";
            }
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
}
