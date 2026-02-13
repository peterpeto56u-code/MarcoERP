using System.Windows;
using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Treasury;

namespace MarcoERP.WpfUI.Views.Treasury
{
    public partial class CashPaymentView : UserControl
    {
        public CashPaymentView()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                if (DataContext is CashPaymentViewModel vm)
                    await vm.LoadPaymentsAsync();
            };
        }

    }
}
