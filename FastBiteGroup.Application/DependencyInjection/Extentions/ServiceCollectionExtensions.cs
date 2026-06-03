using FastBiteGroup.Application.Behaviors;
using FastBiteGroup.Application.Mappers;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FastBiteGroup.Application.DependencyInjection.Extentions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigureMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly);

            cfg.AddOpenBehavior(typeof(PerformancePipelineBehavior<,>));

            cfg.AddOpenBehavior(typeof(TracingPipelineBehaviors<,>));

            cfg.AddOpenBehavior(typeof(TransactionPipelineBehaviors<,>));

            cfg.AddOpenBehavior(typeof(ValidationPipelineBehaviors<,>));

            cfg.LicenseKey = "";
        });
        return services;
    }

    public static IServiceCollection AddConfigureAutoMapper(this IServiceCollection services)
    {
       return services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<ServiceProfile>();
            cfg.LicenseKey = "";
        });
    }
}
