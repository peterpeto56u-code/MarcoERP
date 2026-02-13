using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Inventory;

namespace MarcoERP.WpfUI.Views.Inventory
{
    /// <summary>
    /// Code-behind for ProductView.xaml.
    /// </summary>
    public partial class ProductView : UserControl
    {
        private ProductViewModel ViewModel => DataContext as ProductViewModel;

        public ProductView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (ViewModel != null) await ViewModel.LoadProductsAsync();
            };
        }

    }
}
