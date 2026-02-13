using System.Windows;
using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Accounting;

namespace MarcoERP.WpfUI.Views.Accounting;

public partial class FiscalYearView : UserControl
{
    public FiscalYearView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is FiscalYearViewModel vm)
        {
            await vm.LoadFiscalYearsAsync();
        }
    }
}
