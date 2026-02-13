using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Inventory;

namespace MarcoERP.WpfUI.Views.Inventory
{
    public partial class WarehouseView : UserControl
    {
        private WarehouseViewModel ViewModel => DataContext as WarehouseViewModel;

        public WarehouseView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (ViewModel != null) await ViewModel.LoadWarehousesAsync();
            };
        }

    }
}
