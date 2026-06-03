using FastBiteGroup.Application.Behaviors;
using FastBiteGroup.Application.DependencyInjection.Options;
using FastBiteGroup.Application.Mappers;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FastBiteGroup.Application.DependencyInjection.Extentions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LicenseKeyOptions>(
            configuration.GetSection(nameof(LicenseKeyOptions))); 
        services.AddConfigureMediatR(configuration);
        services.AddConfigureAutoMapper(configuration);

        services.AddValidatorsFromAssembly(
            AssemblyReference.Assembly,
            includeInternalTypes: true);

        return services;
    }


    private static IServiceCollection AddConfigureMediatR(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var licenseOptions = configuration
            .GetSection(nameof(LicenseKeyOptions))
            .Get<LicenseKeyOptions>() ?? new LicenseKeyOptions();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly);
            cfg.AddOpenBehavior(typeof(PerformancePipelineBehavior<,>));
            cfg.AddOpenBehavior(typeof(TracingPipelineBehaviors<,>));
            cfg.AddOpenBehavior(typeof(TransactionPipelineBehaviors<,>));
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehaviors<,>));

            cfg.LicenseKey = licenseOptions.LicenseKeyMediatR;
        });

        return services;
    }

    private static IServiceCollection AddConfigureAutoMapper(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var licenseOptions = configuration
            .GetSection(nameof(LicenseKeyOptions))
            .Get<LicenseKeyOptions>() ?? new LicenseKeyOptions();

        return services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<ServiceProfile>();
            cfg.LicenseKey = licenseOptions.LicenseKeyAutoMapper;
        });
    }
}
