using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Treasury;

namespace MarcoERP.WpfUI.Views.Treasury
{
    public partial class CashboxView : UserControl
    {
        private CashboxViewModel ViewModel => DataContext as CashboxViewModel;

        public CashboxView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (ViewModel != null) await ViewModel.LoadCashboxesAsync();
            };
        }

    }
}
