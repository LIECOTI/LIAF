using LIAF.Windows.Features.Settings.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace LIAF.Windows.Features.Settings.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = App.GetService<SettingsViewModel>();
    }
}
