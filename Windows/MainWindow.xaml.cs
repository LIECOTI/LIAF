using LIAF.Windows.Views;
using Microsoft.UI.Xaml;

namespace LIAF.Windows;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "LIAF — Android Device Toolkit";
        RootFrame.Navigate(typeof(ShellPage));
    }
}
