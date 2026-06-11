using System.Windows;

namespace FastBiteGroup.Desktop.UI.Services;

public sealed class DialogService : IDialogService
{
    public void ShowInformation(string message, string titleResourceKey)
    {
        MessageBox.Show(message, GetString(titleResourceKey), MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string titleResourceKey)
    {
        MessageBox.Show(message, GetString(titleResourceKey), MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowInformationResource(string messageResourceKey, string titleResourceKey)
    {
        ShowInformation(GetString(messageResourceKey), titleResourceKey);
    }

    public void ShowErrorResource(string messageResourceKey, string titleResourceKey)
    {
        ShowError(GetString(messageResourceKey), titleResourceKey);
    }

    private static string GetString(string resourceKey)
    {
        return System.Windows.Application.Current.TryFindResource(resourceKey) as string ?? resourceKey;
    }
}
