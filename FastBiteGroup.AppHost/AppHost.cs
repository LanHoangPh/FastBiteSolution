using FastBiteGroup.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var mediatrLicense = builder.AddParameter("mediatr-license", secret: true);
var autoMapperLicense = builder.AddParameter("automapper-license", secret: true);
// --use local resources for database and cache to speed up development feedback loop
var database = builder.AddApplicationPostgres();
var cache = builder.AddApplicationRedis();


// -- use cloud resources for database and cache to test real-world connectivity and performance
//var database = builder.AddApplicationPostgresConnection();
//var cache = builder.AddApplicationRedisConnection();


var migrations = builder.AddProject<Projects.FastBiteGroup_MigrationService>("migrations")
    .WithReference(database)
    .WaitFor(database);

var api = builder.AddApplicationApi(
        database,
        cache,
        mediatrLicense,
        autoMapperLicense)
    .WaitForCompletion(migrations);

// builder.AddOptionalFrontend(api);
// builder.AddOptionalDesktop(api);

builder.Build().Run();