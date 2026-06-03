using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FastBiteGroup.Infrastructure.DependencyInjection.Extentions;

public static class ServiceCollectionExtensions
{
    public static void AddRedisInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

    }
    public static void AddSecurityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
    }
}
