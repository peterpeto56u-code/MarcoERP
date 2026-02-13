using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Treasury;

namespace MarcoERP.WpfUI.Views.Treasury
{
    public partial class BankReconciliationView : UserControl
    {
        private BankReconciliationViewModel ViewModel => DataContext as BankReconciliationViewModel;

        public BankReconciliationView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (ViewModel != null) await ViewModel.LoadAsync();
            };
        }
    }
}
