using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Treasury;

namespace MarcoERP.WpfUI.Views.Treasury
{
    public partial class CashTransferView : UserControl
    {
        private CashTransferViewModel ViewModel => DataContext as CashTransferViewModel;

        public CashTransferView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (ViewModel != null) await ViewModel.LoadTransfersAsync();
            };
        }

    }
}
