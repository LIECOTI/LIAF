using CommunityToolkit.Mvvm.ComponentModel;

namespace LIAF.Windows.Features.ApkInstaller.ViewModels;

public partial class ApkInstallerViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;
}
