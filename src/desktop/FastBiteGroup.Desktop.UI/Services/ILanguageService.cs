using System.Collections.Generic;

namespace FastBiteGroup.Desktop.UI.Services;

public interface ILanguageService
{
    string CurrentLanguage { get; }
    void Initialize();
    void SetLanguage(string langCode);
    IEnumerable<string> GetSupportedLanguages();
}
