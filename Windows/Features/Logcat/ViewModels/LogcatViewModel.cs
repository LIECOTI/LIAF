using CommunityToolkit.Mvvm.ComponentModel;

namespace LIAF.Windows.Features.Logcat.ViewModels;

public partial class LogcatViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;
}
