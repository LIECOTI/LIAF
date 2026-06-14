using CommunityToolkit.Mvvm.ComponentModel;

namespace LIAF.Windows.Features.FileManager.ViewModels;

public partial class FileManagerViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;
}
