namespace FastBiteGroup.AppHost.Extensions;

internal static class DatabaseExtensions
{
    internal static IResourceBuilder<PostgresDatabaseResource> AddApplicationPostgres(
        this IDistributedApplicationBuilder builder)
    {
        var postgres = builder.AddPostgres("postgres");

        return postgres.AddDatabase("DefaultConnection");
    }

    internal static IResourceBuilder<MongoDBDatabaseResource> AddApplicationMongoDb(
        this IDistributedApplicationBuilder builder)
    {
        var postgres = builder.AddMongoDB("mongodb");

        return postgres.AddDatabase("MongoDBConnection");
    }

    internal static IResourceBuilder<IResourceWithConnectionString> AddApplicationMongoDbConnection(
        this IDistributedApplicationBuilder builder)
    {
        return builder.AddConnectionString("MongoDBConnection");
    }
    internal static IResourceBuilder<IResourceWithConnectionString> AddApplicationPostgresConnection(
        this IDistributedApplicationBuilder builder)
    {
        return builder.AddConnectionString("DefaultConnection");
    }
}
