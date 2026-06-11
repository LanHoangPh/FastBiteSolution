using Microsoft.Extensions.DependencyInjection;
using FastBiteGroup.Desktop.Application.UseCases.Auth;

namespace FastBiteGroup.Desktop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<LoginUseCase>();
        services.AddTransient<RegisterUseCase>();

        return services;
    }
}
