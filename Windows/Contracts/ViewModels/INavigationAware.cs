namespace LIAF.Windows.Contracts.ViewModels;

public interface INavigationAware
{
    void OnNavigatedTo(object? parameter);

    void OnNavigatedFrom();
}
