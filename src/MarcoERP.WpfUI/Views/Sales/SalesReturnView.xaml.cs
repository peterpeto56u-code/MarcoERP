using System.Windows;
using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Sales;

namespace MarcoERP.WpfUI.Views.Sales
{
    public partial class SalesReturnView : UserControl
    {
        public SalesReturnView()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                if (DataContext is SalesReturnViewModel vm)
                    await vm.LoadReturnsAsync();
            };
        }

    }
}
