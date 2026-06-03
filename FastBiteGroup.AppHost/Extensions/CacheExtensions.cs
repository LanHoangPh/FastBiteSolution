namespace FastBiteGroup.AppHost.Extensions;

internal static class CacheExtensions
{
    internal static IResourceBuilder<RedisResource> AddApplicationRedis(
        this IDistributedApplicationBuilder builder)
    {
        return builder.AddRedis("redis");
    }
}