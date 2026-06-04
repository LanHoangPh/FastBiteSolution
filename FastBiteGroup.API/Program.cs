using FastBiteGroup.API.DependencyInjection.Extensions;
using FastBiteGroup.API.Middleware;
using FastBiteGroup.Application.DependencyInjection.Extentions;
using FastBiteGroup.Infrastructure.DependencyInjection.Extentions;
using FastBiteGroup.Persistence.DependencyInjection.Extensions;
using Serilog;

namespace FastBiteGroup.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration;

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();

        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();
        });

        Log.Information("Starting FastBiteGroup API...");

        builder.AddServiceDefaults();

        builder.Services.AddPostgreSqlPersistence(config);
        builder.Services.AddIdentityPersistence();
        builder.Services.AddRepositoryPersistence();
        builder.Services.AddInterceptorPersistence();

        builder.Services.AddRedisInfrastructure(config);
        builder.Services.AddSecurityInfrastructure(config);

        builder.Services.AddApplicationServices(config);

        builder.Services.AddJwtAuthentication(config);

        builder.Services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGenNewtonsoftSupport();
        builder.Services.AddSwagger();

        builder.Services.AddTransient<TokenBlacklistMiddleware>();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddApiVersioning(options =>
            options.ReportApiVersions = true)
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        var app = builder.Build();

        app.MapEndpoints();

        app.MapDefaultEndpoints();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            app.UseDeveloperExceptionPage();
            app.ConfigureSwagger();
        }

        app.UseSerilogRequestLogging();

        app.UseAuthentication();
        app.UseTokenBlacklist();
        app.UseAuthorization();

        try
        {
            await app.RunAsync();
            Log.Information("Stopped FastBiteGroup API");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "FastBiteGroup API terminated unexpectedly");
            await app.StopAsync();
        }
        finally
        {
            Log.CloseAndFlush();
            await app.DisposeAsync();
        }
    }
}
