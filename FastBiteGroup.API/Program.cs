using FastBiteGroup.API.DependencyInjection.Extensions;
using FastBiteGroup.API.Middleware;
using FastBiteGroup.Application.DependencyInjection.Extentions;
using FastBiteGroup.Infrastructure.DependencyInjection.Extentions;
using FastBiteGroup.Persistence.DependencyInjection.Extensions;
using Serilog;
using System.Reflection;

namespace FastBiteGroup.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration;
        // ── Serilog Configuration ───────────────────────────────────────────────
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog();
        Log.Information("Starting FastBiteGroup API...");

        // ── Aspire Service Defaults (OTel, Health Checks, Service Discovery) ──
        builder.AddServiceDefaults();

        // ── Persistence (PostgreSQL + Identity + Repositories) ───────────────
        builder.Services.AddPostgreSqlPersistence(config);
        builder.Services.AddIdentityPersistence();
        builder.Services.AddRepositoryPersistence();
        builder.Services.AddInterceptorPersistence();

        // ── Infrastructure (Redis Cache + JWT Token Service) ─────────────────
        builder.Services.AddRedisInfrastructure(config);
        builder.Services.AddSecurityInfrastructure(config);

        // ── Application (MediatR, AutoMapper, FluentValidation) ─────────────
        builder.Services.AddApplicationServices(config);

        // ── Authentication + Authorization ────────────────────────────────────
        builder.Services.AddJwtAuthentication(config);

        // ── Minimal API Endpoints ─────────────────────────────────────────────
        builder.Services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        // ── Swagger / OpenAPI ─────────────────────────────────────────────────
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGenNewtonsoftSupport();
        builder.Services.AddSwagger();

        // ── Token Blacklist Middleware ─────────────────────────────────────────
        builder.Services.AddTransient<TokenBlacklistMiddleware>();

        // ── Exception Handler ────────────────────────────────────────────────
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

        if (app.Environment.IsDevelopment() || builder.Environment.IsStaging())
        {
            app.UseDeveloperExceptionPage();
            app.ConfigureSwagger();
        }
        app.UseSerilogRequestLogging();

        // ── Authentication & Authorization 
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
