using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Infrastructure.DependencyInjection.Options;
using FastBiteGroup.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Security.Authentication;

namespace FastBiteGroup.Infrastructure.DependencyInjection.Extentions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Redis IConnectionMultiplexer (singleton) and ICacheService (scoped).
    /// Connection string "redis" is injected by .NET Aspire in dev.
    /// </summary>
    public static IServiceCollection AddRedisInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("redis")
            ?? throw new InvalidOperationException(
                "Redis connection string 'redis' is not configured. " +
                "In development, run via Aspire AppHost which provisions Redis automatically.");

        var options = ConfigurationOptions.Parse(redisConnectionString);

        options.AbortOnConnectFail = false;
        options.ConnectTimeout = 10000;
        options.SyncTimeout = 10000;
        options.ConnectRetry = 3;
        options.Ssl = false;

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(options));

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }

    public static IServiceCollection AddSecurityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(
            configuration.GetSection(nameof(JwtOptions)));

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserAuthService, UserAuthService>();

        return services;
    }
}
