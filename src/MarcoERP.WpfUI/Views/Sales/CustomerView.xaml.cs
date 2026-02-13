using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Sales;

namespace MarcoERP.WpfUI.Views.Sales
{
    public partial class CustomerView : UserControl
    {
        private CustomerViewModel ViewModel => DataContext as CustomerViewModel;

        public CustomerView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (ViewModel != null) await ViewModel.LoadCustomersAsync();
            };
        }

    }
}
