using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Purchases;

namespace MarcoERP.WpfUI.Views.Purchases
{
    public partial class SupplierView : UserControl
    {
        private SupplierViewModel ViewModel => DataContext as SupplierViewModel;

        public SupplierView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (ViewModel != null) await ViewModel.LoadSuppliersAsync();
            };
        }

    }
}
