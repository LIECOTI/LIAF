using CommunityToolkit.Mvvm.ComponentModel;

namespace LIAF.Windows.Features.Shell.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;
}
