namespace LIAF.Windows.Contracts.Views;

public interface IViewFor<out TViewModel>
{
    TViewModel ViewModel { get; }
}
