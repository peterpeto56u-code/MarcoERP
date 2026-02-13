using System.Windows.Controls;
using System.Windows.Input;
using MarcoERP.WpfUI.ViewModels.Purchases;

namespace MarcoERP.WpfUI.Views.Purchases
{
    public partial class PurchaseInvoiceDetailView : UserControl
    {
        public PurchaseInvoiceDetailView() => InitializeComponent();

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && sender is DataGrid grid
                && grid.SelectedItem is not null
                && DataContext is PurchaseInvoiceDetailViewModel vm
                && vm.RemoveLineCommand.CanExecute(grid.SelectedItem))
            {
                vm.RemoveLineCommand.Execute(grid.SelectedItem);
                e.Handled = true;
            }
        }

        private void LinesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid grid || grid.SelectedItem == null) return;
            if (DataContext is not PurchaseInvoiceDetailViewModel vm) return;
            if (!vm.IsEditing && !vm.IsNew) return;
            if (vm.EditLineCommand.CanExecute(grid.SelectedItem))
                vm.EditLineCommand.Execute(grid.SelectedItem);
        }
    }
}
