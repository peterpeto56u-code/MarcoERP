using System.Windows.Controls;
using MarcoERP.WpfUI.ViewModels.Sales;

namespace MarcoERP.WpfUI.Views.Sales
{
    public partial class SalesRepresentativeView : UserControl
    {
        private SalesRepresentativeViewModel ViewModel => DataContext as SalesRepresentativeViewModel;

        public SalesRepresentativeView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (ViewModel != null) await ViewModel.LoadAsync();
            };
        }
    }
}
