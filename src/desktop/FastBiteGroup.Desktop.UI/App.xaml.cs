using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using FastBiteGroup.Desktop.Application;
using FastBiteGroup.Desktop.Infrastructure;
using FastBiteGroup.Desktop.UI.Services;
using FastBiteGroup.Desktop.UI.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FastBiteGroup.Desktop.UI;

public partial class App : System.Windows.Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

    #if DEBUG
        configBuilder.AddUserSecrets<App>();
    #endif

        var configuration = configBuilder.Build();

        var syncfusionLicense = configuration["SyncfusionLicense"];
        if (!string.IsNullOrWhiteSpace(syncfusionLicense))
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);
        }

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "FastBite",
                    "logs",
                    "log-.txt"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        AppHost = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureAppConfiguration(config =>
            {
                config.AddConfiguration(configuration);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationServices();
                services.AddInfrastructureServices();

                services.AddSingleton<
                    FastBiteGroup.Desktop.Application.Abstractions.INavigationService,
                    FastBiteGroup.Desktop.UI.Services.NavigationService>();
                services.AddSingleton<IThemeService, ThemeService>();
                services.AddSingleton<ILanguageService, LanguageService>();

                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<MainWindow>();

                services.AddTransient<LoginViewModel>();
                services.AddTransient<LoginWindow>();
                services.AddTransient<RegisterViewModel>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        Log.Information("Starting FastBite Desktop Application...");
        
        try
        {
            await AppHost!.StartAsync();

            AppHost.Services.GetRequiredService<IThemeService>().Initialize();
            AppHost.Services.GetRequiredService<ILanguageService>().Initialize();

            var loginWindow = AppHost.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to start application");
            MessageBox.Show(
                "An unexpected application error occurred during startup. Please check the logs.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Log.Information("Exiting FastBite Desktop Application...");
        
        try
        {
            await AppHost!.StopAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while stopping the application host");
        }
        finally
        {
            AppHost?.Dispose();
            Log.CloseAndFlush();
        }

        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(
        object sender,
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Unhandled UI exception occurred");
        MessageBox.Show(
            "An unexpected application error occurred. Please restart the application.",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Unobserved task exception occurred");
        e.SetObserved();
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Fatal(ex, "Fatal AppDomain exception occurred");
        }

        Log.CloseAndFlush();
    }
}
