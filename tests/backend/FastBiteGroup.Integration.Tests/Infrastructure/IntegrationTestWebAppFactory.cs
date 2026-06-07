using FastBiteGroup.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace FastBiteGroup.Integration.Tests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<API.Program>, IAsyncLifetime
{
    //[Obsolete]
    //private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder(PostgreSqlBuilder.PostgreSqlImage)
    //    .WithDatabase("fastbite_test")
    //    .WithUsername("postgres")
    //    .WithPassword("postgres")
    //    .Build();
    //[Obsolete]
    //private readonly RedisContainer _redisContainer = new RedisBuilder()
    //    .Build();

    //protected override void ConfigureWebHost(IWebHostBuilder builder)
    //{
    //    builder.ConfigureAppConfiguration((context, configBuilder) =>
    //    {
    //        var configData = new Dictionary<string, string?>
    //        {
    //            { "ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString() },
    //            { "ConnectionStrings:redis", _redisContainer.GetConnectionString() }
    //        };

    //        configBuilder.AddInMemoryCollection(configData);
    //    });

    //    // Ensure database is created and migrated
    //    builder.ConfigureServices(services =>
    //    {
    //        var sp = services.BuildServiceProvider();
    //        using var scope = sp.CreateScope();
    //        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //        db.Database.Migrate();
    //    });
    //}

    //public async Task InitializeAsync()
    //{
    //    await _dbContainer.StartAsync();
    //    await _redisContainer.StartAsync();
    //}

    //public new async Task DisposeAsync()
    //{
    //    await _dbContainer.StopAsync();
    //    await _redisContainer.StopAsync();
    //}
    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
