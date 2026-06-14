using LIAF.Windows.Contracts.Services;
using Microsoft.UI.Xaml.Controls;

namespace LIAF.Windows.Views;

public sealed partial class ShellPage : Page
{
    private readonly INavigationService navigationService;

    public ShellPage()
    {
        InitializeComponent();
        navigationService = App.GetService<INavigationService>();
        navigationService.Initialize(ContentFrame);
        navigationService.NavigateTo("devices");
    }

    private void OnSettingsInvoked(NavigationView sender, object args)
    {
        navigationService.NavigateTo("settings");
    }

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem { Tag: string route })
        {
            navigationService.NavigateTo(route);
        }
    }
}
