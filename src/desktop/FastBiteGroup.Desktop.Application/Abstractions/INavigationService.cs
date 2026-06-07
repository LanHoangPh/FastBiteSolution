namespace FastBiteGroup.Desktop.Application.Abstractions;

public interface INavigationService
{
    void NavigateTo<TViewModel>() where TViewModel : class;
    void GoBack();
}
