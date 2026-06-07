using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Application.Abstractions.Emails;
using FastBiteGroup.Infrastructure.BackgroundJobs;
using FastBiteGroup.Infrastructure.DependencyInjection.Options;
using FastBiteGroup.Infrastructure.Emails;
using FastBiteGroup.Infrastructure.Services;

namespace FastBiteGroup.Infrastructure.DependencyInjection.Extentions;

public static class ServiceCollectionExtensions
{
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

        services.AddSingleton<IOtpService, OtpService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserAuthService, UserAuthService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        // Register ICurrentUser — reads claims from HTTP request context
        services.AddCurrentUser();

        // Emails
        services.Configure<SendGridOptions>(
            configuration.GetSection(nameof(SendGridOptions)));
        services.AddTransient<IEmailSender, SendGridEmailSender>();

        var mongoConnectionString = configuration.GetConnectionString("MongoDBConnection")
            ?? configuration.GetConnectionString("MongoDb")
            ?? configuration.GetConnectionString("mongodb")
            ?? configuration["MongoDbOptions:ConnectionString"];

        if (!string.IsNullOrWhiteSpace(mongoConnectionString))
        {
            services.AddHostedService<IntegrationOutboxProcessorService>();
        }

        return services;
    }

    /// <summary>
    /// Registers IHttpContextAccessor (required for ICurrentUser) and CurrentUserService.
    /// ICurrentUser is Scoped — one instance per HTTP request, matching the request pipeline lifetime.
    /// </summary>
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        return services;
    }
}
