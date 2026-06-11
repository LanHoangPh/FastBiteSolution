namespace FastBiteGroup.Desktop.UI.Services;

public interface IDialogService
{
    void ShowInformation(string message, string titleResourceKey);
    void ShowError(string message, string titleResourceKey);
    void ShowInformationResource(string messageResourceKey, string titleResourceKey);
    void ShowErrorResource(string messageResourceKey, string titleResourceKey);
}
