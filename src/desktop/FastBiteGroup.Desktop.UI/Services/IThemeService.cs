using System.Windows;

namespace FastBiteGroup.Desktop.UI.Services;

public interface IThemeService
{
    AppThemeMode CurrentMode { get; }
    ResolvedTheme CurrentResolvedTheme { get; }

    void Initialize();
    void SetTheme(AppThemeMode mode);
    void ApplySyncfusionTheme(DependencyObject target);
}
