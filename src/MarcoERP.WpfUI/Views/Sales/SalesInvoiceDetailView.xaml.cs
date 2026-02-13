using System.Windows.Controls;
using System.Windows.Input;
using MarcoERP.WpfUI.ViewModels.Sales;

namespace MarcoERP.WpfUI.Views.Sales
{
    /// <summary>
    /// Sales invoice detail view — compact layout with popup-based line editing.
    /// </summary>
    public partial class SalesInvoiceDetailView : UserControl
    {
        public SalesInvoiceDetailView()
        {
            InitializeComponent();
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            if (sender is not DataGrid grid) return;
            if (grid.SelectedItem == null) return;
            if (DataContext is not SalesInvoiceDetailViewModel vm) return;
            if (!vm.IsEditing && !vm.IsNew) return;

            vm.RemoveLineCommand.Execute(grid.SelectedItem);
            e.Handled = true;
        }

        private void LinesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid grid || grid.SelectedItem == null) return;
            if (DataContext is not SalesInvoiceDetailViewModel vm) return;
            if (!vm.IsEditing && !vm.IsNew) return;
            if (vm.EditLineCommand.CanExecute(grid.SelectedItem))
                vm.EditLineCommand.Execute(grid.SelectedItem);
        }
    }
}
