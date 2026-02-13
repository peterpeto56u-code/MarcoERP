using System.Windows.Controls;
using System.Windows.Input;
using MarcoERP.WpfUI.ViewModels.Purchases;

namespace MarcoERP.WpfUI.Views.Purchases
{
    public partial class PurchaseReturnDetailView : UserControl
    {
        public PurchaseReturnDetailView() => InitializeComponent();

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && sender is DataGrid grid
                && grid.SelectedItem is not null
                && DataContext is PurchaseReturnDetailViewModel vm
                && vm.RemoveLineCommand.CanExecute(grid.SelectedItem))
            {
                vm.RemoveLineCommand.Execute(grid.SelectedItem);
                e.Handled = true;
            }
        }
    }
}
