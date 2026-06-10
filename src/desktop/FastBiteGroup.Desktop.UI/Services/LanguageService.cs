using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace FastBiteGroup.Desktop.UI.Services;

public sealed class LanguageService : ILanguageService
{
    private const string SettingsFileName = "language.json";
    private static readonly Uri ViLanguageUri = new("/Resources/Languages/Strings.vi.xaml", UriKind.Relative);
    private static readonly Uri EnLanguageUri = new("/Resources/Languages/Strings.en.xaml", UriKind.Relative);

    private readonly string _settingsPath;

    public LanguageService()
    {
        var settingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FastBite");

        _settingsPath = Path.Combine(settingsFolder, SettingsFileName);
    }

    public string CurrentLanguage { get; private set; } = "vi";

    public void Initialize()
    {
        CurrentLanguage = LoadSavedLanguageCode();
        ApplyLanguage(CurrentLanguage, persist: false);
    }

    public void SetLanguage(string langCode)
    {
        if (langCode == "vi" || langCode == "en")
        {
            ApplyLanguage(langCode, persist: true);
        }
    }

    public IEnumerable<string> GetSupportedLanguages()
    {
        return new[] { "vi", "en" };
    }

    private void ApplyLanguage(string langCode, bool persist)
    {
        CurrentLanguage = langCode;
        var uri = langCode == "en" ? EnLanguageUri : ViLanguageUri;

        ReplaceLanguageDictionary(uri);

        // Update default thread culture for string, number, and date formatting
        var cultureName = langCode == "en" ? "en-GB" : "vi-VN";
        var culture = new System.Globalization.CultureInfo(cultureName);
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
        System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

        if (persist)
        {
            SaveLanguageCode(langCode);
        }
    }

    private static void ReplaceLanguageDictionary(Uri source)
    {
        var dictionaries = System.Windows.Application.Current.Resources.MergedDictionaries;

        for (var index = 0; index < dictionaries.Count; index++)
        {
            if (IsLanguageDictionary(dictionaries[index]))
            {
                dictionaries[index] = new ResourceDictionary { Source = source };
                return;
            }
        }

        // Insert at index 0 if not found
        dictionaries.Insert(0, new ResourceDictionary { Source = source });
    }

    private static bool IsLanguageDictionary(ResourceDictionary dictionary)
    {
        var source = dictionary.Source?.OriginalString;
        return source is not null
            && source.Contains("/Resources/Languages/Strings.", StringComparison.OrdinalIgnoreCase);
    }

    private string LoadSavedLanguageCode()
    {
        if (!File.Exists(_settingsPath))
        {
            return "vi"; // Default language
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<LanguageSettings>(json);
            return settings?.LanguageCode == "en" ? "en" : "vi";
        }
        catch
        {
            return "vi";
        }
    }

    private void SaveLanguageCode(string langCode)
    {
        var folder = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var settings = new LanguageSettings { LanguageCode = langCode };
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsPath, json);
    }

    private sealed class LanguageSettings
    {
        public string LanguageCode { get; set; } = "vi";
    }
}
