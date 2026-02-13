using System.Windows;
using MarcoERP.WpfUI.ViewModels.Treasury;

namespace MarcoERP.WpfUI.Views.Treasury
{
    public partial class QuickCashReceiptWindow : Window
    {
        private readonly QuickCashReceiptViewModel _viewModel;

        public QuickCashReceiptWindow(QuickCashReceiptViewModel viewModel)
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
