using System.Windows;
using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Purchases;

namespace MarcoERP.WpfUI.Views.Purchases
{
    public partial class PurchaseReturnView : UserControl
    {
        public PurchaseReturnView()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                if (DataContext is PurchaseReturnViewModel vm)
                    await vm.LoadReturnsAsync();
            };
        }

    }
}
