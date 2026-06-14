using CommunityToolkit.Mvvm.ComponentModel;

namespace LIAF.Windows.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private string selectedRoute = Routes.Devices;
}
