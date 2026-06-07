using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;
using FastBiteGroup.Desktop.Application.Abstractions;
using FastBiteGroup.Desktop.Infrastructure.ApiClients;
using FastBiteGroup.Desktop.Infrastructure.Storage;

namespace FastBiteGroup.Desktop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // 1. Storage
        services.AddSingleton<ITokenStorage, TokenStorage>();

        // 2. Api Handlers
        services.AddTransient<JwtAuthHeaderHandler>();

        // 3. Refit API Clients with Polly Retry Policy
        services.AddRefitClient<IAuthClient>()
            .ConfigureHttpClient((sp, c) => 
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var baseUrl = config["ApiSettings:BaseUrl"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    c.BaseAddress = new Uri(baseUrl);
                }
            })
            .AddHttpMessageHandler<JwtAuthHeaderHandler>()
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)));

        return services;
    }
}
