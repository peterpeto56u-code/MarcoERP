using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Inventory;

namespace MarcoERP.WpfUI.Views.Inventory
{
    public partial class BulkPriceUpdateView : UserControl
    {
        private BulkPriceUpdateViewModel ViewModel => DataContext as BulkPriceUpdateViewModel;

        public BulkPriceUpdateView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (ViewModel != null) await ViewModel.LoadProductsAsync();
            };
        }
    }
}
