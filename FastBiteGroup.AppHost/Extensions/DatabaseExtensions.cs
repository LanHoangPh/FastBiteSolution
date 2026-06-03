namespace FastBiteGroup.AppHost.Extensions;

internal static class DatabaseExtensions
{
    internal static IResourceBuilder<PostgresDatabaseResource> AddApplicationPostgres(
        this IDistributedApplicationBuilder builder)
    {
        var postgres = builder.AddPostgres("postgres");

        return postgres.AddDatabase("DefaultConnection");
    }
}