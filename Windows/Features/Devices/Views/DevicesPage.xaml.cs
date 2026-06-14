using CommunityToolkit.Mvvm.Input;
using LIAF.Windows.Features.Devices.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LIAF.Windows.Features.Devices.Views;

public sealed partial class DevicesPage : Page
{
    public DevicesPage()
    {
        InitializeComponent();
        DataContext = App.GetService<DevicesViewModel>();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is DevicesViewModel { RefreshCommand: IAsyncRelayCommand refreshCommand } && refreshCommand.CanExecute(null))
        {
            refreshCommand.Execute(null);
        }
    }
}
