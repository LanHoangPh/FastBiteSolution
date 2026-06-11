using FastBiteGroup.Desktop.Application.Abstractions;
using FastBiteGroup.Desktop.Infrastructure.Authentication;
using FastBiteGroup.Desktop.Infrastructure.Http;
using FastBiteGroup.Desktop.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;

namespace FastBiteGroup.Desktop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // 1. Storage & Auth Authentication services
        services.AddSingleton<ITokenStorage, TokenStorage>();
        services.AddSingleton<ISecureTokenStore, DpapiSecureTokenStore>();
        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IAuthService, AuthService>();

        // 2. Api Handlers
        services.AddTransient<AuthHeaderHandler>();

        // 3. Refit API Clients with Polly Retry Policy
        void ConfigureClient(IServiceProvider sp, HttpClient c)
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var baseUrl = config["ApiSettings:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                c.BaseAddress = new Uri(baseUrl);
            }
        }

        services.AddRefitClient<IAuthClient>()
            .ConfigureHttpClient(ConfigureClient)
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)));

        services.AddRefitClient<IUserClient>()
            .ConfigureHttpClient(ConfigureClient)
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)));

        return services;
    }
}
