using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Contract.Abstractions.Outbox;
using FastBiteGroup.Persistence.DependencyInjection.Options;
using FastBiteGroup.Persistence.Identity;
using FastBiteGroup.Persistence.Mongo;
using FastBiteGroup.Persistence.Mongo.Messages;
using FastBiteGroup.Persistence.Mongo.Notifications;
using FastBiteGroup.Persistence.Mongo.Outbox;
using FastBiteGroup.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace FastBiteGroup.Persistence.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers EF Core with PostgreSQL (Npgsql) using connection string from configuration.
    /// Connection string "DefaultConnection" is injected by .NET Aspire in dev, or via env var in prod.
    /// </summary>
    public static IServiceCollection AddPostgreSqlPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });

            var environmentName = configuration["ASPNETCORE_ENVIRONMENT"]
                ?? configuration["DOTNET_ENVIRONMENT"];

            if (string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase))
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        return services;
    }

    public static IServiceCollection AddIdentityPersistence(this IServiceCollection services)
    {
        services.AddIdentityCore<AppUser>(options =>
        {
            // Password policy
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;

            // Lockout
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<AppRoles>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        return services;
    }

    public static IServiceCollection AddRepositoryPersistence(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, EFUnitOfWork>();
        services.AddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
    public static IServiceCollection AddInterceptorPersistence(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        // Future: register UpdateAuditableEntitiesInterceptor here
        return services;
    }

    public static IServiceCollection AddMongoPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDBConnection")
            ?? configuration.GetConnectionString("MongoDb");

        if (string.IsNullOrWhiteSpace(connectionString))
            return services;

        services.AddOptions<MongoDbOptions>()
            .Bind(configuration.GetSection(nameof(MongoDbOptions)))
            .PostConfigure(options =>
            {
                if (string.IsNullOrWhiteSpace(options.ConnectionString))
                    options.ConnectionString = connectionString;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<MongoDbOptions>>()
                .Value;

            return new MongoClient(options.ConnectionString);
        });

        services.AddSingleton<MongoDbContext>();
        services.AddScoped<IMessageDocumentStore, MongoMessageDocumentStore>();
        services.AddScoped<INotificationDocumentStore, MongoNotificationDocumentStore>();
        services.AddScoped<IIntegrationOutboxStore, MongoIntegrationOutboxStore>();
        services.AddHostedService<MongoIndexInitializer>();

        return services;
    }
}
