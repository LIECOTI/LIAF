using Microsoft.UI.Xaml.Controls;

namespace LIAF.Windows.Contracts.Services;

public interface INavigationService
{
    string CurrentRoute { get; }

    void Initialize(Frame frame);

    bool NavigateTo(string route, object? parameter = null);
}
