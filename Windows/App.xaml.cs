using LIAF.Windows.Contracts.Services;
using LIAF.Windows.Core.Logging;
using LIAF.Windows.Core.Services;
using LIAF.Windows.Features.Devices.ViewModels;
using LIAF.Windows.Features.Settings.ViewModels;
using LIAF.Windows.Infrastructure.Adb;
using LIAF.Windows.Infrastructure.Fastboot;
using LIAF.Windows.Infrastructure.Process;
using LIAF.Windows.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace LIAF.Windows;

public sealed partial class App : Application
{
    private Window? window;

    public App()
    {
        Services = ConfigureServices();
        InitializeComponent();
        UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    public static IServiceProvider Services { get; private set; } = null!;

    public static T GetService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            window = GetService<MainWindow>();
            window.Activate();
            await GetService<IDeviceMonitoringService>().StartAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            LogStartupException(ex);
            window = new MainWindow();
            window.Activate();
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ILogService, InMemoryLogService>();
        services.AddSingleton<IToolSettingsService, ToolSettingsService>();
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IAdbService, AdbService>();
        services.AddSingleton<IFastbootService, FastbootService>();
        services.AddSingleton<IDeviceMonitoringService, DeviceMonitoringService>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddTransient<DevicesViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static void LogStartupException(Exception exception)
    {
        Services.GetService<ILogService>()?.Add(new LogEntry(DateTimeOffset.Now, LogCategory.System, LogLevel.Error, exception.Message));
    }

    private static void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs args)
    {
        args.Handled = true;
        Services.GetService<ILogService>()?.Add(new LogEntry(DateTimeOffset.Now, LogCategory.System, LogLevel.Error, args.Exception.Message));
    }

    private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        if (args.ExceptionObject is Exception exception)
        {
            Services.GetService<ILogService>()?.Add(new LogEntry(DateTimeOffset.Now, LogCategory.System, LogLevel.Error, exception.Message));
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args)
    {
        Services.GetService<ILogService>()?.Add(new LogEntry(DateTimeOffset.Now, LogCategory.System, LogLevel.Error, args.Exception.Message));
        args.SetObserved();
    }
}
