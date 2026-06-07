using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32;
using Syncfusion.Windows.Shared;

namespace FastBiteGroup.Desktop.UI.Services;

public sealed class ThemeService : IThemeService
{
    private const string SettingsFileName = "settings.json";
    private static readonly Uri LightThemeUri = new("/Resources/Themes/LightColors.xaml", UriKind.Relative);
    private static readonly Uri DarkThemeUri = new("/Resources/Themes/DarkColors.xaml", UriKind.Relative);

    private readonly string _settingsPath;

    public ThemeService()
    {
        var settingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FastBite");

        _settingsPath = Path.Combine(settingsFolder, SettingsFileName);
    }

    public AppThemeMode CurrentMode { get; private set; } = AppThemeMode.System;
    public ResolvedTheme CurrentResolvedTheme { get; private set; } = ResolvedTheme.Light;

    public void Initialize()
    {
        CurrentMode = LoadThemeMode();
        ApplyTheme(CurrentMode, persist: false);
    }

    public void SetTheme(AppThemeMode mode)
    {
        ApplyTheme(mode, persist: true);
    }

    public void ApplySyncfusionTheme(DependencyObject target)
    {
        var visualStyle = CurrentResolvedTheme == ResolvedTheme.Dark
            ? Skin.Office2007Black.ToString()
            : Skin.Default.ToString();

        SkinStorage.SetVisualStyle(target, visualStyle);
    }

    private void ApplyTheme(AppThemeMode mode, bool persist)
    {
        CurrentMode = mode;
        CurrentResolvedTheme = ResolveTheme(mode);

        ReplaceColorDictionary(CurrentResolvedTheme == ResolvedTheme.Dark ? DarkThemeUri : LightThemeUri);

        if (persist)
        {
            SaveThemeMode(mode);
        }
    }

    private static ResolvedTheme ResolveTheme(AppThemeMode mode)
    {
        return mode switch
        {
            AppThemeMode.Light => ResolvedTheme.Light,
            AppThemeMode.Dark => ResolvedTheme.Dark,
            _ => IsWindowsAppThemeDark() ? ResolvedTheme.Dark : ResolvedTheme.Light
        };
    }

    private static bool IsWindowsAppThemeDark()
    {
        const string personalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        using var key = Registry.CurrentUser.OpenSubKey(personalizeKey);
        var appsUseLightTheme = key?.GetValue("AppsUseLightTheme");

        return appsUseLightTheme is int value && value == 0;
    }

    private static void ReplaceColorDictionary(Uri source)
    {
        var dictionaries = System.Windows.Application.Current.Resources.MergedDictionaries;

        for (var index = 0; index < dictionaries.Count; index++)
        {
            if (IsThemeColorDictionary(dictionaries[index]))
            {
                dictionaries[index] = new ResourceDictionary { Source = source };
                return;
            }
        }

        dictionaries.Insert(0, new ResourceDictionary { Source = source });
    }

    private static bool IsThemeColorDictionary(ResourceDictionary dictionary)
    {
        var source = dictionary.Source?.OriginalString;
        return source is not null
            && (source.EndsWith("Colors.xaml", StringComparison.OrdinalIgnoreCase)
                || source.Contains("/Resources/Themes/", StringComparison.OrdinalIgnoreCase));
    }

    private AppThemeMode LoadThemeMode()
    {
        if (!File.Exists(_settingsPath))
        {
            return AppThemeMode.System;
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);

            return Enum.TryParse<AppThemeMode>(settings?.ThemeMode, ignoreCase: true, out var mode)
                ? mode
                : AppThemeMode.System;
        }
        catch
        {
            return AppThemeMode.System;
        }
    }

    private void SaveThemeMode(AppThemeMode mode)
    {
        var folder = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var settings = new AppSettings { ThemeMode = mode.ToString() };
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsPath, json);
    }

    private sealed class AppSettings
    {
        public string ThemeMode { get; set; } = AppThemeMode.System.ToString();
    }
}
