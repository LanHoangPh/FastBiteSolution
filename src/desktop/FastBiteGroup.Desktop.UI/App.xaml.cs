using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FastBiteGroup.Desktop.Application;
using FastBiteGroup.Desktop.Infrastructure;
using Serilog;

namespace FastBiteGroup.Desktop.UI;

public partial class App : System.Windows.Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        // 1. Setup Global Exception Handling
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // 2. Build Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 3. Setup Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FastBite", "logs", "log-.txt"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        AppHost = Host.CreateDefaultBuilder()
            .UseSerilog() // Use Serilog instead of default .NET Logger
            .ConfigureAppConfiguration(config => 
            {
                config.AddConfiguration(configuration);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationServices();
                services.AddInfrastructureServices();

                // Services
                services.AddSingleton<FastBiteGroup.Desktop.Application.Abstractions.INavigationService, FastBiteGroup.Desktop.UI.Services.NavigationService>();

                // ViewModels & Views
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        Log.Information("Starting FastBite Desktop Application...");
        await AppHost!.StartAsync();

        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Log.Information("Exiting FastBite Desktop Application...");
        await AppHost!.StopAsync();
        AppHost.Dispose();
        Log.CloseAndFlush();

        base.OnExit(e);
    }

    // --- Global Exception Handlers ---
    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Unhandled UI Exception occurred");
        MessageBox.Show($"Đã xảy ra lỗi hệ thống: {e.Exception.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true; // Prevent crash if possible
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Unobserved Task Exception occurred");
        e.SetObserved();
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Fatal(ex, "Fatal AppDomain Exception occurred");
        }
        Log.CloseAndFlush();
    }
}
