using Microsoft.Extensions.DependencyInjection;

namespace FastBiteGroup.Desktop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Đăng ký UseCases, Validators, Mappers tại đây
        // Ví dụ: services.AddTransient<LoginUseCase>();

        return services;
    }
}
