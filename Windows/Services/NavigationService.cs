using LIAF.Windows.Contracts.Services;
using LIAF.Windows.Features.ApkInstaller.Views;
using LIAF.Windows.Features.Devices.Views;
using LIAF.Windows.Features.FileManager.Views;
using LIAF.Windows.Features.Logcat.Views;
using Microsoft.UI.Xaml.Controls;

namespace LIAF.Windows.Services;

public sealed class NavigationService : INavigationService
{
    private readonly Dictionary<string, Type> routes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["devices"] = typeof(DevicesPage),
        ["apk"] = typeof(ApkInstallerPage),
        ["files"] = typeof(FileManagerPage),
        ["logcat"] = typeof(LogcatPage),
        ["shell"] = typeof(global::LIAF.Windows.Features.Shell.Views.ShellPage),
        ["settings"] = typeof(global::LIAF.Windows.Features.Settings.Views.SettingsPage)
    };

    private Frame? frame;

    public string CurrentRoute { get; private set; } = string.Empty;

    public void Initialize(Frame frame)
    {
        this.frame = frame;
    }

    public bool NavigateTo(string route, object? parameter = null)
    {
        if (frame is null || !routes.TryGetValue(route, out var pageType))
        {
            return false;
        }

        if (frame.CurrentSourcePageType == pageType)
        {
            CurrentRoute = route;
            return true;
        }

        var navigated = frame.Navigate(pageType, parameter);
        if (navigated)
        {
            CurrentRoute = route;
        }

        return navigated;
    }
}
