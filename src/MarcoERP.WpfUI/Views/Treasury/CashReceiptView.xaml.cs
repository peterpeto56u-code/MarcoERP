using System.Windows;
using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Treasury;

namespace MarcoERP.WpfUI.Views.Treasury
{
    public partial class CashReceiptView : UserControl
    {
        public CashReceiptView()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                if (DataContext is CashReceiptViewModel vm)
                    await vm.LoadReceiptsAsync();
            };
        }

    }
}
