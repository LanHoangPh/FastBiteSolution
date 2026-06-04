using FastBiteGroup.MigrationService;
using FastBiteGroup.Persistence.DependencyInjection.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddPostgreSqlPersistence(builder.Configuration);
builder.Services.AddIdentityPersistence();
builder.Services.Configure<SeedDataOptions>(
    builder.Configuration.GetSection(SeedDataOptions.SectionName));
builder.Services.AddScoped<SeedDataInitializer>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();
