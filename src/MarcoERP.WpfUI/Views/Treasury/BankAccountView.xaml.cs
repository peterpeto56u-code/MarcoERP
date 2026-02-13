using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Treasury;

namespace MarcoERP.WpfUI.Views.Treasury
{
    public partial class BankAccountView : UserControl
    {
        private BankAccountViewModel ViewModel => DataContext as BankAccountViewModel;

        public BankAccountView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (ViewModel != null) await ViewModel.LoadBankAccountsAsync();
            };
        }
    }
}
