using FastBiteGroup.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var mediatrLicense = builder.AddParameter("mediatr-license", secret: true);
var autoMapperLicense = builder.AddParameter("automapper-license", secret: true);

var database = builder.AddApplicationPostgres();
var cache = builder.AddApplicationRedis();

var api = builder.AddApplicationApi(
    database,
    cache,
    mediatrLicense,
    autoMapperLicense);

// builder.AddOptionalFrontend(api);
// builder.AddOptionalDesktop(api);

builder.Build().Run();