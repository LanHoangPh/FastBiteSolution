using FastBiteGroup.MigrationService;
using FastBiteGroup.Persistence.DependencyInjection.Extensions;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.AddServiceDefaults();

builder.Services.AddPostgreSqlPersistence(builder.Configuration);
builder.Services.AddIdentityPersistence();
builder.Services.Configure<SeedDataOptions>(
    builder.Configuration.GetSection(SeedDataOptions.SectionName));
builder.Services.AddScoped<SeedDataInitializer>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

try
{
    await host.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
