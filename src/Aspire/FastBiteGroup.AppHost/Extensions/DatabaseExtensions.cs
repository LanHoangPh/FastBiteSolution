namespace FastBiteGroup.AppHost.Extensions;

internal static class DatabaseExtensions
{
    internal static IResourceBuilder<PostgresDatabaseResource> AddApplicationPostgres(
        this IDistributedApplicationBuilder builder)
    {
        var postgres = builder.AddPostgres("postgres")
            .WithDataVolume();

        return postgres.AddDatabase("DefaultConnection");
    }
    internal static IResourceBuilder<IResourceWithConnectionString> AddApplicationPostgresConnection(
        this IDistributedApplicationBuilder builder)
    {
        return builder.AddConnectionString("DefaultConnection");
    }
}