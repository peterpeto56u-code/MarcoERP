using System.Windows.Controls;
using System.Windows.Input;
using MarcoERP.WpfUI.ViewModels.Inventory;

namespace MarcoERP.WpfUI.Views.Inventory
{
    public partial class UnitView : UserControl
    {
        private UnitViewModel ViewModel => DataContext as UnitViewModel;

        public UnitView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (ViewModel != null) await ViewModel.LoadUnitsAsync();
            };
        }

    }
}
