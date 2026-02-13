using System.Windows;
using MarcoERP.WpfUI.ViewModels.Treasury;

namespace MarcoERP.WpfUI.Views.Treasury
{
    public partial class QuickCashPaymentWindow : Window
    {
        private readonly QuickCashPaymentViewModel _viewModel;

        public QuickCashPaymentWindow(QuickCashPaymentViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.RequestClose += OnRequestClose;
        }

        private void OnRequestClose(bool success)
        {
            DialogResult = success;
            Close();
        }
    }
}
