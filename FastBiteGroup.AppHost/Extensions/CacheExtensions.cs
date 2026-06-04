namespace FastBiteGroup.AppHost.Extensions;

internal static class CacheExtensions
{
    internal static IResourceBuilder<RedisResource> AddApplicationRedis(
        this IDistributedApplicationBuilder builder)
    {
        return builder.AddRedis("redis");
    }
    internal static IResourceBuilder<IResourceWithConnectionString> AddApplicationRedisConnection(
        this IDistributedApplicationBuilder builder)
    {
        return builder.AddConnectionString("redis");
    }
}