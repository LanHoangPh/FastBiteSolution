namespace FastBiteGroup.AppHost.Extensions;

internal static class ApiExtensions
{
    internal static IResourceBuilder<ProjectResource> AddApplicationApi(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<PostgresDatabaseResource> database,
        IResourceBuilder<MongoDBDatabaseResource>mongoDB,
        //IResourceBuilder<RedisResource> cache,
        //IResourceBuilder<IResourceWithConnectionString> databasepos,
        IResourceBuilder<IResourceWithConnectionString> cacheredis,
        IResourceBuilder<ParameterResource> mediatrLicense,
        IResourceBuilder<ParameterResource> autoMapperLicense,
        IResourceBuilder<ParameterResource> secretKey)
    {
        return builder.AddProject<Projects.FastBiteGroup_API>("api")
            .WithReference(database)
            .WithReference(cacheredis)
            .WithReference(mongoDB)
            .WithEnvironment("LicenseKeyOptions__LicenseKeyMediatR", mediatrLicense)
            .WithEnvironment("LicenseKeyOptions__LicenseKeyAutoMapper", autoMapperLicense)
            .WithEnvironment("JwtOptions__SecretKey", secretKey)
            .WaitFor(mongoDB)
            .WaitFor(database);
            //.WaitFor(cache);
    }
}
