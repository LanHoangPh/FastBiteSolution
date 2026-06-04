using FastBiteGroup.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FastBiteGroup.MigrationService;

public sealed class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            var seedDataInitializer = scope.ServiceProvider
                .GetRequiredService<SeedDataInitializer>();

            logger.LogInformation("Applying database migrations...");

            await dbContext.Database.MigrateAsync(stoppingToken);

            logger.LogInformation("Database migrations applied successfully.");

            await seedDataInitializer.SeedAsync(stoppingToken);

            logger.LogInformation("Database seed completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            throw;
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
    }
}
