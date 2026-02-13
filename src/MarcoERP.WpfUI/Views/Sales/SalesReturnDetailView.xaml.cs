using System.Windows.Controls;
using System.Windows.Input;
using MarcoERP.WpfUI.ViewModels.Sales;

namespace MarcoERP.WpfUI.Views.Sales
{
    public partial class SalesReturnDetailView : UserControl
    {
        public SalesReturnDetailView() => InitializeComponent();

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && sender is DataGrid grid
                && grid.SelectedItem is not null
                && DataContext is SalesReturnDetailViewModel vm
                && vm.RemoveLineCommand.CanExecute(grid.SelectedItem))
            {
                vm.RemoveLineCommand.Execute(grid.SelectedItem);
                e.Handled = true;
            }
        }
    }
}
