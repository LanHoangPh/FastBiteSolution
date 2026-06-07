using System;
using Microsoft.Extensions.DependencyInjection;
using FastBiteGroup.Desktop.Application.Abstractions;

namespace FastBiteGroup.Desktop.UI.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public event Action<object>? CurrentViewModelChanged;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        CurrentViewModelChanged?.Invoke(viewModel);
    }

    public void GoBack()
    {
    }
}
